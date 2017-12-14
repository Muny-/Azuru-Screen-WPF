using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using MjpegProcessor;

namespace ASU.SharingSources
{
    public class MJPEGStreamSharingSource : ISharingSource
    {
        public event EventHandler<NewFrameEventArgs> NewFrame;

        public event EventHandler<FPSUpdateEventArgs> FPSUpdate;

        protected virtual void OnNewFrame(NewFrameEventArgs e)
        {
            if (NewFrame != null)
            {
                NewFrame(this, e);
            }
        }

        protected virtual void OnFPSUpdate(FPSUpdateEventArgs e)
        {
            if (FPSUpdate != null)
            {
                FPSUpdate(this, e);
            }
        }

        public string SourceName
        {
            get { return "MJPEG Stream"; }
        }

        public bool PreviewRecommended
        {
            get { return true; }
        }

        MJPEGConnectionProfile ConnectionProfile;

        MJPEGStream stream;

        void FPSLoop_Tick(object sender, EventArgs e)
        {
            real_fps = fps_counter;

            OnFPSUpdate(new FPSUpdateEventArgs(real_fps));

            fps_counter = 0;
        }

        DispatcherTimer FPSLoop;

        int real_fps = 0;
        int fps_counter = 0;

        public void Start()
        {
            stream = new MJPEGStream();


            ShowSettings(null);

            stream.NewFrame += stream_NewFrame;
            try
            {
                stream.Start();

                FPSLoop = new DispatcherTimer();
                FPSLoop.Interval = TimeSpan.FromSeconds(1);
                FPSLoop.Tick += FPSLoop_Tick;
                FPSLoop.Start();
            }
            catch
            {

            }
        }

        void stream_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            fps_counter++;

            OnNewFrame(new NewFrameEventArgs((Bitmap)eventArgs.Frame));
        }

        public void Stop()
        {
            stream.Stop();

            if (FPSLoop != null)
                FPSLoop.Stop();
        }


        public void ShowSettings(Window parentWindow)
        {
            MJPEGProfileDialog md = new MJPEGProfileDialog();

            if (md.ShowDialog() == true)
            {
                ConnectionProfile = md.SelectedProfile;

                stream.ForceBasicAuthentication = ConnectionProfile.IsAuthenticated;
                stream.Login = ConnectionProfile.Username;
                stream.Password = ConnectionProfile.Password;
                stream.Source = ConnectionProfile.Address;
            }
            else
            {
                
            }
        }
    }
}
