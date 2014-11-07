using System;
using Microsoft.SPOT;

namespace HTTPDuino
{

    /// <summary>
    /// Represents a routing: a function to be called when the client uses the given string as a GET request
    /// </summary>
    public struct Routing
    {
        public string RoutingName;
        public delegate HTTPDuino.MicroJSON.JSON RoutingMethod(/*HTTPDuino.HTTPHeaderRequest request*/);
        public RoutingMethod RoutingFunction;


        /// <summary>
        /// Invoke a method which returns a JSON when the client request the given string in a GET request
        /// </summary>
        /// <param name="routingInvoke">The string that is used to invoke the method</param>
        /// <param name="UserFunction">the method that will be invoked</param>
        public Routing(string routingInvoke, RoutingMethod UserFunction)
        {
            //store the string and the method to be invoked
            this.RoutingName = routingInvoke.ToLower();
            this.RoutingFunction = UserFunction;
        }
    }
}
