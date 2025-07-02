using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UHFAPP.MultiDevice;



namespace UHFAPP.MultiDevice.NetWork.API
{
    /// <summary>
    /// 循环盘点标签时间 (cyclical inventory time)
    /// </summary>
    public class InventoryTagEventArgs : EventArgs
    {
        private UHFTAGInfo uhfTagInfo;

        /// <summary>
        /// 标签信息 (tag information)
        /// </summary>
        public UHFTAGInfo UHFTagInfo
        {
            get { return uhfTagInfo; }
            set { uhfTagInfo = value; }
        }

        /// <summary>
        /// 构造函数 (Constructor)
        /// </summary>
        /// <param name="info">标签信息(tag information)</param>
        public InventoryTagEventArgs(UHFTAGInfo info)
        {
            this.uhfTagInfo = info;
        }
    }
}
