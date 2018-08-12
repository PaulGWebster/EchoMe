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

            // Create a control header, the binary/boolnea sequence is reversed.
            Boolean[] ControlPacket = new Boolean[8] { true, false, false, true, false, false, false, false };
            Array.Reverse(ControlPacket); // Copy will invert the order, so swap it before conversion
            Byte[] ControlSequence = BitArrayToByteArray(new BitArray(ControlPacket));

            Console.WriteLine(ControlSequence[0].ToString());

            // Send the header
            TCPConnector.Send(ControlSequence);

            // Connection will be closed 
            Thread.Sleep(10000);
        }

        private static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        private static void SocketRead(Object PassedArgs)
        {
            Console.WriteLine("Waiting for a connection...");

            // Wait and accept a client
            Socket SocketFH = ((Socket)PassedArgs).Accept();
            Boolean FirstByte = true;
            Byte[] StreamHeader = new byte[2];

            if (FirstByte)
            {
                try
                {
                    SocketFH.Receive(StreamHeader, 0, 2, SocketFlags.None);
                }
                catch
                {
                    Console.WriteLine("Failed to extract stream header");
                    return;
                }
            }

            //Console.WriteLine(StreamHeader.ToString());

            SocketFH.Close();
        }
    }
}