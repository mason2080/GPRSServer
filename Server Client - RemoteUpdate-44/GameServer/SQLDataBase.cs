using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data;
using System.Threading;

namespace GameServer
{

    class SQLDataBase
    {
        private static object privateObjectLock = new object();
        private static object privateObjectLock1 = new object();  
        public struct GPRS_DATA
        {
            public string bmsTime;
            public string gprsTime;
            public string moduleTime;
            public string serverTime;
            public string CarNumber;
            public string longitude;
            public string latitude;
            public float SOC;
            public float SOH;
            public float intSumv;
            public float Curr;
            public float extSumv;
            public float avgCurr;
            public float maxVolt;
            public float maxVoltNo;
            public float minVolt;
            public float minVoltNo;
            public float maxTemper;
            public float maxTemperNo;
            public float minTemper;
            public float minTemperNo;
            public float VoltNum;
            public float TempNum;
            public float maxChgCurr;
            public float maxDisChgCurr;
            public float realAh;
            public float inAh;
            public float outAh;
            public string posRelay;
            public string negRelay;
            public string chgRelay;
            public string preRelay;
            public string fanRelay;
            public string heatRelay;
            public string do1;
            public string do2;
            public float cp;
            public string di1;
            public string di2;
            public string di3;
            public string di4;
            public float cc;
            public float ai1;
            public float ai2;
            public float cc2;
            public int[] bat_err;
            public int[] batsys_err;
            public int[] other_err;
            public int[] hd_err;
            public float bmsPowerVolt;
            public float maxChgSumVolt;
            public float maxChgCellVolt;
            public float maxChgCurrent;
            public float minChgCurrent;
            public float chgStep;
            public float maxChgTemper;
            public float chargerOutVolt;
            public float chargerOutCurr;
            public float chargerOutPower;
            public float[] cellVolt;
            public float[] cellTemp;
            public byte gprsLife;
            public string VIN;
            public string IMEI;
            public string SourthNorth;
            public string EastWest;
            public int bmuNumber;
            public int voltNumber;
            public int tempNumber;
        }
        //SqlConnection sqlConn = new SqlConnection("Server=T450-PC\\SQLEXPRESS;Database=LWXY;integrated security = true;");
        SqlConnection sqlConn = new SqlConnection(@"Server=WINDOWS-5BPUEV5\SQLEXPRESS;Database=GPRS;integrated security = true;");
        public GPRS_DATA gprsData;
        public SQLDataBase()
        {

            gprsData.cellTemp = new float[100];
            gprsData.cellVolt = new float[300];
            gprsData.bat_err = new int[8];
            gprsData.batsys_err = new int[8];
            gprsData.other_err = new int[8];
            gprsData.hd_err = new int[8];

        }

