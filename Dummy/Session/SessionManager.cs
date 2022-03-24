using System;
using System.Collections.Generic;

namespace Dummy
{
    public class SessionManager
    {
        private static SessionManager _sessionManager = new SessionManager();

        public static SessionManager Instance
        {
            get { return _sessionManager; }
        }

        private object _lock = new object();

        private List<ServerSession> _sessions = new List<ServerSession>();

        public void SendForEach()
        {
            lock (_lock)
            {
                foreach (ServerSession serverSession in _sessions)
                {
                    C_Chat chatPacket = new C_Chat();
                    chatPacket.chat = "Hello Server";
                    ArraySegment<byte> segment = chatPacket.Write();

                    serverSession.Send(segment);
                }
            }
        }

        public ServerSession Generate()
        {
            lock (_lock)
            {
                ServerSession serverSession = new ServerSession();
                _sessions.Add(serverSession);

                return serverSession;
            }
        }
    }
}