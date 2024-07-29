using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2Email
{
    internal static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;

            if (Environment.UserInteractive)
            {

                MelBox2EmailService service1 = new MelBox2EmailService();
                service1.TestStartupAndStop();
            }
            else //Start als Dienst
            {
                ServicesToRun = new ServiceBase[]
                {
                    new MelBox2EmailService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
