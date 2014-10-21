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
    public class HTTPHeaderResponse : IDisposable
    {
        private ResponseType typeOfResponse;
        public string ContentType;
        public string ContentMD5;
        public long ContentLength;
        public bool ContentChunked;
        public bool ContentInline;
        public bool ConnectionClose;
        public bool GZip;

        public HTTPHeaderResponse(ResponseType type)
        {
            //store the minimal data needed to create a response
            this.typeOfResponse = type;
            this.ContentMD5 = string.Empty;
            this.ContentLength = -1;
            this.ContentInline = false;
            this.ConnectionClose = true;
            this.GZip = false;
        }

        public string Encode()
        {
            //build the http response header
            string headerText = "HTTP/1.1 ";

            switch (this.typeOfResponse)
            {
                case ResponseType.OK_200:
                    headerText += "200 OK";
                    break;
                
                case ResponseType.Not_Found_404:
                    headerText += "404 Not Found";
                    break;

                case ResponseType.Method_Not_Allowed_405:
                    headerText += "405 Method Not Allowed";
                    break;

                case ResponseType.Internal_Server_Error_500:
                    headerText += "500 Internal Server Error";
                    break;

                case ResponseType.HTTP_Version_Not_Supported_505:
                    headerText += "505 HTTP Version Not Supported";
                    break;

                default: //non ho tempo e voglia di scrivere tutti i millemila casi
                    headerText += "503 Service Unavailable";
                    break;
            }

            //insert standard info
            if (this.ContentType == string.Empty)
                this.ContentType = "text/plain";
            headerText += " \r\nServer: NETDuino\r\nX-Powered-By: HTTPDuino\r\nContent-Type: " + ContentType + "; charset=utf-8\r\n";

            //insert the content inline
            if (this.ContentInline)
                headerText += "Content-Disposition: inline;\r\n";

            //insert the content length (if specified)
            if (this.ContentLength > 0)
                headerText += "Content-Length: " + this.ContentLength.ToString() + "\r\n";
            else
                headerText += "Transfer-Encoding: chunked\r\n";

            if (this.GZip)
                headerText += "Content-Encoding: gzip\r\n";

            if (this.ConnectionClose)
                headerText += "Connection: close\r\n\r\n";
            else
                headerText += "Connection: keep-alive\r\n\r\n";

            return headerText;
        }

        #region IDisposable Members
        ~HTTPHeaderResponse()
        {
            Dispose();
        }

        public void Dispose()
        {
            //delete the md5
            this.ContentMD5 = string.Empty;
            this.ContentMD5 = null;

            //delete the content type
            this.ContentType = string.Empty;
            this.ContentType = null;
        }
        #endregion
    }

    public enum ResponseType
    {
        //Informational
        Continue_100,
        Switching_Protocols_101,

        //Successful
        OK_200,
        Created_201,
        Accepted_202,
        Non_Authoritative_Information_203,
        No_Content_204,
        Reset_Content_205,
        Partial_Content_206,

        //Redirection
        Multiple_Choices_300,
        Moved_Permanently_301,
        Found_302,
        See_Other_303,
        Not_Modified_304,
        Use_Proxy_305,
        Temporary_Redirect_307,

        //Client Error
        Bad_Request_400,
        Unauthorized_401,
        Payment_Required_402,
        Forbidden_403,
        Not_Found_404,
        Method_Not_Allowed_405,
        Not_Acceptable_406,
        Proxy_Authentication_Required_407,
        /* some useless stuff, see: http://www.w3.org/Protocols/rfc2616/rfc2616.html */

        //Server Error
        Internal_Server_Error_500,
        Not_Implemented_501,
        Bad_Gateway_502,
        Service_Unavailable_503,
        Gateway_Timeout_504,
        HTTP_Version_Not_Supported_505
    }


}
