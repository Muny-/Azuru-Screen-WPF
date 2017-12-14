using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ASU
{
    public class MJPEGServer : IStreamOutput
    {
        public event EventHandler<ControlPacketReceivedEventArgs> ControlPacketReceived;

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        public event EventHandler<ClientDisonnectedEventArgs> ClientDisconnected;

        public string OutputName
        {
            get { return "MJPEG Server"; }
        }

        int Quality = 30;

        public bool IsRunning { get { return (ServerThread != null && ServerThread.IsAlive); } }

        private List<Socket> Clients;
        private List<MjpegWriter> Client_Writers;
        private Thread ServerThread;

        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
        EncoderParameters encoderParams = new EncoderParameters(1);

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private int port;
        private IPAddress BindIP;

        public MJPEGServer(IPAddress bindIP, int port)
        {
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality);
            this.port = port;
            this.BindIP = bindIP;
            Clients = new List<Socket>();
            Client_Writers = new List<MjpegWriter>();
            ServerThread = null;
        }

        public void Start()
        {
            ServerThread = new Thread(ServerHandler);
            ServerThread.IsBackground = true;
            ServerThread.Start();
        }

        public void Stop()
        {
            if (this.IsRunning)
            {
                try
                {
                    try
                    {
                        Server.Dispose();
                        //Server.Shutdown(SocketShutdown.Both);
                        //Server.Disconnect(false);
                    }
                    catch
                    {

                    }
                    lock (Clients)
                    {

                        foreach (var s in Clients)
                        {
                            try
                            {
                                s.Close();
                            }
                            catch { }
                        }
                        Clients.Clear();

                    }

                    ServerThread.Abort();
                    ServerThread.Join();

                }
                finally
                {





                    ServerThread = null;
                }
            }
        }

        Socket Server;

        private void ServerHandler()
        {

            try
            {
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Server.Bind(new IPEndPoint(BindIP, port));
                Server.Listen(10);

                System.Diagnostics.Debug.WriteLine(string.Format("Server started on port {0}.", port));

                foreach (Socket client in Server.IncommingConnectoins())
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), client);
                }

                Server.Dispose();

            }
            catch { }



            this.Stop();
        }

        /// <summary>
        /// Each client connection will be served by this thread.
        /// </summary>
        /// <param name="client"></param>
        private void ClientThread(object client)
        {
            
            Socket socket = (Socket)client;

            System.Diagnostics.Debug.WriteLine(string.Format("New client from {0}", socket.RemoteEndPoint.ToString()));

            

            lock (Clients)
                Clients.Add(socket);

            OnClientConnected(new ClientConnectedEventArgs(new ClientHandler()));

            MjpegWriter writer = new MjpegWriter(socket);

            lock (Client_Writers)
                Client_Writers.Add(writer);

            writer.ClientDisconnected += writer_ClientDisconnected;

            try
            {

                writer.WriteHeader();

                while (socket.Connected)
                {
                    byte[] buf = new byte[1024];
                    socket.Receive(buf);
                }
            }
            catch
            {
                try
                {
                    socket.Disconnect(false);
                }
                catch
                {

                }
            }
            finally
            {
                

                lock (Clients)
                    Clients.Remove(socket);

                OnClientDisonnected(new ClientDisonnectedEventArgs(new ClientHandler()));

                
            }
        }

        void writer_ClientDisconnected(object sender, EventArgs e)
        {
            MjpegWriter writer = (MjpegWriter)sender;

            try
            {
                Clients.Remove(writer.Sock);
                Client_Writers.Remove(writer);

                writer = null;

            }
            catch
            {

            }
            
        }

        private static byte[] CRLF = new byte[] { 13, 10 };
        private static byte[] EmptyLine = new byte[] { 13, 10, 13, 10 };

        public string Boundary { get { return "--boundary"; } }

        public void SendFrame(BitmapSource frame)
        {
            byte[] img = BitmapSourceToByteArray(frame);

            try
            {

                foreach (MjpegWriter writer in Client_Writers)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object nul) {
                        if (writer.Stream != null)
                            writer.WriteFrame(img);
                    }));
                    
                }
            }
            catch
            {

            }

            //img = null;

            //GC.Collect();
        }

        public byte[] BitmapSourceToByteArray(BitmapSource img)
        {
            try
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = Quality;

                byte[] bit = null;
                using (MemoryStream stream = new MemoryStream())
                {

                    encoder.Frames.Add(BitmapFrame.Create(img));
                    encoder.Save(stream);
                    bit = stream.ToArray();
                    stream.Close();
                }

                return bit;
            }
            catch
            {

            }
            return null;
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, jpgEncoder, encoderParams);
            return ms.ToArray();
        }

        public void SendFrame(System.Drawing.Bitmap frame)
        {
            byte[] img = imageToByteArray(frame);

            try
            {

                foreach (MjpegWriter writer in Client_Writers)
                {
                    writer.WriteFrame(img);
                }
            }
            catch
            {

            }

            frame.Dispose();
            img = null;
        }


        public int ConnectedClients
        {
            get { return Clients.Count; }
        }

        protected virtual void OnControlPacketReceived(ControlPacketReceivedEventArgs e)
        {
            if (ControlPacketReceived != null)
            {
                ControlPacketReceived(this, e);
            }
        }

        protected virtual void OnClientConnected(ClientConnectedEventArgs e)
        {
            if (ClientConnected != null)
            {
                ClientConnected(this, e);
            }
        }

        protected virtual void OnClientDisonnected(ClientDisonnectedEventArgs e)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, e);
            }
        }


        int? IStreamOutput.Port
        {
            get { return port; }
        }
    }

    static class SocketExtensions
    {
        public static IEnumerable<Socket> IncommingConnectoins(this Socket server)
        {
            while (true)
                yield return server.Accept();
        }

    }
}
