﻿using System;
using System.Net;
using System.Threading;
using Core;

namespace Dummy
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPEndPoint endPoint = new IPEndPoint(ipHost.AddressList[0], 7777);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => SessionManager.Instance.Generate(), 10);

            while (true)
            {
                try
                {
                    SessionManager.Instance.SendForEach();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Thread.Sleep(500);
            }
        }
    }
}