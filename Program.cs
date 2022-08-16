﻿using System;
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

namespace ExternalLogger
{
    internal class main
    {
        public static ProxyServer ProxyServer;

        public int ListeningPort => ProxyServer.ProxyEndPoints[0].Port;

        public static bool download = false;

        public static void SetupProxy()
        {
            ProxyServer = new ProxyServer();
            var endpoint = new ExplicitProxyEndPoint(IPAddress.Any, 9999, true);
            ProxyServer.AddEndPoint(endpoint);

            ProxyServer.Start();

            foreach (var endPoint in ProxyServer.ProxyEndPoints)
                Console.WriteLine("proxy listening on {0}:{1}", endPoint.IpAddress, endPoint.Port);
            endpoint.BeforeTunnelConnectResponse += ProcessConnect;

            ProxyServer.SetAsSystemHttpProxy(endpoint);
            ProxyServer.SetAsSystemHttpsProxy(endpoint);
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
                    download = false;
                }
            }



            SetupProxy();

            ProxyServer.BeforeRequest += ProcessRequest;
            ProxyServer.BeforeResponse += ProcessResponse;
            ProxyServer.ServerCertificateValidationCallback += ProcessCertValidation;

            Console.WriteLine("\nfinished init, program will run as normal. press the enter key to exit whenever.");

            Console.Read();

            Console.WriteLine("\ncleaning up...");
            Cleanup();
        }


        public static async Task ProcessRequest(object sender, SessionEventArgs e)
        {
            string url = e.HttpClient.Request.RequestUri.AbsoluteUri;
            if (!url.Contains("api.vrchat.cloud"))
            {
                return;
            }
        }

        public static async Task ProcessResponse(object sender, SessionEventArgs e)
        {
            string url = e.HttpClient.Request.RequestUri.AbsoluteUri;
            if (url.Contains("https://api.vrchat.cloud/api/1/file/") && e.HttpClient.Response.StatusCode == 302)
            {
                var download_link = e.HttpClient.Response.Headers.Headers["Location"];
                var ext = System.IO.Path.GetExtension(download_link.ToString());

                if (ext == ".vrca")
                {
                    var uri = new UriBuilder(download_link.ToString()).Uri;
                    var index = uri.Segments[3].IndexOf("Asset");
                    Console.WriteLine($"successfully logged avatar {uri.Segments[3].Substring(0, index)}");

                    if (download)
                    {
                        if (!Directory.Exists($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA"))
                        {
                            Directory.CreateDirectory($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA");
                        }

                        var client = new HttpClient();
                        DownloadAvatar(uri, client);
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
                            await w.WriteLineAsync($"{System.Web.HttpUtility.UrlDecode(uri.AbsolutePath).Trim()} : {uri.Segments[3].Substring(0, index)}");
                        }
                    }
                }
            }
        }

        private static async void DownloadAvatar(Uri uri, HttpClient client)
        {
            var data = await client.GetByteArrayAsync(System.Web.HttpUtility.UrlDecode(uri.AbsolutePath).Trim());
            await File.WriteAllBytesAsync($"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\VRCA\\{uri.Segments[3]}", data);
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

        public static void Cleanup()
        {
            ProxyServer.BeforeRequest -= ProcessRequest;
            ProxyServer.BeforeResponse -= ProcessResponse;
            ProxyServer.RestoreOriginalProxySettings();
            ProxyServer.Stop();
            ProxyServer.Dispose();
        }

    }
}
