using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Bix.Core
{
    internal class IPEndPointConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPEndPoint));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ipEndPoint = (IPEndPoint)value;
            var jObject = new JObject
            {
                { "Address", JToken.FromObject(ipEndPoint.Address, serializer) },
                { "Port", ipEndPoint.Port }
            };
            jObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var address = jObject["Address"].ToObject<IPAddress>(serializer);
            var port = (int)jObject["Port"];
            return new IPEndPoint(address, port);
        }
    }
}
