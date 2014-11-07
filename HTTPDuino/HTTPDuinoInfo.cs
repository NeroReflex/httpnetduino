using System;
using System.Text;
using System.Xml;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public class HTTPDuinoInfo
    {
        public static HTTPDuino.MicroJSON.JSON getInfo()
        {
            HTTPDuino.MicroJSON.JSON info = new HTTPDuino.MicroJSON.JSON();
            info.AppendEntity("name", 54);
            return info;
        }
    }
}
