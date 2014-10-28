using System;
using Microsoft.SPOT;

namespace HTTPDuino
{
    /// <summary>
    /// Represents the configurtion of the http web server
    /// </summary>
    public struct Configuration
    {
        public readonly Int16 Port;
        public readonly string RootPath;
        public bool UseChunks;
        public int SendTimeout;
        public int ReceiveTimeout;
        public string[] indexes;
        public HTTPDuino.Routing[] routing;

        /// <summary>
        /// Creates a new configuration for the http web server
        /// </summary>
        /// <param name="UserDefinedPort">The port to be used by the server to listen for incoming connections</param>
        /// <param name="UserDefinedRoot">The root path on SD where the files that will be provided are stored</param>
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

        /// <summary>
        /// Stores a user defined routing
        /// </summary>
        /// <param name="routing">The routing to be stored and used by the server</param>
        public void AddRouting(HTTPDuino.Routing routing)
        {
            //if the length of the routing list doesn't exceed limits
            if (this.routing.Length < int.MaxValue)
            { //insert a new routing
                //create a copy of the list, but one element bigger
                HTTPDuino.Routing[] copy = new HTTPDuino.Routing[this.routing.Length + 1];
                
                //copy the list
                for (int i = 0; i < this.routing.Length; i++)
                    copy[i] = this.routing[i];

                //insert the new routing in the routing list
                copy[this.routing.Length] = routing;

                //store the new list
                this.routing = copy;
            }
            else
            {
                throw new Exception("Another route cannot be stored, max of route reached");
            }
            
        }

    }
}
