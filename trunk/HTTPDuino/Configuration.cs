using System;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public struct Configuration
    {
        public readonly Int16 Port;
        public readonly string RootPath;
        public bool IncludeMD5;
        public int SendTimeout;
        public int ReceiveTimeout;
        public string[] indexes;

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
            this.IncludeMD5 = false;
            this.SendTimeout = 500;
            this.ReceiveTimeout = 500;
        }
    }
}
