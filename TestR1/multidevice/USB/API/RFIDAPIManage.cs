using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace UHFAPP.USB.multidevice
{
    public class RFIDAPIManage
    {
        public delegate void DisconnectEventHandler(string chipId);
        public event DisconnectEventHandler DisconnectEvent;

        //第二步：定义断开回调委托
        internal static UHFAPP.UHFAPI.OnDisconnectCallback DisconnectCallback = null;
        private object _lock = new object();
        private Dictionary<string, RFIDAPI> _Devices = new Dictionary<string, RFIDAPI>();
        private List<IdInfo> idLists = new List<IdInfo>();
       
        private static RFIDAPIManage rfidAPIManage = new RFIDAPIManage();
        private RFIDAPIManage() {
            //第三步：给变量赋值
            DisconnectCallback = OnDisconnectCallback;
        }
       
        public static RFIDAPIManage GetInstance  {
            get { return rfidAPIManage; }
        }
        /// <summary>
        /// 获取设备
        /// </summary>
        /// <param name="chipId">芯片ID</param>
        /// <returns></returns>
        public RFIDAPI GetDeviceByChipId(string chipId)
        {
            lock (_lock)
            {
                if (!_Devices.ContainsKey(chipId))
                {
                    _Devices.Add(chipId, new RFIDAPI(chipId));
                }
                RFIDAPI obj = _Devices[chipId];
                return obj;

            }
        }
        /// <summary>
        /// 获取所有连接的设备
        /// </summary>
        /// <returns></returns>
        public List<string> GetConnectedDevice()
        {
            List<DeviceInfo> list = null;

            list= UHFAPI.getInstance().LinkGetDeviceInfo(UHFAPI.DEVICE_CONNECTED);

            lock (_lock)
            {
                List<String> li=new List<string>();
                foreach (IdInfo idInfo in idLists) {
                    li.Add(idInfo.chipId);
                }
                return li;
            }
        }
        public List<string> GetAllDevice() {
            List<DeviceInfo> list = null;

            list = UHFAPI.getInstance().LinkGetDeviceInfo(UHFAPI.DEVICE_ALL);

            lock (_lock)
            {
                List<String> li = new List<string>();
                foreach (IdInfo idInfo in idLists)
                {
                    li.Add(idInfo.chipId);
                }
                return li;
            }
        }

        #region
        //第一步：定义断开回调函数
        private void OnDisconnectCallback(int connectId)
        {
            string chipId = GetChipIdByConnectId(connectId);
            deleteIdInfo(chipId);
            System.Console.WriteLine("OnDisconnectCallback connectId=" + connectId + "  chipId=" + chipId);
            if (DisconnectEvent != null)
            {
                new Thread(new ThreadStart(delegate { DisconnectEvent(chipId); })).Start();
            }
        }
    
        #endregion


        #region  连接断开Connect\Disconnect
 
        //连接
        public bool Connect()
        {
            bool result = UHFAPI.getInstance().OpenUsb();
            //第四步：设置回调
            UHFAPI.SetDisconnectCallback(DisconnectCallback);
            if (result)
            {
                AddIdInfo();
            }
            return result;
        }
        //断开
        public bool Disconnect(string chipId)
        {
            if (!SelectDevice(chipId))
            {
                return false;
            }
            UHFAPI.getInstance().CloseUsb();
            //deleteIdInfo(chipId);
            return true;
        }
        //断开所有连接
        public void DisconnectAll()
        {
            UHFAPI.getInstance().LinkDisConnectAllDevice();
        }

        //选择要操作的设备
        private bool SelectDevice(string chipId)
        {
            List<DeviceInfo> list = UHFAPI.getInstance().LinkGetDeviceInfo(UHFAPI.DEVICE_CONNECTED);
            if (list != null)
            {
                int connectId = GetConnectIdBychipId(chipId) ;
          
                for (int k = 0; k < list.Count; k++)
                {
                    if (list[k].Id == connectId)
                    {
                        UHFAPI.getInstance().LinkSelectDevice(list[k].Id);
                        return true;
                    }
                }
            }
            return false;
        }

        internal int GetConnectIdBychipId(string chipId)
        {
            lock (_lock)
            {
                int connectId = -1;

                for (int k = 0; k < idLists.Count; k++)
                {
                    IdInfo idInfo = idLists[k];
                    if (idInfo.chipId == chipId)
                    {
                        connectId = idInfo.connectId;
                        break;
                    }

                }

                return connectId;
            }
        }
        private string GetChipIdByConnectId(int connectId)
        {
            lock (_lock)
            {
                string ChipId = "";

                for (int k = 0; k < idLists.Count; k++)
                {
                    IdInfo idInfo = idLists[k];
                    if (idInfo.connectId == connectId)
                    {
                        ChipId = idInfo.chipId;
                        break;
                    }

                }

                return ChipId;
            }
        }
        //断开连接删除设备信息
        private void deleteIdInfo(string chipId)
        {
            lock (_lock)
            {
                for(int k = 0; k < idLists.Count; k++)
                {
                    IdInfo idInfo = idLists[k];
                    if (idInfo.chipId == chipId)
                    {
                        idLists.RemoveAt(k);
                        break;
                    }
                }
                GetDeviceByChipId(chipId).Connected = false;//标记连接已经断开
            }
        }
        //连接成功增加设备信息
        private void AddIdInfo() {
            lock (_lock)
            {
                //所有连接的设备
                List<DeviceInfo> list = UHFAPI.getInstance().LinkGetDeviceInfo(UHFAPI.DEVICE_CONNECTED);
                if (list != null)
                {
                    foreach (DeviceInfo deviceInfo in list)
                    {
                        bool isFlag = false;
                        foreach (IdInfo idInfo in idLists)   
                        {
                            if (idInfo.connectId == deviceInfo.Id)
                            {
                                isFlag = true;
                                break;
                            }
                        }
                        if (deviceInfo.Type == "USB")
                        {
                            if (!isFlag)
                            {
                                //选择新连接的设备
                                UHFAPI.getInstance().LinkSelectDevice(deviceInfo.Id);
                                //获取设备的芯片id
                                string chipId = GetChipId();
                                System.Console.WriteLine("AddIdInfo connectId=" + deviceInfo.Id + "  chipId=" + chipId);
                                //添加id信息
                                idLists.Add(new IdInfo(chipId, deviceInfo.Id));
                                //设置连接状态
                                GetDeviceByChipId(chipId).Connected |= true;
                            }

                        }
                
                    }
                }
                

            }

        }
        public  class IdInfo
        {

            public  string chipId;
            public  int connectId;

            public IdInfo(string chipId, int connectId)
            {
                this.chipId = chipId;
                this.connectId = connectId;
            }
        }

        private string GetChipId() {
            string id = UHFAPI.getInstance().GetUHFGetDeviceID();
            return id.Replace(" ","") ;
        }
        #endregion
    }
}
