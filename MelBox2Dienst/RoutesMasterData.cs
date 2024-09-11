using Grapevine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management.Instrumentation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2Dienst
{
    [RestResource]
    internal partial class Routes
    {
        #region Kundenstammdaten

        /// <summary>
        /// Zeit Kundenstammdaten an
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Get", "/customer")]
        public static async Task ListCustomers(IHttpContext context)
        {
            #region Filter setzen
            DataTable dataTable = new DataTable();
            string html;

            if (context.Request.QueryString.HasKeys())
            {
                string namepart = context.Request.QueryString.Get("Name") ?? string.Empty;

                if (uint.TryParse(context.Request.QueryString.Get("Id"), out uint customerId))
                    dataTable = Sql.SelectCustomerById(customerId);
                else if (namepart.Length > 0)
                    dataTable = Sql.SelectCustomerByName(namepart);

                html = Html.CustomerForm(dataTable);
            }
            else
            {
                dataTable = Sql.SelectAllCustomers();

                #endregion

                html = Html.ConvertDataTable(
                    dataTable,
                    new Dictionary<string, string>() {
                        { "Id", "customer" }
                    }
                );
            }
            await context.Response.SendResponseAsync(Html.Sceleton(html)).ConfigureAwait(false);
        }

        /// <summary>
        /// Erzeugt neue Stammdaten zu einem Kunden von einem Formular
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Post", "/customer/create")]
        public async Task CreateCustomer(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];


            Sql.CreateCustomer(formContent);

            await ListCustomers(context);
        }

        /// <summary>
        /// Ändert die Stammdaten eines Kunden in der Datenbank von einem Formular
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Post", "/customer/update")]
        public async Task UpdateCustomer(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.UpdateCustomer(formContent);

            context.Request.QueryString.Add("Id", formContent["Id"]);
            await ListCustomers(context);
        }

        /// <summary>
        /// Ändert die Stammdaten eines Kunden in der Datenbank von einem Formular
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Post", "/customer/delete")]
        public async Task DeleteCustomer(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.DeleteCustomer(formContent);
            await ListCustomers(context);
        }

        #endregion

        #region Mitarbeiterstammdaten


        [RestRoute("Post", "/login")]
        public static async Task Login(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];

            int i = 0;
            foreach (var item in formContent)
            {
                Console.WriteLine($"{++i} {item.Key} = {item.Value}");
            }

            string username = WebUtility.UrlDecode(formContent["uname"]).Replace("<", "&lt;").Replace(">", "&gt;"); //HTML unschädlich machen
            string password = WebUtility.UrlDecode(formContent["pswd"]).Replace("<", "&lt;").Replace(">", "&gt;");
            Console.WriteLine($"TEST: Login '{username}' '{password}'");

            KeyValuePair<string, Service> ident = Sql.CheckCredentials(username, password);

            if (ident.Value.Name is null) //Login nicht erfolgreich
            {
                string html = "<div class='alert alert-danger'\"'>\r\n" +
                    $"<strong>Login fehlgeschlagen</strong> Benutzer '{username}' konnte nicht angemeldet werden. Bitte &uuml;berpr&uuml;fe Benutzernamen und Passwort!\r\n" +
                    "</div>";

                await context.Response.SendResponseAsync(Html.Sceleton(html)).ConfigureAwait(false);
            }
            else
            {
                //Login erfolgreich
                Cookie cookie = new Cookie("auth", ident.Key);
                context.Response.Cookies.Add(cookie);

                //Benutzer merken
                Server.LogedInUser.Add(ident.Key, ident.Value);

                string html = "<div class='alert alert-success'\"'>\r\n" +
                             $"<strong>Login</strong> Benutzer [{ident.Value.Id}] {ident.Value.Name} erfolgreich angemeldet.\r\n" +
                              "</div>";

                //StringBuilder sb = new StringBuilder();

                //sb.AppendLine("<script>");
                //sb.AppendLine("function webstorage() {");
                //sb.AppendLine("  if (typeof (Storage) !== 'undefined') {");
                //sb.AppendLine("  localStorage.setItem('user','" + ident.Value.Name + "');");
                //sb.AppendLine("  localStorage.setItem('ident','" + ident.Key + "');");
                //sb.AppendLine("  window.location.replace('/');");
                //sb.AppendLine("  }");
                //sb.AppendLine("}");
                //sb.AppendLine(" webstorage();");
                //sb.AppendLine("</script>");

                //html += sb.ToString();

                await context.Response.SendResponseAsync(Html.Sceleton(html)).ConfigureAwait(false);
            }
        }

        //[RestRoute("Post", "/register")]
        //public static async Task Register(IHttpContext context)
        //{
        //    Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
        //    string username = WebUtility.UrlDecode(formContent["uname"]).Replace("<", "&lt;").Replace(">", "&gt;"); //HTML unschädlich machen
        //    string password = WebUtility.UrlDecode(formContent["pswd"]).Replace("<", "&lt;").Replace(">", "&gt;");
        //    KeyValuePair<string, Service> ident = Sql.CheckCredentials(username, password);

        //    if (ident.Value.Id > 0) //Registrierung nicht erfolgreich, Benutzer gibt es schon
        //    {
        //        string html = "<div class='alert alert-danger'\"'>\r\n" +
        //           $"<strong>Registrierung fehlgeschlagen</strong> Den Benutzer {ident.Value.Name} gibt es bereits.\r\n" +
        //           "</div>";

        //        await context.Response.SendResponseAsync(Html.Sceleton(html)).ConfigureAwait(false);
        //    }
        //    else //Registrierung erfolgt
        //    {
        //        Sql.Register(username, password);

        //        string html = "<div class='alert alert-success'\"'>\r\n" +
        //            $"<strong>Registrierung</strong> Der Benutzer {username} wird registriert. Die Registrierung muss von einem Administrator freigeschaltet werden.\r\n" +
        //            "</div>";

        //        await context.Response.SendResponseAsync(Html.Sceleton(html));
        //    }
        //}



        /// <summary>
        /// Zeit Servicestammdaten an
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Get", "/service")]
        public static async Task ListService(IHttpContext context)
        {
            #region Filter setzen
            DataTable dataTable = new DataTable();
            string html;

            if (context.Request.QueryString.HasKeys())
            {
                string namepart = context.Request.QueryString.Get("Name") ?? string.Empty;

                if (uint.TryParse(context.Request.QueryString.Get("Id"), out uint customerId))
                    dataTable = Sql.SelectServiceById(customerId);
                else if (namepart.Length > 0)
                    dataTable = Sql.SelectServiceByName(namepart);

                html = Html.ServiceForm(dataTable);
            }
            else
            {
                dataTable = Sql.SelectAllService();

                #endregion

                html = Html.ConvertDataTable(
                    dataTable,
                    new Dictionary<string, string>() {
                        { "Id", "service" }
                    }
                );
            }
            await context.Response.SendResponseAsync(Html.Sceleton(html)).ConfigureAwait(false);
        }

        /// <summary>
        /// Erzeugt neue Stammdaten zu einem Kunden von einem Formular
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Post", "/service/create")]
        public async Task CreateService(IHttpContext context)
        {
            Service logedInUser = HttpHelper.GetLogedInUser(context);

            if (logedInUser != null)
            {
                Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
                Sql.CreateService(formContent);
                MelBox2Dienst.Log.Info($"Rufbereitschaft erstellt von {logedInUser.Name}");
            }

            await ListService(context);
        }

        /// <summary>
        /// Ändert die Stammdaten eines Kunden in der Datenbank von einem Formular
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Post", "/service/update")]
        public async Task UpdateService(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.UpdateService(formContent);

            context.Request.QueryString.Add("Id", formContent["Id"]);
            await ListService(context);
        }

        /// <summary>
        /// Ändert die Stammdaten eines Kunden in der Datenbank von einem Formular
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute("Post", "/service/delete")]
        public async Task DeleteService(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.DeleteService(formContent);
            await ListService(context);
        }



        #endregion

    }
}
