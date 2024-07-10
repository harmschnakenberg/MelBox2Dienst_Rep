using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MelBox2Dienst
{
    public partial class MelBox2Service : ServiceBase
    {
        public MelBox2Service()
        {
            InitializeComponent();

            this.ServiceName = "MelBox2Dienst";
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            Pipe1.StartPipeServer(Pipe1.PipeName.MelBox2Service, true);
            Server.Start();
            Sql.CheckDbFile();
            //CheckCallRelayNumber(); //BÖSE! Macht nach wenigen Sekunden Stack.Overfolow-Fehler!!

            #region Timer
            // Set up a timer that triggers every x minute.
            Timer timer = new Timer
            {
                Interval = 60000 // 300000 // 5 Minuten 
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimerAsync);
            timer.Start();
            #endregion

            Log.Info($"{ServiceName} V{Assembly.GetExecutingAssembly().GetName().Version} gestartet.");
        }

        protected override void OnStop()
        {
            Log.Info(ServiceName + " beendet.");
            Server.Stop();
        }

        internal void TestStartupAndStop()
        {
            this.OnStart(new string[0]);
            Console.WriteLine($"{this.ServiceName} wurde als Konsolenanwendung gestartet. Beliebige Taste zum beenden..");
            Console.ReadLine();            
            this.OnStop();
        }

        public void OnTimerAsync(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            Log.Info("Monitoring the System " + DateTime.Now);

            #region Prüfe Sprachweiterleitung: Sprachanrufe werden an den ersten Datensatz der aktuellen Bereitschaft mit gültiger Telefonnummer weitergeleitet
            CheckCallRelayNumber();
            #endregion
        }

        /// <summary>
        /// Geht die Liste der aktuellen Bereitschaft durch (i.d.R. 1 Person), 
        /// findet den ersten Eintrag mit einer gültigen Telefonnummer 
        /// und veranlasst, dass Sprachanrufe an diese Nummer weitergeleitet werden.
        /// </summary>
        public static async void CheckCallRelayNumber()
        {
            List<Service> currentService = Sql.SelectCurrentGuards();
            foreach (Service service in currentService)
            {
                if (!Pipe1.IsPhoneNumber(service.Phone))
                    continue;

                if (service.Phone != Sql.CallRelayPhone)
                    Sql.CallRelayPhone = await Pipe1.RelayCall(service.Phone);

                break;
            }
        }

    }
}
