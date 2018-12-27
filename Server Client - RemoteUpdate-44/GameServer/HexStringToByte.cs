using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
    class HexStringToByte
    {

          public  byte[] decodeHex(string hex)
        {
            char[] chars = hex.ToCharArray();

            /*for (int i = 0; i < chars.Length; i++)
            {
                Console.WriteLine(chars[i]);
            }*/
            //Console.WriteLine();
            byte[] bytes = new byte[chars.Length / 2];
            int byteCount = 0;
            for (int i = 0; i < chars.Length; i += 2)
            {
                byte newByte = 0x00;
                newByte |= hexCharToByte(chars[i]);
                newByte <<= 4;
                newByte |= hexCharToByte(chars[i + 1]);
                bytes[byteCount] = newByte;
                byteCount++;
            }

            return bytes;
        }


          public byte[] decodeHex(string hex,ref UInt16 CRC)
          {
              char[] chars = hex.ToCharArray();

              /*for (int i = 0; i < chars.Length; i++)
              {
                  Console.WriteLine(chars[i]);
              }*/
              //Console.WriteLine();
              byte[] bytes = new byte[chars.Length / 2];
              int byteCount = 0;
              for (int i = 0; i < chars.Length; i += 2)
              {
                  byte newByte = 0x00;
                  newByte |= hexCharToByte(chars[i]);
                  newByte <<= 4;
                  newByte |= hexCharToByte(chars[i + 1]);
                  bytes[byteCount] = newByte;

                  CRC += newByte;
                  byteCount++;
              }

              return bytes;
          }

          /// 字节数组转16进制字符串
          /// </summary>
          /// <param name="bytes"></param>
          /// <returns></returns>
          public  string byteToHexStr(byte[] bytes)
          {
              string returnStr = "";
              if (bytes != null)
              {
                  for (int i = 0; i < bytes.Length; i++)
                  {
                      returnStr += bytes[i].ToString("X2");
                  }
              }
              return returnStr;
          }


        private static byte hexCharToByte(char ch)
        {
            switch (ch)
            {
                case '0': return 0x00;
                case '1': return 0x01;
                case '2': return 0x02;
                case '3': return 0x03;
                case '4': return 0x04;
                case '5': return 0x05;
                case '6': return 0x06;
                case '7': return 0x07;
                case '8': return 0x08;
                case '9': return 0x09;
                case 'a': return 0x0A;
                case 'b': return 0x0B;
                case 'c': return 0x0C;
                case 'd': return 0x0D;
                case 'e': return 0x0E;
                case 'f': return 0x0F;
                case 'A': return 0x0A;
                case 'B': return 0x0B;
                case 'C': return 0x0C;
                case 'D': return 0x0D;
                case 'E': return 0x0E;
                case 'F': return 0x0F;
            }
            return 0x00;
        }
    } 
    }

