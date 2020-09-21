using System;
using System.Threading.Tasks;
using KafkaFlow.Producers;

namespace KafkaFlow.AspnetCore.Producers
{
    public interface IPrintConsoleProducer
    {
        Task ProduceAsync<T>(T message);
    }

    public class PrintConsoleProducer 
        : IPrintConsoleProducer
    {
        private readonly IMessageProducer<PrintConsoleProducer> producer;

        public PrintConsoleProducer(IMessageProducer<PrintConsoleProducer> producer)
        {
            this.producer = producer;
        }

        public Task ProduceAsync<T>(T message)
        {
            return producer.ProduceAsync(Guid.NewGuid().ToString(), message);
        }

    }
}
