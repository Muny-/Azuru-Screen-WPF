using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ASU
{
    public class ClientHandler
    {
        public NetworkStream NetStream;
        public BinaryReader BR;
        public BinaryWriter BW;
        public TcpClient Client;
        public Action<int, int, int, int, int> updateMouseAction;

        public ClientHandler() { }

        public ClientHandler(TcpClient client, Action<int,int,int,int,int> updateMouseAction)
        {
            this.Client = client;
            this.updateMouseAction = updateMouseAction;
            new Thread(HandleClient).Start();
        }

        private void HandleClient()
        {
            NetStream = Client.GetStream();

            BR = new BinaryReader(NetStream);
            BW = new BinaryWriter(NetStream);

            while (Client.Connected)
            {
                try
                {
                    if (BR.ReadInt32() == 0)
                    {
                        int x = BR.ReadInt32();
                        int y = BR.ReadInt32();
                        int MouseAction = BR.ReadInt32();
                        int Delta = BR.ReadInt32();
                        int VKeyCode = BR.ReadInt32();

                        updateMouseAction(x, y, MouseAction, Delta, VKeyCode);
                    }
                }
                catch
                {
                    Client.Close();
                    break;
                }
            }
            OnClientDisonnected(new ClientDisonnectedEventArgs(this));
        }

        public void Disconnect()
        {
            Client.Close();
            BR.Close();
            BW.Close();

            OnClientDisonnected(new ClientDisonnectedEventArgs(this));
        }

        public event ClientDisconnectedEventHandler ClientDisonnected;

        public delegate void ClientDisconnectedEventHandler(object sender, ClientDisonnectedEventArgs e);

        protected virtual void OnClientDisonnected(ClientDisonnectedEventArgs e)
        {
            if (ClientDisonnected != null)
            {
                ClientDisonnected(this, e);
            }
        }
    }

    
}
