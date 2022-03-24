using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// ReSharper disable All

namespace Core
{
    public class Listener
    {
        private Socket _socket;
        private Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            _sessionFactory += sessionFactory;
            _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(endPoint);
            _socket.Listen(100);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnAcceptCompleted;
            RegisterAccept(args);

            // for (int i = 0; i < 10; i++)
            // {
            //     SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            //     args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            //     RegisterAccept(args);
            // }
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            if (_socket.AcceptAsync(args) == false)
                OnAcceptCompleted(null, args);
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnAcceptCompleted failed: {args.SocketError}");
            }

            // RegisterAccept(args);
        }
    }
}