using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MelBox2Dienst
{
    internal partial class Pipe1
    {
        #region Feste Begriffe für Pipe-Kommunikation
        internal static class PipeName
        {
            internal static string MelBox2Service { get; } = "MelBox2Service";
            internal static string Gsm { get; } = "Gsm";
            internal static string Email { get; } = "Email";
        }

        public static class Verb
        {
            //public const string StartPipe = "StartPipe";
            public const string SmsSend = "SmsSend";
            public const string ReportRecieved = "ReportRecieved";
            public const string SmsRecieved = "SmsRecieved";
            public const string EmailSend = "EmailSend";
            public const string EmailRecieved = "EmailRecieved";
            public const string CallRelay = "CallRelay";
            public const string CallRecieved = "CallRecieved";
            public const string GsmStatus = "GsmStatus";
            public const string Error = "ERROR";
        }
        #endregion

        #region Auf Anfrage warten und Anfragen auswerten

        /// <summary>
        /// Erstellt eine NamedPipe und wartet auf einen Client, verarbeitet die Anfrage udn schickt eine Antwort zurück
        /// </summary>
        /// <param name="pipeName">Name der NamedPipe, die aufgemacht werden sol</param>
        /// <param name="perpetual">gibt an, ob der Server nach Abbruch auomatisch neu gestartet werden soll</param>
        internal static async void StartPipeServer(string pipeName, bool perpetual = false)
        {
            try
            {
                using (var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut))
                {
                    await server.WaitForConnectionAsync();

                    using (StreamReader reader = new StreamReader(server))
                    using (StreamWriter writer = new StreamWriter(server))
                    {
                        while (server.IsConnected)
                        {
                            string line = await reader.ReadLineAsync();
#if DEBUG
                            Log.Info($"Anfrage an {pipeName} > {line}");
#endif
                            if (line == null)
                                break;

                            string answer = ParseQuery(line);
#if DEBUG
                            Log.Info($"Antwort von {pipeName} > {answer}");
#endif
                            #region Antwort zurück                            
                            if (answer != null)
                            {
                                await writer.WriteLineAsync(answer);
                                await writer.FlushAsync();
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (System.IO.IOException)
            {
                if (perpetual)
                    StartPipeServer(pipeName, true);
            }
            catch (Exception ex)
            {
                Log.Error("Fehler StartPipeServer2()\r\n\r\n" + ex.ToString());
            }
     
#if DEBUG
            Log.Info($"PipeServer({pipeName}) beendet.");
#endif
        }

        #endregion

        #region Senden und Antwort empfangen
        /// <summary>
        /// Sendet einen Befehl und Inhalt mittels NamedPipe
        /// </summary>
        /// <param name="pipeName">Name der offenen NamedPipe, an die gesendet werden soll</param>
        /// <param name="verb">Befehl, der vom NamedPipeServer interpreiert werden muss</param>
        /// <param name="arg">Argument / Übergabeparameter zu dem Befehl im JSON-Format</param>
        /// <returns></returns>
        private static async Task<string> Send(string pipeName, string verb, string arg)
        {
            try
            {
                using (var client = new NamedPipeClientStream(pipeName))
                {
                    await client.ConnectAsync(10000);//Timeout nach 10 sec.
                    using (StreamReader reader = new StreamReader(client))
                    using (StreamWriter writer = new StreamWriter(client))
                    {
                        await writer.WriteLineAsync($"{verb}|{arg}");
                        await writer.FlushAsync();
                        return await reader.ReadLineAsync();
                    }
                }
            }
            catch (TimeoutException) { } //Couldn't connect to server
            catch (IOException) { } //Pipe was broken
             
            return string.Empty; 
        }
        #endregion
    }

    internal partial class Pipe1
    {
        public static Dictionary<string, string> GsmStatus { get; set; } = new Dictionary<string, string>();

        #region Anfrage auswerten
        /// <summary>
        /// Wertet die Anfrage der NamedPipe aus
        /// </summary>
        /// <param name="line">Anfrage-String aus einer NamedPipe</param>
        /// <returns>Antwort, die an die NamedPipe auf die Anfrage weitergegeben wird. Ansonsten 'null'</returns>
        private static string ParseQuery(string line)
        {
            string[] args = line.Split('|');
            if (args.Length != 2)
                return null;

            string verb = args[0];
            string arg = args[1];

            switch (verb)
            {
                case Verb.SmsRecieved:
                    //Anfrage von GSM-Modem eine EMpfangene SMS zu registrieren
                    Sms smsRec = JsonSerializer.Deserialize<Sms>(arg);
                    if (Sql.MessageRecieveAndRelay(smsRec))
                        return line; //Erfolg: sende die Anfrage ungverändert zurück
                    else
                        return Answer(Verb.Error, arg);
                case Verb.SmsSend:
                    return Answer(verb, "nicht implementiert");
                case Verb.ReportRecieved:
                    StatusReport report = JsonSerializer.Deserialize<StatusReport>(arg);
#if DEBUG
                    Log.Info($"SMS-StatusReport an Referenz {report.Reference}: {report.DeliveryStatusText}");
#endif
                    if (Sql.UpdateSentSms(report))
                        return line; //Erfolg: sende die Anfrage ungverändert zurück
                    else
                        return Answer(Verb.Error, arg);
                case Verb.CallRelay:
                    //Antwort auf Anfrage zum Ändern der Rufumleitung
                    string actualCallRelayPhone = arg;
                    if (actualCallRelayPhone != Sql.CallRelayPhone)
                    {
                        Log.Info($"Rufumleitung umgeschaltet von '{Sql.CallRelayPhone}' auf '{actualCallRelayPhone}'");
                        Sql.CallRelayPhone = actualCallRelayPhone;
                    }

                    //keine Antwort erforderlich
                    return null;
                case Verb.CallRecieved:
                    string callingNumber = arg;
                    //eingegangenen Sprachanruf protokollieren
                    _ = Sql.CreateSent(callingNumber, Sql.CallRelayPhone);

                    //keine Antwort erforderlich (GSM-Modem leitet selbständig weiter)
                    return null;
                case Verb.EmailRecieved:
                    Email email = JsonSerializer.Deserialize<Email>(arg);

                    if (Sql.MessageRecieveAndRelay(email))
                        return line; //Erfolg: sende die Anfrage ungverändert zurück
                    else
                        return Answer(Verb.Error, arg);
                case Verb.GsmStatus:

                    KeyValuePair<string, string> gsmStatus = JsonSerializer.Deserialize<KeyValuePair<string, string>>(arg);

                    if (!GsmStatus.Keys.Contains(gsmStatus.Key))
                        GsmStatus.Add(gsmStatus.Key, gsmStatus.Value);
                    else
                        GsmStatus[gsmStatus.Key] = gsmStatus.Value;

                    //Keine Rückantwort auf der Gegenseite erwartet
                    return Answer(Verb.GsmStatus, string.Empty);                    
                default:
                    return Answer(verb, "unbekannt " + arg);

            }
        }

        /// <summary>
        /// Formatiert die Antwort für die NamedPipe
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static string Answer(string verb, string arg)
        {
            return $"{verb}|{arg}";
        }



        #endregion

        #region Anfragen absenden

        /// <summary>
        /// Veranlasst das Senden von ein/mehreren SMS. Protokolliert den erfolgreichen Versand in DB
        /// </summary>
        /// <param name="guards">Liste der Telefonnummern, an die die SMS gesendet werden soll</param>
        /// <param name="sms">Sms-Object</param>
        /// <param name="messageId">Id des Nachrichteninhalts aus der DB</param>
        internal async static void SendSms(List<string> guards, Sms sms, uint messageId)
        {
            foreach (string phone in guards)
            {
                if (IsPhoneNumber(phone))
                {
                    sms.Index = -1; // Setze index auf einen ungültigen Wert. In GSM-Modem wird Wert neu gesetzt.
                    sms.Phone = phone; //Setze den neuen Empfänger (Bereitschaft)

                    string result = await Send(PipeName.Gsm, Verb.SmsSend, JsonSerializer.Serialize<Sms>(sms));

                    #region SMS-Id der gesendeten SMS in DB eintragen.
                    //Erwartete Antwort : 'SmsSend|[JSON-Object Sms]' mit sms.Index = aktueller Index in GSM-Modem.

                    string[] args = result?.Split('|');
                    if (args.Length != 2)
                    {
                        Log.Error($"Sms-Versand an '{phone}' konnte nicht bestätigt werden; Antwort von DB ungültig: '{result}'");
                        break;
                    }

                    Sms smsAnswer = JsonSerializer.Deserialize<Sms>(args[1]);

                    if (smsAnswer.Index > 0) //-> Die SMS wurde versand und im GSM-Modem auf einem Index gespeichert.
                        Sql.CreateSent(messageId, smsAnswer);
                    else
                        Log.Error($"Sms-Versand an '{phone}' konnte nicht bestätigt werden; '{sms.Content}'");
                    #endregion
                }
            }

        }


        internal static void SendEmail(Email email)
        {
            Log.Info(
                "NamedPipe SendEmail(): " +
                Send(Pipe1.PipeName.Email, Verb.EmailSend, JsonSerializer.Serialize(email))
                );
        }

        /// <summary>
        /// Prüft, ob ein String dem Format '+000000...' entspricht
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool IsPhoneNumber(string number)
        {
            return Regex.Match(number, @"(\+[0-9]+)").Success;
        }


        /// <summary>
        /// Veranlasst die Rufumleitung von Sprachanrufen an die Nummer 'phone'
        /// </summary>
        /// <param name="phone">Telefonnummer, an die Sprachnachrichten weitergeleitet werden sollen</param>
        /// <returns>Aus dem GSM-Modem ausgelesene, aktuelle Nummer für Rufumleitungen.</returns>
        internal async static Task<CallRelay> RelayCall(string phone)
        {
            CallRelay callRelay = new CallRelay(string.Empty, "uninitialized");

            if (!IsPhoneNumber(phone))
                return callRelay;

            callRelay.Phone = phone;
            string result = await Send(PipeName.Gsm, Verb.CallRelay, JsonSerializer.Serialize( callRelay ) );

            string[] args = result?.Split('|');
            if (args.Length != 2)
                return callRelay;

            //Rückgabe: aktuell im Modem hinterlegte Sprachanrufumleitung
            return JsonSerializer.Deserialize<CallRelay>(args[1]);
            
            
        }

        #endregion
    }

}