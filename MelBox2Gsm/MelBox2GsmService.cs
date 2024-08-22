using System;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using static MelBox2Gsm.Program;

namespace MelBox2Gsm
{
    public partial class MelBox2GsmService : ServiceBase
    {
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
            Program.Port.Close();
            Port.Dispose();            
            Log.Info($"{this.ServiceName} wurde beendet.");
            Thread.Sleep(3000);
        }

        internal void TestStartupAndStop()
        {
            this.OnStart(new string[0]);
            Console.WriteLine($"{this.ServiceName} wurde als Konsolenanwendung gestartet. Beliebige Taste zum beenden..");

            #region Manuelle AT-Befehle

            Console.WriteLine(
                "Hinweis: AT-Befehle eingeben und mit Eingabetaste abschicken.\r\n" +
                "exit = beenden\r\n" +
                "sim = SMS-Empfang simulieren\r\n" +
                "senden = SMS versenden an Handy Harm\r\n"
                );

            while (true)
            {
                string request = Console.ReadLine();

                if (request?.ToLower() == "sim")
                    SimulateSmsRecieved();
                else if (request?.ToLower() == "senden")
                    SimulateSmsSend();
                else if (request?.ToLower() == "exit")
                    break;
                else if (request?.Length > 1) //Als AT-Befehl interpretieren
                    _ = Program.Port.Ask(request);
            }

            #endregion
            
            this.OnStop();
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
//#if DEBUG
//            if (Environment.UserInteractive)
//                Console.WriteLine(DateTime.Now);
//#endif
            GetNetworkRegistration();
            GetSignalQuality();
            GetSms();        
        }
    }
}
