using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Net;

namespace GameServer
{
    /// <summary>
    /// 用于保存每个玩家的基本信息
    /// </summary>
    class User
    {
        public Socket socketClient;
        public Socket userSocketClient;
        public TcpClient client;
        public StreamWriter sw;
        public StreamReader sr;

        public SQLDataBase sqlDb;

        public Queue<byte[]> GPRSMsgQueue;
        public Queue<byte[]> GPRSMsgQueue1;

        public string userName;
        public string IMEI;
        public string fileName;

        public string[] rowInfo = new string[10000];
        public List<byte[]> rowList = new List<byte[]>();
        public int sumRows = 0;

        public UInt16 CRC = 0;

        public bool exitWhile = false;
        public bool exitCmd = false;
        public int recvTimes = 0;
        public int translateTimes = 0;
        public string IP = "";

        public User(TcpClient client) 
        {
            this.client = client;
         
            NetworkStream netStream = client.GetStream();
            sw = new StreamWriter(netStream, Encoding.Default);
            //sr = new StreamReader(netStream, Encoding.UTF8);
            sr = new StreamReader(netStream, Encoding.Default);
            this.userName = "";
        }

        public User(Socket client)
        {
           
            this.socketClient = client;
            this.socketClient.ReceiveBufferSize = 1024;
            this.socketClient.ReceiveTimeout = 100;

            GPRSMsgQueue = new Queue<byte[]>();
            GPRSMsgQueue1 = new Queue<byte[]>();
            
            sqlDb = new SQLDataBase();
            //NetworkStream netStream = client.GetStream();
            //sw = new StreamWriter(netStream, Encoding.Default);
            //sr = new StreamReader(netStream, Encoding.UTF8);
            //sr = new StreamReader(netStream, Encoding.Default);
            this.userName = "";

            IP = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();
        }
    }
}
