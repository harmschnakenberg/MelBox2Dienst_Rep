using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2Gsm
{
    internal static partial class Program
    {
        internal static readonly ReliableSerialPort Port = new ReliableSerialPort();
      
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static void Main()
        {
            Pipe3.StartPipeServer(Pipe3.PipeName.Gsm, true);

            System.ServiceProcess.ServiceBase[] ServicesToRun;

            if (Environment.UserInteractive)
            {
                MelBox2GsmService service1 = new MelBox2GsmService();
                service1.TestStartupAndStop();
                Log.Info($"{nameof(ServicesToRun)}={AppContext.BaseDirectory}");
            }
            else //Start als Dienst
            {
                ServicesToRun = new ServiceBase[]
                {
                    new MelBox2GsmService()
                };
                ServiceBase.Run(ServicesToRun);
            }

#if DEBUG
            Console.WriteLine("ENDE");
            Console.ReadLine();
#endif
        }


        internal static class Log
        {
            internal static void Info(string text)
            {
                if (Environment.UserInteractive)
                    Console.WriteLine("Log > " + text);
                //else
                //{
                // Sql.InsertLog(3, text);
                //}
            }

            internal static void Warn(string text)
            {

                if (Environment.UserInteractive)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine("Log > " + text);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                //else
                //{
                // Sql.InsertLog(1, text);
                //}
            }

            internal static void Error(string text)
            {

                if (Environment.UserInteractive)
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Log > " + text);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                //else
                //{
                // Sql.InsertLog(1, text);
                //}
            }


            internal static void Sent(string text)
            {

                if (Environment.UserInteractive)
                {
                    Console.BackgroundColor = ConsoleColor.DarkMagenta;
                    //Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Log > " + text);
                    //Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                //else
                //{
                // Sql.InsertLog(1, text);
                //}
            }

            internal static void Recieved(string text)
            {

                if (Environment.UserInteractive)
                {
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    //Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Log > " + text);
                    //Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                //else
                //{
                // Sql.InsertLog(1, text);
                //}
            }

        }
    }
}
