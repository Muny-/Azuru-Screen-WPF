using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AForge.Video;

namespace ASU
{
    public class MJPEGClient : IStreamInput
    {

        public event EventHandler<FrameReceivedEventArgs> FrameReceived;

        public event EventHandler<ReceivedStatisticsUpdateEventArgs> ReceivedStatisticsUpdate;

        public event EventHandler<StreamLostEventArgs> StreamLost;

        private string _streamdetails;

        public string StreamDetails
        {
            get
            {
                return _streamdetails;
            }
        }

        MjpegDecoder _DECODER = new MjpegDecoder();
        //MJPEGStream stream;

        public MJPEGConnectionProfile ConnectionProfile;

        public MJPEGClient(MJPEGConnectionProfile profile)
        {
            this.ConnectionProfile = profile;

            if (profile.Name != "Most Recent")
                _streamdetails += profile.Name + " { ";

            if (profile.IsAuthenticated)
                _streamdetails += profile.Username + ":" + new String('*', profile.Password.Length) + "@";

            _streamdetails += profile.Address;

            if (profile.Name != "Most Recent")
                _streamdetails += " }";

            _DECODER.FrameReady += _DECODER_FrameReady;
            _DECODER.Error += _DECODER_Error;
        }

        public void Start()
        {
            
            try
            {
                if (ConnectionProfile.IsAuthenticated)
                    _DECODER.ParseStream(new Uri(ConnectionProfile.Address), ConnectionProfile.Username, ConnectionProfile.Password);
                else
                    _DECODER.ParseStream(new Uri(ConnectionProfile.Address));
            }
            catch (Exception e)
            {
                Lost(e.Message);
            }
            /*
            stream = new MJPEGStream();
            stream.NewFrame += stream_NewFrame;
            stream.PlayingFinished += stream_PlayingFinished;
            stream.VideoSourceError += stream_VideoSourceError;
            stream.Source = this.URL;
            stream.Start();*/
        }

        /*void stream_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            System.Windows.MessageBox.Show(eventArgs.Description);
            stream.Stop();
        }

        void stream_PlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            System.Windows.MessageBox.Show(reason.ToString());
        }

        void stream_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            if (FrameReceived != null)
            {
                BitmapSource src = eventArgs.Frame.ToBitmapSource();
                src.Freeze();
                FrameReceived(this, new FrameReceivedEventArgs(src));
            }

        }*/

        void Lost(string reason)
        {
            if (StreamLost != null)
                StreamLost(this, new StreamLostEventArgs(reason));
        }

        void _DECODER_Error(object sender, ErrorEventArgs e)
        {
            
            _DECODER.StopStream();

            Lost(e.Message);
        }

        void _DECODER_FrameReady(object sender, FrameReadyEventArgs e)
        {
            if (FrameReceived != null)
                FrameReceived(this, new FrameReceivedEventArgs(e.BitmapImage));
        }

        public void Stop()
        {
            _DECODER.StopStream();
            //stream.Stop();
        }

        public void UpdateMouse(int x, int y, int mouse_action, int delta, int vkeycode)
        {
            //throw new NotImplementedException();
        }


        
    }
}
