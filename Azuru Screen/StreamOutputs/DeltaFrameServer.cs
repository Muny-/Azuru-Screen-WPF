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
using AForge.Imaging;


namespace ASU
{
    public class DeltaFrameServer : IStreamOutput
    {
        TcpListener listener;
        bool listening = false;

        private Thread HandleClientsThread;

        public List<ClientHandler> Clients = new List<ClientHandler>();


        int Quality = 30;

        int port;


        public DeltaFrameServer(IPAddress bindIP, int port)
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

        Bitmap oldBitmapFrame;
        IntPtr oldIntPtrFrame;

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

        public void SendFrame(IntPtr frame)
        {
            if (frame != null)
            {
                totalFrames++;

                SendIntPtr(frame);
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

        private void SendIntPtr(IntPtr frame)
        {
            UnmanagedImage img = new UnmanagedImage(frame, 0, 0, 0, PixelFormat.DontCare);

            (img.ToManagedImage(false)).Save("C:/Users/Kevin/Desktop/frame.png", ImageFormat.Png);
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
            get { return "ASU Delta Server"; }
        }


        public int ConnectedClients
        {
            get { return Clients.Count; }
        }


        public int? Port
        {
            get { return port; }
        }

        public static unsafe Bitmap GetDifferenceImage(Bitmap image1, Bitmap image2, Color matchColor)
        {
            if (image1 == null | image2 == null)
                return null;

            if (image1.Height != image2.Height || image1.Width != image2.Width)
                return null;

            Bitmap diffImage = image2.Clone() as Bitmap;

            int height = image1.Height;
            int width = image1.Width;

            BitmapData data1 = image1.LockBits(new Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData data2 = image2.LockBits(new Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData diffData = diffImage.LockBits(new Rectangle(0, 0, width, height),
                                                   ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            byte* data1Ptr = (byte*)data1.Scan0;
            byte* data2Ptr = (byte*)data2.Scan0;
            byte* diffPtr = (byte*)diffData.Scan0;

            byte[] swapColor = new byte[3];
            swapColor[0] = matchColor.B;
            swapColor[1] = matchColor.G;
            swapColor[2] = matchColor.R;

            int rowPadding = data1.Stride - (image1.Width * 3);

            // iterate over height (rows)
            for (int i = 0; i < height; i++)
            {
                // iterate over width (columns)
                for (int j = 0; j < width; j++)
                {
                    int same = 0;

                    byte[] tmp = new byte[3];

                    // compare pixels and copy new values into temporary array
                    for (int x = 0; x < 3; x++)
                    {
                        tmp[x] = data2Ptr[0];
                        if (data1Ptr[0] == data2Ptr[0])
                        {
                            same++;
                        }
                        data1Ptr++; // advance image1 ptr
                        data2Ptr++; // advance image2 ptr
                    }

                    // swap color or add new values
                    for (int x = 0; x < 3; x++)
                    {
                        diffPtr[0] = (same == 3) ? swapColor[x] : tmp[x];
                        diffPtr++; // advance diff image ptr
                    }
                }

                // at the end of each column, skip extra padding
                if (rowPadding > 0)
                {
                    data1Ptr += rowPadding;
                    data2Ptr += rowPadding;
                    diffPtr += rowPadding;
                }
            }

            image1.UnlockBits(data1);
            image2.UnlockBits(data2);
            diffImage.UnlockBits(diffData);

            return diffImage;
        }


        public void SendFrame(BitmapSource frame)
        {
            throw new NotImplementedException();
        }
    }
}
