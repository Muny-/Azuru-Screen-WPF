using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

namespace ASU
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DeltaFrameServer dfs;

        public MainWindow()
        {
            /*SharingSources.EntireDesktopIntPtrSharingSource edipss = new SharingSources.EntireDesktopIntPtrSharingSource();
            edipss.NewFrame += edipss_NewFrame;

            dfs = new DeltaFrameServer(IPAddress.Any, 8076);
            dfs.Start();

            edipss.Start();*/

            InitializeComponent();
        }

        void edipss_NewFrame(object sender, NewFrameEventArgs e)
        {
            dfs.SendFrame(e.Handle);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Client().Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new Server().Show();
        }
    }
}
