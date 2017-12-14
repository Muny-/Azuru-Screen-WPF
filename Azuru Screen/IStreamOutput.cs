using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ASU
{
    public interface IStreamOutput
    {
        event EventHandler<ControlPacketReceivedEventArgs> ControlPacketReceived;

        event EventHandler<ClientConnectedEventArgs> ClientConnected;

        event EventHandler<ClientDisonnectedEventArgs> ClientDisconnected;

        string OutputName { get; }

        int ConnectedClients { get; }

        int? Port { get; }

        void Start();

        void Stop();

        void SendFrame(BitmapSource frame);

        void SendFrame(Bitmap frame);
    }

    public class ControlPacketReceivedEventArgs : EventArgs
    {
        public int X;
        public int Y;

        public int Delta;

        public int VKeyCode;

        // 0 == None
        // 1 == Left Down
        // 2 == Left Up
        // 3 == Right Down
        // 4 == Right Up
        // 5 == Mouse Wheel
        // 6 == Key Down
        // 7 == Key Up
        public int MouseAction;

        public ControlPacketReceivedEventArgs(int X, int Y, int MouseAction, int Delta, int VKeyCode)
        {
            this.X = X;
            this.Y = Y;
            this.Delta = Delta;
            this.MouseAction = MouseAction;
            this.VKeyCode = VKeyCode;
        }
    }

    public class ClientConnectedEventArgs : EventArgs
    {
        public ClientHandler Client;

        public ClientConnectedEventArgs(ClientHandler client)
        {
            this.Client = client;
        }
    }

    public class ClientDisonnectedEventArgs : EventArgs
    {
        public ClientHandler Client;

        public ClientDisonnectedEventArgs(ClientHandler client)
        {
            this.Client = client;
        }
    }
}
