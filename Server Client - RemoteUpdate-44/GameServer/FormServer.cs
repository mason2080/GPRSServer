using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

namespace GameServer
{
    public partial class FormServer : Form
    {
        //游戏室允许进入的最多人数
        private int maxUsers;
        //连接的用户
        System.Collections.Generic.List<User> userList = new List<User>();
        System.Collections.Generic.List<User> userList1 = new List<User>();
        //游戏室开出的桌数
        private int maxTables;
        private GameTable[] gameTable;
        //使用的本机ip地址
        IPAddress localAddress;
        //监听端口
        private int port = 9900;
        private int port1 = 9901;
        private TcpListener myListener;
        private TcpListener myListener1;
        private Service service;
        private Service service1;
        Log loginfo=new Log();
        HexStringToByte hexStringToByte = new HexStringToByte();
        

        string str ;
        string imei;
        string fileName;
        int SendingIndex = 0;
        int SendNextIndex = 0;

        public FormServer()
        {
            InitializeComponent();
            service = new Service(listBox1);
            service1 = new Service(listBox2);
            CheckForIllegalCrossThreadCalls = false;
        }

        //加载窗体时触发的事件
        protected override void OnLoad(EventArgs e)
        {
            listBox1.HorizontalScrollbar = true;
            IPAddress[] addrIP = Dns.GetHostAddresses(Dns.GetHostName());
            localAddress = addrIP[0];
           // localAddress = IPAddress.Parse("10.14.3.51");
            localAddress = IPAddress.Parse("10.14.0.16");
            base.OnLoad(e);
           
        }

        //【开始服务】按钮的Click事件
        private void buttonStart_Click(object sender, EventArgs e)
        {
            maxTables = 50;
            maxUsers = 200;

            //if (maxUsers < 1 || maxUsers > 300)
            //{
            //    MessageBox.Show("允许进入的人数只能在1-300之间");
            //    return;
            //}
            //if (maxTables < 1 || maxTables > 100)
            //{
            //    MessageBox.Show("允许的桌数只能在1-100之间");
            //    return;
            //}
            //textBoxMaxUsers.Enabled = false;
            //textBoxMaxTables.Enabled = false;
            ////初始化数组
            //gameTable = new GameTable[maxTables];
            //for (int i = 0; i < maxTables; i++)
            //{
            //    gameTable[i] = new GameTable(listBox1);
            //}
            myListener = new TcpListener(localAddress, port);
            myListener.Start();
            service.SetListBox(string.Format("开始在{0}:{1}监听客户连接", localAddress, port));
            //创建一个线程监听客户端连接请求
            ThreadStart ts = new ThreadStart(ListenClientConnect);
            Thread myThread = new Thread(ts);
            myThread.Start();
            ////////////////////////////////////////
            myListener1 = new TcpListener(localAddress, port1);
            myListener1.Start();
            service1.SetListBox(string.Format("开始在{0}:{1}监听客户连接", localAddress, port1));
            //创建一个线程监听客户端连接请求
            ThreadStart ts1= new ThreadStart(ListenClientConnect1);
            Thread myThread1 = new Thread(ts1);
            myThread1.Start();


            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
        }

        //【停止服务】按钮的Click事件
        private void buttonStop_Click(object sender, EventArgs e)
        {
            //for (int i = 0; i < maxTables; i++) 
            //{
            //    gameTable[i].StopTimer();
            //}

            service.SetListBox(string.Format("目前连接用户数：{0}", userList.Count));
            service.SetListBox("开始停止服务，并依次使用户退出!");
            for (int i = 0; i < userList.Count; i++)
            {
                //关闭后，客户端接收字符串为null,
                //使接收该客户的线程ReceiveData接收的字符串也为null,
                //从而达到结束线程的目的
                try
                {
                    userList[i].client.Close();
                }
                catch { }
            }

            service1.SetListBox(string.Format("目前连接用户数：{0}", userList1.Count));
            service1.SetListBox("开始停止服务，并依次使用户退出!");
            for (int i = 0; i < userList1.Count; i++)
            {
                //关闭后，客户端接收字符串为null,
                //使接收该客户的线程ReceiveData接收的字符串也为null,
                //从而达到结束线程的目的
                try
                {
                    userList1[i].client.Close();
                }
                catch { }
            }
            //通过停止监听让myListener.AcceptTcpClient()产生异常退出监听线程
            myListener.Stop();
            myListener1.Stop();

            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            //textBoxMaxTables.Enabled = true;
            //textBoxMaxUsers.Enabled = true;
        }

