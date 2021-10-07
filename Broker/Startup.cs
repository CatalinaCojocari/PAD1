using Broker.Services;
using Broker.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Broker
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();

            //addSingleton - permite sa avem o singura instanta a acestui serviciu per toata aplicatia, adica daca o sa injectam acest servicu in alte sevicii, acets serviciu o sa fie unicul.
            // acest lucru este necesar pentru ca atunci cand facem un request la PublisherService, unele servicii se pot recrea mereu: la fiecare request o sa fie creat un serviciu nou, o sa se creeze alt storage 
            services.AddSingleton<IMessageStorageService, MessageStorageService>();
            services.AddSingleton<IConnectionStorageService, ConnectionStorageService>();
            services.AddHostedService<SenderWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // daca cream un serviciu nou, adugam un proto, il definim, facem un build, apoi in Servicies cream implementarea pentru serviciu si deja aici in startup adaugam serviciul sa il mapeze ca acest endpoint sa fie accesibil
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<PublisherService>();
                endpoints.MapGrpcService<SubscriberService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
