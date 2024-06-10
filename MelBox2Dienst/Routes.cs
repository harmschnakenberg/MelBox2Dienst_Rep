using Grapevine;
using Microsoft.SqlServer.Server;

//using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    [RestResource]
    internal partial class Routes
    {

        /// <summary>
        /// Tabelle Empfangene Nachrichten
        /// </summary>
        [RestRoute("Get", "/in")]
        public static async Task RecievedMessages(IHttpContext context)
        {
            DataTable dataTable;
            string html = string.Empty;

            #region Keine Übergabeparameter
            if (!context.Request.QueryString.HasKeys())
            {
                dataTable = Sql.SelectRecieved(100);

                html += Html.DatePicker("in", DateTime.Now) +
                        Html.ConvertDataTable(
                        dataTable,
                        new Dictionary<string, string>() {
                            { "Sperregel", "blocked" },
                            { "Nr", "in" }
                        }
                        );
            }
            #endregion
            else
            {
                #region Suche nach festem Datum
                if (DateTime.TryParse(context.Request.QueryString.Get("datum"), out DateTime date))
                {
                    if (date.CompareTo(DateTime.Now.AddYears(-10)) < 0) date = DateTime.Now.Date; //Älter als 10 Jahre = ungültig
                    if (date.CompareTo(DateTime.Now) > 0) date = DateTime.Now.Date; //Die Zukunft ist noch nicht bestimmt

                    dataTable = Sql.SelectRecieved(date);

                    html +=
                        Html.DatePicker("in", date) +
                        Html.ConvertDataTable(
                        dataTable,
                        new Dictionary<string, string>() {
                            { "Sperregel", "blocked" },
                            { "Nr", "in" }
                        }
                        );
                }
                #endregion
                #region Suche nach Id
                else if (uint.TryParse(context.Request.QueryString.Get("Nr"), out uint recId))
                {
                    html +=
                        Html.ConvertDataTable(Sql.SelectMessageByRecievedId(recId)) +
                        Html.BlockPolicySelection(Sql.SelectAllBlockPolicies(), Sql.SelectBlockPolicyIdFromRecievedId(recId), Sql.SelectMessageIdByRecievedId(recId));
                }
                #endregion
            }

            await context.Response.SendResponseAsync(Html.Sceleton(html)).ConfigureAwait(false);
        }

        /// <summary>
        /// Tabelle Gesendete Nachrichten
        /// </summary>
        [RestRoute("Get", "/out")]
        public static async Task SentMessages(IHttpContext context)
        {
            #region Filter setzen
            DateTime date = DateTime.Now.Date;
            DataTable dataTable;

            if (context.Request.QueryString.HasKeys())
            {
                DateTime.TryParse(context.Request.QueryString.Get("datum"), out date);
                if (date.CompareTo(DateTime.Now.AddYears(-10)) < 0) date = DateTime.Now.Date; //Älter als 10 Jahre = ungültig
                if (date.CompareTo(DateTime.Now) > 0) date = DateTime.Now.Date; //Die Zukunft ist noch nicht bestimmt

                dataTable = Sql.SelectSent(date);
            }
            else
                dataTable = Sql.SelectSent(100);
            #endregion
#if DEBUG
            Console.WriteLine(date);
#endif
            string html = Html.Sceleton(
                    Html.DatePicker("out", date) +
                    Html.ConvertDataTable(
                        dataTable
                    )
                );

            await context.Response.SendResponseAsync(html).ConfigureAwait(false);
        }

        [RestRoute("Get", "/log")]
        public static async Task Log(IHttpContext context)
        {
            #region Filter setzen
            DateTime date = DateTime.Now.Date;
            DataTable dataTable = new DataTable();

            if (!context.Request.QueryString.HasKeys())
                dataTable = Sql.SelectLog(100);
            else if (context.Request.QueryString.Get("inhalt")?.Length > 0)
                dataTable = Sql.SelectLog(context.Request.QueryString.Get("inhalt"));            
            else if (context.Request.QueryString.Get("datum").Length > 0 && DateTime.TryParse(context.Request.QueryString.Get("datum"), out date))
            {
                if (date.CompareTo(DateTime.Now.AddYears(-10)) < 0) date = DateTime.Now.Date; //Älter als 10 Jahre = ungültig
                if (date.CompareTo(DateTime.Now) > 0) date = DateTime.Now.Date; //Die Zukunft ist noch nicht bestimmt

                dataTable = Sql.SelectLog(date);
            }            
            #endregion
#if DEBUG
            Console.WriteLine(date);
#endif
            string html = Html.Sceleton(
                    Html.DatePicker("log", date) +
                    Html.ConvertDataTable(
                        dataTable
                    )
                );

            await context.Response.SendResponseAsync(html).ConfigureAwait(false);
        }


        [RestRoute("Get", "/guard")]
        public static async Task GuradOverview(IHttpContext context)
        {
            #region Filter setzen
            DateTime date = DateTime.Now.Date;
            DataTable dataTable = new DataTable();
            string html = string.Empty;

            if (!context.Request.QueryString.HasKeys())
            {
                dataTable = Sql.SelectAllGuards();
                html += Html.GuardCalender(dataTable);
            }
            else if (uint.TryParse(context.Request.QueryString.Get("id"), out uint shiftId))
            {
                dataTable = Sql.SelectGuardById(shiftId);
                html += Html.GuardFormUpdate(dataTable);
            }
            else if (context.Request.QueryString.Get("datum").Length > 0 && DateTime.TryParse(context.Request.QueryString.Get("datum"), out date))
            {
                Console.WriteLine("Neue Bereitchaft erstellen ab " + date);

                if (date.CompareTo(DateTime.Now.AddYears(-10)) < 0) date = DateTime.Now.Date; //Älter als 10 Jahre = ungültig
                if (date.CompareTo(DateTime.Now) < 0) date = DateTime.Now.Date; //Die Vergangenheit kann nicht geändert werden

                Console.WriteLine("Rufe auf: GuardFormNew()");
                html += Html.GuardFormNew(date, 1); //TODO: Übergabe der Id der Service-Person
            }

            #endregion
#if DEBUG
            Console.WriteLine(date);
#endif

            await context.Response.SendResponseAsync(Html.Sceleton(html)).ConfigureAwait(false);
        }

        /// <summary>
        /// Ändert die Stammdaten eines Kunden in der Datenbank von einem Formular
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Post", "/guard/update")]
        public async Task UpdateGuard(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.UpdateGuard(formContent);

            context.Request.QueryString.Add("id", formContent["Id"]);
            await GuradOverview(context);
        }

        [RestRoute("Post", "/guard/create")]
        public async Task CreateGuard(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.CreateGuard(formContent);

            await GuradOverview(context);
        }


        [RestRoute("Post", "/guard/delete")]
        public async Task DeleteGuard(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.DeleteGuard(formContent);

            await GuradOverview(context);
        }



        [RestRoute]
        public static async Task Home(IHttpContext context)
        {
            string html = Html.Sceleton("<h1>Upps.. den Pfad gibt es nicht..</h1>");

            await context.Response.SendResponseAsync(html).ConfigureAwait(false);
        }

    }
}
