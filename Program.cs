using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Reflection;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace ExternalLogger
{
    internal class main
    {
        public static ProxyServer ProxyServer;

        public int ListeningPort => ProxyServer.ProxyEndPoints[0].Port;

        public static bool download = false;

        public static List<string> list = new List<string>();

        public static void SetupProxy()
        {
            ProxyServer = new ProxyServer();
            var endpoint = new ExplicitProxyEndPoint(IPAddress.Any, 9999, true);
            ProxyServer.AddEndPoint(endpoint);

            ProxyServer.Start();

            foreach (var endPoint in ProxyServer.ProxyEndPoints)
                Console.WriteLine("proxy listening on {0}:{1}", endPoint.IpAddress, endPoint.Port);
            endpoint.BeforeTunnelConnectResponse += ProcessConnect;

            ProxyServer.SetAsSystemProxy(endpoint, ProxyProtocolType.AllHttp);
        }



        public static void Main(string[] args)
        {
            Console.WriteLine("simple external vrchat avatar logger by unixian");
            Console.WriteLine("if this is your first time launching, you will be prompted to install a root certificate.");
            Console.WriteLine("install it to allow the external logger to decrypt HTTPS data from vrchat.\n");

            bool provided_command = false;
            if (args.Length > 0)
            {
                var arg = args[0];

                switch (arg)
                {
                    case "-download":
                        provided_command = true;
                        download = true;
                        break;

                    case "-log":
                        provided_command = true;
                        break;

                    default:
                        Console.WriteLine("You provided an invalid command, the program will continue as normal.\n");
                        Console.WriteLine("Reminder: the only available commands is -download and -log.");
                        break;
                }
            }


            if (provided_command == false)
            {
                Console.WriteLine("Would you like to download all logged avatars into a folder?");
                Console.WriteLine($"Logged avatars will be saved to: \n{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA \n");
                Console.WriteLine($"If you want to start the program with one of these options, append -download or -log before starting the app.");
                Console.WriteLine("Enter a selection [y/n]: ");
                var key = Console.ReadLine();
                if (key.ToLower() == "y")
                {
                    download = true;
                }
                else if (key.ToLower() == "n")
                {
                    Console.WriteLine("\n\nWARNING: Due to VRChat making the links to VRCA files expire, these links may expire at any time meaning you will not be able to download them.");
                    download = false;
                }
            }



            SetupProxy();

            ProxyServer.BeforeRequest += ProcessRequest;
            ProxyServer.ServerCertificateValidationCallback += ProcessCertValidation;
            

            Console.WriteLine("\nproxy is now running, any avatars that you haven't cached will log/download. to exit anytime press the exit key.");

            Console.Read();

            Console.WriteLine("\ncleaning up...");
            Cleanup();
        }


        public static async Task ProcessRequest(object sender, SessionEventArgs e)
        {

            // s/o to angelcoder for telling me that titanium adds a header
            e.HttpClient.Request.Headers.RemoveHeader("Connection");

            string url = e.HttpClient.Request.RequestUri.AbsoluteUri;
            if (url.Contains("files.vrchat.cloud"))
            {
                if (url.Contains(".vrca"))
                {
                    if (!list.Contains(url))
                    {
                        list.Add(url);
                        var uri = e.HttpClient.Request.RequestUri;
                        var index = uri.Segments[1].IndexOf(".");


                        if (download)
                        {
                            if (!Directory.Exists($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA"))
                            {
                                Directory.CreateDirectory($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA");
                            }

                            var client = new HttpClient();
                            DownloadAvatar(uri, client, index);

                        }
                        else
                        {
                            if (!Directory.Exists($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA"))
                            {
                                Directory.CreateDirectory($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA");
                            }

                            if (!File.Exists($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA\\log.txt"))
                            {
                                File.CreateText($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA\\log.txt").Close();
                            }

                            using (StreamWriter w = File.AppendText($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA\\log.txt"))
                            {
                                await w.WriteLineAsync($"{System.Web.HttpUtility.UrlDecode(uri.AbsolutePath).Trim()} : {uri.Segments[1].Substring(0, index)}");
                                Console.WriteLine($"successfully logged avatar {uri.Segments[1].Substring(0, index)}");
                            }
                        }
                    }
                }
            }

        }


        private static async void DownloadAvatar(Uri uri, HttpClient client, int index)
        {
            var data = await client.GetByteArrayAsync(System.Web.HttpUtility.UrlDecode(uri.AbsoluteUri).Trim());
            await File.WriteAllBytesAsync($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA\\{uri.Segments[1]}", data);
            Console.WriteLine($"successfully logged avatar {uri.Segments[1].Substring(0, index)}");
        }

        public static Task ProcessCertValidation(object sender, CertificateValidationEventArgs e)
        {
            if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                e.IsValid = true;

            return Task.CompletedTask;
        }

        private static async Task ProcessConnect(object sender, TunnelConnectSessionEventArgs e)
        {
            string hostname = e.HttpClient.Request.RequestUri.Host;

            // this is to allow sites like google, youtube, mega, etc that use cert pinning to prevent MITM attacks.
            // solution does not fully work for browsers like firefox, and i can't see any info about fixing it.
            if (!hostname.Contains("api.vrchat.cloud"))
            {
                e.DecryptSsl = false;
            }
        }

        private static void WebSocketDataSentReceived(SessionEventArgs args, DataEventArgs e, bool sent)
        {
            foreach (var frame in args.WebSocketDecoder.Decode(e.Buffer, e.Offset, e.Count))
            {
                if (frame.OpCode == WebsocketOpCode.Binary)
                {
                    var data = frame.Data.ToArray();
                    string str = string.Join(",", data.ToArray().Select(x => x.ToString("X2")));
                    Console.WriteLine(str);
                }

                if (frame.OpCode == WebsocketOpCode.Text)
                {
                    Console.WriteLine(frame.GetText());
                }
            }
        }

        public static void Cleanup()
        {
            ProxyServer.BeforeRequest -= ProcessRequest;
            ProxyServer.RestoreOriginalProxySettings();
            ProxyServer.Stop();
            ProxyServer.Dispose();
        }

    }
}
