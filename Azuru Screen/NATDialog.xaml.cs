using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Open.Nat;

namespace ASU
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NATDialog : Window
    {
        public NATDialog(int Port)
        {
            this.Port = Port;
            InitializeComponent();
        }

        public string IPAddress;
        public int Port;

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = true;

            ButtonsGrid.Visibility = System.Windows.Visibility.Hidden;
            StatusGrid.Visibility = System.Windows.Visibility.Visible;

            HandleNAT();
        }

        void HandleNAT()
        {

            //NatDiscoverer nd = new NatDiscoverer();

            

            new Thread(delegate()
            {
                try
                {

                    var discoverer = new NatDiscoverer();
                    var cts = new CancellationTokenSource(5000);
                    Task<NatDevice> discTask = discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                    discTask.Wait();
                    var device = discTask.Result;

                    Task<IPAddress> ipTask = device.GetExternalIPAsync();
                    ipTask.Wait();

                    // display the NAT's IP address
                    Console.WriteLine("The external IP Address is: {0} ", ipTask.Result.ToString());

                    IPAddress = ipTask.Result.ToString();

                    Task portmapTask1 = device.DeletePortMapAsync(new Mapping(Protocol.Tcp, Port, Port));

                    portmapTask1.Wait();

                    Task portMapTask = device.CreatePortMapAsync(new Mapping(Protocol.Tcp, Port, Port, "Azuru Sharing Utility"));

                    portMapTask.Wait();

                    
                    
                }
                catch
                {

                }

                Dispatcher.Invoke(delegate()
                {
                    this.DialogResult = true;
                });

            }).Start();
            
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            IPAddress = LocalIPAddress();
        }

        string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            this.DialogResult = true;
            
            if (IPAddress == null)
                IPAddress = LocalIPAddress();
        }
    }
}
