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
using System.ComponentModel.Design;

namespace MelBox2Pipe
{
    internal class Program
    {
        static async Task Main()
        {
            Pipe2.StartPipeServer(Pipe2.PipeName.Gsm, true);
          
            Pipe2.StartPipeServer(Pipe2.PipeName.Email);

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
                Random random = new Random();

                switch (pipeName)
                {
                    case "sms":
                       
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
                    case Pipe2.Verb.ReportRecieved:
                        StatusReport report = new StatusReport(random.Next(1, 254), random.Next(1, 254), DateTime.UtcNow, 0);
                        var r = await Pipe2.Send(Pipe2.PipeName.MelBox2Service, Pipe2.Verb.ReportRecieved, JsonSerializer.Serialize(report));
                        StatusReport antwort = JsonSerializer.Deserialize<StatusReport>(r);
                        Console.WriteLine($"'{Pipe2.Verb.ReportRecieved}' erfolgreich. Report an Speicherplatz {antwort.Reference} kann gelöscht werden.");
                        break;
                    case Pipe2.Verb.CallRecieved:
                        string phone = "+999999999";
                        await Pipe2.Send(Pipe2.PipeName.MelBox2Service, Pipe2.Verb.CallRecieved, phone);
                        break;
                    case Pipe2.Verb.EmailRecieved:                        
                        Email email = new Email("test@email.com", new List<string> { "harm.schnakenberg@kreutztraeger.de" }, "Testmail Test");
                        await Pipe2.Send(Pipe2.PipeName.MelBox2Service, Pipe2.Verb.EmailRecieved, JsonSerializer.Serialize(email));
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

    /// <summary>
    /// Empfangsbestätigung / Sendebestätigung einer SMS 
    /// </summary>
    public class StatusReport
    {
        public StatusReport(int index, int reference, DateTimeOffset dischargeTimeUtc, int deliveryStatus)
        {
            Index = index;
            Reference = reference;
            DischargeTimeUtc = dischargeTimeUtc;
            DeliveryStatus = deliveryStatus;
        }

        public int Index { get; set; }

        public int Reference { get; set; }

        public DateTimeOffset DischargeTimeUtc { get; set; }

        public int DeliveryStatus { get; set; }

        public string DeliveryStatusText
        {
            get
            {
                //siehe Spezifikation GSM 3.40
                if (GetBit(DeliveryStatus, 6))
                    return "Fehler. Sendeversuch abgebrochen.";

                if (GetBit(DeliveryStatus, 5))
                    return "Fehler. Weiteren Sendeversuch abwarten.";

                if (GetBit(DeliveryStatus, 1))
                    return "Die Nachricht wurde vom Provider ersetzt.";

                if (GetBit(DeliveryStatus, 0))
                    return "Nachricht zugestellt. Keine Empfangsbestätigung.";

                if (DeliveryStatus == 0)
                    return "Empfangsbestätigung vom Empfänger erhalten.";

                return $"Sendunsgstatus = {DeliveryStatus}";
            }
        }

        private bool GetBit(int pByte, int bitNo)
        {
            return (pByte & (1 << bitNo)) != 0;
        }

    }

    public class Email
    {
        public Email(string from, List<string> to, string content)
        {
            From = from;
            To = to;
            Body = content;
        }

        public string From { get; set; }

        public List<string> To { get; set; }

        public string Body { get; set; }
    }

}
