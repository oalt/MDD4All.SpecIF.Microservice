using MDD4All.SpecIF.DataIntegrator.Contracts;
using MDD4All.SpecIF.DataModels;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataIntegrator.KafkaListener
{
    public class KafkaSpecIfEventService : ISpecIfEventService
    {

        private List<Resource> _receivedEvents = new List<Resource>();


        public KafkaSpecIfEventService(string serverAddress, string groupID)
        {
            KafkaSpecIfEventListener kafkaSpecIfEventListener = new KafkaSpecIfEventListener(serverAddress, groupID);

            SpecIfEventListener = kafkaSpecIfEventListener;

            kafkaSpecIfEventListener.SpecIfEventReceived += KafkaSpecIfEventListener_SpecIfEventReceived;

            kafkaSpecIfEventListener.StartListening();
        }

        public KafkaSpecIfEventListener SpecIfEventListener { get; private set; }

        private void KafkaSpecIfEventListener_SpecIfEventReceived(object sender, SpecIfEventArgs eventArguments)
        {
            _receivedEvents.Add(eventArguments.specIfEvent);
        }

        public List<Resource> GetReceivedSpecIfEvents()
        {
            return _receivedEvents;
        }
    }
}
