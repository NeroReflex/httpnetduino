using System;
using System.Text;
using System.Xml;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public class HTTPDuinoInfo
    {
        /// <summary>
        /// The name of the application that is using this web server to share information
        /// </summary>
        public static string ApplicationName = string.Empty;

        /// <summary>
        /// Returns a JSON cointaining information about the HTTP web server
        /// </summary>
        /// <returns>information stored as a JSON</returns>
        public static HTTPDuino.MicroJSON.JSON getInfo()
        {
            //create the empty JSON that will be filled
            HTTPDuino.MicroJSON.JSON info = new HTTPDuino.MicroJSON.JSON();
            
            //server related information
            info.AppendEntity("server", "HTTPNetDuino");
            info.AppendEntity("version", "0.1a");
            info.AppendEntity("project-home", "https://code.google.com/p/httpnetduino/");
            info.AppendEntity("author", "Benato Denis");

            //usage related information
            info.AppendEntity("host", Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress);
            info.AppendEntity("application", HTTPDuinoInfo.ApplicationName);

            //returns information
            return info;
        }
    }
}
