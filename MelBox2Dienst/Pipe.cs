//siehe auch https://michaeljohnpena.com/blog/namedpipes/

using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    public class Pipes
    {
        public class Name
        {
            public const string Sms = "sms";

        }


        /// <summary>
        /// Sendet eine string mittels NamedPipe
        /// </summary>
        /// <param name="pipeName">Name der Named Pipe, wie sie im NamedPipeServer hinterlegt ist</param>
        /// <param name="input">Inhalt der Nachricht</param>
        internal static void Send(string pipeName, string input)
        {
            var client = new NamedPipeClientStream(pipeName);
            client.Connect();
            StreamReader reader = new StreamReader(client);
            StreamWriter writer = new StreamWriter(client);

            writer.WriteLine(input);
            writer.Flush();
            Console.WriteLine(reader.ReadLine());   
        }

        /// <summary>
        /// Startet einen Pipe-Server, der auf die Verbindung durch einen PipeClient wartet.
        /// </summary>
        /// <param name="pipeName">Name der Pipe. Muss auf Server und Client identisch sein</param>
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

        internal static void StartPipeServer(string pipeName)
        {
            Log.Info($"StartPipeServer({pipeName})");

            Task.Factory.StartNew(() =>
            {
                #region PipeServer erstellen und auf Verbindung warten
                var server = new NamedPipeServerStream(pipeName);
                server.WaitForConnection();
                StreamReader reader = new StreamReader(server);
                StreamWriter writer = new StreamWriter(server);
                #endregion

                while (true)
                {
                    #region String von PipeClient empfangen
                    var line = reader.ReadLine();
                    #endregion

                    //mach etwas mit line

                    Log.Info($"{pipeName} > {line}");
                 
                    
                    //ParseAnswer(pipeName, line);

                    #region Antwort zurück
                    //writer.WriteLine(String.Join("", line.Reverse()));
                    writer.WriteLine(line);
                    writer.Flush();
                    #endregion
                }
            });

            Log.Info($"PipeServer({pipeName}) beendet.");
        }

        internal static async void StartPipeServer2(string pipeName) 
        {
            Log.Info($"StartPipeServer({pipeName})");

            try
            {
                using (var server = new NamedPipeServerStream(pipeName))
                {
                    await server.WaitForConnectionAsync();

                    using (StreamReader reader = new StreamReader(server))
                    using (StreamWriter writer = new StreamWriter(server))
                    {
                        while (server.IsConnected)
                        {
                            string line = await reader.ReadLineAsync();

                            Log.Info($"{pipeName} > {line}");

                            //ParseAnswer(pipeName, line);

                            #region Antwort zurück
                            //writer.WriteLine(String.Join("", line.Reverse()));
                            //writer.WriteLine(line);

                            await writer.WriteLineAsync(line);
                            await writer.FlushAsync();

                            #endregion
                        }

                    }
                    server.Close();
                    server.Dispose();

                }
            } 
            catch
            {
                //TODO Abfangen
            }
            Log.Info($"PipeServer({pipeName}) beendet.");
        }


        private static void ParseAnswer(string pipeName, string input)
        {
            switch (pipeName)
            {
                case Name.Sms:
                    Sms sms = JsonSerializer.Deserialize<Sms>(input);
                    Log.Info($"{input}\r\n\r\nSMS:\r\nIndex:\t{sms.Index}\r\nvon:\t{sms.Phone}\r\nInhalt:\t{sms.Content}");
                    break;
                default:
                    Log.Info($"Pipe '{pipeName}': {input}");
                    break;
            }
        }

    }

}

