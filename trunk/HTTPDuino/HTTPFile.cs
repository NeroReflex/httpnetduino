using System;
using System.IO;
using System.Text;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public class HTTPFile : IDisposable
    {
        private string path;

        public HTTPFile(string file)
        {
            //save the file path
            this.path = file;
        }

        public HTTPDuino.HTTPBinaryFile getBinaryFile()
        {
            HTTPDuino.HTTPBinaryFile binaryFile = new HTTPDuino.HTTPBinaryFile(this.path);
            return binaryFile;
        }

        public HTTPDuino.HTTPTextFile getTextFile()
        {
            HTTPDuino.HTTPTextFile textFile = new HTTPDuino.HTTPTextFile(this.path);
            return textFile;
        }

        public string getMIMEType()
        {
            string MIMEType = "";
            string extension = System.IO.Path.GetExtension(this.path).ToLower();
            switch (extension)
            {
                //web related
                case ".json":
                    MIMEType = "application/json";
                    break;
                case ".html":
                    MIMEType = "text/html";
                    break;
                case ".xhtml":
                    MIMEType = "application/xhtml+xml";
                    break;
                case ".js":
                    MIMEType = "text/javascript";
                    break;
                case ".css":
                    MIMEType = "text/css";
                    break;
                case ".xml":
                    MIMEType = "text/xml";
                    break;

                //documents related
                case ".txt":
                    MIMEType = "text/plain";
                    break;
                case ".rtf":
                    MIMEType = "text/rtf";
                    break;
                case ".vcard":
                case ".vcf":
                    MIMEType = "text/vcard";
                    break;
                case ".doc":
                    MIMEType = "text/plain";
                    break;
                case ".docx":
                    MIMEType = "application/msword";
                    break;
                case ".xls":
                    MIMEType = "text/vnd.ms-excel";
                    break;
                case ".xlsx":
                    MIMEType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".ppt":
                    MIMEType = "text/vnd.ms-powerpoint";
                    break;
                case ".pptx":
                    MIMEType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".kml":
                    MIMEType = "application/vnd.google-earth.kml+xml";
                    break;
                case ".kmz":
                    MIMEType = "application/vnd.google-earth.kmz";
                    break;
                case ".pdf":
                    MIMEType = "application/pdf";
                    break;

                //images related
                case ".jpg":
                    MIMEType = "image/jpeg";
                    break;
                case ".gif":
                    MIMEType = "image/gif";
                    break;
                case ".png":
                    MIMEType = "image/png";
                    break;
                case ".svg":
                    MIMEType = "image/svg+xml";
                    break;
                case ".ico":
                    MIMEType = "image/x-icon";
                    break;

                default:
                    MIMEType = "text/plain";
                    break;
            }
            return MIMEType;
        }

        #region IDisposable Members
        ~HTTPFile()
        {
            Dispose();
        }

        public void Dispose()
        {
            //delete the memory used to store the root path
            this.path = string.Empty;
            this.path = null;

            //force the garbage collector to free more memory as it can
            Microsoft.SPOT.Debug.GC(true);
        }
        #endregion
    }

}

