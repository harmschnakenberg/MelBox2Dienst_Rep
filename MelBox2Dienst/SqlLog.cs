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

    }

}
