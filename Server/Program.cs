using System;
using System.Net;
using Core;

namespace Server
{
    class Program
    {
        public static GameRoom Room = new GameRoom();
        public static Listener Listener = new Listener();

        private static readonly Random getrandom = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            return getrandom.Next(min, max);
        }

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPEndPoint endPoint = new IPEndPoint(ipHost.AddressList[0], 7777);

            Console.WriteLine("Listening......");
            Listener.Init(endPoint, () => SessionManager.Instance.Generate());

            while (true)
            {
            }
        }
    }
}