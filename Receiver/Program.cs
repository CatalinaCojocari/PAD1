using Common;
using Grpc.Net.Client;
using GrpcAgent;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Receiver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // dotnet dev-certs https--trust

           var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(EndpointsConstants.SubscribersAddress)
                .Build();

            // run ar bloca firul, dar start ar incepe sa asculte si putem face si alte sarcini
            host.Start();

            await Subscribe(host);

            //Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        // cerere catre broker, pastrarea in memorie a conexiunii date
        private static async Task Subscribe(IWebHost host)
        {
            var address = host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.First();
            Console.WriteLine($"Subscriber is listening at {address}");

            Console.Write("Enter the City you want to get the temperature: ");
            var topic = Console.ReadLine();

            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress(EndpointsConstants.BrokerAddress, new GrpcChannelOptions { HttpHandler = httpHandler });
            var client = new GrpcAgent.Subscriber.SubscriberClient(channel);

            var request = new SubscribeRequest() { Topic = topic, Address = address };

            try
            {
                var reply = await client.SubscribeAsync(request);
                Console.WriteLine($"Subscribed reply: {reply.IsSuccess}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error subscribing: {e.Message}");
            }
        }
    }
}
