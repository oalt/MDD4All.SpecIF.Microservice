using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

namespace MDD4All.Kafka.DataAccess
{
    public class KafkaJsonSerializer<T> : ISerializer<T>
    {
        
        public byte[] Serialize(T data, SerializationContext context)
        {
            string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                                                            {
                                                                NullValueHandling = NullValueHandling.Ignore
                                                            });
            
            return Encoding.UTF8.GetBytes(json);

        }

       
    }
}
