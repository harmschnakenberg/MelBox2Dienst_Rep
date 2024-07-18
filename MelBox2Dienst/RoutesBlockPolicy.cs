using Grapevine;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    [RestResource]
    internal partial class Routes
    {
        #region Sperregeln bearbeiten
        /// <summary>
        /// Tabelle aller Nachrichten, denen eine Sperregel zugteilt wurde
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Get", "/blocked")]
        public async Task BlockedMessages(IHttpContext context)
        {
            string html = string.Empty;

            #region Keine Übergabeparameter
            if (!context.Request.QueryString.HasKeys())
            {
                html +=
                    "<h4>Gesperrte Meldungen</h4>" +
                    "<div>Diese Meldungen werden durch eine Sperregel zu bestimmten Zeiten nicht an die Bereitschaft weitergeleitet:</div>" +
                    Html.ConvertDataTable(
                    Sql.SelectAllBlockedMessages(),
                    new Dictionary<string, string>() { { "Id", "blocked" } }

                ) + "<hr><h4>Sperregeln</h4>" +
                "<div>Dies sind die verfügbaren Sperregeln. In den markierten Zeitr&auml;umen werden eingehende Meldungen nicht an die Bereitschaft weitergeleitet:</div>" +
                Html.BlockPolicySelection(Sql.SelectAllBlockPolicies(), 0,0 );
            }
            #endregion
            #region Suche nach Sperregel
            else if (uint.TryParse(context.Request.QueryString.Get("Sperregel"), out uint blockPolicyId))
            {
                html +=
                    "<div class='container p-1'>" +
                    $"<h3>Sperregel <span class='badge bg-danger'>{blockPolicyId}</span></h3>" +

                    Html.BlockPolicy(Sql.SelectBlockPolicy(blockPolicyId)) +

                    "</div>";
            }
            #endregion
            #region Suche nach Nachricht-Id
            else if (uint.TryParse(context.Request.QueryString.Get("Id"), out uint messageId))
            {
                DataTable dt = Sql.SelectMessageByMessagedId(messageId);
                _= uint.TryParse(dt.Rows[0]["Sperregel"]?.ToString(), out uint messageBlockPolicyId);

                html +=
                       Html.ConvertDataTable(dt) +
                       Html.BlockPolicySelection(
                           Sql.SelectAllBlockPolicies(), 
                           messageBlockPolicyId, 
                           messageId
                           );
            }
            #endregion

            await context.Response.SendResponseAsync(Html.Sceleton(html));
        }

        /// <summary>
        /// Ändert eine Sperregel in der Datenbank von eienm Formular
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Post", "/blocked/update")]
        public async Task BlockPolicyUpdate(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.UpdateBlockPolicy(formContent);
            //MelBox2Dienst.Log.Info($"Meldung [{formContent["MessageId"]}]: Sperregel auf [{formContent["PolicyId"]}] geändert."); //Sinnvoll ohne Meldungs-Klartext

            context.Request.QueryString.Add("Sperregel", formContent["Id"]);
            await BlockedMessages(context);
        }

        /// <summary>
        /// Ändert eine Sperregel in der Datenbank von eienm Formular
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Post", "/blocked/create")]
        public async Task BlockPolicyCreate(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.InsertBlockPolicy(formContent);
           
            await BlockedMessages(context);
        }

        #endregion

        #region Nachrichten bearbeiten
        /// <summary>
        /// Ändert eine Sperregel in der Datenbank von einem Formular
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Post", "/message/update")]
        public async Task MessageUpdate(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.UpdateMessagePolicy(formContent);

            context.Request.QueryString.Add("Sperregel", formContent["PolicyId"]);
            await BlockedMessages(context);
        }

        #endregion
    }
}
