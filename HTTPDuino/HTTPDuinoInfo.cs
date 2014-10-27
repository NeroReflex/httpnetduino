using System;
using System.Text;
using System.Xml;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public class HTTPDuinoInfo
    {
        public static HTTPDuino.JToken getInfo()
        {
            HTTPDuino.JToken info = HTTPDuino.JsonHelpers.Parse("{\"version\":\"\", \"author\":\"\", \"license\":\"\", \"platform\":\"\", \"webServer\":\"HTTPDuino\"}");
            HTTPDuino.JArray data = (HTTPDuino.JArray)info;
            HTTPDuino.JObject dataCollection = (HTTPDuino.JObject)data[0];
            dataCollection["version"] = "0.1a";
            dataCollection = (HTTPDuino.JObject)data[1];
            dataCollection["author"] = "Benato Denis";
            dataCollection = (HTTPDuino.JObject)data[2];
            dataCollection["license"] = "BSD";
            dataCollection = (HTTPDuino.JObject)data[2];
            dataCollection["platform"] = "Netduino Plus 2";
            return info;
        }
    }
}
