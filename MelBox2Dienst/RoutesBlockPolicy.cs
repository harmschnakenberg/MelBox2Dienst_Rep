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
                    "<h4>Gesperrte Nachrichten</h4>" +
                    Html.ConvertDataTable(
                    Sql.SelectAllBlockedMessages(),
                    new Dictionary<string, string>() { { "Nr", "blocked" } }

                ) + "<hr><h4>Sperregeln</h4>" +
                Html.ConvertDataTable(
                    Sql.SelectAllBlockPolicies(),
                    new Dictionary<string, string>() { { "Sperregel", "blocked" } }
                );
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
            else if (uint.TryParse(context.Request.QueryString.Get("Nr"), out uint messageId))
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

            //context.Request.QueryString.Add("Sperregel", formContent["Id"]);
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
