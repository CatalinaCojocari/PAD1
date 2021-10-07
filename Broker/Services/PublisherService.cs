using Broker.Models;
using Broker.Services.Interfaces;
using Grpc.Core;
using GrpcAgent;
using System;
using System.Threading.Tasks;

namespace Broker.Services
{
    public class PublisherService : Publisher.PublisherBase
    {
        private readonly IMessageStorageService _messageStorage;

        // atunci cand se creeaza o instanta la PublisherService, va fi injectat un IMessageStorageService
        public PublisherService(IMessageStorageService messageStorageService)
        {
            _messageStorage = messageStorageService;
        }

        
        public override Task<PublishReply> PublishMessage(PublishRequest request, ServerCallContext context)
        {
            try
            {
                System.Console.WriteLine($"Received: {request.Topic} {request.Content}");

                var message = new Message(request.Topic, request.Content);
                _messageStorage.Add(message); // cand vine in PM o cerere/se publica un mesaj nou, il adaugam in storage
                // mereu se va crea un PService nou, iar MSService va fi cel vechi, mereu se va injecta tot acelasi MStorage

                return Task.FromResult(new PublishReply()
                {
                    IsSuccess = true
                });
            }
            catch(Exception e)
            {
                Console.WriteLine($"Publishing gRPC error message: {e.Message}");
                return Task.FromResult(new PublishReply
                {
                    IsSuccess = false
                });
            }
        }
    }
}