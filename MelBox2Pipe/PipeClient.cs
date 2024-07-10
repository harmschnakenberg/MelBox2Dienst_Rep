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
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace MelBox2Pipe
{
    internal class Program
    {
        static async Task Main()
        {
            Pipe2.StartPipeServer(Pipe2.PipeName.Gsm, true);
            //Pipes.StartPipeServer2(Pipes.Name.Gsm);
            //Pipes.StartPipeServer2(Pipes.Name.Email);

            Console.WriteLine("Testprogramm zur Kommunikation mit NamedPipes.\r\n" +
                "Anleitung: PipeName eingeben <Enter>.\r\n" +
                "Pipe Inhalt eingeben <Enter>.\r\n" +
                "Antwort wird an der Console ausgegeben");

            while (true)
            {
                Console.WriteLine("\r\nPipe Name:");
                Console.WriteLine("\t" + Pipe2.Verb.SmsRecieved);
                //Console.WriteLine("\t" + Pipes.Verb.SmsSend);
                Console.WriteLine("Pipe Name?");

                string pipeName = Console.ReadLine();

                switch (pipeName)
                {
                    case "sms":
                        Random random = new Random(); 
                        Sms simSmsAuto = new Sms(random.Next(1, 254), DateTime.Now, "+49123456789", "PipeTest, Simulierte SMS");
                        var x = await Pipe2.Send(Pipe2.PipeName.MelBox2Service, Pipe2.Verb.SmsRecieved, JsonSerializer.Serialize(simSmsAuto));
                        Console.WriteLine($"Antwort von {Pipe2.PipeName.MelBox2Service}:\r\n\t{x}");
                        //TODO In PRODUCTION: simSmsAuto.Index aus GSM-Modem-Speicher löschen.
                        break;
                    case Pipe2.Verb.SmsRecieved:
                        Console.WriteLine("Inhalt in der Form\r\nId (0..255); Absender (+49123..); Inhalt");
                        string[] args = Console.ReadLine().Split(';');
                        if (args.Length < 3) break;

                        int.TryParse(args[0], out int id);
                        Sms simSms = new Sms(id, DateTime.Now, args[1], args[2]);
                        await Pipe2.Send(Pipe2.PipeName.MelBox2Service, Pipe2.Verb.SmsRecieved, JsonSerializer.Serialize(simSms));
                    
                        break;
                    case Pipe2.Verb.SmsSend:
                        Console.WriteLine($"'{Pipe2.Verb.SmsSend}' ist nicht implementiert.");
                        break;
                    case Pipe2.Verb.CallRecieved:
                        string phone = "+999999999";
                        await Pipe2.Send(Pipe2.PipeName.MelBox2Service, Pipe2.Verb.CallRecieved, phone);

                        //Console.WriteLine($"'{Pipe2.Verb.CallRecieved}' ist nicht implementiert.");
                        break;
                    case "exit":
                        Console.WriteLine("Testprogramm beendet..");
                        Console.ReadKey();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine($"'{pipeName}' ist unbekannt.");
                        break;
                }
            }


            //Pipes.StartPipeServer2(Pipes.Name.EmailSend);

            //Pipes.Send(Pipes.Name.SmsRecieved);

        }
    }

    //class Pipes
    //{

    //    public class Name 
    //    {
    //        public const string SmsSend = "smsSend";
    //        public const string SmsRecieved = "smsRecieved";
    //        public const string EmailSend = "emailSend";
    //        public const string EmailRecieved = "emailRecieved";

    //    }


    //    internal static void Send2(string pipeName, Sms sms)
    //    {
    //        var client = new NamedPipeClientStream(pipeName);
    //        client.Connect();
    //        StreamReader reader = new StreamReader(client);
    //        StreamWriter writer = new StreamWriter(client);

    //        var input = JsonSerializer.Serialize<Sms>(sms);

    //        writer.WriteLine(input);
    //        writer.Flush();
    //        Console.WriteLine(reader.ReadLine());

    //    }



    //    /// <summary>
    //    /// Sendet eine string mittels NamedPipe
    //    /// </summary>
    //    /// <param name="pipeName">Name der Named Pipe, wie sie im NamedPipeServer hinterlegt ist</param>
    //    /// <param name="input">Inhalt der Nachricht</param>
    //    //internal static void Send(string pipeName)
    //    //{
    //    //    var client = new NamedPipeClientStream(pipeName);
    //    //    client.Connect();
    //    //    StreamReader reader = new StreamReader(client);
    //    //    StreamWriter writer = new StreamWriter(client);

    //    //    while (true)
    //    //    {
    //    //        string input = Console.ReadLine();
    //    //        if (input == "sms")
    //    //            input = JsonSerializer.Serialize<Sms>(new Sms(99, DateTime.Now, "+4934567890", "Harm, Ein Test."), JsonSerializerOptions.Default);

    //    //        if (input?.Length == 0) break;
    //    //        writer.WriteLine(input);
    //    //        writer.Flush();
    //    //        Console.WriteLine("\tANTWORT:");
    //    //       Console.WriteLine("\t" + reader.ReadLine());
    //    //    }
    //    //}

    //    internal static async void StartPipeServer2(string pipeName)
    //    {

    //        try
    //        {
    //            using (var server = new NamedPipeServerStream(pipeName))
    //            {
    //                await server.WaitForConnectionAsync();

    //                using (StreamReader reader = new StreamReader(server))
    //                using (StreamWriter writer = new StreamWriter(server))
    //                {
    //                    while (server.IsConnected)
    //                    {
    //                        string line = await reader.ReadLineAsync();
    //                        Console.WriteLine($"{pipeName} > {line}");

    //                        string answer = ParseAnswer(pipeName, line);

    //                        #region Antwort zurück                            
    //                        await writer.WriteLineAsync(answer);
    //                        await writer.FlushAsync();
    //                        #endregion
    //                    }

    //                }
    //                server.Close();
    //                server.Dispose();
    //            }
    //        }
    //        catch (IOException)
    //        {
    //            StartPipeServer2(pipeName);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine("Fehler StartPipeServer2()\r\n\r\n" + ex.ToString());
    //        }

    //        Console.WriteLine($"PipeServer({pipeName}) beendet.");
    //    }

    //    private static string ParseAnswer(string pipeName, string input)
    //    {
    //        try
    //        {
    //            switch (pipeName)
    //            {
    //                case Name.SmsRecieved:
    //                    try
    //                    {
    //                        //Wenn erfolgreich gespeichert, wird die Roh-SMS zurückgesendet
    //                        Sms sms = JsonSerializer.Deserialize<Sms>(input);

    //                        //TODO: Wenn erfolgreich verarbeitet, kann die SMS gelöscht werden
    //                        //AT+CMGD=<index> // sms.Index

    //                        return "";
    //                    }
    //                    catch
    //                    {
    //                        return "Kein Erfolg";
    //                    }
    //                case Name.EmailSend:

    //                    return "";
    //                default:                       
    //                    return "random_antwort";

    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            return ex.ToString();
    //        }

    //    }


    //}

//    public static partial class Pipes
//    {
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
//            Console.WriteLine($"StartPipeServer({pipeName})");
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
//                            Console.WriteLine($"{pipeName} > {line}");
//#endif
//                            if (line != null)
//                            {
//                                //return $"{verb}|{Pipes.Status.Success}|{arg}";
//                                string answer = ParseAnswer(line.Split('|'));

//                                #region Antwort zurück 
//                                if (answer?.Length > 0)
//                                {
//                                    await writer.WriteLineAsync(answer);
//                                    await writer.FlushAsync();
//                                }
//                                #endregion
//                            }
//                        }

//                    }
//                    server.Close();
//                    server.Dispose();
//                }
//            }
//            catch (IOException ex_io)
//            {
//                Console.WriteLine("IO-Fehler StartPipeServer2()\r\n\r\n" + ex_io.ToString());
//                 StartPipeServer2(pipeName);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Fehler StartPipeServer2()\r\n\r\n" + ex.ToString());
//            }
//#if DEBUG
//            Console.WriteLine($"PipeServer({pipeName}) beendet.");
//#endif
//        }


//        /// <summary>
//        /// Sendet einen Befehl und Inhalt mittels NamedPipe
//        /// </summary>
//        /// <param name="pipeName">Name der offenen NamedPipe, an die gesendet werden soll</param>
//        /// <param name="verb">Befehl, der vom NamedPipeServer interpreiert werden muss</param>
//        /// <param name="arg">Argument / Übergabeparameter zu dem Befehl im JSON-Format</param>
//        /// <returns></returns>
//        internal static async Task<string> Send(string pipeName, string verb, string status, string arg)
//        {
//            using (var client = new NamedPipeClientStream(pipeName))
//            {
//                await client.ConnectAsync();
//                using (StreamReader reader = new StreamReader(client))
//                using (StreamWriter writer = new StreamWriter(client))
//                {
//                    await writer.WriteLineAsync($"{verb}|{status}|{arg}");
//                    await writer.FlushAsync();
//                    return await reader.ReadLineAsync();
//                }
//            }
//        }


//        private static string ParseAnswer(string[] args)
//        {
//            if (args.Length < 3) return args.ToString();

//            string verb = args[0];
//            string status = args[1];
//            string arg = args[2];

//            try
//            {
//                switch (verb)
//                {
//                    case Verb.SmsRecieved:
//                        try
//                        {
//                            if (status == Pipes.Status.New) //nicht vorgesehener Fall
//                                return Status.Error;

//                            Sms sms = JsonSerializer.Deserialize<Sms>(arg);

//                            if (status == Pipes.Status.Success)
//                            {                                
//                                Console.WriteLine($"Erfolgreich eingegangene SMS verarbeitet:\r\n\r\nSMS:\r\nIndex:\t{sms.Index}\r\nvon:\t{sms.Phone}\r\nInhalt:\t{sms.Content}\r\n\r\n\tDIE NACHRICHT KANN NUN AU DEM GSM-MODM GELÖSCHT WERDEN.");

//                                //TODO: In der GSM-Anwendung die SMS aus dem Speicher des Modems löschen
//                                return string.Empty;
//                            }
//                            else
//                            {
//                                Console.WriteLine($"Fehler beim Verarbeiten der eingegangen SMS:\r\n\r\nSMS:\r\nIndex:\t{sms.Index}\r\nvon:\t{sms.Phone}\r\nInhalt:\t{sms.Content}\r\n\r\n\tDIE NACHRICHT KANN NUN AU DEM GSM-MODM GELÖSCHT WERDEN.");
//                                return Status.Error;
//                            }
//                        }
//                        catch
//                        {
//                            return Status.Error;
//                        }
//                    case Verb.SmsSend:
//                        if (status != Pipes.Status.New) //nicht vorgesehener Fall
//                            return $"{verb}|{Status.Error}|{arg}";

//                        Sms sms2 = JsonSerializer.Deserialize<Sms>(arg);

//                        //TODO: In der GSM-Anwendung die SMS versenden und den internen Index an MelBox zurückgeben

//                        bool helper_IsSmsSendFromGsmModem = true;
//                        int helper_smsInternalIndex = 255;

//                        if (helper_IsSmsSendFromGsmModem)
//                        {
//                            //Gleiche SMS mit dem aktualiserten Index as Gsm-Modem zurück an DB
//                            sms2.Index = helper_smsInternalIndex;
//                            return $"{verb}|{Status.Success}|{sms2}";
//                        }
//                        else
//                            return Status.Error;
//                    case Verb.EmailSend:
//                        Console.WriteLine("Email versenden ist noch nicht implementiert.");
//                        return $"{verb}|{Status.Error}|{arg}";
//                    default:
//                        string x = $"Unerwartete Anfrage Pipe '{args}'";
//                        Console.WriteLine(x);
//                        return $"{verb}|{Status.Error}|{arg}";
//                }
//            }
//            catch (Exception ex)
//            {
//                return $"{verb}|{Status.Error}|{ex}";
//            }

//        }

//    }
   
    
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

        public override string ToString() 
        {
            return $"Index:\t{Index}\r\nTime:\t{Time}\r\nPhone:\t{Phone}\r\nContent:\t{Content}\r\n";
        }
    }

}
