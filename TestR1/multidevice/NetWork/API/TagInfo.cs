using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLEDeviceAPI;

namespace UHFAPP.MultiDevice.NetWork.API
{
    public class TagInfo
    {
    
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }


        private int errCode;

        public int ErrCode
        {
            get { return errCode; }
            set { errCode = value; }
        }

        private UHFTAGInfo uhfTagInfo;

        public UHFTAGInfo UhfTagInfo
        {
            get { return uhfTagInfo; }
            set { uhfTagInfo = value; }
        }
          
     

    }
}
