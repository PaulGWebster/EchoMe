using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace Server
{
    class Program
    {
        static readonly Int16 ListenPort = 12345;
        static readonly IPAddress ListenHost = IPAddress.Parse("127.0.0.1");
        static readonly IPEndPoint LocalEndPoint = new IPEndPoint(ListenHost, ListenPort);

        static Queue<byte> RXStream = new Queue<byte>();

        static void Main(string[] args)
        {
            Console.WriteLine("System starting up!");
            Console.WriteLine("Binding {0}:{1}", ListenHost, ListenPort);
            try
            {
                Test1();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception: {0}", exception.ToString());
            }
        }

        private static void Test1()
        {
            // Configure the listening socket
            Socket TCPListener = new Socket(LocalEndPoint.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
            TCPListener.Bind(LocalEndPoint);
            TCPListener.Listen(10);

            // Pass reading the socket off to a thread
            ParameterizedThreadStart ConnReaderStart = new ParameterizedThreadStart(SocketRead);
            Thread ConnReader = new Thread(ConnReaderStart);
            ConnReader.Start(TCPListener);

            // Configure the connector socket
            Socket TCPConnector = new Socket(LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            TCPConnector.Connect(LocalEndPoint);

            // Send a hello
            IRMP ProtocolHelper = new IRMP();
            // Send the header
            TCPConnector.Send(ProtocolHelper.Hello(65535));
            // Get a response
            if (true)
            {
                Byte[] StreamCmd = new byte[1];
                Byte[] StreamData = new byte[2];
                TCPConnector.Receive(StreamCmd, 0, 1, SocketFlags.None);
                TCPConnector.Receive(StreamData, 0, 2, SocketFlags.None);
            }
            
            // Connection will be closed 
            Thread.Sleep(10000);
        }

        class IRMP
        {
            static readonly byte CMD_Hello = BitConverter.GetBytes((Byte)1)[0];

            public byte[] Hello(UInt16 PossibleID)
            {
                byte[] PossibleIDStore = BitConverter.GetBytes(PossibleID);
                return new byte[3] { CMD_Hello, PossibleIDStore[0], PossibleIDStore[1] };
            }
        }

        private static void SocketRead(Object PassedArgs)
        {
            Console.WriteLine("Waiting for a connection..."); 

            // Wait and accept a client
            Socket SocketFH =  ((Socket)PassedArgs).Accept();
            Boolean FirstByte = true;
            Byte[] StreamCmd = new byte[1];
            Byte[] StreamData = new byte[2];

            if (FirstByte)
            {
                try
                {
                    SocketFH.Receive(StreamCmd, 0, 1, SocketFlags.None);
                    SocketFH.Receive(StreamData, 0, 2, SocketFlags.None);
                }
                catch
                {
                }
                Console.WriteLine(StreamCmd.ToString());
                Console.WriteLine(BitConverter.ToUInt16(StreamData,0));
            }

            while (true)
            {

                Thread.Sleep(100);
            }

            SocketFH.Close();
        }
    }

}