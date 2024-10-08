﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    internal partial class Sql
    {
        private static List<Service> CurrentGuards { get; set; } = new List<Service>();

        public static string CallRelayPhone { get; set; } = "+491728362586";

        /// <summary>
        /// Stellt eine Liste der aktuellen Empfänger (Bereitschaft) auf.
        /// </summary>
        /// <returns>Liste der aktuellen Meldeempfänger (falls mehrfach belegt)</returns>
        internal static List<Service> SelectCurrentGuards()
        {
            //Gibt die Kontaktdaten der eingeteilten Bereitschaft aus - oder wenn nicht vorhanden - des Bereitschaftshandys
            const string query = @"SELECT Name, Phone, Email FROM View_CurrentShift;";

            DataTable dt = Sql.SelectDataTable(query, null);

            List<Service> serviceList = new List<Service>();

            foreach (DataRow row in dt.Rows)            
                serviceList.Add(new Service(row));
            
            if (serviceList.Count == 0)
                Log.Error("Es konnte kein Empfänger in der Bereitschaft ermittelt werden!");

            if (serviceList != CurrentGuards) //Wenn sich die Bereitschaft geändert hat
            {
                CurrentGuards = serviceList;
                //Rufumleitung ändern
                //MelBox2Service.CheckCallRelayNumber(); //BÖSE?!
            }

            return serviceList;
        }

        internal static List<Service> SelectPermamanentGuards()
        {
            //Gibt die Kontaktdaten der eingeteilten ständigen Empfänger
            const string query = @"SELECT Name, '' AS Phone, Email FROM Service WHERE RecAllMails > 0;";

            DataTable dt = Sql.SelectDataTable(query, null);

            List<Service> permanentServiceList = new List<Service>();

            foreach (DataRow row in dt.Rows)
                permanentServiceList.Add(new Service(row));

            if (permanentServiceList.Count == 0)
                Log.Error("Es konnte kein ständiger Empfänger ermittelt werden!");

            return permanentServiceList;
        }


        internal static DataTable SelectAllGuards()
        {
            return Sql.SelectDataTable(
                @"SELECT 
                Id ||'' AS Id,
                ServiceId ||'' AS ServiceId,
                Name,
                Start AS Beginn,
                End AS Ende,
                KW ||'' AS KW,
                Mo,Di,Mi,Do,Fr,Sa,So
                FROM View_Calendar_Full;", null);
        }

        internal static DataTable SelectGuardById(uint guardId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Id", guardId }
            };

            return Sql.SelectDataTable(
                @"SELECT 
                Shift.Id AS Id, 
                strftime('%d.%m.%Y %H:%M:%S', Time, 'localtime') AS Stand, 
                ToId AS ServiceId, 
                Service.Name AS Name, 
                Start AS Beginn,
                End AS Ende
                FROM Shift 
                JOIN Service 
                ON Shift.ToId = Service.Id 
                WHERE Shift.Id = @Id;", args);
        }


        #region CrUD Bereitschaft

        /// <summary>
        /// Ändert vorhandene Stammdaten eines Mitarbeiters
        /// </summary>
        /// <param name="form"></param>
        internal static void UpdateGuard(Dictionary<string, string> form)
        {
#if DEBUG
            foreach (var key in form.Keys)
            {
                Console.WriteLine(key + "=" + form[key]);
            }
#endif
            _ = DateTime.TryParse(form["StartDate"], out DateTime startDate);
            _ = TimeSpan.TryParse(form["StartTime"], out  TimeSpan startTime);
            _ = DateTime.TryParse(form["EndDate"], out DateTime endDate);
            _ = TimeSpan.TryParse(form["EndTime"], out TimeSpan endTime);


            // TODO: Wenn der Zeitraum mehr als eine Kalenderwoche umfasst, splitten für korrekte Darstellung!?

            #region Bereitschaft aufteilen in Kalenderwochen
            //var days = (endDate - startDate).TotalDays;

            //DateTime localStart = startDate;
            //DateTime localEnd = startDate;

            //while (localEnd.Date != endDate.Date)
            //{
            //    do
            //    {
            //        localEnd = localEnd.AddDays(1);
            //    }
            //    while (localEnd.Date != endDate.Date && localEnd.DayOfWeek != DayOfWeek.Monday);

            //    #region Uhrzeit setzen
            //    //if (localStart.Date == startDate.Date)
            //    //    localEnd = localEnd.Add(endTime);

            //    //if (localEnd.Date == endDate.Date)
            //    //    localEnd = localEnd.Add(endTime);
            //    #endregion

            //    #region Bereitschaft Update oder Neu erstellen
            //    Dictionary<string, object> args = new Dictionary<string, object>
            //    {
            //        { "@Id", form["Id"] },
            //       // { "@Name", WebUtility.UrlDecode(form["Name"]) },
            //        { "@ToId", form["ServiceId"] },
            //        { "@Start", localStart.Add(startTime).ToUniversalTime()  },
            //        { "@End", localEnd.Add(endTime).ToUniversalTime()  }
            //    };

            //    if (!Sql.NonQueryAsync(
            //        @"Update Shift SET 
            //        Time = DATETIME('now'),
            //        ToId = @ToId,
            //        Start = @Start,
            //        End = @End 
            //        WHERE Id = @Id;", args))
            //        _ = Sql.NonQueryAsync(
            //        @"INSERT INTO Shift ( 
            //        ToId, 
            //        Start,
            //        End 
            //        ) VALUES (
            //        @ToId,
            //        @Start,
            //        @End
            //        );", args);

            //    #endregion

            //    localStart = localEnd;
            //}

            // Log.Info($"Rufbereitschaft {localStart.Add(startTime)} bis {localEnd.Add(endTime)} geändert.");


            #endregion

            #region Bereitschaft Update oder Neu erstellen hne Aufteilung in Kalenderwochen
            Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@Id", form["Id"] },
                    { "@ToId", form["ServiceId"] },
                    { "@Start", startDate.Add(startTime).ToUniversalTime()  },
                    { "@End", endDate.Add(endTime).ToUniversalTime()  }
                };

            if (!Sql.NonQueryAsync(
                @"Update Shift SET 
                    Time = DATETIME('now'),
                    ToId = @ToId,
                    Start = @Start,
                    End = @End 
                    WHERE Id = @Id;", args))
                _ = Sql.NonQueryAsync(
                @"INSERT INTO Shift ( 
                    ToId, 
                    Start,
                    End 
                    ) VALUES (
                    @ToId,
                    @Start,
                    @End
                    );", args);

            Log.Info($"Rufbereitschaft {startDate.Add(startTime)} bis {endDate.Add(endTime)} geändert.");
            #endregion


        }


        internal static void CreateGuard(Dictionary<string, string> form)
        {
#if DEBUG
            foreach (var key in form.Keys)
            {
                Console.WriteLine(key + "=" + form[key]);
            }
#endif
            _ = DateTime.TryParse(form["StartDate"], out DateTime startDate);
            _ = TimeSpan.TryParse(form["StartTime"], out TimeSpan startTime);
            _ = DateTime.TryParse(form["EndDate"], out DateTime endDate);
            _ = TimeSpan.TryParse(form["EndTime"], out TimeSpan endTime);


            // TODO: Wenn der Zeitraum mehr als eine Kalenderwoche umfasst, splitten für korrekte Darstellung!?

            #region Bereitschaft aufteilen in Kalenderwochen
            //var days = (endDate - startDate).TotalDays;

            //DateTime localStart = startDate;
            //DateTime localEnd = startDate;

            //while (localEnd.Date != endDate.Date)
            //{
            //    do
            //    {
            //        localEnd = localEnd.AddDays(1);
            //    }
            //    while (localEnd.Date != endDate.Date && localEnd.DayOfWeek != DayOfWeek.Monday);

            //    #region Bereitschaft Update
            //    Dictionary<string, object> args = new Dictionary<string, object>
            //    {
            //        { "@ToId", form["ServiceId"] },
            //        { "@Start", localStart.Add(startTime).ToUniversalTime()  },
            //        { "@End", localEnd.Add(endTime).ToUniversalTime()  }
            //    };

            //    _ = Sql.NonQueryAsync(
            //        @"INSERT INTO Shift ( 
            //        ToId, 
            //        Start,
            //        End 
            //        ) VALUES (
            //        @ToId,
            //        @Start,
            //        @End
            //        );", args);
            //    #endregion

            //    localStart = localEnd;
            //}

            //Log.Info($"Rufbereitschaft {localStart.Add(startTime)} bis {localEnd.Add(endTime)} erstellt.");

            #endregion

            #region Bereitschaft Update ohne Aufteilung in Kalenderwochen
            Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@ToId", form["ServiceId"] },
                    { "@Start", startDate.Add(startTime).ToUniversalTime()  },
                    { "@End", endDate.Add(endTime).ToUniversalTime()  }
                };

            _ = Sql.NonQueryAsync(
                @"INSERT INTO Shift ( 
                    ToId, 
                    Start,
                    End 
                    ) VALUES (
                    @ToId,
                    @Start,
                    @End
                    );", args);
            #endregion
        }


        internal static void DeleteGuard(Dictionary<string, string> form)
        {
#if DEBUG
            foreach (var key in form.Keys)
            {
                Console.WriteLine(key + "=" + form[key]);
            }
#endif
                #region Bereitschaft löschen
                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@Id", form["Id"] }
                };

            _ = Sql.NonQueryAsync(
                @"DELETE FROM Shift  
                WHERE Id = @Id;", args);

            #endregion

            _ = DateTime.TryParse(form["StartDate"], out DateTime startDate);
            _ = TimeSpan.TryParse(form["StartTime"], out TimeSpan startTime);
            _ = DateTime.TryParse(form["EndDate"], out DateTime endDate);
            _ = TimeSpan.TryParse(form["EndTime"], out TimeSpan endTime);

            Log.Info($"Rufbereitschaft {startDate.Add(startTime)} bis {endDate.Add(endTime)} gelöscht.");
        }

        #endregion


    }
}
