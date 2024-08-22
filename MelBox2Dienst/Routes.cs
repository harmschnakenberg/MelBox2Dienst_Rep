using Grapevine;
using Microsoft.SqlServer.Server;

//using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
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
                dataTable = Sql.SelectRecieved(DateTime.Now);

                if (dataTable.Rows.Count == 0) // Wenn heute nichts empfangen wurde
                    dataTable = Sql.SelectRecieved(100);

                html += "<h4>Empfangene Meldungen</h4>" + 
                        Html.DatePicker("in", DateTime.Now) +
                        Html.ConvertDataTable(
                        dataTable,
                        new Dictionary<string, string>() {
                          // { "Sperregel", "blocked" },
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
                        new Dictionary<string, string>() {{ "Nr", "in" }} );
                }
                #endregion
                #region Suche nach Id
                else if (uint.TryParse(context.Request.QueryString.Get("Nr"), out uint recId))
                {
                    html +=
                        Html.ConvertDataTable(Sql.SelectMessageByRecievedId(recId)) +
                        "<h3>Sperregeln</h3>" +
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
//#if DEBUG
//            Console.WriteLine(date);
//#endif
            string html = Html.Sceleton(
                    "<h4>Weitergeleitete Meldungen</h4>" +
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
//#if DEBUG
//            Console.WriteLine(date);
//#endif
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
            string html = string.Empty;

            #region Filter setzen            
            DataTable dataTable;// = new DataTable();
            if (!context.Request.QueryString.HasKeys())
            {
                html += "<h4>Rufbereitschaft</h4>" +
                        $"<div class='container'>Rufannahme geht zurzeit an <span class='badge bg-secondary'>{Sql.CallRelayPhone}</span></div>" +
                        $"<div class='container'>{(Sql.IsBuisinesTimeNow() ? "keine SMS-Weiterleitung während der Geschäftszeiten" : "SMS-Weiterleizung an die Rufbereitschaft aktiv")}</div>";

                dataTable = Sql.SelectAllGuards();
                html += Html.GuardCalender(dataTable);
            }
            else if (uint.TryParse(context.Request.QueryString.Get("id"), out uint shiftId))
            {
                dataTable = Sql.SelectGuardById(shiftId);
                html += Html.GuardFormUpdate(dataTable);
            }
            else if (context.Request.QueryString.Get("datum").Length > 0 && DateTime.TryParse(context.Request.QueryString.Get("datum"), out DateTime date))
            {                
                Console.WriteLine("Neue Bereitschaft erstellen ab " + date);

                if (date.CompareTo(DateTime.Now.AddYears(-10)) < 0) date = DateTime.Now.Date; //Älter als 10 Jahre = ungültig
                if (date.CompareTo(DateTime.Now) < 0) date = DateTime.Now.Date; //Die Vergangenheit kann nicht geändert werden

                Console.WriteLine("Rufe auf: GuardFormNew()");
                html += Html.GuardFormNew(date, 1); //TODO: Übergabe der Id der Service-Person
            }

            #endregion
            //#if DEBUG
            //            Console.WriteLine(date);
            //#endif

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


        [RestRoute("Get", "/gsm")]
        public static async Task GsmRoute(IHttpContext context)
        {        
            string html = "<h4>Status GSM-Modem</h4>" +
                "<div>Statusmeldungen von GSM-Modem</div>" +

            #region Tabelle aktuelle Eigenschaften GSM-Modem
                "<table class='table' style='width:100%'>" +
                "<tr><th>Eigenschaft</th><th>Zeitpunkt</th><th>Wert</th></tr>";
            foreach (var key in Pipe1.GsmStatus.Keys)
            {
                html += $"<tr><td>{key}</td><td>{Pipe1.GsmStatus[key].Item1}</td><td>{Pipe1.GsmStatus[key].Item2}</td></tr>";
            }

            html += "</table>";

            #endregion

            //html += @"<a href='gsm/reinit' class='btn btn-outline-secondary'>GSM-Modem reinitialisieren</a>";
            #region Test-SMS versenden
            html += @"<form action='gsm/testsms' method='post'><div class='input-group mb-3'>                           
                     
                      <span class='input-group-text'>Test-SMS versenden</span>
                      <input type='text' class='form-control' name='message' placeholder='Text der SMS' value='Test-SMS von MelBox2' required>
                      <input type='tel' class='form-control' name='phone' placeholder='+491601234567 (Mobilnummer)' pattern='\+[0-9]{10,}' required>
                      <button class='btn btn-outline-primary' type='submit'>SMS Senden</button>
                    </div></form>
                    <a href='gsm/reinit' class='btn btn-outline-secondary'>GSM-Modem reinitialisieren</a>";

            #endregion

            #region Kurve Mobilfunknetzqualität

            html += "<script src='https://www.gstatic.com/charts/loader.js'></script>" +
                "<div id='myChart' style='width:100%;height:400px;'></div>" +
                "<script>\r\n" +
                "google.charts.load('current',{packages:['corechart']});\r\n" +
                "google.charts.setOnLoadCallback(drawChart);\r\n" +
                "\r\n" +
                "function drawChart() {\r\n" +
                "\r\n" +
                "// Set Data\r\n" +
                "const data = google.visualization.arrayToDataTable([\r\n" +

                Sql.SelectNetworkQualityToArray() +
          
                "]);\r\n" +
                "\r\n" +
                @"// Set Options
                const options = {
                  backgroundColor: '#212529',                
                  chartArea: {
                      backgroundColor: 'transparent',
                  },
                  title: 'Mobilfunksignal',
                  subtitle: 'Mit der Maus ziehen zum Zoomen, Rechtsklick zurücksetzen',                  
                  titleTextStyle: { color: '#FFF' },
                  legend: { textStyle: { color: '#FFF' }},               
                  explorer: { actions: ['dragToZoom', 'rightClickToReset'] },
                  hAxis: { title: 'Zeit', format: 'HH:mm', textStyle:{color: '#AAA'}, titleTextStyle:{color: '#FFF'} },
                  vAxis: { title: 'Mobilfunksignal', textStyle:{color: '#AAA'}, titleTextStyle:{color: '#FFF'} },
                  baselineColor: 'white',
                  legend: 'none'
                }

                const chart = new google.visualization.LineChart(document.getElementById('myChart'));
                chart.draw(data, options);
                
                }" +
                "</script>";

            #endregion

            await context.Response.SendResponseAsync(Html.Sceleton(html)).ConfigureAwait(false);
        }

        [RestRoute("Get", "/gsm/reinit")]
        public static async Task GsmReinit(IHttpContext context)
        {
            Pipe1.GsmReinit();

            await GsmRoute(context);
        }

        [RestRoute("Post", "/gsm/testsms")]
        public async Task GsmTestSms(IHttpContext context)
        {
            Dictionary<string, string> formContent = (Dictionary<string, string>)context.Locals["FormData"];
            string phone = "+" + WebUtility.UrlDecode(formContent["phone"].TrimStart('+'));
            string message = WebUtility.UrlDecode(formContent["message"]);
            if (message?.Length < 3) message = "Test-SMS von MelBox2";

            Sms smsTest = new Sms(-1, DateTime.UtcNow, phone, message);

            Pipe1.SendSmsAsync(smsTest);
#if DEBUG
            MelBox2Dienst.Log.Info($"Sende Test-SMS '{smsTest.Content}' an '{smsTest.Phone}'");
#endif
            await GsmRoute(context);
        }



        [RestRoute("Get", "/overdue")]
        public static async Task OverdueRoute(IHttpContext context)
        {
            DataTable dt = Sql.SelectOverdueCustomers();
            string html = "<h4>Meldeweg&uuml;berwachung</h4>";
               
            if (dt.Rows.Count == 0)
                html += "<div class='alert alert-success alert-dismissible'>\r\n" +
                    "  <button type='button' class='btn-close' data-bs-dismiss='alert'></button>" +
                    "  <strong>Kein Handlungsbedarf</strong> Alle überwachten Absender sind aktiv.\r\n</div>";
            else
                html += "<div class='alert alert-danger'>\r\n" +
                    "  <strong>Handlungsbedarf!</strong> Diese Absender haben innerhalb der maximalen Inaktivitätszeit keine Meldung abgesetzt:<br/>" +
                    "  <strong>Meldeweg prüfen!</strong> <br/>" +
                    "  <ol>" +
                    "   <li>Störweiterleitung vor Ort eingeschaltet?</li>" +
                    "   <li>Testmeldung über Visu erzeugen und Empfang prüfen</li>" +
                    "   <li>Bei E-Mail: Kunden-IT informieren</li>" +
                    "   <li>Bei SMS: GSM-Modem prüfen. Mobilfunkempfang ok?</li> " +                    
                    "</ol>\r\n" + 
                    Html.ConvertDataTable(dt) +
                    "</div>";

            html += "<hr/><h4>&Uuml;berwachte Absender:</h4>" +
            Html.ConvertDataTable(
                Sql.SelectWatchedCustomers(), 
                new Dictionary<string, string>() { { "Id", "customer" } }
                );

            await context.Response.SendResponseAsync(Html.Sceleton(html)).ConfigureAwait(false);
        }

        [RestRoute]
        public static async Task Home(IHttpContext context)
        {
            context.Request.QueryString.Set("datum", DateTime.Now.Date.ToString("yyyy-MM-dd"));
            await RecievedMessages(context);
        }

    }
}
