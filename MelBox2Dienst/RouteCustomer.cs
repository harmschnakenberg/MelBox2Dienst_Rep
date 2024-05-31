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
        [RestRoute("Post", "/customer/delete")]
        public async Task DeleteCustomer(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            Sql.DeleteCustomer(formContent);
            await ListCustomers(context);
        }

    }
}
