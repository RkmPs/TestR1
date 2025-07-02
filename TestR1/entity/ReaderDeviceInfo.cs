using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UHFAPP.Entity
{
    public class ReaderDeviceInfo
    {
        public string mac;
        public string ip;
        public int port;
        public byte[] macBytes;
        public byte[] ipBytes;
        public int lastTime = 0;
        public ReaderDeviceInfo(byte[] macBytes, byte[] ipBytes, int port)
        {
            this.macBytes = macBytes;
            this.ipBytes = ipBytes;
            this.port = port;
            this.lastTime = Environment.TickCount;

            StringBuilder macSB = new StringBuilder();
            macSB.Append(string.Format("{0:X2}", macBytes[0]));// Convert.ToString(macBytes[0], 16)
            macSB.Append(":");
            macSB.Append(string.Format("{0:X2}", macBytes[1]));
            macSB.Append(":");
            macSB.Append(string.Format("{0:X2}", macBytes[2]));
            macSB.Append(":");
            macSB.Append(string.Format("{0:X2}", macBytes[3]));
            macSB.Append(":");
            macSB.Append(string.Format("{0:X2}", macBytes[4]));
            macSB.Append(":");
            macSB.Append(string.Format("{0:X2}", macBytes[5]));
            mac = macSB.ToString();

            StringBuilder macIP = new StringBuilder();
            macIP.Append(ipBytes[0]);
            macIP.Append(":");
            macIP.Append(ipBytes[1]);
            macIP.Append(":");
            macIP.Append(ipBytes[2]);
            macIP.Append(":");
            macIP.Append(ipBytes[3]);
            ip = macIP.ToString();

        }

        public byte[] GetIpAndMac()
        {
            if (String.IsNullOrEmpty(mac) || String.IsNullOrEmpty(ip))
            {
                return null;
            }

            byte[] data=new byte[10];
            for (int k = 0; k < 4; k++)
            {
                data[k] = ipBytes[k];
            }
            for (int k = 0; k < 6; k++)
            {
                data[4 + k] = macBytes[k];
            }

            return data;
        }
    }
}
