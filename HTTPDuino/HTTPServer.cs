using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using SecretLabs.NETMF.IO;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Microsoft.SPOT.Net.NetworkInformation;

namespace HTTPDuino
{
    public class HTTPServer : IDisposable
    {
        private Socket socket = null;
        private HTTPDuino.Configuration serverConfiguration;
        //open connection to onbaord led so we can blink it with every request
        public HTTPServer(HTTPDuino.Configuration configuration)
        {
            //Save the current configuration
            this.serverConfiguration = configuration;
            
            //Initialize Socket class
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Request and bind to an IP from DHCP server
            socket.Bind(new IPEndPoint(IPAddress.Any, this.serverConfiguration.Port));
            Debug.Print(Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress);

            //Start listen for web requests
            socket.Listen(15);
            //onRequest();
        }

        public void Start()
        {
            while (true)
            {
                using (Socket clientSocket = socket.Accept())
                {
                    //Get clients IP
                    IPEndPoint clientIP = clientSocket.RemoteEndPoint as IPEndPoint;
                    EndPoint clientEndPoint = clientSocket.RemoteEndPoint;
                    int bytesReceived = clientSocket.Available;
                    if (bytesReceived > 0)
                    {
                        //set the receive timeout
                        clientSocket.ReceiveTimeout = this.serverConfiguration.ReceiveTimeout;

                        //get the HTTP request
                        byte[] buffer = new byte[bytesReceived];
                        int byteCount = clientSocket.Receive(buffer, bytesReceived, SocketFlags.None);

                        //create the decoder
                        HTTPDuino.HTTPHeaderRequest browserRequest = new HTTPHeaderRequest();

                        //decode the request
                        if (browserRequest.Decode(ref buffer) == 0)
                        {
                            string getRequest = browserRequest.resource;

                            //the resource to be send back to the client
                            bool fileFound = false;
                            string requestedResource = "\\SD" + this.serverConfiguration.RootPath;

                            //check if the SD card is mounted
                            VolumeInfo[] volumes = VolumeInfo.GetVolumes();
                            bool deviceFound = false;
                            foreach (VolumeInfo volumeInfo in volumes)
                            {
                                if (volumeInfo.Name.Equals("SD"))
                                    deviceFound = true;
                            }
                            volumes = null;
                            if (browserRequest.type == HTTPDuino.RequestType.GET)
                            {
                                //check the file or transmit a 500
                                if (deviceFound)
                                {
                                    //check if an index page is required
                                    if (getRequest[getRequest.Length - 1] == '\\')
                                        for (int j = 0; j < this.serverConfiguration.indexes.Length; j++)
                                            if (System.IO.File.Exists(requestedResource + getRequest + this.serverConfiguration.indexes[j]))
                                            {
                                                getRequest += this.serverConfiguration.indexes[j];
                                                break;
                                            }

                                    //check the file/page/script existance
                                    if (System.IO.File.Exists(requestedResource + getRequest))
                                    {
                                        requestedResource += getRequest;
                                        fileFound = true;
                                    }

                                    //transmit the file or a 404
                                    if (fileFound)
                                    {
                                        //should the server send script.min.gzip.js?
                                        bool gzip = false;
                                        string[] fileShrinkedInfo = requestedResource.Split('.');

                                        //if a gzip version of the file might exists.....
                                        if ((fileShrinkedInfo.Length >= 3) && (browserRequest.GZIPCompatible))
                                        {
                                            if (fileShrinkedInfo[fileShrinkedInfo.Length - 2].ToLower() == "min")
                                            {
                                                //is the file gzippable?
                                                bool gzippable = false;

                                                //file extension
                                                string extension = fileShrinkedInfo[fileShrinkedInfo.Length - 1].ToLower();

                                                //check if the file can be gzzipped
                                                if ((extension == "js") || (extension == "css") || (extension == "html"))
                                                    gzippable = true;

                                                //generate its name
                                                string gzipFile = string.Empty;

                                                if (gzippable)
                                                    for (int i = 0; i < fileShrinkedInfo.Length; i++)
                                                    {
                                                        gzipFile += fileShrinkedInfo[i];
                                                        if (i != (fileShrinkedInfo.Length - 1))
                                                            gzipFile += ".";
                                                        if (fileShrinkedInfo[i].ToLower() == "min")
                                                            gzipFile += "gzip.";
                                                    }
                                                
                                                //check wether the gzippeed file is requested and existant
                                                if ((System.IO.File.Exists(gzipFile)) && (gzippable))
                                                {
                                                    gzip = true;
                                                    requestedResource = gzipFile;
                                                }
                                            }
                                        }

                                        //transmit the file
                                        HTTPDuino.HTTPHeaderResponse response = new HTTPHeaderResponse(ResponseType.OK_200);
                                        HTTPDuino.HTTPFile file = new HTTPFile(requestedResource);
                                        response.ContentType = file.getMIMEType();

                                        Debug.Print("Page served: " + requestedResource);

                                        if ((string.Compare(response.ContentType.Split('/')[0].ToLower(), "text") == 0) && (this.serverConfiguration.UseChunks) && (!gzip))
                                        {
                                            //create the file chunketizer
                                            HTTPDuino.HTTPTextFile content = file.getTextFile();

                                            //the page will be send chunked
                                            response.ContentChunked = true;
                                            response.ConnectionClose = true;

                                            //set the send timeout
                                            clientSocket.SendTimeout = this.serverConfiguration.SendTimeout;

                                            //send the header
                                            string Header = response.Encode();
                                            clientSocket.Send(Encoding.UTF8.GetBytes(Header), Header.Length, SocketFlags.None);

                                            //send the file in chunks
                                            while (!content.endOfBlocks())
                                            {
                                                //the chunk and its length
                                                byte[] toSend = null;
                                                int read = 0;

                                                //get a single chunk of data, formatted as it should be
                                                int lengthOfSocketMessage = content.getChunkedBlock(ref toSend, ref read);

                                                //send the chunk of data
                                                //if (!(clientSocket.Poll(this.serverConfiguration.SendTimeout, SelectMode.SelectWrite)) && (clientSocket.Available == 0))
                                                clientSocket.Send(toSend, lengthOfSocketMessage, SocketFlags.None);
                                                //else
                                                //    break;
                                            }

                                            //send the end of the page
                                            string endOfChunks = "00\r\n\r\n";

                                            //if (!(clientSocket.Poll(this.serverConfiguration.SendTimeout, SelectMode.SelectWrite)) && (clientSocket.Available == 0))
                                            clientSocket.Send(Encoding.UTF8.GetBytes(endOfChunks), endOfChunks.Length, SocketFlags.None);

                                            //destroy the object
                                            content.Dispose();
                                        }
                                        else
                                        {
                                            //create the file reader
                                            HTTPDuino.HTTPBinaryFile content = file.getBinaryFile();

                                            //specify if the page is gzipped
                                            response.GZip = gzip;

                                            //get the number of bytes to be sent
                                            long fileLength = content.getLength();

                                            //the page will be sent splitted, but the server will provide its length
                                            response.ContentLength = fileLength;
                                            response.ContentInline = true;
                                            response.ConnectionClose = false;

                                            //set the send timeout
                                            clientSocket.SendTimeout = this.serverConfiguration.SendTimeout;

                                            //send the header
                                            string Header = response.Encode();
                                            clientSocket.Send(Encoding.UTF8.GetBytes(Header), Header.Length, SocketFlags.None);

                                            //send the file splitted in 1kB block or less
                                            while (!content.endOfBlocks)
                                            {
                                                byte[] data = content.getBlock();

                                                //send the buffer
                                                //if (!(clientSocket.Poll(this.serverConfiguration.SendTimeout, SelectMode.SelectWrite)) && (clientSocket.Available == 0))
                                                    clientSocket.Send(data, data.Length, SocketFlags.None);
                                                //else
                                                //    break;
                                            }
                                        }
                                    }
                                    else
                                    {//transmit a 404 Not Found
                                        //the 404 HTML page
                                        string NotFoundPageText = "<!doctype html><html lang=\"en\"><head><title>404 Not Found</title><!--[if lt IE 9]><script src=\"http://html5shiv.googlecode.com/svn/trunk/html5.js\"></script><![endif]--></head>"
                                        + "<body style='background: #E0FFFF'><h2 style='color: #191970; text-align: center;'>404 Not Found</h2>"
                                        + "<p style='text-align: center;'>The requested file cannot be found on this server. If the problem persists, please, contact the system administrator.</p>"
                                        + "<hr>HTTPDuino 0.1a Web Server (2014), Netduino from " + Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress + ""
                                        + "</body></html>";
                                        byte[] NotFoundPage = Encoding.UTF8.GetBytes(NotFoundPageText);

                                        //the 404 MD5 page
                                        HTTPDuino.MD5 MD5Provider = new HTTPDuino.MD5();
                                        MD5Provider.Initialize();
                                        MD5Provider.HashCore(NotFoundPage, 0, NotFoundPage.Length);
                                        MD5Provider.HashFinal();
                                        string NotFoundMD5 = MD5Provider.HexStr();

                                        //the 404 header
                                        HTTPDuino.HTTPHeaderResponse NotFoundHeader = new HTTPHeaderResponse(ResponseType.Not_Found_404);
                                        NotFoundHeader.ContentType = "text/html";
                                        NotFoundHeader.ContentLength = NotFoundPageText.Length;
                                        NotFoundHeader.ContentMD5 = NotFoundMD5;
                                        string NotFound = NotFoundHeader.Encode();

                                        //set the send timeout
                                        clientSocket.SendTimeout = this.serverConfiguration.SendTimeout;

                                        //send the header
                                        clientSocket.Send(Encoding.UTF8.GetBytes(NotFound), NotFound.Length, SocketFlags.None);

                                        //send the HTML page
                                        clientSocket.Send(NotFoundPage, NotFoundPageText.Length, SocketFlags.None);
                                    }
                                }
                                else
                                {//transmit a 500 Internal Server Error
                                    string InternalErrorText = "<!doctype html><html lang=\"en\"><head><title>500 Internal Server Error</title><!--[if lt IE 9]><script src=\"http://html5shiv.googlecode.com/svn/trunk/html5.js\"></script><![endif]--></head>"
                                        + "<body style='background: #E0FFFF'><h2 style='color: #191970; text-align: center;'>500 Internal Server Error</h2>"
                                        + "<p style='text-align: center;'>The request cannot be processed: probably the device containing the requested file is unmounted or has been removed. If the problem persists, please, contact the system administrator.</p>"
                                        + "<hr>HTTPDuino 0.1a Web Server (2014), Netduino from " + Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress + ""
                                        + "</body></html>";
                                    byte[] InternalErrorPage = Encoding.UTF8.GetBytes(InternalErrorText);

                                    //the 500 MD5 page
                                    HTTPDuino.MD5 MD5Provider = new HTTPDuino.MD5();
                                    MD5Provider.Initialize();
                                    MD5Provider.HashCore(InternalErrorPage, 0, InternalErrorPage.Length);
                                    MD5Provider.HashFinal();
                                    string NotFoundMD5 = MD5Provider.HexStr();

                                    //the 500 header
                                    HTTPDuino.HTTPHeaderResponse InternalErrorHeader = new HTTPHeaderResponse(ResponseType.Internal_Server_Error_500);
                                    InternalErrorHeader.ContentType = "text/html";
                                    InternalErrorHeader.ContentLength = InternalErrorText.Length;
                                    InternalErrorHeader.ContentMD5 = NotFoundMD5;
                                    string InternalError = InternalErrorHeader.Encode();

                                    //set the send timeout
                                    clientSocket.SendTimeout = this.serverConfiguration.SendTimeout;

                                    //send the header
                                    clientSocket.Send(Encoding.UTF8.GetBytes(InternalError), InternalError.Length, SocketFlags.None);

                                    //send the HTML page
                                    clientSocket.Send(InternalErrorPage, InternalErrorText.Length, SocketFlags.None);
                                }
                            }
                            else
                            {//transmit a 405 Method Not Allowed
                                //the 405 HTML page
                                string MethodNotAllowedPageText = "<!doctype html><html lang=\"en\"><head><title>405 Method Not Allowed</title><!--[if lt IE 9]><script src=\"http://html5shiv.googlecode.com/svn/trunk/html5.js\"></script><![endif]--></head>"
                                + "<body style='background: #E0FFFF'><h2 style='color: #191970; text-align: center;'>405 Method Not Allowed</h2>"
                                + "<p style='text-align: center;'>The request cannot be processed by this server. If the problem persists, please, contact the system administrator.</p>"
                                + "<hr>HTTPDuino 0.1a Web Server (2014), Netduino from " + Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress + ""
                                + "</body></html>";
                                byte[] MethodNotAllowedPage = Encoding.UTF8.GetBytes(MethodNotAllowedPageText);

                                //the 405 MD5 page
                                HTTPDuino.MD5 MD5Provider = new HTTPDuino.MD5();
                                MD5Provider.Initialize();
                                MD5Provider.HashCore(MethodNotAllowedPage, 0, MethodNotAllowedPage.Length);
                                MD5Provider.HashFinal();
                                string NotFoundMD5 = MD5Provider.HexStr();

                                //the 405 header
                                HTTPDuino.HTTPHeaderResponse MethodNotAllowedHeader = new HTTPHeaderResponse(ResponseType.Method_Not_Allowed_405);
                                MethodNotAllowedHeader.ContentType = "text/html";
                                MethodNotAllowedHeader.ContentLength = MethodNotAllowedPageText.Length;
                                MethodNotAllowedHeader.ContentMD5 = NotFoundMD5;
                                string MethodNotAllowed = MethodNotAllowedHeader.Encode();

                                //set the send timeout
                                clientSocket.SendTimeout = this.serverConfiguration.SendTimeout;

                                //send the header
                                clientSocket.Send(Encoding.UTF8.GetBytes(MethodNotAllowed), MethodNotAllowed.Length, SocketFlags.None);

                                //send the HTML page
                                clientSocket.Send(MethodNotAllowedPage, MethodNotAllowedPageText.Length, SocketFlags.None);
                            }
                        }
                        else
                        {//transmit a 505 HTTP Version Not Supported
                            //the 505 HTML page
                            string VersionNotSupportedPageText = "<!doctype html><html lang=\"en\"><head><title>505 HTTP Version Not Supported</title><!--[if lt IE 9]><script src=\"http://html5shiv.googlecode.com/svn/trunk/html5.js\"></script><![endif]--></head>"
                            + "<body style='background: #E0FFFF'><h2 style='color: #191970; text-align: center;'>505 HTTP Version Not Supported</h2>"
                            + "<p style='text-align: center;'>The request cannot be processed by this server because the HTTP version is not recognized. If the problem persists, please, change your browser.</p>"
                            + "<hr>HTTPDuino 0.1a Web Server (2014), Netduino from " + Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress + ""
                            + "</body></html>";
                            byte[] VersionNotSupportedPage = Encoding.UTF8.GetBytes(VersionNotSupportedPageText);

                            //the 505 MD5 page
                            HTTPDuino.MD5 MD5Provider = new HTTPDuino.MD5();
                            MD5Provider.Initialize();
                            MD5Provider.HashCore(VersionNotSupportedPage, 0, VersionNotSupportedPage.Length);
                            MD5Provider.HashFinal();
                            string NotFoundMD5 = MD5Provider.HexStr();

                            //the 505 header
                            HTTPDuino.HTTPHeaderResponse VersionNotSupportedHeader = new HTTPHeaderResponse(ResponseType.HTTP_Version_Not_Supported_505);
                            VersionNotSupportedHeader.ContentType = "text/html";
                            VersionNotSupportedHeader.ContentLength = VersionNotSupportedPageText.Length;
                            VersionNotSupportedHeader.ContentMD5 = NotFoundMD5;
                            string VersionNotSupported = VersionNotSupportedHeader.Encode();

                            //set the send timeout
                            clientSocket.SendTimeout = this.serverConfiguration.SendTimeout;

                            //send the header
                            clientSocket.Send(Encoding.UTF8.GetBytes(VersionNotSupported), VersionNotSupported.Length, SocketFlags.None);

                            //send the HTML page
                            clientSocket.Send(VersionNotSupportedPage, VersionNotSupportedPageText.Length, SocketFlags.None);
                        }
                    }

                    //close the connection
                    clientSocket.Close();
                }
            }
        }

        #region IDisposable Members
        ~HTTPServer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (socket != null)
                this.socket.Close();
        }
        #endregion

    }

}
