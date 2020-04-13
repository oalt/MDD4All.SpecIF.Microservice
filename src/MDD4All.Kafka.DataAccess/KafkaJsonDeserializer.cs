using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;

namespace MDD4All.Kafka.DataAccess
{
    public class KafkaJsonDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            T result = default(T);

            if(!isNull)
            {
                try
                {
                    byte[] dataBytes = data.ToArray();

                    string json = Encoding.UTF8.GetString(dataBytes);
                    
                    result = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
                                                                    {
                                                                        NullValueHandling = NullValueHandling.Ignore
                                                                    });
                }
                catch(Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }

            return result;
        }
    }
}
