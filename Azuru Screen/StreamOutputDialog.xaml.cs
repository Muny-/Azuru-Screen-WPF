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
    public partial class StreamOutputDialog : Window
    {
        public StreamOutputDialog()
        {
            InitializeComponent();
        }

        public IStreamOutput StreamOutput;

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            StreamOutput = new FrameServer(IPAddress.Any, 8076);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            StreamOutput = new MJPEGServer(IPAddress.Any, 8080);
        }
    }
}
