using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UHFAPP
{
    public class IPConfig
    {
        static string path = System.Environment.CurrentDirectory + "\\ipConfig.txt";
        static string path2 = System.Environment.CurrentDirectory + "\\ipConfig2.txt";
        public static void setIPConfig(IPEntity ipEntity)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("ip=");
                sb.Append(ipEntity.Ip[0]);
                sb.Append(".");
                sb.Append(ipEntity.Ip[1]);
                sb.Append(".");
                sb.Append(ipEntity.Ip[2]);
                sb.Append(".");
                sb.Append(ipEntity.Ip[3]);
                sb.Append("\r\n");
                sb.Append("port=");
                sb.Append(ipEntity.Port);
                FileManage.WriterFile(path, sb.ToString(), false);
            }
            catch (Exception ex)
            {

            }
        }
        public static void setIPConfig(string ip, string port, string ip2, string port2)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ip);
                sb.Append(",");
                sb.Append(port);
                sb.Append("\r\n");
                sb.Append(ip2);
                sb.Append(",");
                sb.Append(port2);
                FileManage.WriterFile(path2, sb.ToString(), false);
            }
            catch (Exception ex)
            {

            }
        }
        public static IPEntity getIPConfig()
        {
            try
            {
                string data = FileManage.ReadFile(path);
                if (data == "") return null;

                string[] ip = data.Split('\n')[0].Replace("ip=", "").Replace(" ", "").Replace("\r", "").Split('.');
                if (ip.Length != 4) return null;

                int port = int.Parse(data.Split('\n')[1].Replace("port=", "").Replace("\r", "").Replace(" ", ""));
                IPEntity ipEntity = new IPEntity();
                ipEntity.Ip = ip;
                ipEntity.Port = port;
                return ipEntity;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        public static List<IPEntity> getIPConfig2()
        {
            try
            {
                string data = FileManage.ReadFile(path2);
                if (data == "") return null;
                data = data.Replace("\r", "");
                string[] ip = data.Split('\n');
                string ip1 = ip[0];
                string ip2 = ip[1];
                IPEntity entity = new IPEntity();
                entity.StrIp = ip1.Split(',')[0];
                entity.Port = int.Parse(ip1.Split(',')[1]);

                IPEntity entity2 = new IPEntity();
                entity2.StrIp = ip2.Split(',')[0];
                entity2.Port = int.Parse(ip2.Split(',')[1]);
                List<IPEntity> list = new List<IPEntity>();
                list.Add(entity);
                list.Add(entity2);
                return list;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public class IPEntity
        {
            private string[] ip;

            private string strIp;

            public string StrIp
            {
                get { return strIp; }
                set { strIp = value; }
            }

            public string[] Ip
            {
                get { return ip; }
                set { ip = value; }
            }
            private int port;

            public int Port
            {
                get { return port; }
                set { port = value; }
            }

        }
    }
}
