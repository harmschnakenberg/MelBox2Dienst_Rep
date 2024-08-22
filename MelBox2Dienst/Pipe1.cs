using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    internal partial class Pipe1
    {
        /// <summary>
        /// Generelle Freigabe zur Verwendung von E-Mail-Kommunikation
        /// </summary>
        public static bool IsEmailUsed { get; set; } = true;

        public static string GsmSignalQuality { get; private set; } = "-1%";

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
            public const string GsmReinit = "GsmReinit";
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
                            if (line == null || line.Length < 2)
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
        private static async Task<KeyValuePair<string,string>> Send(string pipeName, string verb, string arg)
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
                        string result = await reader.ReadLineAsync();
                        if (result != null)
                        {
                            string[] args = result.Split('|');
                            return new KeyValuePair<string, string>(args[0], args[1]);
                        }
                    }
                }
            }
            catch (TimeoutException) { } //Couldn't connect to server
            catch (IOException) { } //Pipe was broken

            return new KeyValuePair<string, string>(verb, string.Empty);
        }
        #endregion
    }

    internal partial class Pipe1
    {
        public static SortedDictionary<string, Tuple<DateTime, string>> GsmStatus { get; set; } = new SortedDictionary<string, Tuple<DateTime, string>>();

        #region Anfrage auswerten
        /// <summary>
        /// Wertet die Anfrage der NamedPipe aus
        /// </summary>
        /// <param name="line">Anfrage-String aus einer NamedPipe</param>
        /// <returns>Antwort, die an die NamedPipe auf die Anfrage weitergegeben wird. Ansonsten 'null'</returns>
        private static string ParseQuery(string line)
        {
#if DEBUG
            Log.Info($"Von Pipe: '{line}'");
#endif
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
                        return Answer(Verb.SmsRecieved, arg); //Erfolg: sende die Anfrage ungverändert zurück
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
                        return Answer(Verb.ReportRecieved, arg); //Erfolg: sende die Anfrage ungverändert zurück
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
                    return null; // Answer(Verb.CallRelay, string.Empty);
                case Verb.CallRecieved:
                    string callingNumber = arg;
                    //eingegangenen Sprachanruf protokollieren
                    _ = Sql.CreateSent(callingNumber, Sql.CallRelayPhone);

                    //keine Antwort erforderlich (GSM-Modem leitet selbständig weiter)
                    return null;
                case Verb.EmailRecieved:
#if DEBUG
                    Log.Info($"Email recieved:\r\n{arg}\r\n\r\n---\r\n\r\n");
#endif
                    Email email = new Email();
                    try
                    {
                        email = JsonSerializer.Deserialize<Email>(arg);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"ParseQuery() mit {verb}: Fehler:\r\n" + ex);
                    }

                    if (!IsEmailUsed)
                        Sql.CreateRecievedMessage(email); //nur Empfang protollieren
                    else if (Sql.MessageRecieveAndRelay(email))
                        return line; //Erfolg: sende die Anfrage ungverändert zurück
                    
                        return Answer(Verb.Error, arg);
                case Verb.GsmStatus:                    
                    UpdateGsmStatus(JsonSerializer.Deserialize<Tuple<string, DateTime, string>>(arg));
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

        /// <summary>
        /// Protokolliert gemeldete Änderungen einer Statusmeldung aus dem GSM-Modem z.B. Signalstärke,..
        /// </summary>
        /// <param name="gsmStatus"></param>
        private static void UpdateGsmStatus(Tuple<string, DateTime, string> gsmStatus)
        {
            if (!GsmStatus.Keys.Contains(gsmStatus.Item1))
                GsmStatus.Add(gsmStatus.Item1, new Tuple<DateTime, string>(gsmStatus.Item2, gsmStatus.Item3));
            else
                GsmStatus[gsmStatus.Item1] = new Tuple<DateTime, string>(gsmStatus.Item2, gsmStatus.Item3);

            if (gsmStatus.Item1 == "SignalQuality" && Pipe1.GsmSignalQuality != gsmStatus.Item3)
            {
                if (int.TryParse(gsmStatus.Item3.TrimEnd('%'), out int quality))
                    Sql.CreateNetworkQualityEntry(quality)
;
                Log.Info("GSM-Signalstärke " + gsmStatus.Item3);                
                Pipe1.GsmSignalQuality = gsmStatus.Item3;
            }
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

                    var pipeAnswer = await Send(PipeName.Gsm, Verb.SmsSend, JsonSerializer.Serialize<Sms>(sms));
                    
                    if (pipeAnswer.Value?.Length == 0) //Fehler (z.B. Pipe zu GSM-Modem?): Wie abfangen?
                        return;

                    #region SMS-Id der gesendeten SMS in DB eintragen.
                    //Erwartete Antwort : 'SmsSend|[JSON-Object Sms]' mit sms.Index = aktueller Index in GSM-Modem.
                    Sms smsAnswer = JsonSerializer.Deserialize<Sms>(pipeAnswer.Value);

                    if (smsAnswer.Index > 0) //-> Die SMS wurde versand und im GSM-Modem auf einem Index gespeichert.
                        Sql.CreateSent(messageId, smsAnswer);
                    else
                        Log.Error($"Sms-Versand an '{phone}' konnte nicht bestätigt werden; '{sms.Content}'\r\nMessageId [{messageId}]");
                    #endregion
                }
            }

        }

        internal static async void SendSmsAsync(Sms sms)
        {
            if (!IsPhoneNumber(sms.Phone))
            {
                Log.Error($"SMS kann nicht gesendet werden.'{sms.Phone}' ist keien gültige Telefonnummer.");
                return;
            }
                KeyValuePair<string, string> pipeAnswer = await Send(PipeName.Gsm, Verb.SmsSend, JsonSerializer.Serialize(sms));

            if (pipeAnswer.Value?.Length == 0) //Fehler (z.B. Pipe zu GSM-Modem?): Wie abfangen?
                return;

            #region SMS-Id der gesendeten SMS in DB eintragen.
            //Erwartete Antwort : 'SmsSend|[JSON-Object Sms]' mit sms.Index = aktueller Index in GSM-Modem.
            Sms smsAnswer = JsonSerializer.Deserialize<Sms>(pipeAnswer.Value);

            if (smsAnswer.Index > 0) //-> Die SMS wurde versand und im GSM-Modem auf einem Index gespeichert.
                Sql.CreateSent(0, smsAnswer);
            else
                Log.Error($"Sms-Versand an '{sms.Phone}' konnte nicht bestätigt werden; '{sms.Content}'");
            #endregion
        }


        internal static async void SendEmail(Email email)
        {
            if (!IsEmailUsed)
            {
                Log.Info($"E-Mail senden ist deaktiviert: " + email.ToString());
                return;
            }
#if DEBUG
            Log.Info(email.ToString());
#endif
            KeyValuePair<string, string> pair = await Send(Pipe1.PipeName.Email, Verb.EmailSend, JsonSerializer.Serialize(email));
#if DEBUG
            Log.Info(
                "NamedPipe SendEmail(): " + pair.Key + "\t" + pair.Value
                );
#endif
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

        internal static async void GsmReinit()
        {
            await Send(Pipe1.PipeName.Gsm, Verb.GsmReinit, string.Empty);
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
            KeyValuePair<string, string> result = await Send(PipeName.Gsm, Verb.CallRelay, JsonSerializer.Serialize( callRelay ) );
            callRelay.Phone = result.Key;
            callRelay.Status = result.Value;

            if (result.Value?.Length == 0)
                return callRelay;

            //Rückgabe: aktuell im Modem hinterlegte Sprachanrufumleitung
            return JsonSerializer.Deserialize<CallRelay>(result.Value);
            
            
        }

        #endregion
    }

}