using System;
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


            // TODO: Wenn der Zeitraum mehr als eine Kalenderwoche umfasst, splitten für korrekte Darstellung!

            #region Bereitschaft aufteilen in Kalenderwochen
            //var days = (endDate - startDate).TotalDays;

            DateTime localStart = startDate;
            DateTime localEnd = startDate;

            while (localEnd.Date != endDate.Date)
            {
                do
                {
                    localEnd = localEnd.AddDays(1);
                }
                while (localEnd.Date != endDate.Date && localEnd.DayOfWeek != DayOfWeek.Monday);


                #region Bereitschaft Update oder Neu erstellen
                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@Id", form["Id"] },
                   // { "@Name", WebUtility.UrlDecode(form["Name"]) },
                    { "@ToId", form["ServiceId"] },
                    { "@Start", startDate.Add(startTime).ToUniversalTime()  },
                    { "@End", endDate.Add(endTime).ToUniversalTime()  }
                };

                if (!Sql.NonQuery(
                    @"Update Shift SET 
                    Time = DATETIME('now'),
                    ToId = @ToId,
                    Start = @Start,
                    End = @End 
                    WHERE Id = @Id;", args))
                    _ = Sql.NonQuery(
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

                localStart = localEnd;
            }

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


            // TODO: Wenn der Zeitraum mehr als eine Kalenderwoche umfasst, splitten für korrekte Darstellung!

            #region Bereitschaft aufteilen in Kalenderwochen
            var days = (endDate - startDate).TotalDays;

            DateTime localStart = startDate;
            DateTime localEnd = startDate;

            while (localEnd.Date != endDate.Date)
            {
                do
                {
                    localEnd = localEnd.AddDays(1);
                }
                while (localEnd.Date != endDate.Date && localEnd.DayOfWeek != DayOfWeek.Monday);


                #region Bereitschaft Update
                Dictionary<string, object> args = new Dictionary<string, object>
                {

                    { "@ToId", form["ServiceId"] },
                    { "@Start", startDate.Add(startTime).ToUniversalTime()  },
                    { "@End", endDate.Add(endTime).ToUniversalTime()  }
                };

                _ = Sql.NonQuery(
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

                localStart = localEnd;
            }

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

            Sql.NonQuery(
                @"DELETE FROM Shift  
                WHERE Id = @Id;", args);
                  
                #endregion

            }

         


        


    }
}
