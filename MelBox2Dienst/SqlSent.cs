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
    }

}
