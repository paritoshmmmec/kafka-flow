using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KafkaFlow.AspnetCore.HostedServices;
using KafkaFlow.AspnetCore.Producers;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using KafkaFlow.Serializer.Json;
using KafkaFlow.Serializer.ProtoBuf;
using KafkaFlow.TypedHandler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaFlow.AspnetCore
{
    public class Startup
    {
        private const string Topic = "test-topic";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddKafka(c => c.AddCluster(BuildClusterConfiguration));

            services.AddHostedService<KafkaHostedService>();

        }

        private static void BuildClusterConfiguration(IClusterConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .WithBrokers(new[] {"localhost:9092"});

            configurationBuilder
                .AddProducer<PrintConsoleProducer>(BuildProducerConfiguration);

            configurationBuilder
                .AddConsumer(BuildConsumerConfiguration);
        }

        private static void BuildConsumerConfiguration(IConsumerConfigurationBuilder 
            configurationBuilder)
        {
            configurationBuilder
                .Topic(Topic)
                .WithGroupId("print-console-handler")
                .WithBufferSize(5)
                .WithWorkersCount(1)
                .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                .AddMiddlewares(m => m
                    .AddSerializer<JsonMessageSerializer>()
                    .AddTypedHandlers(
                        handlers => handlers
                            .WithHandlerLifetime(InstanceLifetime.Singleton)
                            .AddHandler<PrintConsoleHandler>())
                );
        }

        private static void BuildProducerConfiguration(IProducerConfigurationBuilder
            configurationBuilder)
        {
            configurationBuilder
                .DefaultTopic(Topic)
                .AddMiddlewares(middleware => middleware
                    .AddSerializer<JsonMessageSerializer>())
                .WithAcks(Acks.All);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
