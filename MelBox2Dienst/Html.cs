using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MelBox2Dienst
{
    internal class Html
    {
        #region Statische Textbausteine

       

        public static string Sceleton(string content, string containerclass = "container")
        {
            StringBuilder sb = new StringBuilder();
            #region Head
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='de' data-bs-theme='dark'>");
            sb.AppendLine("  <head>");
            sb.AppendLine("    <title>MelBox2</title>");
            sb.AppendLine("    <meta charset='utf-8'>");
            sb.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1'>");
            sb.AppendLine("    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css' rel='stylesheet'>");
            sb.AppendLine("    <script src='https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js'></script>");
            sb.AppendLine("    <script src='https://kit.fontawesome.com/980c3ae727.js' crossorigin='anonymous'></script>");
            sb.AppendLine("    <script src='https://www.w3schools.com/lib/w3.js'></script> ");
            sb.AppendLine("  </head>");
            sb.AppendLine("<body>");
            #endregion
            #region Navigation-Bar

            sb.AppendLine("<nav class='navbar navbar-expand-lg'>");
            sb.AppendLine(" <div class='container-fluid'>");
            sb.AppendLine(Logo);
            sb.AppendLine("<a class='navbar-brand' href='http://192.168.165.192:5555' target='_blank'>Kreutztr&auml;ger</a>");
            sb.AppendLine("<button class='navbar-toggler' type='button' data-bs-toggle='collapse' data-bs-target='#mynavbar'>");
            sb.AppendLine(" <span class='navbar-toggler-icon'></span>");
            sb.AppendLine("</button>");
            sb.AppendLine("<div class='collapse navbar-collapse' id='mynavbar'>");
            sb.AppendLine("<ul class='navbar-nav me-auto'>");

            sb.Append(NavIcon("Posteingang", "fas fa-arrow-circle-down", "/in"));
            sb.Append(NavIcon("Postausgang", "fas fa-arrow-circle-up", "/out"));
            sb.Append(NavIcon("Gesperrt", "fas fa-bell-slash", "/blocked"));
            sb.Append(NavIcon("&Uuml;berwacht", "fas fa-binoculars", "/overdue"));
            sb.Append(NavIcon("Bereitschaft", "fas fa-business-time", "/guard"));
            sb.Append(NavIcon("Service", "fas fa-user-clock", "/service"));
            sb.Append(NavIcon("Kunden", "fas fa-user-cog", "/customer"));
            sb.Append(NavIcon("GSM", "fas fa-broadcast-tower", "/gsm"));
            sb.Append(NavIcon("Log", "fas fa-book", "/log"));
            sb.Append(NavIcon("Fernwartung", "fas fa-globe", "http://192.168.165.192:5555"));

            sb.AppendLine("</ul>");
            sb.Append(GsmQualityIndicator());
            //sb.AppendLine("<form class='d-flex'>");
            
            sb.AppendLine(" <input class='  me-3' type='text' placeholder='Suche..' oninput=\"w3.filterHTML('#table01', '.item', this.value)\">");
            //sb.AppendLine(" <button class='btn btn-primary' type='button'>Suche</button>");
            // "  <input class=\"form-check-input\" type=\"checkbox\" id=\"mySwitch\" name=\"darkmode\" value=\"yes\" checked>\r\n  <label class=\"form-check-label\" for=\"mySwitch\">Dark Mode</label>" +
            //sb.AppendLine("</form>");
            sb.AppendLine(LoginForm);
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</nav>");
            #endregion

            sb.AppendLine($"<div class='{containerclass}'>");

            sb.Append(content);
            
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
            //"<!DOCTYPE html>\r\n" +
            //"<html lang='de' data-bs-theme='dark'>\r\n" +
            //"  <head>\r\n" +
            //"    <title>MelBox2</title>\r\n" +
            //"    <meta charset=\"utf-8\">\r\n" +
            //"    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\r\n" +
            //"    <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css\" rel=\"stylesheet\">\r\n" +
            //"    <script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js\"></script>" +
            //"    <script src='https://kit.fontawesome.com/980c3ae727.js' crossorigin='anonymous'></script>\r\n" +
            //"  </head>\r\n" +
            //"<body>\r\n" +
            //NavBar +
            //"<div class=\"container\">\r\n" +
            //content +
            //GsmQualityIndicator() +
            //"</div>\r\n" +
            //"\r\n" +
            //"</body>\r\n" +
            //"</html>";
        }

        private static string NavIcon(string name, string icon, string link)
        {
            return "<li class='nav-item'>\r\n" +
                    $" <a class='nav-link' href='{link}'>\r\n" +
                    $"  <i class='{icon}'></i>\r\n" +
                       name +
                    " </a>\r\n" +
                    "</li>\r\n";
        }

       //const string NavBar = "<nav class='navbar navbar-expand-sm'>\r\n" +
       //                             "  <div class='container-fluid'>\r\n" +
       //                             Logo +             
       //                             "    <a class='navbar-brand' href='http://192.168.165.192:5555' target='_blank'>Kreutztr&auml;ger</a>\r\n" +
       //                             "    <button class='navbar-toggler' type='button' data-bs-toggle='collapse' data-bs-target='#mynavbar'>\r\n" +
       //                             "      <span class='navbar-toggler-icon'></span>\r\n" +
       //                             "    </button>\r\n" +
       //                             "    <div class='collapse navbar-collapse' id='mynavbar'>\r\n" +
       //                             "      <ul class='navbar-nav me-auto'>\r\n" +
       //                             "        <li class='nav-item'>\r\n" +
       //                             "          <a class='nav-link' href='/in'>" +
       //                             "           <i class='fas fa-arrow-circle-down'></i>\r\n" +
       //                             " Posteingang" +
       //                             "          </a>\r\n" +
       //                             "        </li>\r\n" +
       //                             "        <li class='nav-item'>\r\n" +
       //                             "          <a class='nav-link' href='/out'>" +
       //                             "            <i class='fas fa-arrow-circle-up'></i>\r\n" +
       //                             " Postausgang" +
       //                             "          </a>\r\n" +
       //                             "        </li>\r\n" +
       //                             "        <li class='nav-item'>\r\n" +
       //                             "          <a class='nav-link' href='/blocked'>" +
       //                             "           <i class='fas fa-bell-slash'></i>\r\n" +
       //                             " Gesperrt" +
       //                             "          </a>\r\n" +
       //                             "        </li>\r\n" +
       //                             "        <li class='nav-item'>\r\n" +
       //                             "          <a class='nav-link' href='/guard'>" +
       //                             "           <i class='fas fa-business-time'></i>\r\n" +
       //                             " Bereitschaft" +
       //                             "          </a>\r\n" +
       //                             "        </li>\r\n" +
       //                             "        <li class='nav-item'>\r\n" +
       //                             "          <a class='nav-link' href='/service'>" +
       //                             "            <i class='fas fa-user-clock'></i>\r\n" +
       //                             " Service" +
       //                             "          </a>\r\n" +
       //                             "        </li>\r\n" +
       //                             "        <li class='nav-item'>\r\n" +
       //                             "          <a class='nav-link' href='/customer'>" +
       //                             "           <i class='fas fa-user-cog'></i>\r\n" +
       //                             " Kunden" +
       //                             "          </a>\r\n" +
       //                             "        </li>\r\n" +

       //                             "        <li class='nav-item'>\r\n" +
       //                             "          <a class='nav-link' href='/gsm'>" +
       //                             "           <i class='fas fa-broadcast-tower'></i>\r\n" +
       //                             " GSM" +
       //                             "          </a>\r\n" +
       //                             "        </li>\r\n" +

       //                             "        <li class='nav-item'>\r\n" +
       //                             "          <a class='nav-link' href='/log'>" +
       //                             "           <i class='fas fa-book'></i>\r\n" +
       //                             " Log" +
       //                             "          </a>\r\n" +
       //                             "        </li>\r\n" +
       //                             "        <li>\r\n" +
       //                             "          " +
       //                             "        </li>\r\n" +

       //                             "        <li class='nav-item'>\r\n" +
       //                             "          <a class='nav-link' href='http://192.168.165.192:5555' target='_blank'>" +
       //                             "           <i class='fas fa-globe'></i>\r\n" +
       //                             " Fernwartung" +
       //                             "          </a>\r\n" +
       //                             "        </li>\r\n" +
       //                             "        <li>\r\n" +
       //                             "          " +
       //                             "        </li>\r\n" +
            
       //                             "      </ul>\r\n" +                                    
       //                             "      <form class='d-flex'>\r\n" +        
       //                             "        <input class='form-control me-3' type='text' placeholder='Suche..'>\r\n" +
       //                             "        <button class='btn btn-primary' type='button'>Suche</button>\r\n" +
       //                             // "  <input class=\"form-check-input\" type=\"checkbox\" id=\"mySwitch\" name=\"darkmode\" value=\"yes\" checked>\r\n  <label class=\"form-check-label\" for=\"mySwitch\">Dark Mode</label>" +
       //                             "      </form>\r\n" +                                   
       //                             "    </div>\r\n" +
       //                             "  </div>\r\n" +
       //                             "</nav>";

        public const string Logo = @"<svg height='35' width='37'>
                                    <style>svg {background-color:#ffffff;margin-right:10px;}</style>
                                    <line x1='0' y1='0' x2='0' y2='35' style='stroke:darkcyan;stroke-width:2' />
                                    <polygon points='10,0 10,15 25,0' style='fill:#00004d;' />
                                    <polygon points='10,20 10,35 25,35' style='fill:#00004d;' />
                                    <polygon points='20,17 37,0 37,35' style='fill:darkcyan;' />
                                    Sorry, your browser does not support inline SVG.
                                  </svg>";


        const string LoginForm = @" 
                <!-- Button to Open the Modal -->
                <button type='button' class='btn btn-primary' data-bs-toggle='modal' data-bs-target='#loginModal'>
                  <i class='fas fa-key'></i>
                </button>

                <!-- The Modal -->
                <div class='modal' id='loginModal'>
                  <div class='modal-dialog'>
                    <div class='modal-content'>
                    <form action='/login' method='post' class='was-validated'>
                      <!-- Modal Header -->
                      <div class='modal-header'>
                        <h4 class='modal-title'>Login</h4>
                        <button type='button' class='btn-close' data-bs-dismiss='modal'></button>
                      </div>

                      <!-- Modal body -->
                      <div class='modal-body'>
                            
                              <div class='mb-3 mt-3'>
                                <label for='uname' class='form-label'>Benutzername:</label>
                                <input type='text' class='form-control' id='uname' placeholder='Benutzername' name='uname' required>
                            
                                <div class='invalid-feedback'>Eintrag erforderlich</div>
                              </div>
                              <div class='mb-3'>
                                <label for='pwd' class='form-label'>Passwort:</label>
                                <input type='password' class='form-control' id='pwd' placeholder='Passwort' name='pswd' required>
                               
                                <div class='invalid-feedback'>Eintrag erforderlich</div>
                              </div>                     
                      </div>

                          <!-- Modal footer -->
                          <div class='modal-footer'>
                            <button type='submit' class='btn btn-primary'>Login</button>
                           <!-- <button type='submit' class='btn btn-info' formaction='/register'>Registrieren</button> -->
                            <button type='button' class='btn btn-danger' data-bs-dismiss='modal'>Schließen</button>
                          </div>
                        </form> 
                        </div>
                      </div>
                    </div>";


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
        public static string SelectBlockedHour(string label, string name1, string name2, int selectedValue1, int selectedValue2)
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

        private static string GsmQualityIndicator()
        {
            string html = "<span>GSM</span>" +
                "<div class='progress me-3' style='top:4em;right:2em;width:10em;margin:1em;'>";

            if (System.Text.RegularExpressions.Regex.IsMatch(Pipe1.GsmSignalQuality, @"(\d{1,3})%"))
                html += $"<div class='progress-bar' style='width:{Pipe1.GsmSignalQuality}'>{Pipe1.GsmSignalQuality}</div>";
            else
                html += "<div class='progress-bar bg-secondary progress-bar-striped progress-bar-animated' style='width:100%'>GSM</div>";

            html += "</div>";

            return html;
        }


        #region Tabellendarstellung
        /// <summary>
        /// Wandelt DateTable in HTML-Table um
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>HTML-Tabelle</returns>
        public static string ConvertDataTable(DataTable dt)
        {
            string html = "<table id='table01' class='table table-striped'>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<th>" + dt.Columns[i].ColumnName + "</th>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr class='item'>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        /// <summary>
        /// Wandelt DateTable in HTML-Table um. Erstellt Links für die Spalten 'Dictionary.Key' in der Form '/<Dictionary.Value>/<Zellinhalt>''
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="links"></param>
        /// <returns></returns>
        public static string ConvertDataTable(DataTable dt, Dictionary<string, string> links)
        {
            string html = "<table id='table01' class='table table-striped'>";
            try //TEST
            {
                //add header row
                html += "<tr>";
                for (int i = 0; i < dt.Columns.Count; i++)
                    html += "<th>" + dt.Columns[i].ColumnName + "</th>";
                html += "</tr>";
                //add rows
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    html += "<tr class='item'>";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (links.ContainsKey(dt.Columns[j].ColumnName))
                            html += $"<td><a class='btn btn-primary' href='/{links[dt.Columns[j].ColumnName]}?{dt.Columns[j].ColumnName}={dt.Rows[i][j]}'>{dt.Rows[i][j]}</a></td>";
                        else if (dt.Rows[i][j].ToString().StartsWith("#") && dt.Rows[i][j].ToString().Length == 7)
                            html += $"<td style='background-color:{dt.Rows[i][j]};'></td>";
                        else
                            html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                    }
                    html += "</tr>";
                }
                html += "</table>";
            }
            catch (Exception ex)
            {
                html += ex;
            }
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
                "Weiterleitung gesperrt zu folgenden Zeiten:" +
                $" <input type='hidden' name='Id' value='{dt.Rows[0]["Id"]}'>" +
                " <div class=\"input-group mb-3\">" +
                    SelectBlockedHour("Mo", "MonStart", "MonEnd", int.Parse(dt.Rows[0]["MonStart"].ToString()), int.Parse(dt.Rows[0]["MonEnd"].ToString())) +
                    SelectBlockedHour("Di", "TueStart", "TueEnd", int.Parse(dt.Rows[0]["TueStart"].ToString()), int.Parse(dt.Rows[0]["ThuEnd"].ToString())) +
                    SelectBlockedHour("Mi", "WenStart", "WenEnd", int.Parse(dt.Rows[0]["WenStart"].ToString()), int.Parse(dt.Rows[0]["WenEnd"].ToString())) +
                    SelectBlockedHour("Do", "ThuStart", "ThuEnd", int.Parse(dt.Rows[0]["ThuStart"].ToString()), int.Parse(dt.Rows[0]["ThuEnd"].ToString())) +
                    SelectBlockedHour("Fr", "FriStart", "FriEnd", int.Parse(dt.Rows[0]["FriStart"].ToString()), int.Parse(dt.Rows[0]["FriEnd"].ToString())) +
                    SelectBlockedHour("Sa", "SatStart", "SatEnd", int.Parse(dt.Rows[0]["SatStart"].ToString()), int.Parse(dt.Rows[0]["SatEnd"].ToString())) +
                    SelectBlockedHour("So", "SunStart", "SunEnd", int.Parse(dt.Rows[0]["SunStart"].ToString()), int.Parse(dt.Rows[0]["SunEnd"].ToString())) +
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
               // "   alert(o1.value + ' - ' + o2.value);" +
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
                "<span class='input-group-text text-secondary'>Max. tolerierte Zeit zwischen Meldungen. 0 = keine Überwachung.</span>" +
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
        public static string ServiceForm(DataTable dt, bool authorized = true)
        {
            if (dt == null || dt.Rows.Count == 0) return "<span class='badge bg-danger'>Mitarbeiter unbekannt</span>";

            string form =
                "<h4>Mitarbeiterstammdaten</h4>" +
                "<form action='/service/update' method='post'>\r\n" +
               $" <input type='hidden' name='Id' value='{dt.Rows[0]["Id"]}'>" +
                " <div class='input-group mb-3'>" +
                "  <span class='input-group-text'>Name</span>" +
               $"  <input type='text' name='Name' class='form-control' placeholder='Anzeigename des Kollegen' value='{dt.Rows[0]["Name"]}' required>\r\n" +
                "  <span class='input-group-text'>Passwort</span>" +
               "  <input type='password' name='Password' pattern='.{3..}' class='form-control'>\r\n" +
                " </div>\r\n" +
                " <div class='input-group mb-3'>" +
                "  <span class='input-group-text'>Kontakt</span>" +
               $"  <input type='text' name='Phone' class='form-control' placeholder='Mobilnummer (SMS)' value='{dt.Rows[0]["Mobil"]}'>\r\n" +
               $"  <input type='text' name='Email' class='form-control' placeholder='E-Mail-Adresse' value='{dt.Rows[0]["Email"]}'>\r\n" +
               $"  <input type='color' name='Color' class='form-control form-control-color' value='{dt.Rows[0]["Farbe"]}' title='Wähle eine Anzeigefarbe'>" +
                " </div>\r\n" +

                "<div class='btn-group'>\r\n" +
                $"  <button type='submit' class='btn btn-primary{(authorized ? "" : " disabled")}'>Ändern</button>\r\n" +
                $"  <button type='submit' class='btn btn-secondary{(authorized ? "" : " disabled")}' formaction='/service/create'>Neu erstellen</button>\r\n" +
                $"  <button type='submit' class='btn btn-secondary{(authorized ? "" : " disabled")}' formaction='/service/delete'>Stammdaten l&ouml;schen</button>\r\n" +
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

                form += "<div class='form-check'>\r\n";
                    
                if (messageId != 0)
                    form +=     $"  <input type='radio' class='form-check-input' id='policy{currentPolicyId}' name='PolicyId' value='{currentPolicyId}' {(selectedPolicy == uint.Parse(currentPolicyId.ToString()) ? "checked" : "")}>\r\n" +
                                $"  <label class='form-check-label' for='policy{currentPolicyId}'>{currentPolicyId}</label></div>\r\n";
                else
                    //form +=  $"<span class='badge bg-secondary'>{currentPolicyId}</span>";
                    form += $"<a class='btn btn-primary' href='/blocked?Sperregel={currentPolicyId}'>{currentPolicyId}</span>";

                form += "</td>\r\n<td>";

                form += "<div class='progress'  style='height:3.3em; width:48vw;'>" +
                    ProgressbarHelper("Mo", dt.Rows[i]["MonStart"], dt.Rows[i]["MonEnd"]) +
                    "<div class='progress-bar bg-light' style='width:4px'></div>" +
                    ProgressbarHelper("Mo", dt.Rows[i]["TueStart"], dt.Rows[i]["TueEnd"]) +
                    "<div class='progress-bar bg-light' style='width:4px'></div>" +
                    ProgressbarHelper("Mo", dt.Rows[i]["WenStart"], dt.Rows[i]["WenEnd"]) +
                    "<div class='progress-bar bg-light' style='width:4px'></div>" +
                    ProgressbarHelper("Mo", dt.Rows[i]["ThuStart"], dt.Rows[i]["ThuEnd"]) +
                    "<div class='progress-bar bg-light' style='width:4px'></div>" +
                    ProgressbarHelper("Mo", dt.Rows[i]["FriStart"], dt.Rows[i]["FriEnd"]) +
                    "<div class='progress-bar bg-light' style='width:4px'></div>" +
                    ProgressbarHelper("Mo", dt.Rows[i]["SatStart"], dt.Rows[i]["SatEnd"]) +
                    "<div class='progress-bar bg-light' style='width:4px'></div>" +
                    ProgressbarHelper("Mo", dt.Rows[i]["SunStart"], dt.Rows[i]["SunEnd"]) +                  
                "</div>";

                form += "</td>\r\n" +
                       $"<td>{dt.Rows[i]["Kommentar"]}</td>" +
                        "</tr>\r\n";
            }
            form += "</table>";

            form += "<script>" +
                "</script>";
            
            if (messageId != 0)
                form += "<button type='submit' class='btn btn-primary mt-3'>Sperregel f&uuml;r diese Meldung &auml;ndern</button>\r\n";
            
            form += "</form>";

            return form;
        }

        /// <summary>
        /// Erstellt ein Teilelement für einen Progress-Bar zum anzeigen eines Zeitraumes an einem Tag
        /// </summary>
        /// <param name="dayName">Anzeigename für den Wochentag</param>
        /// <param name="startObj">Startstunde als Object</param>
        /// <param name="endObj">Endstunde als Object</param>
        /// <returns></returns>
        private static string ProgressbarHelper(string dayName, object startObj, object endObj)
        {
            _ = uint.TryParse(startObj.ToString(), out uint start);
            _ = uint.TryParse(endObj.ToString(), out uint end);

            return $" <div class='progress-bar bg-secondary' style='width:{start}vw'></div>\r\n" +
                   $" <div class='progress-bar bg-danger' style='width:{end - start}vw' data-bs-toggle='tooltip' title='{dayName} {start}-{end} Uhr'>{start}-{end}<br/>Uhr</div>\r\n" +
                   $" <div class='progress-bar bg-secondary' style='width:{24 - end}vw'></div>\r\n";
        }

        public static string GuardCalender(DataTable dt)
        {
            //Zugewiesene Farbe für Empfänger ermitteln, um Kalender entsprechen einzufärben
            Dictionary<uint, string> colorDict = Sql.ServiceColors();

            string html =

            "<script>\r\n" + //Bootsstrap script für Tooltips
            "var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle=\"tooltip\"]'))\r\n" +
            "var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {\r\n" +
            "  return new bootstrap.Tooltip(tooltipTriggerEl)\r\n" +
            "})\r\n" +
            "</script>\r\n" +

            "<table class='table table-striped text-center'>";

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
                    _ = DateTime.TryParse(dt.Rows[i]["Mo"]?.ToString().Substring(0, 10), out DateTime monday);

                    html += "<tr>";

                    //Id
                    if (guardId != 0)
                        html += $"<td><a class='btn btn-primary btn-sm' href='/guard?id={guardId}'><i class='fa fa-edit'></i></a></td>";
                    else
                        html += $"<td><a class='btn btn-primary btn-sm' href='/guard?datum={monday:yyyy-MM-dd}'><i class='fa fa-edit'></i></a></td>";

                    //  html += $"<td>{dt.Rows[i]["ServiceId"]}</td>";

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
                    html += GuardTableDayCol(dt.Rows[i]["Mo"], colorDict);
                    html += GuardTableDayCol(dt.Rows[i]["Di"], colorDict);
                    html += GuardTableDayCol(dt.Rows[i]["Mi"], colorDict);
                    html += GuardTableDayCol(dt.Rows[i]["Do"], colorDict);
                    html += GuardTableDayCol(dt.Rows[i]["Fr"], colorDict);
                    html += GuardTableDayCol(dt.Rows[i]["Sa"], colorDict);
                    html += GuardTableDayCol(dt.Rows[i]["So"], colorDict);

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


        private static string GuardTableDayCol(object dayValue, Dictionary<uint, string> colorDict)
        {
            //Format yyyy-MM-ddBId mit B = gleichzeitige Belegung, Id = Shift.Id

            if (!DateTime.TryParse(dayValue?.ToString().Substring(0, 10), out DateTime h))
                return $"<td>???</td>";

            string color = string.Empty;

            if (dayValue.ToString().Length > 11)
            {
                ushort.TryParse(dayValue?.ToString().Substring(10, 1), out ushort guardCount);
                uint.TryParse(dayValue?.ToString().Substring(11), out uint serviceId);
                colorDict.TryGetValue(serviceId, out color);

                if (color.Length == 0) // keine Farbe zugewiesen / auslesbar
                    color = "#cccccc";

                if (guardCount < 2) // Tag eindeutig einem Empfänger zugewiesen
                    color = $"style='background-color:{color};color:{PicTextColorOnBgColor(color)};'";
                else // mehrere Zuordnungen an einem Tag (z.B. Übergabe Bereitschaft)
                {
                    color = "style='background:" +
                                    "repeating-linear-gradient(" +
                                    "45deg," +
                                    $"{color}," +
                                    $"{color},5px," +
                                    $"#ff8888 5px," +
                                    $"#ff8888 10px" +
                                    ");" +
                                    $"color:{PicTextColorOnBgColor(color)};' " +
                                    "data-bs-toggle='tooltip' title='&Uuml;bergabe'";
                }
            }

            bool weekend = h.DayOfWeek == DayOfWeek.Sunday || h.DayOfWeek == DayOfWeek.Saturday;
            bool today = h.Date == DateTime.Now.Date;
            bool holyday = HttpHelper.IsHolyday(h);
            
            StringBuilder sb = new StringBuilder();

            if (holyday)
                sb.Append("<td class='bg-danger' data-bs-toggle='tooltip' title='Feiertag'>");
            else if (today)
                sb.Append("<td class='bg-success' data-bs-toggle='tooltip' title='heute'>");
            else if (weekend) 
                sb.Append($"<td class='bg-secondary'>");
            else
                sb.Append($"<td>");

            //sb.Append($"<span class='badge rounded-pill {(isAssigned ? colorClass : string.Empty)}' {(colorString)}>{h.Day:00}.</span>");
            sb.Append($"<span class='badge rounded-pill' {color}>{h.Day:00}.</span>");
            sb.Append("</td>");

            return sb.ToString();
        }

        private static string PicTextColorOnBgColor(string bgColor, string lightColor = "#ffffff", string darkColor= "#000000")
        {
            if (bgColor.Length < 7)
                return lightColor;

            //Quelle: https://stackoverflow.com/questions/98559/how-to-parse-hex-values-into-a-uint
            var color = bgColor.Substring(1); //  (bgColor.StartsWith("#")) ? bgColor.Substring(1, 7) : bgColor;
        
            var r = Convert.ToUInt32(color.Substring(0, 2), 16); // hexToR
            var g = Convert.ToUInt32(color.Substring(2, 2), 16); // hexToG
            var b = Convert.ToUInt32(color.Substring(4, 2), 16); // hexToB   

            return (((r * 0.299) + (g * 0.587) + (b * 0.114)) > 186) ?
              darkColor : lightColor;
        }

        //private static string ComplementColor(string origColor)
        //{
        //    if (origColor.Length < 7)
        //        return "#000000";

        //    //Quelle: https://stackoverflow.com/questions/98559/how-to-parse-hex-values-into-a-uint
        //    var color = origColor.Substring(1);

        //    var r = Convert.ToUInt32(color.Substring(0, 2), 16); // hexToR
        //    var g = Convert.ToUInt32(color.Substring(2, 2), 16); // hexToG
        //    var b = Convert.ToUInt32(color.Substring(4, 2), 16); // hexToB   

        //    return $"#{255-r:X2}{g:X2}{b:X2}";
        //}

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

        #region LogIn auf Weboberfläche

        ///// <summary>
        ///// Javascript, um im lokalen Browser Benutzername und Identität zu speichern.
        ///// </summary>
        ///// <param name="username"></param>
        ///// <param name="ident"></param>
        ///// <returns></returns>
        //internal static string SetWebStorage(string username, string ident)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    sb.AppendLine("<script>");
        //    sb.AppendLine("function webstorage() {");
        //    sb.AppendLine("  if (typeof (Storage) !== 'undefined') {");
        //    sb.AppendLine("  localStorage.setItem('user','" + username + "');");
        //    sb.AppendLine("  localStorage.setItem('ident','" + ident + "');");
        //    sb.AppendLine("  window.location.replace('/');");
        //    sb.AppendLine("  }");
        //    sb.AppendLine("}");
        //    sb.AppendLine(" webstorage();");
        //    sb.AppendLine("</script>");

        //    return sb.ToString();
        //}

        #endregion
    }
}
