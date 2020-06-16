using GalaSoft.MvvmLight;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataModels.Manipulation;

namespace MDD4All.SpecIF.ViewModels.SpecIfEvent
{
    public class SpecIfEventViewModel : ViewModelBase
    {
        private Resource _specifEvent;

        private ISpecIfMetadataReader _metadataReader;

        

        public SpecIfEventViewModel(Resource specifEvent, ISpecIfMetadataReader specIfMetadataReader)
        {
            _specifEvent = specifEvent;
            _metadataReader = specIfMetadataReader;
        }

        public string Timestamp
        {
            get
            {
                return _specifEvent.ChangedAt.ToString();
            }
        }

        public string Origin
        {
            get
            {
                return _specifEvent.GetPropertyValue("SpecIF:Origin", _metadataReader);
            }
        }

        public string EventType
        {
            get
            {
                string result = "";

                result = _specifEvent.GetPropertyValue("SpecIF:specifEventType", _metadataReader);

                return result;
            }
        }

        public string ID
        {
            get
            {
                return _specifEvent.GetPropertyValue("SpecIF:id", _metadataReader);
            }
        }

        public string Revision
        {
            get
            {
                return _specifEvent.GetPropertyValue("SpecIF:revision", _metadataReader);
            }
        }

        public string Class
        {
            get
            {
                string result = _specifEvent.GetPropertyValue("SpecIF:classId", _metadataReader);

                return result;
            }
        }
    }
}
