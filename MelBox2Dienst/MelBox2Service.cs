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
           
           // Pipes.StartPipeServer("myPipe");
            Pipes.StartPipeServer2(Pipes.Name.Sms);
            Server.Start();
            Sql.CheckDbFile();

            #region Timer
            // Set up a timer that triggers every minute.
            Timer timer = new Timer
            {
                Interval = 60000 // 60 seconds
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
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

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
           Log.Info("Monitoring the System " + DateTime.Now);
        }

       
    }
}
