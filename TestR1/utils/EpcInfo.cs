using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UHFAPP.utils
{
    public  class EpcInfo
    {
        public EpcInfo(string epc, string tid, int count, byte[] epcBytes, byte[] tidBytes)
        {
            this.tid = tid;
            this.epc = epc;
            this.count = count;
            this.epcBytes = epcBytes;
            this.epcBytes = epcBytes;
            if (epcBytes != null && epcBytes.Length > 0 && tidBytes != null && tidBytes.Length > 0)
            {
                epcAndTidBytes = new byte[epcBytes.Length + tidBytes.Length];
                for (int k = 0; k < epcBytes.Length; k++)
                {
                    epcAndTidBytes[k] = epcBytes[k];
                }
                for (int k = 0; k < tidBytes.Length; k++)
                {
                    epcAndTidBytes[k + epcBytes.Length] = tidBytes[k];
                }
            }
            else if (epcBytes != null && epcBytes.Length > 0)
            {
                epcAndTidBytes = epcBytes;
            }
            else if (tidBytes != null && tidBytes.Length > 0)
            {
                epcAndTidBytes = tidBytes;
            }
            else
            {
                epcAndTidBytes = new byte[0];
            }
        }
        public EpcInfo(string epc, string tid, int count, byte[] epcBytes, byte[] tidBytes, int ant, string rssi, string user)
        {
            this.user = user;
            this.tid = tid;
            this.epc = epc;
            this.count = count;
            this.rssi = rssi;
            this.ant = ant;
            this.epcBytes = epcBytes;
            this.epcBytes = epcBytes;
            if (epcBytes != null && epcBytes.Length > 0 && tidBytes != null && tidBytes.Length > 0)
            {
                epcAndTidBytes = new byte[epcBytes.Length + tidBytes.Length];
                for (int k = 0; k < epcBytes.Length; k++)
                {
                    epcAndTidBytes[k] = epcBytes[k];
                }
                for (int k = 0; k < tidBytes.Length; k++)
                {
                    epcAndTidBytes[k + epcBytes.Length] = tidBytes[k];
                }
            }
            else if (epcBytes != null && epcBytes.Length > 0)
            {
                epcAndTidBytes = epcBytes;
            }
            else if (tidBytes != null && tidBytes.Length > 0)
            {
                epcAndTidBytes = tidBytes;
            }
            else
            {
                epcAndTidBytes = new byte[0];
            }
            if (antList == null)
            {
                antList = new List<AntennaInfo>();
                antList.Add(new AntennaInfo(ant, rssi, 1));
            }
            else
            {
                AntennaInfo antennaInfo= GetAntennaInfoByAnt(ant);
                if (antennaInfo == null)
                {
                    antList.Add(new AntennaInfo(ant, rssi, 1));
                }
                else
                {
                    antennaInfo.Rssi = rssi;
                    antennaInfo.Count = antennaInfo.Count + 1;
                }
            }
        }
        private string user;
        private string epc;
        private int count;
        private byte[] epcBytes;
        private byte[] tidBytes;
        private byte[] epcAndTidBytes;
        private string tid;
        private int ant;
        private string rssi;
        private List<AntennaInfo> antList=null;



        public string User
        {
            get { return user; }
            set { user = value; }
        }

        public string Epc
        {
            get { return epc; }
            set { epc = value; }
        }


        public int Count
        {
            get
            {
                if (antList == null || antList.Count == 0)
                {
                    return count;
                }
                else
                {
                    int number = 0;
                    for (int k = 0; k < antList.Count; k++)
                    {
                        number+= antList[k].Count;
                    }
                    return number;
                }
            }
            set { count = value; }
        }

 
        public byte[] EpcBytes
        {
            get { return epcBytes; }
            set { epcBytes = value; }
        }


        public byte[] TidBytes
        {
            get { return tidBytes; }
            set { tidBytes = value; }
        }



        public byte[] EpcAndTidBytes
        {
            get { return epcAndTidBytes; }
            set { epcAndTidBytes = value; }
        }
     

        public string Tid
        {
            get { return tid; }
            set { tid = value; }
        }


        public bool AddAntennaInfoByAnt(int ant,string rssi)
        {
            for (int k = 0; k < antList.Count; k++)
            {
                if (antList[k].AntennaPort == ant)
                {
                    antList[k].Count= antList[k].Count +1;
                    antList[k].Rssi = rssi;
                    return true;
                }
            }
            for (int k = 0; k < antList.Count; k++)
            {
                if (antList[k].AntennaPort > ant)
                {
                    antList.Insert(k, new AntennaInfo(ant, rssi, 1));
                    return true;
                }
            }

            antList.Add(new AntennaInfo(ant, rssi, 1));
            return true;
        }

        public AntennaInfo GetAntennaInfoByAnt(int ant)
        {
            for (int k = 0; k < antList.Count; k++)
            {
                if (antList[k].AntennaPort == ant)
                {
                    return antList[k];
                }
            }
            return null;
        }

        public   List<AntennaInfo> AntList { get => antList; set => antList = value; }
        public int Ant { get => ant; set => ant = value; }
        public string Rssi { get => rssi; set => rssi = value; }

        public class AntennaInfo {

           
            public AntennaInfo(int antennaPort, string rssi, int count)
            {
                this.antennaPort = antennaPort;
                this.rssi = rssi;
                this.count = count;
            }
            private   int antennaPort = 0;
            private string rssi;
            private   int count;

            public   int AntennaPort { get => antennaPort; set => antennaPort = value; }
            public string Rssi { get => rssi; set => rssi = value; }
            public   int Count { get => count; set => count = value; }
        }

        public int No { get; set; } // Nomor urut di grid

        public string AntString
        {
            get
            {
                if (AntList == null || AntList.Count == 0) return "";
                return string.Join("\n", AntList.Select(a => $"ANT{a.AntennaPort}: {a.Count}"));
            }
        }

        public string AsciiEpc
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(epc)) return "";
                    byte[] epcBytes = UHFAPP.DataConvert.HexStringToByteArray(epc);
                    return Encoding.ASCII.GetString(epcBytes);
                }
                catch { return ""; }
            }
        }



    }
}
