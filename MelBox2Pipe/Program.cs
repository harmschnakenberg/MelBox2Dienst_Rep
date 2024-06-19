using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using System.Text.Json;

namespace MelBox2Pipe
{
    internal class Program
    {

        static void Main()
        {
            

            // Pipes.StartPipeServer(pipeName);

            //while (true)
            //{
                string input = Console.ReadLine();
                //if (String.IsNullOrEmpty(input)) break;
                //if (input.ToLower() == "end") break;
                //if (input.ToLower() == "sms")
                //    input = SimSms();
                Pipes.Send(Pipes.Name.Sms, input);
            //}

           Console.WriteLine("Testprogramm beendet..");
            Console.ReadKey();
        }


        static string SimSms()
        {
            Sms sms = new Sms(99, DateTime.UtcNow, "+123456789", "Der Inhalt der SMS");

            return JsonSerializer.Serialize(sms);
        }
    }

    class Pipes
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

            while (true)
            {
                input = Console.ReadLine();
                if (input == "sms")
                    input = JsonSerializer.Serialize<Sms>(new Sms(99, DateTime.Now, "34567890", "Ein Test."), JsonSerializerOptions.Default);

                if (input?.Length == 0) break;
                writer.WriteLine(input);
                writer.Flush();
                Console.WriteLine(reader.ReadLine());
            }
        }

        /// <summary>
        /// Startet einen Pipe-Server, der auf die Verbindung durch einen PipeClient wartet.
        /// </summary>
        /// <param name="pipeName">Name der Pipe. Muss auf Server und Client identisch sein</param>
        //Quelle: https://stackoverflow.com/questions/13806153/example-of-named-pipes
        internal static void StartPipeServer(string pipeName)
        {
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

                   
                    #region Antwort zurück
                    writer.WriteLine(String.Join("", line.Reverse()));
                    writer.Flush();
                    #endregion
                }
            });
        }

    }


    /// <summary>
    /// SMS
    /// </summary>
    public class Sms
    {
        public Sms(int index, DateTimeOffset time, string phone, string content)
        {
            Index = index;
            Time = time;
            Phone = phone;
            Content = content;
        }

        public int Index { get; set; }

        public DateTimeOffset Time { get; set; }

        public string Phone { get; set; }

        public string Content { get; set; }
    }

}
