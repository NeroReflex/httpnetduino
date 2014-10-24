using System;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public struct Routing
    {
        private string RoutingName;
        public delegate string RoutingMethod(/*HTTPDuino.HTTPHeaderRequest request*/);
        public RoutingMethod RoutingFunction;


        public Routing(string routingInvoke, RoutingMethod UserFunction)
        {
            this.RoutingName = routingInvoke.ToLower();
            this.RoutingFunction = UserFunction;
        }
    }
}
