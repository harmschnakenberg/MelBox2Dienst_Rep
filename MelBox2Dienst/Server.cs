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
                    if (Environment.UserInteractive) //Start als Konsolenanwendung                    
                        System.Diagnostics.Process.Start("explorer", s.Prefixes.First().Replace("+", System.Net.Dns.GetHostName()));

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


    }
}
