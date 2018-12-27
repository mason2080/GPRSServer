using System.Text;
using System.Net.Sockets;
using System.IO;

namespace GameServer
{
    /// <summary>
    /// 用于保存每个玩家的基本信息
    /// </summary>
    class User
    {
        public TcpClient client;
        public StreamWriter sw;
        public StreamReader sr;

        public string userName;
        public User(TcpClient client) 
        {
            this.client = client;
            NetworkStream netStream = client.GetStream();
            sw = new StreamWriter(netStream, Encoding.UTF8);
            sr = new StreamReader(netStream, Encoding.UTF8);
            this.userName = "";
        }
    }
}
