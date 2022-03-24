using System;
using System.Collections.Generic;

namespace Server
{
    public class SessionManager
    {
        static SessionManager _sessionManager = new SessionManager();

        public static SessionManager Instance
        {
            get { return _sessionManager; }
        }

        object _lock = new object();

        static int _sessionId;
        private Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();

        public ClientSession Generate()
        {
            lock (_lock)
            {
                int sessionId = ++_sessionId;
                ClientSession session = new ClientSession();
                session.SessionId = sessionId;
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Connected: {session.SessionId.ToString()}");

                return session;
            }
        }

        public ClientSession Find(int id)
        {
            lock (_lock)
            {
                ClientSession session = null;
                _sessions.TryGetValue(id, out session);

                return session;
            }
        }

        public void Remove(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session.SessionId);
            }
        }
    }
}