using Grapevine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];

            Sql.CreateService(formContent);

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
