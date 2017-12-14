using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ASU
{
    /// <summary>
    /// Interaction logic for SharingSourceDialog.xaml
    /// </summary>
    public partial class SharingSourceDialog : Window
    {
        public SharingSourceDialog()
        {
            InitializeComponent();
        }

        public ISharingSource SharingSource;

        private void EntireDesktopButton_Click(object sender, RoutedEventArgs e)
        {
            this.SharingSource = new SharingSources.EntireDesktopSharingSource();
            this.DialogResult = true;

            this.Close();
        }

        private void ActiveWindowButton_Click(object sender, RoutedEventArgs e)
        {
            this.SharingSource = new SharingSources.ActiveWindowSharingSource();
            this.DialogResult = true;

            this.Close();
        }

        private void WebcamButton_Click(object sender, RoutedEventArgs e)
        {
            this.SharingSource = new SharingSources.WebcamSharingSource();
            this.DialogResult = true;

            this.Close();
        }

        private void MJPEGStreamButton_Click(object sender, RoutedEventArgs e)
        {
            this.SharingSource = new SharingSources.MJPEGStreamSharingSource();
            this.DialogResult = true;

            this.Close();
        }
    }
}
