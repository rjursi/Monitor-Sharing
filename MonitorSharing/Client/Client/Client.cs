﻿using System;

using System.Drawing;

using System.Text;

using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Client
{
    public partial class Client : Form
    {
        Socket socketServer;
        Thread clientReceiveThread;
        bool clientShutdownFlag;
        ClientListener clientCommandListener;


        Thread clientControlThread;
        delegate void ThreadDelegate(int imgSize, Image img);
        delegate void clientShutdownDelegate();

        public Client()
        {
            InitializeComponent();
            clientShutdownFlag = false;

            clientCommandListener = new ClientListener();
            
        }


        private void runClientListenerThread()
        {
            clientCommandListener.Start();
        }

        public void receiveThread()
        {
            Byte[] sendData_r, sendData_s;

            Byte[] recvData = new Byte[8];
            int imgSize = 0;

            while (!clientShutdownFlag)
            {
                sendData_r = Encoding.UTF8.GetBytes("ready");
                sendData_s = Encoding.UTF8.GetBytes("start");

                try
                {
                    socketServer.Send(sendData_r);
                }
                catch (SocketException)
                {
                    MessageBox.Show("서버 종료로 인해 클라이언트를 종료합니다.");
                    clientShutdown();
                }
                
                try
                {
                    socketServer.Receive(recvData);
                    imgSize = BitConverter.ToInt32(recvData, 0);

                    Array.Resize<Byte>(ref recvData, imgSize);

                    try
                    {
                        socketServer.Send(sendData_s);
                    }
                    catch (SocketException)
                    {
                        MessageBox.Show("서버 종료로 인해 클라이언트를 종료합니다.");
                        clientShutdown();
                    }

                    socketServer.Receive(recvData);

                    if (Encoding.UTF8.GetString(recvData).Equals("server finished"))
                    {
                        this.Invoke(new clientShutdownDelegate(clientShutdown));
                        break;
                    }
                }
                catch (SocketException) { }

                try
                {
                    using (MemoryStream ms = new MemoryStream(recvData))
                    {
                        using(MemoryStream post_ms = new MemoryStream())
                        {
                            using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
                            {
                                ds.CopyTo(post_ms);
                                ds.Close();
                            }
                            Image img = Image.FromStream(post_ms);
                            this.Invoke(new ThreadDelegate(outputDelegate), imgSize, img);

                            post_ms.Close();
                        }
                        ms.Close();
                    }
                }
                catch { }

                Array.Clear(recvData, 0, recvData.Length);
                Array.Clear(sendData_r, 0, sendData_r.Length);
                Array.Clear(sendData_s, 0, sendData_s.Length);
            }
        }

        public void outputDelegate(int imgSize, Image img)
        {
            label1.Text = imgSize.ToString();
            pictureBox1.Image = img;
        }

        public void clientShutdown()
        {
            clientShutdownFlag = true;

            socketServer.Close();
            Dispose();
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Invoke(new clientShutdownDelegate(clientShutdown));
        }

        private void Client_Load(object sender, EventArgs e)
        {
            try
            {
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("192.168.35.231"), 7979); // 192.168.31.218 // 192.168.31.200
                socketServer.Connect(serverEndPoint);

                clientReceiveThread = new Thread(() => receiveThread());
                clientReceiveThread.Start();

                clientControlThread = new Thread(() => runClientListenerThread());
                clientControlThread.Start();
                
            }
            catch (SocketException)
            {
                MessageBox.Show("서버가 아직 동작중이지 않습니다.", "서버 확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               
            }
        }
    }
}
