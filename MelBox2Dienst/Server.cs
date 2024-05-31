using Grapevine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HttpMultipartParser;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MelBox2Dienst
{
    internal class Server
    {
        private static IRestServer RestServer { get; set; }

        public static void Start()
        {
            if (RestServer != null && RestServer.IsListening) return;

            try
            {
                RestServer = RestServerBuilder.From<Startup>().Build();

                RestServer.AfterStarting += (s) =>
                {
                    //Route Scanner
                    //s.RouteScanner.Scan();

                    if (Environment.UserInteractive) //Start als Konsolenanwendung                    
                                                     //System.Diagnostics.Process.Start("explorer", s.Prefixes.First().Replace("+", System.Net.Dns.GetHostName()));
                        OpenBrowser(s.Prefixes.First());

                    Log.Info("Web-Server gestartet.");
                };

                RestServer.AfterStopping += (s) =>
                {
                    Log.Info("Web-Server beendet.");
                };

                RestServer.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + Environment.NewLine + ex.StackTrace);
                throw ex;
            }
        }

        public static void Stop()
        {
            RestServer?.Stop();
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }

        }

        internal class Startup
        {
            public IConfiguration Configuration { get; private set; }

            private readonly string _serverPort = PortFinder.FindNextLocalOpenPort(7307);

            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(LogLevel.Warning);
                });
            }

            public void ConfigureServer(IRestServer server)
            {
                server.AutoParseMultipartFormData();

                // The path to your static content
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "static");
                server.ContentFolders.Add(folderPath);
                //server.UseContentFolders();

                if (Program.IsRunningInConsole)
                    foreach (var item in server.ContentFolders)
                    {
                        Console.WriteLine($"ContentFolder '{item.FolderPath}'\t{item.IndexFileName}\t{item.Prefix}");
                    }

                if (Program.IsRunningInConsole)
                    server.Prefixes.Add($"http://localhost:{_serverPort}/");
                else
                    server.Prefixes.Add($"http://+:{_serverPort}/");

                /* Configure server to auto parse application/x-www-for-urlencoded data*/
                server.AutoParseFormUrlEncodedData();

                /* Configure Router Options (if supported by your router implementation) */
                server.Router.Options.SendExceptionMessages = true;
            }
        }
    }

    public static class MultiPartFormData
    {
        public async static Task Parse(IHttpContext context, IRestServer server)
        {
            if (string.IsNullOrWhiteSpace(context.Request.ContentType) || !context.Request.ContentType.StartsWith("multipart/form-data; boundary=")) return;
            context.Locals.TryAdd("FormData", await MultipartFormDataParser.ParseAsync(context.Request.InputStream, context.Request.MultipartBoundary, context.Request.ContentEncoding));
        }

        public static IRestServer AutoParseMultipartFormData(this IRestServer server)
        {
            server.OnRequestAsync -= MultiPartFormData.Parse;
            server.OnRequestAsync += MultiPartFormData.Parse;
            return server;
        }
    }

    internal static class HttpHelper
    {
        //public static string ReplaceSpecialChars(string inputString)
        //{
        //    string asAscii = Encoding.ASCII.GetString(
        //        Encoding.Convert(
        //            Encoding.UTF8,
        //            Encoding.GetEncoding(
        //                Encoding.ASCII.EncodingName,
        //                new EncoderReplacementFallback(string.Empty),
        //                new DecoderExceptionFallback()
        //                ),
        //            Encoding.UTF8.GetBytes(inputString)
        //        )
        //    );

        //    return asAscii.Replace(' ', '_');

        //}

        #region Feiertage

        // Aus VB konvertiert
        private static DateTime DateOsterSonntag(DateTime pDate)
        {
            int viJahr, viMonat, viTag;
            int viC, viG, viH, viI, viJ, viL;

            viJahr = pDate.Year;
            viG = viJahr % 19;
            viC = viJahr / 100;
            viH = (viC - viC / 4 - (8 * viC + 13) / 25 + 19 * viG + 15) % 30;
            viI = viH - viH / 28 * (1 - 29 / (viH + 1) * (21 - viG) / 11);
            viJ = (viJahr + viJahr / 4 + viI + 2 - viC + viC / 4) % 7;
            viL = viI - viJ;
            viMonat = 3 + (viL + 40) / 44;
            viTag = viL + 28 - 31 * (viMonat / 4);

            return new DateTime(viJahr, viMonat, viTag);
        }

        // Aus VB konvertiert
        internal static List<DateTime> Holydays(DateTime pDate)
        {
            int viJahr = pDate.Year;
            DateTime vdOstern = DateOsterSonntag(pDate);
            List<DateTime> feiertage = new List<DateTime>
            {
                new DateTime(viJahr, 1, 1),    // Neujahr
                new DateTime(viJahr, 5, 1),    // Erster Mai
                vdOstern.AddDays(-2),          // Karfreitag
                vdOstern.AddDays(1),           // Ostermontag
                vdOstern.AddDays(39),          // Himmelfahrt
                vdOstern.AddDays(50),          // Pfingstmontag
                new DateTime(viJahr, 10, 3),   // TagderDeutschenEinheit
                new DateTime(viJahr, 10, 31),  // Reformationstag
                new DateTime(viJahr, 12, 24),  // Heiligabend
                new DateTime(viJahr, 12, 25),  // Weihnachten 1
                new DateTime(viJahr, 12, 26),  // Weihnachten 2
                new DateTime(viJahr, 12, DateTime.DaysInMonth(viJahr, 12)) // Silvester
            };

            return feiertage;
        }

        internal static bool IsHolyday(DateTime date)
        {
            return Holydays(date).Contains(date);
        }

        #endregion


    }
}
