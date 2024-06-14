using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MelBox2Dienst
{
    internal class Html
    {
        #region Statische Textbausteine

        public static string Sceleton(string content)
        {
            return
            "<!DOCTYPE html>\r\n" +
            "<html lang='de' data-bs-theme='dark'>\r\n" +
            "  <head>\r\n" +
            "    <title>MelBox2</title>\r\n" +
            "    <meta charset=\"utf-8\">\r\n" +
            "    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\r\n" +
            "    <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css\" rel=\"stylesheet\">\r\n" +
            "    <script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js\"></script>" +
            "    <script src='https://kit.fontawesome.com/980c3ae727.js' crossorigin='anonymous'></script>\r\n" +
            "  </head>\r\n" +
            "<body>\r\n" +
            "\r\n" +
            NavBar +
            "<div class=\"container\">\r\n" +
            //"  <h1>My First Bootstrap Page</h1>\r\n" +
            //"  <p>This part is inside a .container class.</p>\r\n" +
            //"  <p>The .container class provides a responsive fixed width container.</p>\r\n" +
            content +
            "</div>\r\n" +
            "\r\n" +
            "</body>\r\n" +
            "</html>";
        }

        public const string NavBar = "\r\n" +
                                    "<nav class='navbar navbar-expand-sm'>\r\n" +
                                    "  <div class='container-fluid'>\r\n" +
                                    Logo +
                                    "    <a class='navbar-brand' href='javascript:void(0)'>Kreutztr&auml;ger</a>\r\n" +
                                    "    <button class='navbar-toggler' type='button' data-bs-toggle='collapse' data-bs-target='#mynavbar'>\r\n" +
                                    "      <span class='navbar-toggler-icon'></span>\r\n" +
                                    "    </button>\r\n" +
                                    "    <div class='collapse navbar-collapse' id='mynavbar'>\r\n" +
                                    "      <ul class='navbar-nav me-auto'>\r\n" +
                                    "        <li class='nav-item'>\r\n" +
                                    "          <a class='nav-link' href='/in'>" +
                                    "           <i class='fas fa-arrow-circle-down'></i>\r\n" +
                                    " Posteingang" +
                                    "          </a>\r\n" +
                                    "        </li>\r\n" +
                                    "        <li class='nav-item'>\r\n" +
                                    "          <a class='nav-link' href='/out'>" +
                                    "            <i class='fas fa-arrow-circle-up'></i>\r\n" +
                                    " Postausgang" +
                                    "          </a>\r\n" +
                                    "        </li>\r\n" +
                                    "        <li class='nav-item'>\r\n" +
                                    "          <a class='nav-link' href='/blocked'>" +
                                    "           <i class='fas fa-bell-slash'></i>\r\n" +
                                    " Gesperrt" +
                                    "          </a>\r\n" +
                                    "        </li>\r\n" +
                                    "        <li class='nav-item'>\r\n" +
                                    "          <a class='nav-link' href='/guard'>" +
                                    "           <i class='fas fa-business-time'></i>\r\n" +
                                    " Bereitschaft" +
                                    "          </a>\r\n" +
                                    "        </li>\r\n" +
                                    "        <li class='nav-item'>\r\n" +
                                    "          <a class='nav-link' href='/service'>" +
                                    "            <i class='fas fa-user-clock'></i>\r\n" +
                                    " Service" +
                                    "          </a>\r\n" +
                                    "        </li>\r\n" +
                                    "        <li class='nav-item'>\r\n" +
                                    "          <a class='nav-link' href='/customer'>" +
                                    "           <i class='fas fa-user-cog'></i>\r\n" +
                                    " Kunden" +
                                    "          </a>\r\n" +
                                    "        </li>\r\n" +
                                    "        <li class='nav-item'>\r\n" +
                                    "          <a class='nav-link' href='/log'>" +
                                    "           <i class='fas fa-book'></i>\r\n" +
                                    " Log" +
                                    "          </a>\r\n" +
                                    "        </li>\r\n" +
                                    "        <li>\r\n" +
                                    "          " +
                                    "        </li>\r\n" +

                                    "      </ul>\r\n" +
                                    "      <form class='d-flex'>\r\n" +
                                    "        <input class='form-control me-3' type='text' placeholder='Suche..'>\r\n" +
                                    "        <button class='btn btn-primary' type='button'>Suche</button>\r\n" +
                                    // "  <input class=\"form-check-input\" type=\"checkbox\" id=\"mySwitch\" name=\"darkmode\" value=\"yes\" checked>\r\n  <label class=\"form-check-label\" for=\"mySwitch\">Dark Mode</label>" +
                                    "      </form>\r\n" +
                                    "    </div>\r\n" +
                                    "  </div>\r\n" +
                                    "</nav>";

        public const string Logo = @"<svg height='35' width='37'>
                                    <style>svg {background-color:#ffffff;margin-right:10px;}</style>
                                    <line x1='0' y1='0' x2='0' y2='35' style='stroke:darkcyan;stroke-width:2' />
                                    <polygon points='10,0 10,15 25,0' style='fill:#00004d;' />
                                    <polygon points='10,20 10,35 25,35' style='fill:#00004d;' />
                                    <polygon points='20,17 37,0 37,35' style='fill:darkcyan;' />
                                    Sorry, your browser does not support inline SVG.
                                  </svg>";

        //private const string Link1 = "<li class='nav-item'>\r\n" +
        //                             "  <a class='nav-link' href='/in'>" +
        //                             "    <i class='fas fa-arrow-circle-down'></i>\r\n" +
        //                             "    Posteingang" +
        //                             "  </a>\r\n" +
        //                             "</li>\r\n";

        //private const string Link2 = "<li class='nav-item'>\r\n" +
        //                             "  <a class='nav-link' href='/in'>" +
        //                             "    <i class='fas fa-arrow-circle-up'></i>\r\n" +
        //                             "    Postausgang" +
        //                             "  </a>\r\n" +
        //                             "</li>\r\n";


        public static string DatePicker(string route, DateTime date)
        {
            return $"<a type='button' class='btn fa fa-angle-double-left' href='/{route}?datum={date.AddDays(-1):yyyy-MM-dd}'></a>" +
                   $"  <input type='date' value='{date:yyyy-MM-dd}' max='{DateTime.Now.Date:yyyy-MM-dd}' onblur='window.open(\"/{route}?datum=\"+this.value,\"_self\")'>" +
                   $"<a type='button' class='btn fa fa-angle-double-right' href='/{route}?datum={date.AddDays(1):yyyy-MM-dd}'></a>" +
                   "</p>";

        }

        /// <summary>
        /// Erzeugt zwei HTML-Form-Elemente 'Select' mit Uhrzeit-Auswahl
        /// </summary>
        /// <param name="label">Anzeige</param>
        /// <param name="name1">FormName Wert 1</param>
        /// <param name="name2">FormName Wert 2</param>
        /// <returns></returns>
        public static string SelectFieledHour(string label, string name1, string name2, int selectedValue1, int selectedValue2)
        {
            // Console.WriteLine($"{label}: {name1}={selectedValue1}, {name2}={selectedValue2}");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<div class='col'>");
            sb.AppendLine($"<span class='input-group-text bg-secondary'>{label}</span>");
            sb.AppendLine("<span class='input-group-text'>von</span>");
            sb.AppendLine($"<select class='form-select {(selectedValue1 >= selectedValue2 ? "bg-secondary" : string.Empty)}' " + //Farbänderung, wenn ungültige Uhrzeitkombination ausgelesen wurde
                $"name='{name1}' onchange=\"f(this.name,'{name2}')\">");

            for (int i = 0; i < 24; i++)
                sb.AppendLine($"<option value='{i}' {(selectedValue1 == i ? "selected" : "")}>{i} Uhr</option>");

            sb.AppendLine("</select>");
            sb.AppendLine("<span class='input-group-text'>bis</span>");
            sb.AppendLine($"<select class='form-select {(selectedValue1 >= selectedValue2 ? "bg-secondary" : string.Empty)}' " +
                $"name='{name2}' onchange=\"f('{name1}', this.name)\">");

            for (int i = 0; i <= 24; i++)
                sb.AppendLine($"<option value='{i}' {(selectedValue2 == i ? "selected" : "")}>{i} Uhr</option>");

            sb.AppendLine("</select>");
            sb.AppendLine("</div>");

            return sb.ToString();
        }

        #endregion

        #region Tabellendarstellung
        /// <summary>
        /// Wandelt DateTable in HTML-Table um
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>HTML-Tabelle</returns>
        public static string ConvertDataTable(DataTable dt)
        {
            string html = "<table class='table table-striped'>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<th>" + dt.Columns[i].ColumnName + "</th>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        /// <summary>
        /// Wandelt DateTable in HTML-Table um. Erstellt Links für die Spalten 'Dictionary.Key' in der Form '/<Dictionary.Value>/<Zellinkalt>''
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="links"></param>
        /// <returns></returns>
        public static string ConvertDataTable(DataTable dt, Dictionary<string, string> links)
        {
            string html = "<table class='table table-striped'>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<th>" + dt.Columns[i].ColumnName + "</th>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (links.ContainsKey(dt.Columns[j].ColumnName))
                        html += $"<td><a class='btn btn-primary' href='/{links[dt.Columns[j].ColumnName]}?{dt.Columns[j].ColumnName}={dt.Rows[i][j]}'>{dt.Rows[i][j]}</a></td>";
                    else
                        html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                }
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        /// <summary>
        /// Zeigt eine Sperregel in einem Formular an.
        /// </summary>
        /// <param name="dt">SQL-Abfrage mit einer Zeile aus Tabelle 'Block'</param>
        /// <returns></returns>
        public static string BlockPolicy(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return "<span class='badge bg-danger'>Sperregel unbekannt</span>";

            //DataTable mit Kopfzeile und einer Datenzeile
            string form =
                "<form action='/blocked/update' method='post'>\r\n" +
                "  <label for='comment'>Beschreibung:</label>\r\n" +
                $" <textarea class='form-control' id='comment' name='Comment'>{dt.Rows[0]["Comment"]}</textarea>\r\n" +
                "<span class='badge'>Weiterleitung gesperrt</span>" +
                $" <input type='hidden' name='Id' value='{dt.Rows[0]["Id"]}'>" +
                " <div class=\"input-group mb-3\">" +
                    SelectFieledHour("Mo", "MonStart", "MonEnd", int.Parse(dt.Rows[0]["MonStart"].ToString()), int.Parse(dt.Rows[0]["MonEnd"].ToString())) +
                    SelectFieledHour("Di", "TueStart", "TueEnd", int.Parse(dt.Rows[0]["TueStart"].ToString()), int.Parse(dt.Rows[0]["ThuEnd"].ToString())) +
                    SelectFieledHour("Mi", "WenStart", "WenEnd", int.Parse(dt.Rows[0]["WenStart"].ToString()), int.Parse(dt.Rows[0]["WenEnd"].ToString())) +
                    SelectFieledHour("Do", "ThuStart", "ThuEnd", int.Parse(dt.Rows[0]["ThuStart"].ToString()), int.Parse(dt.Rows[0]["ThuEnd"].ToString())) +
                    SelectFieledHour("Fr", "FriStart", "FriEnd", int.Parse(dt.Rows[0]["FriStart"].ToString()), int.Parse(dt.Rows[0]["FriEnd"].ToString())) +
                    SelectFieledHour("Sa", "SatStart", "SatEnd", int.Parse(dt.Rows[0]["SatStart"].ToString()), int.Parse(dt.Rows[0]["SatEnd"].ToString())) +
                    SelectFieledHour("So", "SunStart", "SunEnd", int.Parse(dt.Rows[0]["SunStart"].ToString()), int.Parse(dt.Rows[0]["SunEnd"].ToString())) +
                "</div>\r\n" +

                "<div class='btn-group'>\r\n" +
                "  <button type='submit' class='btn btn-primary'>Global Speichern</button>\r\n" +
                "  <button type='submit' class='btn btn-secondary' formaction='/blocked/create' method='post'>Neu erstellen</button>\r\n" +
                "  <button type='submit' class='btn btn-secondary' formaction='/blocked/delete'>Sperregel l&ouml;schen</button>\r\n" +
                "</div>\r\n" +
                "<script>" +
                "function f(n1, n2) {\r\n" + //Dynamischer Farbumschlag: Prüfe gültige Uhrzeitkombination
                "  let o1 = document.getElementsByName(n1)[0];\r\n" +
                "  let o2 = document.getElementsByName(n2)[0];\r\n" +
                "   alert(o1.value + ' - ' + o2.value);" +
                "  if (parseInt(o1.value) >= parseInt(o2.value)) {\r\n" +
                "    o1.classList.add('bg-secondary');\r\n" +
                "    o2.classList.add('bg-secondary');\r\n" +
                "  } else {\r\n" +
                "    o1.classList.remove('bg-secondary');\r\n" +
                "    o2.classList.remove('bg-secondary');\r\n" +
                "  }\r\n" +
                "}" +
                "</script>" +
            "</form>\r\n";
            return form;
        }



        #endregion

        #region Formularansichten

        /// <summary>
        /// Formular zum Ändern der Stammdaten eines Kunden
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string CustomerForm(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return "<span class='badge bg-danger'>Kunde unbekannt</span>";

            string form =
                "<h4>Kundenstammdaten</h4>" +
                "<form action='/customer/update' method='post'>\r\n" +
                $" <input type='hidden' name='Id' value='{dt.Rows[0]["Id"]}'>" +
                "<div class='input-group mb-3'>" +
                " <span class='input-group-text'>Name</span>" +
                $"<input type='text' name='Name' class='form-control' placeholder='Anzeigename des Kunden' value='{dt.Rows[0]["Name"]}' required>\r\n" +
                "</div>\r\n" +
                "<div class='input-group mb-3'>" +
                " <span class='input-group-text'>Kontakt</span>" +
                $"<input type='text' name='Phone' class='form-control' placeholder='Mobilnummer (SMS)' value='{dt.Rows[0]["Mobil"]}'>\r\n" +
                $"<input type='text' name='Email' class='form-control' placeholder='E-Mail-Adresse' value='{dt.Rows[0]["Email"]}'>\r\n" +
                "</div>\r\n" +
                "<div class='input-group mb-3'>" +
                " <span class='input-group-text'>Kennung</span>" +
                $"<input type='text' name='KeyWord' class='form-control' placeholder='Identifizierung, wenn Rufnummer nicht übertragen wird' value='{dt.Rows[0]["Kennung"]}' readonly>\r\n" +
                "</div>\r\n" +
                "<div class='input-group mb-3'>" +
                " <span class='input-group-text'>Maximale Inaktivität</span>" +
                $"<input type='number' name='MaxInactiveHours' class='form-control' value='{dt.Rows[0]["Max_Inaktiv"]}' step='12' min='0'>\r\n" +
                "<span class='input-group-text'>Stunden</span>" +
                "<span class='input-group-text text-secondary'>Geht nach dieser Zeit keine Nachricht von diesem Kontakt ein, wird davon ausgegengen, dass die Meldelinie unterbrochen ist.</span>" +
                "</div>\r\n" +

                "<div class='btn-group'>\r\n" +
                "  <button type='submit' class='btn btn-primary'>Ändern</button>\r\n" +
                "  <button type='submit' class='btn btn-secondary' formaction='/customer/create'>Neu erstellen</button>\r\n" +
                "  <button type='submit' class='btn btn-secondary' formaction='/customer/delete'>Stammdaten l&ouml;schen</button>\r\n" +
                "</div>\r\n" +
            "</form>\r\n";
            return form;
        }

        /// <summary>
        /// Formular zum Ändern der Stammdaten eines Bereitshcaftsmitarbeiters
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ServiceForm(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return "<span class='badge bg-danger'>Mitarbeiter unbekannt</span>";

            string form =
                "<h4>Mitarbeiterstammdaten</h4>" +
                "<form action='/service/update' method='post'>\r\n" +
               $" <input type='hidden' name='Id' value='{dt.Rows[0]["Id"]}'>" +
                " <div class='input-group mb-3'>" +
                "  <span class='input-group-text'>Name</span>" +
               $"  <input type='text' name='Name' class='form-control' placeholder='Anzeigename des Kollegen' value='{dt.Rows[0]["Name"]}' required>\r\n" +
                " </div>\r\n" +
                " <div class='input-group mb-3'>" +
                "  <span class='input-group-text'>Kontakt</span>" +
               $"  <input type='text' name='Phone' class='form-control' placeholder='Mobilnummer (SMS)' value='{dt.Rows[0]["Mobil"]}'>\r\n" +
               $"  <input type='text' name='Email' class='form-control' placeholder='E-Mail-Adresse' value='{dt.Rows[0]["Email"]}'>\r\n" +
                " </div>\r\n" +

                "<div class='btn-group'>\r\n" +
                "  <button type='submit' class='btn btn-primary'>Ändern</button>\r\n" +
                "  <button type='submit' class='btn btn-secondary' formaction='/service/create'>Neu erstellen</button>\r\n" +
                "  <button type='submit' class='btn btn-secondary' formaction='/service/delete'>Stammdaten l&ouml;schen</button>\r\n" +
                "</div>\r\n" +
            "</form>\r\n";
            return form;
        }

        /// <summary>
        /// Erstellt eine Auswahl (Radio-Button) aller verfügbaren Sperregeln.
        /// TODO: graphische Darstellung dieser Tabelle als Sperrzeiten-Diagramm (Balken?)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="selectedPolicy"></param>
        /// <returns></returns>
        public static string BlockPolicySelection(DataTable dt, uint selectedPolicy, uint messageId)
        {
            string idColName = "Sperregel"; //Name der Spalte, die eine Sperregel eindeutig bezeichnet.

            string form =
                "<form action='/message/update' method='post'>\r\n" +
                $"<input type='hidden' name='MessageId' value ='{messageId}'>" +
                "<table class='table table-striped'>";

            //add header row
            form += "<tr>";
            //for (int i = 0; i < dt.Columns.Count; i++)
            //    form += "<th>" + dt.Columns[i].ColumnName + "</th>";
            form += "<th>Sperregel</th>" +
                "<th><div class='row' style='width:48vw;'>" +

                " <div class='col'>Mo</div>" +
                " <div class='col'>Di</div>" +
                " <div class='col'>Mi</div>" +
                " <div class='col'>Do</div>" +
                " <div class='col'>Fr</div>" +
                " <div class='col'>Sa</div>" +
                " <div class='col'>So</div>" +

                "</div></th>" +

                "<th>Kommentar</th>";
            form += "</tr>";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                form += "<tr>";
                var currentPolicyId = dt.Rows[i][idColName];

                form += "<td>";

                form += "<div class='form-check'>\r\n" +
                                $"  <input type='radio' class='form-check-input' id='policy{currentPolicyId}' name='PolicyId' value='{currentPolicyId}' {(selectedPolicy == uint.Parse(currentPolicyId.ToString()) ? "checked" : "")}>\r\n" +
                                $"  <label class='form-check-label' for='policy{currentPolicyId}'>{currentPolicyId}</label></div>\r\n";

                form += "</td>\r\n<td>";

                _ = uint.TryParse(dt.Rows[i]["MonStart"].ToString(), out uint monStart);
                _ = uint.TryParse(dt.Rows[i]["MonEnd"].ToString(), out uint monEnd);
                _ = uint.TryParse(dt.Rows[i]["TueStart"].ToString(), out uint tueStart);
                _ = uint.TryParse(dt.Rows[i]["TueEnd"].ToString(), out uint tueEnd);
                _ = uint.TryParse(dt.Rows[i]["WenStart"].ToString(), out uint wenStart);
                _ = uint.TryParse(dt.Rows[i]["WenEnd"].ToString(), out uint wenEnd);
                _ = uint.TryParse(dt.Rows[i]["ThuStart"].ToString(), out uint thuStart);
                _ = uint.TryParse(dt.Rows[i]["ThuEnd"].ToString(), out uint thuEnd);
                _ = uint.TryParse(dt.Rows[i]["FriStart"].ToString(), out uint friStart);
                _ = uint.TryParse(dt.Rows[i]["FriEnd"].ToString(), out uint friEnd);
                _ = uint.TryParse(dt.Rows[i]["SatStart"].ToString(), out uint satStart);
                _ = uint.TryParse(dt.Rows[i]["SatEnd"].ToString(), out uint satEnd);
                _ = uint.TryParse(dt.Rows[i]["SunStart"].ToString(), out uint sunStart);
                _ = uint.TryParse(dt.Rows[i]["SunEnd"].ToString(), out uint sunEnd);

                form += "<div class='progress'  style='height:2em; width:48vw;'>" +

                      $" <div class='progress-bar bg-secondary' style='width:{monStart}vw'></div>" +
                      $" <div class='progress-bar bg-danger' style='width:{monEnd - monStart}vw'>{monStart}-{monEnd} Uhr</div>" +
                      $" <div class='progress-bar bg-secondary' style='width:{24 - monEnd}vw'></div>" +
                      $" <div class='progress-bar bg-light' style='width:4px'></div>" +

                      $" <div class='progress-bar bg-secondary' style='width:{tueStart}vw'></div>" +
                      $" <div class='progress-bar bg-danger' style='width:{tueEnd - tueStart}vw'>{tueStart}-{tueEnd} Uhr</div>" +
                      $" <div class='progress-bar bg-secondary' style='width:{24 - tueEnd}vw'></div>" +
                      $" <div class='progress-bar bg-light' style='width:4px;'></div>" +

                      $" <div class='progress-bar bg-secondary' style='width:{wenStart}vw'></div>" +
                      $" <div class='progress-bar bg-danger' style='width:{wenEnd - wenStart}vw'>{wenStart}-{wenEnd} Uhr</div>" +
                      $" <div class='progress-bar bg-secondary' style='width:{24 - wenEnd}vw'></div>" +
                      $" <div class='progress-bar bg-light' style='width:4px;'></div>" +

                      $" <div class='progress-bar bg-secondary' style='width:{thuStart}vw'></div>" +
                      $" <div class='progress-bar bg-danger' style='width:{thuEnd - thuStart}vw'>{thuStart}-{thuEnd} Uhr</div>" +
                      $" <div class='progress-bar bg-secondary' style='width:{24 - thuEnd}vw'></div>" +
                      $" <div class='progress-bar bg-light' style='width:4px;'></div>" +

                      $" <div class='progress-bar bg-secondary' style='width:{friStart}vw'></div>" +
                      $" <div class='progress-bar bg-danger' style='width:{friEnd - friStart}vw'>{friStart}-{friEnd} Uhr</div>" +
                      $" <div class='progress-bar bg-secondary' style='width:{24 - friEnd}vw'></div>" +
                      $" <div class='progress-bar bg-light' style='width:4px;'></div>" +

                      $" <div class='progress-bar bg-secondary' style='width:{satStart}vw'></div>" +
                      $" <div class='progress-bar bg-danger' style='width:{satEnd - satStart}vw'> {satStart}-{satEnd} Uhr</div>" +
                      $" <div class='progress-bar bg-secondary' style='width:{24 - satEnd}vw'></div>" +
                      $" <div class='progress-bar bg-light' style='width:4px;'></div>" +

                      $" <div class='progress-bar bg-secondary' style='width:{sunStart}vw'></div>" +
                      $" <div class='progress-bar bg-danger' style='width:{sunEnd - sunStart}vw'> {sunStart}-{sunEnd} Uhr</div>" +
                      $" <div class='progress-bar bg-secondary' style='width:{24 - sunEnd}vw'></div>" +
            
                "</div>";



                form += "</td>\r\n";
                form += $"<td>{dt.Rows[i]["Kommentar"]}</td>";

                //for (int j = 0; j < dt.Columns.Count; j++)
                //{
                //    form += "<td>";
                //    if (dt.Columns[j].ColumnName == idColName)
                //        form += "<div class='form-check'>\r\n" +
                //                $"  <input type='radio' class='form-check-input' id='policy{currentPolicyId}' name='PolicyId' value='{currentPolicyId}' {(selectedPolicy == uint.Parse(currentPolicyId.ToString()) ? "checked" : "")}>\r\n" +
                //                $"  <label class='form-check-label' for='policy{currentPolicyId}'>{currentPolicyId}</label></div>\r\n";
                //    else if (uint.TryParse(dt.Rows[i][j].ToString(), out uint h))
                //        if (h > 24)
                //            form += string.Empty;
                //        else
                //        {
                //            if(dt.Columns[j].ColumnName.EndsWith("Start"))
                //            form += "<div class='progress'>" +
                //                   $" <div class='progress-bar bg-secondary' style='width:{h*100/24}%'>{h}</div>" +
                //                   $" <div class='progress-bar bg-primary' style='width:{(24-h) * 100 / 24}%'></div>" +
                //                    "</div>";

                //            if (dt.Columns[j].ColumnName.EndsWith("End"))
                //                form += "<div class='progress'>" +
                //                       $" <div class='progress-bar bg-primary' style='width:{h * 100 / 24}%'></div>" +
                //                       $" <div class='progress-bar bg-secondary' style='width:{(24 - h) * 100 / 24}%'>{h}</div>" +
                //                        "</div>";
                //        }
                //    else
                //        form += dt.Rows[i][j];

                //    form += "</td>";
                //}

                form += "</tr>\r\n";
            }
            form += "</table>" +
            "<button type='submit' class='btn btn-primary mt-3'>Sperregel ändern</button>\r\n" +
            "</form>";


            // Style Skala + Balkendarstellung
            //"<style>\r\n.g {\r\nheight: 5px;\r\nbackground: repeating-linear-gradient(\r\n90deg,\r\nblue,\r\nblue 2px,\r\nblack 2px,\r\nblack 4.17%\r\n);\r\n}\r\n        \r\n#Mo {\r\n  height: 20px;\r\n  background-image: linear-gradient(to right, black 0%, black 95.91% , orange 95.91%, orange 100%, black 100%, black 100% );\r\n}\r\n</style>"
            //"<div id=\"Mo\"></div>\r\n<div class=\"g\"></div>"
            return form;
        }


        public static string GuardCalender(DataTable dt)
        {

            List<uint> ServiceIds = new List<uint>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
                if (!row.IsNull("ServiceId") && uint.TryParse(row["ServiceId"].ToString(), out uint serviceId))
                    ServiceIds.Add(serviceId);

            //ToDo: Zugewiesene Farbe für Empfänger ermitteln und Kalender entsprechen einfärben

            string html = "<table class='table table-striped text-center'>";
            try
            {

                //Kopfzeile
                html += "<tr>";
                for (int i = 0; i < dt.Columns.Count; i++)
                    if (dt.Columns[i].ColumnName != "ServiceId")
                        html += "<th>" + dt.Columns[i].ColumnName + "</th>";
                html += "</tr>";

                //Tabelleninhalt
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _ = uint.TryParse(dt.Rows[i]["Id"].ToString(), out uint guardId);
                    _ = uint.TryParse(dt.Rows[i]["ServiceId"].ToString(), out uint serviceId);
                    _ = DateTime.TryParse(dt.Rows[i]["Mo"]?.ToString().Substring(0,10), out DateTime monday);

                   // Console.WriteLine("Montag ist " + monday);

                    html += "<tr>";

                    //Id
                    if (guardId != 0)
                        html += $"<td><a class='btn btn-primary btn-sm' href='/guard?id={guardId}'><i class='fa fa-edit'></i></a></td>";                   
                    else
                        html += $"<td><a class='btn btn-primary btn-sm' href='/guard?datum={monday:yyyy-MM-dd}'><i class='fa fa-edit'></i></a></td>";

                    //html += $"<td>{dt.Rows[i]["ServiceId"]}</td>";

                    //Name
                    if (dt.Rows[i]["Name"] == null)
                        html += $"<td>&nbsp;</td>";
                    else
                        html += $"<td class='text-start'><a class='btn btn-sm ' href='/service?id={serviceId}'>{dt.Rows[i]["Name"]}</a></td>";

                    //Daten
                    if (DateTime.TryParse(dt.Rows[i]["Beginn"]?.ToString(), out DateTime startDate))
                        html += $"<td>{startDate.ToShortDateString()}</td>";
                    else
                        html += $"<td>&nbsp;</td>";

                    if (DateTime.TryParse(dt.Rows[i]["Ende"].ToString(), out DateTime endDate))
                        html += $"<td>{endDate.ToShortDateString()}</td>";
                    else
                        html += "<td>&nbsp;</td>";

                    html += $"<td>{dt.Rows[i]["KW"]}</td>";


                    //Tage
                    html += GuardTableDayCol(dt.Rows[i]["Mo"]);
                    html += GuardTableDayCol(dt.Rows[i]["Di"]);
                    html += GuardTableDayCol(dt.Rows[i]["Mi"]);
                    html += GuardTableDayCol(dt.Rows[i]["Do"]);
                    html += GuardTableDayCol(dt.Rows[i]["Fr"]);
                    html += GuardTableDayCol(dt.Rows[i]["Sa"]);
                    html += GuardTableDayCol(dt.Rows[i]["So"]);

                    html += "</tr>";
                }

                html += "</table>";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            
                return html;
            
        }


        private static string GuardTableDayCol(object dayValue)
        {
            if (!DateTime.TryParse(dayValue?.ToString().Substring(0, 10), out DateTime h))            
                return $"<td>???</td>";
           
            string colorClass;
            if (dayValue?.ToString().Length > 10 && ushort.TryParse(dayValue?.ToString().Substring(10), out ushort guardCount) && guardCount > 1)
                colorClass = "bg-primary"; // mehrere Zuordnungen an einem Tag (z.B. Übergabe Bereitschaft)
            else
                colorClass = "bg-info"; // Zuordnung zu einem Empfänger vorhanden

            bool weekend = h.DayOfWeek == DayOfWeek.Sunday || h.DayOfWeek == DayOfWeek.Saturday;
            bool today = h.Date == DateTime.Now.Date;
            bool holyday = HttpHelper.IsHolyday(h);
            bool isAssigned = dayValue.ToString().Length > 10;

            StringBuilder sb = new StringBuilder();

            if (holyday)
                sb.Append("<td class='bg-danger'>");
            else if (today)
                sb.Append("<td class='bg-success'>");
            else if (weekend) 
                sb.Append($"<td class='bg-secondary'>");
            else
                sb.Append($"<td>");

            sb.Append($"<span class='badge rounded-pill {(isAssigned ? colorClass : string.Empty)}'>{h.Day:00}.</span>");
            sb.Append("</td>");

            return sb.ToString();
        }

        public static string GuardFormUpdate(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return "<span class='badge bg-danger'>Bereitschafseinteilung unbekannt</span>";

            _ = uint.TryParse(dt.Rows[0]["Id"]?.ToString(), out uint guardId);
            _ = uint.TryParse(dt.Rows[0]["ServiceId"]?.ToString(), out uint serviceId);
            _ = DateTime.TryParse(dt.Rows[0]["Beginn"]?.ToString(), out DateTime start);
            _ = DateTime.TryParse(dt.Rows[0]["Ende"]?.ToString(), out DateTime end);
            start = start.ToLocalTime();
            end = end.ToLocalTime();

            //  Shift.Id AS Id, 
            //strftime(Time, 'localtime') AS Stand, 
            //ToId AS ServiceId, 
            //Service.Name AS Name, 
            //Start AS Beginn,
            //End AS Ende
            //FROM Shift 
            //JOIN Service 
            //ON Shift.ToId = Service.Id

            string form =
                    "<h4>Bereitschaft einteilen</h4>" +
                    "<form action='/guard/update' method='post'>\r\n" +
                   $"<div class='text-muted'>Stand {dt.Rows[0]["Stand"]}</div>" +
                   $" <input type='hidden' name='Id' value='{guardId}'>" +
                    " <div class='input-group mb-3'>" +
                    "  <span class='input-group-text'>Name</span>" +            
                   $"  <select class='form-select'  name='ServiceId' id='ServiceId' placeholder='Anzeigename des Kollegen' value='{serviceId}' required>\r\n" +
                        ServiceNameListOptions(serviceId) +
                    "  </select>" +                
                    "<script>\r\n" +
                        "function mint(id, val){\r\n" +
                        " document.getElementById(id).min = val;\r\n" +
                        "}\r\n" +
                        "function maxt(id, val){\r\n" +
                        " document.getElementById(id).max = val;\r\n" +
                        "}\r\n" +
                    "</script>\r\n" +

                    "</div>\r\n" +
                    " <div class='input-group mb-3'>" +
                    "  <span class='input-group-text'>von</span>" +
                    $"<input type='date' name='StartDate' id='StartDate' value='{start:yyyy-MM-dd}' max='{end:yyyy-MM-dd}' onchange='mint(\"EndDate\", this.value);'>" +
                    $"<input type='time' name='StartTime' value='{start:HH:mm}'>" +
                    "  <span class='input-group-text'>bis</span>" +
                    $"<input type='date' name='EndDate' id='EndDate' value='{end:yyyy-MM-dd}' min='{start:yyyy-MM-dd}' onchange='maxt(\"StartDate\", this.value);'>" +
                    $"<input type='time' name='EndTime' value='{end:HH:mm}'>" +
                    "</div>\r\n" +

                    "<div class='btn-group'>\r\n" +          
                    "<button type='submit' class='btn btn-primary'>&Auml;ndern</button>\r\n" +
                    "<button type='submit' class='btn btn-secondary' formaction='/guard/create'>Neu erstellen</button>\r\n" +       
                    "<button type='submit' class='btn btn-secondary' formaction='/guard/delete'>Einteilung l&ouml;schen</button>\r\n" +
                    "</div>\r\n" +
                    "</form>\r\n";
            return form;
        }

        public static string GuardFormNew(DateTime start, uint serviceId)
        {
            #region Uhrzeiten vorauswählen
            start = start.Date; // 0 Uhr

            switch (start.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:      
                    // nichts machen
                    break;
                case DayOfWeek.Friday:
                    start = start.AddHours(15);
                    break;
                default:
                    start = start.AddHours(17);
                    break;
            }

            DateTime end = start.AddDays(7).Date.AddHours(7);

#if DEBUG
            Console.WriteLine($"Neue Bereitschaft von {start} bis {end}.");
#endif
#endregion

            string form =
                    "<h4>Bereitschaft einteilen</h4>" +
                    "<form action='/guard/create' method='post'>\r\n" +
                    " <div class='input-group mb-3'>" +
                    "  <span class='input-group-text'>Name</span>" +
                   $"  <select class='form-select'  name='ServiceId' id='ServiceId' placeholder='Anzeigename des Kollegen' value='{serviceId}' required>\r\n" +
                        ServiceNameListOptions(serviceId) +
                    "  </select>" +            
                    "</div>\r\n" +
                    " <div class='input-group mb-3'>" +
                    "  <span class='input-group-text'>von</span>" +
                    $"<input type='date' name='StartDate' value='{start:yyyy-MM-dd}'>" +
                    $"<input type='time' name='StartTime' value='{start:HH:mm}'>" +
                    "  <span class='input-group-text'>bis</span>" +
                    $"<input type='date' name='EndDate'value='{end:yyyy-MM-dd}'>" +
                    $"<input type='time' name='EndTime' value='{end:HH:mm}'>" +
                    "</div>\r\n" +

                    "<div class='btn-group'>\r\n";
   
                form += "<button type='submit' class='btn btn-primary' formaction='/guard/create'>Neu erstellen</button>\r\n";

            form += "</div>\r\n" +
                    "</form>\r\n";
            return form;
        }


        /// <summary>
        /// Liste aller potentillen Empfänger
        /// </summary>
        /// <returns></returns>
        private static string ServiceNameListOptions(uint serviceId)
        {
            Dictionary<uint, string> names = Sql.ServiceNames();

            StringBuilder sbNames = new StringBuilder();

            foreach (var name in names)            
                sbNames.AppendLine($"<option value='{name.Key}' {(serviceId == name.Key ? "selected" : "")}>{name.Value}</option>");
            
            return sbNames.ToString();
        }

        #endregion
    }
}
