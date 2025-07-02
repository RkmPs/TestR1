using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UHFAPP.MultiDevice.NetWork.API
{
    /// <summary>
    /// 天线功率的实体对象(Antenna Power Entity)
    /// </summary>
    public class AntennaPowerEntity
    {
        public AntennaPowerEntity(AntennaEnum antenna, int power)
        {
            this.antenna = antenna;
            this.power = power;
        }
        private AntennaEnum antenna;
        private int power;

        /// <summary>
        /// 天线号(No signal)
        /// </summary>
        public AntennaEnum Antenna
        {
            get { return antenna; }
            set { antenna = value; }
        }
        /// <summary>
        /// 天线功率(Antenna Power)
        /// </summary>
        public int Power
        {
            get { return power; }
            set { power = value; }
        }


    }
}
