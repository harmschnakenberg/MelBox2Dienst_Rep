using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    internal static partial class Sql
    {
        public static string From { get; set; } = "SMSZentrale@Kreutztraeger.de"; 


        #region Empfang von Nachrichten
        /// <summary>
        /// Liste empfangene Nachrichten eines bestimmten Datums
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static DataTable SelectRecieved(DateTime date)
        {
            if (date == DateTime.MinValue) { date = DateTime.Now.Date; }

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Empfangen", date.ToString("yyyy-MM-dd") }
            };

            return Sql.SelectDataTable(
                @"SELECT Nr, 
                    Empfangen, 
                    Von, 
                    Inhalt, 
                    Sperregel
                    FROM View_Recieved 
                    WHERE DATE(Empfangen) = DATE(@Empfangen) 
                    ORDER BY Empfangen DESC", args);
        }

        /// <summary>
        /// Liste eine Anzahl der neuesten empfangenen Nachrichten
        /// </summary>
        /// <param name="limit">Anzahl anzuzeigender Nachrichten</param>
        /// <returns></returns>
        internal static DataTable SelectRecieved(uint limit)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Limit", limit }
            };

            return Sql.SelectDataTable(
                @"SELECT Nr, 
                    Empfangen, 
                    Von, 
                    Inhalt, 
                    Sperregel
                    FROM View_Recieved                     
                    ORDER BY Empfangen DESC
                    LIMIT @Limit", args);
        }

        /// <summary>
        /// Trägt eine neu Empfangene Nachricht in die Datenbank ein und gibt derne Id wieder.
        /// </summary>
        /// <param name="sms">Empfangene Nachricht</param>
        /// <returns>Id des Inhalts der Nachricht (Message.Id)</returns>
        internal static uint CreateRecievedMessage(Sms sms)
        {
            #region Absender protokollieren und identifizieren
            uint customerId = Sql.GetCustomerId(sms);
            uint messageId = Sql.SelectMessageIdByContent(sms.Content);
            #endregion

            #region Sms-Empfang protokollieren
            Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@Time", sms.Time.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")},
                    { "@SenderId", customerId},
                    { "@ContentId", messageId }
                };

            const string query = "INSERT INTO Recieved (Time, SenderId, ContentId) VALUES (@Time, @SenderId, @ContentId);";
                //"SELECT MAX(Id) FROM Recieved;";

            _ = Sql.NonQueryAsync(query, args);
            //_ = uint.TryParse(Sql.SelectValue(query, args)?.ToString(), out uint recivedId);
            #endregion

            return messageId;
        }

        internal static uint CreateRecievedMessage(Email email)
        {
            #region Absender protokollieren und identifizieren
            uint customerId = Sql.GetCustomerId(email);
            uint messageId = Sql.SelectMessageIdByContent(email.Body);
            #endregion

            #region Sms-Empfang protokollieren
            Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@Time", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")},
                    { "@SenderId", customerId},
                    { "@ContentId", messageId }
                };

            const string query = "INSERT INTO Recieved (Time, SenderId, ContentId) VALUES (@Time, @SenderId, @ContentId);";
            //"SELECT MAX(Id) FROM Recieved;";

            _ = Sql.NonQueryAsync(query, args);
            //_ = uint.TryParse(Sql.SelectValue(query, args)?.ToString(), out uint recivedId);
            #endregion

            return messageId;
        }


        internal static bool MessageRecieveAndRelay(Sms sms)
        {
            try
            {
                uint messageId = Sql.CreateRecievedMessage(sms);
                bool isBlocked = Sql.IsMessageBlocked(messageId);

                #region Finde heraus, wer Bereitschaft hat
                List<Service> permanentGuards = Sql.SelectPermamanentGuards();
                List<Service> currentGuards = Sql.SelectCurrentGuards();

                //if (!isBlocked) //wieder raus genommen
                //    permanentGuards.AddRange(currentGuards); //Wenn Bereitschaft eien E-Mail-Adresse angegeben hat, auch dahin schicken
                #endregion

                #region Email versenden
                string body = $"SMS Absender >{sms.Phone}<\r\n" +
                    $"SMS Text >{sms.Content}<\r\n" +
                    $"SMS Sendezeit >{sms.Time}<\r\n\r\n";

                if (isBlocked)
                    body += "Keine Weiterleitung an Bereitschaftshandy da SMS gesperrt.\r\n";

                Email email = new Email(From, permanentGuards.Select(x => x.Email).Where(y => y.Contains("@")).ToList(), body);
                Pipe1.SendEmail(email);

                #endregion

                #region SMS versenden
                //TODO
                if (!isBlocked)
                    //Pipes.SendSms(currentGuards.Select(x => x.Phone).ToList(), sms, messageId);
                    Pipe1.SendSms(currentGuards.Select(x => x.Phone).ToList(), sms, messageId);
                #endregion

                return messageId > 0;
            }
            catch (Exception ex) 
            {
                Log.Error($"Fehler MessageRecieveAndRelay() {ex}");
                return false; 
            }
        }

        internal static bool MessageRecieveAndRelay(Email emailIn)
        {
            try
            {
                uint messageId = Sql.CreateRecievedMessage(emailIn);
                bool isBlocked = Sql.IsMessageBlocked(messageId);

                #region Finde heraus, wer Bereitschaft hat
                List<Service> permanentGuards = Sql.SelectPermamanentGuards();
                List<Service> currentGuards = Sql.SelectCurrentGuards();

                //if (!isBlocked) //wieder raus genommen
                //    permanentGuards.AddRange(currentGuards); //Wenn Bereitschaft eien E-Mail-Adresse angegeben hat, auch dahin schicken
                #endregion

                #region Email versenden
                string body = $"Email Absender >{emailIn.From}<\r\n" +
                    $"SMS Text >{emailIn.Body}<\r\n\r\n";

                if (isBlocked)
                    body += "Keine Weiterleitung an Bereitschaftshandy da gesperrt.\r\n";

                Email emailOut = new Email(From, permanentGuards.Select(x => x.Email).Where(y => y.Contains("@")).ToList(), body);
                Pipe1.SendEmail(emailOut);

                #endregion

                #region SMS versenden
                Sms smsOut = new Sms(-1, DateTime.Now, "", RemoveUmlauts(emailIn.Body).Substring(0, Math.Min(emailIn.Body.Length, 160) ) );

                if (!isBlocked)
                    //Pipes.SendSms(currentGuards.Select(x => x.Phone).ToList(), sms, messageId);
                    Pipe1.SendSms(currentGuards.Select(x => x.Phone).ToList(), smsOut, messageId);
                #endregion

                return messageId > 0;
            }
            catch (Exception ex)
            {
                Log.Error($"Fehler MessageRecieveAndRelay() {ex}");
                return false;
            }
        }


        private static string RemoveUmlauts(string orig)
        {
            //Umlaute durch ASCII-Zeichen ersetzen
            var s = orig
                .Replace("Ä", "Ae")
                .Replace("Ü", "Ue")
                .Replace("Ö", "Oe")
                .Replace("ä", "ae")
                .Replace("ü", "ue")
                .Replace("ö", "oe")
                .Replace("ß", "ss")
                ;

            //Alle nicht erfassten, Nicht-ASCII-Zeichen entfernen
            return Regex.Replace(s, @"[^\u0000-\u007F]+", string.Empty);
        }

        /// <summary>
        /// Prüft, ob die empfangene Nachricht gerade gesperrt ist.
        /// </summary>
        /// <param name="messageId">Id der Nachricht (Inhalt) in Tabelle Message</param>
        /// <returns>true = Nachricht ist zur Zeit gesperrt</returns>
        internal static bool IsMessageBlocked(uint messageId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@MessageId", messageId }
            };

            short isMessageBlocked = Convert.ToInt16(
                Sql.SelectValue(
                //@"SELECT 1 WHERE (SELECT BlockPolicyId FROM Message 
                //WHERE Id = (SELECT ContentId FROM Recieved WHERE Id = 4)) IN (SELECT BlockPolicyId FROM View_BlockedNow WHERE BlockedNow > 0);"
                @"SELECT 1 WHERE (SELECT BlockPolicyId FROM Message WHERE Id = @MessageId) IN (SELECT BlockPolicyId FROM View_BlockedNow WHERE BlockedNow > 0);"
                , args));

            return isMessageBlocked > 0;
        }


        #endregion


        #region Sperregeln für Nachrichten
        /// <summary>
        /// Listet alle Nachrichten auf, denen eine Sperregel zugewiesen wurde
        /// </summary>
        /// <returns></returns>
        internal static DataTable SelectAllBlockedMessages()
        {
            const string query1 = "SELECT Id, Content AS Inhalt, BlockPolicyId AS Sperregel FROM Message Where BlockPolicyId <> 0;"; //Alle Nachrichten, die eine Sperregel haben
            return Sql.SelectDataTable(query1, null);
        }

        /// <summary>
        /// Ändert eine vorhandene Sperregel
        /// </summary>
        /// <param name="form"></param>
        internal static void UpdateBlockPolicy(Dictionary<string, string> form)
        {
#if DEBUG
            foreach (var key in form.Keys)
            {
                Console.WriteLine(key + "=" + form[key]);
            }
#endif
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Id", form["Id"] },
                { "@MonStart", form["MonStart"] },
                { "@MonEnd", form["MonEnd"] },
                { "@TueStart", form["TueStart"] },
                { "@TueEnd", form["TueEnd"] },
                { "@WenStart", form["WenStart"] },
                { "@WenEnd", form["WenEnd"] },
                { "@ThuStart", form["ThuStart"] },
                { "@ThuEnd", form["ThuEnd"] },
                { "@FriStart", form["FriStart"] },
                { "@FriEnd", form["FriEnd"] },
                { "@SatStart", form["SatStart"] },
                { "@SatEnd", form["SatEnd"] },
                { "@SunStart", form["SunStart"] },
                { "@SunEnd", form["SunEnd"] },
                { "@Comment", WebUtility.UrlDecode(form["Comment"]) },
            };

            _ = Sql.NonQueryAsync(
                @"Update BlockPolicy SET 
                MonStart = @MonStart,
                MonEnd = @MonEnd,
                TueStart = @TueStart,
                TueEnd = @TueEnd,
                WenStart = @WenStart,
                WenEnd = @WenEnd,
                ThuStart = @ThuStart,
                ThuEnd = @ThuEnd,
                FriStart = @FriStart,
                FriEnd = @FriEnd,
                SatStart = @SatStart,
                SatEnd = @SatEnd,
                SunStart = @SunStart,
                SunEnd = @SunEnd,
                Comment = @Comment
                WHERE Id = @Id;", args);
        }

        /// <summary>
        /// Erstellt eine neue Sperregel 
        /// </summary>
        /// <param name="form"></param>
        internal static void InsertBlockPolicy(Dictionary<string, string> form)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@MonStart", form["MonStart"] },
                { "@MonEnd", form["MonEnd"] },
                { "@TueStart", form["TueStart"] },
                { "@TueEnd", form["TueEnd"] },
                { "@WenStart", form["WenStart"] },
                { "@WenEnd", form["WenEnd"] },
                { "@ThuStart", form["ThuStart"] },
                { "@ThuEnd", form["ThuEnd"] },
                { "@FriStart", form["FriStart"] },
                { "@FriEnd", form["FriEnd"] },
                { "@SatStart", form["SatStart"] },
                { "@SatEnd", form["SatEnd"] },
                { "@SunStart", form["SunStart"] },
                { "@SunEnd", form["SunEnd"] },
                { "@Comment", WebUtility.UrlDecode(form["Comment"]) },
            };

            _ = Sql.NonQueryAsync(
                @"INSERT INTO BlockPolicy ( 
                MonStart,
                MonEnd,
                TueStart,
                TueEnd,
                WenStart,
                WenEnd,
                ThuStart,
                ThuEnd,
                FriStart,
                FriEnd,
                SatStart,
                SatEnd,
                SunStart,
                SunEnd,
                Comment 
                ) VALUES (
                @MonStart,
                @MonEnd,
                @TueStart,
                @TueEnd,
                @WenStart,
                @WenEnd,
                @ThuStart,
                @ThuEnd,
                @FriStart,
                @FriEnd,
                @SatStart,
                @SatEnd,
                @SunStart,
                @SunEnd,
                @Comment
                );", args);
        }

        /// <summary>
        /// Listet alle vorhandenen Sperregeln tabellarisch auf.
        /// </summary>
        /// <returns></returns>
        internal static DataTable SelectAllBlockPolicies()
        {      
            //Darstellung als Balken??:
            //24 Std = 100% | 1 Std = 100/24 = 4.17%
            return Sql.SelectDataTable(
                @"SELECT 
                Id AS Sperregel,
                MonStart,
                MonEnd,
                TueStart,
                TueEnd,
                WenStart,
                WenEnd,
                ThuStart,
                ThuEnd,
                FriStart,
                FriEnd,
                SatStart,
                SatEnd, SunStart,SunEnd,
                Comment AS Kommentar
                FROM BlockPolicy;", null); //Sperregel
        }

        /// <summary>
        /// Ruft eine bestimmte Sperregel ab
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        internal static DataTable SelectBlockPolicy(uint blockId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@BlockPolicyId", blockId }
            };

            return Sql.SelectDataTable(
                @"SELECT * FROM BlockPolicy WHERE Id = @BlockPolicyId;", args); //Sperregel
        }

        /// <summary>
        /// Ermittelt die Sperregel einer Empfangenen Nachricht anhand der Id
        /// </summary>
        /// <param name="recievedId"></param>
        /// <returns></returns>
        internal static uint SelectBlockPolicyIdFromRecievedId(uint recievedId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@RecId", recievedId }
            };

            var blockPolicyIdObj = Sql.SelectValue(
                @"SELECT BlockPolicyId FROM Message WHERE Id = (SELECT ContentId FROM Recieved WHERE Id = @RecId);", args);

            _ = uint.TryParse(blockPolicyIdObj.ToString(), out uint blockPolicyId);

            return blockPolicyId;
        }

        #endregion


        #region Inhalt von Nachrichten
        /// <summary>
        /// Ruft eine Nachricht anhand einer Empfangs-Id  ab
        /// </summary>
        /// <param name="recievedId">Empfangs-Id aus Tabelle 'Recieved'</param>
        /// <returns></returns>
        internal static DataTable SelectMessageByRecievedId(uint recievedId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@RecId", recievedId }
            };

            return Sql.SelectDataTable(
                @"SELECT Id, Content AS Inhalt, BlockPolicyId AS Sperregel 
                FROM Message WHERE Id = (SELECT ContentId FROM Recieved WHERE Id = @RecId);", args); 
        }

        internal static DataTable SelectMessageByMessagedId(uint messageId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Id", messageId }
            };

            return Sql.SelectDataTable(
                @"SELECT Id, Content AS Inhalt, BlockPolicyId AS Sperregel 
                FROM Message WHERE Id = @Id;", args);
        }

        internal static uint SelectMessageIdByContent(string message) //ungetestet
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Message", message }
            };

            //TODO: Sicherstellen, dass der inhalt von Message eindeuig in der Datenbank ist!!!
            const string query = "INSERT OR IGNORE INTO Message (Content, BlockPolicyId) VALUES (@Message, 0); " +
                "SELECT Id, BlockPolicyId FROM Message WHERE Content = @Message; ";

            var messageIdObj = Sql.SelectValue(query, args);
            _ = uint.TryParse(messageIdObj?.ToString(), out uint messageId);
           
            return messageId;
        }

        /// <summary>
        /// Finde die Sperregel zu einer empfangenen Nachricht
        /// </summary>
        /// <param name="recievedId">Empfangs-ID einer Nachricht</param>
        /// <returns></returns>
        internal static uint SelectMessageIdByRecievedId(uint recievedId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@RecId", recievedId }
            };

            var messageId = Sql.SelectValue(@"SELECT Id FROM Message WHERE Id = (SELECT ContentId FROM Recieved WHERE Id = @RecId);", args);

            return uint.Parse(messageId.ToString());
        }

        

       



        /// <summary>
        /// Ändert eine vorhandene Sperregel
        /// </summary>
        /// <param name="form"></param>
        internal static void UpdateMessagePolicy(Dictionary<string, string> form)
        {
#if DEBUG
            foreach (var key in form.Keys)
            {
                Console.WriteLine(key + "=" + form[key]);
            }
#endif
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@MessageId", form["MessageId"] },
                { "@BlockPolicyId", form["PolicyId"] }
            };

            _ = Sql.NonQueryAsync(
                @"Update Message SET 
                BlockPolicyId = @BlockPolicyId
                WHERE Id = @MessageId;", args);
        }

        #endregion


        #region Versendete Nachrichten
        /// <summary>
        /// Liste versendete Nachrichten eines bestimmten Datums
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static DataTable SelectSent(DateTime date)
        {
            if (date == DateTime.MinValue) { date = DateTime.Now.Date; }

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Gesendet", date.ToString("yyyy-MM-dd") }
            };

            return Sql.SelectDataTable(
                @"SELECT Gesendet, 
                An, 
                Inhalt, 
                Ref, 
                Sendestatus 
                FROM View_Sent 
                WHERE DATE(Gesendet) = DATE(@Gesendet) 
                ORDER BY Gesendet DESC", args);
        }

        /// <summary>
        /// Liste eine Anzahl der zuletzt versendeten Nachrichten
        /// </summary>
        /// <param name="limit">Anzahl anzuzeigender Nachrichten</param>
        /// <returns></returns>
        internal static DataTable SelectSent(uint limit)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Limit", limit }
            };

            return Sql.SelectDataTable(
                @"SELECT Gesendet, 
                An, 
                Inhalt, 
                Ref, 
                Sendestatus 
                FROM View_Sent                         
                ORDER BY Gesendet DESC 
                LIMIT @Limit", args);
        }

        /// <summary>
        /// Trägt eine versendete Nachricht in die DB ein 
        /// </summary>
        /// <param name="sms">SMS-Object mit indernem Index aus dem GSM-Modem</param>
        internal static uint CreateSent(uint messageId, Sms sms)
        {
            #region Empfänger protokollieren und identifizieren
            uint serviceId = Sql.GetServiceId(sms);
            //uint messageId = Sql.SelectMessageIdByContent(sms.Content);
            #endregion

            #region Sms-Empfang protokollieren
            Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@Time", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")},
                    { "@ToId", serviceId},
                    { "@ContentId", messageId },
                    { "@Reference", sms.Index }
                };

            const string query = @"INSERT INTO Sent (Time, ToId, ContentId, Reference) VALUES (@Time, @ToId, @ContentId, @Reference );
                SELECT MAX(Id) FROM Sent;";

            _ = uint.TryParse(Sql.SelectValue(query, args)?.ToString(), out uint sentId);
            #endregion

            return sentId;
        }

        internal static uint CreateSent(string incomingCallFromPhone, string relayCallToPhone)
        {
            uint serviceId = Sql.GetServiceId(relayCallToPhone);
            uint messageId = SelectMessageIdByContent($"Sprachanruf von '{incomingCallFromPhone}' weitergeleitet an '{relayCallToPhone}'");

            Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@Time", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")},
                    { "@ToId", serviceId},
                    { "@ContentId", messageId }
                };

            const string query = @"INSERT INTO Sent (Time, ToId, ContentId) VALUES (@Time, @ToId, @ContentId);
                                    SELECT MAX(Id) FROM Sent;";

            _ = uint.TryParse(Sql.SelectValue(query, args)?.ToString(), out uint sentId);
           
            return sentId;
        }

        /// <summary>
        /// Ergänzt bei einer versendeten Nachricht die Empfangsbestätigung in DB.
        /// Wenn 'Reference' nicht gefunden wird, gibt es keine Warnung.
        /// </summary>
        /// <param name="report">SMS StatusReport mit Refernez zu einer versendeten SMS</param>
        /// <returns>true = StatusReport wurde registriert</returns>
        internal static bool UpdateSentSms(StatusReport report)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@Reference", report.Reference },
                    { "@DeliveryCode", report.DeliveryStatus}                    
                };

            const string query = @"UPDATE Sent SET DeliveryCode = @DeliveryCode WHERE Id = (SELECT MAX(Id) FROM Sent WHERE Reference = @Reference);";

            return Sql.NonQueryAsync(query, args);
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
        public CallRelay(string phone, string status) {
            Phone = phone;
            Status = status;
        }
        public string Phone { get; set; }
        public string Status { get; set; }
    }


    public class Email
    {
        public Email(string from, List<string> to, string content) {
            From = from;
            To = to;
            Body = content;
        }

        public string From { get; set; }

        public List<string> To { get; set; }

        public string Body {get; set; }
    }

    #endregion
}
