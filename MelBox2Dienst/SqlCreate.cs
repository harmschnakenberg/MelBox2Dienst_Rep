using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace MelBox2Dienst
{
    internal partial class Sql
    {
        static string DbPath { get; } = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "db", "MelBox2.db");

        internal static bool CheckDbFile()
        {
            if (!File.Exists(DbPath))
                CreateNewDataBase();

            #region Mehrfach versuchen die Datenbank zu öffnen, falls sie grade in Benutzung ist

            int numTries = 10;

            while (numTries > 0)
            {
                --numTries;

                try
                {
                    using (FileStream stream = new FileInfo(DbPath).Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        stream.Close();
                        break;
                    }
                }
                catch (IOException)
                {
                    //the file is unavailable because it is:
                    //still being written to
                    //or being processed by another thread
                    //or does not exist (has already been processed)
                    Thread.Sleep(200);
                }
            }

            if (numTries == 0)
            {
                string txt = $"Die Datenbankdatei >{DbPath}< ist durch ein anderes Programm gesperrt.";
                Console.WriteLine(txt);
                Sql.InsertLog(1, txt);
                Log.Error(txt);
            }
            return numTries > 0;

            #endregion
        }


        private static void CreateNewDataBase()
        {
            Log.Info($"Erstelle eine neue Datenbank-Datei unter '{DbPath}'");

            try
            {
                //Erstelle Datenbank-Datei und öffne einmal zum Testen
                _ = Directory.CreateDirectory(Path.GetDirectoryName(DbPath));
                FileStream stream = File.Create(DbPath);
                stream.Close();

                System.Threading.Thread.Sleep(500);

                #region Tabellen erstellen
                string query =  @"CREATE TABLE IF NOT EXISTS Log ( 
                          Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                          Time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, 
                          Prio INTEGER NOT NULL,
                          Content TEXT 
                          ); ";
                NonQuery(query, null);

                query = @"CREATE TABLE IF NOT EXISTS Customer ( 
                          Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                          Name TEXT NOT NULL UNIQUE,
                          Phone TEXT, 
                          Email TEXT, 
                          KeyWord TEXT, 
                          MaxInactiveHours INTEGER DEFAULT 0 
                          ); ";
                NonQuery(query, null);

                query = @"CREATE TABLE IF NOT EXISTS Service (
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                        Name TEXT NOT NULL UNIQUE, 
                        Phone TEXT, 
                        Email TEXT,
                        RecAllMails INTEGER DEFAULT 0,
                        Color TEXT DEFAULT '#CCCCCC'                       
                        ); ";
                NonQuery(query, null);

                query = @"CREATE TABLE IF NOT EXISTS BlockPolicy ( 
                         Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                         MonStart INTEGER DEFAULT 0 , 
                         MonEnd INTEGER DEFAULT 0 , 
                         TueStart INTEGER DEFAULT 0 , 
                         TueEnd INTEGER DEFAULT 0 , 
                         WenStart INTEGER DEFAULT 0 , 
                         WenEnd INTEGER DEFAULT 0 , 
                         ThuStart INTEGER DEFAULT 0 , 
                         ThuEnd INTEGER DEFAULT 0 , 
                         FriStart INTEGER DEFAULT 0 , 
                         FriEnd INTEGER DEFAULT 0 , 
                         SatStart INTEGER DEFAULT 0 , 
                         SatEnd INTEGER DEFAULT 0 , 
                         SunStart INTEGER DEFAULT 0 , 
                         SunEnd INTEGER DEFAULT 0 , 
                         Comment TEXT 
                         ); ";
                NonQuery(query, null);

                query = @"CREATE TABLE IF NOT EXISTS Message ( 
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                        Time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, 
                        Content TEXT NOT NULL UNIQUE, 
                        BlockPolicyId INTEGER, 

                        CONSTRAINT fk_BlockPolicyId FOREIGN KEY (BlockPolicyId) REFERENCES BlockPolicy (Id) ON DELETE SET DEFAULT
                        ); ";
                NonQuery(query, null);

                query = @"CREATE TABLE IF NOT EXISTS Recieved ( 
                       Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                       Time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, 
                       SenderId INTEGER, 
                       ContentId INTEGER, 

                       CONSTRAINT fk_SenderId FOREIGN KEY (SenderId) REFERENCES Customer (Id) ON DELETE RESTRICT, 
                       CONSTRAINT fk_ContentId FOREIGN KEY (ContentId) REFERENCES Message (Id) ON DELETE RESTRICT 
                       ); ";
                NonQuery(query, null);

                query = @"CREATE TABLE IF NOT EXISTS Sent ( 
                        Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                        Time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, 
                        ToId INTEGER NOT NULL, 
                        ContentId INTEGER, 
                        Reference INTEGER, 
                        DeliveryCode INTEGER, 

                        CONSTRAINT fk_ToId FOREIGN KEY (ToId) REFERENCES Service (Id) ON DELETE RESTRICT , 
                        CONSTRAINT fk_ContentId FOREIGN KEY (ContentId) REFERENCES Message (Id) ON DELETE RESTRICT 
                        ); ";
                NonQuery(query, null);


                query = @"CREATE TABLE IF NOT EXISTS Shift ( 
                        Id INTEGER NOT NULL PRIMARY KEY, 
                        Time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, 
                        ToId INTEGER, 
                        Start TEXT NOT NULL, 
                        End TEXT NOT NULL, 

                        CONSTRAINT fk_ToId FOREIGN KEY (ToId) REFERENCES Service (Id) ON DELETE RESTRICT
                        ); ";
                NonQuery(query, null);

                #endregion

                #region Views erstellen

                query = @"CREATE VIEW View_YearFromToday AS 
                        SELECT 
                        CASE(CAST(strftime('%w', d) AS INT) +6) % 7 WHEN 0 THEN 'Mo' WHEN 1 THEN 'Di' WHEN 2 THEN 'Mi' WHEN 3 THEN 'Do' WHEN 4 THEN 'Fr' WHEN 5 THEN 'Sa' ELSE 'So' END AS Tag
                        , d
                        , strftime('%W', d) AS KW 
                        FROM(
                          WITH RECURSIVE dates(d) AS(
                            VALUES(date('now')) 
                            UNION ALL 
                            SELECT date(d, '+1 day') FROM dates WHERE d < date('now', '+1 year')
	                        )
	                        SELECT d FROM dates
                        ); ";
                NonQuery(query, null);

                query = @"CREATE VIEW View_BlockedNow AS 
                        SELECT Id AS BlockPolicyId,
                        CAST(strftime('%H', 'now', 'localtime') AS INTEGER) 
                        BETWEEN 
                         CASE CAST (strftime('%w', 'now', 'localtime') AS INTEGER)
                          WHEN 0 THEN SunStart
                          WHEN 1 THEN MonStart
                          WHEN 2 THEN TueStart
                          WHEN 3 THEN WenStart
                          WHEN 4 THEN ThuStart
                          WHEN 5 THEN FriStart 
                          WHEN 6 THEN SatStart 
                         END 
                        AND
                         CASE CAST (strftime('%w', 'now', 'localtime') AS INTEGER)
                          WHEN 0 THEN SunEnd
                          WHEN 1 THEN MonEnd
                          WHEN 2 THEN TueEnd
                          WHEN 3 THEN WenEnd
                          WHEN 4 THEN ThuEnd
                          WHEN 5 THEN FriEnd 
                          WHEN 6 THEN SatEnd 
                         END AS BlockedNow
                         FROM BlockPolicy; ";

                query = @"CREATE VIEW View_AllShiftDays AS 
                        SELECT d AS Tag, COUNT(d) AS Belegung, ToId
                        FROM View_YearFromToday 
                        JOIN Shift ON d BETWEEN Date(Start) AND Date(End) 
                        GROUP BY d 
                        HAVING Max(Start) 
                        ORDER BY d; ";
                NonQuery(query, null);

                query = @"CREATE VIEW View_CurrentShift AS 
                        SELECT Name, Phone, Email
                        FROM SHIFT AS g
                        JOIN Service AS s ON g.ToId = s.Id
                        WHERE current_timestamp BETWEEN Start AND End
                        UNION 
                        SELECT Name, Phone, Email
                        FROM Service WHERE Name = 'Bereitschaftshandy' AND NOT EXISTS (SELECT Id FROM Shift WHERE current_timestamp BETWEEN Start AND End)";
                NonQuery(query, null);

                query = @"CREATE VIEW View_Sent AS 
                        SELECT strftime('%Y-%m-%d %H:%M:%S', ls.Time, 'localtime') AS Gesendet, s.Name AS An, Content AS Inhalt, Reference AS Ref, 
                        CASE WHEN DeliveryCode = 255 THEN 'abgesendet' WHEN DeliveryCode > 32 THEN 'abgebrochen' WHEN DeliveryCode > 16 THEN 'abwarten' WHEN DeliveryCode = 1 THEN 'unbestätigt' WHEN DeliveryCode = 0 THEN 'bestätigt' ELSE 'Status '||DeliveryCode END AS Sendestatus 
                        FROM Sent AS ls JOIN Service AS s ON ToId = s.Id JOIN Message AS mc ON mc.id = ls.ContentId; ";
                NonQuery(query, null);

                query = @"CREATE VIEW View_Recieved AS 
                        SELECT r.Id As Nr, strftime('%Y-%m-%d %H:%M:%S', r.Time, 'localtime') AS Empfangen, Name AS Von, Content AS Inhalt, BlockPolicyId AS Sperregel 
                        FROM Recieved AS r JOIN Customer AS c ON SenderId = c.Id JOIN Message AS m ON r.ContentId = m.Id;";
                NonQuery(query, null);

                query = @"  CREATE VIEW View_Calendar AS
                            SELECT DISTINCT
                            strftime('%W', d, 'weekday 6') AS KW ,
                            DATE(d, 'localtime', 'weekday 6', '-5 days') AS Mo,
                            DATE(d, 'localtime', 'weekday 6', '-4 days') AS Di,
                            DATE(d, 'localtime', 'weekday 6', '-3 days') AS Mi,
                            DATE(d, 'localtime', 'weekday 6', '-2 days') AS Do,
                            DATE(d, 'localtime', 'weekday 6', '-1 days') AS Fr,
                            DATE(d, 'localtime', 'weekday 6', '-0 days') AS Sa,
                            DATE(d, 'localtime', 'weekday 6', '+1 days') AS So
                            FROM View_YearFromToday
                            GROUP BY KW
                            ORDER BY d; ";

                NonQuery(query, null);

                query = @"CREATE VIEW View_Calendar_Full AS
                            SELECT 
                            s.Id
                            , s.ToId AS ServiceId
                            , p.name
                            , p.Color
                            , s.Start
                            , s.End
                            , KW
                            , Mo || CASE WHEN Mo IN (SELECT Tag FROM View_AllShiftDays) THEN (SELECT Belegung||ToId FROM View_AllShiftDays WHERE Tag = Mo) ELSE '' END AS Mo
                            , Di || CASE WHEN Di IN (SELECT Tag FROM View_AllShiftDays) THEN (SELECT Belegung||ToId FROM View_AllShiftDays WHERE Tag = Di) ELSE '' END AS Di
                            , Mi || CASE WHEN Mi IN (SELECT Tag FROM View_AllShiftDays) THEN (SELECT Belegung||ToId FROM View_AllShiftDays WHERE Tag = Mi) ELSE '' END AS Mi
                            , Do || CASE WHEN Do IN (SELECT Tag FROM View_AllShiftDays) THEN (SELECT Belegung||ToId FROM View_AllShiftDays WHERE Tag = Do) ELSE '' END AS Do
                            , Fr || CASE WHEN Fr IN (SELECT Tag FROM View_AllShiftDays) THEN (SELECT Belegung||ToId FROM View_AllShiftDays WHERE Tag = Fr) ELSE '' END AS Fr
                            , Sa || CASE WHEN Sa IN (SELECT Tag FROM View_AllShiftDays) THEN (SELECT Belegung||ToId FROM View_AllShiftDays WHERE Tag = Sa) ELSE '' END AS Sa
                            , So || CASE WHEN So IN (SELECT Tag FROM View_AllShiftDays) THEN (SELECT Belegung||ToId FROM View_AllShiftDays WHERE Tag = So) ELSE '' END AS So
                            FROM View_Calendar
                            LEFT JOIN Shift AS s ON (strftime('%Y%W', Mo) BETWEEN strftime('%Y%W', s.Start) AND strftime('%Y%W', s.End, '-1 day'))
                            LEFT JOIN Service p ON s.ToId = p.Id
                            ORDER BY Mo; ";
                NonQuery(query, null);

                //query = @"CREATE TABLE IF NOT EXISTS Call (                        
                //            Phone TEXT NOT NULL DEFAULT '+491728362586' 
                //            );";
                #endregion

                #region Tabellen füllen

                query = "INSERT INTO Log (Prio, Content) VALUES (3, 'Datenbank neu erstellt.'); ";

                query += $"INSERT INTO Customer (Name, Phone, Email, KeyWord, MaxInactiveHours) VALUES ('TestKunde', '+4916095285xxx', 'harm.schnakenberg@kreutztraeger.de', 'TestKunde', 0); ";

                query += $"INSERT INTO Service (Id, Name, Phone, Email, RecAllMails, Color) VALUES (0, 'SMSZentrale', '+4916095285xxx', 'harm.schnakenberg@kreutztraeger.de', 1, '#b4b4b4'); ";
