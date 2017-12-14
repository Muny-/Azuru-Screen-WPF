using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Interop;

namespace ASU
{
    public class FrameClient : IStreamInput
    {
        public TcpClient Client;

        public IPConnectionProfile ConnectionProfile;

        public delegate void FrameReceivedEventHandler(object sender, FrameReceivedEventArgs e);
        public delegate void ReceivedStatisticsUpdateEventHandler(object sender, ReceivedStatisticsUpdateEventArgs e);

        public event EventHandler<StreamLostEventArgs> StreamLost;

        private string _streamdetails;

        public string StreamDetails
        {
            get
            {
                return _streamdetails;
            }
        }

        protected virtual void OnFrameReceived(FrameReceivedEventArgs e)
        {
            if (FrameReceived != null)
            {
                FrameReceived(this, e);
            }
        }

        protected virtual void OnReceivedStatisticsUpdate(ReceivedStatisticsUpdateEventArgs e)
        {
            if (ReceivedStatisticsUpdate != null)
            {
                ReceivedStatisticsUpdate(this, e);
            }
        }

        public BinaryReader br;
        public BinaryWriter bw;
        DispatcherTimer ReceivedStatisticsLoop;
        Thread ClientReadThread;

        public FrameClient(IPConnectionProfile profile)
        {
            this.ConnectionProfile = profile;
            _streamdetails = "ASU { ";

            if (profile.Name != "Most Recent")
                _streamdetails += profile.Name + " ";

            _streamdetails += profile.Address + ":" + profile.Port;

            _streamdetails += " }";

            ReceivedStatisticsLoop = new DispatcherTimer();
            ReceivedStatisticsLoop.Interval = TimeSpan.FromSeconds(1);
            ReceivedStatisticsLoop.Tick += ReceivedStatisticsLoop_Tick;
        }

        long bytes_received_counter = 0;

        void ReceivedStatisticsLoop_Tick(object sender, EventArgs e)
        {
            OnReceivedStatisticsUpdate(new ReceivedStatisticsUpdateEventArgs(bytes_received_counter));

            bytes_received_counter = 0;
        }

        public BitmapSource ReadImageBinaryReader()
        {
            int len = br.ReadInt32();

            bytes_received_counter += 4;

            byte[] imgbyt = br.ReadBytes(len);

            bytes_received_counter += len;

            return BitmapImageFromByteArray(imgbyt);
        }

        public void UpdateMouse(int x, int y, int mouse_action, int delta, int vkeycode)
        {
            /*if (mouse_action != 0 && mouse_action != 3)
                System.Windows.MessageBox.Show(mouse_action.ToString());*/
            if (bw != null)
            {
                try
                {
                    bw.Write(0);
                    bw.Write(x);
                    bw.Write(y);
                    bw.Write(mouse_action);
                    bw.Write(delta);
                    bw.Write(vkeycode);
                }
                catch
                {
                    
                }
            }
        }

        /*public BitmapImage BitmapImageFromByteArray(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }*/

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private static readonly ImageConverter _imageConverter = new ImageConverter();

        public static BitmapSource BitmapImageFromByteArray(byte[] bytes)
        {
            return GetImageFromByteArray(bytes).ToBitmapSource();
        }

        public static Bitmap GetImageFromByteArray(byte[] byteArray)
        {
            Bitmap bm = (Bitmap)_imageConverter.ConvertFrom(byteArray);

            if (bm != null && (bm.HorizontalResolution != (int)bm.HorizontalResolution ||
                               bm.VerticalResolution != (int)bm.VerticalResolution))
            {
                // Correct a strange glitch that has been observed in the test program when converting 
                //  from a PNG file image created by CopyImageToByteArray() - the dpi value "drifts" 
                //  slightly away from the nominal integer value
                bm.SetResolution((int)(bm.HorizontalResolution + 0.5f),
                                 (int)(bm.VerticalResolution + 0.5f));
            }

            return bm;
        }

        public event EventHandler<FrameReceivedEventArgs> FrameReceived;

