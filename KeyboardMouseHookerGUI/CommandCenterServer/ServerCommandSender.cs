﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommandCenterServer
{
    class ServerCommandSender
    {
        private const int CMDSERVICE_PORT = 9990;

        public static string returnedMsg = "control stopped";
        UdpClient client;
        IPEndPoint ip;
        byte[] bytes;
        byte[] returnedBytes;
        

        public void Send(string message)
        {
           
            bytes = Encoding.UTF8.GetBytes(message);
            client.Send(bytes, bytes.Length, ip);

            returnedBytes = client.Receive(ref ip);
           

            returnedMsg = Encoding.Unicode.GetString(returnedBytes);
            
        }

        public ServerCommandSender()
        {
            this.client = new UdpClient();
            this.ip = new IPEndPoint(IPAddress.Any, CMDSERVICE_PORT);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        ~ServerCommandSender()
        {

            string shutdownMsg;
            byte[] shutdownMsgBytes;

            shutdownMsg = "server shutdown";

            shutdownMsgBytes = Encoding.UTF8.GetBytes(shutdownMsg);
            client.Send(shutdownMsgBytes, shutdownMsgBytes.Length, ip);
            client.Close();
        }
    }
}
