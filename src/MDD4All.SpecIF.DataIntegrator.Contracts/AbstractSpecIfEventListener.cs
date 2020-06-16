using MDD4All.SpecIF.DataModels;
using System;

namespace MDD4All.SpecIF.DataIntegrator.Contracts
{
    public abstract class AbstractSpecIfEventListener
    {
        public event EventHandler<SpecIfEventArgs> SpecIfEventReceived;

        protected virtual void OnSpecIfEventReveived(Resource specifEvent)
        {
            SpecIfEventReceived?.Invoke(this, new SpecIfEventArgs { specIfEvent = specifEvent });
        }

        public abstract void StartListening();

        public abstract void StopListening();
    }
}
