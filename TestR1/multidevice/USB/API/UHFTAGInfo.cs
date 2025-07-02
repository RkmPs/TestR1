using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UHFAPP.USB.multidevice
{
    /// <summary>
    /// 标签信息(tag information)
    /// </summary>
    public class UHFTAGInfo
    {
        /// <summary>
        /// 构造函数 (Constructor)
        /// </summary>
        public UHFTAGInfo()
        {
            Count = 1;
        }
        /// <summary> 
        /// 标签次数 (tag count times)
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 标签PC值 (tag PC value)
        /// </summary>
        public string Pc { get; set; }
        /// <summary>
        /// 标签EPC (tag EPC)
        /// </summary>
        public string Epc { get; set; }
        /// <summary> 
        /// 标签TID (tag TID)
        /// </summary>
        public string Tid { get; set; }
        /// <summary>
        /// 标签USER (tag USER)
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// 读写器天线号 (reader antenna number)
        /// </summary>
        public int Ant { get; set; }
        /// <summary>
        /// 标签RSSI (tag RSSI)
        /// </summary>
        public float Rssi { get; set; }
        /// <summary>
        /// 标签EPC原始数据 (tag initial EPC data)
        /// </summary>
        public byte[] EpcBytes { get; set; }


    }
}
