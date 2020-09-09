#if NET461 || NETSTANDARD
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TigerGraph.Models
{
    public class TupleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Dictionary<string, object>) == objectType;
        
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var results = new Dictionary<string, object>();
            foreach (var a in JArray.Load(reader))
            {
                var o = (JObject) a;
                var k = o.Properties().First().Name;
                var v = o[k].Value<JValue>().Value;
                results.Add(k, v);
            }
            return results;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
#endif