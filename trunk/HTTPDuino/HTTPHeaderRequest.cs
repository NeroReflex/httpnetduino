using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using SecretLabs.NETMF.IO;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace HTTPDuino
{
    public class HTTPHeaderRequest : IDisposable
    {

        public RequestType type;
        public string resource;
        public string client;
        public bool GZIPCompatible;

        public HTTPHeaderRequest()
        {
            //initialize the header with empty values
            this.type = RequestType.GET;
            this.GZIPCompatible = false;
            this.resource = string.Empty;
            this.client = string.Empty;
            this.client = string.Empty;
        }

        public Int16 Decode(ref byte[] request)
        {
            //build the request string from received bytes
            string requestText = new string(Encoding.UTF8.GetChars(request));

            //check the request string (compare with the smallest header possible)
            if (requestText.Length < "get / HTTP/1.1\r\n".Length)
                return 1; //Invalid header

            //the character that i am currently parsing
            int i;

            //get the string-encoded method
            StringBuilder builder = new StringBuilder();
            for (i = 0; (i < requestText.Length) && (requestText[i] != ' ') && (requestText[i] != ';') && (requestText[i] != '/') && (requestText[i] != '\r') && (requestText[i] != '\n'); i++)
            {
                builder.Append(requestText[i]);
            }
            string temp = builder.ToString().ToUpper();
            
            //clean the string encoder for future use
            builder.Clear();

            //parse the request type
            if (string.Compare(temp, "GET") == 0)
                this.type = RequestType.GET;
            else if (string.Compare(temp, "POST") == 0)
                this.type = RequestType.POST;
            else if (string.Compare(temp, "HEAD") == 0)
                this.type = RequestType.HEAD;
            else if (string.Compare(temp, "PUT") == 0)
                this.type = RequestType.PUT;
            else if (string.Compare(temp, "DELETE") == 0)
                this.type = RequestType.DELETE;
            else if (string.Compare(temp, "TRACE") == 0)
                this.type = RequestType.TRACE;
            else if (string.Compare(temp, "OPTIONS") == 0)
                this.type = RequestType.OPTIONS;
            else if (string.Compare(temp, "CONNECT") == 0)
                this.type = RequestType.CONNECT;
            else
                this.type = RequestType.UNKNOWN;

            //get the starting position of the resource
            while (((requestText[i] == ' ') || (requestText[i] == '\r') || (requestText[i] == '\n')) && (i < requestText.Length))
                i++;

            //get the resource
            for (i = i; (i < requestText.Length) && (requestText[i] != ' ') && (requestText[i] != ';') && (requestText[i] != '\r') && (requestText[i] != '\n'); i++)
            {
                if (requestText[i] == '/')
                    builder.Append('\\');
                else
                    builder.Append(requestText[i]);
            }
            this.resource = builder.ToString();

            //clean the string encoder for future use
            builder.Clear();

            //get the starting position of the HTTP protocol
            while (((requestText[i] == ' ') || (requestText[i] == '\r') || (requestText[i] == '\n')) && (i < requestText.Length))
                i++;

            //get the HTTP protocol
            for (i = i; (i < requestText.Length) && (requestText[i] != ' ') && (requestText[i] != '\r') && (requestText[i] != '\n'); i++)
            {
                builder.Append(requestText[i]);
            }
            temp = builder.ToString().ToUpper();

            //check the used HTTP protocol
            if (temp != "HTTP/1.1")
                return 2; //Unsupported HTTP protocol

            //clean the string encoder for future use
            builder.Clear();

            //check if che client want a GZip encoded content
            if (requestText.ToLower().IndexOf("gzip") > 0)
                this.GZIPCompatible = true;
            else
                this.GZIPCompatible = false;

            //get the starting position of the client name (browser)
            int user_agent_index = requestText.IndexOf("User-Agent:");
            if (user_agent_index > 0)
            {
                i = user_agent_index + "User-Agent:".Length;
                while (((requestText[i] == ' ') || (requestText[i] == '\r') || (requestText[i] == '\n')) && (i < requestText.Length))
                    i++;

                //get the client name (browser)
                for (i = i; (i < requestText.Length) && (requestText[i] != '\r') && (requestText[i] != '\n'); i++)
                {
                    builder.Append(requestText[i]);
                }
                this.client = builder.ToString();

                //clean the string encoder for future use
                builder.Clear();
            }
            else
            {
                this.client = string.Empty;
            }

            

            return 0;
        }

        #region IDisposable Members
        ~HTTPHeaderRequest()
        {
            Dispose();
        }

        public void Dispose()
        {
            //delete the resource
            this.resource = string.Empty;
            this.resource = null;

            //delete the client info
            this.client = string.Empty;
            this.client = null;

            //force the garbage collector to free more memory as it can
            Microsoft.SPOT.Debug.GC(true);
        }
        #endregion
    }

    public enum RequestType
    {
        GET = 0,
        POST,
        HEAD,
        PUT,
        DELETE,
        TRACE,
        OPTIONS,
        CONNECT,
        UNKNOWN
    }
}
