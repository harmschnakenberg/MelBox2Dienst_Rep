using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using System.Xml.Linq;

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
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 60000 // 300000 // 5 Minuten 
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimerAsync);
            timer.Start();

            System.Timers.Timer timer2 = new System.Timers.Timer
            {
                Interval = 3600000 //1 Stunde
            };
            timer2.Elapsed += new ElapsedEventHandler(this.OnTimer2Async);
            timer2.Start();

            #endregion

            Log.Info($"{ServiceName} V{Assembly.GetExecutingAssembly().GetName().Version} gestartet.");

            Thread.Sleep(10000);//Warte, weil sonst eine StackOverflow passierne kann.

            //initiale Prüfungen
            CheckCallRelayNumber();
            CheckOverdueSenders();
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
#if DEBUG
            Log.Info("Monitoring the System " + DateTime.Now);
#endif
            #region Prüfe Sprachweiterleitung: Sprachanrufe werden an den ersten Datensatz der aktuellen Bereitschaft mit gültiger Telefonnummer weitergeleitet
            CheckCallRelayNumber();
            #endregion
        }

        private void OnTimer2Async(object sender, ElapsedEventArgs e)
        {
            #region Prüfe auf Inaktivität: Melde, wenn von Absendern zu lange keien NAchrichten eingegangen sind           
            CheckOverdueSenders();
            SendDailyNotification();
            #endregion
        }


        #region Timer-Aktionen

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

                if (service.Phone != Sql.CallRelayPhone) //Nur anfragen, wenn sich die gewünschte Telefonnummer geändert hat
                {
                    CallRelay relay;
                    relay = await Pipe1.RelayCall(service.Phone);
                    Sql.CallRelayPhone = relay.Phone;
                    Log.Info($"Relay phone: {relay.Phone}: {relay.Status}");
                }
                break;
            }
        }

        /// <summary>
        /// Prüft auf inaktive Absender
        /// </summary>
        public static void CheckOverdueSenders()
        {
            if (Sql.IsBuisinesTimeNow()) return; //Inaktivität nur zur Geschäftszeit prüfen.

            DataTable dt = Sql.SelectOverdueCustomers();

            if (dt.Rows.Count > 0)
            {
                List<Service> permanentGuards = Sql.SelectPermamanentGuards();

                #region Email versenden
                for (int i = 0; i < dt.Rows.Count; i++)
                {                    
                    string name = dt.Rows[i]["Name"].ToString();
                    string tel = dt.Rows[i]["Telefon"].ToString();
                    string email = dt.Rows[i]["E-Mail"].ToString();
                    string due = dt.Rows[i]["Fällig seit"].ToString();

                    string contact =  (tel.Length > 1 ? tel : string.Empty) + (email.Length > 1 ? " " + email : string.Empty);                    
                    string subject = $"Inaktivität >{name}<";
                    string body = $" Inaktivität >{name}< {contact}. Meldung fällig seit >{due}<. \r\nMelsys bzw. Segno vor Ort prüfen.\r\n\r\n";

                    Email emailOut = new Email(Sql.From, permanentGuards.Select(x => x.Email).Where(y => y.Contains("@")).ToList(), null, subject, body);
                    Pipe1.SendEmail(emailOut);
                    Thread.Sleep(5000);
                }
                #endregion
            }
        }

        /// <summary>
        /// Sendet eine Routinemeldung an Admin-Phone zur Stunde 'hour'
        /// </summary>
        /// <param name="hour">Tagesstunde zu der die Routinemeldung abgesetzt wird.</param>
        private static void SendDailyNotification(int hour = 8)
        {
            if (DateTime.Now.Hour != hour)
                return;

            string adminPhone = Properties.Settings.Default.AdminPhone;
            Sms sms = new Sms(0, DateTime.Now, adminPhone, "SMS-Zentrale Routinemeldung.");
            Pipe1.SendSmsAsync(sms);
        }
        
        #endregion
    }
}