        public int TranslateBMSMsg(List<byte> data,ref string gprsInfo,ref string IMEI)
        {
            int i = 0;
            byte []tempArray=new byte[10];

            if ((data[8] == 0x01) || (data[8] == 0x02) || (data[8] == 0x03) || (data[8] == 0x04))//////1:BHN01 2:BHN02 3:BLN01 4:BHN02一体机
            {
                if (data[7] == 0x01)////Frame Number  one frame max length:500 Bytes
                {
                    #region
                    lock (privateObjectLock1)
                    {
                        gprsData.gprsLife = data[9];
                        //gprsData.CarNumber = GetProvince(data.GetRange(10, 2)) + ListToString(data.GetRange(12, 1)) +"." + ListToString(data.GetRange(13, 5));
                        gprsData.CarNumber ="CARNO"+ GetProvince(data.GetRange(10, 2)) + ListToString(data.GetRange(12, 6));
                        gprsData.VIN = "VIN"+ListToString(data.GetRange(18, 13));
                        gprsData.IMEI = "IMEI" + ListToString(data.GetRange(31, 15));
                        IMEI = gprsData.IMEI;


                      
                        gprsData.inAh = (data[48] * 0x1000000 + data[49] * 0x10000 + data[50] * 0x100 + data[51]) / 1000f;


                        gprsData.bmsTime = "20" + data[52].ToString() + "-" + data[53].ToString() + "-" + data[54].ToString() + " " + data[55].ToString() + "-" + data[56].ToString() + "-" + data[57].ToString(); ;//  ListToString(data.GetRange(46+6, 6));

                        gprsData.outAh = (data[60] * 0x1000000 + data[61] * 0x10000 + data[62] * 0x100 + data[63]) / 1000f;
                        //Thread.Sleep(10);
                        gprsData.moduleTime = "20" + data[64].ToString() + "-" + data[65].ToString() + "-" + data[66].ToString() + " " + data[67].ToString() + "-" + data[68].ToString() + "-" + data[69].ToString(); ; //ListToString(data.GetRange(58 + 6, 6));
                        gprsData.gprsTime = ListToString(data.GetRange(70, 12));
                        gprsData.serverTime = System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToLongTimeString();// .UtcNow.ToShortDateString() + System.DateTime.UtcNow.ToLongTimeString();

                        gprsInfo = gprsData.serverTime + " " + gprsData.IMEI + gprsData.VIN + gprsData.CarNumber;

                        gprsData.latitude = ListToString(data.GetRange(82, 10));
                        gprsData.longitude = ListToString(data.GetRange(92, 11));
                        tempArray[0] = data[103];
                        gprsData.SourthNorth = System.Text.Encoding.Default.GetString(tempArray, 0, 1);
                        tempArray[0] = data[104];
                        gprsData.EastWest = System.Text.Encoding.Default.GetString(tempArray, 0, 1);

                        gprsData.intSumv = (data[105] * 256 + data[106]) / 10f;
                        gprsData.avgCurr = (data[107] * 256 + data[108] - 30000) / 10f;
                        gprsData.SOC = (data[109] * 256 + data[110]) / 10f;
                        gprsData.SOH = (data[111] * 256 + data[112]) / 10f;

                        gprsData.maxVolt = (data[113] * 256 + data[114]) / 1000f;
                        gprsData.maxVoltNo = (data[115] * 256 + data[116]);

                        gprsData.minVolt = (data[117] * 256 + data[118]) / 1000f;
                        gprsData.minVoltNo = (data[119] * 256 + data[120]);

                        gprsData.maxTemper = data[121] - 40;
                        gprsData.maxTemperNo = (data[122] * 256 + data[123]);

                        gprsData.minTemper = data[124] - 40;
                        gprsData.minTemperNo = (data[125] * 256 + data[126]);
                        //Thread.Sleep(10);
                        gprsData.posRelay = ((byte)((data[127] & 0xc0) >> 6) == 1) ? "ON" : "OFF";
                        gprsData.negRelay = ((byte)((data[127] & 0x30) >> 4) == 1) ? "ON" : "OFF";
                        gprsData.preRelay = ((byte)((data[127] & 0x0c) >> 2) == 1) ? "ON" : "OFF";
                        gprsData.chgRelay = ((byte)(data[127] & 0x03) == 1) ? "ON" : "OFF";

                        gprsData.fanRelay = ((byte)((data[128] & 0xc0) >> 6) == 1) ? "ON" : "OFF";
                        gprsData.heatRelay = ((byte)((data[128] & 0x30) >> 4) == 1) ? "ON" : "OFF";
                        gprsData.do1 = ((byte)((data[128] & 0x0c) >> 2) == 1) ? "ON" : "OFF";
                        gprsData.do2 = ((byte)(data[128] & 0x03) == 1) ? "ON" : "OFF";

                        gprsData.cp = data[129] / 10f;

                        gprsData.di1 = ((byte)((data[130] & 0xc0) >> 6) == 0) ? "ON" : "OFF";
                        gprsData.di2 = ((byte)((data[130] & 0x30) >> 4) == 0) ? "ON" : "OFF";
                        gprsData.di3 = ((byte)((data[130] & 0x0c) >> 2) == 0) ? "ON" : "OFF";
                        gprsData.di4 = ((byte)(data[130] & 0x03) == 0) ? "ON" : "OFF";

                        gprsData.cc = data[131] * 0.2f;
                        gprsData.ai1 = data[132] * 0.2f;
                        gprsData.ai2 = data[133] * 0.2f;
                        gprsData.cc2 = data[134] * 0.2f;
                        //Thread.Sleep(10);
                        for (i = 0; i < 8; i++)
                        {
                            gprsData.bat_err[i] = data[135 + i];
                        }


                        for (i = 0; i < 8; i++)
                        {
                            gprsData.batsys_err[i] = data[143 + i];
                        }

                        Thread.Sleep(10);
                        for (i = 0; i < 8; i++)
                        {
                            gprsData.other_err[i] = data[151 + i];
                        }


                        for (i = 0; i < 8; i++)
                        {
                            gprsData.hd_err[i] = data[159 + i];
                        }

                        gprsData.bmuNumber = data[167];
                        //Thread.Sleep(10);
                        gprsData.voltNumber = (data[168] * 256 + data[169]);
                        if (gprsData.voltNumber > 250)
                        {
                            gprsData.voltNumber = 250;
                        }
                        if (gprsData.voltNumber == 0)
                        {
                            gprsData.voltNumber = 1;//方便后面插入数据库处理
                            gprsData.cellVolt[0] = 65535;
                        }

                        gprsData.tempNumber = (data[170] * 256 + data[171]);

                        if (gprsData.tempNumber > 100)
                        {
                            gprsData.tempNumber = 100;
                        }

                        if (gprsData.tempNumber == 0)
                        {
                            gprsData.tempNumber = 1;
                            gprsData.cellTemp[0] = 0xff;
                        }



                        if (gprsData.voltNumber != 1)
                        {
                            for (i = 0; i < gprsData.voltNumber; i++)
                            {
                                gprsData.cellVolt[i] = (data[172 + i * 2] * 256 + data[172 + i * 2 + 1]) / 1000f;

                                if (i % 20 == 0)
                                {
                                    //Thread.Sleep(5);
                                }
                            }
                        }


                        if (gprsData.tempNumber != 1)
                        {
                            for (i = 0; i < gprsData.tempNumber; i++)
                            {
                                gprsData.cellTemp[i] = (data[172 + gprsData.voltNumber * 2 + i] - 40);

                                if (i % 20 == 0)
                                {
                                    //Thread.Sleep(5);
                                }
                            }
                        }
                        InsertGprsData(gprsData.IMEI);
                    }
                    //InsertGprsData(gprsData.IMEI);
                    #endregion
                }
                return 1;   
            }
           // }

            return 0;
        }

