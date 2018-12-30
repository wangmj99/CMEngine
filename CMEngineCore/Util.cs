using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using CMEngineCore.Models;
using IBApi;

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


        public static string PrintExecutionMsg(ExecutionMessage msg)
        {
            Execution exe = msg.Execution;
            return string.Format("executioID {0}, orderID {1}, side {2}, price {3}, qty {4}, reqeustID {5}", exe.ExecId, exe.OrderId, exe.Side, exe.Price, exe.Shares, msg.ReqId);
        }
    }
}
