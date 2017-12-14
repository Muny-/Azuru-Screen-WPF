using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
namespace ASU
{
    public class FrameServer : IStreamOutput
    {
        TcpListener listener;
        bool listening = false;

        private Thread HandleClientsThread;

        public List<ClientHandler> Clients = new List<ClientHandler>();


        int Quality = 30;

        int port;


        public FrameServer(IPAddress bindIP, int port)
        {
            this.port = port;
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality);

            listener = new TcpListener(bindIP, port);
        }

        public void Start()
        {
            listening = true;
            listener.Start();

            /*new Thread(delegate()
            {
                while(listening)
                {
                    if (FrameQueue.Count > 0)
                    {
                        Bitmap frame = FrameQueue.Dequeue();

                        totalFrames++;

                        //Bitmap[] tiles = new Bitmap[16];

                        /*int wid = frame.Width/10;
                        int height = frame.Height/10;

                        for (int y = 0; y < 10; y++ )
                        {
                            for(int x = 0; x < 10; x++)
                            {
                                Point point = new Point(wid * x, height * y);
                                Size size = new Size(wid, height);
                                Rectangle bounds = new Rectangle(point, size);
                                Bitmap tile = frame.Clone(bounds, PixelFormat.DontCare);
                                SendBitmap(tile);
                            }
                        }*/

                        /*if (frame != null)
                            SendBitmap(frame);

                        //frame.Dispose();
                    }
                    else
                        Thread.Sleep(1);
                }
            }).Start();*/

            HandleClientsThread = new Thread(delegate()
            {
                while (listening)
                {
                    try
                    {
                        ClientHandler client = new ClientHandler(listener.AcceptTcpClient(), (int a, int b, int c, int d, int e) => { this.UpdateMouse(a, b, c, d, e); });
                        Clients.Add(client);
                        OnClientConnected(new ClientConnectedEventArgs(client));
                        client.ClientDisonnected += client_ClientDisonnected;
                    }
                    catch
                    {

                    }
                    
                }
            });

            HandleClientsThread.Start();
        }

        void client_ClientDisonnected(object sender, ClientDisonnectedEventArgs e)
        {
            Clients.Remove(e.Client);
            OnClientDisonnected(e);
        }

        public void Stop()
        {
            List<ClientHandler> tmpLst = new List<ClientHandler>(Clients);
            foreach (ClientHandler c in tmpLst)
            {
                c.Disconnect();
            }

            listening = false;
            listener.Stop();

            

            HandleClientsThread.Abort();
        }

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

        int totalFrames = 0;

        public void SendFrame(Bitmap frame)
        {
            if (frame != null)
            {
                /*if (FrameQueue.Count < 10)
                    FrameQueue.Enqueue(frame);
                else
                    frame.Dispose();*/

                totalFrames++;

                //Bitmap[] tiles = new Bitmap[16];

                /*int wid = frame.Width/10;
                int height = frame.Height/10;

                for (int y = 0; y < 10; y++ )
                {
                    for(int x = 0; x < 10; x++)
                    {
                        Point point = new Point(wid * x, height * y);
                        Size size = new Size(wid, height);
                        Rectangle bounds = new Rectangle(point, size);
                        Bitmap tile = frame.Clone(bounds, PixelFormat.DontCare);
                        SendBitmap(tile);
                    }
                }*/
                SendBitmap(frame);

                frame.Dispose();
            }
        }

        public void SendFrame(BitmapSource frame)
        {
            if (frame != null)
            {
                totalFrames++;

                SendBitmapSource(frame);
            }
        }

        private void SendBitmap(Bitmap bmp)
        {
            byte[] img = imageToByteArray(bmp);

            if (img != null)
            {
                try
                {
                    foreach (ClientHandler client in Clients)
                    {
                        try
                        {
                            client.BW.Write(199071337);
                            client.BW.Write(img.Length);
                            client.BW.Write(img);
                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {

                }

                img = null;
            }
        }

        private void SendBitmapSource(BitmapSource img)
        {
            try
            {
                byte[] imgbyt = BitmapSourceToByteArray(img);


                foreach (ClientHandler client in Clients)
                {
                    try
                    {
                        client.BW.Write(199071337);
                        client.BW.Write(imgbyt.Length);
                        client.BW.Write(imgbyt);
                    }
                    catch
                    {

                    }
                }

                imgbyt = null;
            }
            catch
            {

            }
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

        public void UpdateMouse(int x, int y, int mouse_action, int delta, int vkeycode)
        {
            OnControlPacketReceived(new ControlPacketReceivedEventArgs(x, y, mouse_action, delta, vkeycode));
        }

        public event EventHandler<ControlPacketReceivedEventArgs> ControlPacketReceived;

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        public event EventHandler<ClientDisonnectedEventArgs> ClientDisconnected;

        public string OutputName
        {
            get { return "ASU Server"; }
        }


        public int ConnectedClients
        {
            get { return Clients.Count; }
        }


        public int? Port
        {
            get { return port; }
        }
    }
}
