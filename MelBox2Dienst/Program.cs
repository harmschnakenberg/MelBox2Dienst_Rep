using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    internal static class Program
    {
        public static bool IsRunningInConsole = Environment.UserInteractive;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main()
        {

            System.ServiceProcess.ServiceBase[] ServicesToRun;

            if (Environment.UserInteractive)
            {               
                MelBox2Service service1 = new MelBox2Service();
                service1.TestStartupAndStop();
                Log.Info($"{nameof(ServicesToRun)}={AppContext.BaseDirectory}");
            }
            else //Start als Dienst
            {
                ServicesToRun = new ServiceBase[]
                {
                    new MelBox2Service()
                };
                ServiceBase.Run(ServicesToRun);
            }
#if DEBUG
            Console.WriteLine("ENDE");
            Console.ReadLine();
#endif
        }
       
    }

    internal static class Log
    {
        internal static void Info (string text)
        {
            if (Program.IsRunningInConsole)
                Console.WriteLine("Log > " + text);
            else
            {
                Sql.InsertLog(3, text);
            }
        }

        internal static void Error(string text)
        {

            if (Program.IsRunningInConsole)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Log {DateTime.Now.ToShortTimeString()} > {text}");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Sql.InsertLog(1, text);
            }
        }
    }

}
