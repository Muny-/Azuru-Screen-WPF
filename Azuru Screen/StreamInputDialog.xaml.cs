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
    public partial class StreamInputDialog : Window
    {
        public StreamInputDialog()
        {
            InitializeComponent();
        }

        public IStreamInput StreamInput;

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            IPProfileDialog ippd = new IPProfileDialog();

            if (ippd.ShowDialog() == true)
            {
                this.DialogResult = true;

                StreamInput = new FrameClient(ippd.SelectedProfile);
                this.Close();
            }
            else
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void Button_Click_3(object ss, RoutedEventArgs ee)
        {
            MJPEGProfileDialog md = new MJPEGProfileDialog();

            if (md.ShowDialog() == true)
            {
                StreamInput = new MJPEGClient(md.SelectedProfile);

                this.DialogResult = true;
            }
            else
            {
                this.DialogResult = false;
            }
        }
    }
}
