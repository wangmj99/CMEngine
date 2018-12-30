using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace CMEngineCore
{
    public static class Util
    {
        public static void SerializeObject<T>(T serializableObject, string fileName)
        {
            var indented = Newtonsoft.Json.Formatting.Indented;
            var setting = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            string s = JsonConvert.SerializeObject(serializableObject, indented, setting);
            File.WriteAllText(fileName, s);
        }

        public static T DeSerializeObject<T>(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName))
            {
                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                JsonSerializer serializer = JsonSerializer.Create(settings);
                return (T)serializer.Deserialize(file, typeof(T));
            }
        }
    }
}