        //接收客户端连接
        private void ListenClientConnect() 
        {
            while (true) 
            {
                TcpClient newClient = null;
                Socket newSocketClient = null;
                try
                {
                    //等待用户进入
                    //newClient = myListener.AcceptTcpClient();

                    newSocketClient=myListener.AcceptSocket();
                }
                catch 
                {
                    //当单击"停止监听"或者退出窗体acceptTcpClient()会产生异常
                    //因此可以利用此异常退出循环
                    break;
                }

                //每接受一个客户端连接,就创建一个对应的线程循环接收该客户端发来的信息
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ReceiveData);
                Thread threadReceive = new Thread(pts);

                ParameterizedThreadStart pts1 = new ParameterizedThreadStart(TranslateData);
                Thread threadTranslate = new Thread(pts1);

               // User user = new User(newClient);
                User user = new User(newSocketClient);
                threadReceive.Start(user);
                threadTranslate.Start(user);

                userList.Add(user);
                service.SetListBox(string.Format("{0}进入", newSocketClient.RemoteEndPoint));
                service.SetListBox(string.Format("当前连接用户数：{0}", userList.Count));
            }
        }

        //接收客户端连接
        private void ListenClientConnect1()
        {
            while (true)
            {
                TcpClient newClient = null;
                Socket newSocketClient = null;
                try
                {
                    //等待用户进入
                    //newClient = myListener.AcceptTcpClient();

                    newSocketClient = myListener1.AcceptSocket();
                }
                catch
                {
                    //当单击"停止监听"或者退出窗体acceptTcpClient()会产生异常
                    //因此可以利用此异常退出循环
                    break;
                }

                //每接受一个客户端连接,就创建一个对应的线程循环接收该客户端发来的信息
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ReceiveData1);
                Thread threadReceive = new Thread(pts);

                ParameterizedThreadStart pts1 = new ParameterizedThreadStart(TranslateData1);
                Thread threadTranslate = new Thread(pts1);

                // User user = new User(newClient);
                User user = new User(newSocketClient);
                threadReceive.Start(user);
                threadTranslate.Start(user);

                userList1.Add(user);
                service1.SetListBox(string.Format("{0}进入", newSocketClient.RemoteEndPoint));
                service1.SetListBox(string.Format("当前连接用户数：{0}", userList1.Count));
            }
        }

        public byte[] HexStringToByteArray(string s)
        {
            byte[] buffer = new byte[s.Length];
            for (int i = 0; i < s.Length; i ++)
                buffer[i] = (byte)Convert.ToByte(s.Substring(i, 1), 16);
            return buffer;
        }


                /// <summary>
        /// 解析客户端发送的信息
        /// </summary>
        /// <param name="obj"></param>
        private void TranslateData(object obj)
        {
            User user = (User)obj;
            List<byte> data = new List<byte>();
            int msgLen=0;
            string gprsInfo = "" ;
            int sumRows = 0;
            int sumPacks=0;
            byte[] rowMsg = new byte[44*48];
            bool flagFirstTime = false;

            while(true)
            {
                Thread.Sleep(10);
                if (user.GPRSMsgQueue.Count > 0)
                {
                    byte[] recvData = user.GPRSMsgQueue.Dequeue();
                    string temp = recvData.ToString();
                    data.AddRange(recvData);
                }
                if(data.Count>10)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if ((data[i] == '$') && (data[i + 1] == 'G') && (data[i + 2] == 'P') && (data[i + 3] == 'R') && (data[i + 4] == 'S'))
                        {
                            msgLen = data[i + 5] * 256 + data[i + 6];
                            try
                            {
                                if ((data[i + msgLen + 2] == '#') && (data[i + msgLen + 3] == 'E') && (data[i + msgLen + 4] == 'N') && (data[i + msgLen + 5] == 'D'))
                                {
                                    if (user.sqlDb.TranslateBMSMsg(data.GetRange(i, msgLen + 6), ref  gprsInfo,ref user.IMEI) == 1)
                                    {
                                        service.SetListBox(gprsInfo);
                                    }
                                    data.RemoveRange(0, i + msgLen + 6);
                                }
                            }
                            catch 
                            {
                            }
                        }
                        else if ((data[i] == '$') && (data[i + 1] == 'M') && (data[i + 2] == 'S') && (data[i + 3] == 'G') && (data[i + 4] == '1'))
                        {
                            msgLen = data[i + 5] * 256 + data[i + 6];
                            try
                            {
                                if ((data[i + msgLen + 2] == '#') && (data[i + msgLen + 3] == 'E') && (data[i + msgLen + 4] == 'N') && (data[i + msgLen + 5] == 'D'))
                                {
                                    if (System.Text.Encoding.Default.GetString(data.ToArray(), i + 7, 13) == "ENTERUPDATEMD")
                                    {
                                        int sendingIndex =  data[i + 20] * 0x100 + data[i + 21];
                                        for (int j = 0; j < userList1.Count; j++)
                                        {
                                            if (userList1[j].IMEI == user.IMEI)
                                            {
                                                //GprsClient = userList[j];
                                                string msg;
                                                if (sendingIndex == 0xffff)
                                                {
                                                    msg = "MSG0UpdateOk...";
                                                    msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0UpdateOk..." + "#END";

                                                    user.socketClient.Send(System.Text.Encoding.Default.GetBytes("monitormode"));//DownLoadFileMode

                                                }
                                                else
                                                {
                                                    msg = "MSG0UPDATEMODE..." + sendingIndex.ToString().PadLeft(4,'0')+"/"+user.rowList.Count.ToString().PadLeft(4,'0');
                                                    msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0UPDATEMODE..." + sendingIndex.ToString().PadLeft(4, '0') + "/" + user.rowList.Count.ToString().PadLeft(4, '0') + "#END";
                                                }


                                                try
                                                {
                                                    userList1[j].socketClient.Send(System.Text.Encoding.Default.GetBytes(msg));
                                                    user.recvTimes = 0;
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                                data.RemoveRange(0, i + msgLen + 6);

                            }
                            catch
                            { }
                        }
                        else if ((data[i] == '$') && (data[i + 1] == 'M') && (data[i + 2] == 'S') && (data[i + 3] == 'G') && (data[i + 4] == '6'))//Config Result
                        {
                            msgLen = data[i + 5] * 256 + data[i + 6];
                            try
                            {
                                if ((data[i + msgLen + 2] == '#') && (data[i + msgLen + 3] == 'E') && (data[i + msgLen + 4] == 'N') && (data[i + msgLen + 5] == 'D'))
                                {
                                    if (System.Text.Encoding.Default.GetString(data.ToArray(), i + 7, 4) == "READ")
                                    {
                                       // int sendingIndex = data[i + 20] * 0x100 + data[i + 21];
                                        for (int j = 0; j < userList1.Count; j++)
                                        {
                                            if (userList1[j].IMEI == user.IMEI)
                                            {
                                                //GprsClient = userList[j];
                                                byte[] cmd=new byte[100];
                                                byte p=0;

                                                cmd[p++] = (byte)'S';
                                                cmd[p++] = (byte)'T';
                                                cmd[p++] = (byte)'A';
                                                cmd[p++] = (byte)'R';
                                                cmd[p++] = (byte)'T';
                                                cmd[p++] = (byte)'7';
                                                cmd[p++] = (byte)'3';
                                                cmd[p++] = (byte)'M';
                                                cmd[p++] = (byte)'S';
                                                cmd[p++] = (byte)'G';
                                                cmd[p++] = (byte)'0';
                                                cmd[p++] = (byte)'R';
                                                cmd[p++] = (byte)'E';
                                                cmd[p++] = (byte)'A';
                                                cmd[p++] = (byte)'D';
                                                for ( i = 0; i < 65; i++)
                                                {
                                                    cmd[p++] =data[i + 11];
                                                }
                                                cmd[p++] = (byte)'#';
                                                cmd[p++] = (byte)'E';
                                                cmd[p++] = (byte)'N';
                                                cmd[p++] = (byte)'D';

                                                try
                                                {
                                                    userList1[j].socketClient.Send(cmd,p,SocketFlags.None);
                                                    user.recvTimes = 0;
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                                data.RemoveRange(0, i + msgLen + 6);

                            }
                            catch
                            { }
                        }
                        else if ((data[i] == '$') && (data[i + 1] == 'M') && (data[i + 2] == 'S') && (data[i + 3] == 'G') && (data[i + 4] == '5'))//Config Result
                        {
                            msgLen = data[i + 5] * 256 + data[i + 6];
                            try
                            {
                                if ((data[i + msgLen + 2] == '#') && (data[i + msgLen + 3] == 'E') && (data[i + msgLen + 4] == 'N') && (data[i + msgLen + 5] == 'D'))
                                {
                                    if (System.Text.Encoding.Default.GetString(data.ToArray(), i + 7, 12) == "CONFIGRESULT")
                                    {
                                    
                                        for (int j = 0; j < userList1.Count; j++)
                                        {
                                            if (userList1[j].IMEI == user.IMEI)
                                            {
                                                //GprsClient = userList[j];
                                                byte[] cmd = new byte[50];
                                                byte p = 0;

                                                cmd[p++] = (byte)'S';
                                                cmd[p++] = (byte)'T';
                                                cmd[p++] = (byte)'A';
                                                cmd[p++] = (byte)'R';
                                                cmd[p++] = (byte)'T';
                                                cmd[p++] = (byte)'1';
                                                cmd[p++] = (byte)'8';
                                                cmd[p++] = (byte)'M';
                                                cmd[p++] = (byte)'S';
                                                cmd[p++] = (byte)'G';
                                                cmd[p++] = (byte)'0';
                                                cmd[p++] = (byte)'C';
                                                cmd[p++] = (byte)'O';
                                                cmd[p++] = (byte)'N';
                                                cmd[p++] = (byte)'F';
                                                cmd[p++] = (byte)'I';
                                                cmd[p++] = (byte)'G';
                                                cmd[p++] = (byte)'R';
                                                cmd[p++] = (byte)'E';
                                                cmd[p++] = (byte)'S';
                                                cmd[p++] = (byte)'U';
                                                cmd[p++] = (byte)'L';
                                                cmd[p++] = (byte)'T';
                                                cmd[p++] = data[i + 19];
                                                cmd[p++] = data[i + 20];
                                                cmd[p++] = (byte)'#';
                                                cmd[p++] = (byte)'E';
                                                cmd[p++] = (byte)'N';
                                                cmd[p++] = (byte)'D';

                                                try
                                                {
                                                    userList1[j].socketClient.Send(cmd, p, SocketFlags.None);
                                                    user.recvTimes = 0;
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                                data.RemoveRange(0, i + msgLen + 6);

                            }
                            catch
                            { }
                        }
                        else if ((data[i] == '$') && (data[i + 1] == 'M') && (data[i + 2] == 'S') && (data[i + 3] == 'G') && (data[i + 4] == '4'))
                        {
                            msgLen = data[i + 5] * 256 + data[i + 6];
                            try
                            {
                                if ((data[i + msgLen + 2] == '#') && (data[i + msgLen + 3] == 'E') && (data[i + msgLen + 4] == 'N') && (data[i + msgLen + 5] == 'D'))
                                {
                                    if (System.Text.Encoding.Default.GetString(data.ToArray(), i + 7, 13) == "CHECKFILEMODE")
                                    {                                                                         
                                        int crc = data[i + 20] * 0x100 + data[i + 21];
                                        int sum = data[i + 22] * 0x100 + data[i + 23];

                                        for (int j = 0; j < userList1.Count; j++)
                                        {
                                            if (userList1[j].IMEI == user.IMEI)
                                            {
                                                //GprsClient = userList[j];
                                                string msg;
                                                if((crc==user.CRC)&&(sum== user.rowList.Count))
                                                {
                                                    msg = "MSG0CheckOk...";
                                                    msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0CheckOk..." + "#END";

                                                    user.socketClient.Send(System.Text.Encoding.Default.GetBytes("monitormode"));//DownLoadFileMode

                                                }
                                                else
                                                {
                                                    msg = "MSG0CheckNG...";
                                                    msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0CheckNG..." + "#END";

                                                    user.socketClient.Send(System.Text.Encoding.Default.GetBytes("monitormode"));//DownLoadFileMode
                                                }


                                                try
                                                {
                                                    userList1[j].socketClient.Send(System.Text.Encoding.Default.GetBytes(msg));
                                                    user.recvTimes = 0;
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                }
                                data.RemoveRange(0, i + msgLen + 6);

                            }
                            catch
                            { }
                        }
                        else if ((data[i] == '$') && (data[i + 1] == 'M') && (data[i + 2] == 'S') && (data[i + 3] == 'G') && (data[i + 4] == '0'))
                        {
                            msgLen = data[i + 5] * 256 + data[i + 6];
                            try
                            {
                                if ((data[i + msgLen + 2] == '#') && (data[i + msgLen + 3] == 'E') && (data[i + msgLen + 4] == 'N') && (data[i + msgLen + 5] == 'D'))
                                {
                                    if (System.Text.Encoding.Default.GetString(data.ToArray(), i + 7, 13) == "ENTERDOWNMODE")
                                    {
                                        int requesPackNo = data[i + 20] * 0x1000000 + data[i + 21] * 0x10000 + data[i + 22] * 0x100 + data[i + 23];

                                        if (requesPackNo == 0xffff)
                                        {
                                        }
                                        else
                                        {

                                            sumPacks = user.rowList.Count / 48;

                                            if (user.rowList.Count % 48 != 0)
                                            {
                                                sumPacks++;
                                            }



                                            if (requesPackNo <= sumPacks)
                                            {
                                                for (int n = 0; n < 48; n++)
                                                {
                                                    rowMsg[0 + n * 44] = (byte)'#';
                                                    rowMsg[1 + n * 44] = (byte)'&';
                                                    int index = (requesPackNo - 1) * 48 + n;
                                                    rowMsg[2 + n * 44] = (byte)((index + 1) >> 8);
                                                    rowMsg[3 + n * 44] = (byte)(index + 1);

                                                    for (int k = 0; k < 38; k++)
                                                    {
                                                        if (index < user.rowList.Count)
                                                        {
                                                            if (k < user.rowList[index].GetLength(0))
                                                            {
                                                                try
                                                                {
                                                                    rowMsg[4 + k + n * 44] = (byte)user.rowList[index].GetValue(k);// temp[k];
                                                                }
                                                                catch { }
                                                            }
                                                            else
                                                            {
                                                                rowMsg[4 + k + n * 44] = (byte)('*');
                                                            }
                                                        }
                                                    }
                                                    rowMsg[42 + n * 44] = (byte)'@';
                                                    rowMsg[43 + n * 44] = (byte)'$';


                                                    //user.socketClient.Send(rowMsg);
                                                    Thread.Sleep(1);
                                                    //  Thread.Sleep(1);
                                                }
                                                user.socketClient.Send(rowMsg);
                                            }
                                        }

                                        if (userList1.Count > 0)
                                        {
                                            for (int j = 0; j < userList1.Count; j++)
                                            {
                                                if (userList1[j].IMEI == user.IMEI)
                                                {
                                                    //GprsClient = userList[j];
                                                    string msg;
                                                    if (requesPackNo == 0xffff)
                                                    {
                                                        msg = "MSG0DownLoadOk...";
                                                        msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0DownLoadOk..." + "#END";

                                                        user.socketClient.Send(System.Text.Encoding.Default.GetBytes("monitormode"));//DownLoadFileMode
                                                        user.socketClient.Send(System.Text.Encoding.Default.GetBytes("monitormode"));//DownLoadFileMode
                                                    }
                                                    else
                                                    {
                                                        msg = "MSG0DOWNMODE....." + (requesPackNo*48).ToString().PadLeft(4, '0') + "/" + user.rowList.Count.ToString().PadLeft(4, '0');
                                                        msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0DOWNMODE....." + (requesPackNo * 48).ToString().PadLeft(4, '0') + "/" + user.rowList.Count.ToString().PadLeft(4, '0') + "#END";
                                                    }


                                                    try
                                                    {
                                                        userList1[j].socketClient.Send(System.Text.Encoding.Default.GetBytes(msg));
                                                        user.recvTimes = 0;
                                                    }
                                                    catch { }

                                                    //  break;
                                                }
                                                else
                                                {
                                                    if (j == userList1.Count - 1)
                                                    {
                                                        string msg1 = "MSG0OffLine...";
                                                        msg1 = "START" + msg1.Length.ToString().PadLeft(2, '0') + "MSG0OffLine..." + "#END";

                                                        try
                                                        {
                                                            userList1[j].socketClient.Send(System.Text.Encoding.Default.GetBytes(msg1));
                                                            user.recvTimes = 0;
                                                        }
                                                        catch { }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    data.RemoveRange(0, i + msgLen + 6);
                                }
                            }
                            catch { }
                        }
                    }
 }

            }
        }

        private void TranslateData1(object obj)
        {
            User user = (User)obj;
            User GprsClient=null;
            List<byte> data = new List<byte>();
            int msgLen = 0;
            string temp;
            UInt16 crc = 0;
            int sumRows = 0;
            while (user.exitWhile==false)
            {
                Thread.Sleep(20);

                if (user.GPRSMsgQueue1.Count > 0)
                {
                    byte[] recvData = user.GPRSMsgQueue1.Dequeue();
                  
                    data.AddRange(recvData);
                    DateTime dataTime = DateTime.Now;
                    service.SetListBox(dataTime.ToString() + recvData.Length.ToString());
                }

               if (data.Count > 4)
               {
                
                    for (int i = 0; i < data.Count; i++)
                    {
                        if ((data[i] == 'S') && (data[i + 1] == 'T') && (data[i + 2] == 'A') && (data[i + 3] == 'R') && (data[i + 4] == 'T')
                            && (data[i + 5] == 'S') && (data[i + 6] == 'E') && (data[i + 7] == 'T') && (data[i + 48] == '#') && (data[i + 49] == 'E') && (data[i + 50] == 'N') && (data[i + 51] == 'D'))
                        {//转发参数设置命令
                            byte[] cmd=new byte[44];
                            byte[] cmd1 = new byte[40];
                            cmd[0] = (byte)'&';
                            cmd[1] = (byte)'&';

                            for (int k = 0; k < 40; k++)
                            {
                                cmd[2 + k] = data[i+8+k];// temp[k];
                                cmd1[k] = data[i + 8 + k];
                            }
                            cmd[42] = (byte)'@';
                            cmd[43] = (byte)'@';

                            for (int j = 0; j < userList.Count; j++)
                            {
                                if (userList[j].IMEI == user.IMEI)
                                {
                                    try
                                    {
                                        userList[j].socketClient.Send(cmd);
                                        System.DateTime dt = DateTime.Now;
                                        loginfo.LogUserControlMsg(user.IMEI, dt.ToLongDateString() + dt.ToLongTimeString() +":IP: "+user.IP+" :SetCmd:" + hexStringToByte.byteToHexStr(cmd1));
                                    }
                                    catch { }
                                }
                            }

                            data.RemoveRange(0, i + 51);

                            break;
                        }

                        if ((data[i] == 'S') && (data[i + 1] == 'T') && (data[i + 2] == 'A') && (data[i + 3] == 'R') && (data[i + 4] == 'T')
                       && (data[i + 5] == 'R') && (data[i + 6] == 'E') && (data[i + 7] == 'A') && (data[i + 8] == 'D') && (data[i + 10] == '#') && (data[i + 11] == 'E') && (data[i + 12] == 'N') && (data[i + 13] == 'D'))
                        {//转发参数设置命令
                            byte[] cmd = new byte[44];
                            cmd[0] = (byte)'$';
                            cmd[1] = (byte)'$';
                            cmd[2] = (byte)'R';
                            cmd[3] = (byte)'E';
                            cmd[4] = (byte)'A';
                            cmd[5] = (byte)'D';
                            cmd[6] = data[i + 9];
                            cmd[7] = (byte)'#';
                            cmd[8] = (byte)'#';

                            for (int j = 0; j < userList.Count; j++)
                            {
                                if (userList[j].IMEI == user.IMEI)
                                {
                                    try
                                    {
                                        System.DateTime dt = DateTime.Now;

                                        loginfo.LogUserControlMsg(user.IMEI, dt.ToLongDateString() + dt.ToLongTimeString() + ":IP: " + user.IP + ":ReadCmd:" + data[i + 9].ToString("X"));
                                        userList[j].socketClient.Send(cmd);
                                    }
                                    catch { }
                                }
                            }
                            data.RemoveRange(0, i + 13);
                            break;
                        }

                        if ((data[i] == 'S') && (data[i + 1] == 'T') && (data[i + 2] == 'A') && (data[i + 3] == 'R') && (data[i + 4] == 'T'))
                        {
                            msgLen = (int.Parse(data[i + 5].ToString()) - 0x30) * 10 + int.Parse(data[i + 6].ToString())-0x30;

                            try
                            {
                                if ((data[i + msgLen + 7] == '#') && (data[i + msgLen + 8] == 'E') && (data[i + msgLen + 9] == 'N') && (data[i + msgLen + 10] == 'D'))
                                {
                                    //DateTime dataTime = DateTime.Now;
                                    if ((data[7] == 'C') && (data[8] == 'M') && (data[9] == 'D') && (data[10] == '0'))
                                    {
                                        str = System.Text.Encoding.Default.GetString(data.ToArray(), 0, msgLen + 11);
                                        user.IMEI= str.Substring(str.IndexOf("CMD0") + 4, str.IndexOf("FILENAME") - str.IndexOf("CMD0") - 4);
                                        user.fileName = str.Substring(str.IndexOf("FILENAME") + 8, str.IndexOf("#END") - str.IndexOf("FILENAME") - 8);
                                        service1.SetListBox("Upload FIle To GPRS Module..");
                                        service1.SetListBox(user.IMEI);
                                        service1.SetListBox(user.fileName);
                                        data.RemoveRange(0, i + msgLen + 11);
                                        Thread.Sleep(5);

                                        if (userList.Count > 0)
                                        {
                                            for (int j = 0; j < userList.Count; j++)
                                            {
                                                if (userList[j].IMEI == user.IMEI)
                                                {

                                                    System.DateTime dt = DateTime.Now;
                                                    loginfo.LogUserControlMsg(user.IMEI, dt.ToLongDateString() + dt.ToLongTimeString() + ":IP: " + user.IP + ":Upload FIle To GPRS Module" + user.fileName);

                                                    userList[j].fileName = user.fileName;
                                                    GprsClient = userList[j];
                                                    string msg = "MSG0OnLine...";
                                                    msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0OnLine..." + "#END";
                                                    user.socketClient.Send(System.Text.Encoding.Default.GetBytes(msg));

                                                    FileStream filest = new FileStream(@"D:\" + user.fileName, FileMode.Open, FileAccess.ReadWrite);
                                                    StreamReader sr = new StreamReader(filest, Encoding.Default);

                                                    string row = "";
                                                    userList[j].rowList.Clear();
                                                    userList[j].CRC = 0;
                                                    for (int k = 0; k< 10000; k++)
                                                    {
                                                        row = sr.ReadLine();
                                                        if (row.Substring(0, 2) == "S0")//去首行
                                                        {
                                                           userList[j].sumRows = 0;//清行数
                                                        }
                                                        else if (row.Substring(0, 2) == "S9")//去尾行
                                                        {
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            //userList[j].rowInfo[userList[j].sumRows++] = hexStringToByte.decodeHex(row).ToString();
                                                            userList[j].rowList.Add(hexStringToByte.decodeHex(row, ref userList[j].CRC));
                                                            crc = userList[j].CRC;
                                                        }

                                                    }
                                                    sumRows = userList[j].rowList.Count;

                                                    sr.Close();
                                                    filest.Close();
                                                }
                                                else
                                                {
                                                    //if (j == userList.Count - 1)
                                                    //{
                                                    //    string msg1 = "MSG0OffLine...";
                                                    //    msg1 = "START" + msg1.Length.ToString().PadLeft(2, '0') + "MSG0OffLine..." + "#END";
                                                    //    user.socketClient.Send(System.Text.Encoding.Default.GetBytes(msg1));
                                                    //}
                                                }
                                            }
                                        }
                                        else
                                        {
                                            string msg1 = "MSG0OffLine...";
                                            msg1 = "START" + msg1.Length.ToString().PadLeft(2, '0') + "MSG0OffLine..." + "#END";
                                            user.socketClient.Send(System.Text.Encoding.Default.GetBytes(msg1));
                                        }

                                    }
                                    if ((data[7] == 'C') && (data[8] == 'M') && (data[9] == 'D') && (data[10] == '5'))
                                    {
                                        str = System.Text.Encoding.Default.GetString(data.ToArray(), 0, msgLen + 11);
                                        user.IMEI = str.Substring(str.IndexOf("CMD5") + 4, str.IndexOf("FILENAME") - str.IndexOf("CMD5") - 4);
                                        user.fileName = str.Substring(str.IndexOf("FILENAME") + 8, str.IndexOf("#END") - str.IndexOf("FILENAME") - 8);
                                        service1.SetListBox("Upload FIle To GPRS Module..");
                                        service1.SetListBox(user.IMEI);
                                        service1.SetListBox(user.fileName);
                                        data.RemoveRange(0, i + msgLen + 11);
                                        Thread.Sleep(5);

                                        if (userList.Count > 0)
                                        {
                                            for (int j = 0; j < userList.Count; j++)
                                            {
                                                if (userList[j].IMEI == user.IMEI)
                                                {
                                                    userList[j].fileName = user.fileName;
                                                    GprsClient = userList[j];
                                                    string msg = "MSG0OnLine...";
                                                    msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0OnLine..." + "#END";
                                                    user.socketClient.Send(System.Text.Encoding.Default.GetBytes(msg));
                                                }
                                                else
                                                {

                                                }
                                            }
                                        }
                                        else
                                        {
                                            string msg1 = "MSG0OffLine...";
                                            msg1 = "START" + msg1.Length.ToString().PadLeft(2, '0') + "MSG0OffLine..." + "#END";
                                            user.socketClient.Send(System.Text.Encoding.Default.GetBytes(msg1));
                                        }

                                    }
                                    else if ((data[7] == 'C') && (data[8] == 'M') && (data[9] == 'D') && (data[10] == '1'))
                                    {
                                        string msg = "MSG0准备进入文件下载模式...";
                                        msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0准备进入文件下载模式..." + "#END";
                                        data.RemoveRange(0, i + msgLen + 11);
                                        user.socketClient.Send(System.Text.Encoding.Default.GetBytes(msg));
                                        byte[] cmd = new byte[15];
                                        cmd[0] = (byte)'d';
                                        cmd[1] = (byte)'o';
                                        cmd[2] = (byte)'w';
                                        cmd[3] = (byte)'n';
                                        cmd[4] = (byte)'m';
                                        cmd[5] = (byte)'o';
                                        cmd[6] = (byte)'d';
                                        cmd[7] = (byte)'e';

                                        cmd[8] = (byte)(crc >> 8);
                                        cmd[9] = (byte)(crc);
                                        cmd[10] = (byte)(sumRows >> 8);
                                        cmd[11] = (byte)(sumRows);

                                        cmd[12] = (byte)'e';
                                        cmd[13] = (byte)'n';
                                        cmd[14] = (byte)'d';

                                        GprsClient.socketClient.Send(cmd);//DownLoadFileMode

                                    }
                                    else if ((data[7] == 'C') && (data[8] == 'M') && (data[9] == 'D') && (data[10] == '3'))
                                    {
                                        string msg = "MSG0进入远程升级模式...";
                                        msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0进入远程升级模式..." + "#END";


                                        System.DateTime dt = DateTime.Now;

                                        loginfo.LogUserControlMsg(user.IMEI, dt.ToLongDateString() + dt.ToLongTimeString() + ":IP: " + user.IP + ":进入远程升级模式:" + user.fileName);


                                        data.RemoveRange(0, i + msgLen + 11);
                                        user.socketClient.Send(System.Text.Encoding.Default.GetBytes(msg));
                                        byte[] cmd = new byte[10];
                                        cmd[0] = (byte)'u';
                                        cmd[1] = (byte)'p';
                                        cmd[2] = (byte)'d';
                                        cmd[3] = (byte)'a';
                                        cmd[4] = (byte)'t';
                                        cmd[5] = (byte)'e';
                                        cmd[6] = (byte)'m';
                                        cmd[7] = (byte)'o';
                                        cmd[8] = (byte)'d';
                                        cmd[9] = (byte)'e';
                                        GprsClient.socketClient.Send(cmd);//DownLoadFileMode
                                       // GprsClient.socketClient.Send(cmd);//DownLoadFileMode

                                    }
                                    else if ((data[7] == 'C') && (data[8] == 'M') && (data[9] == 'D') && (data[10] == '4'))
                                    {
                                        string msg = "MSG0检查升级可行性...";
                                        msg = "START" + msg.Length.ToString().PadLeft(2, '0') + "MSG0检查升级可行性..." + "#END";

                                        System.DateTime dt = DateTime.Now;
                                        loginfo.LogUserControlMsg(user.IMEI, dt.ToLongDateString() + dt.ToLongTimeString() + ":IP: " + user.IP + ":检查升级可行性");

                                        data.RemoveRange(0, i + msgLen + 11);
                                        user.socketClient.Send(System.Text.Encoding.Default.GetBytes(msg));
                                        byte[] cmd = new byte[10];
                                        cmd[0] = (byte)'c';
                                        cmd[1] = (byte)'h';
                                        cmd[2] = (byte)'e';
                                        cmd[3] = (byte)'c';
                                        cmd[4] = (byte)'k';
                                        cmd[5] = (byte)'f';
                                        cmd[6] = (byte)'m';
                                        cmd[7] = (byte)'o';
                                        cmd[8] = (byte)'d';
                                        cmd[9] = (byte)'e';
                                        GprsClient.socketClient.Send(cmd);//DownLoadFileMode

                                    }
                                    else if ((data[7] == 'C') && (data[8] == 'M') && (data[9] == 'D') && (data[10] == '2'))
                                    {
                                        user.exitCmd = true;

                                        try
                                        {
                                            System.DateTime dt = DateTime.Now;
                                            loginfo.LogUserControlMsg(user.IMEI, dt.ToLongDateString() + dt.ToLongTimeString() + ":IP: " + user.IP + "进入正常监控模式");

                                            GprsClient.socketClient.Send(System.Text.Encoding.Default.GetBytes("monitormode"));//DownLoadFileMode
                                            GprsClient.socketClient.Send(System.Text.Encoding.Default.GetBytes("monitormode"));//DownLoadFileMode
                                        }
                                        catch { }
                                        

                                        return;


                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 接收并处理客户端发送的信息
        /// </summary>
        /// <param name="obj"></param>
        private void ReceiveData(object obj) 
        {
            User user = (User)obj;
            Socket socketClient = user.socketClient;
            //是否正常退出接收线程
            bool exitWhile = false;
            int times = 0;
            while (exitWhile == false) 
            {
                List<byte> data = new List<byte>();
                int len=0;
                Thread.Sleep(100);
                user.recvTimes = user.recvTimes < 65535 ? (user.recvTimes + 1) : 65535;
                if (user.recvTimes > 300)
                {
                    user.socketClient.Disconnect(true);
                    user.socketClient.Close();
                    // socketClient.Disconnect(false);
                    userList.Remove(user);
                    service.SetListBox(string.Format("长时间没有有效数据，超时退出，剩余连接用户数：{0}", userList.Count));
                    return;
                }
                try
                {
                    byte[] buffer = new byte[10000];
                    try
                    {
                        len = user.socketClient.Receive(buffer);
                    }
                    catch { }

                        if (len > 0)
                        {
                            user.recvTimes = 0;
                            byte[] subBuffer = new byte[len];
                            Array.Copy(buffer, subBuffer, len);
                            user.GPRSMsgQueue.Enqueue(subBuffer);
                        }
                }
                catch 
                {
                    service.SetListBox("接收数据失败");

                    user.socketClient.Disconnect(true);
                    user.socketClient.Close();
                    // socketClient.Disconnect(false);
                    userList.Remove(user);
                    service.SetListBox(string.Format("长时间没有有效数据，超时退出，剩余连接用户数：{0}", userList.Count));
                    return;
                }

            }
        }
        private void ReceiveData1(object obj)
        {
            User user = (User)obj;
            Socket socketClient = user.socketClient;
           // bool exitWhile = false;
            int times = 0;
            while (user.exitWhile == false)
            {
                List<byte> data = new List<byte>();
                int len = 0;
                Thread.Sleep(100);
                user.recvTimes = user.recvTimes < 65535 ? (user.recvTimes + 1) : 65535;
                if ((user.recvTimes > 36000) || (user.exitCmd == true))
                {
                    try
                    {
                        userList1.Remove(user);
                        user.exitWhile = true;
                        user.socketClient.Disconnect(true);
                        user.socketClient.Close();
                        // socketClient.Disconnect(false);
                        
                        service1.SetListBox(string.Format("长时间没有有效数据，超时退出，剩余连接用户数：{0}", userList1.Count));
                       
                        return;
                    }
                    catch 
                    {
                    }
                    return;
                }
                try
                {

                    byte[] buffer = new byte[10000];
                    try
                    {
                        len = user.socketClient.Receive(buffer);
                    }
                    catch { }

                    if (len > 0)
                    {
                        user.recvTimes = 0;
                        byte[] subBuffer = new byte[len];
                        Array.Copy(buffer, subBuffer, len);
                        user.GPRSMsgQueue1.Enqueue(subBuffer);
                    }
                }
                catch
                {
                    service.SetListBox("接收数据失败");
                    user.socketClient.Disconnect(true);
                    user.socketClient.Close();
                    // socketClient.Disconnect(false);
                    userList1.Remove(user);
                    service1.SetListBox(string.Format("长时间没有有效数据，超时退出，剩余连接用户数：{0}", userList.Count));
                    user.exitWhile = true;
                    return;
                }

            }
        }


        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
               //未单击开始服务就直接退出时，myListener为null
            if (myListener != null) 
            {
                buttonStop_Click(null, null);
            }
        }

        private void FormServer_Load(object sender, EventArgs e)
        {
            buttonStart.PerformClick();
        }

        private void FormServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
