using System;
using System.IO;
using System.Text;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public class HTTPBinaryFile : IDisposable
    {
        private string filePath;
        private System.IO.StreamReader fileBytes;
        private int currentPosition;

        public HTTPBinaryFile(string path)
        {
            //check the file existance
            if (!System.IO.File.Exists(path))
                throw new Exception("The file " + path + " doesn't exists");

            //store the file path
            this.filePath = path;

            //initialize the reader
            this.currentPosition = 0;
            //this.fileBytes = System.IO.File.OpenWrite(this.filePath);
            this.fileBytes = new StreamReader(this.filePath);
        }

        public long getLength()
        {
            return this.fileBytes.BaseStream.Length;
        }

        public string getBlock(ref int read)
        {
            //empty the byte container that will be filled by a portion of file
            char[] buffer = new char[255];

            MemoryStream memory = new MemoryStream();

            read = 0;
            while ((read < 255) && ((this.currentPosition + read) < this.fileBytes.BaseStream.Length)/*this.fileBytes.EndOfStream*/)
            {
                buffer[read] = (char)this.fileBytes.Read();
                //memory.Write(buffer, 0, buffer.Length);
                read++;
            }

            memory.Close();/*
            bytes = memory.ToArray();

            char[] mychars = Encoding.UTF8.GetChars(bytes);
            */
            string dbg = new string(buffer);
            Debug.Print(dbg);

            return dbg;
        }

        public bool endOfBlocks()
        {
            return (this.fileBytes.BaseStream.Length == this.currentPosition);
        }

        #region IDisposable Members
        ~HTTPBinaryFile()
        {
            Dispose();
        }

        public void Dispose()
        {
            //delete every file-related objects
            this.fileBytes.Close();
            this.fileBytes.Dispose();
            this.filePath = string.Empty;
            this.filePath = null;
        }
        #endregion
    }
}