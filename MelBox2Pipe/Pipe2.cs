using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MelBox2Pipe
{
    internal partial class Pipe2
    {
        #region Feste Begriffe für Pipe-Kommunikation
        internal static class PipeName
        {
            internal static string MelBox2Service { get; } = "MelBox2Service";
            internal static string Gsm { get; } = "Gsm";
            internal static string Email { get; } = "Email";
        }

        public class Verb
        {
            //public const string StartPipe = "StartPipe";
            public const string SmsSend = "SmsSend";
            public const string SmsRecieved = "SmsRecieved";
            public const string ReportRecieved = "ReportRecieved";
            //public const string EmailSend = "EmailSend";
            public const string EmailRecieved = "EmailRecieved";
            public const string CallRelay = "CallRelay";
            public const string CallRecieved = "CallRecieved";
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
                            Console.WriteLine($"Anfrage an {pipeName} > {line}");
#endif
                            string answer = ParseQuery(line);
#if DEBUG
                            Console.WriteLine($"Antwort von {pipeName} > {answer}");
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
            catch (Exception ex)
            {
               Console.WriteLine("Fehler StartPipeServer2()\r\n\r\n" + ex.ToString());
            }
            finally
            {
                if (perpetual)
                    StartPipeServer(pipeName, true);
            }
#if DEBUG
            Console.WriteLine($"PipeServer({pipeName}) beendet.");
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
        internal static async Task<string> Send(string pipeName, string verb, string arg)
        {
            using (var client = new NamedPipeClientStream(pipeName))
            {
                await client.ConnectAsync();
                using (StreamReader reader = new StreamReader(client))
                using (StreamWriter writer = new StreamWriter(client))
                {
                    await writer.WriteLineAsync($"{verb}|{arg}");
                    await writer.FlushAsync();
                    return await reader.ReadLineAsync();
                }
            }
        }
        #endregion
    }

    internal partial class Pipe2
    {
        /// <summary>
        /// Wertet die Anfrage der NamedPipe aus
        /// </summary>
        /// <param name="line">Anfrage-String aus einer NamedPipe</param>
        /// <returns>Antwort, die an die NamedPipe auf die Anfrage weitergegeben wird. Ansonsten 'null'</returns>
        private static string ParseQuery(string line)
        {
            string[] args = line?.Split('|');
            if (args?.Length != 2)
                return null;

            string verb = args[0];
            string arg = args[1];

            switch (verb)
            {
                case Verb.SmsRecieved:
                    //Antwort bei Erfolg auf die Anfrage "SMS empfangen"; Diese SMS ist damit in DB registriert.
                    Sms smsRec = JsonSerializer.Deserialize<Sms>(arg);                    
                    Console.WriteLine("Empfangene SMS ist in DB registriert:\r\n" + smsRec.ToString() );

                    //TODO IN PRODUCTION: Diese Empfangene SMS kann aus dem GSM-Modem gelöscht werden.

                    //Keine Rückantwort
                    return null; 
                case Verb.SmsSend:
                    //Anfrage eine SMS zu versenden.
                    Sms smsSend = JsonSerializer.Deserialize<Sms>(arg);

                    //TODO IN PRODUCTION: GSM-Modem sendet diese SMS raus. Der vom GSM-Modem vergebene Index wird in das SMS-Object geschrieben. 
                    smsSend.Index = new Random().Next(1, 254); //Simuliert erfolgreich versendete SMS.
                    
                    //Antwort: angefragte SMS zurück mit aktuellem Index aus GSM-Modem für diese gesendete SMS.
                    return Answer(Verb.SmsSend, JsonSerializer.Serialize(smsSend) );
                case Verb.CallRelay:
                    string desiredCallRelayPhone = arg;

                    //TODO IN PRODUCTION: GSM-Modem weist 'esiredCallRelayPhone' für die Rufumleitung zu,
                    //fragt dann die aktuelle Rufumleitung für Sprachanrufe ab und gibt das Ergebnis als Antwort zurück zur DB.

                    string confirmedRelayPhone = desiredCallRelayPhone;// "+4987654321";//Simulierte aktuelle Rufumleitung
                    //Antwort: Aus dem GSM-Modem ausgelesene, aktive Nummer für Rufumleitung.
                    return Answer(Verb.CallRelay, confirmedRelayPhone);
                case Verb.LastError:
                    Console.WriteLine("Ein Fehler wurde zurückgegeben für:\r\n\t" + arg);

                    return null;
                default:
                    Console.WriteLine($"Die Anfrage '{verb}' für:\r\n\t" + arg);
                    return Answer(verb, "unbekannt");

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
        /// Prüft, ob ein String dem Format '+000000...' entspricht
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool IsPhoneNumber(string number)
        {
            return Regex.Match(number, @"(\+[0-9]+)").Success;
        }



    }

}
