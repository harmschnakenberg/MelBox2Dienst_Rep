using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
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
                            Log.Info($"Anfrage an {pipeName} > '{line}'");
#endif
                            if (line == null)
                                break;

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
            catch (Exception ex)
            {
                Log.Error($"Fehler beim Senden an Pipe: {pipeName}|{verb}" +
                    $"\r\n\t{ex.GetType()}: {ex.Message}" +
                    $"\r\n\t{arg}");
                return string.Empty; 
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
                //                case Verb.SmsRecieved:
                //                    //Anfrage von GSM-Modem eine EMpfangene SMS zu registrieren
                //                    Sms smsRec = JsonSerializer.Deserialize<Sms>(arg);
                //                    if (Sql.MessageRecieveAndRelay(smsRec))
                //                        return line; //Erfolg: sende die Anfrage ungverändert zurück
                //                    else
                //                        return Answer(Verb.Error, arg);
                //                case Verb.SmsSend:
                //                    return Answer(verb, "nicht implementiert");
                //                case Verb.ReportRecieved:
                //                    StatusReport report = JsonSerializer.Deserialize<StatusReport>(arg);
                //#if DEBUG
                //                    Log.Info($"SMS-StatusReport an Referenz {report.Reference}: {report.DeliveryStatusText}");
                //#endif
                //                    if (Sql.UpdateSentSms(report))
                //                        return line; //Erfolg: sende die Anfrage ungverändert zurück
                //                    else
                //                        return Answer(Verb.Error, arg);
                case Verb.CallRelay:
                    //Anfrage zum Ändern der Rufumleitung
                    
                    CallRelay callRelay = JsonSerializer.Deserialize<CallRelay>(arg);

                    if (IsPhoneNumber(callRelay.Phone))
                    {
                        callRelay = SetCallRedirection(callRelay.Phone);

                        if (callRelay.Phone != CallRelayPhone)
                        {
                            callRelay.Status = $"Rufumleitung umgeschaltet von '{CallRelayPhone}' auf '{callRelay.Phone}'";
                            Log.Info(callRelay.Status);
                            CallRelayPhone = callRelay.Phone;
                        }
                    }
                   
                    return Answer(Verb.CallRelay, JsonSerializer.Serialize(callRelay));
                //                case Verb.CallRecieved:
                //                    string callingNumber = arg;
                //                    //eingegangenen Sprachanruf protokollieren
                //                    _ = Sql.CreateSent(callingNumber, Sql.CallRelayPhone);

                //                    //keine Antwort erforderlich (GSM-Modem leitet selbständig weiter)
                //                    return null;
                //                case Verb.EmailRecieved:
                //                    Email email = JsonSerializer.Deserialize<Email>(arg);

                //                    if (Sql.MessageRecieveAndRelay(smsRec))
                //                        return line; //Erfolg: sende die Anfrage ungverändert zurück
                //                    else
                //                        return Answer(Verb.Error, arg);


                //                    _ = Sql.CreateRecievedMessage(email);

                //                    return null;
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
            //Sende Empfangene SMS zur DB
            var answer = await Send(PipeName.MelBox2Service, Verb.SmsRecieved, JsonSerializer.Serialize<Sms>(smsIn));
            //Bei Erfolg wird die gleiche Nachricht zurückgesandt
            Sms back = JsonSerializer.Deserialize<Sms>(answer);
            //Entferne SMS aus GSM-Speicher
            DeleteSms(back.Index);
        }

        internal async static void ReportRecieved(StatusReport report)
        {            
            var r = await Pipe3.Send(Pipe3.PipeName.MelBox2Service, Pipe3.Verb.ReportRecieved, JsonSerializer.Serialize(report));
            //Bei erfolg wird die Anfrage unverändetr zurückgeschickt
            StatusReport answer = JsonSerializer.Deserialize<StatusReport>(r);
#if DEBUG
            Log.Info($"Statusrepart an Index {answer.Reference} erfolgreich protokolliert. Kann jetzt gelöscht werden.");
#else
            DeleteSms(answer.Reference); //SMS aus GSM-Speicher löschen
            DeleteSms(answer.Index); //StatusReport aus GSM-Speicher löschen
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        internal async static void SendGsmStatus(string property, string status)
        {
            string query = JsonSerializer.Serialize(new KeyValuePair<string, string>(property, status));
            _ = await Pipe3.Send(PipeName.MelBox2Service, Verb.GsmStatus, query);
            //Keine Rückantwort erwartet
        }


        /// <summary>
        /// Veranlasst das Senden von ein/mehreren SMS. Protokolliert den erfolgreichen Versand in DB
        /// </summary>
        /// <param name="guards">Liste der Telefonnummern, an die die SMS gesendet werden soll</param>
        /// <param name="sms">Sms-Object</param>
        /// <param name="messageId">Id des Nachrichteninhalts aus der DB</param>
        //internal async static void SendSms(Sms sms)
        //{

        //        if (IsPhoneNumber(sms.Phone))
        //        {                    
        //            string result = await Send(PipeName.Gsm, Verb.SmsSend, JsonSerializer.Serialize<Sms>(sms));

        //            #region SMS-Id der gesendeten SMS in DB eintragen.
        //            //Erwartete Antwort : 'SmsSend|[JSON-Object Sms]' mit sms.Index = aktueller Index in GSM-Modem.

        //            string[] args = result?.Split('|');
        //            if (args.Length != 2)
        //            {
        //                Log.Error($"Sms-Versand an '{phone}' konnte nicht bestätigt werden; Antwort von DB ungültig: '{result}'");
        //                break;
        //            }

        //            Sms smsAnswer = JsonSerializer.Deserialize<Sms>(args[1]);

        //            if (smsAnswer.Index > 0) //-> Die SMS wurde versand und im GSM-Modem auf einem Index gespeichert.
        //                Sql.CreateSent(messageId, smsAnswer);
        //            else
        //                Log.Error($"Sms-Versand an '{phone}' konnte nicht bestätigt werden; '{sms.Content}'");
        //            #endregion
        //        }


        //}

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