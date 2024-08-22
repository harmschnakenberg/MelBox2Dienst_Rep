using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static MelBox2Gsm.Program;

namespace MelBox2Gsm
{

    public class ReliableSerialPort : SerialPort
    {

        #region Connection
        public ReliableSerialPort(string portName = "", int baudRate = 38400, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            #region COM-Port verifizieren
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();

            if (ports?.Length == 0)
            {
                Program.Log.Error("Es wurden keine COM-Ports erkannt.");
                Pipe3.SendGsmStatus(Pipe3.Verb.Error, "Es wurden keine COM-Ports (GSM-Modem) erkannt.");
                base.Close();
                return;
            }

            if (!Array.Exists(ports, x => x == portName))
            {
                int pos = ports.Length - 1;
                portName = ports[pos];
            }
            #endregion

            PortName = portName;
            BaudRate = baudRate;
            DataBits = dataBits;
            Parity = parity;
            StopBits = stopBits;
            Handshake = Handshake.None;
            DtrEnable = true;
            NewLine = Environment.NewLine;
            ReceivedBytesThreshold = 1024;
            WriteTimeout = 300;
            ReadTimeout = 500;
            //RtsEnable = true; //TEST
            DtrEnable = true; //TEST
            ErrorReceived += SerialPortErrorEvent;
        }

        private void SerialPortErrorEvent(object sender, SerialErrorReceivedEventArgs e)
        {
            Pipe3.GsmErrorOccuredAsync(e.EventType.ToString());
        }

        new public void Open()
        {
            int Try = 10;

            do
            {
                try
                {
                    base.Open();
                    ContinuousRead();
                }
                catch
                {

                    Program.Log.Warn($"{base.PortName} ist {(base.IsOpen ? "offen" : "geschlossen")}. Verbleibende Verbindungsversuche: " + Try);
                    System.Threading.Thread.Sleep(4000);
                }
            } while (!base.IsOpen && --Try > 0);

            if (!base.IsOpen)
            {
                string errorText = $"Der COM-Port {base.PortName} ist nicht bereit. Das Programm {System.Reflection.Assembly.GetExecutingAssembly().GetName().Name} wird beendet.";
                Console.WriteLine(errorText);
                base.Close();
                Log.Error(errorText);
                Pipe3.SendGsmStatus(Pipe3.Verb.Error, errorText);
            }
        }
        
        #endregion

        #region Read



        private void ContinuousRead()
        {
            ThreadPool.QueueUserWorkItem(async delegate (object unused)
            {
                byte[] buffer = new byte[4096];

                while (true)
                {
                    try
                    {
                        if (!base.IsOpen) return;
                        int count = await BaseStream.ReadAsync(buffer, 0, buffer.Length);
                        byte[] dst = new byte[count];
                        Buffer.BlockCopy(buffer, 0, dst, 0, count);
                        OnDataReceived(dst);

                        BaseStream.Flush();
                    }
                    catch (System.IO.IOException ex_io)
                    {
                        Log.Error("ContinuousRead(): Lesefehler an COM-Port:\r\n" +
                        ex_io.Message);

                        base.Close();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"ContinuousRead() Lesefehler an COM-Port: {ex.Message}");
                    }
                }
            });
        }

        public const string Terminator = "\r\nOK\r\n";
        public const string ErrorIndicator = "ERROR";

        static string recLine = string.Empty;

        /// <summary>
        /// Sammelt die vom Modem empfangenen Daten für die Weiterleitung
        /// </summary>
        /// <param name="data"></param>
        public virtual void OnDataReceived(byte[] data)
        {
            recLine += System.Text.Encoding.UTF8.GetString(data);

            if (recLine.Contains(ErrorIndicator.ToUpper()))
            {
                ParseErrorResponse(recLine);
                _wait.Set();
            }

            //Melde empfangne Daten, wenn...
            if (recLine.Contains(Terminator))
                _wait.Set();

            //Prüfe auf unerwartete Meldungen vom Modem
            if (CeckUnsolicatedIndicators(recLine))
                _wait.Set();
        }

        private static int ErrorCount;

        private static void ParseErrorResponse(string answer)
        {
            Log.Error("Fehler von GSM-Modem:" + answer + "\r\n");

            Match m = Regex.Match(answer, @"\+CM[ES] ERROR: (.+)");
            if (m.Success)
            {
                var currentError = m.Groups[1].Value.TrimEnd('\r');

                if (currentError != LastError) //Gleichen Fehler nur einmal melden
                {
                    LastError = currentError;
                    Log.Error("Fehler von GSM-Modem:" + LastError);
                    Pipe3.SendGsmStatus(nameof(LastError), LastError);
                }
                else //wenn der Fehler zu oft gemeldet wird, doch nochmal melden
                {
                    ErrorCount++;
                    if (ErrorCount > 2)
                    {
                        LastError = string.Empty;
                        ErrorCount = 0;
                    }
                }
            }
        }


        #endregion

        #region Write

        private readonly AutoResetEvent _wait = new AutoResetEvent(false);


        public string Ask(string request, int timeout = 3000, bool log = true)
        {
            try
            {
                if (!base.IsOpen) Open(); 
                if (!base.IsOpen) return recLine;

                base.WriteLine(request);
#if DEBUG
                if (log)
                    Program.Log.Sent(request);
#endif
                _wait.Reset();

                if (!_wait.WaitOne(timeout))
                {
#if DEBUG
                    //Log.Warn("Timeout");
#endif
                }

#if DEBUG
                if (log)
                    Program.Log.Recieved(recLine);
#endif
                string x = recLine;
                recLine = string.Empty;
                return x;
            }
            catch (System.IO.IOException)
            {
                Program.Log.Error("Das GSM-Modem ist nicht vorhanden.");
            }

            return recLine;
        }

        #endregion
    }



}


