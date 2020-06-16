using Confluent.Kafka;
using MDD4All.Kafka.DataAccess;
using MDD4All.SpecIF.DataIntegrator.Contracts;
using MDD4All.SpecIF.DataModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MDD4All.SpecIF.DataIntegrator.KafkaListener
{
    public class KafkaSpecIfEventListener : AbstractSpecIfEventListener
    {

        private string _kafkaBootstrapServer = "localhost:9092";

        private string _groupID = "ea-consumer-group";

        public KafkaSpecIfEventListener()
        {

        }

        public KafkaSpecIfEventListener(string kafkaBootstrapServer, string groupID)
        {
            _kafkaBootstrapServer = kafkaBootstrapServer;
            _groupID = groupID;
        }


        public override void StartListening()
        {
            RunEventListenerAsync();
        }

        public override void StopListening()
        {
           
        }

        private async void RunEventListenerAsync()
        {
            ConsumerConfig consumerConfiguration = new ConsumerConfig
            {
                GroupId = _groupID,
                BootstrapServers = _kafkaBootstrapServer,

                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this example, consumption will only start from the
                // earliest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            IConsumer<Ignore, Resource> consumer = new ConsumerBuilder<Ignore, Resource>(consumerConfiguration)
                                                            .SetValueDeserializer(new KafkaJsonDeserializer<Resource>())
                                                            .Build();

            consumer.Subscribe("specif-events");

            await Task.Run(() =>
            {

                while (true)
                {
                    try
                    {
                        ConsumeResult<Ignore, Resource> cr = consumer.Consume();

                        //Debug.WriteLine("Receieved event");

                        Resource receivedEvent = cr.Message.Value;

                        if (receivedEvent != null)
                        {
                            OnSpecIfEventReveived(receivedEvent);
                        }
                    }
                    catch (ConsumeException consumeException)
                    {
                        Debug.WriteLine($"Error occured: {consumeException.Error.Reason}");
                    }
                }

            });

        }
    }
}
