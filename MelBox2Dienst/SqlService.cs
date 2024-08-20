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

        internal static Service GetService(string phone)
        {            
            //Erst nach Keyword suchen, da Phone nicht eindeutig sein kann.
            const string query1 = "SELECT * FROM Service WHERE Phone = @Phone LIMIT 1;";
            const string query2 = "INSERT OR IGNORE INTO Service (Name, Phone) VALUES ('Neu_' || @Phone, @Phone); ";

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Phone", phone }
            };
            DataTable dt1 = Sql.SelectDataTable(query1, args);

            if (dt1.Rows.Count == 0)
            {
                _ = Sql.NonQueryAsync(query2, args);
                dt1 = Sql.SelectDataTable(query1, args);
            }

            return new Service(dt1);
        }

        internal static uint GetServiceId(string phone)
        {
            //Erst nach Keyword suchen, da Phone nicht eindeutig sein kann.
            const string query1 = "SELECT Id FROM Service WHERE Phone = @Phone LIMIT 1;";
            const string query2 = "INSERT OR IGNORE INTO Service (Name, Phone) VALUES ('Neu_' || @Phone, @Phone); ";

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Phone", phone }
            };

            if (uint.TryParse(Sql.SelectValue(query1, args)?.ToString(), out uint serviceId)) // Nicht gefunden
            {
                _ = Sql.NonQueryAsync(query2, args);
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
        /// Registriert  einen neuen Benutzer 'Service' in der Datenbank.
        /// Der Benutzer muss durch einen Adminsitrator freigeschaltet werden, damit der Login funktioniert.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        internal static void Register(string name, string password)
        {
            string encryped_pw = Encrypt(password);

            const string query = @"INSERT INTO Service (Name, Password ) VALUES (@Name, @Password)
                                    SELECT Id, Name FROM Service WHERE Name = @Name AND Password = @Password;";

            Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@Name", name },
                    { "@Password", "_"  +encryped_pw } // Vorangestelltes Zeichen muss beim Freischalten durch den Admin wieder entfernt werden.
                };

            _ = Sql.NonQueryAsync(query, args);
        }


        internal static Service CheckCredentials(string name, string password)
        {
            //try
            //{
                string encryped_pw = Encrypt(password);

                const string query = "SELECT Id, Name FROM Service WHERE Name = @Name AND Password = @Password;";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@Name", name },
                    { "@Password", encryped_pw }
                };

                DataTable dt = SelectDataTable(query, args);
                return new Service(dt);

                //if (service.Id > 0)
                //{
                //    while (Server.LogedInHash.Count > 10) //Max. 10 Benutzer gleichzetig eingelogged
                //    {
                //        Server.LogedInHash.Remove(Server.LogedInHash.Keys.GetEnumerator().Current);
                //    }

                //    string guid = Guid.NewGuid().ToString("N");

                //    Server.LogedInHash.Add(guid, p);

                //    return guid;
                //}
            //}
            //catch (Exception)
            //{
            //    throw;
            //    // Was tun?
            //}

            //return new Service();
        }

        private static string Encrypt(string password)
        {
            if (password == null) return password;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(password);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            return System.Text.Encoding.UTF8.GetString(data);
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

            _ = Sql.NonQueryAsync(
                @"INSERT INTO Service ( 
                Name, Phone, Email, Color
                ) VALUES ( 
                @Name, @Phone, @Email, @Color
                );", args);

            Log.Info($"Neuer Service-Kollege erstellt:\r\n" +
                $"Name: {args["@Name"]}\r\n" +
                $"Telefon: {args["@Phone"]}\r\n" +
                $"E-Mail: {args["@Email"]}\r\n");
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

            _ = Sql.NonQueryAsync(
                @"Update Service SET 
                Name = @Name,
                Phone = @Phone,
                Email = @Email,
                Color = @Color  
                WHERE Id = @Id;", args);

            Log.Info($"Service-Kollege geändert:\r\n" +
                $"Name: {args["@Name"]}\r\n" +
                $"Telefon: {args["@Phone"]}\r\n" +
                $"E-Mail: {args["@Email"]}\r\n");
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

            if (uint.TryParse(form["Id"], out uint servieceId))
            {
                DataTable dt = SelectServiceById(servieceId);
                    if (dt?.Rows?.Count > 0)                
                        Log.Info($"Service-Kollege [{dt.Rows[0]["Id"]}] wird gelöscht:\r\n" +
                            $"Name: {dt.Rows[0]["Name"]}\r\n" +
                            $"Telefon: {dt.Rows[0]["Mobil"]}\r\n" +
                            $"E-Mail: {dt.Rows[0]["Email"]}\r\n" +
                            "(Löschen nicht möglich, wenn eingeteilt in Rufbereitschaft)");
            }

            // Keine Empfänger löschen, wenn sie noch in einer kommenden Bereitschaft eingeteilt sind.
            _ = Sql.NonQueryAsync(
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
