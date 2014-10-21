using System;
using System.IO;
using System.Text;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public class HTTPBinaryFile : IDisposable
    {
        private string filePath;
        private System.IO.FileStream fileBytes;
        private System.IO.FileInfo fileData;
        private long currentPosition;
        public bool endOfBlocks;

        public HTTPBinaryFile(string path)
        {
            //check the file existance
            if (!System.IO.File.Exists(path))
                throw new Exception("The file " + path + " doesn't exists");

            //store the file path
            this.filePath = path;

            //initialize the reader
            this.currentPosition = 0;
            this.endOfBlocks = false;

            //retrive file info
            this.fileData = new FileInfo(this.filePath);
        }

        public long getLength()
        {
            return this.fileData.Length;
        }

        public byte[] getBlock()
        {
            byte[] buffer = new byte[1024]; // 1kB buffer
            using (this.fileBytes = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                this.fileBytes.Position = this.currentPosition;
                int read = this.fileBytes.Read(buffer, 0, buffer.Length);
                if (read > 0)
                    this.currentPosition += read;
                else
                    this.endOfBlocks = true;
                this.fileBytes.Close();
                this.fileBytes.Dispose();
            }
            return buffer;
        }

        #region IDisposable Members
        ~HTTPBinaryFile()
        {
            Dispose();
        }

        public void Dispose()
        {
            //delete every file-related objects
            this.fileData = null;
            this.filePath = string.Empty;
            this.filePath = null;
        }
        #endregion
    }
}