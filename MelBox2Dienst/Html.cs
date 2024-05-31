﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            string form = "<span class='badge bg-danger'>Muss noch graphisch aufbereitet werden!</span>" +
                "<form action='/message/update' method='post'>\r\n" +
                $"<input type='hidden' name='MessageId' value ='{messageId}'>" +
                "<table class='table table-striped'>";

            //add header row
            form += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                form += "<th>" + dt.Columns[i].ColumnName + "</th>";
            form += "</tr>";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                form += "<tr>";
                var currentPolicyId = dt.Rows[i][idColName];

                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    form += "<td>";
                    if (dt.Columns[j].ColumnName == idColName)
                        form += "<div class='form-check'>\r\n" +
                                $"  <input type='radio' class='form-check-input' id='policy{currentPolicyId}' name='PolicyId' value='{currentPolicyId}' {(selectedPolicy == uint.Parse(currentPolicyId.ToString()) ? "checked" : "")}>\r\n" +
                                $"  <label class='form-check-label' for='policy{currentPolicyId}'>{currentPolicyId}</label></div>\r\n";
                    else if (uint.TryParse(dt.Rows[i][j].ToString(), out uint h) && h > 24)
                        form += string.Empty;
                    else
                        form += dt.Rows[i][j];

                    form += "</td>";
                }

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


        #endregion

    }
}
