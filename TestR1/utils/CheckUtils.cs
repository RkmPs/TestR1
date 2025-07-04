using BLEDeviceAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UHFAPP.utils;
using System.Net;
using UHFAPP.Entity;
namespace UHFAPP
{
    public class CheckUtils
    {

        public static int getInsertIndex(List<ReaderDeviceInfo> listIp, ReaderDeviceInfo info, bool[] exists)
        {

            int startIndex = 0;
            int endIndex = listIp.Count;
            int judgeIndex;
            int ret;
            if (endIndex == 0)
            {
                exists[0] = false;
                return 0;
            }

            byte[] ipAndMac = info.GetIpAndMac() ; //ip.Split('.'); //System.Text.ASCIIEncoding.ASCII.GetBytes(ip.Replace(".",""));
            endIndex--;
            while (true)
            {
                judgeIndex = (startIndex + endIndex) / 2;
                byte[] temp = listIp[judgeIndex].GetIpAndMac();// System.Text.ASCIIEncoding.ASCII.GetBytes(listIp[judgeIndex].Replace(".", ""));
                ret = compareBytes(ipAndMac, temp);
                if (ret > 0)
                {
                    if (judgeIndex == endIndex)
                    {
                        exists[0] = false;
                        return judgeIndex + 1;
                    }
                    startIndex = judgeIndex + 1;
                }
                else if (ret < 0)
                {
                    if (judgeIndex == startIndex)
                    {
                        exists[0] = false;
                        return judgeIndex;
                    }
                    endIndex = judgeIndex - 1;
                }
                else
                {
                    exists[0] = true;
                    return judgeIndex;
                }
            }

        }


        public static int getInsertIndex(List<EpcInfo> listEpc, string Epc, string tid, bool[] exists)
        {

            int startIndex = 0;
            int endIndex = listEpc.Count;
            int judgeIndex;
            int ret;
            if (endIndex == 0)
            {
                exists[0] = false;
                return 0;
            }
            endIndex--;
            //----------�ϲ�EPC��TID----------------
            int tidLen = string.IsNullOrEmpty(tid) ? 0 : tid.Length / 2;
            byte[] EpcAndTidBytes = new byte[Epc.Length / 2 + tidLen];
            byte[] epcBytes = DataConvert.HexStringToByteArray(Epc);
            for (int k = 0; epcBytes != null && k < epcBytes.Length; k++)
            {
                EpcAndTidBytes[k] = epcBytes[k];
            }
            if (tidLen > 0)
            {

                byte[] tidBytes = DataConvert.HexStringToByteArray(tid);
                for (int k = 0; k < tidLen; k++)
                {
                    if (epcBytes != null)
                    {
                        EpcAndTidBytes[k + epcBytes.Length] = tidBytes[k];
                    }
                    else
                    {
                        EpcAndTidBytes[k] = tidBytes[k];
                    }

                }
            }


            while (true)
            {
                judgeIndex = (startIndex + endIndex) / 2;
                if (EpcAndTidBytes == null && listEpc[judgeIndex].EpcAndTidBytes == null)
                {
                    exists[0] = true;
                    return judgeIndex;
                }
                ret = compareBytes(EpcAndTidBytes, listEpc[judgeIndex].EpcAndTidBytes);
                if (ret > 0)
                {
                    if (judgeIndex == endIndex)
                    {
                        exists[0] = false;
                        return judgeIndex + 1;
                    }
                    startIndex = judgeIndex + 1;
                }
                else if (ret < 0)
                {
                    if (judgeIndex == startIndex)
                    {
                        exists[0] = false;
                        return judgeIndex;
                    }
                    endIndex = judgeIndex - 1;
                }
                else
                {
                    exists[0] = true;
                    return judgeIndex;
                }
            }

        }

        //return 1,2 b1>b2
        //return -1,-2 b1<b2
        //retrurn 0;b1 == b2
        private static int compareBytes(byte[] b1, byte[] b2)
        {
            int len = b1.Length < b2.Length ? b1.Length : b2.Length;
            int value1;
            int value2;
            for (int i = 0; i < len; i++)
            {
                value1 = b1[i] & 0xFF;
                value2 = b2[i] & 0xFF;
                if (value1 > value2)
                {
                    return 1;
                }
                else if (value1 < value2)
                {
                    return -1;
                }
            }
            if (b1.Length > b2.Length)
            {
                return 2;
            }
            else if (b1.Length < b2.Length)
            {
                return -2;
            }
            else
            {
                return 0;
            }
        }
    }
}



