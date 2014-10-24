using System;
using System.IO;
using System.Text;
using Microsoft.SPOT;

namespace HTTPDuino
{
    public class HTTPTextFile : IDisposable
    {
        private string filePath;
        public readonly System.IO.FileInfo fileInfo;
        private System.IO.StreamReader fileStream;
        private int currentPosition;

        public HTTPTextFile(string path)
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
            this.fileStream = new System.IO.StreamReader(this.filePath);
        }

        public int getChunkedBlock(ref byte[] bytes, ref int read)
        {
            //empty the byte container that will be filled by a portion of file
            bytes = null;

            //the buffer that will be filled with 1024 (or less) chars 
            char[] buffer = new char[1024];

            for (read = 0; ((read < 1024) && ((this.currentPosition + read) < this.fileInfo.Length)); read++)
                buffer[read] = (char)this.fileStream.Read();

            //convert read characters to a string
            string readCharacters = new string(buffer);
            readCharacters = read.ToString("X8") + "\r\n" + readCharacters + "\r\n";

            //convert the string to an UTF-8 array of data
            bytes = Encoding.UTF8.GetBytes(readCharacters);

            //update the current position
            this.currentPosition += read;

            //return the number of character read
            return readCharacters.Length;
        }

        public bool endOfBlocks()
        {
            return (this.fileInfo.Length == this.currentPosition);
        }

        #region IDisposable Members
        ~HTTPTextFile()
        {
            Dispose();
        }

        public void Dispose()
        {
            //delete every file-related objects
            this.fileStream.Close();
            this.fileStream.Dispose();
            this.filePath = string.Empty;
            this.filePath = null;

            //force the garbage collector to free more memory as it can
            Microsoft.SPOT.Debug.GC(true);
        }
        #endregion
    }
}
