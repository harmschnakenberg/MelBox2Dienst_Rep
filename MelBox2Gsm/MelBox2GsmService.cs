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
        public static string SimPin { get; set; } = ConfigurationManager.AppSettings["SimPin"];

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

            _ = Port.Ask("AT");         //Test, ob Verbindung besteht
            _ = Port.Ask("AT+CMGF=1");  //Textmodus  
            _ = Port.Ask("AT+CMEE=2");  //Fehlermeldungen im Klartext           
            _ = Port.Ask("AT+CSMP=49,167,0,0"); //Set SMS text Mode Parameters, 49, indicates the request for delivery report https://stackoverflow.com/questions/41676661/get-delivery-status-after-sending-sms-via-gsm-modem-using-at-commands
            _ = Port.Ask("AT+CNMI=2,1,2,2,1"); //New SMS message indication S.367ff.
            _ = Port.Ask("AT+GMM");     //Modem Hersteller Type

#if DEBUG
            //Log.Info("Setze PIN " + SimPin);
#endif
            GetSimPinStatus(SimPin);
            GetProviderName();
            GetOwnNumber();

            //SetCallRedirection(CallRedirection);

            GetSignalQuality();
            GetNetworkRegistration();
            GetSms();


            #endregion

            // Set up a timer that triggers every minute.
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 30000 // 60 seconds
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

        }

        protected override void OnStop()
        {
            Log.Info($"{this.ServiceName} wurde beendet.");
        }

        internal void TestStartupAndStop()
        {
            this.OnStart(new string[0]);
            Console.WriteLine($"{this.ServiceName} wurde als Konsolenanwendung gestartet. Beliebige Taste zum beenden..");
            Console.ReadLine();

            //#region Manuelle AT-Befehle
            //try
            //{
            //    Console.WriteLine(
            //        "Hinweis: AT-Befehle eingeben und mit Eingabetaste abschicken.\r\n" +
            //        "exit = beenden\r\n" +
            //        "sim = SMS-Empfang simulieren\r\n" +
            //        "trace = Rohdaten GSM-Kommunikation ein/ausblenden");

            //    while (true)
            //    {
            //        string request = Console.ReadLine();

            //        if (request.ToLower() == "sim")
            //            SimulateSmsRecieved();

            //        else if (request.ToLower() == "exit") break;
            //        else
            //            _ = Program.Port.Ask(request);
            //    }
            //}
            //finally { Port.Dispose(); }
            //#endregion
            this.OnStop();
        }



        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            //_eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);

            GetSignalQuality();
            GetSms();
        
        }
    }
}
