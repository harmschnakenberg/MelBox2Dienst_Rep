using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MelBox2Gsm.Program;

namespace MelBox2Gsm
{

    internal partial class Pipe3
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
            public const string GsmReinit = "GsmReinit";
            public const string Error = "ERROR";
        }
        #endregion

        #region Auf Anfrage warten und Anfragen auswerten

        internal static async void StartPipeServer2(string pipeName)
        {
            using (var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut))
            {
                while (true)
                {
                    server.WaitForConnection();
                    using (StreamReader sr = new StreamReader(server))
                    using (StreamWriter writer = new StreamWriter(server))
                    {
                        string line = sr.ReadToEnd();
                        string answer = ParseQuery(line);

                        #region Antwort zurück                            
                        if (answer != null)
                        {
                            await writer.WriteLineAsync(answer);
                            await writer.FlushAsync();
                        }
                        #endregion
                    }
                    server.Disconnect();
                }
            }
        }



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
                            Log.Info($"Anfrage an {pipeName} > '{line}'");
#endif
                            if (line?.Length > 0)
                            {
                                string answer = ParseQuery(line);
#if DEBUG
                                Log.Info($"Antwort von {pipeName} > '{answer}'");
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
            }
            catch (System.IO.IOException)
            {
                Log.Error("NamedPipeServer IOException. Neustart NamedPipe: " + perpetual);
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
        private static async Task<KeyValuePair<string, string>> Send(string pipeName, string verb, string arg)
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
                        string result = await reader?.ReadLineAsync();
                        string[] args = result?.Split('|');
                        return new KeyValuePair<string, string>(args[0], args[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Fehler beim Senden an Pipe: {pipeName}|{verb}" +
                    $"\r\n\t{ex.GetType()}: {ex.Message}" +
                    $"\r\n\t{arg}");
                return new KeyValuePair<string, string>(verb, string.Empty);
            }
        }
        #endregion
    }

    internal partial class Pipe3
    {
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
                    //Antwort auf Anfrage von GSM-Modem eine empfangene SMS zu registrieren
                    Sms smsRec = JsonSerializer.Deserialize<Sms>(arg);
                    Log.Recieved($"Empfangene Sms ist verarbeitet und kann jetzt gelöscht werden: " +
                        $"{smsRec.Index}, {smsRec.Phone}, {smsRec.Content}, {smsRec.Time}");
                    return null;
                case Verb.SmsSend:
                    //Anfrage von DB eine SMS zu versenden
                    Sms smsOut = JsonSerializer.Deserialize<Sms>(arg);
                    Sms result = SendSms(smsOut.Phone, smsOut.Content);
                    return Answer(verb, JsonSerializer.Serialize<Sms>(result));
                case Verb.ReportRecieved:
                    //Antwort auf Anfrage von GSM-Modem einen empfangenen StatusReport zu registrieren
                    //Keine Antwort erforderlich
                    return null;
                case Verb.CallRelay:
                    //Anfrage zum Ändern der Rufumleitung
                    CallRelay callRelay = JsonSerializer.Deserialize<CallRelay>(arg);

                    if (IsPhoneNumber(callRelay.Phone))
                    {
                        callRelay = SetCallRedirection(callRelay.Phone);

                        if (callRelay.Phone != CallRelayPhone)
                        {
                            //if (CallRelayPhone is null)
                            //    callRelay.Status += $" Rufumleitung eingerichtet auf '{callRelay.Phone}'";
                            //else
                            //    callRelay.Status += $" Rufumleitung umgeschaltet von '{CallRelayPhone}' auf '{callRelay.Phone}'";

                            Log.Info(callRelay.Status);
                            CallRelayPhone = callRelay.Phone;
                        }

                        Pipe3.SendGsmStatus(nameof(CallRelayPhone), callRelay.Status);
                    }

                    return Answer(Verb.CallRelay, JsonSerializer.Serialize<CallRelay>(callRelay));
                case Verb.CallRecieved:
                    //Antwort auf Anfrage Sprachanruf protokollierten 
                    //keine Antwort erforderlich
                    return null;
                case Verb.GsmReinit:
                    SetupGsmModem();
                    //keine Antwort erforderlich
                    return null;                    
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
        /// Gibt eine Fehlermeldung aus dem GSM-Modem an DB weiter
        /// </summary>
        /// <param name="msg"></param>
        internal static async void GsmErrorOccuredAsync(string msg)
        {
            _ = await Send(PipeName.MelBox2Service, Verb.Error, msg);
            Log.Error(msg);
        }

        /// <summary>
        /// Überträgt eine empfangene SMS an die Datenbank.
        /// Bei erfolg wird die empfangene SMS aus dem GSM-Modem gelöscht.
        /// </summary>
        /// <param name="smsIn"></param>
        internal async static void RecievedSms(Sms smsIn)
        {
            //TEST
            Log.Info($"Empfange SMS: {smsIn.Index}, {smsIn.Phone}, {smsIn.Content}");

            //Sende Empfangene SMS zur DB
            KeyValuePair<string, string> answer = await Send(PipeName.MelBox2Service, Verb.SmsRecieved, JsonSerializer.Serialize<Sms>(smsIn));
            //Bei Erfolg wird die gleiche Nachricht zurückgesandt
            Log.Info($"Antwort: '{answer.Value}'");

            if (answer.Value?.Length < 3)
            {
                Log.Error($"SMS an Index {smsIn.Index} konnte nicht übermittelt werden.");
                return;
            }

            Sms back = JsonSerializer.Deserialize<Sms>(answer.Value);
#if DEBUG
            Log.Info($"SMS an Index {back.Index} erfolgreich protokolliert. Kann jetzt gelöscht werden.");
#endif
            DeleteSms(back.Index); //SMS aus GSM-Speicher löschen

        }

        /// <summary>
        /// Überträgt einen empfangenen StatusReport zu einer versendeten SMS an die Datenbank.
        /// Bei erfolg wird der SttusReport und die gesendete SMS aus dem GSM-Modem gelöscht.
        /// </summary>
        /// <param name="reportIn"></param>
        internal async static void ReportRecieved(StatusReport reportIn)
        {
            KeyValuePair<string, string> answer = await Pipe3.Send(Pipe3.PipeName.MelBox2Service, Pipe3.Verb.ReportRecieved, JsonSerializer.Serialize(reportIn));
            //Bei erfolg wird die Anfrage unverändetr zurückgeschickt

            if (answer.Value?.Length < 1)
            {
                Log.Error("Empfangener Report konnte nicht übermittelt werden.");
                return;
            }

            StatusReport reportAnswer = JsonSerializer.Deserialize<StatusReport>(answer.Value);
#if DEBUG
            Log.Info($"Statusrepart an Index {reportAnswer.Reference} erfolgreich protokolliert. Kann jetzt gelöscht werden.");
#endif
            DeleteSms(reportAnswer.Reference); //SMS aus GSM-Speicher löschen
            DeleteSms(reportAnswer.Index); //StatusReport aus GSM-Speicher löschen

        }

        /// <summary>
        /// Meldet einen eingehenden Sprachanruf an DB
        /// </summary>
        /// <param name="incomingCallphone"></param>
        internal static void CallRecieved(string incomingCallphone)
        {
            _ = Send(PipeName.MelBox2Service, Verb.CallRecieved, incomingCallphone);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        internal async static void SendGsmStatus(string property, string status)
        {
            // string query = JsonSerializer.Serialize(new KeyValuePair<string, string>(property, DateTime.Now + ":<br/>" + status));
            string query = JsonSerializer.Serialize(new Tuple<string, DateTime, string>(property, DateTime.Now, status));
            _ = await Pipe3.Send(PipeName.MelBox2Service, Verb.GsmStatus, query);
            //Keine Rückantwort erwartet
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

        #endregion
    }

}