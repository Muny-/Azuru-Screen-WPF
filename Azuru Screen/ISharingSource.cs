using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ASU
{
    public interface ISharingSource
    {
        event EventHandler<NewFrameEventArgs> NewFrame;

        event EventHandler<FPSUpdateEventArgs> FPSUpdate;

        string SourceName { get; }

        bool PreviewRecommended { get; }

        void Start();

        void Stop();

        void ShowSettings(System.Windows.Window ParentWindow);
    }

    public class NewFrameEventArgs : EventArgs
    {
        public Bitmap Frame { get; set; }

        public BitmapSource WPFFrame { get; set; }

        public IntPtr Handle { get; set; }

        public NewFrameEventArgs(Bitmap Frame)
        {
            this.Frame = Frame;
        }

        public NewFrameEventArgs(BitmapSource WPFFrame)
        {
            this.WPFFrame = WPFFrame;
        }

        public NewFrameEventArgs(IntPtr Handle)
        {
            this.Handle = Handle;
        }
    }

    public class FPSUpdateEventArgs : EventArgs
    {
        public int FPS { get; set; }

        public FPSUpdateEventArgs(int FPS)
        {
            this.FPS = FPS;
        }
    }
}
