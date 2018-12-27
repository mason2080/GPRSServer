using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GameServer
{
    class Log
    {
        public void LogInfo(string info)
        {

        }


        public void LogUserControlMsg(string imei, string info)
        {
            string filename = @"d:\" + imei + ".txt";

            FileInfo file = new FileInfo(filename);
            if (file.Exists == false)
            {
                file.Create();
            }

            StreamWriter sw1 = File.AppendText(filename);// new StreamWriter(@"c:\log.txt");
            sw1.WriteLine(info);
           //sw1.Flush();
            sw1.Close();

        }
        
    }
}
