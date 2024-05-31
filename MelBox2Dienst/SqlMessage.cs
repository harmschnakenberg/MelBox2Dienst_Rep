﻿using System;
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
        /// <summary>
        /// Listet alle Nachrichten auf, denen eine Sperregel zugewiesen wurde
        /// </summary>
        /// <returns></returns>
        internal static DataTable SelectAllBlockedMessages()
        {
            const string query1 = "SELECT Id, Content AS Inhalt, BlockPolicyId AS Sperregel FROM Message Where BlockPolicyId <> 0;"; //Alle Nachrichten, die eine Sperregel haben
            return Sql.SelectDataTable(query1, null);
        }

        /// <summary>
        /// Ruft eine bestimmte Sperregel ab
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        internal static DataTable SelectBlockPolicy(uint blockId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@BlockPolicyId", blockId }
            };

            return Sql.SelectDataTable(
                @"SELECT * FROM BlockPolicy WHERE Id = @BlockPolicyId;", args); //Sperregel
        }

        /// <summary>
        /// Ändert eine vorhandene Sperregel
        /// </summary>
        /// <param name="form"></param>
        internal static void UpdateBlockPolicy(Dictionary<string, string> form)
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
                { "@MonStart", form["MonStart"] },
                { "@MonEnd", form["MonEnd"] },
                { "@TueStart", form["TueStart"] },
                { "@TueEnd", form["TueEnd"] },
                { "@WenStart", form["WenStart"] },
                { "@WenEnd", form["WenEnd"] },
                { "@ThuStart", form["ThuStart"] },
                { "@ThuEnd", form["ThuEnd"] },
                { "@FriStart", form["FriStart"] },
                { "@FriEnd", form["FriEnd"] },
                { "@SatStart", form["SatStart"] },
                { "@SatEnd", form["SatEnd"] },
                { "@SunStart", form["SunStart"] },
                { "@SunEnd", form["SunEnd"] },
                { "@Comment", WebUtility.UrlDecode(form["Comment"]) },
            };

            _ = Sql.NonQuery(
                @"Update BlockPolicy SET 
                MonStart = @MonStart,
                MonEnd = @MonEnd,
                TueStart = @TueStart,
                TueEnd = @TueEnd,
                WenStart = @WenStart,
                WenEnd = @WenEnd,
                ThuStart = @ThuStart,
                ThuEnd = @ThuEnd,
                FriStart = @FriStart,
                FriEnd = @FriEnd,
                SatStart = @SatStart,
                SatEnd = @SatEnd,
                SunStart = @SunStart,
                SunEnd = @SunEnd,
                Comment = @Comment
                WHERE Id = @Id;", args);
        }

        /// <summary>
        /// Erstellt eine neue Sperregel 
        /// </summary>
        /// <param name="form"></param>
        internal static void InsertBlockPolicy(Dictionary<string, string> form)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@MonStart", form["MonStart"] },
                { "@MonEnd", form["MonEnd"] },
                { "@TueStart", form["TueStart"] },
                { "@TueEnd", form["TueEnd"] },
                { "@WenStart", form["WenStart"] },
                { "@WenEnd", form["WenEnd"] },
                { "@ThuStart", form["ThuStart"] },
                { "@ThuEnd", form["ThuEnd"] },
                { "@FriStart", form["FriStart"] },
                { "@FriEnd", form["FriEnd"] },
                { "@SatStart", form["SatStart"] },
                { "@SatEnd", form["SatEnd"] },
                { "@SunStart", form["SunStart"] },
                { "@SunEnd", form["SunEnd"] },
                { "@Comment", WebUtility.UrlDecode(form["Comment"]) },
            };

            _ = Sql.NonQuery(
                @"INSERT INTO BlockPolicy ( 
                MonStart,
                MonEnd,
                TueStart,
                TueEnd,
                WenStart,
                WenEnd,
                ThuStart,
                ThuEnd,
                FriStart,
                FriEnd,
                SatStart,
                SatEnd,
                SunStart,
                SunEnd,
                Comment 
                ) VALUES (
                @MonStart,
                @MonEnd,
                @TueStart,
                @TueEnd,
                @WenStart,
                @WenEnd,
                @ThuStart,
                @ThuEnd,
                @FriStart,
                @FriEnd,
                @SatStart,
                @SatEnd,
                @SunStart,
                @SunEnd,
                @Comment
                );", args);
        }


        /// <summary>
        /// Listet alle vorhandenen Sperregeln tabellarisch auf.
        /// </summary>
        /// <returns></returns>
        internal static DataTable SelectAllBlockPolicies()
        {      
            //Darstellung als Balken??:
            //24 Std = 100% | 1 Std = 100/24 = 4.17%
            return Sql.SelectDataTable(
                @"SELECT 
                Id AS Sperregel,
                MonStart,
                MonEnd,
                TueStart,
                TueEnd,
                WenStart,
                WenEnd,
                ThuStart,
                ThuEnd,
                FriStart,
                FriEnd,
                SatStart,
                SatEnd, SunStart,SunEnd,
                Comment AS Kommentar
                FROM BlockPolicy;", null); //Sperregel
        }

        /// <summary>
        /// Ruft eine Nachricht anhand einer Empfangs-Id  ab
        /// </summary>
        /// <param name="recievedId">Empfangs-Id aus Tabelle 'Recieved'</param>
        /// <returns></returns>
        internal static DataTable SelectMessageByRecievedId(uint recievedId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@RecId", recievedId }
            };

            return Sql.SelectDataTable(
                @"SELECT Id, Content AS Inhalt, BlockPolicyId AS Sperregel 
                FROM Message WHERE Id = (SELECT ContentId FROM Recieved WHERE Id = @RecId);", args); 
        }

        internal static uint SelectMessageIdByRecievedId(uint recievedId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@RecId", recievedId }
            };

            var messageId = Sql.SelectValue(@"SELECT Id FROM Message WHERE Id = (SELECT ContentId FROM Recieved WHERE Id = @RecId);", args);

            return uint.Parse(messageId.ToString());
        }

        /// <summary>
        /// Ändert eine vorhandene Sperregel
        /// </summary>
        /// <param name="form"></param>
        internal static void UpdateMessagePolicy(Dictionary<string, string> form)
        {
#if DEBUG
            foreach (var key in form.Keys)
            {
                Console.WriteLine(key + "=" + form[key]);
            }
#endif
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@MessageId", form["MessageId"] },
                { "@BlockPolicyId", form["PolicyId"] }
            };

            _ = Sql.NonQuery(
                @"Update Message SET 
                BlockPolicyId = @BlockPolicyId
                WHERE Id = @MessageId;", args);
        }


        /// <summary>
        /// Ermittelt die Sperregel einer Empfangenen Nachricht anhand der Id
        /// </summary>
        /// <param name="recievedId"></param>
        /// <returns></returns>
        internal static uint SelectBlockPolicyFromRecievedId(uint recievedId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@RecId", recievedId }
            };

            var blockPolicyIdObj = Sql.SelectValue(
                @"SELECT BlockPolicyId FROM Message WHERE Id = (SELECT ContentId FROM Recieved WHERE Id = @RecId);", args);

            _ = uint.TryParse(blockPolicyIdObj.ToString(), out uint blockPolicyId);

            return blockPolicyId;
        }
       
    }
}
