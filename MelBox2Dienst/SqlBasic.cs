using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    internal static partial class Sql
    {
        /// <summary>
        /// Führt einen SQL-Befehl gegen die Datenbank aus.
        /// </summary>
        /// <param name="query">SQL-Abfrage</param>
        /// <param name="args">Parameter für SQL-Abfrage</param>
        /// <returns>true = mindestens eine Zeile in der Datenbank wurde eingefügt, geändert oder gelöscht.</returns>
        internal static bool NonQuery(string query, Dictionary<string, object> args)
        {

            //if (!CheckDbFile()) return false;

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
                    Log.Info("SelectDataTable(): Hinweis: Abfrage hat Schema nicht eingehalten."); //Debug-Info
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
                                    string colType = myTable.Columns[i].DataType.Name;

                                    if (reader.IsDBNull(i))
                                    {
                                        row.Add(string.Empty);
                                    }
                                    else
                                    {
                                        string r = reader.GetFieldValue<string>(i);
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

    }
}
