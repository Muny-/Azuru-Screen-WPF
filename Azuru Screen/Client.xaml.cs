using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ASU
{
    /// <summary>
    /// Interaction logic for Client.xaml
    /// </summary>
    public partial class Client : Window
    {
        IStreamInput client;

        DispatcherTimer FPSTimer = new DispatcherTimer();

        Storyboard sb = new Storyboard();

        DoubleAnimation autoHideAnimation;
        DoubleAnimation autoShowAnimation;

        ExponentialEase ee = new ExponentialEase();

        public Client()
        {
            InitializeComponent();

            FPSTimer.Interval = TimeSpan.FromSeconds(1);
            FPSTimer.Tick += FPSTimer_Tick;
            FPSTimer.Start();

            ee.EasingMode = EasingMode.EaseOut;
            ee.Exponent = 6;
        }

        bool Expanded = true;

        void HideControls()
        {
            var anim = new DoubleAnimation(1, (Duration)TimeSpan.FromSeconds(1));
            anim.EasingFunction = ee;
            anim.Completed += (s, _) => Expanded = false;
            ControlGrid.BeginAnimation(ContentControl.WidthProperty, anim);
        }

        void ShowControls()
        {
            var anim = new DoubleAnimation(107, (Duration)TimeSpan.FromSeconds(1));
            anim.EasingFunction = ee;
            anim.Completed += (s, _) => Expanded = true;
            ControlGrid.BeginAnimation(ContentControl.WidthProperty, anim);
        }

        void client_ReceivedStatisticsUpdate(object sender, ReceivedStatisticsUpdateEventArgs e)
        {
            bps = e.BytesPerSecond;           
        }

        static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0 " + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }

        long bps = 0;

        void FPSTimer_Tick(object sender, EventArgs e)
        {
            fpsLabel.Text = frames + " FPS\n" + BytesToString(bps) + "/s";
            frames = 0;
        }

        int frames = 0;

        double width = 0;
        double height = 0;

        void client_FrameReceived(object sender, FrameReceivedEventArgs e)
        {
            if (statusVisible)
                HideStatus();
            frames++;
            width = e.Frame.Width;
            height = e.Frame.Height;
            RemoteImage.Source = e.Frame;
            
        }

        private void RemoteImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (control)
            {
                Point p = GetRelativeCursorPos(RemoteImage, e);

                client.UpdateMouse((int)p.X, (int)p.Y, 0, 0, 0);
            }

            //cursorLocation.Content = "x=" + x + ", y=" + y;
        }

        private void RemoteImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (control)
            {
                Point p = GetRelativeCursorPos(RemoteImage, e);

                if (e.LeftButton == MouseButtonState.Pressed)
                    client.UpdateMouse((int)p.X, (int)p.Y, 1, 0, 0);
                else if (e.RightButton == MouseButtonState.Pressed)
                    client.UpdateMouse((int)p.X, (int)p.Y, 3, 0, 0);
            }
        }

        private void RemoteImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (control)
            {
                Point p = GetRelativeCursorPos(RemoteImage, e);

                if (e.ChangedButton == MouseButton.Left)
                    client.UpdateMouse((int)p.X, (int)p.Y, 2, 0, 0);
                else if (e.ChangedButton == MouseButton.Right)
                    client.UpdateMouse((int)p.X, (int)p.Y, 4, 0, 0);
            }
        }

        private void RemoteImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (control)
            {
                Point p = GetRelativeCursorPos(RemoteImage, e);

                client.UpdateMouse((int)p.X, (int)p.Y, 5, e.Delta, 0);
            }
        }

        Point GetRelativeCursorPos(FrameworkElement element, MouseEventArgs e)
        {
            
            Point p = e.GetPosition(element);
            double x = p.X;
            double y = p.Y;

            x = x.Map(0, RemoteImage.ActualWidth, 0, width);
            y = y.Map(0, RemoteImage.ActualHeight, 0, height);

            return new Point(x, y);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (control)
                client.UpdateMouse(0, 0, 6, 0, KeyInterop.VirtualKeyFromKey(e.Key));
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (control)
                client.UpdateMouse(0, 0, 7, 0, KeyInterop.VirtualKeyFromKey(e.Key));
        }

        private void Window_Loaded(object s, RoutedEventArgs ea)
        {
            /*client = new MJPEGClient("http://81.167.44.38/mjpg/video.mjpg");
            client.FrameReceived += delegate(object sender, FrameReceivedEventArgs e)
            {
                Dispatcher.Invoke(delegate() { client_FrameReceived(sender, e); });
            };
            client.Start();*/

            StreamInputDialog sid = new StreamInputDialog();

            if (sid.ShowDialog() == true)
            {
                client = sid.StreamInput;
                client.FrameReceived += delegate(object sender, FrameReceivedEventArgs e)
                {
                    Dispatcher.Invoke(delegate() { client_FrameReceived(sender, e); });
                };
                client.ReceivedStatisticsUpdate +=client_ReceivedStatisticsUpdate;
                client.StreamLost += client_StreamLost;
                Connect();
            }
            else
            {
                this.Close();
            }
        }

        void client_StreamLost(object sender, StreamLostEventArgs e)
        {
            ShowStatus("Stream lost: " + e.Reason);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (client != null)
                Disconnect();

            FPSTimer.Stop();
        }

        bool doAutoHidePanel = false;

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                SetAutoHide(true);
                this.WindowStyle = System.Windows.WindowStyle.None;
                this.WindowState = System.Windows.WindowState.Maximized;
                FullscreenButton.Content = "Exit Fullscreen";
                FullscreenButton.FontSize = 14;
            }
            else
            {
                SetAutoHide(false);
                this.WindowState = System.Windows.WindowState.Normal;
                this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                FullscreenButton.Content = "Fullscreen";
                FullscreenButton.FontSize = 16;
            }
        }

        bool showCursor = false;

        private void ShowCursorButton_Click(object sender, RoutedEventArgs e)
        {
            showCursor = !showCursor;

            if (showCursor)
            {
                RemoteImage.Cursor = Cursors.Arrow;
                ShowCursorButton.Content = "Hide Local Cursor";
            }
            else
            {
                RemoteImage.Cursor = Cursors.None;
                ShowCursorButton.Content = "Show Local Cursor";
            }
        }

        bool control = true;

        private void ControlToggleButton_Click(object sender, RoutedEventArgs e)
        {
            control = !control;

            if (control)
            {
                ControlToggleButton.Content = "Disable Control";
            }
            else
            {
                ControlToggleButton.Content = "Enable Control";
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (client != null)
                Disconnect();

            this.Close();
        }

        public void Connect()
        {
            ShowStatus("Connecting...");
            client.Start();
        }

        public void Disconnect()
        {
            client.Stop();
            ShowStatus("Disconnected");
        }

        BlurEffect be = new BlurEffect();

        bool statusVisible = false;

        public void ShowStatus(string text)
        {
            Dispatcher.Invoke(delegate()
            {
                statusVisible = true;
                be.Radius = 10;
                be.RenderingBias = RenderingBias.Quality;
                RemoteImage.Effect = be;

                StatusLabel.Text = text;
                StatusGrid.Visibility = System.Windows.Visibility.Visible;
            });
            
        }

        public void HideStatus()
        {
            Dispatcher.Invoke(delegate()
            {
                statusVisible = false;
                RemoteImage.Effect = null;
                StatusGrid.Visibility = System.Windows.Visibility.Hidden;
            });
        }

        public void Reconnect()
        {
            Disconnect();
            Dispatcher.Invoke(delegate()
            {
                ShowStatus("Reconnecting in 1s...");
            });

            new Thread(delegate()
            {
                Thread.Sleep(1000);

                Connect();
            }).Start();
            
            
        }

        private void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            Reconnect();
        }

        private void ControlGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (doAutoHidePanel)
                ShowControls();
        }

        private void ControlGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (doAutoHidePanel)
                HideControls();
        }

        void SetAutoHide(bool enabled)
        {
            doAutoHidePanel = enabled;

            if (doAutoHidePanel)
            {
                AutoHideToggleButton.Content = "Disable Auto Hide";
            }
            else
            {
                AutoHideToggleButton.Content = "Enable Auto Hide";
            }
        }

        private void AutoHideToggleButton_Click(object sender, RoutedEventArgs e)
        {
            SetAutoHide(!doAutoHidePanel);
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (doAutoHidePanel)
                ShowControls();
        }
    }
}
