﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MelBox2Gsm
{
    internal static partial class Program
    {
        const string ctrlz = "\u001a";

        #region GsmModemStatus
        public static string LastError { get; set; }
        public static string Pin { get; set; }
        public static string PinStatus { get; set; }
        public static string ProviderName { get; set; }
        public static string OwnNumber { get; set; }
        public static string OwnName { get; set; }
        public static string CallRelayPhone { get; set; }
        public static int NetworkRegistration { get; set; }
        public static int RingSecondsBeforeCallForwarding { get; private set; } = 5;
        public static int SignalQuality { get; set; }

        
        #endregion

        #region SMS
        internal static void SimulateSmsRecieved(string phone = "+4916095285304", string message = "Simulierte SMS")
        {
            /*  S.363f.           
                AT+CMGW
                > Text im Speicher des GSM-Modems.
                +CMGW: 1

                OK
                AT+CMGR=1
                +CMGR: "STO UNSENT","",
                Text im Speicher des GSM-Modems.

                OK
                AT+CMGW="+49123456789"
                > Zweiter Test im Speicher.
                +CMGW: 2

                OK
                AT+CMGR=2
                +CMGR: "STO UNSENT","+49123456789",
                Zweiter Test im Speicher.

                OK

            AT+CMGW=<oa> / <da> [, [ <tooa> / <toda> ][,  <stat> ]] <CR>  Text can be entered.  <CTRL-Z> / <ESC>
            */

            _ = Port.Ask($"AT+CMGW=\"{phone}\",145,\"REC UNREAD\"\r");
            Thread.Sleep(200); //Test
            _ = Port.Ask(message + ctrlz, 10000);

            Log.Info("Simuliere SMS-Empfang..");

            GetSms();

           // Pipe3.RecievedSms(new Sms(new Random().Next(0, 255), DateTime.UtcNow, "+4916095285304", "Simulierte SMS"));
        }

        /// <summary>
        /// Ruft dem SMS-Speicher aus dem GSM-Modem ab.
        /// </summary>
        /// <param name="filter">ALL, REC READ, REAC UNREAD</param>
        public static void GetSms(string filter = "ALL")
        {
            string answer = Port.Ask($"AT+CMGL=\"{filter}\"");

            //Log.Info($"SMS-Abfrage empfangen: \r\n{answer}");
            #region Neue SMSen

            //Nachricht:
            //+CMGL: <index> ,  <stat> ,  <oa> / <da> , [ <alpha> ], [ <scts> ][,  <tooa> / <toda> ,  <length> ]
            //<data>
            //[... ]
            //OK

            //+CMGL: 9,"REC READ","+4917681371522",,"20/11/08,13:47:10+04"
            //Ein Test 08.11.2020 13:46 PS sms38.de

            //\+CMGL: (\d+),"(.+)","(.+)",(.*),"(.+),(.+)([\+-].{2})"\n(.+\n)+
            //  MatchCollection mc = Regex.Matches(answer, "\\+CMGL: (\\d+),\"(.+)\",\"(.+)\",(.*),\"(.+),(.+)([\\+-].{2})\"\\n(.+)");
            
            MatchCollection mc = Regex.Matches(answer, @"\+CMGL: (\d+),""(.+)"",""(.+)"",(.*),""(.+),(.+)([\+-].{2})""\r\n(.+)");

            foreach (Match m in mc)
            {
                try
                {
                    int.TryParse(m.Groups[1].Value, out int index);
                    string status = m.Groups[2].Value.Trim('"');

                    string phone = m.Groups[3].Value.Trim('"');
                    string phonebookentry = m.Groups[4].Value.Trim('"');

                    string dateStr = "20" + m.Groups[5].Value.Trim('"').Replace('/', '-');
                    string timeStr = m.Groups[6].Value;
                    _ = int.TryParse(m.Groups[7].Value.Trim('"'), out int timeZone);
                    timeZone /= 4;

                    string message = m.Groups[8].Value.TrimEnd('\r'); //.DecodeUcs2();

                    _ = DateTime.TryParse($"{dateStr} {timeStr}", out DateTime time);

                    Log.Info($"Neue SMS {index} von {phone} erhalten {time}({time.ToShortDateString()} {time.ToShortTimeString()})\t{message}");

                    //Nach status filtern?
                    Pipe3.RecievedSms(new Sms(index, time, phone, message));
                    //                    recievedSms.Add($"{index},{time},{phone},{message}");
                }
                catch (Exception ex)
                {
                    Log.Error("Fehler beim Abrufen von SMS aus GSM-Modem." + ex);
                }
            }

           
            #endregion

            GetStatusReports(answer);
        }

        /// <summary>
        /// Liest Statusreports (Empfangsbestätigungen) aus der Modemantwort
        /// </summary>
        /// <param name="answer">Modemantwort</param>
        private static void GetStatusReports(string answer)
        {
            #region StatusReport

            //Statusreport:
            //+CMGL: < index > ,  < stat > ,  < fo > ,  < mr > , [ < ra > ], [ < tora > ],  < scts > ,  < dt > ,  < st >
            //[... ]
            //OK
            //z.B.: +CMGL: 1,"REC READ",6,34,,,"20/11/06,16:08:45+04","20/11/06,16:08:50+04",0

            //\+CMGL: (\d+),"(.+)",(\d+),(\d+),,,"(.+),(.+)([\+-]\d+)","(.+),(.+)([\+-]\d+)",(\d+)
            //getestet: @"\+CMGL: (\d+),(.+),(\d+),(\d+),,,(.+),(.+)([\+-].+),(.+),(.+)([\+-].+),(\d)"
            MatchCollection reports = Regex.Matches(answer, @"\+CMGL: (\d+),(.+),(\d+),(\d+),,,(.+),(.+)([\+-].+),(.+),(.+)([\+-].+),(\d)");

            if (reports.Count == 0) return;

            List<StatusReport> statusReports = new List<StatusReport>();

            foreach (Match m in reports)
            {
                try
                {
                    int index = int.Parse(m.Groups[1].Value);
                    string status = m.Groups[2].Value.Trim('"');
                    int firstOctet = int.Parse(m.Groups[3].Value); // Bedeutung?
                    int reference = int.Parse(m.Groups[4].Value);
                    string dateCenter = "20" + m.Groups[5].Value.Trim('"').Replace('/', '-');
                    string timeCenter = m.Groups[6].Value;
                    int zoneCenter = int.Parse(m.Groups[7].Value.Trim('"')) / 4;
                    string dateDischarge = "20" + m.Groups[8].Value.Trim('"').Replace('/', '-');
                    string timeDischarge = m.Groups[9].Value;
                    int zoneDischarge = int.Parse(m.Groups[10].Value.Trim('"')) / 4;
                    int delviveryStatus = int.Parse(m.Groups[11].Value);

                    _ = DateTime.TryParse(dateCenter + " " + timeCenter, out DateTime SmsCenterTime);
                    _ = DateTime.TryParse(dateDischarge + " " + timeDischarge, out DateTime DischargeTime);

                    DateTime ServiceCenterTimeUtc = SmsCenterTime.AddHours(-zoneCenter);
                    DateTime DischargeTimeUtc = DischargeTime.AddHours(-zoneDischarge);

                    //Delivery-Status <st> - Quelle: https://en.wikipedia.org/wiki/GSM_03.40#Discharge_Time
                    //0-31:     delivered or, more generally, other transaction completed.
                    //32-63:    still trying to deliver the message.
                    //64-127:   not making any more delivery attempts.

                    string msg = $"StatusReport erhalten\r\n" +
                        $"\tSpeicherplatz {index}, Status '{status}', \r\n" +
                        $"\tReferenz {reference}: StatusCode [{delviveryStatus}]: " + (delviveryStatus > 63 ? "Senden fehlgeschlagen" : delviveryStatus > 31 ? "senden im Gange" : "erfolgreich versendet") + "\r\n" +
                        $"\tServicecenterZeit {SmsCenterTime} (UTZ {ServiceCenterTimeUtc})\r\n" +
                        $"\tSendezeit {DischargeTime} (UTC {DischargeTimeUtc})";

                    Log.Info(msg);

                    Pipe3.ReportRecieved(new StatusReport(index, reference, DischargeTimeUtc, delviveryStatus));
                }
                catch (Exception)
                {
#if DEBUG
                    throw;
#else
                    Log.Error("Fehler beim Abrufen von Statusreports aus GSM-Modem.");
#endif
                }                
            }

            //TrackSentSms(newReports);

            #endregion
        }

        /// <summary>
        /// SMS versenden
        /// siehe Quelle: https://www.smssolutions.net/tutorials/gsm/sendsmsat/
        /// </summary>
        /// <param name="phone">Telefonnummer des Empfängers in der Form '+49...'</param>
        /// <param name="message">Inhalt der Nachricht</param>
        /// <returns>Referneznummer der versendeten SMS zur Sendungsverfolgung, -1 bei Fehler</returns>
        public static Sms SendSms(string phone, string message)
        {
            
            _ = Port.Ask($"AT+CMGS=\"{phone}\"\r");
            Thread.Sleep(1000); //Test

            string answer = Port.Ask(message + ctrlz, 10000);

            Match m = Regex.Match(answer, @"\+CMGS: (\d+)");

            int reference;

            if (!m.Success)
                reference = 888;
            else if (!int.TryParse(m.Groups[1].Value, out reference))
                reference = 999;

            Log.Error($"SendeSms() Antwort: '{answer}' \r\nReferenz={m.Groups[1].Value}");

            return new Sms(reference, DateTimeOffset.UtcNow, phone, message);
        }


        public static void DeleteSms(int smsId)
        {
            _ = Port.Ask($"AT+CMGD={smsId}");
        }

        #endregion


        #region GSM-Modem Status / Setup

        /// <summary>
        /// Prüft, ob die SIM-Karte eine PIN-Eingabe erfordert und gibt ggf. den PIN ein.
        /// Achtung! 3-malige Fehleingabe führt zur PUK-Abfrage, die nicht im Programm berücksichtigt ist! siehe MC75 S.114ff
        /// </summary>
        /// <param name="simPin">PIN für die SIM-Karte. Beliebig, wenn keine PIN-Abfrage an SIM</param>
        /// <param name="unlockPin">true = deaktiviert PIN-Abfrage</param>
        public static void GetSimPinStatus(string simPin, bool unlockPin = true)
        {
            string answer = Port.Ask("AT+CPIN?");
            MatchCollection mc = Regex.Matches(answer, @"\+CPIN: (.+)\r");

            if (mc.Count > 0)
            {
                PinStatus = mc[0].Groups[1].Value;

                switch (mc[0].Groups[1].Value)
                {
                    case "READY":
                        Log.Info("PIN ist freigeschaltet.");
                        break;
                    case "SIM PIN":
                        answer = Port.Ask(@"AT^SPIC"); //Anzahl der freien versuche ermitteln
                        MatchCollection mc2 = Regex.Matches(answer, @"\^SPIC: (\d+)"); //^SPIC: 3

                        if (mc2.Count > 0 && int.TryParse(mc2[0].Groups[1].Value, out int freeAttempts) && freeAttempts > 2)
                        {
                            Log.Warn($"PIN-Eingabe erfolgt (noch {freeAttempts} Versuche frei).");

                            if (unlockPin) //PIN-Abfrage deaktivieren
                                _ = Port.Ask($"AT+CLCK=\"SC\",0,\"{simPin}\"");
                            else
                                _ = Port.Ask("AT+CPIN=" + simPin);

                            Thread.Sleep(2000);
                            GetSimPinStatus(simPin); //Anzah rekursiver Aufrufe begrenzen?
                        }
                        break;
                    case "SIM PUK":
                        Log.Error("PUK-Eingabe erforderlich! siehe AT+CPIN=");
                        Pipe3.GsmErrorOccuredAsync("PUK - Eingabe erforderlich!");
                        break;
                    default:
                        Log.Error("PIN-Status:" + mc[0].Groups[1].Value);
                        Pipe3.GsmErrorOccuredAsync("PIN-Eingabe fehlgeschlagen. PIN-Status:" + mc[0].Groups[1].Value);
                        break;
                }
            }
        }


        /// <summary>
        /// Liest den aktuellen Mobilfunknetzbetreiber aus.
        /// </summary>
        internal static void GetProviderName()
        {
            string answer = Port.Ask("AT+COPS?");
            MatchCollection mc = Regex.Matches(answer, @"\+COPS: (\d),(\d),(.+)\r");

            if (mc.Count > 0)
            {
                string providerName = mc[0].Groups[3].Value.Trim('"');
                Log.Info("Mobilfunkanbieter: " + providerName);
                ProviderName = providerName;                
            }
        }

        /// <summary>
        /// Liest die Eigene Nummer und den eigenen Telefonbucheintrag für die SIM-Karte im GSM-Modem aus.
        /// </summary>
        internal static void GetOwnNumber()
        {
            string answer = Port.Ask("AT+CNUM");
            //+CNUM: [<alpha>], <number> ,<type> ]
            MatchCollection mc = Regex.Matches(answer, @"\+CNUM: ""(.+)"",""(.+)"",(?:\d+)");

            if (mc.Count > 0)
            {
                OwnName = mc[0].Groups[1].Value;
                string ownNumber = mc[0].Groups[2].Value;               
                OwnNumber = ownNumber;
                Log.Info("Eigene Mobilfunknummer: " + ownNumber);
            }
        }

        /// <summary>
        /// Aktiviert die Rufumleitung auf die Angegeben Telefonnummer. siehe MC75 S.221.
        /// </summary>
        /// <param name="phone">Nummer zu der Sprachanrufe umgeleitet werden sollen. Die Nummer wird nicht auf Plausibilität geprüft.</param>
        public static CallRelay SetCallRedirection(string phone)
        {
            CallRelay relay = new CallRelay(phone, "unbekannt");

            if (NetworkRegistration != 1)
            {
                relay.Status = "Kein Netz. Sprachanrufe werden nicht weitergeleitet";
                Log.Warn(relay.Status);
                return relay;
            }

            MatchCollection mc = Regex.Matches(phone, @"\+(\d{10,})"); //Format beginnt mit '+...' und mind. 10 Ziffern lang

            if (mc.Count == 0)
            {
                relay.Status = $"Rufumleitung an '{phone}' ist ungültig.";
                Log.Warn(relay.Status);
                return relay;
            }

            //Setze Rufumleitung
            _ = Port.Ask($"AT+CCFC=0,3,\"{phone}\",145,1,{RingSecondsBeforeCallForwarding}");
            //AT+CCFC=<reason>,<mode>[,<number>[,<type>[,<class>[,<time>]]]]

            //Prüfe Rufumleitung
            string answer = Port.Ask("AT+CCFC=2,2", 6000);
            //+CCFC: <status> ,  <class> [,  <number> ,  <type> ]
            MatchCollection mc2 = Regex.Matches(answer, @"\+CCFC: (\d),(\d),""(.+)"",(?:\d+),(\d+)");

            if (int.TryParse(mc2[0].Groups[1].Value, out int status) && status == 1)
            {
                relay.Phone = mc2[0].Groups[3].Value.Trim('"');
                relay.Status = $"Sprachanrufe werden nach {RingSecondsBeforeCallForwarding} Sek. an {relay.Phone} weitergeleitet.";
                Log.Info(relay.Status);
                return relay;
            }
            else
            {
                relay.Status = $"Weiterleitung von Sprachanrufen an {phone} NICHT aktiv.";
                Log.Warn(relay.Status);
                return relay;
            }
        }


        /// <summary>
        /// Fragt die Mobilnetzempfangsqualität ab. 
        /// </summary>
        internal static void GetSignalQuality()
        {
            string answer = Port.Ask("AT+CSQ");
            MatchCollection mc = Regex.Matches(answer, @"\+CSQ: (\d+),(\d+)");

            if (mc.Count > 0 && int.TryParse(mc[0].Groups[1].Value, out int quality))
            {
                quality = (int)(quality / 0.31); //in %, siehe Kapitel 8.5 'AT+CSQ' Seite 193 

                if (quality > 100) quality = 0;

                if (SignalQuality != quality) //Wenn sich die Signalqualität geändert hat
                {
                    SignalQuality = quality;
                    Pipe3.SendGsmStatus(nameof(SignalQuality),SignalQuality.ToString());
                  
                    if (quality < 25)
                    { Log.Warn($"Mobilfunksignal {quality}%"); }
                    else
                    { Log.Info($"Mobilfunksignal {quality}%"); }
                }
            }
        }


        

        /// <summary>
        /// Fragt ab, ob das Modem im Mobilfunnetz registriert ist.
        /// </summary>
        internal static void GetNetworkRegistration()
        {
            string answer = Port.Ask("AT+CREG?");
            MatchCollection mc = Regex.Matches(answer, @"\+CREG: (\d),(\d)");

            if (mc.Count == 0) return;

            if (int.TryParse(mc[0].Groups[2].Value, out int regStatus))
            {
                if (NetworkRegistration != regStatus) //Wenn sich die Signalqualität geändert hat
                {
                    if (int.TryParse(mc[0].Groups[1].Value, out int mode))
                        Log.Info($"Die Verbindung zum Provider wird {((mode > 0) ? "" : "nicht")} überwacht.");

                    string regStatusStr;
                    switch (regStatus)
                    {
                        //MC75 S.191
                        case 0:
                            regStatusStr = "Mobilfunknetz: nicht registriert.";                            
                            break;
                        case 1:
                            regStatusStr = "Mobilfunknetz: registriert.";                          
                            break;
                        case 2:
                            regStatusStr = "Mobilfunknetz: suche Provider.";                         
                            break;
                        case 3:
                            regStatusStr = "Mobilfunknetz: Anmeldung verweigert.";                         
                            break;
                        case 5:
                            regStatusStr = "Mobilfunknetz: Roaming aktiv.";                          
                            break;
                        default:
                            regStatusStr = "Mobilfunknetz: Status unbekannt.";                        
                            break;
                    }

                    if (regStatus == 1)
                        Log.Info(regStatusStr);
                    else
                        Log.Warn(regStatusStr);

                    Pipe3.SendGsmStatus(nameof(NetworkRegistration), regStatusStr);
                    NetworkRegistration = regStatus;
                }
            }
        }

        #endregion

    }

 

    #region Datencontainer-Klassen
    /// <summary>
    /// SMS
    /// </summary>
    public class Sms
    {
        public Sms(int index, DateTimeOffset time, string phone, string content)
        {
            Index = index;
            Time = time;
            Phone = phone;
            Content = content;
        }

        public int Index { get; set; }

        public DateTimeOffset Time { get; set; }

        public string Phone { get; set; }

        public string Content { get; set; }
    }


    /// <summary>
    /// Empfangsbestätigung / Sendebestätigung einer SMS 
    /// </summary>
    public class StatusReport
    {
        public StatusReport(int index, int reference, DateTimeOffset dischargeTimeUtc, int deliveryStatus)
        {
            Index = index;
            Reference = reference;
            DischargeTimeUtc = dischargeTimeUtc;
            DeliveryStatus = deliveryStatus;
        }

        public int Index { get; set; }

        public int Reference { get; set; }

        public DateTimeOffset DischargeTimeUtc { get; set; }

        public int DeliveryStatus { get; set; }

        public string DeliveryStatusText
        {
            get
            {
                //siehe Spezifikation GSM 3.40
                if (GetBit(DeliveryStatus, 6))
                    return "Fehler. Sendeversuch abgebrochen.";

                if (GetBit(DeliveryStatus, 5))
                    return "Fehler. Weiteren Sendeversuch abwarten.";

                if (GetBit(DeliveryStatus, 1))
                    return "Die Nachricht wurde vom Provider ersetzt.";

                if (GetBit(DeliveryStatus, 0))
                    return "Nachricht zugestellt. Keine Empfangsbestätigung.";

                if (DeliveryStatus == 0)
                    return "Empfangsbestätigung vom Empfänger erhalten.";

                return $"Sendunsgstatus = {DeliveryStatus}";
            }
        }

        private bool GetBit(int pByte, int bitNo)
        {
            return (pByte & (1 << bitNo)) != 0;
        }

    }

    public class CallRelay
    {
        public CallRelay(string phone, string status)
        {
            Phone = phone;
            Status = status;
        }
        public string Phone { get; set; }
        public string Status { get; set; }
    }

    #endregion
}
