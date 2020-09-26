using Autofac;
using KafkaFlow.Autofac.DependencyInjection;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer.NewtonsoftJson;

namespace KafkaFlow.Sample
{
    using System;
    using System.Threading.Tasks;
    using global::Microsoft.Extensions.DependencyInjection;
    using KafkaFlow.Admin;
    using KafkaFlow.Admin.Messages;
    using KafkaFlow.Compressor;
    using KafkaFlow.Compressor.Gzip;
    using KafkaFlow.Consumers;
    using KafkaFlow.Producers;
    using KafkaFlow.Serializer;
    using KafkaFlow.Serializer.ProtoBuf;
    using KafkaFlow.TypedHandler;

    internal static class Program
    {
        private static async Task Main()
        {
            var services = new ServiceCollection();

            const string producerName = "PrintConsole";

            const string consumerName = "test";

            var containerBuilder = new ContainerBuilder();

            var configurator = new KafkaFlowConfigurator(
                    // Install KafkaFlow.Unity package
                    new AutofacDependencyConfigurator(containerBuilder),
                    kafka => kafka
                       .AddCluster(
                        cluster => cluster
                            .WithBrokers(new[] { "host.docker.internal:32769" })
                            .EnableAdminMessages("kafka-flow.admin", Guid.NewGuid().ToString())
                            .AddProducer(
                                producerName,
                                producer => producer
                                    .DefaultTopic("test-topic")
                                    .AddMiddlewares(
                                        middlewares => middlewares
                                            .AddSerializer<NewtonsoftJsonMessageSerializer>()
                                            .AddCompressor<GzipMessageCompressor>()
                                    )
                                    .WithAcks(Acks.All)
                            )
                            .AddConsumer(
                                consumer => consumer
                                    .Topic("test-topic")
                                    .WithGroupId("print-console-handler")
                                    .WithName(consumerName)
                                    .WithBufferSize(100)
                                    .WithWorkersCount(20)
                                    .WithAutoOffsetReset(AutoOffsetReset.Latest)
                                    .AddMiddlewares(
                                        middlewares => middlewares
                                            .AddCompressor<GzipMessageCompressor>()
                                            .AddSerializer<NewtonsoftJsonMessageSerializer>()
                                            .AddTypedHandlers(
                                                handlers => handlers
                                                    .WithHandlerLifetime(InstanceLifetime.Singleton)
                                                    .AddHandler<PrintConsoleHandler>())
                                    )
                            )
                    )
                );

            var container = containerBuilder.Build();

            var bus = configurator.CreateBus(new AutofacDependencyResolver(container));

            await bus.StartAsync();

            var producer1 = container.Resolve<IProducerAccessor>();

            producer1[producerName].Produce(Guid.NewGuid().ToString(), new TestMessage()
            { 
                Text = "Test"
            });

            Console.WriteLine("Hello");
            Console.ReadLine();
            await bus.StopAsync();
        }
    }
}
