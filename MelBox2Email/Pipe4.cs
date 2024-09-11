using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MelBox2Email
{
    internal partial class Pipe4
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
            public const string EmailStatus = "EmailStatus";
            public const string CallRelay = "CallRelay";
            public const string CallRecieved = "CallRecieved";
            public const string GsmStatus = "GsmStatus";
            public const string LastError = "LastError";
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
                          //  Log.Info($"Anfrage an {pipeName} > '{line}'");
#endif
                            if (line == null)
                                break;

                            string answer = ParseQuery(line);
#if DEBUG
                           // Log.Info($"Antwort von {pipeName} > '{answer}'");
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
                Console.WriteLine("Fehler StartPipeServer2()\r\n\r\n" + ex.ToString());
               // Log.Error("Fehler StartPipeServer2()\r\n\r\n" + ex.ToString());
            }

#if DEBUG
          //  Log.Info($"PipeServer({pipeName}) beendet.");
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
                        string result = await reader.ReadLineAsync();
                        if (result == null)
                            return new KeyValuePair<string, string> (string.Empty, string.Empty);

                        string[] args = result.Split('|');
                        return new KeyValuePair<string, string>(args[0], args[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log.Error
                Console.WriteLine($"Fehler beim Senden an Pipe: {pipeName}|{verb}" +
                     $"\r\n\t{ex.GetType()}: {ex.Message}" +
                     $"\r\n\t{arg}");
                return new KeyValuePair<string, string>(verb, string.Empty);
            }
        }
        #endregion
    }

    internal partial class Pipe4
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
                case Verb.EmailSend:
                    MelBox2EmailService.OutlookApp_SendMail(JsonSerializer.Deserialize<Email>(arg));
                    return Answer(verb, arg);
                    case Verb.EmailRecieved: 
                    //keine Antwort erwartet
                    return Answer(verb, string.Empty);
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
            _ = await Send(PipeName.MelBox2Service, Verb.LastError, msg);
            //Log.Error(msg);
        }

        internal static async void EmailRecieved(Email email)
        {
            
            _ = await Send(PipeName.MelBox2Service, Verb.EmailRecieved, JsonSerializer.Serialize(email));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        internal async static void SendEmailStatus(string property, string status)
        {            
            string query = JsonSerializer.Serialize(new Tuple<string, DateTime, string>(property, DateTime.Now, status));
            _ = await Pipe4.Send(PipeName.MelBox2Service, Verb.EmailStatus, query);
            //Keine Rückantwort erwartet
        }


        #endregion
    }

    public class Email
    {
        public Email()
        {
            //ohne leeren Constructor Fehler:
            // ystem.InvalidOperationException: Each parameter in the deserialization constructor on type 'MelBox2Dienst.Email' must bind to an object property or field on deserialization. Each parameter name must match with a property or field on the object. Fields are only considered when 'JsonSerializerOptions.IncludeFields' is enabled. The match can be case-insensitive.
        }

        public Email(string from, List<string> to, List<string> cc, string subject, string content)
        {
            From = from;
            To = to ?? new List<string>();
            Cc = cc ?? new List<string>();
            Subject = subject;
            Body = content;
        }

        public string From { get; set; }

        public List<string> To { get; set; } = new List<string>();

        public List<string> Cc { get; set; } = new List<string>();

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