        public string ListToString(List<byte> input)
        {
            string output="";
            for (int i = 0; i < input.Count; i++)
            {
                output +=Convert.ToChar( input[i]);
            }
            return output;
        }

        public string GetProvince(List<byte> input)
        {
            string output = "";
            byte[] s = new byte[2];

            s[0] = input[0];
            s[1] = input[1];

            output = System.Text.Encoding.Default.GetString(s);

            return output;
        }

        public int InsertGprsData(string TableName)
        {
            string VoltColumn = "";
            string TempColumn = "";
            string VoltString = "";
            string TempString = "";

            string VoltColumn1 = "";
            string TempColumn1 = "";
            string VoltString1 = "";
            string TempString1 = "";

            string MyInsert = "";

            if (TableExist(TableName) == false)
            {
                CreateNewTable(TableName);

                InsertNewDeviceToLatestDataTable(TableName);
            }

            MyInsert = @"INSERT INTO dbo."+TableName+
                                    @"( 
                                     Life
                                    ,BMS时间
                                    ,GPS_UTC时间
                                    ,模块时间
                                    ,服务器时间
                                    ,车牌号
                                    ,VIN码
                                    ,经度
                                    ,纬度
                                    ,南北
                                    ,东西
                                    ,SOC
                                    ,SOH
                                    ,总电压
                                    ,外总压
                                    ,平均电流
                                    ,最高电压
                                    ,最高电压位置
                                    ,最小电压
                                    ,最小电压位置
                                    ,最高温度
                                    ,最高温度位置
                                    ,最低温度
                                    ,最低温度位置
                                    ,充电安时
                                    ,放电安时
                                    ,单体电压个数
                                    ,单体温度个数
                                    ,总正继电器
                                    ,总负继电器
                                    ,预充继电器
                                    ,充电继电器
                                    ,风机继电器
                                    ,加热继电器
                                    ,DO1
                                    ,DO2
                                    ,CP
                                    ,DI1
                                    ,DI2
                                    ,DI3
                                    ,DI4
                                    ,CC
                                    ,CC2
                                    ,AI1
                                    ,AI2
                                    ,Bat_ErrCode0
                                    ,Bat_ErrCode1
                                    ,Bat_ErrCode2
                                    ,Bat_ErrCode3
                                    ,Bat_ErrCode4
                                    ,Bat_ErrCode5
                                    ,Bat_ErrCode6
                                    ,Bat_ErrCode7
                                    ,BatSYS_ErrCode0
                                    ,BatSYS_ErrCode1
                                    ,BatSYS_ErrCode2
                                    ,BatSYS_ErrCode3
                                    ,BatSYS_ErrCode4
                                    ,BatSYS_ErrCode5
                                    ,BatSYS_ErrCode6
                                    ,BatSYS_ErrCode7
                                    ,Other_ErrCode0
                                    ,Other_ErrCode1
                                    ,Other_ErrCode2
                                    ,Other_ErrCode3
                                    ,Other_ErrCode4
                                    ,Other_ErrCode5
                                    ,Other_ErrCode6
                                    ,Other_ErrCode7
                                    ,HD_ErrCode0
                                    ,HD_ErrCode1
                                    ,HD_ErrCode2
                                    ,HD_ErrCode3
                                    ,HD_ErrCode4
                                    ,HD_ErrCode5
                                    ,HD_ErrCode6
                                    ,HD_ErrCode7
                                    ,";

            string LatestInsert = @"Update dbo.LatestData " +
                                  "SET Life='" + gprsData.gprsLife.ToString() + "',BMS时间='"
                                  + gprsData.bmsTime.Replace(",", "") + "',GPS_UTC时间='" 
                                 + gprsData.gprsTime.Replace(",", "") + "',模块时间='"
                                  + gprsData.moduleTime.Replace(",", "") + "',服务器时间='"
                                  + gprsData.serverTime.Replace(",", "") + "',车牌号='"
                                  + gprsData.CarNumber.Replace(",", "") + "',VIN码='"
                                  + gprsData.VIN.Replace(",", "") + "',经度='"
                                  + gprsData.longitude.ToString().Replace(",","") + "',纬度='"
                                  + gprsData.latitude.ToString().Replace(",","") + "',南北='"
                                  + gprsData.SourthNorth.Replace(",", "") + "',东西='"
                                  + gprsData.EastWest.Replace(",", "") + "',SOC='"
                                  + gprsData.SOC.ToString() + "',SOH='"
                                  + gprsData.SOH.ToString() + "',总电压='"
                                  + gprsData.intSumv.ToString() + "',外总压='"
                                  + gprsData.extSumv.ToString() + "',平均电流='"

                                  + gprsData.avgCurr.ToString() + "',最高电压='"
                                  + gprsData.maxVolt.ToString() + "',最高电压位置='"
                                  + gprsData.maxVoltNo.ToString() + "',最小电压='"
                                  + gprsData.minVolt.ToString() + "',最小电压位置='"
                                  + gprsData.minVoltNo.ToString() + "',最高温度='"
                                  + gprsData.maxTemper.ToString() + "',最高温度位置='"
                                  + gprsData.maxTemperNo.ToString() + "',最低温度='"
                                  + gprsData.minTemper.ToString() + "',最低温度位置='"
                                  + gprsData.minTemperNo.ToString() + "',充电安时='"
                                  + gprsData.inAh.ToString() + "',放电安时='"
                                  + gprsData.outAh.ToString() + "',单体电压个数='"

                                  + gprsData.voltNumber.ToString() + "',单体温度个数='"
                                  + gprsData.tempNumber.ToString() + "',总正继电器='"
                                  + gprsData.posRelay + "',总负继电器='"
                                  + gprsData.negRelay + "',预充继电器='"
                                  + gprsData.preRelay + "',充电继电器='"

                                  + gprsData.chgRelay+ "',风机继电器='"
                                  + gprsData.fanRelay + "',加热继电器='"
                                  + gprsData.heatRelay+ "',DO1='"
                                  + gprsData.do1 + "',DO2='"
                                  + gprsData.do2 + "',CP='"
                                  + gprsData.cp.ToString() + "',DI1='"
                                  + gprsData.di1 + "',DI2='"
                                  + gprsData.di2 + "',DI3='"
                                  + gprsData.di3 + "',DI4='"
                                  + gprsData.di4 + "',CC='"
                                  + gprsData.cc.ToString() + "',CC2='"
                                  + gprsData.cc2.ToString() + "',AI1='"
                                  + gprsData.ai1.ToString() + "',AI2='"

                                  + gprsData.ai2.ToString() + "',Bat_ErrCode0='"
                                  + gprsData.bat_err[0].ToString() + "',Bat_ErrCode1='"
                                  + gprsData.bat_err[1].ToString() + "',Bat_ErrCode2='"
                                  + gprsData.bat_err[2].ToString() + "',Bat_ErrCode3='"
                                  + gprsData.bat_err[3].ToString() + "',Bat_ErrCode4='"
                                  + gprsData.bat_err[4].ToString() + "',Bat_ErrCode5='"
                                  + gprsData.bat_err[5].ToString() + "',Bat_ErrCode6='"
                                  + gprsData.bat_err[6].ToString() + "',Bat_ErrCode7='"

                                  + gprsData.bat_err[7].ToString() + "',BatSYS_ErrCode0='"
                                  + gprsData.batsys_err[0].ToString() + "',BatSYS_ErrCode1='"
                                  + gprsData.batsys_err[1].ToString() + "',BatSYS_ErrCode2='"
                                  + gprsData.batsys_err[2].ToString() + "',BatSYS_ErrCode3='"
                                  + gprsData.batsys_err[3].ToString() + "',BatSYS_ErrCode4='"
                                  + gprsData.batsys_err[4].ToString() + "',BatSYS_ErrCode5='"
                                  + gprsData.batsys_err[5].ToString() + "',BatSYS_ErrCode6='"
                                  + gprsData.batsys_err[6].ToString() + "',BatSYS_ErrCode7='"

                                  + gprsData.batsys_err[7].ToString() + "',Other_ErrCode0='"
                                  + gprsData.other_err[0].ToString() + "',Other_ErrCode1='"
                                  + gprsData.other_err[1].ToString() + "',Other_ErrCode2='"
                                  + gprsData.other_err[2].ToString() + "',Other_ErrCode3='"
                                  + gprsData.other_err[3].ToString() + "',Other_ErrCode4='"
                                  + gprsData.other_err[4].ToString() + "',Other_ErrCode5='"
                                  + gprsData.other_err[5].ToString() + "',Other_ErrCode6='"
                                  + gprsData.other_err[6].ToString() + "',Other_ErrCode7='"

                                  + gprsData.other_err[7].ToString() + "',HD_ErrCode0='"
                                  + gprsData.hd_err[0].ToString() + "',HD_ErrCode1='"
                                  + gprsData.hd_err[1].ToString() + "',HD_ErrCode2='"
                                  + gprsData.hd_err[2].ToString() + "',HD_ErrCode3='"
                                  + gprsData.hd_err[3].ToString() + "',HD_ErrCode4='"
                                  + gprsData.hd_err[4].ToString() + "',HD_ErrCode5='"
                                  + gprsData.hd_err[5].ToString() + "',HD_ErrCode6='"
                                  + gprsData.hd_err[6].ToString() + "',HD_ErrCode7='"
                                  + gprsData.hd_err[7].ToString() + "',";

            for (int i = 0; i < gprsData.voltNumber; i++)
            {
                VoltColumn1 += "电压" + (i + 1).ToString() + "='" + gprsData.cellVolt[i].ToString() + "',";

                //if (i % 20 == 0)
                //{
                //    Thread.Sleep(5);
                //}
            }


            for (int i = 0; i < gprsData.tempNumber; i++)
            {
                if (i == gprsData.tempNumber - 1)
                {
                    TempColumn1 += "温度" + (i + 1).ToString() + "='" + gprsData.cellTemp[i].ToString()+"'";
                }
                else
                {
                    TempColumn1 += "温度" + (i + 1).ToString() + "='" + gprsData.cellTemp[i].ToString()+"',";
                }

                //if (i % 20 == 0)
                //{
                //    Thread.Sleep(5);
                //}
            }



            for (int i = 0; i < gprsData.voltNumber; i++)
            {
                    VoltColumn += "电压" + (i + 1).ToString() + ",";

                    //if (i % 20 == 0)
                    //{
                    //    Thread.Sleep(5);
                    //}
            }

            for (int i = 0; i < gprsData.tempNumber; i++)
            {
                if (i == gprsData.tempNumber-1)
                {
                    TempColumn += "温度" + (i + 1).ToString() + " )VALUES(";
                }
                else
                {
                    TempColumn += "温度" + (i + 1).ToString() + ",";
                }

                //if (i % 20 == 0)
                //{
                //    Thread.Sleep(5);
                //}
            }
            string MyInsert1 =    "'" + gprsData.gprsLife.ToString() + "',"
                                + "'" + gprsData.bmsTime.Replace(",", "") + "',"
                                + "'" + gprsData.gprsTime.Replace(",", "") + "',"
                                + "'" + gprsData.moduleTime.Replace(",", "") + "',"
                                + "'" + gprsData.serverTime.Replace(",", "") + "',"
                                + "'" + gprsData.CarNumber + "',"
                                + "'" + gprsData.VIN + "',"
                                + "'" + gprsData.longitude.ToString().Replace(",","") + "',"
                                + "'" + gprsData.latitude.ToString().Replace(",", "") + "',"
                                + "'" + gprsData.SourthNorth.Replace(",", "") + "',"
                                + "'" + gprsData.EastWest.Replace(",", "") + "',"
                                + "'" + gprsData.SOC.ToString() + "',"
                                + "'" + gprsData.SOH.ToString() + "',"
                                + "'" + gprsData.intSumv.ToString() + "',"
                                + "'" + gprsData.extSumv.ToString() + "',"
                                + "'" + gprsData.avgCurr.ToString() + "',"
                                + "'" + gprsData.maxVolt.ToString() + "',"
                                + "'" + gprsData.maxVoltNo.ToString() + "',"
                                + "'" + gprsData.minVolt.ToString() + "',"
                                + "'" + gprsData.minVoltNo.ToString() + "',"
                                + "'" + gprsData.maxTemper.ToString() + "',"
                                + "'" + gprsData.maxTemperNo.ToString() + "',"
                                + "'" + gprsData.minTemper.ToString() + "',"
                                + "'" + gprsData.minTemperNo.ToString() + "',"
                                + "'" + gprsData.inAh.ToString() + "',"
                                + "'" + gprsData.outAh.ToString() + "',"
                                + "'" + gprsData.voltNumber.ToString() + "',"
                                + "'" + gprsData.tempNumber.ToString() + "',"
                                + "'" + gprsData.posRelay + "',"
                                + "'" + gprsData.negRelay + "',"
                                + "'" + gprsData.preRelay + "',"
                                + "'" + gprsData.chgRelay + "',"
                                + "'" + gprsData.fanRelay + "',"
                                + "'" + gprsData.heatRelay + "',"
                                + "'" + gprsData.do1 + "',"
                                + "'" +  gprsData.do2  + "',"
                                + "'" + gprsData.cp.ToString() + "',"
                                + "'" + gprsData.di1 + "',"
                                + "'" + gprsData.di2 + "',"
                                + "'" + gprsData.di3 + "',"
                                + "'" + gprsData.di4 + "',"
                                + "'" + gprsData.cc.ToString() + "',"
                                + "'" + gprsData.cc2.ToString() + "',"
                                + "'" + gprsData.ai1.ToString() + "',"
                                + "'" + gprsData.ai2.ToString() + "',"
                                + "'" + gprsData.bat_err[0].ToString() + "',"
                                + "'" + gprsData.bat_err[1].ToString() + "',"
                                + "'" + gprsData.bat_err[2].ToString() + "',"
                                + "'" + gprsData.bat_err[3].ToString() + "',"
                                + "'" + gprsData.bat_err[4].ToString() + "',"
                                + "'" + gprsData.bat_err[5].ToString() + "',"
                                + "'" + gprsData.bat_err[6].ToString() + "',"
                                + "'" + gprsData.bat_err[7].ToString() + "',"
                                + "'" + gprsData.batsys_err[0].ToString() + "',"
                                + "'" + gprsData.batsys_err[1].ToString() + "',"
                                + "'" + gprsData.batsys_err[2].ToString() + "',"
                                + "'" + gprsData.batsys_err[3].ToString() + "',"
                                + "'" + gprsData.batsys_err[4].ToString() + "',"
                                + "'" + gprsData.batsys_err[5].ToString() + "',"
                                + "'" + gprsData.batsys_err[6].ToString() + "',"
                                + "'" + gprsData.batsys_err[7].ToString() + "',"
                                + "'" + gprsData.other_err[0].ToString() + "',"
                                + "'" + gprsData.other_err[1].ToString() + "',"
                                + "'" + gprsData.other_err[2].ToString() + "',"
                                + "'" + gprsData.other_err[3].ToString() + "',"
                                + "'" + gprsData.other_err[4].ToString() + "',"
                                + "'" + gprsData.other_err[5].ToString() + "',"
                                + "'" + gprsData.other_err[6].ToString() + "',"
                                + "'" + gprsData.other_err[7].ToString() + "',"
                                + "'" + gprsData.hd_err[0].ToString() + "',"
                                + "'" + gprsData.hd_err[1].ToString() + "',"
                                + "'" + gprsData.hd_err[2].ToString() + "',"
                                + "'" + gprsData.hd_err[3].ToString() + "',"
                                + "'" + gprsData.hd_err[4].ToString() + "',"
                                + "'" + gprsData.hd_err[5].ToString() + "',"
                                + "'" + gprsData.hd_err[6].ToString() + "',"
                                + "'" + gprsData.hd_err[7].ToString() + "','";

                                for (int j= 0; j < gprsData.voltNumber; j++)
                                {
                                    VoltString += gprsData.cellVolt[j].ToString() + "','";

                                    //if (j % 20 == 0)
                                    //{
                                    //    Thread.Sleep(5);
                                    //}
                                }


                                    for (int j = 0; j < gprsData.tempNumber; j++)
                                    {
                                        if (j == gprsData.tempNumber-1)
                                        {
                                            TempString += gprsData.cellTemp[j].ToString() + "')";
                                        }
                                        else
                                        {
                                            TempString += gprsData.cellTemp[j].ToString() + "','";
                                        }

                                        //if (j % 20 == 0)
                                        //{
                                        //    Thread.Sleep(5);
                                        //}
                                    }

                                    MyInsert = MyInsert + VoltColumn + TempColumn + MyInsert1 + VoltString + TempString;// +" select @@identity";

                                string insertLatest = LatestInsert + VoltColumn1 + TempColumn1 + " where IMEI like " + "'" + TableName+"%'";

                                //lock (privateObjectLock)
                                //{
                                    SqlCommand MyCommand = new SqlCommand(MyInsert, sqlConn);
                                    try//异常处理
                                    {
                                        sqlConn.Open();
                                        MyCommand.ExecuteNonQuery();

                                        MyCommand = new SqlCommand(insertLatest, sqlConn);
                                        MyCommand.ExecuteNonQuery();

                                        sqlConn.Close();
                                        return 1;
                                    }
                                    catch (Exception ex)
                                    {
                                        sqlConn.Close();
                                        MessageBox.Show("Insert New Data Error");
                                        return 0;

                                    }
                                //}
        }

