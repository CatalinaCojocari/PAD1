using Broker.Services.Interfaces;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcAgent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Broker.Services
{
    // background worker services
    public class SenderWorker : IHostedService
    {
        private Timer _timer;
        private const int TimeToWait = 2000; // perioada de reapel a functiei DoSendWork
        private readonly IMessageStorageService _messageStorage;
        private readonly IConnectionStorageService _connectionStorage;
        private readonly HttpClientHandler _httpHandler;

        // ISSFactory ne ajuta sa gasim ulterior serviciile noastre injectate
        public SenderWorker(IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _messageStorage = scope.ServiceProvider.GetRequiredService<IMessageStorageService>();
                _connectionStorage = scope.ServiceProvider.GetRequiredService<IConnectionStorageService>();
            }

            _httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async o =>
            {
                await DoSendWork(cancellationToken);
            }, null, 0, TimeToWait);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // nu va mai rula DoSendWork-ul peste un infinit de timp
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        // logica de transmitere a mesajelor la subscriberi
        private async Task DoSendWork(object state)
        {
            while (!_messageStorage.IsEmpty())
            {
                var message = _messageStorage.GetNext();

                if (message != null)
                {
                    // luam conexiunile care trebuie sa primeasca acest message
                    var connections = _connectionStorage.GetConnectionsByTopic(message.Topic);

                    if (connections.Count == 0)
                    {
                         _messageStorage.Add(message);
                    }

                    foreach (var connection in connections)
                    {
                        var channel = GrpcChannel.ForAddress(connection.Address, new GrpcChannelOptions { HttpHandler = _httpHandler });
                        var client = new Notifier.NotifierClient(channel);
                        var request = new NotifyRequest() { City = message.Topic, Content = message.Content };

                        try
                        {
                            // la toate conexiunile va fi transmis mesajul
                            var reply = client.Notify(request);
                            Console.WriteLine($"Notified subscriber: {connection.Address} with {message.Content}. Response: {reply.IsSuccess}");
                        }
                        catch(RpcException rpcException)
                        {
                            if(rpcException.StatusCode == StatusCode.Internal || rpcException.StatusCode == StatusCode.Unavailable)
                            {
                                 _connectionStorage.Remove(connection.Address);
                            }

                            Console.WriteLine($"RPC Error notifying subscriber {connection.Address}. \nDetails: {rpcException.Status.Detail}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error notifying subscriber {connection.Address}. {e.Message}");
                        }
                        
                    }
                }
            }
        }
    }
}