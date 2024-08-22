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
        /// Ermittel die Id des Absender einer SMS oder erstellt sie.
        /// </summary>
        /// <param name="sms">Empfangene SMS</param>
        /// <returns>Id die Absenders ind er Datenbank</returns>
        internal static uint GetCustomerId(Sms sms)
        {
            //Erst nach Keyword suchen, da Phone nicht eindeutig sein kann. (siehe MelBox1)
            /*
            If psAbsKey <> "" Then
            sqlCommand.CommandText = "SELECT AbsID, AbsName FROM " & vSMSEing.psDBAbsTab & " WHERE AbsKey = '" & psAbsKey & "'"
            Else
            sqlCommand.CommandText = "SELECT AbsID, AbsName FROM " & vSMSEing.psDBAbsTab & " WHERE AbsNr = '" & vsFormatAbsNr(psAbsNr) & "'"
            End If
            */
            const string query1 = "SELECT Id FROM Customer WHERE LOWER(KeyWord) = @KeyWord UNION ALL SELECT Id FROM Customer WHERE Phone =@Phone LIMIT 1;";
            const string query2 = "INSERT INTO Customer (Name, Phone, KeyWord) VALUES ('Neu_' || @KeyWord || @Phone, @Phone, @KeyWord); ";

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Phone", sms.Phone },
                { "@KeyWord", GetKeyWord(sms.Content) }
            };

            _ = uint.TryParse(Sql.SelectValue(query1, args)?.ToString(), out uint customerId);

            if (customerId == 0) // Nicht gefunden
            {
                _ = Sql.NonQueryAsync(query2, args);
                _ = uint.TryParse(Sql.SelectValue(query1, args)?.ToString(), out customerId);
            }

            return customerId;
        }

        internal static uint GetCustomerId(Email email)
        {
            //Erst nach Keyword suchen, da Phone nicht eindeutig sein kann.
            const string query1 = "SELECT Id FROM Customer WHERE Email = @Email; ";
            const string query2 = "INSERT INTO Customer (Name, Email) VALUES (@Email, @Email); ";

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@Email", email.From }
            };

            _ = uint.TryParse(Sql.SelectValue(query1, args)?.ToString(), out uint customerId);

            if (customerId == 0) // Nicht gefunden
            {
                Sql.NonQueryAsync(query2, args);
                _ = uint.TryParse(Sql.SelectValue(query1, args)?.ToString(), out customerId);
            }

            return customerId;
        }


/// <summary>
/// Aus altem MelBox:
///   AbsKey: in AbsKey kann ein Schlüsselwort eingetragen werden, um Absender die
///   keine Nummern in gesendeten SMS'en übermitteln (z.B. D2) zu unterscheiden.
///   Wird in den ersten 10 Zeichen ein gleichnamiges Wort VOR EINEM KOMMA gefunden
///   wird die SMS dem Absender AbsID zugeordnet.
/// </summary>
/// <param name="message">Inhalt einer Nachricht</param>
/// <returns>Keyword in Kleinschreibung</returns>
private static string GetKeyWord(string message)
{
// char[] split = new char[] { ' ', ',', '-', '.', ':', ';' };
char[] split = new char[] { ',' };
string[] words = message.Split(split);
string keyWords;

if (words.Length > 0)
    keyWords = words[0].Trim().Substring(0, Math.Min(10, words[0].Length));
else
    keyWords = message.Substring(0, 10);

//if (words.Length > 1)
//{
//    KeyWords += words[1].Trim();
//}

return keyWords.ToLower();
}


internal static DataTable SelectWatchedCustomers()
{
return Sql.SelectDataTable(@"SELECT * FROM View_WatchedSenders;", null);
}

internal static DataTable SelectOverdueCustomers()
{
return Sql.SelectDataTable(@"SELECT * FROM View_Overdue;", null);
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
    { "@Name", WebUtility.UrlDecode(form["Name"]) },
    { "@Phone", '+' + WebUtility.UrlDecode(form["Phone"].TrimStart('+').Replace(" ", "")) },
    { "@Email", WebUtility.UrlDecode(form["Email"]) },
    { "@KeyWord", WebUtility.UrlDecode(form["KeyWord"]) },
    { "@MaxInactiveHours", form["MaxInactiveHours"] }
};

_ = Sql.NonQueryAsync(
    @"INSERT INTO Customer ( 
    Name, Phone, Email, Keyword, MaxInactiveHours
    ) VALUES ( 
    @Name, @Phone, @Email, @Keyword, @MaxInactiveHours 
    );", args);

Log.Info($"Neuer Kunde erstellt:\r\n" +
    $"Name: {args["@Name"]}\r\n" +
    $"Telefon: {args["@Phone"]}\r\n" +
    $"E-Mail: {args["@Email"]}\r\n");
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
    { "@Name", WebUtility.UrlDecode(form["Name"]) },
    { "@Phone",'+' + WebUtility.UrlDecode(form["Phone"].TrimStart('+').Replace(" ", "")) },
    { "@Email", WebUtility.UrlDecode(form["Email"]) },
    { "@KeyWord", WebUtility.UrlDecode(form["KeyWord"]) },
    { "@MaxInactiveHours", form["MaxInactiveHours"] }
};

_ = Sql.NonQueryAsync(
    @"Update Customer SET 
    Name = @Name,
    Phone = @Phone,
    Email = @Email,
    KeyWord = @KeyWord,
    MaxInactiveHours = @MaxInactiveHours
    WHERE Id = @Id;", args);

Log.Info($"Kunde geändert:\r\n" +
    $"Name: {args["@Name"]}\r\n" +
    $"Telefon: {args["@Phone"]}\r\n" +
    $"E-Mail: {args["@Email"]}\r\n" +
    $"Max. Inaktivität: {args["@MaxInactiveHours"]} Std.\r\n");
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

var customerName = Sql.SelectValue(@"SELECT Name FROM Customer WHERE Id = @Id;", args);
Log.Info($"Kunde [{form["Id"]}] '{customerName}' wurde gelöscht.");

_ = Sql.NonQueryAsync(
    @"DELETE FROM Customer WHERE Id = @Id;", args);            
}

}
}
