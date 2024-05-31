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
        internal static DataTable SelectAllCustomers()
        {            
            return Sql.SelectDataTable(
                @"SELECT Id, 
                Name, 
                Phone AS Mobil, 
                Email,
                KeyWord AS Kennung,
                MaxInactiveHours AS Max_Inaktiv
                FROM Customer                         
                ORDER BY Name;", null);
        }

        internal static DataTable SelectCustomerById(uint customerId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Id", customerId }
            };

            return Sql.SelectDataTable(
                @"SELECT Id, 
                Name, 
                Phone AS Mobil, 
                Email,
                KeyWord AS Kennung,
                MaxInactiveHours AS Max_Inaktiv
                FROM Customer                         
                WHERE Id = @Id;", args);            
        }

        internal static DataTable SelectCustomerByName(string namepart)
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
                KeyWord AS Kennung,
                MaxInactiveHours AS Max_Inaktiv
                FROM Customer                         
                WHERE Name LIKE %@Name%;", args);
        }

        /// <summary>
        /// Ändert vorhandene Stammdaten eines Kunden
        /// </summary>
        /// <param name="form"></param>
        internal static void UpdateCustomer(Dictionary<string, string> form)
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
                { "@Name", form["Name"] },
                { "@Phone", form["Phone"] },
                { "@Email", form["Email"] },
                { "@KeyWord", form["KeyWord"] },
                { "@MaxInactiveHours", form["MaxInactiveHours"] }
            };

            _ = Sql.NonQuery(
                @"Update Customer SET 
                Name = @Name,
                Phone = @Phone,
                Email = @Email,
                KeyWord = @KeyWord,
                MaxInactiveHours = @MaxInactiveHours
                WHERE Id = @Id;", args);
        }

        /// <summary>
        /// Erzeugt neue Stammdaten eines Kunden
        /// </summary>
        /// <param name="form"></param>
        internal static void CreateCustomer(Dictionary<string, string> form)
        {
#if DEBUG
            foreach (var key in form.Keys)
            {
                Console.WriteLine(key + "=" + form[key]);
            }
#endif
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Name", form["Name"] },
                { "@Phone", form["Phone"] },
                { "@Email", form["Email"] },
                { "@KeyWord", form["KeyWord"] },
                { "@MaxInactiveHours", form["MaxInactiveHours"] }
            };

            _ = Sql.NonQuery(
                @"INSERT INTO Customer ( 
                Name, Phone, Email, Keyword, MaxInactiveHours
                ) VALUES ( 
                @Name, @Phone, @Email, @Keyword, @MaxInactiveHours 
                );", args);
        }

        /// <summary>
        /// Löscht vorhandene Stammdaten eines Kunden
        /// </summary>
        /// <param name="form"></param>
        internal static void DeleteCustomer(Dictionary<string, string> form)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Id", form["Id"] }
            };

            _ = Sql.NonQuery(
                @"DELETE FROM Customer WHERE Id = @Id;", args);
        }

    }
}
