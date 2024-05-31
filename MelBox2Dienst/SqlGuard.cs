using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                Id,
                ServiceId,
                Name,
                Start AS Beginn,
                End AS Ende,
                KW,
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
                strftime(Time, 'localtime') AS Stand, 
                ToId AS ServiceId, 
                Service.Name AS Name, 
                Start AS Beginn,
                End AS Ende
                FROM Shift 
                JOIN Service 
                ON Shift.ToId = Service.Id 
                WHERE Shift.Id = @Id;", args);
        }

    }
}
