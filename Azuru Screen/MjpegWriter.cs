using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Net.Sockets;

namespace ASU
{

    /// <summary>
    /// Provides a stream writer that can be used to write images as MJPEG 
    /// or (Motion JPEG) to any stream.
    /// </summary>
    public class MjpegWriter : IDisposable
    {

        public event EventHandler<EventArgs> ClientDisconnected;

        public Socket Sock;

        private static byte[] CRLF = new byte[] { 13, 10 };
        private static byte[] EmptyLine = new byte[] { 13, 10, 13, 10 };

        public MjpegWriter(Socket sock)
            : this(new NetworkStream(sock, true), "--boundary")
        {
            this.Sock = sock;
        }

        public MjpegWriter(Stream stream, string boundary)
        {
            this.Stream = stream;
            this.Boundary = boundary;
        }

        public string Boundary { get; private set; }
        public Stream Stream { get; private set; }

        public void WriteHeader()
        {

            Write(
                    "HTTP/1.1 200 OK\r\n" +
                    "Content-Type: multipart/x-mixed-replace; boundary=" +
                    this.Boundary +
                    "\r\n"
                 );

            this.Stream.Flush();
        }

        public void WriteFrame(byte[] img)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine();
                sb.AppendLine(this.Boundary);
                sb.AppendLine("Content-Type: image/jpeg");
                sb.AppendLine("Content-Length: " + img.Length.ToString());
                sb.AppendLine();

                Write(sb.ToString());
                Write(img);
                Write("\r\n");

                this.Stream.Flush();

                
            }
            catch
            {
                this.Dispose();
            }

        }        

        private void Write(byte[] data)
        {
            this.Stream.Write(data, 0, data.Length);
        }

        private void Write(string text)
        {
            byte[] data = BytesOf(text);
            try
            {
                this.Stream.Write(data, 0, data.Length);
            }
            catch
            {
                this.Dispose();
            }
        }

        private static byte[] BytesOf(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        public string ReadRequest(int length)
        {

            byte[] data = new byte[length];
            int count = this.Stream.Read(data, 0, data.Length);

            if (count != 0)
                return Encoding.ASCII.GetString(data, 0, count);

            return null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (ClientDisconnected != null)
                ClientDisconnected(this, new EventArgs());

            try
            {

                if (this.Stream != null)
                    this.Stream.Dispose();

            }
            finally
            {
                this.Stream = null;
            }
        }

        #endregion
    }
}
