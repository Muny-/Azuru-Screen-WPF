using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ASU
{
    /// <summary>
    /// Interaction logic for Server.xaml
    /// </summary>
    public partial class Server : Window
    {
        public Server()
        {
            InitializeComponent();
            enableAero.IsChecked = IsDWMCompositionEnabled();
        }

        IStreamOutput output;


        ISharingSource SharingSource;

        string lastIpAddress = "127.0.0.1";

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((string)button1.Content == "Start Sharing")
            {
                SharingSource = null;

                SharingSourceDialog ssd = new SharingSourceDialog();
                if (ssd.ShowDialog() == true)
                {
                    

                    StreamOutputDialog sod = new StreamOutputDialog();

                    if (sod.ShowDialog() == true)
                    {
                        SharingSource = ssd.SharingSource;

                        SharingSource.NewFrame += SharingSource_NewFrame;
                        SharingSource.FPSUpdate += SharingSource_FPSUpdate;


                        button1.Content = "Stop Sharing";

                        //output = new FrameServer(IPAddress.Any, 8076);
                        output = sod.StreamOutput;

                        if (output.Port != null)
                        {
                            NATDialog natd = new NATDialog((int)output.Port);
                            if (natd.ShowDialog() == true)
                            {
                                lastIpAddress = natd.IPAddress;
                            }
                        }
                        else
                        {
                            lastIpAddress = "::";
                        }

                        

                        output.ControlPacketReceived += server_ControlPacketReceived;
                        output.ClientConnected += server_ClientConnected;
                        output.ClientDisconnected += server_ClientDisonnected;
                        output.Start();

                        SharingSource.Start();

                        if (lastIpAddress != "::")
                        {
                            serverStatusLabel.Content = "Server listening at: " + lastIpAddress + ":" + output.Port;
                        }
                        else
                        {
                            serverStatusLabel.Content = "Server listening";
                        }

                        serverStatusLabel.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(116, 224, 131));

                        
                    }

                    
                }                
            }
            else
            {
                button1.Content = "Start Sharing";

                StopSharing();
            }
        }

        void server_ClientDisonnected(object sender, ClientDisonnectedEventArgs e)
        {
            UpdateClientCount();
        }

        void server_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            UpdateClientCount();
        }

        void UpdateClientCount()
        {
            clientsConnectedLabel.Dispatcher.Invoke(delegate()
            {
                clientsConnectedLabel.Content = output.ConnectedClients;
            });
        }

        void StopSharing()
        {
            if (output != null)
                output.Stop();

            if (SharingSource != null)
            SharingSource.Stop();

            Dispatcher.Invoke(delegate()
            {
                serverStatusLabel.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(193,58,58));
                serverStatusLabel.Content = "Server Not Listening";
            });
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x10;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;

        [DllImport("user32.dll")]
        static extern bool keybd_event(byte bVk, byte bScan, uint dwFlags,
           UIntPtr dwExtraInfo);

        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        const uint KEYEVENTF_KEYUP = 0x0002;

        void server_ControlPacketReceived(object sender, ControlPacketReceivedEventArgs e)
        {
            if (allowCursor)
            {
                int flags = 0;

                if (e.MouseAction == 1)
                    flags = (int)MOUSEEVENTF_LEFTDOWN;
                else if (e.MouseAction == 2)
                    flags = (int)MOUSEEVENTF_LEFTUP;
                else if (e.MouseAction == 3)
                    flags = (int)MOUSEEVENTF_RIGHTDOWN;
                else if (e.MouseAction == 4)
                    flags = (int)MOUSEEVENTF_RIGHTUP;
                else if (e.MouseAction == 5)
                {
                    flags = (int)MOUSEEVENTF_WHEEL;
                }
                else if (e.MouseAction == 6)
                {
                    flags = -1;

                    keybd_event((byte)e.VKeyCode, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                }
                else if (e.MouseAction == 7)
                {
                    flags = -1;

                    keybd_event((byte)e.VKeyCode, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
                }

                if (flags != -1)
                {
                    if (SharingSource.GetType() == typeof(ASU.SharingSources.ActiveWindowSharingSource))
                    {
                        SharingSources.ActiveWindowSharingSource aws = (SharingSources.ActiveWindowSharingSource)SharingSource;

                        //pci.ptScreenPos.x - CaptureWindowRegion.Left - (CaptureWindowRegion.Width - CaptureRegion.Width), pci.ptScreenPos.y - CaptureWindowRegion.Top - (CaptureWindowRegion.Height - CaptureRegion.Height)

                        e.X += aws.CaptureWindowRegion.Left + (aws.CaptureWindowRegion.Width - aws.CaptureRegion.Width);
                        e.Y += aws.CaptureWindowRegion.Top + (aws.CaptureWindowRegion.Height - aws.CaptureRegion.Height);
                    }

                    if (flags == MOUSEEVENTF_WHEEL)
                    {
                        mouse_event((uint)flags, (uint)e.X, (uint)e.Y, e.Delta, 0);
                    }
                    else if (flags == 0)
                        SetCursorPos(e.X, e.Y);
                    else
                        mouse_event((uint)flags, (uint)e.X, (uint)e.Y, 0, 0);
                }
            }
        }

        void SharingSource_FPSUpdate(object sender, FPSUpdateEventArgs e)
        {
            fpsLabel.Content = e.FPS + " FPS";
        }

        void SharingSource_NewFrame(object sender, NewFrameEventArgs e)
        {
            if (e.Frame != null)
            {
                

                if (showPreview)
                {
                    Bitmap bmp = (Bitmap)e.Frame.Clone();
                    BitmapSource bms = bmp.ToBitmapSource();
                    Dispatcher.InvokeAsync(delegate()
                    {
                        PreviewImage.Source = bmp.ToBitmapSource();
                        bmp.Dispose();
                    });
                }

                output.SendFrame(e.Frame);
            }
            else
            {



                output.SendFrame(e.WPFFrame);

                if (showPreview)
                {
                    if (e.WPFFrame != null)
                    {
                        e.WPFFrame.Freeze();
                        Dispatcher.InvokeAsync(delegate()
                        {

                            PreviewImage.Source = e.WPFFrame;
                        });
                    }
                }
            }
            
        }

        bool showPreview = false;

        bool allowCursor = true;

        private void sharingSourceSettings_Click(object sender, RoutedEventArgs e)
        {
            if (SharingSource != null)
                SharingSource.ShowSettings(this);
        }

        private void previewCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            showPreview = true;
            interfaceGrid.SetValue(Grid.ColumnSpanProperty, 1);
        }

        private void previewCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            showPreview = false;
            interfaceGrid.SetValue(Grid.ColumnSpanProperty, 2);
        }

        private void remoteControlCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            allowCursor = true;
        }

        private void remoteControlCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
           allowCursor = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopSharing();
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                try
                {
                    Clipboard.SetText(lastIpAddress);
                    MessageBox.Show("Copied IP Address to clipboard!");
                }
                catch
                {
                    MessageBox.Show("Unable to copy to clipboard!");
                }
            }
        }

        bool IsDWMCompositionEnabled()
        {
            bool enabled = false;

            DwmIsCompositionEnabled(out enabled);

            return enabled;
        }

        private void enableAero_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsDWMCompositionEnabled())
                    DwmEnableComposition(CompositionAction.DWM_EC_ENABLECOMPOSITION);
            }
            catch
            {

            }
        }

        private void enableAero_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsDWMCompositionEnabled())
                    DwmEnableComposition(CompositionAction.DWM_EC_DISABLECOMPOSITION);
            }
            catch
            {

            }
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmEnableComposition(CompositionAction uCompositionAction);

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);

        [Flags]
        public enum CompositionAction : uint
        {
            /// <summary>
            /// To enable DWM composition
            /// </summary>
            DWM_EC_DISABLECOMPOSITION = 0,
            /// <summary>
            /// To disable composition.
            /// </summary>
            DWM_EC_ENABLECOMPOSITION = 1
        }

    }

    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);
    }
}
