using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ASU
{
    public interface IStreamInput
    {
        event EventHandler<FrameReceivedEventArgs> FrameReceived;
        event EventHandler<ReceivedStatisticsUpdateEventArgs> ReceivedStatisticsUpdate;
        event EventHandler<StreamLostEventArgs> StreamLost;

        void Start();

        void Stop();

        void UpdateMouse(int x, int y, int mouse_action, int delta, int vkeycode);

        string StreamDetails
        {
            get;
        }
    }

    public class FrameReceivedEventArgs : EventArgs
    {
        public BitmapSource Frame;

        public FrameReceivedEventArgs(BitmapSource frame)
        {
            this.Frame = frame;
        }
    }

    public class ReceivedStatisticsUpdateEventArgs : EventArgs
    {
        public long BytesPerSecond;

        public ReceivedStatisticsUpdateEventArgs(long bps)
        {
            this.BytesPerSecond = bps;
        }
    }

    public class StreamLostEventArgs : EventArgs
    {
        public string Reason;

        public StreamLostEventArgs(string Reason)
        {
            this.Reason = Reason;
        }
    }
}
