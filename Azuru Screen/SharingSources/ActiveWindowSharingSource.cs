using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ASU.SharingSources
{
    public class ActiveWindowSharingSource : ISharingSource
    {

        public event EventHandler<NewFrameEventArgs> NewFrame;
        public event EventHandler<FPSUpdateEventArgs> FPSUpdate;

        private bool isStreaming = false;
        private int fps_counter = 0;


        protected virtual void OnNewFrame(NewFrameEventArgs e)
        {
            if (NewFrame != null)
                NewFrame(this, e);
        }

        protected virtual void OnFPSUpdate(FPSUpdateEventArgs e)
        {
            if (FPSUpdate != null)
                FPSUpdate(this, e);
        }

        public string SourceName
        {
            get { return "ActiveWindow"; }
        }

        public void Start()
        {
            isStreaming = true;

            FPSLoop = new DispatcherTimer();
            FPSLoop.Interval = TimeSpan.FromSeconds(1);
            FPSLoop.Tick += FPSLoop_Tick;
            FPSLoop.Start();

            SleepTimeUpdate = new DispatcherTimer();
            SleepTimeUpdate.Interval = TimeSpan.FromMilliseconds(1);
            SleepTimeUpdate.Tick += SleepTimeUpdate_Tick;
            SleepTimeUpdate.Start();

            CaptureRectangleUpdate = new DispatcherTimer();
            CaptureRectangleUpdate.Interval = TimeSpan.FromMilliseconds(200);
            CaptureRectangleUpdate.Tick += CaptureRectangleUpdate_Tick;
            CaptureRectangleUpdate.Start();

            new Thread(delegate()
            {
                while (isStreaming)
                {
                    fps_counter++;

                    OnNewFrame(new NewFrameEventArgs(GetScreen()));

                    if (sleep_time > 0)
                        Thread.Sleep(TimeSpan.FromTicks(sleep_time));
                }
            }).Start();
        }

        public void Stop()
        {
            isStreaming = false;

            SleepTimeUpdate.Stop();
            FPSLoop.Stop();
            CaptureRectangleUpdate.Stop();
        }

        void CaptureRectangleUpdate_Tick(object sender, EventArgs e)
        {
            CaptureHWND = GetForegroundWindow();

            var rect = new Rect();
            GetClientRect(CaptureHWND, ref rect);

            if (rect.Bottom != 0 && rect.Right != 0)
            {
                CaptureRegion = new System.Drawing.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }

            rect = new Rect();
            GetWindowRect(CaptureHWND, ref rect);

            if (rect.Bottom != 0 && rect.Right != 0)
            {
                CaptureWindowRegion = new System.Drawing.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
        }

        void SleepTimeUpdate_Tick(object sender, EventArgs e)
        {
            if (real_fps > desired_fps)
            {
                if (real_fps - desired_fps > 10)
                    sleep_time += 1000;
                else if (real_fps - desired_fps > 1)
                    sleep_time += 100;
                else
                    sleep_time += 10;
            }
            else if (real_fps < desired_fps)
            {
                if (desired_fps - real_fps > 10)
                    sleep_time -= 1000;
                else if (desired_fps - real_fps > 1)
                    sleep_time -= 100;
                else
                    sleep_time -= 10;

                if (sleep_time < 0)
                    sleep_time = 0;
            }
        }

        void FPSLoop_Tick(object sender, EventArgs e)
        {
            real_fps = fps_counter;

            OnFPSUpdate(new FPSUpdateEventArgs(real_fps));

            fps_counter = 0;
        }

        DispatcherTimer FPSLoop;
        DispatcherTimer SleepTimeUpdate;
        DispatcherTimer CaptureRectangleUpdate;

        int sleep_time = 0;

        int real_fps = 0;

        int desired_fps = 60;

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        IntPtr CaptureHWND;
        public System.Drawing.Rectangle CaptureRegion;
        public System.Drawing.Rectangle CaptureWindowRegion;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetClientRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows 
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }

        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;

        Bitmap GetScreen()
        {
            IntPtr hdc = GetDC(CaptureHWND); // get the desktop device context
            IntPtr hDest = CreateCompatibleDC(hdc); // create a device context to use yourself

            // create a bitmap
            IntPtr hbmp = CreateCompatibleBitmap(hdc, CaptureRegion.Width, CaptureRegion.Height);

            // use the previously created device context with the bitmap
            SelectObject(hDest, hbmp);

            // copy from the desktop device context to the bitmap device context
            // call this once per 'frame'
            BitBlt(hDest, 0, 0, CaptureRegion.Width, CaptureRegion.Height, hdc, 0, 0, TernaryRasterOperations.SRCCOPY);

            CURSORINFO pci;
            pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

            if (GetCursorInfo(out pci))
            {
                if (pci.flags == CURSOR_SHOWING)
                {
                    DrawIcon(hDest, pci.ptScreenPos.x - CaptureWindowRegion.Left - (CaptureWindowRegion.Width - CaptureRegion.Width - 8), pci.ptScreenPos.y - CaptureWindowRegion.Top - (CaptureWindowRegion.Height - CaptureRegion.Height-8), pci.hCursor);
                }
            }

            // after the recording is done, release the desktop context you got..
            ReleaseDC(IntPtr.Zero, hdc);

            // ..and delete the context you created
            DeleteDC(hDest);

            Bitmap srcImage = null;
            try
            {
                srcImage = Bitmap.FromHbitmap(hbmp);

                /*Graphics srcGraphics = Graphics.FromImage(srcImage);
                Point cursorPoint = new Point(Cursor.Position.X - CaptureRegion.X - 8, Cursor.Position.Y - CaptureRegion.Y + 200);
                try
                {
                    Cursor.Draw(srcGraphics, new Rectangle(cursorPoint, new Size(32, 32)));
                }
                catch
                {

                }
                srcGraphics.Dispose();*/


                DeleteObject(hbmp);
            }
            catch
            {

            }

            return srcImage;
        }


        public bool PreviewRecommended
        {
            get { return false; }
        }

        public void ShowSettings(System.Windows.Window ParentWindow)
        {
            
        }
    }

    public static class BitmapExtensions
    {
        public static Bitmap ScaleByPercent(this Bitmap imgPhoto, int Percent)
        {
            try
            {
                float nPercent = ((float)Percent / 100);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;
                var destWidth = (int)(sourceWidth * nPercent);
                var destHeight = (int)(sourceHeight * nPercent);

                var bmPhoto = new Bitmap(destWidth, destHeight,
                                         System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                                      imgPhoto.VerticalResolution);

                Graphics grPhoto = Graphics.FromImage(bmPhoto);
                grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                grPhoto.DrawImage(imgPhoto,
                                  new Rectangle(0, 0, destWidth, destHeight),
                                  new Rectangle(0, 0, sourceWidth, sourceHeight),
                                  GraphicsUnit.Pixel);

                grPhoto.Dispose();
                return bmPhoto;
            }
            catch
            {

            }
            return imgPhoto;
        }
    }
}
