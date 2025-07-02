using BLEDeviceAPI;
using BLEDeviceAPI.interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
 

namespace UHFAPP.MultiDevice.NetWork.API
{
    public class RFIDAPI
    {
        ///// <summary>
        ///// 盘点标签委托 (inventory tag delegation)
        ///// </summary>
        ///// <param name="device">设备对象(device)</param>
        ///// <param name="eventArgs">标签上报事件(tag report event)</param>
        //public delegate void InventoryTagEventHandler(string device, InventoryTagEventArgs eventArgs);
        ///// <summary>
        ///// 盘点标签事件(inventory tag event)
        ///// </summary>
        //public  event InventoryTagEventHandler InventoryTagEvent = null;

        public delegate void DisconnectEventHandler(string ip);
        public event DisconnectEventHandler DisconnectEvent;

        private UHFAPI uhf = UHFAPI.getInstance();
        private string ip = "";
        private bool Connected = false;
        private static bool isRuning = false;
        private int id = -1;

        public int Id { get => id;  }

        public RFIDAPI(string deviceIp)
        {
            ip = deviceIp;
        }
        public string GetIP()
        {
            return ip;
        }

        #region  连接断开Connect\Disconnect

        public bool IsConnected() {
            Console.WriteLine("IsConnected Connected="+ Connected);
            return Connected;
        }
        public bool Connect(int port)
        {
            bool result = uhf.TcpConnect(ip, (uint)port);
            //第四步：设置回调
            UHFAPI.SetDisconnectCallback(RFIDAPIManage.DisconnectCallback);
            Connected = result;
            Console.WriteLine("Connect result=" + Connected);
            if (result)
            {
                id = GetId(ip);
            }
            return result;
        }
        public bool Disconnect()
        {
            if (!IsConnected())
            {
                return false;
            }
            StopInventory(0);
            if (!SelectDevice(ip))
            {
                Connected = false;
                return false;
            }
            uhf.TcpDisconnect();
            Connected = false;
            Console.WriteLine("Disconnect Connected=false");
            id = -1;
            RFIDAPIManage.GetInstance.StopThread();
            RFIDDisconnectCallbank();
            return true;
        }
        internal void RFIDDisconnectCallbank()
        {
            if (DisconnectEvent != null)
            {
                Connected = false;
                Console.WriteLine("RFIDDisconnectCallbank Connected=false");
                id = -1;
                new Thread(new ThreadStart(delegate { DisconnectEvent(ip); })).Start();
            }
        }
        #endregion

        #region 盘点Inventory


        /// <summary>
        /// 设置寻标签过滤设置 (To find target tag)
        /// </summary>
        /// <param name="saveflag">1:掉电保存， 0：不保存</param>
        /// <param name="bank">0x01:EPC , 0x02:TID, 0x03:USR</param>
        /// <param name="startaddr">起始地址，单位：字节</param>
        /// <param name="datalen">数据长度， 单位:字节</param>
        /// <param name="databuf">数据</param>
        /// <returns>true:success  false:failure</returns>
        public bool SetFilter(byte saveflag, byte bank, int startaddr, int datalen, byte[] databuf)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            return uhf.SetFilter(saveflag, bank, startaddr, datalen, databuf);
        }

        /// <summary>
        /// 连续盘存标签(continuous inventory)
        /// </summary>
        /// <remarks>在调用开始盘点标签之前,先监听InventoryTagEventArgs事件，当前有标签数据后会通过InventoryTagEventArgs事件上报到用户；
        /// 注意: 执行开始盘点命令后RFID模块只响应StopInventory()函数的命令。所以在盘点的过程中无法获取功率、模块温度、读写等操作。
        /// (before inventory, it will monitor InventoryTagEventArgs event and report existing tag data to users through InventoryTagArgs event;
        ///  Attention: RFID module will only response to StopInventory() command during the inventory. Therefore, it will not response to other command like getPower,getModuleTemperature,read or write tags)
        /// </remarks>
        /// <returns>true:success  false:failure</returns>
        public bool StartInventory()
        {
            int id = GetId(ip);
            if (id < 0)
            {
                return false;
            }
            bool result = uhf.InventoryById(id);
            if (result)
            {
                RFIDAPIManage.GetInstance.StartThread();
            }
            return result;
        }
        /// <summary>
        /// 停止盘存标签 (Stop inventory)
        /// </summary>
        /// <returns>true:success  false:failure</returns>
        public bool StopInventory() {
            return StopInventory(0);
        }

