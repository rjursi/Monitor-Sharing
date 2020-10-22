﻿using System;
using System.Drawing;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading.Tasks;
using InternetControl;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Client
{
    public partial class Client : Form
    {
        private const int CLASSNETPORT = 53178;
        private string SERVER_IP = "";

        private Socket server;
        private bool isConnected = false;

        private TransparentForm transparentForm;
        private LoginForm loginForm;
        private string stuInfo; // 로그인 데이터를 담을 변수
        private bool isLogin = false;

        private static Action mainAction;

        private static SignalObj standardSignalObj;

        private TaskMgrController taskMgrController;
        private CmdProcessController cmdProcessController;
        private FirewallPortBlock firewallPortBlocker;

        private delegate void ScreenOnDelegate(int imgSize, Byte[] recvData, bool isShow);

        private static Byte[] recvData;
        private static Byte[] sendData;

        private static ImageCodecInfo codec;
        private static EncoderParameters param;

        private static bool isFirst;
        private static bool isCapture;

        public Client()
        {
            InitializeComponent();
        }

        // DPI 설정 부분 시작
        private enum ProcessDPIAwareness
        {
            ProcessDPIUnaware = 0,
            ProcessSystemDPIAware = 1,
            ProcessPerMonitorDPIAware = 2
        }

        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);

        private static void SetDpiAwareness()
        {
            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                    SetProcessDpiAwareness(ProcessDPIAwareness.ProcessPerMonitorDPIAware);
            }
            catch (EntryPointNotFoundException) { } // OS가 해당 API를 구현하지 않을 경우 예외가 발생하지만 무시
        }
        // DPI 설정 부분 끝

        private void Client_Load(object sender, EventArgs e)
        {
            if (ClassNetConfig.GetAppConfig("SERVER_IP").Equals(""))
            {
                MessageBox.Show("서버 IP 설정이 되어있지 않습니다. 서버 IP 설정 창으로 넘어갑니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);

                using (SetIPAddressForm setIPAddressForm = new SetIPAddressForm())
                {
                    var dialogResult = setIPAddressForm.ShowDialog();

                    if (dialogResult == DialogResult.OK)
                    {
                        ClassNetConfig.SetAppConfig("SERVER_IP", setIPAddressForm.ServerIP);
                    }
                }
            }

            this.SERVER_IP = ClassNetConfig.GetAppConfig("SERVER_IP");

            // DPI 설정 메소드 호출
            SetDpiAwareness();

            while (!isLogin)
            {
                loginForm = new LoginForm();
                loginForm.ShowDialog(); // ShowDialog 실행, 닫힐 때 까지 프로그램은 일시정지.
                stuInfo = loginForm.stuInfo; // 로그인 데이터를 변수에 담음.

                if (stuInfo.Length > 0) isLogin = true;
            }

            
            transparentForm = new TransparentForm();

            if (stuInfo.Equals(ClassNetConfig.GetAppConfig("ADMIN_ID")))
            {   
                transparentForm.FormStatus = TransparentForm.ADMINFORM;
                transparentForm.ShowDialog();
            }
            else
            {
                transparentForm.FormStatus = TransparentForm.USERFORM;
                
                transparentForm.Show();
                transparentForm.Hide();
            }
            
            while (!isConnected)
            {
                try
                {
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint ep = new IPEndPoint(IPAddress.Parse(SERVER_IP), CLASSNETPORT);
                    server.Connect(ep);

                    // 작업표시줄 상에서 프로그램이 표시되지 않도록 설정
                    this.ShowInTaskbar = false;

                    // 받은 이미지를 풀스크린으로 띄우는 설정
                    /*FormBorderStyle = FormBorderStyle.None;
                    WindowState = FormWindowState.Maximized;
                    screenImage.Width = Screen.PrimaryScreen.Bounds.Width;
                    screenImage.Height = Screen.PrimaryScreen.Bounds.Height;*/

                    // 화면 폼을 가장 맨 위로
                    TopMost = true;

                    isConnected = true;
                }
                catch (SocketException)
                {
                    isConnected = false; // 연결이 안 되면 대기상태 유지
                }
            }

            NotifyIconSetting();
            taskMgrController = new TaskMgrController();
            cmdProcessController = new CmdProcessController();
            firewallPortBlocker = new FirewallPortBlock();

            taskMgrController.KillTaskMgr();
            recvData = new Byte[327675]; // 327,675 Byte = 65,535 Byte * 5
            isFirst = true;
            isCapture = false;

            // JPEG 손실 압축 수준 설정
            codec = GetEncoder(ImageFormat.Jpeg);
            param = new EncoderParameters();
            param.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 30L);

            this.Hide();

            // 이런식으로 구현 기능에 대한 메소드를 추가하기 위해 아래와 같이 람다식으로 작성
            InsertAction(() => ControllingLock());
            InsertAction(() => ControllingInternet());
            InsertAction(() => ControllingPower());
            InsertAction(() => ImageProcessing());
            InsertAction(() => CaptureProcessing());
            InsertAction(() => ControllingTaskMgr());

            Task.Run(() => MainTask());
        }
       
        public void InsertAction(Action action)
        {
            mainAction += action;
        }

        // 이미지 파일 형식(포맷) 인코더
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public SignalObj ByteToObject(Byte[] buffer)
        {
            string jsonData = Encoding.Default.GetString(buffer);
            SignalObj signal = JsonConvert.DeserializeObject<SignalObj>(jsonData);
            return signal;
        }

        public SignalObj ReceiveObject()
        {
            try
            {
                if (isFirst)
                {
                    sendData = Encoding.UTF8.GetBytes("info" + "&" + stuInfo);
                    isFirst = false;
                }
                else if (isCapture)
                {
                    sendData = CaptureImage();
                    isCapture = false;
                }
                else sendData = Encoding.UTF8.GetBytes("recv");

                server.Send(sendData);
                server.Receive(recvData);

                return ByteToObject(recvData);
            }
            catch (SocketException)
            {
                server.Close();
                if (cmdProcessController.NowCtrlStatus) cmdProcessController.QuitProcess();
                taskMgrController.CheckTaskMgrStatus(true);
                this.Invoke(new MethodInvoker(() => { Dispose(); }));

                return null;
            }
        }

        public void MainTask()
        {
            while (true)
            {
                try
                {
                    using (standardSignalObj = ReceiveObject())
                    {
                        if (standardSignalObj != null) mainAction();
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (JsonReaderException)
                {
                    break;
                }
                finally
                {
                    Array.Clear(recvData, 0, recvData.Length);
                }
            }
        }

        private void NotifyIconSetting()
        {
            switch (transparentForm.FormStatus)
            {
                case TransparentForm.ADMINFORM:
                    setAdminFormTrayIcon();
                    break;
                case TransparentForm.USERFORM:
                    setUserFormTrayIcon();
                    break;
            }
        }

        public void ControllingTaskMgr()
        {
            taskMgrController.CheckTaskMgrStatus(standardSignalObj.IsTaskMgrEnabled);
        }
        public void ControllingLock()
        {
            cmdProcessController.CtrlStatusEventCheck(standardSignalObj.IsLock);
        }

        public void ControllingInternet()
        {
            firewallPortBlocker.CtrlStatusEventCheck(standardSignalObj.IsInternet);
        }

        public void ControllingPower()
        {
            if (standardSignalObj.IsPower) System.Diagnostics.Process.Start("ShutDown.exe", "-s -f -t 00");
        }

        public void ImageProcessing()
        {
            if(standardSignalObj.ServerScreenData != null)
            {
                // 이미지를 받아서 여기서 버퍼를 설정하는 부분
                this.Invoke(new ScreenOnDelegate(OutputDelegate),
                    standardSignalObj.ServerScreenData.Length, standardSignalObj.ServerScreenData, true);
            }
            else this.Invoke(new ScreenOnDelegate(OutputDelegate), 0, null, false);
        }

        public void CaptureProcessing()
        {
            if (standardSignalObj.IsMonitoring) isCapture = true;
        }

        public Byte[] CaptureImage()
        {
            Byte[] preData;
            Byte[] tempData;

            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(0, 0, 0, 0, new Size(bmp.Width, bmp.Height));

            using (MemoryStream pre_ms = new MemoryStream())
            {
                bmp.Save(pre_ms, codec, param);
                preData = pre_ms.ToArray();

                using (MemoryStream post_ms = new MemoryStream())
                {
                    using (DeflateStream ds = new DeflateStream(post_ms, CompressionMode.Compress))
                    {
                        ds.Write(preData, 0, preData.Length);
                        ds.Close();
                    }
                    tempData = post_ms.ToArray();
                    post_ms.Close();
                }
                pre_ms.Close();
            }
            return tempData;
        }

        public void OutputDelegate(int imgSize, Byte[] recvData, bool isShow)
        {
            if (!isShow) this.Hide();
            else
            {
                this.Show();

                using (MemoryStream pre_ms = new MemoryStream(recvData))
                {
                    using (MemoryStream post_ms = new MemoryStream())
                    {
                        using (DeflateStream ds = new DeflateStream(pre_ms, CompressionMode.Decompress))
                        {
                            try
                            {
                                ds.CopyTo(post_ms);
                            }
                            finally
                            {
                                ds.Close();
                            }
                        }
                        imageSize.Text = imgSize.ToString();
                        screenImage.Image = Image.FromStream(post_ms);

                        post_ms.Close();
                    }
                    pre_ms.Close();
                }
            }
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            taskMgrController.CheckTaskMgrStatus(true);
            server.Close();
            this.Invoke(new MethodInvoker(() => { Dispose(); }));
        }

        private void setUserFormTrayIcon()
        {
            ContextMenu ctx = new ContextMenu();
            ctx.MenuItems.Add(new MenuItem("로그아웃", new EventHandler((s, ea) => BtnLogout_Click(s, ea))));

            this.notifyIcon.ContextMenu = ctx;
            this.notifyIcon.Visible = true;
        }

        private void setAdminFormTrayIcon()
        {
            ContextMenu ctx = new ContextMenu();
            ctx.MenuItems.Add(new MenuItem("설정", new EventHandler((s, ea) => BtnSetServerIP_Click(s, ea))));
            ctx.MenuItems.Add(new MenuItem("로그아웃", new EventHandler((s, ea) => BtnLogout_Click(s, ea))));

            this.notifyIcon.ContextMenu = ctx;
            this.notifyIcon.Visible = true;
        }

        private void BtnSetServerIP_Click(object sender, EventArgs ea)
        {
            using (SetIPAddressForm setIPAddressForm = new SetIPAddressForm())
            {
                var dialogResult = setIPAddressForm.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    ClassNetConfig.SetAppConfig("SERVER_IP", setIPAddressForm.ServerIP);

                    MessageBox.Show("서버 IP가 수정되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnLogout_Click(object sender, EventArgs ea)
        {
            this.DialogResult = DialogResult.OK;

            this.Close();
        }
    }
}
