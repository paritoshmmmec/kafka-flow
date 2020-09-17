namespace KafkaFlow.Sample
{
    using System;
    using System.Threading.Tasks;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Hosting;
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
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        const string producerName = "PrintConsole";

                        const string consumerName = "test";

                        services.AddScoped<FakeDependency>();

                        services.AddKafka(
                            kafka => kafka
                                .UseLogHandler<FakeLogHandler>()
                                .AddCluster(
                                    cluster => cluster
                                        .WithBrokers(new[] { "localhost:9092" })
                                        .EnableAdminMessages("kafka-flow.admin", Guid.NewGuid().ToString())
                                        .AddProducer(
                                            producerName,
                                            producer => producer
                                                .DefaultTopic("test-topic")
                                                .AddMiddlewares(
                                                    middlewares => middlewares
                                                        .AddSerializer(
                                                            r => r.Resolve<ProtobufMessageSerializer>(),
                                                            r => r.Resolve<FakeResolver>())
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
                                                        .AddSerializer<ProtobufMessageSerializer>()
                                                        .AddTypedHandlers(
                                                            handlers => handlers
                                                                .WithHandlerLifetime(InstanceLifetime.Singleton)
                                                                .AddHandler<PrintConsoleHandler>())
                                                )
                                        )
                                )
                        );
                    })
                .UseDefaultServiceProvider(
                    options =>
                    {
                        options.ValidateScopes = true;
                        options.ValidateOnBuild = true;
                    });
    }

    public class FakeLogHandler : ILogHandler
    {
        private FakeDependency dependency;

        public FakeLogHandler(FakeDependency dependency)
        {
            this.dependency = dependency;
        }

        public void Error(string message, Exception ex, object data)
        {
        }

        public void Info(string message, object data)
        {
        }

        public void Warning(string message, object data)
        {
        }
    }

    public class FakeDependency
    {
    }

    public class FakeResolver : DefaultMessageTypeResolver
    {
        private readonly ILogHandler logHandler;

        public FakeResolver(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }
    }
}
