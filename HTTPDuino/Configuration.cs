using System;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public struct Configuration
    {
        public readonly Int16 Port;
        public readonly string RootPath;
        public bool UseChunks;
        public int SendTimeout;
        public int ReceiveTimeout;
        public string[] indexes;
        public HTTPDuino.Routing[] routing;

        public Configuration(Int16 UserDefinedPort, string UserDefinedRoot)
        {
            if (UserDefinedPort <= 0)
                throw new Exception("Cannot use a zero or negative port");

            //store the minimal configuration of web server
            this.Port = UserDefinedPort;
            this.RootPath = UserDefinedRoot;

            //standard index pages
            this.indexes = new string[] { "index.html", "index.xhtml" };

            //setup some non-essential data to a default suggested value
            this.UseChunks = true;
            this.SendTimeout = 500;
            this.ReceiveTimeout = 500;

            //setup the basic routing
            this.routing = new HTTPDuino.Routing[1];
            this.routing[0] = new HTTPDuino.Routing("info", HTTPDuino.HTTPDuinoInfo.getInfo);
        }
    }
}