        public void CreateNewTable(string TableName)
        {
                string MyInsert = @"SELECT * INTO dbo." + TableName + @" FROM dbo.总信息_模板 WHERE 1=2";
                SqlCommand MyCommand = new SqlCommand(MyInsert, sqlConn);
                try//异常处理
                {
                     sqlConn.Open();
                     MyCommand.ExecuteNonQuery();

                     //MyInsert = @"SELECT * INTO dbo." +"MAXID"+ TableName + @" FROM dbo.MAXID_模板";
                     //MyCommand = new SqlCommand(MyInsert, sqlConn);
                     //MyCommand.ExecuteNonQuery();


                    sqlConn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("CreateNewTable Error");
                    // Console.WriteLine("{0} Exception caught.", ex);
                }
        }

        public void InsertNewDeviceToLatestDataTable(string TableName)
        {
            string MyInsert = @"SELECT * From dbo.LatestData WHERE IMEI like"+"'"+TableName+ "%'";
            SqlCommand MyCommand = new SqlCommand(MyInsert, sqlConn);
            try//异常处理
            {
                sqlConn.Open();
                //MyCommand.ExecuteNonQuery();
                SqlDataAdapter adapter = new SqlDataAdapter(MyCommand);
                DataTable data = new DataTable();
                try
                {
                    adapter.Fill(data);
                }
                catch 
                {
                    sqlConn.Close();
                }

                if (data.Rows.Count == 0)
                {
                    MyInsert = @"INSERT INTO dbo.LatestData (IMEI) Values (" + "'"+ TableName+"')";
                    MyCommand = new SqlCommand(MyInsert, sqlConn);
                    MyCommand.ExecuteNonQuery();
                }

                sqlConn.Close();
            }
            catch (Exception ex)
            {
                sqlConn.Close();
                MessageBox.Show("InsertNewDeviceToLatestDataTable Error");
                // Console.WriteLine("{0} Exception caught.", ex);
            }
        }

        public bool TableExist(string TableName)
        {
            string MyInsert = @"select count(1) from sysobjects where name =" + "'"+TableName+"'";
            SqlCommand MyCommand = new SqlCommand(MyInsert, sqlConn);
            try//异常处理
            {
                sqlConn.Open();
                int result = Convert.ToInt32(MyCommand.ExecuteScalar());
                sqlConn.Close();
                if (result == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Check Table Exist Status Error");
                // Console.WriteLine("{0} Exception caught.", ex);
                return false;
            }
        }
    }
}
