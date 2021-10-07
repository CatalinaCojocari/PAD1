using Common;
using Grpc.Net.Client;
using GrpcAgent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Publisher");
            //AppContext.SetSwitch("System.Net.Http.SocketsHandler.Http2UnencryptedSupport", true);

            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            // pentru a comunica cu service-ul de la broker si de a face apeluri la el
            var channel = GrpcChannel.ForAddress(EndpointsConstants.BrokerAddress, new GrpcChannelOptions { HttpHandler = httpHandler });
            var client = new Publisher.PublisherClient(channel);

            while (true)
            {
                Console.Write("\nEnter the City: ");
                var topic = Console.ReadLine();
                var temperatures = new Dictionary<Temperature, double>();
                //temperatures.Clear();
                Console.WriteLine("Enter the temperatures for each part of day: ");
                //var content = Console.ReadLine();

                foreach (Temperature temp in (Temperature[])Enum.GetValues(typeof(Temperature)))
                {
                    Console.Write($" - {temp}: ");
                    double value = Convert.ToDouble(Console.ReadLine());
                    temperatures.Add(temp, value);
                }
                var tempString = JsonConvert.SerializeObject(temperatures);
                var request = new PublishRequest() { Topic = topic, Content = tempString };

                try
                {
                    // async si await - firul de ex principal se executa, ajunge aici, pe un alt fir se face PublishMessageAsync si firul princ este liber (daca ar fi o aplicatie de UI, ar putea desena UI si nu ar bloca firul princ)
                    var reply = await client.PublishMessageAsync(request);
                    Console.WriteLine($"Publish Reply: {reply.IsSuccess}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error publishing the message {e.Message}");
                }
            }
        }
    }
}
