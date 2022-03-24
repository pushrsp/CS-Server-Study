using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Core;

namespace Dummy
{
    public class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
        }

        public override void OnSend(int numOfBytes)
        {
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
        }

        public override void OnRecvPacket(ArraySegment<byte> s)
        {
            PacketManager.Instance.OnRecvPacket(this, s);
        }
    }
}