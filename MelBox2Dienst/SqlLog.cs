using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    internal static partial class Sql
    {

        internal static void InsertLog(int prio, string content)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Prio", prio },
                { "@Content", content }
            };

            _ = NonQueryAsync($"INSERT INTO Log (Prio, Content) VALUES (@Prio, @Content);", args);
        }

        internal static DataTable SelectLog(DateTime date)
        {
            if (date == DateTime.MinValue) { date = DateTime.Now.Date; }

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Time", date.ToString("yyyy-MM-dd") }
            };

            return Sql.SelectDataTable(
                @"SELECT 
                    Time AS Zeit,
                    Prio,
                    Content AS Inhalt
                    FROM Log
                    WHERE DATE(Time) = DATE(@Time) 
                    ORDER BY Time DESC", args);
        }

        internal static DataTable SelectLog(string content)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Content", content }
            };

            return Sql.SelectDataTable(
                @"SELECT 
                    datetime(Time,'localtime') AS Zeit,
                    Prio,
                    Content AS Inhalt
                    FROM Log
                    WHERE Content LIKE '%'||@Content||'%' 
                    ORDER BY Time DESC", args);
        }

        internal static DataTable SelectLog(uint limit)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Limit", limit }
            };

            return Sql.SelectDataTable(
                @"SELECT 
                    datetime(Time,'localtime') AS Zeit,
                    Prio,
                    Content AS Inhalt
                    FROM Log
                    ORDER BY Time DESC
                    LIMIT @Limit", args);
        }

        #region Mobilfunknetzqualoität dokumentieren
        /// <summary>
        /// Protokolliert die Qualität des Mobilfunkempfangs
        /// </summary>
        /// <param name="quality"></param>
        internal static void CreateNetworkQualityEntry(int quality)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Quality", quality }
            };

            _ = NonQueryAsync(@"INSERT INTO NetworkQuality (Quality) VALUES (@Quality);  
                DELETE FROM NetworkQuality WHERE Id < (SELECT MAX(Id) - 1000 FROM NetworkQuality);", args); //max. 1000 Einträge
        }

        /// <summary>
        /// Erstellt einen String zur Darstellung der Mobilfunksignalqualität in einem Diagramm
        /// siehe Google Charts https://developers.google.com/chart?hl=de
        /// </summary>
        /// <returns></returns>
        internal static string SelectNetworkQualityToArray()
        {
            DataTable dt = Sql.SelectDataTable(@"SELECT datetime(Time, 'localtime')||'' AS Time, Quality FROM NetworkQuality", null);
            StringBuilder sb = new StringBuilder();
            sb.Append("['Zeit', 'Signal']");

            foreach (DataRow row in dt.Rows)
            {                
                _ = DateTime.TryParse(row[0].ToString(), out DateTime d);
                sb.Append($",[new Date({d.Year},{d.Month},{d.Day},{d.Hour},{d.Minute},{d.Second}),{row[1]}]");
            }

            return sb.ToString();
        }
        #endregion
    }

}
