using MissionBossAot.Models;
using MissionBossAot.Views;
using S7.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml.Linq;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MissionBossAot.Common
{
    /// <summary>
    /// 全局设备操作维护类
    /// </summary>
    public class GlobalDevice
    {
        private int readTimeoutMs = 1000;
        //与设备保持连接的心跳间隔秒数
        private int heartSeconds = 30;
        private List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
        //设备连接操作队列
        private ConcurrentQueue<DeviceConnectInfo> deviceConnectInfos = new ConcurrentQueue<DeviceConnectInfo>();
        //正在请求连接的队列
        private ConcurrentQueue<DeviceConnecting> deviceConnectingList = new ConcurrentQueue<DeviceConnecting>();
        public GlobalDevice()
        {
            SetGlobalDevice();
            GetStationDevice();
            UpdateDeviceStatus();
            OpenDevice();
            
        }
        public static GlobalDevice sGlobalDevice = new GlobalDevice();
        public void DeviceListInit()
        {}
        //获取设备列表
        public List<DeviceInfo> GetDeviceInfos() { return deviceInfos; }
        //获取设备信息
        public List<BoxItem> GetDeviceCardList()
        {
            List<BoxItem> boxItems = new List<BoxItem>();
            try
            {
                foreach (var item in deviceInfos)
                {
                    string col = "red";
                    if (item.DeviceConnectStatus)
                    {
                        col = "#4CAF50";
                    }
                    boxItems.Add(new BoxItem() {DevId= item.Id, Label= item.ExecutionOrder + "PLC", Value=item.DeviceName, TopRightColor=col});
                }
            }
            catch (Exception)
            {
            }
            return boxItems;
        }
        //关闭所有硬件连接
        public void CloseAllDeviceConnect(bool act)
        {
            //当主程序关闭的时候 需要断开所有设备的连接
        }
        //新增连接状态，心跳操作，数据交互操作都要返回连接状态
        public void AddConnectStatus(DeviceConnectInfo info)
        {
            deviceConnectInfos.Enqueue(info);
        }
        //更新连接状态
        public void OnConnectionStatusUpdate(Plc_S7 plc_S7, int d, bool success)
        {
            DeviceConnectInfo info = new DeviceConnectInfo() { DevId = d, ConnectSuccess = success, PlcS7 = plc_S7 };
            deviceConnectInfos.Enqueue(info);
        }
        //通过线程更新设备状态
        private void UpdateDeviceStatus()
        {
            Thread td = new Thread(new ThreadStart(UpdateStatusInList));
            td.Start();
        }
        //持续更新设备连接状态信息
        private void UpdateStatusInList()
        {
            while (!GlobalProperty.sGlobalProperty.GetMainFormCloseStatus()) 
            {
                //设备列表中的设备是否正在尝试连接，为了避免出现重复请求连接，如果正在请求连接则不再发起新的连接
                while (deviceConnectingList.TryDequeue(out DeviceConnecting deviceConnecting))
                {
                    DeviceInfo deviceInfo = deviceInfos.FindAll(a => a.Id == deviceConnecting.DevId).FirstOrDefault();
                    if (deviceInfo != null)
                    {
                        deviceInfo.DeviceTryConnecting = deviceConnecting.IfConnecting;
                    }
                    Thread.Sleep(50);
                }

                while (deviceConnectInfos.TryDequeue(out DeviceConnectInfo deviceConnectInfo))
                {
                    DeviceInfo deviceInfo = deviceInfos.FindAll(a => a.Id == deviceConnectInfo.DevId).FirstOrDefault();
                    if (deviceInfo != null)
                    {
                        deviceInfo.DeviceConnectStatus = deviceConnectInfo.ConnectSuccess;
                        if (!deviceInfo.DeviceConnectStatus)
                        {
                            if (!deviceInfo.DeviceTryConnecting)
                            {
                                //如果没有连接上 同时也没有在尝试连接 则发起一次新的连接请求
                                //Thread td = new Thread(new ParameterizedThreadStart(OpenDeviceProcess));
                                //td.Start(deviceInfo);
                                OpenDeviceProcess(deviceInfo);
                                deviceInfo.DeviceTryConnecting = true;
                            }
                        }
                        else
                        {
                            deviceInfo.s7Comm = deviceConnectInfo.PlcS7;
                            deviceInfo.DeviceTryConnecting = false;
                        }
                    }
                    Thread.Sleep(50);
                }
               
                Thread.Sleep(100);
            }
        }
      
        //启动设备连接
        private void OpenDevice()
        {
            foreach (var item in deviceInfos)
            {
                DeviceConnectInfo info = new DeviceConnectInfo() { DevId = item.Id, ConnectSuccess = false };
                deviceConnectInfos.Enqueue(info);
            }
        }
        //连接回调函数
        private void OnConnectionResult(Plc_S7 plc_S7, int d, bool success)
        {
            //将连接结果返回操作队列
            DeviceConnectInfo info = new DeviceConnectInfo() { DevId = d, ConnectSuccess = success, PlcS7 = plc_S7 };
            deviceConnectingList.Enqueue(new DeviceConnecting() { DevId = d, IfConnecting = false });//连接已结束 取消正在请求连接的状态
            deviceConnectInfos.Enqueue(info);
        }
        //打开设备连接
        private void OpenDeviceProcess(object obj)
        {
            try
            {
                DeviceInfo dev = obj as DeviceInfo;
                switch (dev.ConnectType)
                {
                    case "S7Comm":
                        byte rack = 0, slot = 1;
                        Plc_S7 plc_S7 = new Plc_S7(dev, rack, slot, CpuType.S71200, OnConnectionResult);
                        break;
                    case "TCP/IP":
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        //搜集所有设备信息写入设备表
        private void SetGlobalDevice()
        {
            List<WorkStationItem> stas = GlobalBaseData.sGlobalBaseData.GetWorkOrderStation(true);
            if (stas != null && stas.Count > 0)
            {
                using (var helper = new SqlConnectionHelper())
                {
                    try
                    {
                        helper.BeginTransaction();

                        string sqlDel = "delete from DeviceInfo";
                        helper.ExecuteNonQuery(sqlDel);

                        string sql = "insert into DeviceInfo(IncludType,Ip,Port,ParentId,DeviceType,ConnectType,ExecutionOrder) values";
                        for (int i = 0; i < stas.Count; i++)
                        {
                            WorkStationItem workStationItem = stas[i];
                            sql += "('" + WorkDeviceParentType.Station + "'," +
                                "'" + workStationItem.PlcIp + "'," +
                                "'" + workStationItem.PlcPort + "'," +
                                "'" + workStationItem.Id + "'," +
                                "'" + WorkDeviceType.Plc + "'," +
                                "'"+ workStationItem.ConnectType+ "'," +
                                ""+ workStationItem.ExecutionOrder + ")";
                            if (i != (stas.Count - 1))
                            {
                                sql += ",";
                            }
                        }
                        helper.ExecuteNonQuery(sql);

                        helper.CommitTransaction();
                    }
                    catch (Exception)
                    {
                        helper.RollbackTransaction();
                    }
                }
            }
        }
        //从设备表中获取所有设备信息
        private void GetStationDevice()
        {
            DataTable dt = null;
            try
            {
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM DeviceInfo";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    List<WorkStationItem> wSta=GlobalBaseData.sGlobalBaseData.GetWorkOrderStation();
                    foreach (DataRow row in dt.Rows)
                    {
                        string idStr = !Convert.IsDBNull(row["ID"]) ? row["ID"].ToString() : "0";
                        int.TryParse(idStr, out int idInt);

                        string postStr = !Convert.IsDBNull(row["Port"]) ? row["Port"].ToString() : "0";
                        int.TryParse(postStr, out int portInt);

                        string parStr = !Convert.IsDBNull(row["ParentId"]) ? row["ParentId"].ToString() : "0";
                        int.TryParse(parStr, out int parInt);

                        string eoStr = !Convert.IsDBNull(row["ExecutionOrder"]) ? row["ExecutionOrder"].ToString() : "0";
                        int.TryParse(eoStr, out int eoInt);

                        string parCode = "";
                        if (wSta != null && wSta.Count > 0)
                        {
                            WorkStationItem ws = wSta.FindAll(a => a.Id == parInt).FirstOrDefault();
                            if (ws != null) 
                            {
                                parCode = ws.Code;
                            }
                        }
                        deviceInfos.Add(new DeviceInfo
                        {
                            Id = idInt,
                            DeviceName = parCode,
                            Ip = !Convert.IsDBNull(row["Ip"]) ? row["Ip"].ToString() : "",
                            Port = portInt,
                            IncludType = !Convert.IsDBNull(row["IncludType"]) ? row["IncludType"].ToString() : "",
                            DeviceType = !Convert.IsDBNull(row["DeviceType"]) ? row["DeviceType"].ToString() : "",
                            ParentId = parInt,
                            ParentCode= parCode,
                            ExecutionOrder = eoInt,
                            ConnectType = !Convert.IsDBNull(row["ConnectType"]) ? row["ConnectType"].ToString() : ""
                        });
                        deviceInfos = deviceInfos.OrderBy(a => a.ExecutionOrder).ToList();
                    }
                }
            }
            catch (Exception)
            {
            }
        }









        #region P10
        
        #endregion
    }
    public class DeviceConnectInfo 
    {
        public int DevId { get; set; }
        public bool ConnectSuccess { get; set; }
        public Plc_S7 PlcS7 { get; set; }
    }
    public class DeviceConnecting
    {
        public int DevId { get; set; }
        public bool IfConnecting { get; set; }
    }
    /// <summary>
    /// 设备信息类
    /// </summary>
    public class DeviceInfo
    {
        public int Id { get; set; }
        public int ExecutionOrder { get; set; }
        public int ParentId { get; set; }
        public string ParentCode { get; set; }
        public string DeviceName { get; set; }
        public string Ip {  get; set; }
        public int Port { get; set; }
        public string IncludType { get; set; }
        public string DeviceType { get; set; }
        public bool DeviceConnectStatus { get; set; }
        public bool DeviceTryConnecting { get; set; }
        public string ConnectType { get; set; }
        public Plc_S7 s7Comm { get; set; }
    }
    //设备类型
    public enum WorkDeviceType
    {
        Plc,
        Other
    }
    //设备所属父类型
    public enum WorkDeviceParentType
    {
        Station,
        Other
    }
}
