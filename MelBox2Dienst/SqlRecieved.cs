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
    }
}
