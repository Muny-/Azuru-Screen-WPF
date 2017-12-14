using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using AForge.Video.DirectShow;

namespace ASU.SharingSources
{
    public class WebcamSharingSource : ISharingSource
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
            get { return "Webcam"; }
        }

        public bool PreviewRecommended
        {
            get { return true; }
        }

        VideoCaptureDevice capturingDevice;

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
            AForge.Video.DirectShow.VideoCaptureDeviceForm f = new AForge.Video.DirectShow.VideoCaptureDeviceForm();
            f.ShowDialog();

            capturingDevice = f.VideoDevice;

            f.VideoDevice.NewFrame += VideoDevice_NewFrame;
            f.VideoDevice.Start();

            FPSLoop = new DispatcherTimer();
            FPSLoop.Interval = TimeSpan.FromSeconds(1);
            FPSLoop.Tick += FPSLoop_Tick;
            FPSLoop.Start();
        }

        void VideoDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            fps_counter++;

            OnNewFrame(new NewFrameEventArgs(eventArgs.Frame));
        }

        public void Stop()
        {
            capturingDevice.Stop();

            FPSLoop.Stop();
        }


        public void ShowSettings(Window parentWindow)
        {
            capturingDevice.DisplayPropertyPage(new WindowInteropHelper(parentWindow).Handle);
        }
    }
}
