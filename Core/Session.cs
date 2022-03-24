using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// ReSharper disable All

namespace Core
{
    public abstract class PacketSession : Session
    {
        private const int HEADER_SIZE = 2;

        public sealed override int OnRecv(ArraySegment<byte> s)
        {
            int processLen = 0;
            while (true)
            {
                if (s.Count < HEADER_SIZE)
                    break;

                ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
                if (s.Count != size)
                    break;

                OnRecvPacket(s);
                processLen += size;
                s = new ArraySegment<byte>(s.Array, s.Offset + size, s.Count - size);
            }

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> s);
    }

    public abstract class Session
    {
        private Socket _socket;

        private object _lock = new object();
        public int disconnected = 0;

        private RecvBuffer _recvBuffer = new RecvBuffer(4096);

        private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        private List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> s);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += OnRecvCompleted;
            _sendArgs.Completed += OnSendCompleted;

            RegisterRecv();
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        public void Send(ArraySegment<byte> s)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(s);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        private void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        #region Send

        private void RegisterSend()
        {
            while (_sendQueue.Count > 0)
                _pendingList.Add(_sendQueue.Dequeue());

            _sendArgs.BufferList = _pendingList;
            try
            {
                if (_socket.SendAsync(_sendArgs) == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend error: {e}");
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    _sendArgs.BufferList = null;
                    _pendingList.Clear();

                    OnSend(args.BytesTransferred);

                    if (_sendQueue.Count > 0)
                        RegisterSend();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnSendCompleted error: {e}");
            }
        }

        #endregion

        #region Recv

        private void RegisterRecv()
        {
            if (disconnected == 1)
                return;

            _recvBuffer.Clear();
            ArraySegment<byte> s = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(s.Array, s.Offset, s.Count);
            _recvArgs.AcceptSocket = null;

            try
            {
                if (_socket.ReceiveAsync(_recvArgs) == false)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv error: {e}");
            }
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred < 0)
                return;

            try
            {
                if (args.SocketError == SocketError.Success)
                {
                    if (!_recvBuffer.OnWrite(args.BytesTransferred))
                    {
                        Disconnect();
                        return;
                    }

                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || processLen > _recvBuffer.AllocSize)
                    {
                        Disconnect();
                        return;
                    }

                    if (!_recvBuffer.OnRead(processLen))
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                else
                {
                    Console.WriteLine($"OnRecvCompleted failed: {args.SocketError}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnRecvCompleted error: {e}");
                throw;
            }
        }

        #endregion
    }
}