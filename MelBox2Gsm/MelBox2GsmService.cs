using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static MelBox2Gsm.Program;
using System.Configuration;
using System.Threading;

namespace MelBox2Gsm
{
    public partial class MelBox2GsmService : ServiceBase
    {
       
        public static int GsmPokeInterval { get; set; } = 30; // int.Parse(ConfigurationManager.AppSettings["GsmPokeInterval"]);

        public MelBox2GsmService()
        {
            InitializeComponent();

            this.ServiceName = "MelBox2GsmDienst";
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            Thread.Sleep(5000);
            #region GSM-Modem vorbereiten
            SetupGsmModem();
 

            #endregion

            // Set up a timer that triggers every minute.
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = GsmPokeInterval * 1000
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

        }

        protected override void OnStop()
        {
            //Setze Rufumleitung zurück
            DeactivateCallRedirection();

            Log.Info($"{this.ServiceName} wurde beendet.");
        }

        internal void TestStartupAndStop()
        {
            this.OnStart(new string[0]);
            Console.WriteLine($"{this.ServiceName} wurde als Konsolenanwendung gestartet. Beliebige Taste zum beenden..");
            //Console.ReadLine();

            #region Manuelle AT-Befehle
            try
            {
                Console.WriteLine(
                    "Hinweis: AT-Befehle eingeben und mit Eingabetaste abschicken.\r\n" +
                    "exit = beenden\r\n" +
                    "sim = SMS-Empfang simulieren\r\n" +
                    "trace = Rohdaten GSM-Kommunikation ein/ausblenden");

                while (true)
                {
                    string request = Console.ReadLine();

                    if (request.ToLower() == "sim")
                        SimulateSmsRecieved();

                    else if (request.ToLower() == "exit") break;
                    else
                        _ = Program.Port.Ask(request);
                }
            }
            finally { Port.Dispose(); }
            #endregion
            this.OnStop();
        }



        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            //_eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);

#if DEBUG
            if (Environment.UserInteractive)
                Console.WriteLine(DateTime.Now);
#endif

            GetSignalQuality();
            GetSms();
        
        }
    }
}
