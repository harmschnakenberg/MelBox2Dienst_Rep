using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    internal static partial class Sql
    {
        /// <summary>
        /// Externer Ordner, in den Backups der Datenbank abgelegt werden.
        /// </summary>
        public static string BackupDbDirectoryExtern { get; set; } = @"\\192.168.160.8\user\_Leitsystem\Diskette\Melbox2 DBSicherung";

        // private static Queue<KeyValuePair<string, Dictionary<string, object>>> queryList = new Queue<KeyValuePair<string, Dictionary<string, object>>>();

        /// <summary>
        /// A queue that is protected by Monitor.
        /// </summary>
        private static bool lockQuery = false;

        /// <summary>
        /// Führt einen SQL-Befehl gegen die Datenbank aus.
        /// </summary>
        /// <param name="query">SQL-Abfrage</param>
        /// <param name="args">Parameter für SQL-Abfrage</param>
        /// <returns>true = mindestens eine Zeile in der Datenbank wurde eingefügt, geändert oder gelöscht.</returns>
        internal static bool NonQueryAsync(string query, Dictionary<string, object> args)
        {

            //if (!CheckDbFile()) return false;
        

            while(lockQuery)
            {
                Thread.Sleep(100);
            }

            lockQuery = true;

            try
            {
                using (var connection = new SQLiteConnection("Data Source=" + DbPath))
                {                    
                    //SQLitePCL.Batteries.Init();
                    connection.Open();                   
                    var command = connection.CreateCommand();
                    command.CommandText = query;
                    if (args != null && args.Count > 0)
                    {
                        foreach (string key in args.Keys)
                        {
                            command.Parameters.AddWithValue(key, args[key]);
                        }
                    }

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                string argsShow = "";
                if (args != null)
                    foreach (string key in args.Keys)
                    {
                        argsShow += "\r\n" + key + "\t'" + args[key] + "'";
                    }

                Log.Error("SqlNonQuery(): " + query + argsShow + "\r\n" + ex.GetType() + "\r\n" + ex.Message + "\r\n" + ex.InnerException + "\r\n");
                return false;
            }
            finally
            {
                // Ensure that the lock is released.
                lockQuery = false;
            }
        }

        /// <summary>
        /// Fragt Tabellen-Daten mit einem SQL-Befehl gegen die Datenbank ab.
        /// </summary>
        /// <param name="query">SQL-Abfrage</param>
        /// <param name="args">Parameter für SQL-Abfrage</param>
        /// <returns>Tabelle mit dem Ergebnis der Abfrage.</returns>
        internal static DataTable SelectDataTable(string query, Dictionary<string, object> args)
        {
            DataTable myTable = new DataTable();

            if (!CheckDbFile()) return myTable;

            try
            {
                using (var connection = new SQLiteConnection("Data Source=" + DbPath))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = query;


                    if (args != null && args.Count > 0)
                    {
                        foreach (string key in args.Keys)
                        {
                            command.Parameters.AddWithValue(key, args[key]);
                        }
                    }

                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            //Mit Schema einlesen
                            myTable.Load(reader);
                        }

                        return myTable;
                    }
                    catch
                    {
#if DEBUG
                    Log.Info("SelectDataTable(): Hinweis: Abfrage hat Schema nicht eingehalten:\r\n" + query); //Debug-Info
#endif
                        myTable = new DataTable();

                        //Wenn Schema aus DB nicht eingehalten wird (z.B. UNIQUE Constrain in SELECT Abfragen); dann neue DataTable, alle Spalten <string>
                        using (var reader = command.ExecuteReader())
                        {
                            //if (reader.FieldCount == 0)
                            //    return myTable;

                            //zu Fuß einlesen
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                //Spalten einrichten
                                myTable.Columns.Add(reader.GetName(i), typeof(string));
                            }

                            while (reader.Read())
                            {
                                List<object> row = new List<object>();

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                   // string colType = myTable.Columns[i].DataType.Name;

                                    if (reader.IsDBNull(i))
                                    {
                                        row.Add(string.Empty);
                                    }
                                    else
                                    {                                 
                                        string r = reader.GetFieldValue<object>(i).ToString();
                                        row.Add(r);
                                    }
                                }

                                myTable.Rows.Add(row.ToArray());
                            }
                        }

                        return myTable;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("SqlSelectDataTable(): " + query + "\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }

            return myTable;
        }

        /// <summary>
        /// Fragt einen Einzelwert mit einem SQL-Befehl gegen die Datenbank ab.
        /// </summary>
        /// <param name="query">SQL-Abfrage</param>
        /// <param name="args">Parameter für SQL-Abfrage</param>
        /// <returns>Ergebniswert der Abfrage.</returns>
        internal static object SelectValue(string query, Dictionary<string, object> args)
        {
            try
            {
                using (var connection = new SQLiteConnection("Data Source=" + DbPath))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    if (args != null && args.Count > 0)
                    {
                        foreach (string key in args.Keys)
                        {
                            command.Parameters.AddWithValue(key, args[key]);
                        }
                    }

                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Log.Error("SqlSelectValue(): " + query + "\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gibt die erste Zeile der Abfrage als Dictionary (colName, Wert) aus.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns>Dictionary (colName, Wert)</returns>
        internal static Dictionary<string, object> SelectFirstRow(string query, Dictionary<string, object> args)
        {
            DataTable myTable = new DataTable();
            Dictionary<string, object> dict = new Dictionary<string, object>();

            try
            {
                using (var connection = new SQLiteConnection("Data Source=" + DbPath))
                {

                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    if (args != null && args.Count > 0)
                    {
                        foreach (string key in args.Keys)
                        {
                            command.Parameters.AddWithValue(key, args[key]);
                        }
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        myTable.Load(reader);
                    }

                    foreach (DataColumn col in myTable.Columns)                    
                        dict.Add(col.ColumnName, myTable.Rows[0][col]);                    
                }
            }
            catch (Exception ex)
            {
                Log.Error("SqlSelectValue(): " + query + "\r\n" + ex.GetType() + "\r\n" + ex.Message);
            }

            return dict;
        }

        /// <summary>
        /// Erzeugt wöchentlich ein Backup der kompletten Datenbank. Backup wird im Datenbank-Ordner abgelegt. 
        /// </summary>
        internal static void DbBackup()
        {
            try
            {
                string backupFileName = string.Format("MelBox2_{0}_KW{1:00}.db", DateTime.UtcNow.Year, GetIso8601WeekOfYear(DateTime.UtcNow));
                string backupPath = Path.Combine(Path.GetDirectoryName(DbPath), backupFileName);
                if (File.Exists(backupPath) || !File.Exists(DbPath)) return;

                using (var connection = new SQLiteConnection("Data Source=" + DbPath))
                {
                    connection.Open();

                    // Create a full backup of the database
                    // Quelle: https://stackoverflow.com/questions/34760033/how-to-take-sqlite-database-backup-using-c-sharp-or-sqlite-query
                    //var backup = new SQLiteConnection("Data Source=" + backupPath);
                    //using (var location = new SQLiteConnection(@"Data Source=C:\activeDb.db; Version=3;"))
                    //using (var destination = new SQLiteConnection(string.Format(@"Data Source={0}:\backupDb.db; Version=3;", strDestination)))
                    //{
                    //    connection.Open();
                    //    destination.Open();
                    //    connection.BackupDatabase(destination, "main", "main", -1, null, 0);
                    //}

                    SQLiteCommand sqlCmd = connection.CreateCommand();
                    sqlCmd.CommandText = $"VACUUM INTO '{backupPath}'";
                    sqlCmd.ExecuteNonQuery();
                }

                #region Backup-Datenbank an einen sicheren Ort kopieren
                try
                {
                    if (Directory.Exists(BackupDbDirectoryExtern) && File.Exists(backupPath))
                        File.Copy(backupPath, Path.Combine(BackupDbDirectoryExtern, backupFileName));
                }
                catch (Exception ex)
                {
                    Log.Error($"Das Backup der Datenbank konnte nicht in den Sicherungsordner '{BackupDbDirectoryExtern}' kopiert werden " + ex);
                }
                #endregion

                Log.Info("Backup der Datenbank erstellt unter " + backupPath);
            }
            catch (Exception ex)
            {
                Log.Error("Sql - Fehler DbBackup() \r\n" + ex.Message + Environment.NewLine + ex.InnerException + Environment.NewLine + ex.GetBaseException().Message);
#if DEBUG
                throw new Exception("Sql-Fehler DbBackup()\r\n" + ex.Message);
#endif
            }
        }

        /// <summary>
        /// Gibt die Kalenderwoche des übergebenen Datums aus. 
        /// </summary>
        /// <param name="time">Datum für das die Kalenderwoche bestimmt werden soll</param>
        /// <returns>Kalenderwoche</returns>
        private static int GetIso8601WeekOfYear(DateTime time)
        {
            // This presumes that weeks start with Monday.
            // Week 1 is the 1st week of the year with a Thursday in it.
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

    }
}
