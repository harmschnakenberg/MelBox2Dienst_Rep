using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    internal static partial class Sql
    {
        internal static DataTable SelectAllService()
        {
            return Sql.SelectDataTable(
                @"SELECT Id, 
                Name, 
                Phone AS Mobil, 
                Email
                FROM Service 
                ORDER BY Name;", null);
        }

        internal static DataTable SelectServiceById(uint serviceId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Id", serviceId }
            };

            return Sql.SelectDataTable(
                @"SELECT Id, 
                Name, 
                Phone AS Mobil, 
                Email
                FROM Service 
                WHERE Id = @Id;", args);
        }

        internal static DataTable SelectServiceByName(string namepart)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Name", namepart}
            };

            return Sql.SelectDataTable(
                @"SELECT Id, 
                Name, 
                Phone AS Mobil, 
                Email                
                FROM Service                       
                WHERE Name LIKE %@Name%;", args);
        }

        internal static Dictionary<uint, string> ServiceNames()
        {
            Dictionary<uint, string> dict = new Dictionary<uint, string>();

            DataTable dt = Sql.SelectDataTable(
                @"SELECT 
                Id, 
                Name                                
                FROM Service;", null);

            for (int x = 0; x < dt.Rows.Count; x++)
            {
                uint.TryParse(dt.Rows[x][0].ToString(), out uint id);

                dict.Add(id, dt.Rows[x][1].ToString());
            }
            return dict;
        }

        /// <summary>
        /// Erzeugt neue Stammdaten eines Mitarbeiters
        /// </summary>
        /// <param name="form"></param>
        internal static void CreateService(Dictionary<string, string> form)
        {
#if DEBUG
            foreach (var key in form.Keys)
            {
                Console.WriteLine(key + "=" + form[key]);
            }
#endif
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Name", WebUtility.UrlDecode(form["Name"]) },
                { "@Phone", '+' + WebUtility.UrlDecode(form["Phone"].TrimStart('+').Replace(" ", "")) },
                { "@Email", WebUtility.UrlDecode(form["Email"]) }
            };

            _ = Sql.NonQuery(
                @"INSERT INTO Service ( 
                Name, Phone, Email
                ) VALUES ( 
                @Name, @Phone, @Email 
                );", args);
        }

        /// <summary>
        /// Ändert vorhandene Stammdaten eines Mitarbeiters
        /// </summary>
        /// <param name="form"></param>
        internal static void UpdateService(Dictionary<string, string> form)
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
                { "@Name", WebUtility.UrlDecode(form["Name"]) },
                { "@Phone", '+' + WebUtility.UrlDecode(form["Phone"].TrimStart('+').Replace(" ", "")) },
                { "@Email", WebUtility.UrlDecode(form["Email"]) }               
            };

            _ = Sql.NonQuery(
                @"Update Service SET 
                Name = @Name,
                Phone = @Phone,
                Email = @Email              
                WHERE Id = @Id;", args);
        }

        /// <summary>
        /// Löscht vorhandene Stammdaten eines Mitarbeiters
        /// </summary>
        /// <param name="form"></param>
        internal static void DeleteService(Dictionary<string, string> form)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Id", form["Id"] }
            };

            _ = Sql.NonQuery(
                @"DELETE FROM Service WHERE Id = @Id;", args);
        }

    }
}
