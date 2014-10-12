using System;
using System.IO;
using System.Text;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public class HTTPFile : IDisposable
    {
        private string filePath;
        public readonly System.IO.FileInfo fileInfo;
        private System.IO.StreamReader fileStream;
        private int currentPosition;

        public HTTPFile(string path)
        {
            //check the file existance
            if (!System.IO.File.Exists(path))
                throw new Exception("The file " + path + " doesn't exists");

            //store the file path
            this.filePath = path;

            //obtain info about the file
            this.fileInfo = new System.IO.FileInfo(this.filePath);

            //initialize the reader
            this.currentPosition = 0;
            this.fileStream = new StreamReader(this.filePath);
        }

        public string getMIMEType()
        {
            string MIMEType = "";
            string extension = this.fileInfo.Extension.ToLower();
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

        public int getChunkedBlock(ref byte[] bytes, ref int read)
        {
            //empty the byte container that will be filled by a portion of file
            bytes = null;

            //the buffer that will be filled with 50 (or less) chars 
            char[] buffer = new char[255];

            for (read = 0;((read < 255) && ((this.currentPosition + read) < this.fileInfo.Length)); read++)
                buffer[read] = (char)this.fileStream.Read();

            //convert read characters to a string
            string readCharacters = new string(buffer);
            readCharacters = read.ToString("X2") + "\r\n" + readCharacters + "\r\n";

            //convert the string to an UTF-8 array of data
            bytes = Encoding.UTF8.GetBytes(readCharacters);

            //update the current position
            this.currentPosition += read;

            //return the number of character read
            return readCharacters.Length;
        }

        public void getBlock(ref byte[] bytes, ref int read)
        {
            //empty the byte container that will be filled by a portion of file
            bytes = null;

            //the buffer that will be filled with 50 (or less) chars 
            MemoryStream buffer = new MemoryStream();

            for (read = 0; ((read < 255) && ((this.currentPosition + read) < this.fileInfo.Length)); read++)
            {
                byte[] data = BitConverter.GetBytes(this.fileStream.Read());
                buffer.Write(data, 0, data.Length);
            }
            //get the bytes
            buffer.Close();
            bytes = buffer.ToArray();
            buffer.Dispose();
            buffer = null;

            //update the current position
            this.currentPosition += read;
        }

        public bool endOfBlocks()
        {
            return (this.fileInfo.Length == this.currentPosition);
        }

        #region IDisposable Members
        ~HTTPFile()
        {
            Dispose();
        }

        public void Dispose()
        {
            //delete every file-related objects
            this.fileStream.Close();
            //this.fileStream = null;
            this.filePath = string.Empty;
            this.filePath = null;
            //this.fileInfo = null;
        }
        #endregion
    }
}