        public event EventHandler<ReceivedStatisticsUpdateEventArgs> ReceivedStatisticsUpdate;

        bool waitForFrames = false;

        bool finishLoop = false;

        private class State
        {
            public TcpClient Client { get; set; }
            public bool Success { get; set; }
        }

        public TcpClient TcpConnect(string hostName, int port, int timeout)
        {
            var client = new TcpClient();

            //when the connection completes before the timeout it will cause a race
            //we want EndConnect to always treat the connection as successful if it wins
            var state = new State { Client = client, Success = true };

            IAsyncResult ar = client.BeginConnect(hostName, port, EndConnect, state);
            state.Success = ar.AsyncWaitHandle.WaitOne(timeout, false);

            if (!state.Success || !client.Connected)
                Lost("Failed to connect");

            return client;
        }

        void EndConnect(IAsyncResult ar)
        {
            var state = (State)ar.AsyncState;
            TcpClient client = state.Client;

            try
            {
                client.EndConnect(ar);
            }
            catch { }

            try
            {
                if (client.Connected && state.Success)
                    return;
            }
            catch
            {

            }

            client.Close();
        }

        public void Start()
        {
            if (ClientReadThread != null && ClientReadThread.ThreadState == ThreadState.Running)
                ClientReadThread.Abort();

            if (Client != null)
            {
                try
                {
                    Client.Close();
                }
                catch
                {

                }
            }

            ClientReadThread = new Thread(delegate()
            {
                try
                {
                    Client = TcpConnect(ConnectionProfile.Address, ConnectionProfile.Port, 10000);
                    //Client.Connect(Address, Port);

                    if (Client.Connected)
                    {

                        br = new BinaryReader(Client.GetStream());
                        bw = new BinaryWriter(Client.GetStream());

                        ReceivedStatisticsLoop.Start();
                        waitForFrames = true;


                        while (waitForFrames)
                        {
                            //MessageBox.Show(waitForFrames.ToString());
                            try
                            {
                                int cmd = br.ReadInt32();

                                switch (cmd)
                                {
                                    case 199071337:
                                    {
                                        BitmapSource img = ReadImageBinaryReader();



                                        //img.MakeTransparent(Color.FromArgb(0,0,0));

                                        img.Freeze();

                                        OnFrameReceived(new FrameReceivedEventArgs(img));



                                        /*if (lastFrame != null)
                                        {
                                            lastFrame = MergeTwoImages(lastFrame, img);
                                            OnFrameReceived(new FrameReceivedEventArgs(lastFrame));
                                        }
                                        else
                                        {
                                            OnFrameReceived(new FrameReceivedEventArgs(img));
                                            lastFrame = img;
                                        }*/


                                    }
                                    break;

                                    case 199071338:
                                    {
                                        BitmapSource img = ReadImageBinaryReader();
                                        OnFrameReceived(new FrameReceivedEventArgs(img));
                                    }
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Lost(ex.Message);
                                Stop();
                            }
                            finishLoop = true;

                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Thread was being aborted.")
                        Lost("Disconnected");
                    else
                        Lost(ex.Message);
                    Stop();
                }
            });

            try
            {
                ClientReadThread.Start();
            }
            catch (Exception ex)
            {
                Lost(ex.Message);
            }
            
        }

        void Lost(string reason)
        {
            if (StreamLost != null)
                StreamLost(this, new StreamLostEventArgs(reason));
        }

        public void Stop()
        {
            waitForFrames = false;

            try
            {
                if (Client != null)
                    Client.Close();
                Client = null;
            }
            catch
            {

            }
            
            try
            {
                if (br != null)
                    br.Close();
                if (bw != null)
                    bw.Close();
            }
            catch
            {

            }

            try
            {
                ClientReadThread.Abort();
            }
            catch
            {

            }

            if (ReceivedStatisticsLoop != null)
                ReceivedStatisticsLoop.Stop();

            finishLoop = false;
        }
    }
}
