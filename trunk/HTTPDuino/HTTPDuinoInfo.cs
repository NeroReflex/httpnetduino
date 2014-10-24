using System;
using System.Text;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public class HTTPDuinoInfo
    {
        public static string getInfo()
        {
            HTTPDuino.JToken info = HTTPDuino.JsonHelpers.Parse("{null, null, null, null, \"webServer\":\"HTTPDuino\"}");
            HTTPDuino.JArray data = (HTTPDuino.JArray)info;
            HTTPDuino.JObject dataCollection = (HTTPDuino.JObject)data[0];
            dataCollection["version"] = "0.1a";
            dataCollection = (HTTPDuino.JObject)data[1];
            dataCollection["author"] = "Benato Denis";
            dataCollection = (HTTPDuino.JObject)data[2];
            dataCollection["license"] = "BSD";
            dataCollection = (HTTPDuino.JObject)data[2];
            dataCollection["platform"] = "Netduino Plus 2";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            info.Serialize(sb);
            return sb.ToString();
        }
    }
}
