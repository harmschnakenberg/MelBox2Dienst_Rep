//siehe auch https://michaeljohnpena.com/blog/namedpipes/

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Pipes;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MelBox2Dienst
//{
//    internal class Pipe
//    {

//        async Task TestAsync()
//        {
//            //Quelle: https://medium.com/@volkanalkilic/using-named-pipes-in-c-for-interprocess-communication-83d44b07be88



//            using (var pipeServer = new NamedPipeServerStream("my_pipe_name"))
//            {
//                Console.WriteLine("Waiting for client connection...");
//                //server.WaitForConnection();
//                await pipeServer.WaitForConnectionAsync();
//                Console.WriteLine("Client connected.");

//                // Read and write data through the pipe
//                using (StreamWriter sw = new StreamWriter(pipeServer))
//                {
//                    sw.AutoFlush = true;
//                    sw.WriteLine(Console.ReadLine());
//                }

//            }

//            using (var client = new NamedPipeClientStream(".", "my_pipe_name", PipeDirection.InOut))
//            {
//                Console.WriteLine("Connecting to server...");
//                client.Connect();
//                Console.WriteLine("Connected to server.");


//                // Read and write data through the pipe
//            }
//        }

//        /*++++++++++++++++++*/

//        void Test2()
//        {
//            StartServer();
//            Task.Delay(1000).Wait();


//            //Client
//            var client = new NamedPipeClientStream("PipesOfPiece");
//            client.Connect();
//            StreamReader reader = new StreamReader(client);
//            StreamWriter writer = new StreamWriter(client);

//            while (true)
//            {
//                string input = Console.ReadLine();
//                if (String.IsNullOrEmpty(input)) break;
//                writer.WriteLine(input);
//                writer.Flush();
//                Console.WriteLine(reader.ReadLine());
//            }
//        }

//        //Quelle: https://stackoverflow.com/questions/13806153/example-of-named-pipes
//        static void StartServer()
//        {
//            Task.Factory.StartNew(() =>
//            {
//                var server = new NamedPipeServerStream("PipesOfPiece");
//                server.WaitForConnection();
//                StreamReader reader = new StreamReader(server);
//                StreamWriter writer = new StreamWriter(server);
//                while (true)
//                {
//                    var line = reader.ReadLine();
//                    writer.WriteLine(String.Join("", line.Reverse()));
//                    writer.Flush();
//                }
//            });
//        }
//    }
//}

