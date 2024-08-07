﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    internal static partial class Sql
    {
        // private static Queue<KeyValuePair<string, Dictionary<string, object>>> queryList = new Queue<KeyValuePair<string, Dictionary<string, object>>>();

        // A queue that is protected by Monitor.
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

    }
}
