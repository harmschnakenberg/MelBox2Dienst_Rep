//siehe auch https://michaeljohnpena.com/blog/namedpipes/

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    public static partial class Pipes
    {
//        /// <summary>
//        /// Aufzähöung der verwendeten NamedPipes Namen
//        /// </summary>
//        public class Name
//        {
//            public const string MelBox2Service = "MelBox2Service";
//            public const string Gsm = "Gsm";
//            public const string Email = "Email";
//        }

//        public class Verb
//        {
//            public const string SmsSend = "SmsSend";
//            public const string SmsRecieved = "SmsRecieved";
//            public const string EmailSend = "EmailSend";
//            //public const string EmailRecieved = "EmailRecieved";
//        }

//        public class Status
//        {
//            public const string New = "New";
//            public const string Success = "Success";
//            public const string Error = "Error";

//        }

//        /// <summary>
//        /// Startet einen Pipe-Server, der auf die Verbindung durch einen PipeClient wartet.
//        /// </summary>
//        /// <param name="pipeName">Name der Pipe. Muss auf Server und Client identisch sein</param>
//        internal static async void StartPipeServer2(string pipeName) 
//        {
//#if DEBUG
//            Log.Info($"StartPipeServer({pipeName})");
//#endif
//            try
//            {
//                using (var server = new NamedPipeServerStream(pipeName))
//                {
//                    await server.WaitForConnectionAsync();

//                    using (StreamReader reader = new StreamReader(server))
//                    using (StreamWriter writer = new StreamWriter(server))
//                    {
//                        while (server.IsConnected)
//                        {
//                            string line = await reader.ReadLineAsync();
//#if DEBUG
//                            Log.Info($"Anfrage an {pipeName} > {line}");
//#endif
//                            if (line != null)
//                            {
//                                string answer = ParseAnswer(line.Split('|'));
//#if DEBUG
//                                Log.Info($"Antwort von {pipeName} > {answer}");
//#endif
//                                #region Antwort zurück                            
//                                await writer.WriteLineAsync(answer);
//                                await writer.FlushAsync();
//                                #endregion
//                            }
//                        }

//                    }
//                    server.Close();
//                    server.Dispose();
//                }
//            }
//            catch (IOException)
//            {
//                StartPipeServer2(pipeName);
//            }
//            catch(NullReferenceException)
//            {

//            }
//            catch (Exception ex)
//            {
//                Log.Error("Fehler StartPipeServer2()\r\n\r\n" + ex.ToString());
//            }
//#if DEBUG
//            Log.Info($"PipeServer({pipeName}) beendet.");
//#endif
//        }


//        /// <summary>
//        /// Sendet einen Befehl und Inhalt mittels NamedPipe
//        /// </summary>
//        /// <param name="pipeName">Name der offenen NamedPipe, an die gesendet werden soll</param>
//        /// <param name="verb">Befehl, der vom NamedPipeServer interpreiert werden muss</param>
//        /// <param name="arg">Argument / Übergabeparameter zu dem Befehl im JSON-Format</param>
//        /// <returns></returns>
//        private static async Task<string> Send(string pipeName, string verb, string status, string arg)
//        {
//            using (var client = new NamedPipeClientStream(pipeName))
//            {
//               await  client.ConnectAsync();
//                using (StreamReader reader = new StreamReader(client))
//                using (StreamWriter writer = new StreamWriter(client))
//                {
//                    await writer.WriteLineAsync($"{verb}|{status}|{arg}");
//                    await writer.FlushAsync();
//                    return await reader.ReadLineAsync();
//                }
//            }
//        }

        /// <summary>
        /// Sendet eine string mittels NamedPipe
        /// </summary>
        /// <param name="pipeName">Name der Named Pipe, wie sie im NamedPipeServer hinterlegt ist</param>
        /// <param name="input">Inhalt der Nachricht</param>
        //private static string SendBACKUP(string pipeName, string input)
        //{

        //    var client = new NamedPipeClientStream(pipeName);
        //    client.Connect();
        //    StreamReader reader = new StreamReader(client);
        //    StreamWriter writer = new StreamWriter(client);

        //    writer.WriteLine(input);
        //    writer.Flush();
        //    return reader.ReadLine();
        //}

    }

    #region Magazin


    //public static string PipeNameMelbox2Servi { get; private set; } = "MelBox2Service";

    //Quelle: https://stackoverflow.com/questions/13806153/example-of-named-pipes
    //internal static void StartPipeServer(string pipeName)
    //{
    //    Log.Info($"StartPipeServer({pipeName})");

    //    Task.Factory.StartNew(() =>
    //    {
    //        #region PipeServer erstellen und auf Verbindung warten
    //        using (var server = new NamedPipeServerStream(pipeName))
    //        {
    //            server.WaitForConnection();
    //            Log.Info($"PipeServer {pipeName} verbunden.");
    //            using (StreamReader reader = new StreamReader(server))
    //            using (StreamWriter writer = new StreamWriter(server))
    //            {
    //                #endregion

    //                try
    //                {
    //                    while (server.IsConnected)
    //                    {
    //                        #region String von PipeClient empfangen
    //                        var line = reader.ReadLine();
    //                        #endregion

    //                        //ParseAnswer(pipeName, line);
    //                        Log.Info($"Pipe '{pipeName}': {line}");

    //                        #region Antwort zurück
    //                        //writer.WriteLine(String.Join("", line.Reverse()));
    //                        writer.WriteLine("#" + line);
    //                        writer.Flush();
    //                        #endregion
    //                    }
    //                }
    //                finally
    //                { 
    //                    server.Close(); 
    //                    server.Dispose();
    //                }
    //            }
    //        }

    //        Log.Info($"PipeServer {pipeName} beendet.");
    //    });

    //    StartPipeServer(pipeName);

    //}

    //internal static void StartPipeServer(string pipeName)
    //{
    //    Log.Info($"StartPipeServer({pipeName})");

    //    Task.Factory.StartNew(() =>
    //    {
    //        #region PipeServer erstellen und auf Verbindung warten
    //        var server = new NamedPipeServerStream(pipeName);
    //        server.WaitForConnection();
    //        StreamReader reader = new StreamReader(server);
    //        StreamWriter writer = new StreamWriter(server);
    //        #endregion

    //        while (true)
    //        {
    //            #region String von PipeClient empfangen
    //            var line = reader.ReadLine();
    //            #endregion

    //            //mach etwas mit line

    //            Log.Info($"{pipeName} > {line}");


    //            //ParseAnswer(pipeName, line);

    //            #region Antwort zurück
    //            //writer.WriteLine(String.Join("", line.Reverse()));
    //            writer.WriteLine(line);
    //            writer.Flush();
    //            #endregion
    //        }
    //    });

    //    Log.Info($"PipeServer({pipeName}) beendet.");
    //}



    #endregion
}

