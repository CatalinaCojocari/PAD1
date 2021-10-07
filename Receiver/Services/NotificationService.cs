using Common;
using Grpc.Core;
using GrpcAgent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Receiver.Services
{
    public class NotificationService : Notifier.NotifierBase
    {
        public override Task<NotifyReply> Notify(NotifyRequest request, ServerCallContext context)
        {
            var temperatures = JsonConvert.DeserializeObject<Dictionary<Temperature, double>>(request.Content);

            if (String.Equals(request.City, "Chisinau"))
            {
                System.Console.WriteLine($"Received: {request.City}, capital of Republic of Moldova.");
                foreach (var temp in temperatures)
                {
                    Console.WriteLine($" - {temp.Key}: {temp.Value}");
                }
            }
            else
            {
                System.Console.WriteLine($"Received: {request.City}");
                foreach (var temp in temperatures)
                {
                    Console.WriteLine($" - {temp.Key}: {temp.Value}");
                }
            }

           
            return Task.FromResult(new NotifyReply() { IsSuccess = true });
        }
    }
}