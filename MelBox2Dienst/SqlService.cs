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
                Email,
                Color AS Farbe
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
                Email,
                RecAllMails AS Immer_Email,
                Color AS Farbe
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
                Email,
                Color AS Farbe
                FROM Service                       
                WHERE Name LIKE %@Name%;", args);
        }

        internal static uint GetServiceId(Sms sms)
        {
            //Erst nach Keyword suchen, da Phone nicht eindeutig sein kann.
            const string query1 = "SELECT Id FROM Service WHERE Phone = @Phone LIMIT 1;";
            const string query2 = "INSERT INTO Service (Name, Phone) VALUES ('Neu_' || @Phone, @Phone); ";

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Phone", sms.Phone }
            };

            _ = uint.TryParse(Sql.SelectValue(query1, args)?.ToString(), out uint serviceId);

            if (serviceId == 0) // Nicht gefunden
            {
                Sql.NonQuery(query2, args);
                _ = uint.TryParse(Sql.SelectValue(query1, args)?.ToString(), out serviceId);
            }

            return serviceId;
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
        /// Farbe je Empfänger
        /// </summary>
        /// <returns>ServiceId, Color</returns>
        internal static Dictionary<uint, string> ServiceColors()
        {
            Dictionary<uint, string> dict = new Dictionary<uint, string>();

            DataTable dt = Sql.SelectDataTable(
                @"SELECT 
                Id, 
                Color                               
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
                { "@Email", WebUtility.UrlDecode(form["Email"]) },
                { "@Color", WebUtility.UrlDecode(form["Color"]) }
            };

            _ = Sql.NonQuery(
                @"INSERT INTO Service ( 
                Name, Phone, Email, Color
                ) VALUES ( 
                @Name, @Phone, @Email, @Color
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
                { "@Email", WebUtility.UrlDecode(form["Email"]) },
                { "@Color", WebUtility.UrlDecode(form["Color"]) }
            };

            _ = Sql.NonQuery(
                @"Update Service SET 
                Name = @Name,
                Phone = @Phone,
                Email = @Email,
                Color = @Color  
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

            // Keine Empfänger löschen, wenn sie noch in einer kommenden Bereitschaft eingeteilt sind.
            _ = Sql.NonQuery(
                @"DELETE FROM Service WHERE Id = @Id AND @Id NOT IN (SELECT ToId FROM Shift WHERE END > DATE('now') );", args);

        }

    }

    public class Service
    {

        public Service(DataTable dt)
        {
            Id = uint.Parse(dt.Rows[0]["Id"].ToString());
            Name = dt.Rows[0]["Name"].ToString();
            Phone = dt.Rows[0]["Phone"].ToString();
            Email = dt.Rows[0]["Email"].ToString();
        }

        public Service(DataRow row)
        {
            //Id = uint.Parse(row["Id"].ToString());
            Name = row["Name"].ToString();
            Phone = row["Phone"].ToString();
            Email = row["Email"].ToString();
        }


        public uint Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