#if !DEBUG
                query += $"INSERT INTO Service (Name, Phone, Email, Color)  VALUES ('Bereitschaftshandy', '+491728362586', 'bereitschaftshandy@kreutztraeger.de', '#0000ff'); ";
#else
                query += $"INSERT INTO Service (Name, Phone, Email, Color)  VALUES ('Bereitschaftshandy', '+4916095285304', 'harm.schnakenberg@kreutztraeger.de', '#3cb371'); ";
#endif

                query += "INSERT INTO BlockPolicy (Id, MonStart, MonEnd, TueStart, TueEnd, WenStart, WenEnd, ThuStart, ThuEnd, FriStart, FriEnd, SatStart, SatEnd, SunStart, SunEnd, Comment) VALUES (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,'nie gesperrt'); ";
                query += "INSERT INTO BlockPolicy (Id, MonStart, MonEnd, TueStart, TueEnd, WenStart, WenEnd, ThuStart, ThuEnd, FriStart, FriEnd, SatStart, SatEnd, SunStart, SunEnd, Comment) VALUES (1, 0,24, 0,24, 0,24, 0,24, 0,24, 0,24, 0,24,'immer gesperrt'); ";
                query += "INSERT INTO BlockPolicy (Id, MonStart, MonEnd, TueStart, TueEnd, WenStart, WenEnd, ThuStart, ThuEnd, FriStart, FriEnd, SatStart, SatEnd, SunStart, SunEnd, Comment) VALUES (3, 7,17, 7,17, 7,17, 7,17, 7,15, 0,24, 0,24,'in der Woche tagsüber gesperrt'); ";
                query += "INSERT INTO BlockPolicy (Id, MonStart, MonEnd, TueStart, TueEnd, WenStart, WenEnd, ThuStart, ThuEnd, FriStart, FriEnd, SatStart, SatEnd, SunStart, SunEnd, Comment) VALUES (4, 0, 0, 0, 0, 0, 0, 0, 0,15,24, 0,24, 0,24,'am Wochenende gesperrt'); ";

                query += "INSERT INTO Message (Content, BlockPolicyId) VALUES ('Datenbank neu erstellt.', 1); ";
#if DEBUG
                query += "INSERT INTO Message (Content, BlockPolicyId) VALUES ('Testnachricht. Am Wochenende gesperrt.', 4); ";
#endif

                query += "INSERT INTO Recieved (SenderId, ContentId) VALUES (1, 1); ";

                query += "INSERT INTO Sent (ToId, ContentId, Reference, DeliveryCode) VALUES (0, 1, 0, 0); ";

                query += "INSERT INTO Shift (ToId, Start, End) VALUES (0, DATETIME('now','weekday 1', '-3 days'), DATETIME('now','weekday 1','+7 day')); ";

                //query += "INSERT INTO Call (Phone) VALUES ('+491728362586'); ";
                NonQuery(query, null);

                #endregion

            }
            catch (Exception ex)
            {
                Log.Error("CreateNewDataBase() " + ex.Message + ex.StackTrace);
                throw ex;
            }

        }
    }
}