        #endregion

        #region  功率AntennaPower
        /// <summary>
        /// 设置天线功率(set antenna power)
        /// </summary>
        /// <param name="ant">天线号(antenna number)</param>
        /// <param name="power">功率(power)</param>
        /// <returns>true:Success  false:Failure</returns>
        public bool SetAntennaPower(AntennaEnum ant, int power)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            bool isSave = true;
            return uhf.SetAntennaPower((byte)(isSave ? 1 : 0), (byte)ant, (byte)power);
        }

        /// <summary>
        /// 获取天线功率(get antenna power)
        /// </summary>
        /// <returns>所有天线的功率(all antenna power)</returns>
        public List<AntennaPowerEntity> GetAntennaPower() {
            if (!SelectDevice(ip))
            {
                return null;
            }
            byte[] power = new byte[16];
            if (uhf.GetAntennaPower(power))
            {
                List<AntennaPowerEntity> list = new List<AntennaPowerEntity>();
                list.Add(new AntennaPowerEntity(AntennaEnum.ANT1, power[0]));
                list.Add(new AntennaPowerEntity(AntennaEnum.ANT2, (power[1] == 0 ? power[0] : power[1])  ));
                list.Add(new AntennaPowerEntity(AntennaEnum.ANT3, (power[2] == 0 ? power[0] : power[2]) ));
                list.Add(new AntennaPowerEntity(AntennaEnum.ANT4, (power[3] == 0 ? power[0] : power[3]) ));
                list.Add(new AntennaPowerEntity(AntennaEnum.ANT5, (power[4] == 0 ? power[0] : power[4]) ));
                list.Add(new AntennaPowerEntity(AntennaEnum.ANT6, (power[5] == 0 ? power[0] : power[5]) ));
                list.Add(new AntennaPowerEntity(AntennaEnum.ANT7, (power[6] == 0 ? power[0] : power[6]) ));
                list.Add(new AntennaPowerEntity(AntennaEnum.ANT8, (power[7] == 0 ? power[0] : power[7]) ));
                return list;
            }
            return null;
        }

        #endregion

        #region   盘点模式InventoryMode
        /// <summary>
        /// 设置EPC模式(set EPC mode)
        /// </summary>
        /// <returns>true:success  false:failure</returns>
        public bool SetEPCMode()
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            bool isSave = true;
            return uhf.setEPCMode(isSave);
        }
        /// <summary>
        /// 设置EPC 和TID 模式(set EPC + TID mode)
        /// </summary>
        /// <returns>true:success  false:failure</returns>
        public bool SetEPCAndTIDMode()
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            bool isSave = true;
            return uhf.setEPCAndTIDMode(isSave);
        }
        /// <summary>
        /// 设置EPC+TID+User模式 (set EPC+TID+User mode)
        /// </summary>
        /// <param name="userAddress">user区域起始地址(start and end address of User bank)</param>
        /// <param name="userLenth">user区长度(user bank length)</param>
        /// <returns>true:success  false:failure</returns>
        public bool SetEPCAndTidUserMode(int userAddress, int userLenth)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            bool isSave = true;
            return uhf.setEPCAndTIDUSERMode(isSave, (byte)userAddress, (byte)userLenth);
        }
        /// <summary>
        /// 获取盘点模式(get inventory mode)
        /// </summary>
        /// <param name="userAddress">user区域起始地址(start and end address of user bank)</param>
        /// <param name="userLenth">user区长度(user bank length)</param>
        /// <returns>  0x00:EPC ;  0x01:EPC+TID ;  0x02:EPC+TID+USER </returns>
        public int GetEPCAndTidUserMode(ref int userAddress, ref int userLenth)
        {
            if (!SelectDevice(ip))
            {
                return -1;
            }
            byte userPtr = 0;
            byte userLen = 0;

            int mode = uhf.getEPCTIDUSERMode(ref userPtr, ref userLen);
            return mode;
        }
        #endregion

        #region  Session
        /// <summary>
        /// 设置GEN2参数，包括Session、Q、Coding 等参数 ( set GEN2 parameter,including parameters like Session、Q、Coding.)
        /// </summary>
        /// <param name="Target">query 命令的 Target 参数，0-A，1-B</param>
        /// <param name="Session">query 命令的 session 参数，0-S0、1-S1、2-S2、3-S3</param>
        /// <returns>true:success  false:failure</returns>
        public bool SetGen2(int Target, int Session)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            byte truncate = 0;
            byte Action = 0;
            byte T = 0;
            byte Q = 0;
            byte StartQ = 0;
            byte MinQ = 0;
            byte MaxQ = 0;
            byte D = 0;
            byte Coding = 0;
            byte P = 0;
            byte Sel = 0;
            byte Session_temp = 0;
            byte G = 0;
            byte LF = 0;

            bool result = uhf.GetGen2(ref truncate, ref Action, ref T, ref Q,
                     ref StartQ, ref MinQ,
                     ref MaxQ, ref D, ref Coding, ref P,
                     ref Sel, ref Session_temp, ref G, ref LF);
            if (!result)
            {
                return false;
            }

            //Target = G;
            //Session = Session_temp;

            if (uhf.SetGen2(truncate, Action, T, Q, StartQ, MinQ, MaxQ, D, Coding, P, Sel, (byte)Session, (byte)Target, LF))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 获取Gen2参数，包括Session、Q、Coding 等参数 (get Gen2 parameter, including Session、Q、Coding)
        /// </summary>
        /// <param name="Target">query 命令的 Target 参数，0-A，1-B</param>
        /// <param name="Session">query 命令的 session 参数，0-S0、1-S1、2-S2、3-S3</param>
        /// <returns></returns>
        public bool GetGen2(ref int Target, ref int Session)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            byte truncate = 0;
            byte Action = 0;
            byte T = 0;
            byte Q = 0;
            byte StartQ = 0;
            byte MinQ = 0;
            byte MaxQ = 0;
            byte D = 0;
            byte Coding = 0;
            byte P = 0;
            byte Sel = 0;
            byte Session_temp = 0;
            byte G = 0;
            byte LF = 0;

            bool result = uhf.GetGen2(ref truncate, ref Action, ref T, ref Q,
                     ref StartQ, ref MinQ,
                     ref MaxQ, ref D, ref Coding, ref P,
                     ref Sel, ref Session_temp, ref G, ref LF);
            if (result)
            {
                Target = G;
                Session = Session_temp;
            }
            return result;
        }
        #endregion

        #region  协议Protocol
        /// <summary>
        /// 设置协议类型 (set protocol type)
        /// </summary>
        /// <param name="type">0x00:ISO18000-6C, 0x01:GB/T 29768, 0x02:GJB 7377.1</param>
        /// <returns>true:success  false:failure</returns>
        public bool SetProtocol(int type)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            return uhf.SetProtocol((byte)type);
        }
        /// <summary>
        /// 获取协议类型 (get protocol type)
        /// </summary>
        /// <returns>0x00:ISO18000-6C, 0x01:GB/T 29768, 0x02:GJB 7377.1</returns>
        public int GetProtocol()
        {
            if (!SelectDevice(ip))
            {
                return -1;
            }
            return uhf.GetProtocol();
        }
        #endregion

        #region 蜂鸣器
        /// <summary>
        /// 设置蜂鸣器 (Set buzzer)
        /// </summary>
        /// <param name="IsOpen">true:开(on)  false:关(off)</param>
        /// <remarks>蜂鸣器如果是打开状态那么盘点过程中读到标签后会有嘟嘟嘟的声音 (if the buzzer is on,it will "beep" during the inventory)</remarks>
        /// <returns>true:success  false:failure</returns>
        public bool SetBeep(bool IsOpen)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            byte mode = IsOpen ? (byte)1 : (byte)0;
            return uhf.UHFSetBuzzer(mode);
        }
        /// <summary>
        /// 获取蜂鸣器状态 (Get buzzer state)
        /// </summary>
        /// <returns>1:打开(on)  0:关闭(off)</returns>
        public int GetBeep()
        {
            if (!SelectDevice(ip))
            {
                return -1;
            }
            byte[] mode = new byte[10];
            if (!uhf.UHFGetBuzzer(mode))
            {
                return -1;
            }
            else
            {
                return mode[0];
            }
        }
        #endregion 蜂鸣器

        #region 版本号FirmwareVersion
        /// <summary>
        /// 获取UHF固件版本 (Get UHF firmware version)
        /// </summary>
        /// <returns>UHF固件版本 (UHF firmware version)</returns>
        public string GetUHFFirmwareVersion()
        {
            if (!SelectDevice(ip))
            {
                return "";
            }
            return uhf.GetSoftwareVersion().Replace("\0", ""); ;
        }
        #endregion

        #region 频率FrequencyMode
        /// <summary>
        /// 设置频率(set frequency)
        /// </summary>
        /// <param name="region">0x01(China1),0x02(China2),0x04(Europe),0x08(USA),0x16(Korea),0x32(Japan)</param>
        /// <returns>true:success  false:failure</returns>
        public bool SetFrequencyMode(int region)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            return uhf.SetRegion((byte)1, (byte)region);
        }
        /// <summary>
        /// 获取频率(get frequency)
        /// </summary>
        /// <returns>0x01(China1),0x02(China2),0x04(Europe),0x08(USA),0x16(Korea),0x32(Japan)</returns>
        public int GetFrequencyMode()
        {
            if (!SelectDevice(ip))
            {
                return -1;
            }
            byte region2 = (byte)0;
            if (!uhf.GetRegion(ref region2))
            {
                return -1;
            }
            return (int)region2;
        }
        #endregion

        #region RFLink FastID Tagfocus
        /// <summary>
        /// 设置链路组合 (Set RFlink)
        /// </summary>
        /// <param name="mode">0:DSB_ASK/FM0/40KHZ , 1:PR_ASK/Miller4/250KHZ , 2:PR_ASK/Miller4/300KHZ, 3:DSB_ASK/FM0/400KHZ</param>
        /// <returns>true:success  false:failure</returns>
        public bool SetRFLink(int mode)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }

            return uhf.SetRFLink((byte)1, (byte)mode);
        }
        /// <summary>
        /// 获取链路组合 (get RFlink )
        /// </summary>
        /// <returns>0:DSB_ASK/FM0/40KHZ , 1:PR_ASK/Miller4/250KHZ , 2:PR_ASK/Miller4/300KHZ, 3:DSB_ASK/FM0/400KHZ</returns>
        public int GetRFLink()
        {
            if (!SelectDevice(ip))
            {
                return -1;
            }

            byte mode = 0;
            if (!uhf.GetRFLink(ref mode))
            {
                return -1;
            }
            return mode;
        }
        /// <summary>
        /// 设置FastID功能 (set FastID function)
        /// </summary>
        /// <param name="flag">1:开启(on)， 0：关闭(off)</param>
        /// <returns>true:success  false:failure</returns>
        public bool SetFastID(int flag)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            return uhf.SetFastID((byte)flag);
        }
        /// <summary>
        /// 获取FastID功能 (get FastID function)
        /// </summary>
        /// <returns>1:开启(on)， 0：关闭(off)</returns>
        public int GetFastID()
        {
            if (!SelectDevice(ip))
            {
                return -1;
            }

            byte flag = 0;
            if (!uhf.GetFastID(ref flag)) {
                return -1;
            }
            return flag;
        }
        /// <summary>
        /// 设置Tagfocus功能 (set Tagfocus function)
        /// </summary>
        /// <param name="flag">1:开启(on)， 0：关闭(off)</param>
        /// <returns>true:success  false:failure</returns>
        public bool SetTagfocus(int flag)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }

            return uhf.SetTagfocus((byte)flag);
        }
        /// <summary>
        /// 获取Tagfocus功能 (get Tagfocus function)
        /// </summary>
        /// <returns>1:开启(on)， 0：关闭(off)</returns>
        public int GetTagfocus()
        {
            if (!SelectDevice(ip))
            {
                return -1;
            }

            byte flag = 0;
            if (!uhf.GetTagfocus(ref flag))
            {
                return -1;
            }
            return flag;
        }
        #endregion

        #region Read、Write、Lock、kill
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="uAccessPwd">4字节密码</param>
        /// <param name="FilterBank">启动过滤的bank号， 1：EPC, 2:TID, 3:USR</param>
        /// <param name="FilterStartaddr">启动过滤的起始地址， 单位：字节</param>
        /// <param name="FilterLen">启动过滤的长度， 单位：字节</param>
        /// <param name="FilterData">启动过滤的数据</param>
        /// <param name="uBank">读取数据的bank</param>
        /// <param name="uPtr">读取数据的起始地址， 单位：字</param>
        /// <param name="uCnt">读取数据的长度， 单位：字</param>
        /// <returns>返回十六进制数据，读取失败返回""</returns>
        public string ReadData(byte[] uAccessPwd, byte FilterBank, int FilterStartaddr, int FilterLen, byte[] FilterData, byte uBank, int uPtr, int uCnt)
        {
            if (!SelectDevice(ip))
            {
                return null;
            }
            return uhf.ReadData(uAccessPwd, FilterBank, FilterStartaddr, FilterLen, FilterData, uBank, uPtr, uCnt);
        }

        /// <summary>
        /// 写标签数据区(bank for writing data)
        /// </summary>
        /// <param name="uAccessPwd">4字节密码</param>
        /// <param name="FilterBank">启动过滤的bank号， 1：EPC, 2:TID, 3:USR</param>
        /// <param name="FilterStartaddr">启动过滤的起始地址， 单位：bit</param>
        /// <param name="FilterLen">启动过滤的长度， 单位：bit</param>
        /// <param name="FilterData">启动过滤的数据</param>
        /// <param name="uBank">写入数据的bank</param>
        /// <param name="uPtr">写入数据的起始地址， 单位：字</param>
        /// <param name="uCnt">写入数据的长度， 单位：字</param>
        /// <param name="uDatabuf">写入的数据</param>
        /// <returns></returns>
        public bool WriteData(byte[] uAccessPwd, byte FilterBank, int FilterStartaddr, int FilterLen, byte[] FilterData, byte uBank, int uPtr, byte uCnt, byte[] uDatabuf)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }

            return uhf.WriteData(uAccessPwd, FilterBank, FilterStartaddr, FilterLen, FilterData, uBank, uPtr, uCnt, uDatabuf);
        }
        /// <summary>
        /// 锁标签(lock tags)
        /// </summary>
        /// <param name="uAccessPwd"> 4字节密码</param>
        /// <param name="FilterBank">启动过滤的bank号， 1：EPC, 2:TID, 3:USR</param>
        /// <param name="FilterStartaddr">启动过滤的起始地址， 单位：字节</param>
        /// <param name="FilterLen">启动过滤的长度， 单位：字节</param>
        /// <param name="FilterData">启动过滤的数据</param>
        /// <param name="lockbuf">3字节，第0-9位为Action位， 第10-19位为Mask位</param>
        /// <returns>true:success  false:failure</returns>
        public bool LockTag(byte[] uAccessPwd, byte FilterBank, int FilterStartaddr, int FilterLen, byte[] FilterData, byte[] lockbuf)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }

            return uhf.LockTag(uAccessPwd, FilterBank, FilterStartaddr, FilterLen, FilterData, lockbuf);
        }
        /// <summary>
        ///Kill标签 (kill tags)
        /// </summary>
        /// <param name="uAccessPwd">4字节密码</param>
        /// <param name="FilterBank">启动过滤的bank号， 1：EPC, 2:TID, 3:USR</param>
        /// <param name="FilterStartaddr">启动过滤的起始地址， 单位：字节</param>
        /// <param name="FilterLen">启动过滤的长度， 单位：字节</param>
        /// <param name="FilterData">启动过滤的数据</param>
        /// <returns>true:success  false:failure</returns>
        public bool KillTag(byte[] uAccessPwd, byte FilterBank, int FilterStartaddr, int FilterLen, byte[] FilterData)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }

            return uhf.KillTag(uAccessPwd, FilterBank, FilterStartaddr, FilterLen, FilterData);
 
        }
        #endregion

        #region Reader IP

        /// <summary>
        /// 设置读写器IP地址
        /// </summary>
        /// <param name="ip">ip</param>
        /// <param name="port">port</param>
        /// <param name="mask">mask</param>
        /// <param name="gate">gate</param>
        /// <returns></returns>
        public bool SetReaderIP(string ip, int port, string mask, string gate)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            return uhf.SetReaderIP(ip, port, mask, gate);
        }
        /// <summary>
        /// 获取读写器IP地址
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="mask"></param>
        /// <param name="gate"></param>
        /// <returns></returns>
        public bool GetReaderIP(ref string ip, ref int port, ref string mask, ref string gate)
        {
            if (!SelectDevice(ip))
            {
                return false;
            }
            StringBuilder sb_ip = new StringBuilder();
            StringBuilder sb_port = new StringBuilder();
            StringBuilder sb_mask = new StringBuilder();
            StringBuilder sb_gate = new StringBuilder();

            if(uhf.GetReaderIP(sb_ip, sb_port, sb_mask, sb_gate))
            {
                ip=sb_ip.ToString();
                port=int.Parse(sb_port.ToString());
                mask=sb_mask.ToString();
                gate = sb_gate.ToString();
                return true;
            }

            return false;
        }
        #endregion








        #region
        private bool StopInventory(int delay)
        {
            int id = GetId(ip);
            if (id < 0)
            {
                return false;
            }
            bool result = uhf.StopById(id);
            if (result)
            {
                Thread.Sleep(delay);
            }
            
            return result;
        }

    
        private static string GetIpById(int id)
        {
            List<DeviceInfo> deviceList = UHFAPI.getInstance().LinkGetDeviceInfo(UHFAPI.DEVICE_CONNECTED);
            if (deviceList != null && deviceList.Count > 0)
            {
                for (int k = 0; k < deviceList.Count; k++)
                {
                    DeviceInfo deviceInfo = deviceList[k];
                    if (id == deviceInfo.Id)
                    {
                        return deviceInfo.Ip;
                    }
                }
            }
            return "";
        }

   
        #endregion
        #region

        private bool SelectDevice(string ip)
        {
            List<DeviceInfo> list = uhf.LinkGetDeviceInfo(UHFAPI.DEVICE_CONNECTED);
            if (list != null)
            {
                for (int k = 0; k < list.Count; k++)
                {
                    if (list[k].Ip == ip)
                    {
                        uhf.LinkSelectDevice(list[k].Id);
                        return true;
                    }
                }

            }
            return false;
        }

        private int GetId(string ip)
        {
            List<DeviceInfo> deviceList = uhf.LinkGetDeviceInfo(UHFAPI.DEVICE_CONNECTED);
            if (deviceList != null && deviceList.Count > 0)
            {
                for (int k = 0; k < deviceList.Count; k++)
                {
                    DeviceInfo deviceInfo = deviceList[k];
                    if (ip == deviceInfo.Ip)
                    {
                        return deviceInfo.Id;
                    }
                }
            }
            return -1;
        }

      
 

        #endregion
    }
}
