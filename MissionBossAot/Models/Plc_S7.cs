using HandyControl.Tools.Extension;
using MissionBossAot.Common;
using MissionBossAot.Views;
using S7.Net;
using S7.Net.Types;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using DateTime = System.DateTime;

namespace MissionBossAot.Models
{
    // 定义回调委托
    public delegate void ConnectCallback(Plc_S7 plc_S7,int devId,bool success);
    /// <summary>
    /// PLC S7
    /// </summary>
    public class Plc_S7
    {
        private int timeCounter = 0;
        //设备信息
        private DeviceInfo _deviceInfo;
        //设备编号
        private int deviceId = 0;
        //回调函数
        private event ConnectCallback StatusChanged;
        //读取到工作数据之后通过队列写数据 
        private ConcurrentQueue<P10DB2000ReturnData> p10DB2000ReturnDatas = new ConcurrentQueue<P10DB2000ReturnData> ();
        private ConcurrentQueue<P20DB2000ReturnData> p20DB2000ReturnDatas = new ConcurrentQueue<P20DB2000ReturnData>();
        private ConcurrentQueue<P30DB3000ReturnData> p30DB2000ReturnDatas = new ConcurrentQueue<P30DB3000ReturnData>();
        private ConcurrentQueue<P40DB3001ReturnData> p40DB2000ReturnDatas = new ConcurrentQueue<P40DB3001ReturnData>();
        private ConcurrentQueue<P50DB3002ReturnData> p50DB2000ReturnDatas = new ConcurrentQueue<P50DB3002ReturnData>();
        private ConcurrentQueue<P60DB3000ReturnData> p60DB2000ReturnDatas = new ConcurrentQueue<P60DB3000ReturnData>();
        private ConcurrentQueue<P70DB3003ReturnData> p70DB2000ReturnDatas = new ConcurrentQueue<P70DB3003ReturnData>();
        private ConcurrentQueue<P80DB3004ReturnData> p80DB2000ReturnDatas = new ConcurrentQueue<P80DB3004ReturnData>();
        private ConcurrentQueue<P70DB3001UploadData> p70_biaoding_ReturnDatas = new ConcurrentQueue<P70DB3001UploadData>();
        private ConcurrentQueue<P70DB3001UploadData2> p70_fuzai_ReturnDatas = new ConcurrentQueue<P70DB3001UploadData2>();

        public Plc _plc;
        public Plc_S7(DeviceInfo deviceInfo, byte rack, byte slot, CpuType cpuType, ConnectCallback callback)
        {
            _deviceInfo = deviceInfo;
            _plc = new Plc(cpuType, _deviceInfo.Ip, rack, slot);

            // 设置读写超时时间为1秒
            _plc.ReadTimeout = 1000;
            _plc.WriteTimeout = 1000;

            StatusChanged += callback;
            deviceId = _deviceInfo.Id;
            bool b=Connect();

            if (b) 
            {
                Thread td = new Thread(new ThreadStart(ReadRecord));
                td.IsBackground = true;
                td.Start();
            }
        }
        /// <summary>
        /// 断开PLC连接    外界主动断开
        /// </summary>
        public void Disconnect()
        {
            if (_plc != null)
            {
                _plc.Close();
                GC.SuppressFinalize(this);
            }
        }


        #region 私有方法
        //记录采集数据
        private void ReadRecord() 
        {
            while (!GlobalProperty.sGlobalProperty.GetMainFormCloseStatus())
            {
                if (p70_fuzai_ReturnDatas.TryDequeue(out P70DB3001UploadData2 returnData_P70_fuzai))
                {
                    using (var helper = new SqlConnectionHelper())
                    {
                        try
                        {
                            helper.BeginTransaction();
                            string sqlDel = "delete from signal_data2 where Product_BarCode='" + returnData_P70_fuzai.ProductBarCode + "'";
                            helper.ExecuteNonQuery(sqlDel);

                            string sql = "insert into signal_data2(" +
                                "force_current_500n," +
                                "force_current_600n," +
                                "max_current," +
                                "max_voltage," +
                                "max_torque," +
                                "max_pressure," +
                                "reserve1," +
                                "reserve2," +
                                "reserve3," +
                                "reserve4," +
                                "CreatedTime," +
                                "Product_BarCode)" +
                                    "values(" +
                                    "@force_current_500n," +
                                    "@force_current_600n," +
                                    "@max_current," +
                                    "@max_voltage," +
                                    "@max_torque," +
                                    "@max_pressure," +
                                    "@reserve1," +
                                    "@reserve2," +
                                    "@reserve3," +
                                    "@reserve4," +
                                    "@CreatedTime," +
                                    "@Product_BarCode)";
                            var parametersInsert = new[]
                                {
                                        SqlConnectionHelper.CreateParameter("@force_current_500n", returnData_P70_fuzai.ForceCurrent500n),
                                        SqlConnectionHelper.CreateParameter("@force_current_600n", returnData_P70_fuzai.ForceCurrent600n),
                                        SqlConnectionHelper.CreateParameter("@max_current", returnData_P70_fuzai.MaxCurrent),
                                        SqlConnectionHelper.CreateParameter("@max_voltage", returnData_P70_fuzai.MaxVoltage),
                                        SqlConnectionHelper.CreateParameter("@max_torque", returnData_P70_fuzai.MaxTorque),
                                        SqlConnectionHelper.CreateParameter("@max_pressure", returnData_P70_fuzai.MaxPressure),
                                        SqlConnectionHelper.CreateParameter("@reserve1", returnData_P70_fuzai.Reserve1),
                                        SqlConnectionHelper.CreateParameter("@reserve2", returnData_P70_fuzai.Reserve2),
                                        SqlConnectionHelper.CreateParameter("@reserve3", returnData_P70_fuzai.Reserve3),
                                        SqlConnectionHelper.CreateParameter("@reserve4", returnData_P70_fuzai.Reserve4),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P70_fuzai.ProductBarCode),
                                    };
                            helper.ExecuteNonQuery(sql, parametersInsert);

                            string saveObjJson = ObjectJson.sObjectJson.Serialize(returnData_P70_fuzai);
                            ObjectJson.sObjectJson.TryDeserialize<P70DB3001UploadData2>(saveObjJson, out P70DB3001UploadData2 resObj);
                            resObj.IsNG = false;
                            resObj.MESCanRead = false;
                            DateTime.TryParse("1700-01-01 00:00:00", out DateTime dateTimeRes);
                            resObj.CreateTime = dateTimeRes;
                            string saveMd5String = MD5Helper.sMD5Helper.GetMd5InString(ObjectJson.sObjectJson.Serialize(resObj));

                            bool insertNg = true;
                            if (returnData_P70_fuzai.IsNG)
                            {
                                string sqlDelA = "delete from signal_data2_Detail where MD5String='" + saveMd5String + "'";
                                helper.ExecuteNonQuery(sqlDelA);
                            }
                            else
                            {
                                using (var helperQuery = new SqlConnectionHelper())
                                {
                                    string sqlSelect = "SELECT * FROM signal_data2_Detail where  MD5String='" + saveMd5String + "'";
                                    DataTable dtWorkOrder = helperQuery.ExecuteDataTable(sqlSelect);
                                    if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                                    {
                                        insertNg = false;
                                    }
                                }
                            }
                            if (insertNg)
                            {
                                string sqlDet = "insert into signal_data2_Detail(" +
                               "force_current_500n," +
                               "force_current_600n," +
                               "max_current," +
                               "max_voltage," +
                               "max_torque," +
                               "max_pressure," +
                               "reserve1," +
                               "reserve2," +
                               "reserve3," +
                               "reserve4," +
                               "CreatedTime," +
                               "Product_BarCode," +
                               "Md5String," +
                               "IsNG)" +
                                   "values(" +
                                   "@force_current_500n," +
                                   "@force_current_600n," +
                                   "@max_current," +
                                   "@max_voltage," +
                                   "@max_torque," +
                                   "@max_pressure," +
                                   "@reserve1," +
                                   "@reserve2," +
                                   "@reserve3," +
                                   "@reserve4," +
                                   "@CreatedTime," +
                                   "@Product_BarCode,@Md5String,@IsNG)";
                                var parametersInsertDet = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@force_current_500n", returnData_P70_fuzai.ForceCurrent500n),
                                        SqlConnectionHelper.CreateParameter("@force_current_600n", returnData_P70_fuzai.ForceCurrent600n),
                                        SqlConnectionHelper.CreateParameter("@max_current", returnData_P70_fuzai.MaxCurrent),
                                        SqlConnectionHelper.CreateParameter("@max_voltage", returnData_P70_fuzai.MaxVoltage),
                                        SqlConnectionHelper.CreateParameter("@max_torque", returnData_P70_fuzai.MaxTorque),
                                        SqlConnectionHelper.CreateParameter("@max_pressure", returnData_P70_fuzai.MaxPressure),
                                        SqlConnectionHelper.CreateParameter("@reserve1", returnData_P70_fuzai.Reserve1),
                                        SqlConnectionHelper.CreateParameter("@reserve2", returnData_P70_fuzai.Reserve2),
                                        SqlConnectionHelper.CreateParameter("@reserve3", returnData_P70_fuzai.Reserve3),
                                        SqlConnectionHelper.CreateParameter("@reserve4", returnData_P70_fuzai.Reserve4),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P70_fuzai.ProductBarCode),
                                        SqlConnectionHelper.CreateParameter("@Md5String", saveMd5String),
                                        SqlConnectionHelper.CreateParameter("@IsNG", returnData_P70_fuzai.IsNG?"NG":"OK")
                                    };
                                helper.ExecuteNonQuery(sqlDet, parametersInsertDet);
                            }


                            helper.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            helper.RollbackTransaction();
                        }
                    }
                }
                if (p70_biaoding_ReturnDatas.TryDequeue(out P70DB3001UploadData returnData_P70_biaoding))
                {
                    using (var helper = new SqlConnectionHelper())
                    {
                        try
                        {
                            helper.BeginTransaction();
                            string sqlDel = "delete from signal_data where Product_BarCode='" + returnData_P70_biaoding.ProductBarCode + "'";
                            helper.ExecuteNonQuery(sqlDel);

                            string sql = "insert into signal_data(" +
                                "a_slot," +
                                "b_slot," +
                                "c_slot," +
                                "amplitude_a," +
                                "amplitude_b," +
                                "amplitude_c," +
                                "mag_low_a," +
                                "mag_low_b," +
                                "mag_low_c," +
                                "a_last_point," +
                                "a_second_last_point," +
                                "b_last_point," +
                                "b_second_last_point," +
                                "c_last_point," +
                                "c_second_last_point," +
                                "a_first_point," +
                                "a_second_point," +
                                "b_first_point," +
                                "b_second_point," +
                                "c_first_point," +
                                "c_second_point," +
                                "induction_distance," +
                                "actual_distance," +
                                "outermost_distance," +
                                "innermost_distance," +
                                "CreatedTime," +
                                "Product_BarCode)" +
                                    "values(" +
                                    "@a_slot," +
                                    "@b_slot," +
                                    "@c_slot," +
                                    "@amplitude_a," +
                                    "@amplitude_b," +
                                    "@amplitude_c," +
                                    "@mag_low_a," +
                                    "@mag_low_b," +
                                    "@mag_low_c," +
                                    "@a_last_point," +
                                    "@a_second_last_point," +
                                    "@b_last_point," +
                                    "@b_second_last_point," +
                                    "@c_last_point," +
                                    "@c_second_last_point," +
                                    "@a_first_point," +
                                    "@a_second_point," +
                                    "@b_first_point," +
                                    "@b_second_point," +
                                    "@c_first_point," +
                                    "@c_second_point," +
                                    "@induction_distance," +
                                    "@actual_distance," +
                                    "@outermost_distance," +
                                    "@innermost_distance," +
                                    "@CreatedTime," +
                                    "@Product_BarCode)";
                            var parametersInsert = new[]
                                {
                                        SqlConnectionHelper.CreateParameter("@a_slot", returnData_P70_biaoding.ASLOT),
                                        SqlConnectionHelper.CreateParameter("@b_slot", returnData_P70_biaoding.BSLOT),
                                        SqlConnectionHelper.CreateParameter("@c_slot", returnData_P70_biaoding.CSLOT),
                                        SqlConnectionHelper.CreateParameter("@amplitude_a", returnData_P70_biaoding.AmplitudeValueA),
                                        SqlConnectionHelper.CreateParameter("@amplitude_b", returnData_P70_biaoding.AmplitudeValueB),
                                        SqlConnectionHelper.CreateParameter("@amplitude_c", returnData_P70_biaoding.AmplitudeValueC),
                                        SqlConnectionHelper.CreateParameter("@mag_low_a", returnData_P70_biaoding.MagLowValueA),
                                        SqlConnectionHelper.CreateParameter("@mag_low_b", returnData_P70_biaoding.MagLowValueB),
                                        SqlConnectionHelper.CreateParameter("@mag_low_c", returnData_P70_biaoding.MagLowValueC),
                                        SqlConnectionHelper.CreateParameter("@a_last_point", returnData_P70_biaoding.APenultimatePoint),
                                        SqlConnectionHelper.CreateParameter("@a_second_last_point", returnData_P70_biaoding.AThirdLastPoint),
                                        SqlConnectionHelper.CreateParameter("@b_last_point", returnData_P70_biaoding.BPenultimatePoint),
                                        SqlConnectionHelper.CreateParameter("@b_second_last_point", returnData_P70_biaoding.BThirdLastPoint),
                                        SqlConnectionHelper.CreateParameter("@c_last_point", returnData_P70_biaoding.CPenultimatePoint),
                                        SqlConnectionHelper.CreateParameter("@c_second_last_point", returnData_P70_biaoding.CThirdLastPoint),
                                        SqlConnectionHelper.CreateParameter("@a_first_point", returnData_P70_biaoding.AFirstPoint),
                                        SqlConnectionHelper.CreateParameter("@a_second_point", returnData_P70_biaoding.ASecondPoint),
                                        SqlConnectionHelper.CreateParameter("@b_first_point", returnData_P70_biaoding.BFirstPoint),
                                        SqlConnectionHelper.CreateParameter("@b_second_point", returnData_P70_biaoding.BSecondPoint),
                                        SqlConnectionHelper.CreateParameter("@c_first_point", returnData_P70_biaoding.CFirstPoint),
                                        SqlConnectionHelper.CreateParameter("@c_second_point", returnData_P70_biaoding.CSecondPoint),
                                        SqlConnectionHelper.CreateParameter("@induction_distance", returnData_P70_biaoding.SensingDistance),
                                        SqlConnectionHelper.CreateParameter("@actual_distance", returnData_P70_biaoding.ActualDistance),
                                        SqlConnectionHelper.CreateParameter("@outermost_distance", returnData_P70_biaoding.OutermostDistance),
                                        SqlConnectionHelper.CreateParameter("@innermost_distance", returnData_P70_biaoding.InnermostDistance),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P70_biaoding.ProductBarCode),
                                    };
                            helper.ExecuteNonQuery(sql, parametersInsert);

                            string saveObjJson = ObjectJson.sObjectJson.Serialize(returnData_P70_biaoding);
                            ObjectJson.sObjectJson.TryDeserialize<P70DB3001UploadData>(saveObjJson, out P70DB3001UploadData resObj);
                            resObj.IsNG = false;
                            resObj.MESCanRead = false;
                            string saveMd5String = MD5Helper.sMD5Helper.GetMd5InString(ObjectJson.sObjectJson.Serialize(resObj));

                            bool insertNg = true;
                            if (returnData_P70_biaoding.IsNG)
                            {
                                string sqlDelA = "delete from signal_data_Detail where MD5String='" + saveMd5String + "'";
                                helper.ExecuteNonQuery(sqlDelA);
                            }
                            else
                            {
                                using (var helperQuery = new SqlConnectionHelper())
                                {
                                    string sqlSelect = "SELECT * FROM signal_data_Detail where  MD5String='" + saveMd5String + "'";
                                    DataTable dtWorkOrder = helperQuery.ExecuteDataTable(sqlSelect);
                                    if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                                    {
                                        insertNg = false;
                                    }
                                }
                            }
                            if (insertNg)
                            {
                                string sqlDet = "insert into signal_data_Detail(" +
                                "a_slot," +
                                "b_slot," +
                                "c_slot," +
                                "amplitude_a," +
                                "amplitude_b," +
                                "amplitude_c," +
                                "mag_low_a," +
                                "mag_low_b," +
                                "mag_low_c," +
                                "a_last_point," +
                                "a_second_last_point," +
                                "b_last_point," +
                                "b_second_last_point," +
                                "c_last_point," +
                                "c_second_last_point," +
                                "a_first_point," +
                                "a_second_point," +
                                "b_first_point," +
                                "b_second_point," +
                                "c_first_point," +
                                "c_second_point," +
                                "induction_distance," +
                                "actual_distance," +
                                "outermost_distance," +
                                "innermost_distance," +
                                "CreatedTime," +
                                "Product_BarCode,Md5String,IsNG)" +
                                    "values(" +
                                    "@a_slot," +
                                    "@b_slot," +
                                    "@c_slot," +
                                    "@amplitude_a," +
                                    "@amplitude_b," +
                                    "@amplitude_c," +
                                    "@mag_low_a," +
                                    "@mag_low_b," +
                                    "@mag_low_c," +
                                    "@a_last_point," +
                                    "@a_second_last_point," +
                                    "@b_last_point," +
                                    "@b_second_last_point," +
                                    "@c_last_point," +
                                    "@c_second_last_point," +
                                    "@a_first_point," +
                                    "@a_second_point," +
                                    "@b_first_point," +
                                    "@b_second_point," +
                                    "@c_first_point," +
                                    "@c_second_point," +
                                    "@induction_distance," +
                                    "@actual_distance," +
                                    "@outermost_distance," +
                                    "@innermost_distance," +
                                    "@CreatedTime," +
                                    "@Product_BarCode,@Md5String,@IsNG)";
                                var parametersInsertDet = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@a_slot", returnData_P70_biaoding.ASLOT),
                                        SqlConnectionHelper.CreateParameter("@b_slot", returnData_P70_biaoding.BSLOT),
                                        SqlConnectionHelper.CreateParameter("@c_slot", returnData_P70_biaoding.CSLOT),
                                        SqlConnectionHelper.CreateParameter("@amplitude_a", returnData_P70_biaoding.AmplitudeValueA),
                                        SqlConnectionHelper.CreateParameter("@amplitude_b", returnData_P70_biaoding.AmplitudeValueB),
                                        SqlConnectionHelper.CreateParameter("@amplitude_c", returnData_P70_biaoding.AmplitudeValueC),
                                        SqlConnectionHelper.CreateParameter("@mag_low_a", returnData_P70_biaoding.MagLowValueA),
                                        SqlConnectionHelper.CreateParameter("@mag_low_b", returnData_P70_biaoding.MagLowValueB),
                                        SqlConnectionHelper.CreateParameter("@mag_low_c", returnData_P70_biaoding.MagLowValueC),
                                        SqlConnectionHelper.CreateParameter("@a_last_point", returnData_P70_biaoding.APenultimatePoint),
                                        SqlConnectionHelper.CreateParameter("@a_second_last_point", returnData_P70_biaoding.AThirdLastPoint),
                                        SqlConnectionHelper.CreateParameter("@b_last_point", returnData_P70_biaoding.BPenultimatePoint),
                                        SqlConnectionHelper.CreateParameter("@b_second_last_point", returnData_P70_biaoding.BThirdLastPoint),
                                        SqlConnectionHelper.CreateParameter("@c_last_point", returnData_P70_biaoding.CPenultimatePoint),
                                        SqlConnectionHelper.CreateParameter("@c_second_last_point", returnData_P70_biaoding.CThirdLastPoint),
                                        SqlConnectionHelper.CreateParameter("@a_first_point", returnData_P70_biaoding.AFirstPoint),
                                        SqlConnectionHelper.CreateParameter("@a_second_point", returnData_P70_biaoding.ASecondPoint),
                                        SqlConnectionHelper.CreateParameter("@b_first_point", returnData_P70_biaoding.BFirstPoint),
                                        SqlConnectionHelper.CreateParameter("@b_second_point", returnData_P70_biaoding.BSecondPoint),
                                        SqlConnectionHelper.CreateParameter("@c_first_point", returnData_P70_biaoding.CFirstPoint),
                                        SqlConnectionHelper.CreateParameter("@c_second_point", returnData_P70_biaoding.CSecondPoint),
                                        SqlConnectionHelper.CreateParameter("@induction_distance", returnData_P70_biaoding.SensingDistance),
                                        SqlConnectionHelper.CreateParameter("@actual_distance", returnData_P70_biaoding.ActualDistance),
                                        SqlConnectionHelper.CreateParameter("@outermost_distance", returnData_P70_biaoding.OutermostDistance),
                                        SqlConnectionHelper.CreateParameter("@innermost_distance", returnData_P70_biaoding.InnermostDistance),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P70_biaoding.ProductBarCode),
                                        SqlConnectionHelper.CreateParameter("@Md5String", saveMd5String),
                                        SqlConnectionHelper.CreateParameter("@IsNG", returnData_P70_biaoding.IsNG?"NG":"OK")
                                    };
                                helper.ExecuteNonQuery(sqlDet, parametersInsertDet);
                            }


                            helper.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            helper.RollbackTransaction();
                        }
                    }
                }
                if (p80DB2000ReturnDatas.TryDequeue(out P80DB3004ReturnData returnData_P80))
                {
                    using (var helper = new SqlConnectionHelper())
                    {
                        try
                        {
                            helper.BeginTransaction();
                            string sqlDel = "delete from P80DB3004ReturnData where Product_BarCode='" + returnData_P80.Product_BarCode + "'";
                            helper.ExecuteNonQuery(sqlDel);

                            string sql = "insert into P80DB3004ReturnData(ReadFlag," +
                                "SoundPressure31Hz," +
                                "SoundPressure63Hz," +
                                "SoundPressure125Hz," +
                                "SoundPressure250Hz," +
                                "SoundPressure500Hz," +
                                "SoundPressure1000Hz," +
                                "SoundPressure2000Hz," +
                                "SoundPressure4000Hz," +
                                "SoundPressure8000Hz," +
                                "SoundPressure16000Hz," +
                                "SoundPressureAPC," +
                                "SoundPressureAPA," +
                                "SoundPressureAPLIN," +
                                "CreatedTime," +
                                "Product_BarCode)" +
                                    "values(@ReadFlag," +
                                    "@SoundPressure31Hz," +
                                    "@SoundPressure63Hz," +
                                    "@SoundPressure125Hz," +
                                    "@SoundPressure250Hz," +
                                    "@SoundPressure500Hz," +
                                    "@SoundPressure1000Hz," +
                                    "@SoundPressure2000Hz," +
                                    "@SoundPressure4000Hz," +
                                    "@SoundPressure8000Hz," +
                                    "@SoundPressure16000Hz," +
                                    "@SoundPressureAPC," +
                                    "@SoundPressureAPA," +
                                    "@SoundPressureAPLIN," +
                                    "@CreatedTime," +
                                    "@Product_BarCode)";
                            var parametersInsert = new[]
                                {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", returnData_P80.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure31Hz", returnData_P80.SoundPressure31Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure63Hz", returnData_P80.SoundPressure63Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure125Hz", returnData_P80.SoundPressure125Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure250Hz", returnData_P80.SoundPressure250Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure500Hz", returnData_P80.SoundPressure500Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure1000Hz", returnData_P80.SoundPressure1000Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure2000Hz", returnData_P80.SoundPressure2000Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure4000Hz", returnData_P80.SoundPressure4000Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure8000Hz", returnData_P80.SoundPressure8000Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure16000Hz", returnData_P80.SoundPressure16000Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressureAPC", returnData_P80.SoundPressureAPC),
                                        SqlConnectionHelper.CreateParameter("@SoundPressureAPA", returnData_P80.SoundPressureAPA),
                                        SqlConnectionHelper.CreateParameter("@SoundPressureAPLIN", returnData_P80.SoundPressureAPLIN),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P80.Product_BarCode),
                                    };
                            helper.ExecuteNonQuery(sql, parametersInsert);


                            string saveObjJson = ObjectJson.sObjectJson.Serialize(returnData_P80);
                            ObjectJson.sObjectJson.TryDeserialize<P80DB3004ReturnData>(saveObjJson, out P80DB3004ReturnData resObj);
                            resObj.ReadFlag = false;
                            resObj.IsNG = false;
                            string saveMd5String = MD5Helper.sMD5Helper.GetMd5InString(ObjectJson.sObjectJson.Serialize(resObj));

                            bool insertNg = true;
                            if (returnData_P80.IsNG)
                            {
                                string sqlDelA = "delete from P80DB3004ReturnData_Detail where MD5String='" + saveMd5String + "'";
                                helper.ExecuteNonQuery(sqlDelA);
                            }
                            else
                            {
                                using (var helperQuery = new SqlConnectionHelper())
                                {
                                    string sqlSelect = "SELECT * FROM P80DB3004ReturnData_Detail where  MD5String='" + saveMd5String + "'";
                                    DataTable dtWorkOrder = helperQuery.ExecuteDataTable(sqlSelect);
                                    if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                                    {
                                        insertNg = false;
                                    }
                                }
                            }
                            if (insertNg)
                            {
                                string sqlDet = "insert into P80DB3004ReturnData_Detail(ReadFlag," +
                               "SoundPressure31Hz," +
                               "SoundPressure63Hz," +
                               "SoundPressure125Hz," +
                               "SoundPressure250Hz," +
                               "SoundPressure500Hz," +
                               "SoundPressure1000Hz," +
                               "SoundPressure2000Hz," +
                               "SoundPressure4000Hz," +
                               "SoundPressure8000Hz," +
                               "SoundPressure16000Hz," +
                               "SoundPressureAPC," +
                               "SoundPressureAPA," +
                               "SoundPressureAPLIN," +
                               "CreatedTime," +
                               "Product_BarCode,Md5String,IsNG)" +
                                   "values(@ReadFlag," +
                                   "@SoundPressure31Hz," +
                                   "@SoundPressure63Hz," +
                                   "@SoundPressure125Hz," +
                                   "@SoundPressure250Hz," +
                                   "@SoundPressure500Hz," +
                                   "@SoundPressure1000Hz," +
                                   "@SoundPressure2000Hz," +
                                   "@SoundPressure4000Hz," +
                                   "@SoundPressure8000Hz," +
                                   "@SoundPressure16000Hz," +
                                   "@SoundPressureAPC," +
                                   "@SoundPressureAPA," +
                                   "@SoundPressureAPLIN," +
                                   "@CreatedTime," +
                                   "@Product_BarCode,@Md5String,@IsNG)";
                                var parametersInsertDet = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", returnData_P80.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure31Hz", returnData_P80.SoundPressure31Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure63Hz", returnData_P80.SoundPressure63Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure125Hz", returnData_P80.SoundPressure125Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure250Hz", returnData_P80.SoundPressure250Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure500Hz", returnData_P80.SoundPressure500Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure1000Hz", returnData_P80.SoundPressure1000Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure2000Hz", returnData_P80.SoundPressure2000Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure4000Hz", returnData_P80.SoundPressure4000Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure8000Hz", returnData_P80.SoundPressure8000Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressure16000Hz", returnData_P80.SoundPressure16000Hz),
                                        SqlConnectionHelper.CreateParameter("@SoundPressureAPC", returnData_P80.SoundPressureAPC),
                                        SqlConnectionHelper.CreateParameter("@SoundPressureAPA", returnData_P80.SoundPressureAPA),
                                        SqlConnectionHelper.CreateParameter("@SoundPressureAPLIN", returnData_P80.SoundPressureAPLIN),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P80.Product_BarCode),
                                        SqlConnectionHelper.CreateParameter("@IsNG", returnData_P80.IsNG?"NG":"OK"),
                                        SqlConnectionHelper.CreateParameter("@Md5String", saveMd5String)
                                    };
                                helper.ExecuteNonQuery(sqlDet, parametersInsertDet);
                            }



                            helper.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            helper.RollbackTransaction();
                        }
                    }
                }
                if (p70DB2000ReturnDatas.TryDequeue(out P70DB3003ReturnData returnData_P70))
                {
                    using (var helper = new SqlConnectionHelper())
                    {
                        try
                        {
                            helper.BeginTransaction();
                            string sqlDel = "delete from P70DB3003ReturnData where Product_BarCode='" + returnData_P70.Product_BarCode + "'";
                            helper.ExecuteNonQuery(sqlDel);

                            string sql = "insert into P70DB3003ReturnData(ReadFlag1," +
                                "CalibrationResult1," +
                                "MaxLength1," +
                                "AmplitudeValue1," +
                                "MagLowValue1," +
                                "ReadFlag2," +
                                "RightAgingMaxVoltage2," +
                                "RightAgingMinVoltage2," +
                                "RightAgingAvgVoltage2," +
                                "RightAgingMaxCurrent2," +
                                "RightAgingMinCurrent2," +
                                "RightAgingAvgCurrent2," +
                                "ReadFlag3," +
                                "MiddleAgingMaxVoltage3," +
                                "MiddleAgingMinVoltage3," +
                                "MiddleAgingAvgVoltage3," +
                                "MiddleAgingMaxCurrent3," +
                                "MiddleAgingMinCurrent3," +
                                "MiddleAgingAvgCurrent3," +
                                "ReadFlag4," +
                                "LeftAgingMaxVoltage4," +
                                "LeftAgingMinVoltage4," +
                                "LeftAgingAvgVoltage4," +
                                "LeftAgingMaxCurrent4," +
                                "LeftAgingMinCurrent4," +
                                "LeftAgingAvgCurrent4," +
                                "CreatedTime," +
                                "Product_BarCode)" +
                                    "values(@ReadFlag1," +
                                    "@CalibrationResult1," +
                                    "@MaxLength1," +
                                    "@AmplitudeValue1," +
                                    "@MagLowValue1," +
                                    "@ReadFlag2," +
                                    "@RightAgingMaxVoltage2," +
                                    "@RightAgingMinVoltage2," +
                                    "@RightAgingAvgVoltage2," +
                                    "@RightAgingMaxCurrent2," +
                                    "@RightAgingMinCurrent2," +
                                    "@RightAgingAvgCurrent2," +
                                    "@ReadFlag3," +
                                    "@MiddleAgingMaxVoltage3," +
                                    "@MiddleAgingMinVoltage3," +
                                    "@MiddleAgingAvgVoltage3," +
                                    "@MiddleAgingMaxCurrent3," +
                                    "@MiddleAgingMinCurrent3," +
                                    "@MiddleAgingAvgCurrent3," +
                                    "@ReadFlag4," +
                                    "@LeftAgingMaxVoltage4," +
                                    "@LeftAgingMinVoltage4," +
                                    "@LeftAgingAvgVoltage4," +
                                    "@LeftAgingMaxCurrent4," +
                                    "@LeftAgingMinCurrent4," +
                                    "@LeftAgingAvgCurrent4," +
                                    "@CreatedTime," +
                                    "@Product_BarCode)";
                            var parametersInsert = new[]
                                {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag1", returnData_P70.ReadFlag1),
                                        SqlConnectionHelper.CreateParameter("@CalibrationResult1", returnData_P70.CalibrationResult1),
                                        SqlConnectionHelper.CreateParameter("@MaxLength1", returnData_P70.MaxLength1),
                                        SqlConnectionHelper.CreateParameter("@AmplitudeValue1", returnData_P70.AmplitudeValue1),
                                        SqlConnectionHelper.CreateParameter("@MagLowValue1", returnData_P70.MagLowValue1),
                                        SqlConnectionHelper.CreateParameter("@ReadFlag2", returnData_P70.ReadFlag2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingMaxVoltage2", returnData_P70.RightAgingMaxVoltage2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingMinVoltage2", returnData_P70.RightAgingMinVoltage2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingAvgVoltage2", returnData_P70.RightAgingAvgVoltage2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingMaxCurrent2", returnData_P70.RightAgingMaxCurrent2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingMinCurrent2", returnData_P70.RightAgingMinCurrent2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingAvgCurrent2", returnData_P70.RightAgingAvgCurrent2),
                                        SqlConnectionHelper.CreateParameter("@ReadFlag3", returnData_P70.ReadFlag3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingMaxVoltage3", returnData_P70.MiddleAgingMaxVoltage3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingMinVoltage3", returnData_P70.MiddleAgingMinVoltage3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingAvgVoltage3", returnData_P70.MiddleAgingAvgVoltage3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingMaxCurrent3", returnData_P70.MiddleAgingMaxCurrent3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingMinCurrent3", returnData_P70.MiddleAgingMinCurrent3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingAvgCurrent3", returnData_P70.MiddleAgingAvgCurrent3),
                                        SqlConnectionHelper.CreateParameter("@ReadFlag4", returnData_P70.ReadFlag4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingMaxVoltage4", returnData_P70.LeftAgingMaxVoltage4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingMinVoltage4", returnData_P70.LeftAgingMinVoltage4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingAvgVoltage4", returnData_P70.LeftAgingAvgVoltage4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingMaxCurrent4", returnData_P70.LeftAgingMaxCurrent4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingMinCurrent4", returnData_P70.LeftAgingMinCurrent4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingAvgCurrent4", returnData_P70.LeftAgingAvgCurrent4),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P70.Product_BarCode),
                                    };
                            helper.ExecuteNonQuery(sql, parametersInsert);




                            string saveObjJson = ObjectJson.sObjectJson.Serialize(returnData_P70);
                            ObjectJson.sObjectJson.TryDeserialize<P70DB3003ReturnData>(saveObjJson, out P70DB3003ReturnData resObj);
                            resObj.IsNG = false;
                            resObj.ReadFlag1 = false;
                            resObj.ReadFlag2 = false;
                            resObj.ReadFlag3 = false;
                            resObj.ReadFlag4 = false;
                            resObj.CalibrationResult1 = false;
                            string saveMd5String = MD5Helper.sMD5Helper.GetMd5InString(ObjectJson.sObjectJson.Serialize(resObj));

                            bool insertNg = true;
                            if (returnData_P70.IsNG)
                            {
                                string sqlDelA = "delete from P70DB3003ReturnData_Detail where MD5String='" + saveMd5String + "'";
                                helper.ExecuteNonQuery(sqlDelA);
                            }
                            else
                            {
                                using (var helperQuery = new SqlConnectionHelper())
                                {
                                    string sqlSelect = "SELECT * FROM P70DB3003ReturnData_Detail where  MD5String='" + saveMd5String + "'";
                                    DataTable dtWorkOrder = helperQuery.ExecuteDataTable(sqlSelect);
                                    if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                                    {
                                        insertNg = false;
                                    }
                                }
                            }
                            if (insertNg)
                            {
                                string sqlDet = "insert into P70DB3003ReturnData_Detail(ReadFlag1," +
                                "CalibrationResult1," +
                                "MaxLength1," +
                                "AmplitudeValue1," +
                                "MagLowValue1," +
                                "ReadFlag2," +
                                "RightAgingMaxVoltage2," +
                                "RightAgingMinVoltage2," +
                                "RightAgingAvgVoltage2," +
                                "RightAgingMaxCurrent2," +
                                "RightAgingMinCurrent2," +
                                "RightAgingAvgCurrent2," +
                                "ReadFlag3," +
                                "MiddleAgingMaxVoltage3," +
                                "MiddleAgingMinVoltage3," +
                                "MiddleAgingAvgVoltage3," +
                                "MiddleAgingMaxCurrent3," +
                                "MiddleAgingMinCurrent3," +
                                "MiddleAgingAvgCurrent3," +
                                "ReadFlag4," +
                                "LeftAgingMaxVoltage4," +
                                "LeftAgingMinVoltage4," +
                                "LeftAgingAvgVoltage4," +
                                "LeftAgingMaxCurrent4," +
                                "LeftAgingMinCurrent4," +
                                "LeftAgingAvgCurrent4," +
                                "CreatedTime," +
                                "Product_BarCode," +
                                "Md5String," +
                                "IsNG)" +
                                    "values(@ReadFlag1," +
                                    "@CalibrationResult1," +
                                    "@MaxLength1," +
                                    "@AmplitudeValue1," +
                                    "@MagLowValue1," +
                                    "@ReadFlag2," +
                                    "@RightAgingMaxVoltage2," +
                                    "@RightAgingMinVoltage2," +
                                    "@RightAgingAvgVoltage2," +
                                    "@RightAgingMaxCurrent2," +
                                    "@RightAgingMinCurrent2," +
                                    "@RightAgingAvgCurrent2," +
                                    "@ReadFlag3," +
                                    "@MiddleAgingMaxVoltage3," +
                                    "@MiddleAgingMinVoltage3," +
                                    "@MiddleAgingAvgVoltage3," +
                                    "@MiddleAgingMaxCurrent3," +
                                    "@MiddleAgingMinCurrent3," +
                                    "@MiddleAgingAvgCurrent3," +
                                    "@ReadFlag4," +
                                    "@LeftAgingMaxVoltage4," +
                                    "@LeftAgingMinVoltage4," +
                                    "@LeftAgingAvgVoltage4," +
                                    "@LeftAgingMaxCurrent4," +
                                    "@LeftAgingMinCurrent4," +
                                    "@LeftAgingAvgCurrent4," +
                                    "@CreatedTime," +
                                    "@Product_BarCode," +
                                    "@Md5String," +
                                    "@IsNG)";
                                var parametersInsertDet = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag1", returnData_P70.ReadFlag1),
                                        SqlConnectionHelper.CreateParameter("@CalibrationResult1", returnData_P70.CalibrationResult1),
                                        SqlConnectionHelper.CreateParameter("@MaxLength1", returnData_P70.MaxLength1),
                                        SqlConnectionHelper.CreateParameter("@AmplitudeValue1", returnData_P70.AmplitudeValue1),
                                        SqlConnectionHelper.CreateParameter("@MagLowValue1", returnData_P70.MagLowValue1),
                                        SqlConnectionHelper.CreateParameter("@ReadFlag2", returnData_P70.ReadFlag2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingMaxVoltage2", returnData_P70.RightAgingMaxVoltage2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingMinVoltage2", returnData_P70.RightAgingMinVoltage2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingAvgVoltage2", returnData_P70.RightAgingAvgVoltage2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingMaxCurrent2", returnData_P70.RightAgingMaxCurrent2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingMinCurrent2", returnData_P70.RightAgingMinCurrent2),
                                        SqlConnectionHelper.CreateParameter("@RightAgingAvgCurrent2", returnData_P70.RightAgingAvgCurrent2),
                                        SqlConnectionHelper.CreateParameter("@ReadFlag3", returnData_P70.ReadFlag3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingMaxVoltage3", returnData_P70.MiddleAgingMaxVoltage3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingMinVoltage3", returnData_P70.MiddleAgingMinVoltage3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingAvgVoltage3", returnData_P70.MiddleAgingAvgVoltage3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingMaxCurrent3", returnData_P70.MiddleAgingMaxCurrent3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingMinCurrent3", returnData_P70.MiddleAgingMinCurrent3),
                                        SqlConnectionHelper.CreateParameter("@MiddleAgingAvgCurrent3", returnData_P70.MiddleAgingAvgCurrent3),
                                        SqlConnectionHelper.CreateParameter("@ReadFlag4", returnData_P70.ReadFlag4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingMaxVoltage4", returnData_P70.LeftAgingMaxVoltage4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingMinVoltage4", returnData_P70.LeftAgingMinVoltage4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingAvgVoltage4", returnData_P70.LeftAgingAvgVoltage4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingMaxCurrent4", returnData_P70.LeftAgingMaxCurrent4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingMinCurrent4", returnData_P70.LeftAgingMinCurrent4),
                                        SqlConnectionHelper.CreateParameter("@LeftAgingAvgCurrent4", returnData_P70.LeftAgingAvgCurrent4),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P70.Product_BarCode),
                                        SqlConnectionHelper.CreateParameter("@Md5String",saveMd5String),
                                        SqlConnectionHelper.CreateParameter("@IsNG", returnData_P70.IsNG?"NG":"OK")
                                    };
                                helper.ExecuteNonQuery(sqlDet, parametersInsertDet);
                            }



                            helper.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            helper.RollbackTransaction();
                        }
                    }
                }
                if (p60DB2000ReturnDatas.TryDequeue(out P60DB3000ReturnData returnData_P60))
                {
                    if (!string.IsNullOrEmpty(returnData_P60.Product_BarCode) && returnData_P60.ReadFlag)
                    {
                        using (var helper = new SqlConnectionHelper())
                        {
                            try
                            {
                                helper.BeginTransaction();
                                string sqlDel = "delete from P60DB3000ReturnData where Product_BarCode='" + returnData_P60.Product_BarCode + "'";
                                helper.ExecuteNonQuery(sqlDel);

                                string sql = "insert into P60DB3000ReturnData(ReadFlag,TimeDate," +
                                    "LeakRate," +
                                    "CreatedTime," +
                                    "Product_BarCode)" +
                                        "values(@ReadFlag,@TimeDate," +
                                        "@LeakRate," +
                                        "@CreatedTime," +
                                        "@Product_BarCode)";
                                var parametersInsert = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", returnData_P60.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@TimeDate", returnData_P60.TimeDate),
                                        SqlConnectionHelper.CreateParameter("@LeakRate", returnData_P60.LeakRate),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P60.Product_BarCode),
                                    };
                                helper.ExecuteNonQuery(sql, parametersInsert);



                                string saveObjJson = ObjectJson.sObjectJson.Serialize(returnData_P60);
                                ObjectJson.sObjectJson.TryDeserialize<P60DB3000ReturnData>(saveObjJson, out P60DB3000ReturnData resObj);
                                resObj.IsNG = false;
                                resObj.ReadFlag = false;
                                string saveMd5String = MD5Helper.sMD5Helper.GetMd5InString(ObjectJson.sObjectJson.Serialize(resObj));
                                

                                bool insertNg = true;
                                if (returnData_P60.IsNG)
                                {
                                    string sqlDelA = "delete from P60DB3000ReturnData_Detail where MD5String='" + saveMd5String + "'";
                                    helper.ExecuteNonQuery(sqlDelA);
                                }
                                else
                                {
                                    using (var helperQuery = new SqlConnectionHelper())
                                    {
                                        string sqlSelect = "SELECT * FROM P60DB3000ReturnData_Detail where  MD5String='" + saveMd5String + "'";
                                        DataTable dtWorkOrder = helperQuery.ExecuteDataTable(sqlSelect);
                                        if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                                        {
                                            insertNg = false;
                                        }
                                    }
                                }
                                if (insertNg)
                                {
                                    string sqlDe = "insert into P60DB3000ReturnData_Detail(ReadFlag,TimeDate," +
                                   "LeakRate," +
                                   "CreatedTime," +
                                   "Product_BarCode," +
                                   "IsNG,Md5String)" +
                                       "values(@ReadFlag,@TimeDate," +
                                       "@LeakRate," +
                                       "@CreatedTime," +
                                       "@Product_BarCode,@IsNG,@Md5String)";
                                    var parametersInsertDe = new[]
                                        {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", returnData_P60.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@TimeDate", returnData_P60.TimeDate),
                                        SqlConnectionHelper.CreateParameter("@LeakRate", returnData_P60.LeakRate),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P60.Product_BarCode),
                                        SqlConnectionHelper.CreateParameter("@IsNG", returnData_P60.IsNG?"NG":"OK"),
                                        SqlConnectionHelper.CreateParameter("@Md5String",  saveMd5String)
                                    };
                                    helper.ExecuteNonQuery(sqlDe, parametersInsertDe);
                                } 

                                helper.CommitTransaction();
                            }
                            catch (Exception ex)
                            {
                                helper.RollbackTransaction();
                            }
                        }
                    }
                }
                if (p50DB2000ReturnDatas.TryDequeue(out P50DB3002ReturnData returnData_P50))
                {
                    if (!string.IsNullOrEmpty(returnData_P50.Product_BarCode) && returnData_P50.ReadFlag)
                    {
                        using (var helper = new SqlConnectionHelper())
                        {
                            try
                            {
                                helper.BeginTransaction();
                                string sqlDel = "delete from P50DB3002ReturnData where Product_BarCode='" + returnData_P50.Product_BarCode + "'";
                                helper.ExecuteNonQuery(sqlDel);

                                string sql = "insert into P50DB3002ReturnData(ReadFlag,CompletedTorque1," +
                                    "CompletedAngle1," +
                                    "CompletedTorque2," +
                                    "CompletedAngle2," +
                                    "CompletedTorque3," +
                                    "CompletedAngle3," +
                                    "CompletedTorque4," +
                                    "CompletedAngle4," +
                                     "CompletedTorque5," +
                                    "CompletedAngle5," +
                                     "CompletedTorque6," +
                                    "CompletedAngle6," +
                                     "CompletedTorque7," +
                                    "CompletedAngle7," +
                                    "CompletedTorque8," +
                                    "CompletedAngle8," +
                                    "CompletedTorque9," +
                                    "CompletedAngle9," +
                                    "CreatedTime," +
                                    "Product_BarCode)" +
                                        "values(@ReadFlag,@CompletedTorque1," +
                                        "@CompletedAngle1," +
                                        "@CompletedTorque2," +
                                        "@CompletedAngle2," +
                                        "@CompletedTorque3," +
                                        "@CompletedAngle3," +
                                        "@CompletedTorque4," +
                                        "@CompletedAngle4," +
                                        "@CompletedTorque5," +
                                        "@CompletedAngle5," +
                                        "@CompletedTorque6," +
                                        "@CompletedAngle6," +
                                        "@CompletedTorque7," +
                                        "@CompletedAngle7," +
                                        "@CompletedTorque8," +
                                        "@CompletedAngle8," +
                                        "@CompletedTorque9," +
                                        "@CompletedAngle9," +
                                        "@CreatedTime," +
                                        "@Product_BarCode)";
                                var parametersInsert = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", returnData_P50.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque1", returnData_P50.CompletedTorque1),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle1", returnData_P50.CompletedAngle1),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque2", returnData_P50.CompletedTorque2),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle2", returnData_P50.CompletedAngle2),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque3", returnData_P50.CompletedTorque3),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle3", returnData_P50.CompletedAngle3),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque4", returnData_P50.CompletedTorque4),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle4", returnData_P50.CompletedAngle4),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque5", returnData_P50.CompletedTorque5),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle5", returnData_P50.CompletedAngle5),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque6", returnData_P50.CompletedTorque6),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle6", returnData_P50.CompletedAngle6),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque7", returnData_P50.CompletedTorque7),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle7", returnData_P50.CompletedAngle7),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque8", returnData_P50.CompletedTorque8),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle8", returnData_P50.CompletedAngle8),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque9", returnData_P50.CompletedTorque9),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle9", returnData_P50.CompletedAngle9),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P50.Product_BarCode),
                                    };
                                helper.ExecuteNonQuery(sql, parametersInsert);


                                string saveObjJson = ObjectJson.sObjectJson.Serialize(returnData_P50);
                                ObjectJson.sObjectJson.TryDeserialize<P50DB3002ReturnData>(saveObjJson, out P50DB3002ReturnData resObj);
                                resObj.IsNG = false;
                                resObj.ReadFlag = false;
                                string saveMd5String = MD5Helper.sMD5Helper.GetMd5InString(ObjectJson.sObjectJson.Serialize(resObj));

                                bool insertNg = true;
                                if (returnData_P50.IsNG)
                                {
                                    string sqlDelA = "delete from P50DB3002ReturnData_Detail where MD5String='" + saveMd5String + "'";
                                    helper.ExecuteNonQuery(sqlDelA);
                                }
                                else
                                {
                                    using (var helperQuery = new SqlConnectionHelper())
                                    {
                                        string sqlSelect = "SELECT * FROM P50DB3002ReturnData_Detail where  MD5String='" + saveMd5String + "'";
                                        DataTable dtWorkOrder = helperQuery.ExecuteDataTable(sqlSelect);
                                        if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                                        {
                                            insertNg = false;
                                        }
                                    }
                                }
                                if (insertNg)
                                {
                                    string sql_det = "insert into P50DB3002ReturnData_Detail(ReadFlag,CompletedTorque1," +
                                    "CompletedAngle1," +
                                    "CompletedTorque2," +
                                    "CompletedAngle2," +
                                    "CompletedTorque3," +
                                    "CompletedAngle3," +
                                    "CompletedTorque4," +
                                    "CompletedAngle4," +
                                     "CompletedTorque5," +
                                    "CompletedAngle5," +
                                     "CompletedTorque6," +
                                    "CompletedAngle6," +
                                     "CompletedTorque7," +
                                    "CompletedAngle7," +
                                    "CompletedTorque8," +
                                    "CompletedAngle8," +
                                    "CompletedTorque9," +
                                    "CompletedAngle9," +
                                    "CreatedTime," +
                                    "Product_BarCode," +
                                    "IsNG,Md5String)" +
                                        "values(@ReadFlag,@CompletedTorque1," +
                                        "@CompletedAngle1," +
                                        "@CompletedTorque2," +
                                        "@CompletedAngle2," +
                                        "@CompletedTorque3," +
                                        "@CompletedAngle3," +
                                        "@CompletedTorque4," +
                                        "@CompletedAngle4," +
                                        "@CompletedTorque5," +
                                        "@CompletedAngle5," +
                                        "@CompletedTorque6," +
                                        "@CompletedAngle6," +
                                        "@CompletedTorque7," +
                                        "@CompletedAngle7," +
                                        "@CompletedTorque8," +
                                        "@CompletedAngle8," +
                                        "@CompletedTorque9," +
                                        "@CompletedAngle9," +
                                        "@CreatedTime," +
                                        "@Product_BarCode,@IsNG,@Md5String)";
                                    var parametersInsert_det = new[]
                                        {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", returnData_P50.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque1", returnData_P50.CompletedTorque1),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle1", returnData_P50.CompletedAngle1),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque2", returnData_P50.CompletedTorque2),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle2", returnData_P50.CompletedAngle2),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque3", returnData_P50.CompletedTorque3),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle3", returnData_P50.CompletedAngle3),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque4", returnData_P50.CompletedTorque4),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle4", returnData_P50.CompletedAngle4),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque5", returnData_P50.CompletedTorque5),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle5", returnData_P50.CompletedAngle5),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque6", returnData_P50.CompletedTorque6),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle6", returnData_P50.CompletedAngle6),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque7", returnData_P50.CompletedTorque7),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle7", returnData_P50.CompletedAngle7),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque8", returnData_P50.CompletedTorque8),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle8", returnData_P50.CompletedAngle8),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque9", returnData_P50.CompletedTorque9),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle9", returnData_P50.CompletedAngle9),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P50.Product_BarCode),
                                        SqlConnectionHelper.CreateParameter("@IsNG", returnData_P50.IsNG?"NG":"OK"),
                                        SqlConnectionHelper.CreateParameter("@Md5String",saveMd5String)
                                    };
                                    helper.ExecuteNonQuery(sql_det, parametersInsert_det);
                                }


                                helper.CommitTransaction();
                            }
                            catch (Exception ex)
                            {
                                helper.RollbackTransaction();
                            }
                        }
                    }
                }
                if (p40DB2000ReturnDatas.TryDequeue(out P40DB3001ReturnData returnData_P40))
                {
                    if (!string.IsNullOrEmpty(returnData_P40.Product_BarCode) && returnData_P40.ReadFlag)
                    {
                        using (var helper = new SqlConnectionHelper())
                        {
                            try
                            {
                                helper.BeginTransaction();
                                string sqlDel = "delete from P40DB3001ReturnData where Product_BarCode='" + returnData_P40.Product_BarCode + "'";
                                helper.ExecuteNonQuery(sqlDel);

                                string sql = "insert into P40DB3001ReturnData(ReadFlag,CompletedTorque1," +
                                    "CompletedAngle1," +
                                    "CompletedTorque2," +
                                    "CompletedAngle2," +
                                    "CompletedTorque3," +
                                    "CompletedAngle3," +
                                    "CompletedTorque4," +
                                    "CompletedAngle4," +
                                     "CompletedTorque5," +
                                    "CompletedAngle5," +
                                     "CompletedTorque6," +
                                    "CompletedAngle6," +
                                     "CompletedTorque7," +
                                    "CompletedAngle7," +
                                    "CompletedTorque8," +
                                    "CompletedAngle8," +
                                    "CompletedTorque9," +
                                    "CompletedAngle9," +
                                    "GearHeight1," +
                                    "GearHeight2," +
                                    "CreatedTime," +
                                    "Product_BarCode," +
                                    "MotorCode)" +
                                        "values(@ReadFlag,@CompletedTorque1," +
                                        "@CompletedAngle1," +
                                        "@CompletedTorque2," +
                                        "@CompletedAngle2," +
                                        "@CompletedTorque3," +
                                        "@CompletedAngle3," +
                                        "@CompletedTorque4," +
                                        "@CompletedAngle4," +
                                        "@CompletedTorque5," +
                                        "@CompletedAngle5," +
                                        "@CompletedTorque6," +
                                        "@CompletedAngle6," +
                                        "@CompletedTorque7," +
                                        "@CompletedAngle7," +
                                        "@CompletedTorque8," +
                                        "@CompletedAngle8," +
                                        "@CompletedTorque9," +
                                        "@CompletedAngle9," +
                                        "@GearHeight1," +
                                        "@GearHeight2," +
                                        "@CreatedTime," +
                                        "@Product_BarCode," +
                                        "@MotorCode)";
                                var parametersInsert = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", returnData_P40.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque1", returnData_P40.CompletedTorque1),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle1", returnData_P40.CompletedAngle1),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque2", returnData_P40.CompletedTorque2),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle2", returnData_P40.CompletedAngle2),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque3", returnData_P40.CompletedTorque3),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle3", returnData_P40.CompletedAngle3),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque4", returnData_P40.CompletedTorque4),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle4", returnData_P40.CompletedAngle4),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque5", returnData_P40.CompletedTorque5),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle5", returnData_P40.CompletedAngle5),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque6", returnData_P40.CompletedTorque6),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle6", returnData_P40.CompletedAngle6),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque7", returnData_P40.CompletedTorque7),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle7", returnData_P40.CompletedAngle7),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque8", returnData_P40.CompletedTorque8),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle8", returnData_P40.CompletedAngle8),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque9", returnData_P40.CompletedTorque9),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle9", returnData_P40.CompletedAngle9),
                                        SqlConnectionHelper.CreateParameter("@GearHeight1", returnData_P40.GearHeight1),
                                        SqlConnectionHelper.CreateParameter("@GearHeight2", returnData_P40.GearHeight2),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P40.Product_BarCode),
                                        SqlConnectionHelper.CreateParameter("@MotorCode", returnData_P40.MotorCode)
                                    };
                                helper.ExecuteNonQuery(sql, parametersInsert);



                                string saveObjJson = ObjectJson.sObjectJson.Serialize(returnData_P40);
                                ObjectJson.sObjectJson.TryDeserialize<P40DB3001ReturnData>(saveObjJson, out P40DB3001ReturnData resObj);
                                resObj.IsNG = false;
                                resObj.ReadFlag = false;
                                string saveMd5String = MD5Helper.sMD5Helper.GetMd5InString(ObjectJson.sObjectJson.Serialize(resObj));

                                bool insertNg = true;
                                if (returnData_P40.IsNG)
                                {
                                    string sqlDelA = "delete from P40DB3001ReturnData_Detail where MD5String='" + saveMd5String + "'";
                                    helper.ExecuteNonQuery(sqlDelA);
                                }
                                else
                                {
                                    using (var helperQuery = new SqlConnectionHelper())
                                    {
                                        string sqlSelect = "SELECT * FROM P40DB3001ReturnData_Detail where  MD5String='" + saveMd5String + "'";
                                        DataTable dtWorkOrder = helperQuery.ExecuteDataTable(sqlSelect);
                                        if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                                        {
                                            insertNg = false;
                                        }
                                    }
                                }
                                if (insertNg)
                                {
                                    string sqlNg = "insert into P40DB3001ReturnData_Detail(ReadFlag,CompletedTorque1," +
                                    "CompletedAngle1," +
                                    "CompletedTorque2," +
                                    "CompletedAngle2," +
                                    "CompletedTorque3," +
                                    "CompletedAngle3," +
                                    "CompletedTorque4," +
                                    "CompletedAngle4," +
                                     "CompletedTorque5," +
                                    "CompletedAngle5," +
                                     "CompletedTorque6," +
                                    "CompletedAngle6," +
                                     "CompletedTorque7," +
                                    "CompletedAngle7," +
                                    "CompletedTorque8," +
                                    "CompletedAngle8," +
                                    "CompletedTorque9," +
                                    "CompletedAngle9," +
                                    "GearHeight1," +
                                    "GearHeight2," +
                                    "CreatedTime," +
                                    "Product_BarCode," +
                                    "MotorCode,Md5String,IsNG)" +
                                        "values(@ReadFlag,@CompletedTorque1," +
                                        "@CompletedAngle1," +
                                        "@CompletedTorque2," +
                                        "@CompletedAngle2," +
                                        "@CompletedTorque3," +
                                        "@CompletedAngle3," +
                                        "@CompletedTorque4," +
                                        "@CompletedAngle4," +
                                        "@CompletedTorque5," +
                                        "@CompletedAngle5," +
                                        "@CompletedTorque6," +
                                        "@CompletedAngle6," +
                                        "@CompletedTorque7," +
                                        "@CompletedAngle7," +
                                        "@CompletedTorque8," +
                                        "@CompletedAngle8," +
                                        "@CompletedTorque9," +
                                        "@CompletedAngle9," +
                                        "@GearHeight1," +
                                        "@GearHeight2," +
                                        "@CreatedTime," +
                                        "@Product_BarCode," +
                                        "@MotorCode," +
                                        "@Md5String," +
                                        "@IsNG)";
                                    var parametersInsertNg = new[]
                                        {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", returnData_P40.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque1", returnData_P40.CompletedTorque1),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle1", returnData_P40.CompletedAngle1),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque2", returnData_P40.CompletedTorque2),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle2", returnData_P40.CompletedAngle2),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque3", returnData_P40.CompletedTorque3),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle3", returnData_P40.CompletedAngle3),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque4", returnData_P40.CompletedTorque4),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle4", returnData_P40.CompletedAngle4),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque5", returnData_P40.CompletedTorque5),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle5", returnData_P40.CompletedAngle5),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque6", returnData_P40.CompletedTorque6),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle6", returnData_P40.CompletedAngle6),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque7", returnData_P40.CompletedTorque7),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle7", returnData_P40.CompletedAngle7),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque8", returnData_P40.CompletedTorque8),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle8", returnData_P40.CompletedAngle8),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque9", returnData_P40.CompletedTorque9),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle9", returnData_P40.CompletedAngle9),
                                        SqlConnectionHelper.CreateParameter("@GearHeight1", returnData_P40.GearHeight1),
                                        SqlConnectionHelper.CreateParameter("@GearHeight2", returnData_P40.GearHeight2),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", returnData_P40.Product_BarCode),
                                        SqlConnectionHelper.CreateParameter("@MotorCode", returnData_P40.MotorCode),
                                        SqlConnectionHelper.CreateParameter("@Md5String", saveMd5String),
                                        SqlConnectionHelper.CreateParameter("@IsNG", returnData_P40.IsNG?"NG":"OK")
                                    };
                                    helper.ExecuteNonQuery(sqlNg, parametersInsertNg);
                                }


                                helper.CommitTransaction();
                            }
                            catch (Exception ex)
                            {
                                helper.RollbackTransaction();
                            }
                        }
                    }
                }
                if (p30DB2000ReturnDatas.TryDequeue(out P30DB3000ReturnData result_p30))
                {
                    if (!string.IsNullOrEmpty(result_p30.Product_BarCode) && result_p30.ReadFlag)
                    {
                        using (var helper = new SqlConnectionHelper())
                        {
                            try
                            {
                                helper.BeginTransaction();
                                string sqlDel = "delete from P30DB3000ReturnData where Product_BarCode='" + result_p30.Product_BarCode + "'";
                                helper.ExecuteNonQuery(sqlDel);

                                string sql = "insert into P30DB3000ReturnData(ReadFlag,CompletedTorque1," +
                                    "CompletedAngle1," +
                                    "CompletedTorque2," +
                                    "CompletedAngle2," +
                                    "CompletedTorque3," +
                                    "CompletedAngle3," +
                                    "CompletedTorque4," +
                                    "CompletedAngle4," +
                                     "CompletedTorque5," +
                                    "CompletedAngle5," +
                                     "CompletedTorque6," +
                                    "CompletedAngle6," +
                                     "CompletedTorque7," +
                                    "CompletedAngle7," +
                                    "CompletedTorque8," +
                                    "CompletedAngle8," +
                                    "CompletedTorque9," +
                                    "CompletedAngle9," +
                                    "CreatedTime," +
                                    "Product_BarCode)" +
                                        "values(@ReadFlag,@CompletedTorque1," +
                                        "@CompletedAngle1," +
                                        "@CompletedTorque2," +
                                        "@CompletedAngle2," +
                                        "@CompletedTorque3," +
                                        "@CompletedAngle3," +
                                        "@CompletedTorque4," +
                                        "@CompletedAngle4," +
                                        "@CompletedTorque5," +
                                        "@CompletedAngle5," +
                                        "@CompletedTorque6," +
                                        "@CompletedAngle6," +
                                        "@CompletedTorque7," +
                                        "@CompletedAngle7," +
                                        "@CompletedTorque8," +
                                         "@CompletedAngle8," +
                                        "@CompletedTorque9," +
                                         "@CompletedAngle9," +
                                        "@CreatedTime," +
                                        "@Product_BarCode)";
                                var parametersInsert = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", result_p30.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque1", result_p30.CompletedTorque1),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle1", result_p30.CompletedAngle1),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque2", result_p30.CompletedTorque2),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle2", result_p30.CompletedAngle2),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque3", result_p30.CompletedTorque3),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle3", result_p30.CompletedAngle3),
                                         SqlConnectionHelper.CreateParameter("@CompletedTorque4", result_p30.CompletedTorque4),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle4", result_p30.CompletedAngle4),
                                         SqlConnectionHelper.CreateParameter("@CompletedTorque5", result_p30.CompletedTorque5),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle5", result_p30.CompletedAngle5),
                                         SqlConnectionHelper.CreateParameter("@CompletedTorque6", result_p30.CompletedTorque6),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle6", result_p30.CompletedAngle6),
                                         SqlConnectionHelper.CreateParameter("@CompletedTorque7", result_p30.CompletedTorque7),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle7", result_p30.CompletedAngle7),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque8", result_p30.CompletedTorque8),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle8", result_p30.CompletedAngle8),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque9", result_p30.CompletedTorque9),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle9", result_p30.CompletedAngle9),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", result_p30.Product_BarCode)
                                    };
                                helper.ExecuteNonQuery(sql, parametersInsert);



                                string saveObjJson = ObjectJson.sObjectJson.Serialize(result_p30);
                                ObjectJson.sObjectJson.TryDeserialize<P30DB3000ReturnData>(saveObjJson, out P30DB3000ReturnData resObj);
                                resObj.IsNG = false;
                                resObj.ReadFlag = false;
                                string saveMd5String = MD5Helper.sMD5Helper.GetMd5InString(ObjectJson.sObjectJson.Serialize(resObj));

                                bool insertNg = true;
                                if (result_p30.IsNG)
                                {
                                    string sqlDelA = "delete from P30DB3000ReturnData_Detail where MD5String='" + saveMd5String + "'";
                                    helper.ExecuteNonQuery(sqlDelA);
                                }
                                else
                                {
                                    using (var helperQuery = new SqlConnectionHelper())
                                    {
                                        string sqlSelect = "SELECT * FROM P30DB3000ReturnData_Detail where  MD5String='" + saveMd5String + "'";
                                        DataTable dtWorkOrder = helperQuery.ExecuteDataTable(sqlSelect);
                                        if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                                        {
                                            insertNg = false;
                                        }
                                    }
                                }

                                if (insertNg)
                                {
                                    string sql_det = "insert into P30DB3000ReturnData_Detail(ReadFlag,CompletedTorque1," +
                                    "CompletedAngle1," +
                                    "CompletedTorque2," +
                                    "CompletedAngle2," +
                                    "CompletedTorque3," +
                                    "CompletedAngle3," +
                                    "CompletedTorque4," +
                                    "CompletedAngle4," +
                                     "CompletedTorque5," +
                                    "CompletedAngle5," +
                                     "CompletedTorque6," +
                                    "CompletedAngle6," +
                                     "CompletedTorque7," +
                                    "CompletedAngle7," +
                                    "CompletedTorque8," +
                                    "CompletedAngle8," +
                                    "CompletedTorque9," +
                                    "CompletedAngle9," +
                                    "CreatedTime," +
                                    "Product_BarCode," +
                                    "Md5String,IsNG)" +
                                        "values(@ReadFlag,@CompletedTorque1," +
                                        "@CompletedAngle1," +
                                        "@CompletedTorque2," +
                                        "@CompletedAngle2," +
                                        "@CompletedTorque3," +
                                        "@CompletedAngle3," +
                                        "@CompletedTorque4," +
                                        "@CompletedAngle4," +
                                        "@CompletedTorque5," +
                                        "@CompletedAngle5," +
                                        "@CompletedTorque6," +
                                        "@CompletedAngle6," +
                                        "@CompletedTorque7," +
                                        "@CompletedAngle7," +
                                        "@CompletedTorque8," +
                                         "@CompletedAngle8," +
                                        "@CompletedTorque9," +
                                         "@CompletedAngle9," +
                                        "@CreatedTime," +
                                        "@Product_BarCode," +
                                        "@Md5String,@IsNG)";
                                    var parametersInsert_det = new[]
                                        {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", result_p30.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque1", result_p30.CompletedTorque1),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle1", result_p30.CompletedAngle1),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque2", result_p30.CompletedTorque2),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle2", result_p30.CompletedAngle2),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque3", result_p30.CompletedTorque3),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle3", result_p30.CompletedAngle3),
                                         SqlConnectionHelper.CreateParameter("@CompletedTorque4", result_p30.CompletedTorque4),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle4", result_p30.CompletedAngle4),
                                         SqlConnectionHelper.CreateParameter("@CompletedTorque5", result_p30.CompletedTorque5),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle5", result_p30.CompletedAngle5),
                                         SqlConnectionHelper.CreateParameter("@CompletedTorque6", result_p30.CompletedTorque6),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle6", result_p30.CompletedAngle6),
                                         SqlConnectionHelper.CreateParameter("@CompletedTorque7", result_p30.CompletedTorque7),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle7", result_p30.CompletedAngle7),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque8", result_p30.CompletedTorque8),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle8", result_p30.CompletedAngle8),
                                        SqlConnectionHelper.CreateParameter("@CompletedTorque9", result_p30.CompletedTorque9),
                                        SqlConnectionHelper.CreateParameter("@CompletedAngle9", result_p30.CompletedAngle9),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", result_p30.Product_BarCode),
                                        SqlConnectionHelper.CreateParameter("@Md5String", saveMd5String),
                                        SqlConnectionHelper.CreateParameter("@IsNG", result_p30.IsNG?"NG":"OK")
                                    };
                                    helper.ExecuteNonQuery(sql_det, parametersInsert_det);
                                }
                                

                                helper.CommitTransaction();
                            }
                            catch (Exception ex)
                            {
                                helper.RollbackTransaction();
                            }
                        }
                    }
                }
                if (p20DB2000ReturnDatas.TryDequeue(out P20DB2000ReturnData result_p20))
                {
                    if (!string.IsNullOrEmpty(result_p20.Product_BarCode) && result_p20.ReadFlag)
                    {
                        using (var helper = new SqlConnectionHelper())
                        {
                            try
                            {
                                helper.BeginTransaction();
                                string sqlDel = "delete from P20DB2000ReturnData where Product_BarCode='" + result_p20.Product_BarCode + "'";
                                helper.ExecuteNonQuery(sqlDel);

                                string sql = "insert into P20DB2000ReturnData(ReadFlag,Cycle1Head1PressHeight," +
                                    "Cycle1Head1FinalPressure," +
                                    "Cycle2Head2PressHeight," +
                                    "Cycle2Head2FinalPressure," +
                                    "Cycle3Head3PressHeight," +
                                    "Cycle3Head3FinalPressure," +
                                    "CreatedTime," +
                                    "Product_BarCode)" +
                                        "values(@ReadFlag,@Cycle1Head1PressHeight," +
                                        "@Cycle1Head1FinalPressure," +
                                        "@Cycle2Head2PressHeight," +
                                        "@Cycle2Head2FinalPressure," +
                                        "@Cycle3Head3PressHeight," +
                                        "@Cycle3Head3FinalPressure," +
                                        "@CreatedTime," +
                                        "@Product_BarCode)";
                                var parametersInsert = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", result_p20.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1PressHeight", result_p20.Cycle1Head1PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1FinalPressure", result_p20.Cycle1Head1FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head2PressHeight", result_p20.Cycle2Head2PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head2FinalPressure", result_p20.Cycle2Head2FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle3Head3PressHeight", result_p20.Cycle3Head3PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle3Head3FinalPressure", result_p20.Cycle3Head3FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", result_p20.Product_BarCode)
                                    };
                                helper.ExecuteNonQuery(sql, parametersInsert);


                                string saveObjJson = ObjectJson.sObjectJson.Serialize(result_p20);
                                ObjectJson.sObjectJson.TryDeserialize<P20DB2000ReturnData>(saveObjJson, out P20DB2000ReturnData resObj);
                                resObj.IsNG = false;
                                resObj.ReadFlag = false;
                                string saveMd5String = MD5Helper.sMD5Helper.GetMd5InString(ObjectJson.sObjectJson.Serialize(resObj));

                                bool insertNg = true;
                                if (result_p20.IsNG)
                                {
                                    string sqlDelA = "delete from P20DB2000ReturnData_Detail where MD5String='" + saveMd5String + "'";
                                    helper.ExecuteNonQuery(sqlDelA);
                                }
                                else
                                {
                                    using (var helperQuery = new SqlConnectionHelper())
                                    {
                                        string sqlSelect = "SELECT * FROM P20DB2000ReturnData_Detail where  MD5String='" + saveMd5String + "'";
                                        DataTable dtWorkOrder = helperQuery.ExecuteDataTable(sqlSelect);
                                        if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                                        {
                                            insertNg = false;
                                        }
                                    }
                                }
                                if (insertNg)
                                {
                                    string sql_det = "insert into P20DB2000ReturnData_Detail(ReadFlag,Cycle1Head1PressHeight," +
                                    "Cycle1Head1FinalPressure," +
                                    "Cycle2Head2PressHeight," +
                                    "Cycle2Head2FinalPressure," +
                                    "Cycle3Head3PressHeight," +
                                    "Cycle3Head3FinalPressure," +
                                    "CreatedTime," +
                                    "Product_BarCode," +
                                    "Md5String,IsNG)" +
                                        "values(@ReadFlag,@Cycle1Head1PressHeight," +
                                        "@Cycle1Head1FinalPressure," +
                                        "@Cycle2Head2PressHeight," +
                                        "@Cycle2Head2FinalPressure," +
                                        "@Cycle3Head3PressHeight," +
                                        "@Cycle3Head3FinalPressure," +
                                        "@CreatedTime," +
                                        "@Product_BarCode," +
                                        "@Md5String," +
                                        "@IsNG)";
                                    var parametersInsert_det = new[]
                                        {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", result_p20.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1PressHeight", result_p20.Cycle1Head1PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1FinalPressure", result_p20.Cycle1Head1FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head2PressHeight", result_p20.Cycle2Head2PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head2FinalPressure", result_p20.Cycle2Head2FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle3Head3PressHeight", result_p20.Cycle3Head3PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle3Head3FinalPressure", result_p20.Cycle3Head3FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", result_p20.Product_BarCode),
                                        SqlConnectionHelper.CreateParameter("@Md5String", saveMd5String),
                                        SqlConnectionHelper.CreateParameter("@IsNG", result_p20.IsNG?"NG":"OK")
                                    };
                                    helper.ExecuteNonQuery(sql_det, parametersInsert_det);
                                }
                                


                                helper.CommitTransaction();
                            }
                            catch (Exception ex)
                            {
                                helper.RollbackTransaction();
                            }
                        }
                    }
                }
                if (p10DB2000ReturnDatas.TryDequeue(out P10DB2000ReturnData result))
                {
                    if (!string.IsNullOrEmpty(result.Product_BarCode) && result.ReadFlag)
                    {
                        using (var helper = new SqlConnectionHelper())
                        {
                            try
                            {
                                helper.BeginTransaction();
                                string sqlDel = "delete from P10DB2000ReturnData where Product_BarCode='" + result.Product_BarCode + "'";
                                helper.ExecuteNonQuery(sqlDel);

                                string sql = "insert into P10DB2000ReturnData(ReadFlag,Cycle1Head1PressHeight1," +
                                    "Cycle1Head1FinalPressure1," +
                                    "Cycle1Head1PressHeight2," +
                                    "Cycle1Head1FinalPressure2," +
                                    "Cycle1Head2PressHeight," +
                                    "Cycle1Head2FinalPressure," +
                                    "Cycle1Head3PressHeight," +
                                    "Cycle1Head3FinalPressure," +
                                    "Cycle1Head4PressHeight," +
                                    "Cycle1Head4FinalPressure," +
                                    "Cycle2Head2PressHeight," +
                                    "Cycle2Head2FinalPressure," +
                                    "Cycle2Head3PressHeight," +
                                    "Cycle2Head3FinalPressure," +
                                    "Cycle2Head4PressHeight," +
                                    "Cycle2Head4FinalPressure," +
                                    "Cycle3Head2PressHeight," +
                                    "Cycle3Head2FinalPressure," +
                                    "CreatedTime," +
                                    "Product_BarCode)" +
                                        "values(@ReadFlag,@Cycle1Head1PressHeight1," +
                                        "@Cycle1Head1FinalPressure1," +
                                        "@Cycle1Head1PressHeight2," +
                                        "@Cycle1Head1FinalPressure2," +
                                        "@Cycle1Head2PressHeight," +
                                        "@Cycle1Head2FinalPressure," +
                                        "@Cycle1Head3PressHeight," +
                                        "@Cycle1Head3FinalPressure," +
                                        "@Cycle1Head4PressHeight," +
                                        "@Cycle1Head4FinalPressure," +
                                        "@Cycle2Head2PressHeight," +
                                        "@Cycle2Head2FinalPressure," +
                                        "@Cycle2Head3PressHeight," +
                                        "@Cycle2Head3FinalPressure," +
                                        "@Cycle2Head4PressHeight," +
                                        "@Cycle2Head4FinalPressure," +
                                        "@Cycle3Head2PressHeight," +
                                        "@Cycle3Head2FinalPressure," +
                                        "@CreatedTime," +
                                        "@Product_BarCode)";
                                var parametersInsert = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", result.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1PressHeight1", result.Cycle1Head1PressHeight1),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1FinalPressure1", result.Cycle1Head1FinalPressure1),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1PressHeight2", result.Cycle1Head1PressHeight2),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1FinalPressure2", result.Cycle1Head1FinalPressure2),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head2PressHeight", result.Cycle1Head2PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head2FinalPressure", result.Cycle1Head2FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head3PressHeight", result.Cycle1Head3PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head3FinalPressure", result.Cycle1Head3FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head4PressHeight", result.Cycle1Head4PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head4FinalPressure", result.Cycle1Head4FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head2PressHeight", result.Cycle2Head2PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head2FinalPressure", result.Cycle2Head2FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head3PressHeight", result.Cycle2Head3PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head3FinalPressure", result.Cycle2Head3FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head4PressHeight", result.Cycle2Head4PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head4FinalPressure", result.Cycle2Head4FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle3Head2PressHeight", result.Cycle3Head2PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle3Head2FinalPressure", result.Cycle3Head2FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", result.Product_BarCode)
                                    };
                                helper.ExecuteNonQuery(sql, parametersInsert);


                                string saveObjJson = ObjectJson.sObjectJson.Serialize(result);
                                ObjectJson.sObjectJson.TryDeserialize<P10DB2000ReturnData>(saveObjJson,out P10DB2000ReturnData resObj);
                                resObj.IsNG = false;
                                resObj.ReadFlag = false;
                                DateTime.TryParse("1700-01-01 00:00:00", out DateTime dateTimeRes);
                                resObj.CreatedTime = dateTimeRes;
                                string saveMd5String = MD5Helper.sMD5Helper.GetMd5InString(ObjectJson.sObjectJson.Serialize(resObj));

                                bool insertNg= true;
                                if (result.IsNG)
                                {
                                    string sqlDelA = "delete from P10DB2000ReturnData_Detail where MD5String='"+ saveMd5String + "'";
                                    helper.ExecuteNonQuery(sqlDelA);
                                }
                                else
                                {
                                    using (var helperQuery = new SqlConnectionHelper())
                                    {
                                        string sqlSelect = "SELECT * FROM P10DB2000ReturnData_Detail where  MD5String='"+ saveMd5String + "'";
                                        DataTable dtWorkOrder = helperQuery.ExecuteDataTable(sqlSelect);
                                        if (dtWorkOrder != null && dtWorkOrder.Rows.Count>0)
                                        {
                                            insertNg = false;
                                        }
                                    }
                                }

                                if (insertNg)
                                {
                                    string sqlDetail = "insert into P10DB2000ReturnData_Detail(ReadFlag,Cycle1Head1PressHeight1," +
                                    "Cycle1Head1FinalPressure1," +
                                    "Cycle1Head1PressHeight2," +
                                    "Cycle1Head1FinalPressure2," +
                                    "Cycle1Head2PressHeight," +
                                    "Cycle1Head2FinalPressure," +
                                    "Cycle1Head3PressHeight," +
                                    "Cycle1Head3FinalPressure," +
                                    "Cycle1Head4PressHeight," +
                                    "Cycle1Head4FinalPressure," +
                                    "Cycle2Head2PressHeight," +
                                    "Cycle2Head2FinalPressure," +
                                    "Cycle2Head3PressHeight," +
                                    "Cycle2Head3FinalPressure," +
                                    "Cycle2Head4PressHeight," +
                                    "Cycle2Head4FinalPressure," +
                                    "Cycle3Head2PressHeight," +
                                    "Cycle3Head2FinalPressure," +
                                    "CreatedTime," +
                                    "Product_BarCode," +
                                    "IsNG," +
                                    "MD5String)" +
                                        "values(@ReadFlag,@Cycle1Head1PressHeight1," +
                                        "@Cycle1Head1FinalPressure1," +
                                        "@Cycle1Head1PressHeight2," +
                                        "@Cycle1Head1FinalPressure2," +
                                        "@Cycle1Head2PressHeight," +
                                        "@Cycle1Head2FinalPressure," +
                                        "@Cycle1Head3PressHeight," +
                                        "@Cycle1Head3FinalPressure," +
                                        "@Cycle1Head4PressHeight," +
                                        "@Cycle1Head4FinalPressure," +
                                        "@Cycle2Head2PressHeight," +
                                        "@Cycle2Head2FinalPressure," +
                                        "@Cycle2Head3PressHeight," +
                                        "@Cycle2Head3FinalPressure," +
                                        "@Cycle2Head4PressHeight," +
                                        "@Cycle2Head4FinalPressure," +
                                        "@Cycle3Head2PressHeight," +
                                        "@Cycle3Head2FinalPressure," +
                                        "@CreatedTime," +
                                        "@Product_BarCode," +
                                        "@IsNG," +
                                        "@MD5String)";


                                    var parametersInsertDetail = new[]
                                        {
                                        SqlConnectionHelper.CreateParameter("@ReadFlag", result.ReadFlag),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1PressHeight1", result.Cycle1Head1PressHeight1),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1FinalPressure1", result.Cycle1Head1FinalPressure1),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1PressHeight2", result.Cycle1Head1PressHeight2),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head1FinalPressure2", result.Cycle1Head1FinalPressure2),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head2PressHeight", result.Cycle1Head2PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head2FinalPressure", result.Cycle1Head2FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head3PressHeight", result.Cycle1Head3PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head3FinalPressure", result.Cycle1Head3FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head4PressHeight", result.Cycle1Head4PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle1Head4FinalPressure", result.Cycle1Head4FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head2PressHeight", result.Cycle2Head2PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head2FinalPressure", result.Cycle2Head2FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head3PressHeight", result.Cycle2Head3PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head3FinalPressure", result.Cycle2Head3FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head4PressHeight", result.Cycle2Head4PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle2Head4FinalPressure", result.Cycle2Head4FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@Cycle3Head2PressHeight", result.Cycle3Head2PressHeight),
                                        SqlConnectionHelper.CreateParameter("@Cycle3Head2FinalPressure", result.Cycle3Head2FinalPressure),
                                        SqlConnectionHelper.CreateParameter("@CreatedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        SqlConnectionHelper.CreateParameter("@Product_BarCode", result.Product_BarCode),
                                        SqlConnectionHelper.CreateParameter("@IsNG", result.IsNG?"NG":"OK"),
                                        SqlConnectionHelper.CreateParameter("@MD5String",saveMd5String)
                                    };
                                    helper.ExecuteNonQuery(sqlDetail, parametersInsertDetail);
                                }

                                helper.CommitTransaction();
                            }
                            catch (Exception ex)
                            {
                                helper.RollbackTransaction();
                            }
                        }
                    }
                }




                if (timeCounter > 10)
                {
                    timeCounter = 0;

                    try
                    {
                        switch (_deviceInfo.ParentCode)
                        {
                            case "P10":
                                Thread t = new Thread(new ThreadStart(UpdateCounting_P10));
                                t.Start();
                                break;
                            case "P20":
                                Thread t20 = new Thread(new ThreadStart(UpdateCounting_P20));
                                t20.Start();
                                break;
                            case "P30":
                                Thread t30 = new Thread(new ThreadStart(UpdateCounting_P30));
                                t30.Start();
                                break;
                            case "P35":
                                Thread t35 = new Thread(new ThreadStart(UpdateCounting_P35));
                                t35.Start();
                                break;
                            case "P40":
                                Thread t40 = new Thread(new ThreadStart(UpdateCounting_P40));
                                t40.Start();
                                break;
                            case "P50":
                                Thread t50 = new Thread(new ThreadStart(UpdateCounting_P50));
                                t50.Start();
                                break;
                            case "P60":
                                Thread t60 = new Thread(new ThreadStart(UpdateCounting_P60));
                                t60.Start();
                                break;
                            case "P70":
                                Thread t70 = new Thread(new ThreadStart(UpdateCounting_P70));
                                t70.Start();
                                break;
                            case "P80":
                                Thread t80 = new Thread(new ThreadStart(UpdateCounting_P80));
                                t80.Start();
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                timeCounter++;
                

                Thread.Sleep(3000);
            }
        }
        //异步连接PLC  通过10秒钟的等待设备连接结果
        private bool Connect()
        {
            if (_plc != null)
            {
                _plc.Open();
                
                if (StatusChanged != null)
                {
                    StatusChanged.Invoke(this, deviceId, _plc.IsConnected);
                    //设备连接成功之后开始读写数据
                    if (_plc.IsConnected)
                    {
                        DeviceDataExchange();
                    }
                }
                return _plc.IsConnected;
            }
            else
            {
                return false;
            }
        }
        //设备连接成功之后开始读写数据
        private void DeviceDataExchange()
        {
            Task task = Task.Run(() =>
            {
                while (!GlobalProperty.sGlobalProperty.GetMainFormCloseStatus() && _plc.IsConnected)
                {
                    DevDataCommunicate();
                    GlobalDevice.sGlobalDevice.OnConnectionStatusUpdate(this, this._deviceInfo.Id, true);//设备连接成功的就更新设备的连接状态
                    Thread.Sleep(200);
                }
            });
        }
        //设备数据交互
        private void DevDataCommunicate()
        {
            try
            {
                switch (_deviceInfo.ParentCode)
                {
                    case "P10":
                        P10_Action();
                        break;
                    case "P20":
                        P20_Action();
                        break;
                    case "P30":
                        P30_Action();
                        break;
                    case "P35":
                        P35_Action();
                        break;
                    case "P40":
                        P40_Action();
                        break;
                    case "P50":
                        P50_Action();
                        break;
                    case "P60":
                        P60_Action();
                        break;
                    case "P70":
                        P70_Action();
                        break;
                    case "P80":
                        P80_Action();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
            }
        }
        private void P35_Action()
        {
            //WriteBool("DB1501.DBX0.2", true);不允许上线
            try
            {
                P30UploadData data = ReadData_P35();
                if (data != null && data.WorkpieceOnlineRequest)
                {
                    WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.Barcode);
                    if (workOrderLinkCode != null)
                    {
                        List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                        string recipeToUse = "";
                        if (order != null && order.Count > 0)
                        {
                            recipeToUse = order[0].FormulaText;
                        }
                        WriteInt("DB1501.DBW2", GetCommandCode(recipeToUse));
                        WriteBool("DB1501.DBX0.1", true);

                        LickMicosOrder("P30DB3000ReturnData_StartWork", data.Barcode);
                    }
                }
                if (data != null && data.StationProcessFinished)
                {
                    WriteBool("DB1501.DBX0.3", true);
                }
                if (data != null && data.ReceivedOnlineInstruction)
                {
                    WriteBool("DB1501.DBX0.1", false);
                    WriteBool("DB1501.DBX0.2", false);
                }
                if (data != null && data.ReceivedFinishInstruction)
                {
                    WriteBool("DB1501.DBX0.3", false);
                }
                if (data != null && data.ManualNGOffline)
                {
                    WriteBool("DB1501.DBX0.4", true);
                }
                if (data != null && data.ReceivedOfflineInstruction)
                {
                    WriteBool("DB1501.DBX0.4", false);
                }
               
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Error, Content = "P30_Action 方法报错了，详细信息：" + ex.Message });
            }
        }
        private void P80_Action()
        {
            try
            {
                P80UploadData data = ReadPlcData_ope_P80();
                if (data != null && data.WorkpieceOnlineRequest)
                {
                    DataTable dtLao = GlobalBaseData.sGlobalBaseData.GetStationData("P70DB3003ReturnData_Detail", data.Barcode);
                    DataTable dtBiaoding = GlobalBaseData.sGlobalBaseData.GetStationData("signal_data_Detail", data.Barcode);
                    DataTable dtFuzai = GlobalBaseData.sGlobalBaseData.GetStationData("signal_data2_Detail", data.Barcode);
                    if (dtLao != null && dtLao.Rows.Count > 0 && dtBiaoding != null && dtBiaoding.Rows.Count > 0 && dtFuzai != null && dtFuzai.Rows.Count > 0)
                    {
                        bool canUp = true;
                        DataRow dataRowlao = dtLao.Rows[0];
                        string isNgLao = !Convert.IsDBNull(dataRowlao["IsNG"]) ? dataRowlao["IsNG"].ToString() : "";
                        if (isNgLao == "NG")
                        {
                            canUp = false;
                        }
                        if (canUp)
                        {
                            DataRow dataRowBiao = dtBiaoding.Rows[0];
                            string isNgBiao = !Convert.IsDBNull(dataRowBiao["IsNG"]) ? dataRowBiao["IsNG"].ToString() : "";
                            if (isNgBiao == "NG")
                            {
                                canUp = false;
                            }
                        }
                        if (canUp)
                        {
                            DataRow dataRowFuzai = dtFuzai.Rows[0];
                            string isNgFuzai = !Convert.IsDBNull(dataRowFuzai["IsNG"]) ? dataRowFuzai["IsNG"].ToString() : "";
                            if (isNgFuzai == "NG")
                            {
                                canUp = false;
                            }
                        }
                        if (canUp)
                        {
                            WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.Barcode);
                            if (workOrderLinkCode != null)
                            {
                                List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                                string recipeToUse = "";
                                if (order != null && order.Count > 0)
                                {
                                    recipeToUse = order[0].FormulaText;
                                }
                                WriteInt("DB1001.DBW2", GetCommandCode(recipeToUse));
                                WriteBool("DB1001.DBX0.1", true);
                                WriteBool("DB1001.DBX0.2", false);
                                LickMicosOrder("P80DB3004ReturnData_StartWork", data.Barcode);
                            }
                        }
                        else
                        {
                            WriteBool("DB1001.DBX0.1", false);
                            WriteBool("DB1001.DBX0.2", true);
                        }
                    }
                    else
                    {
                        WriteBool("DB1001.DBX0.1", false);
                        WriteBool("DB1001.DBX0.2", true);
                    }
                }
                if (data != null && data.StationProcessFinished)
                {
                    WriteBool("DB1001.DBX0.3", true);
                }
                if (data != null && data.ReceivedOnlineInstruction)
                {
                    WriteBool("DB1001.DBX0.1", false);
                    WriteBool("DB1001.DBX0.2", false);
                }
                if (data != null && data.ReceivedFinishInstruction)
                {
                    WriteBool("DB1001.DBX0.3", false);
                }
                if (data != null && data.ManualNGOffline)
                {
                    WriteBool("DB1001.DBX0.4", true);
                    P80DB3004ReturnData dataWorkingNg = ReadDB3001Data_ope_P80();
                    if (dataWorkingNg != null && !string.IsNullOrEmpty(data.Barcode))
                    {
                        dataWorkingNg.Product_BarCode = data.Barcode;
                        p80DB2000ReturnDatas.Enqueue(dataWorkingNg);
                    }
                }
                if (data != null && data.ReceivedOfflineInstruction)
                {
                    WriteBool("DB1001.DBX0.4", false);
                }
                P80DB3004ReturnData dataWorking = ReadDB3001Data_ope_P80();
                if (dataWorking != null && dataWorking.ReadFlag)
                {
                    dataWorking.Product_BarCode = data.Barcode;
                    if (!string.IsNullOrEmpty(dataWorking.Product_BarCode))
                    {
                        p80DB2000ReturnDatas.Enqueue(dataWorking);
                    }
                    //以下是关闭Read 置为false
                    P80DB3004ReturnData returnData = new P80DB3004ReturnData();
                    returnData.ReadFlag = false;
                    WriteDB3001Data_readClose_P80(returnData);
                }
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Error, Content = "P80_Action 方法报错了，详细信息：" + ex.Message });
            }
        }
        private void P70_Action()
        {
            try
            {
                bool overWorkRead = false;
                P70UploadData data = ReadPlcData_ope_P70();
                string codeStr = data.CalibrationBarcode;
                if (!string.IsNullOrEmpty(codeStr) && codeStr.Length > 3)
                {
                    char c = codeStr[0];
                    if (c != '1')
                    {
                        codeStr = codeStr.Substring(2);
                        data.CalibrationBarcode = codeStr;
                    }
                }
                string codeStr2 = data.AgingBarcode2;
                if (!string.IsNullOrEmpty(codeStr2) && codeStr2.Length > 3)
                {
                    char c = codeStr2[0];
                    if (c != '1')
                    {
                        codeStr2 = codeStr2.Substring(2);
                        data.AgingBarcode2 = codeStr2;
                    }
                }
                if (data != null && data.WorkpieceOnlineRequest)
                {
                    DataTable dt = GlobalBaseData.sGlobalBaseData.GetStationData("P60DB3000ReturnData_Detail", data.CalibrationBarcode);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow dataRow = dt.Rows[0];
                        string isNg = !Convert.IsDBNull(dataRow["IsNG"]) ? dataRow["IsNG"].ToString() : "";
                        if (isNg == "NG")
                        {
                            WriteBool("DB1501.DBX0.1", false);
                            WriteBool("DB1501.DBX0.2", true);
                        }
                        else
                        {
                            WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.CalibrationBarcode);
                            if (workOrderLinkCode != null)
                            {
                                List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                                string recipeToUse = "";
                                if (order != null && order.Count > 0)
                                {
                                    recipeToUse = order[0].FormulaText;
                                }
                                WriteInt("DB1501.DBW2", GetCommandCode(recipeToUse));
                                WriteBool("DB1501.DBX0.1", true);
                                WriteBool("DB1501.DBX0.2", false);
                            }
                        }
                    }
                    else
                    {
                        WriteBool("DB1501.DBX0.1", false);
                        WriteBool("DB1501.DBX0.2", true);
                    }
                }

                if (data != null && data.AgingOnlineRequest2)
                {
                    DataTable dt = GlobalBaseData.sGlobalBaseData.GetStationData("P60DB3000ReturnData_Detail", data.AgingBarcode2);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow dataRow = dt.Rows[0];
                        string isNg = !Convert.IsDBNull(dataRow["IsNG"]) ? dataRow["IsNG"].ToString() : "";
                        if (isNg == "NG")
                        {
                            WriteBool("DB1501.DBX26.0", false);
                            WriteBool("DB1501.DBX26.1", true);
                        }
                        else
                        {
                            WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.AgingBarcode2);
                            if (workOrderLinkCode != null)
                            {
                                List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                                string recipeToUse = "";
                                if (order != null && order.Count > 0)
                                {
                                    recipeToUse = order[0].FormulaText;
                                }
                                WriteInt("DB1501.DBW74", GetCommandCode(recipeToUse));
                                WriteBool("DB1501.DBX26.0", true);
                                WriteBool("DB1501.DBX26.1", false);
                            }
                        }
                    }
                    else
                    {
                        WriteBool("DB1501.DBX26.0", false);
                        WriteBool("DB1501.DBX26.1", true);
                    }
                }
                if (data != null && data.AgingReceivedOnlineCommand2)
                {
                    WriteBool("DB1501.DBX26.0", false);
                }
                if (data != null && data.AgingReceivedEndCommand2)
                {
                    WriteBool("DB1501.DBX26.2", false);
                }
                if (data != null && data.LoadTestNotification)
                {
                    WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.CalibrationBarcode);
                    if (workOrderLinkCode != null)
                    {
                        List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                        string recipeToUse = "";
                        if (order != null && order.Count > 0)
                        {
                            recipeToUse = order[0].FormulaText;
                        }
                        WriteInt("DB1501.DBW2", GetCommandCode(recipeToUse));
                    }
                }
                if (data != null && data.AgingManualNgOffline2)
                {
                    WriteBool("DB1501.DBX26.3", false);
                }
                if (data != null && data.AgingReceivedOfflineCommand2)
                {
                    WriteBool("DB1501.DBX26.3", false);
                }
                if (data != null && data.AgingOnlineRequest3)
                {//todo
                }
                if (data != null && data.StationProcessFinished)
                {
                    WriteBool("DB1501.DBX0.3", true);
                    overWorkRead = true;
                }
                if (data != null && data.ReceivedOnlineInstruction)
                {
                    WriteBool("DB1501.DBX0.1", false);
                    WriteBool("DB1501.DBX0.2", false);
                }
                if (data != null && data.ReceivedFinishInstruction)
                {
                    WriteBool("DB1501.DBX0.3", false);
                    overWorkRead = true;
                }
                if (data != null && data.ManualNGOffline)
                {
                    WriteBool("DB1501.DBX0.4", true);
                    P70DB3003ReturnData dataWorkingLaoNg = ReadDB3001Data_wor_P70();
                    if (dataWorkingLaoNg != null)
                    {
                        dataWorkingLaoNg.IsNG = true;
                        dataWorkingLaoNg.Product_BarCode = data.CalibrationBarcode;
                        p70DB2000ReturnDatas.Enqueue(dataWorkingLaoNg);
                    }
                    P70DB3001UploadData biaoding_Ng = ReadDB3001Data_P70_biaoding();
                    if (biaoding_Ng != null)
                    {
                        biaoding_Ng.IsNG = true;
                        biaoding_Ng.ProductBarCode = data.CalibrationBarcode;
                        p70_biaoding_ReturnDatas.Enqueue(biaoding_Ng);
                    }
                    P70DB3001UploadData2 fuzai_Ng = ReadDB3001Data2_P70_fuzai();
                    if (fuzai_Ng != null)
                    {
                        fuzai_Ng.IsNG = true;
                        fuzai_Ng.ProductBarCode = data.CalibrationBarcode;
                        p70_fuzai_ReturnDatas.Enqueue(fuzai_Ng);
                    }
                }
                if (data != null && data.ReceivedOfflineInstruction)
                {
                    WriteBool("DB1501.DBX0.4", false);
                    overWorkRead = true;
                }
                if (data != null && data.AgingProcessEnded2)
                {
                    WriteBool("DB1501.DBX26.2", true);
                }
                if (data != null && data.CalibrationStartRequest)
                {
                    WriteBool("DB1501.DBX10", true);
                }
                P70DB3003ReturnData dataWorking = ReadDB3001Data_wor_P70();
                if (dataWorking != null)
                {
                    if (dataWorking.ReadFlag1 || dataWorking.ReadFlag2 || dataWorking.ReadFlag3 || dataWorking.ReadFlag4 || overWorkRead)
                    {
                        if (dataWorking.ReadFlag1)
                        {
                            dataWorking.Product_BarCode = data.CalibrationBarcode;
                        }
                        else if (dataWorking.ReadFlag2)
                        {
                            dataWorking.Product_BarCode = data.AgingBarcode2;
                        }
                        else
                        {
                            dataWorking.Product_BarCode = data.CalibrationBarcode;
                        }
                        p70DB2000ReturnDatas.Enqueue(dataWorking);
                        //以下是关闭Read 置为false
                        P70DB3003ReturnData returnData = new P70DB3003ReturnData();
                        returnData.ReadFlag1 = false;
                        WriteDB3001Data_readClose_P70(returnData);
                        P70DB3003ReturnData returnData2 = new P70DB3003ReturnData();
                        returnData2.ReadFlag2 = false;
                        WriteDB3001Data_readClose_P70(returnData2);
                    }
                }
                P70DB3001UploadData biaoding = ReadDB3001Data_P70_biaoding();
                if (biaoding != null) 
                {
                    if (biaoding.MESCanRead || overWorkRead)
                    {
                        if (string.IsNullOrEmpty(biaoding.ProductBarCode))
                        {
                            biaoding.ProductBarCode = data.CalibrationBarcode;
                        }
                        p70_biaoding_ReturnDatas.Enqueue(biaoding);

                        var writeData = new P70DB3001UploadData { MESCanRead = false };
                        WriteDB3001Data_P70_readClose(writeData);
                    }
                }
                P70DB3001UploadData2 fuzai = ReadDB3001Data2_P70_fuzai();
                if (fuzai != null)
                {
                    if (fuzai.MESCanRead || overWorkRead)
                    {
                        if (string.IsNullOrEmpty(fuzai.ProductBarCode))
                        {
                            fuzai.ProductBarCode = data.CalibrationBarcode;
                        }
                        p70_fuzai_ReturnDatas.Enqueue(fuzai);

                        var writeData = new P70DB3001UploadData2 { MESCanRead = false };
                        WriteDB3001Data2_P70_fuzai(writeData);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Error, Content = "P70_Action 方法报错了，详细信息：" + ex.Message });
            }
        }
        private void P60_Action()
        {
            try
            {
                P60UploadData data = ReadPlcData_ope_P60();
                string codeStr = data.BarcodeInfo;
                if (!string.IsNullOrEmpty(codeStr) && codeStr.Length > 3)
                {
                    char c = codeStr[0];
                    if (c != '1')
                    {
                        codeStr = codeStr.Substring(2);
                        data.BarcodeInfo = codeStr;
                    }
                }
              
                if (data != null && data.WorkpieceOnlineRequest)
                {
                    DataTable dt = GlobalBaseData.sGlobalBaseData.GetStationData("P50DB3002ReturnData_Detail", data.BarcodeInfo);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow dataRow = dt.Rows[0];
                        string isNg = !Convert.IsDBNull(dataRow["IsNG"]) ? dataRow["IsNG"].ToString() : "";
                        if (isNg == "NG")
                        {
                            WriteBool("DB1001.DBX0.1", false);
                            WriteBool("DB1001.DBX0.2", true);
                        }
                        else
                        {
                            WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.BarcodeInfo);
                            if (workOrderLinkCode != null)
                            {
                                List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                                string recipeToUse = "";
                                if (order != null && order.Count > 0)
                                {
                                    recipeToUse = order[0].FormulaText;
                                }
                                WriteInt("DB1001.DBW2", GetCommandCode(recipeToUse));
                                WriteBool("DB1001.DBX0.1", true);
                                WriteBool("DB1001.DBX0.2", false);

                                LickMicosOrder("P60DB3000ReturnData_StartWork", data.BarcodeInfo);
                            }
                        }
                    }
                    else
                    {
                        WriteBool("DB1001.DBX0.1", false);
                        WriteBool("DB1001.DBX0.2", true);
                    }
                }
                if (data != null && data.StationProcessFinished)
                {
                    WriteBool("DB1001.DBX0.3", true);
                }
                if (data != null && data.ReceivedOnlineInstruction)
                {
                    WriteBool("DB1001.DBX0.1", false);
                    WriteBool("DB1001.DBX0.2", false);
                }
                if (data != null && data.ReceivedFinishInstruction)
                {
                    WriteBool("DB1001.DBX0.3", false);
                }
                if (data != null && data.ManualNGOffline)
                {
                    WriteBool("DB1001.DBX0.4", true);
                    P60DB3000ReturnData dataWorkingNg = ReadDB3000Data_wor_P60();
                    if (dataWorkingNg != null)
                    {
                        dataWorkingNg.IsNG = true;
                        p60DB2000ReturnDatas.Enqueue(dataWorkingNg);
                    }
                }
                if (data != null && data.ReceivedOfflineInstruction)
                {
                    WriteBool("DB1001.DBX0.4", false);
                }
                P60DB3000ReturnData dataWorking = ReadDB3000Data_wor_P60();
                if (dataWorking != null && dataWorking.ReadFlag)
                {
                    p60DB2000ReturnDatas.Enqueue(dataWorking);
                    //关闭read
                    P60DB3000ReturnData returnData = new P60DB3000ReturnData();
                    returnData.ReadFlag = false;
                    WriteDB3000Data_readClose_P60(returnData);
                }
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Error, Content = "P50_Action 方法报错了，详细信息：" + ex.Message });
            }
        }
        private void P50_Action()
        {
            //WriteBool("DB2001.DBX0.1", false);
            //WriteBool("DB2001.DBX0.2", true);不允许上线
            try
            {
                P50UploadData data = ReadPlcData_ope_P50();
                if (data != null && data.WorkpieceOnlineRequest)
                {
                    DataTable dt = GlobalBaseData.sGlobalBaseData.GetStationData("P40DB3001ReturnData_Detail", data.Barcode);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow dataRow = dt.Rows[0];
                        string isNg = !Convert.IsDBNull(dataRow["IsNG"]) ? dataRow["IsNG"].ToString() : "";
                        if (isNg == "NG")
                        {
                            WriteBool("DB2001.DBX0.1", false);
                            WriteBool("DB2001.DBX0.2", true);
                        }
                        else
                        {
                            WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.Barcode);
                            if (workOrderLinkCode != null)
                            {
                                List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                                string recipeToUse = "";
                                if (order != null && order.Count > 0)
                                {
                                    recipeToUse = order[0].FormulaText;
                                }
                                WriteInt("DB2001.DBW2", GetCommandCode(recipeToUse));
                                WriteBool("DB2001.DBX0.1", true);
                                WriteBool("DB2001.DBX0.2", false);
                                LickMicosOrder("P50DB3002ReturnData_StartWork", data.Barcode);
                            }
                        }
                    }
                    else
                    {
                        WriteBool("DB2001.DBX0.1", false);
                        WriteBool("DB2001.DBX0.2", true);
                    }
                }
                if (data != null && data.StationProcessFinished)
                {
                    WriteBool("DB2001.DBX0.3", true);
                }
                if (data != null && data.ReceivedOnlineInstruction)
                {
                    WriteBool("DB2001.DBX0.1", false);
                    WriteBool("DB2001.DBX0.2", false);
                }
                if (data != null && data.ReceivedFinishInstruction)
                {
                    WriteBool("DB2001.DBX0.3", false);
                }
                if (data != null && data.ManualNGOffline)
                {
                    WriteBool("DB2001.DBX0.4", true);
                    P50DB3002ReturnData dataWorkingNg = ReadDB3002Data_workData_P50();
                    if (dataWorkingNg != null) 
                    {
                        dataWorkingNg.IsNG = true;
                        dataWorkingNg.Product_BarCode = data.Barcode;
                        p50DB2000ReturnDatas.Enqueue(dataWorkingNg);
                    }
                }
                if (data != null && data.ReceivedOfflineInstruction)
                {
                    WriteBool("DB2001.DBX0.4", false);
                }
                P50DB3002ReturnData dataWorking = ReadDB3002Data_workData_P50();
                if (dataWorking != null && dataWorking.ReadFlag)
                {
                    dataWorking.Product_BarCode = data.Barcode;
                    p50DB2000ReturnDatas.Enqueue(dataWorking);
                    //关闭read
                    P50DB3002ReturnData returnData = new P50DB3002ReturnData();
                    returnData.ReadFlag = false;
                    WriteDB3002Data_readClose_P50(returnData);
                }
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Error, Content = "P50_Action 方法报错了，详细信息：" + ex.Message });
            }
        }
        private void P40_Action()
        {
            //WriteBool("DB1501.DBX0.1", false);
            //WriteBool("DB1501.DBX0.2", true);   不允许上线
            try
            {
                P40UploadData data = ReadOperateData_P40();
                string codeStr = data.Barcode;
                if (!string.IsNullOrEmpty(codeStr) && codeStr.Length > 3)
                {
                    char c = codeStr[0];
                    if (c != '1')
                    {
                        codeStr = codeStr.Substring(2);
                        data.Barcode = codeStr;
                    }
                }
                if (data != null && data.WorkpieceOnlineRequest)
                {
                    DataTable dt = GlobalBaseData.sGlobalBaseData.GetStationData("P30DB3000ReturnData_Detail", data.Barcode);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow dataRow = dt.Rows[0];
                        string isNg = !Convert.IsDBNull(dataRow["IsNG"]) ? dataRow["IsNG"].ToString() : "";
                        if (isNg == "NG")
                        {
                            WriteBool("DB1501.DBX0.1", false);
                            WriteBool("DB1501.DBX0.2", true);
                        }
                        else
                        {
                            bool b = GetCanToDo(data.Barcode);//胶水凝固时间是否满足预定值
                            if (b)
                            {
                                WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.Barcode);
                                if (workOrderLinkCode != null)
                                {
                                    List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                                    string recipeToUse = "";
                                    if (order != null && order.Count > 0)
                                    {
                                        recipeToUse = order[0].FormulaText;
                                    }
                                    WriteInt("DB1501.DBW2", GetCommandCode(recipeToUse));
                                    WriteBool("DB1501.DBX0.1", true);
                                    WriteBool("DB1501.DBX0.2", false);
                                    LickMicosOrder("P40DB3001ReturnData_StartWork", data.Barcode);
                                }
                            }
                            else
                            {
                                WriteBool("DB1501.DBX0.1", false);
                                WriteBool("DB1501.DBX0.2", true);
                            }
                        }
                    }
                    else
                    {
                        WriteBool("DB1501.DBX0.1", false);
                        WriteBool("DB1501.DBX0.2", true);
                    }
                }
                if (data != null && data.StationProcessFinished)
                {
                    WriteBool("DB1501.DBX0.3", true);
                }
                if (data != null && data.ReceivedOnlineInstruction)
                {
                    WriteBool("DB1501.DBX0.1", false);
                    WriteBool("DB1501.DBX0.2", false);
                }
                if (data != null && data.ReceivedFinishInstruction)
                {
                    WriteBool("DB1501.DBX0.3", false);
                }
                if (data != null && data.ManualNGOffline)
                {
                    WriteBool("DB1501.DBX0.4", true);
                    P40DB3001ReturnData dataWorkingNg = ReadDB3001Data_WorkingData_P40();
                    if (dataWorkingNg != null)
                    {
                        dataWorkingNg.IsNG = true;
                        string motorCode = ReadString("DB1500.DBB74", 50);
                        motorCode = motorCode.TrimEnd('\r', '\n', '\0');
                        motorCode = motorCode.Replace(" ", "-");
                        dataWorkingNg.MotorCode = motorCode;
                        dataWorkingNg.Product_BarCode = data.Barcode;
                        p40DB2000ReturnDatas.Enqueue(dataWorkingNg);
                    }
                }
                if (data != null && data.ReceivedOfflineInstruction)
                {
                    WriteBool("DB1501.DBX0.4", false);
                }
                P40DB3001ReturnData dataWorking = ReadDB3001Data_WorkingData_P40();
                if (dataWorking != null && dataWorking.ReadFlag)
                {
                    string motorCode = ReadString("DB1500.DBB74", 50);
                    motorCode = motorCode.TrimEnd('\r','\n','\0');
                    motorCode = motorCode.Replace(" ","-");
                    dataWorking.MotorCode = motorCode;
                    dataWorking.Product_BarCode = data.Barcode;
                    p40DB2000ReturnDatas.Enqueue(dataWorking);
                    //关闭read
                    P40DB3001ReturnData returnData = new P40DB3001ReturnData();
                    returnData.ReadFlag = false;
                    WriteDB3001Data_readClose_P40(returnData);
                }
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Error, Content = "P40_Action 方法报错了，详细信息：" + ex.Message });
            }
        }
        private void P30_Action()
        {
            try
            {
                P30UploadData data = ReadData_P30();
                if (data != null && data.WorkpieceOnlineRequest)
                {
                    DataTable dt = GlobalBaseData.sGlobalBaseData.GetStationData("P20DB2000ReturnData_Detail", data.Barcode);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow dataRow = dt.Rows[0];
                        string isNg = !Convert.IsDBNull(dataRow["IsNG"]) ? dataRow["IsNG"].ToString() : "";
                        if (isNg == "NG")
                        {
                            WriteBool("DB1001.DBX0.1", false);
                            WriteBool("DB1001.DBX0.2", true);
                        }
                        else
                        {
                            WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.Barcode);
                            if (workOrderLinkCode != null)
                            {
                                List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                                string recipeToUse = "";
                                if (order != null && order.Count > 0)
                                {
                                    recipeToUse = order[0].FormulaText;
                                }

                                string ssStr = "await";
                                string turnCode = data.TurnHeadCode;
                                if (turnCode=="110001")
                                {
                                    ssStr = "起落架";
                                }
                                if (turnCode == "220002")
                                {
                                    ssStr = "机械臂";
                                }
                                if (!recipeToUse.Contains(ssStr))
                                {
                                    //TODO   根据配方指定转头，如果转头不对不允许上线
                                    WriteBool("DB1001.DBX0.1", false);
                                    WriteBool("DB1001.DBX0.2", true);
                                }
                                else
                                {
                                    WriteInt("DB1001.DBW2", GetCommandCode(recipeToUse));
                                    WriteBool("DB1001.DBX0.1", true);
                                    WriteBool("DB1001.DBX0.2", false);
                                    LickMicosOrder("P30DB3000ReturnData_StartWork", data.Barcode);
                                }
                            }
                        }
                    }
                    else
                    {//上一个工站P120 没有做完 不允许开始P30
                        WriteBool("DB1001.DBX0.1", false);
                        WriteBool("DB1001.DBX0.2", true);
                    }
                }
                if (data != null && data.StationProcessFinished)
                {
                    WriteBool("DB1001.DBX0.3", true);
                }
                if (data != null && data.ReceivedOnlineInstruction)
                {
                    WriteBool("DB1001.DBX0.1", false);
                    WriteBool("DB1001.DBX0.2", false);
                }
                if (data != null && data.ReceivedFinishInstruction)
                {
                    WriteBool("DB1001.DBX0.3", false);
                }
                if (data != null && data.ManualNGOffline)
                {
                    WriteBool("DB1001.DBX0.4", true);
                    
                    P30DB3000ReturnData dataWorking_NG = ReadDB3000Data_P30();
                    if (dataWorking_NG != null)
                    {
                        dataWorking_NG.Product_BarCode = data.Barcode;
                        dataWorking_NG.IsNG = true;
                        List<DeviceInfo> devs = GlobalDevice.sGlobalDevice.GetDeviceInfos();
                        DeviceInfo d35 = devs.FindAll(a => a.ParentCode == "P35").FirstOrDefault();
                        P30DB3000ReturnData dataWorking_35 = d35.s7Comm.ReadDB3000Data_P35();
                        if (dataWorking_35 != null)
                        {
                            dataWorking_NG.CompletedTorque3 = dataWorking_35.CompletedTorque1;
                            dataWorking_NG.CompletedAngle3 = dataWorking_35.CompletedAngle1;
                            dataWorking_NG.CompletedTorque4 = dataWorking_35.CompletedTorque2;
                            dataWorking_NG.CompletedAngle4 = dataWorking_35.CompletedAngle2;
                            dataWorking_NG.CompletedAngle5 = dataWorking_35.CompletedAngle3;
                            dataWorking_NG.CompletedTorque5 = dataWorking_35.CompletedTorque3;
                        }
                        p30DB2000ReturnDatas.Enqueue(dataWorking_NG);
                    }
                }
                if (data != null && data.ReceivedOfflineInstruction)
                {
                    WriteBool("DB1001.DBX0.4", false);
                }
                P30DB3000ReturnData dataWorking = ReadDB3000Data_P30();
                if (dataWorking != null && dataWorking.ReadFlag)
                {
                    dataWorking.Product_BarCode = data.Barcode;

                    List<DeviceInfo> devs =GlobalDevice.sGlobalDevice.GetDeviceInfos();
                    DeviceInfo d35= devs.FindAll(a => a.ParentCode == "P35").FirstOrDefault();
                    if (d35 != null) 
                    {
                        P30DB3000ReturnData dataWorking_35 = d35.s7Comm.ReadDB3000Data_P35();
                        if (dataWorking_35 != null)
                        {
                            dataWorking.CompletedTorque3 = dataWorking_35.CompletedTorque1;
                            dataWorking.CompletedAngle3 = dataWorking_35.CompletedAngle1;
                            dataWorking.CompletedTorque4 = dataWorking_35.CompletedTorque2;
                            dataWorking.CompletedAngle4 = dataWorking_35.CompletedAngle2;
                            dataWorking.CompletedAngle5 = dataWorking_35.CompletedAngle3;
                            dataWorking.CompletedTorque5 = dataWorking_35.CompletedTorque3;
                        }
                    }

                    p30DB2000ReturnDatas.Enqueue(dataWorking);
                    //关闭read
                    P30DB3000ReturnData returnData = new P30DB3000ReturnData();
                    returnData.ReadFlag = false;
                    WriteDB3000Data_readClose_P30(returnData);
                }
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Error, Content = "P30_Action 方法报错了，详细信息：" + ex.Message });
            }
        }
        //P20 站的PLC数据交互
        private void P20_Action()
        {
            try
            {
                P20UploadData data = ReadPlcDataForP20();
                if (data != null && data.WorkpieceOnlineRequest)
                {
                    DataTable dtDet = GlobalBaseData.sGlobalBaseData.GetStationData("P10DB2000ReturnData_Detail", data.Barcode);
                    if (dtDet != null && dtDet.Rows.Count > 0)
                    {
                        DataRow dataRow = dtDet.Rows[0];
                        string isNg = !Convert.IsDBNull(dataRow["IsNG"]) ? dataRow["IsNG"].ToString() : "";
                        if (isNg == "NG")
                        {
                            WriteBool("DB1001.DBX0.1", false);
                            WriteBool("DB1001.DBX0.2", true);
                        }
                        else
                        {
                            WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.Barcode);
                            if (workOrderLinkCode != null)
                            {
                                List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                                string recipeToUse = "";
                                if (order != null && order.Count > 0)
                                {
                                    recipeToUse = order[0].FormulaText;
                                }
                                WriteInt("DB1001.DBW2", GetCommandCode(recipeToUse));
                                WriteBool("DB1001.DBX0.1", true);
                                WriteBool("DB1001.DBX0.2", false);

                                LickMicosOrder("P20DB2000ReturnData_StartWork", data.Barcode);
                            }
                        }
                    }
                    else
                    {
                        WriteBool("DB1001.DBX0.1", false);
                        WriteBool("DB1001.DBX0.2", true);
                    }

                }
                if (data != null && data.StationProcessFinished)
                {
                    WriteBool("DB1001.DBX0.3", true);
                }
                if (data != null && data.ReceivedOnlineInstruction)
                {
                    WriteBool("DB1001.DBX0.1", false);
                    WriteBool("DB1001.DBX0.2", false);
                }
                if (data != null && data.ReceivedFinishInstruction)
                {
                    WriteBool("DB1001.DBX0.3", false);
                }
                if (data != null && data.ManualNGOffline)
                {
                    WriteBool("DB1001.DBX0.4", true);
                    P20DB2000ReturnData dataWorkingNG = ReadDB2000Data_P20();
                    if (dataWorkingNG != null)
                    {
                        dataWorkingNG.Product_BarCode = data.Barcode;
                        dataWorkingNG.IsNG = true;
                        p20DB2000ReturnDatas.Enqueue(dataWorkingNG);
                    }
                }
                if (data != null && data.ReceivedOfflineInstruction)
                {
                    WriteBool("DB1001.DBX0.4", false);
                }
                 
                P20DB2000ReturnData dataWorking = ReadDB2000Data_P20();
                if (dataWorking != null && dataWorking.ReadFlag)
                {
                    dataWorking.Product_BarCode = data.Barcode;
                    p20DB2000ReturnDatas.Enqueue(dataWorking);
                    P20DB2000ReturnData returnData = new P20DB2000ReturnData();
                    returnData.ReadFlag = false;
                    WriteDB2000Data_readClose_P20(returnData);
                }
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType= LoggerType.Error,Content= "P20_Action 方法报错了，详细信息：" + ex.Message});
            }
        }
        private void UpdateCounting_P80()
        {
            WorkOrderStationResultStatus workOrderStationResultStatus = GlobalBaseData.sGlobalBaseData.GetWorkOrderMissionStatus("P80DB3004ReturnData_Detail",
             "P80DB3004ReturnData_StartWork", "P80DB3004ReturnData");
            CsDo(workOrderStationResultStatus);
        }
        private void UpdateCounting_P70()
        {
            //WorkOrderStationResultStatus workOrderStationResultStatus = GlobalBaseData.sGlobalBaseData.GetWorkOrderMissionStatus("P40DB3001ReturnData_Detail",
            //   "P40DB3001ReturnData_StartWork", "P70DB3001ReturnData");
            //Micssd(workOrderStationResultStatus);
        }
        private void UpdateCounting_P60()
        {
            WorkOrderStationResultStatus workOrderStationResultStatus = GlobalBaseData.sGlobalBaseData.GetWorkOrderMissionStatus("P60DB3000ReturnData_Detail",
              "P60DB3000ReturnData_StartWork", "P60DB3000ReturnData");
            CsDo(workOrderStationResultStatus);
        }
        private void UpdateCounting_P50()
        {
            WorkOrderStationResultStatus workOrderStationResultStatus = GlobalBaseData.sGlobalBaseData.GetWorkOrderMissionStatus("P50DB3002ReturnData_Detail",
               "P50DB3002ReturnData_StartWork", "P50DB3002ReturnData");
            var list = new List<DataParameter>
            {
                RealWrite(4, workOrderStationResultStatus.timeClockCount,(ushort)2001),                    // 4.0 生产计时（秒）
                RealWrite(8, workOrderStationResultStatus.timeClockCountPer,(ushort)2001)                      // 8.0 生产节拍（秒）
            };
            Write_readClose_P(list);

            WriteInt("DB2001.DBW12", (short)workOrderStationResultStatus.OkCount);//当日合格数量
            WriteInt("DB2001.DBW14", (short)workOrderStationResultStatus.NgCount); //当日NG数量
            WriteInt("DB2001.DBW16", (short)workOrderStationResultStatus.ProductionCount);//日产量
        }
        private void UpdateCounting_P40()
        {
            WorkOrderStationResultStatus workOrderStationResultStatus = GlobalBaseData.sGlobalBaseData.GetWorkOrderMissionStatus("P40DB3001ReturnData_Detail",
               "P40DB3001ReturnData_StartWork", "P40DB3001ReturnData");
            Micssd(workOrderStationResultStatus);
        }
        private void Micssd(WorkOrderStationResultStatus workOrderStationResultStatus)
        {
            var list = new List<DataParameter>
            {
                RealWrite(4, workOrderStationResultStatus.timeClockCount,(ushort)1501),                    // 4.0 生产计时（秒）
                RealWrite(8, workOrderStationResultStatus.timeClockCountPer,(ushort)1501)                      // 8.0 生产节拍（秒）
            };
            Write_readClose_P(list);

            WriteInt("DB1501.DBW12", (short)workOrderStationResultStatus.OkCount);//当日合格数量
            WriteInt("DB1501.DBW14", (short)workOrderStationResultStatus.NgCount); //当日NG数量
            WriteInt("DB1501.DBW16", (short)workOrderStationResultStatus.ProductionCount);//日产量
        }
        private void UpdateCounting_P35()
        {
            WorkOrderStationResultStatus workOrderStationResultStatus = GlobalBaseData.sGlobalBaseData.GetWorkOrderMissionStatus("P30DB3000ReturnData_Detail",
                "P30DB3000ReturnData_StartWork", "P30DB3000ReturnData");
            Micssd(workOrderStationResultStatus);
        }
        private void UpdateCounting_P30()
        {
            WorkOrderStationResultStatus workOrderStationResultStatus = GlobalBaseData.sGlobalBaseData.GetWorkOrderMissionStatus("P30DB3000ReturnData_Detail",
                "P30DB3000ReturnData_StartWork", "P30DB3000ReturnData");
            CsDo(workOrderStationResultStatus);
        }
        private void UpdateCounting_P20()
        {
            WorkOrderStationResultStatus workOrderStationResultStatus = GlobalBaseData.sGlobalBaseData.GetWorkOrderMissionStatus("P20DB2000ReturnData_Detail", "P20DB2000ReturnData_StartWork", "P20DB2000ReturnData");
            CsDo(workOrderStationResultStatus);
        }
        private void UpdateCounting_P10()
        {
            WorkOrderStationResultStatus workOrderStationResultStatus = GlobalBaseData.sGlobalBaseData.GetWorkOrderMissionStatus("P10DB2000ReturnData_Detail", "P10DB2000ReturnData_StartWork", "P10DB2000ReturnData");
            CsDo(workOrderStationResultStatus);
        }
        private void CsDo(WorkOrderStationResultStatus workOrderStationResultStatus)
        {
            var list = new List<DataParameter>
            {
                RealWrite(4, workOrderStationResultStatus.timeClockCount,(ushort)1001),                    // 4.0 生产计时（秒）
                RealWrite(8, workOrderStationResultStatus.timeClockCountPer,(ushort)1001)                      // 8.0 生产节拍（秒）
            };
            Write_readClose_P(list);

            WriteInt("DB1001.DBW12", (short)workOrderStationResultStatus.OkCount);//当日合格数量
            WriteInt("DB1001.DBW14", (short)workOrderStationResultStatus.NgCount); //当日NG数量
            WriteInt("DB1001.DBW16", (short)workOrderStationResultStatus.ProductionCount);//日产量
        }
        private void P10_Action()
        {
            try
            {
                 
                P10UploadData data = ReadPlcData_P10();
                if (data != null && data.WorkpieceOnlineRequest)
                {
                    WorkOrderLinkCode workOrderLinkCode = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(data.Barcode);
                    if (workOrderLinkCode == null)
                    {
                        WriteBool("DB1001.DBX0.1", false);
                        WriteBool("DB1001.DBX0.2", true);
                    }
                    else
                    {
                        List<WorkOrderModel> order = GlobalBaseData.sGlobalBaseData.GetWorkOrders(workOrderLinkCode.WorkOrderId);
                        string recipeToUse = "";
                        if (order != null && order.Count > 0)
                        {
                            recipeToUse = order[0].FormulaText;
                        }
                        WriteInt("DB1001.DBW2", GetCommandCode(recipeToUse));
                        WriteBool("DB1001.DBX0.1", true);
                        WriteBool("DB1001.DBX0.2", false);

                       
                        LickMicosOrder("P10DB2000ReturnData_StartWork", data.Barcode);
                    }
                }
                if (data != null && data.StationProcessFinished)
                {
                    WriteBool("DB1001.DBX0.3", true);
                }
                if (data != null && data.ReceivedOnlineInstruction)
                {
                    WriteBool("DB1001.DBX0.1", false);
                    WriteBool("DB1001.DBX0.2", false);
                }
                if (data != null && data.ReceivedFinishInstruction)
                {
                    WriteBool("DB1001.DBX0.3", false);
                }
                if (data != null && data.ManualNGOffline)
                {
                    WriteBool("DB1001.DBX0.4", true);
                    P10DB2000ReturnData dataWorkingNG = ReadDB2000Data();
                    if (dataWorkingNG != null)
                    {
                        dataWorkingNG.IsNG = true;
                        p10DB2000ReturnDatas.Enqueue(dataWorkingNG);
                    }
                }
                if (data != null && data.ReceivedOfflineInstruction)
                {
                    WriteBool("DB1001.DBX0.4", false);
                }
                P10DB2000ReturnData dataWorking =ReadDB2000Data();
                if (dataWorking != null && dataWorking.ReadFlag)
                {
                    p10DB2000ReturnDatas.Enqueue(dataWorking);
                    P10DB2000ReturnData returnData = new P10DB2000ReturnData();
                    returnData.ReadFlag = false;
                    WriteDB2000Data_readClose_P10(returnData);
                }
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Error, Content = "P10_Action 方法报错了，详细信息：" + ex.Message });
            }
        }












        public List<DataParameter> ReadDataParameters_ope_P50(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public List<DataParameter> ReadDataParameters_wor_P50(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public List<DataParameter> ReadDataParameters_wor_P60(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public P80DB3004ReturnData ReadDB3001Data_ope_P80()
        {
            try
            {
                var returnData = new P80DB3004ReturnData();
                // 创建所有需要读取的参数
                var parameters = new List<DataParameter>();
                // 添加读取参数
                parameters.Add(BitRead(0, 0,(ushort)DBNumber.P80_wor));                           // 0.0 读取标志位
                // 添加浮点型数据读取参数 (2.0, 6.0, 10.0, 14.0, 18.0, 22.0, 26.0, 30.0, 34.0, 38.0, 42.0, 46.0, 50.0)
                parameters.Add(RealRead(2, (ushort)DBNumber.P80_wor));                   // 2.0 31HZ声压值
                parameters.Add(RealRead(6, (ushort)DBNumber.P80_wor));                   // 6.0 63HZ声压值
                parameters.Add(RealRead(10, (ushort)DBNumber.P80_wor));                 // 10.0 125HZ声压值
                parameters.Add(RealRead(14, (ushort)DBNumber.P80_wor));                 // 14.0 250HZ声压值
                parameters.Add(RealRead(18, (ushort)DBNumber.P80_wor));                 // 18.0 500HZ声压值
                parameters.Add(RealRead(22, (ushort)DBNumber.P80_wor));                // 22.0 1000HZ声压值
                parameters.Add(RealRead(26, (ushort)DBNumber.P80_wor));                // 26.0 2000HZ声压值
                parameters.Add(RealRead(30, (ushort)DBNumber.P80_wor));                // 30.0 4000HZ声压值
                parameters.Add(RealRead(34, (ushort)DBNumber.P80_wor));                // 34.0 8000HZ声压值
                parameters.Add(RealRead(38, (ushort)DBNumber.P80_wor));               // 38.0 16000HZ声压值
                parameters.Add(RealRead(42, (ushort)DBNumber.P80_wor));                   // 42.0 APC声压值
                parameters.Add(RealRead(46, (ushort)DBNumber.P80_wor));                   // 46.0 APA声压值
                parameters.Add(RealRead(50, (ushort)DBNumber.P80_wor));                 // 50.0 APLIN声压值

                // 执行读取
                var result = ReadDataParameters_wor_P80(parameters);
                // 解析读取结果
                int index = 0;
                returnData.ReadFlag = (bool)parameters[index++].Datas[0];                           // 0.0 读取标志位
                // 解析浮点型数据 (2.0, 6.0, 10.0, 14.0, 18.0, 22.0, 26.0, 30.0, 34.0, 38.0, 42.0, 46.0, 50.0)
                returnData.SoundPressure31Hz = (float)parameters[index++].Datas[0];                   // 2.0 31HZ声压值
                returnData.SoundPressure63Hz = (float)parameters[index++].Datas[0];                   // 6.0 63HZ声压值
                returnData.SoundPressure125Hz = (float)parameters[index++].Datas[0];                 // 10.0 125HZ声压值
                returnData.SoundPressure250Hz = (float)parameters[index++].Datas[0];                 // 14.0 250HZ声压值
                returnData.SoundPressure500Hz = (float)parameters[index++].Datas[0];                 // 18.0 500HZ声压值
                returnData.SoundPressure1000Hz = (float)parameters[index++].Datas[0];                // 22.0 1000HZ声压值
                returnData.SoundPressure2000Hz = (float)parameters[index++].Datas[0];                // 26.0 2000HZ声压值
                returnData.SoundPressure4000Hz = (float)parameters[index++].Datas[0];                // 30.0 4000HZ声压值
                returnData.SoundPressure8000Hz = (float)parameters[index++].Datas[0];                // 34.0 8000HZ声压值
                returnData.SoundPressure16000Hz = (float)parameters[index++].Datas[0];               // 38.0 16000HZ声压值
                returnData.SoundPressureAPC = (float)parameters[index++].Datas[0];                   // 42.0 APC声压值
                returnData.SoundPressureAPA = (float)parameters[index++].Datas[0];                   // 46.0 APA声压值
                returnData.SoundPressureAPLIN = (float)parameters[index++].Datas[0];                 // 50.0 APLIN声压值
                                                                                                     //读取主条码 DB1000.6 长度50
                var parameters3 = new List<DataParameter>();
                parameters3.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 1000,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 8,
                    Count = 48,
                    DataType = typeof(string)
                });
                var result3 = ReadDataParameters_wor_P80(parameters3);
                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取DB3004数据失败: {ex.Message}", ex);
            }
        }
        public List<DataParameter> ReadDataParameters_wor_P80(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public P70DB3003ReturnData ReadDB3001Data_wor_P70()
        {
            try
            {
                var returnData = new P70DB3003ReturnData();
                // 第一次读取：读取前半部分数据 (0.0, 0.1, 14.0, 2.0, 6.0, 10.0, 16.0, 20.0, 24.0)
                var parameters1 = new List<DataParameter>();

                // 添加读取参数
                parameters1.Add(BitRead(0, 0,(ushort)DBNumber.P70_wor));                          // 0.0 读取标志位1
                parameters1.Add(BitRead(0, 1, (ushort)DBNumber.P70_wor));                // 0.1 标定结果1
                parameters1.Add(BitRead(14, 0, (ushort)DBNumber.P70_wor));                        // 14.0 读取标志位2

                // 添加前半部分浮点型数据读取参数 (2.0, 6.0, 10.0, 16.0, 20.0, 24.0)
                parameters1.Add(RealRead(2, (ushort)DBNumber.P70_wor));                           // 2.0 最大长度1
                parameters1.Add(RealRead(6, (ushort)DBNumber.P70_wor));                     // 6.0 AMPLITUDE值1
                parameters1.Add(RealRead(10, (ushort)DBNumber.P70_wor));                       // 10.0 MAG_LOW值1
                parameters1.Add(RealRead(16, (ushort)DBNumber.P70_wor));              // 16.0 右老化最大电压2
                parameters1.Add(RealRead(20, (ushort)DBNumber.P70_wor));              // 20.0 右老化最小电压2
                parameters1.Add(RealRead(24, (ushort)DBNumber.P70_wor));              // 24.0 右老化平均电压2

                // 执行第一次读取
                var result1 = ReadDataParameters_Wor_P70(parameters1);
                // 解析第一次读取结果
                int index1 = 0;
                returnData.ReadFlag1 = (bool)parameters1[index1++].Datas[0];                          // 0.0 读取标志位1
                returnData.CalibrationResult1 = (bool)parameters1[index1++].Datas[0];                // 0.1 标定结果1
                returnData.ReadFlag2 = (bool)parameters1[index1++].Datas[0];                        // 14.0 读取标志位2

                // 解析前半部分浮点型数据 (2.0, 6.0, 10.0, 16.0, 20.0, 24.0)
                returnData.MaxLength1 = (float)parameters1[index1++].Datas[0];                           // 2.0 最大长度1
                returnData.AmplitudeValue1 = (float)parameters1[index1++].Datas[0];                     // 6.0 AMPLITUDE值1
                returnData.MagLowValue1 = (float)parameters1[index1++].Datas[0];                       // 10.0 MAG_LOW值1
                returnData.RightAgingMaxVoltage2 = (float)parameters1[index1++].Datas[0];              // 16.0 右老化最大电压2
                returnData.RightAgingMinVoltage2 = (float)parameters1[index1++].Datas[0];              // 20.0 右老化最小电压2
                returnData.RightAgingAvgVoltage2 = (float)parameters1[index1++].Datas[0];              // 24.0 右老化平均电压2

                // 第二次读取：读取后半部分数据 (40.0, 66.0, 28.0, 32.0, 36.0, 42.0, 46.0, 50.0, 54.0, 58.0, 62.0, 68.0, 72.0, 76.0, 80.0, 84.0, 88.0)
                var parameters2 = new List<DataParameter>();

                // 添加读取参数
                parameters2.Add(BitRead(40, 0, (ushort)DBNumber.P70_wor));                        // 40.0 读取标志位3
                parameters2.Add(BitRead(66, 0, (ushort)DBNumber.P70_wor));                        // 66.0 读取标志位4

                // 添加后半部分浮点型数据读取参数 (28.0, 32.0, 36.0, 42.0, 46.0, 50.0, 54.0, 58.0, 62.0, 68.0, 72.0, 76.0, 80.0, 84.0, 88.0)
                parameters2.Add(RealRead(28, (ushort)DBNumber.P70_wor));              // 28.0 右老化最大电流2
                parameters2.Add(RealRead(32, (ushort)DBNumber.P70_wor));              // 32.0 右老化最小电流2
                parameters2.Add(RealRead(36, (ushort)DBNumber.P70_wor));              // 36.0 右老化平均电流2
                parameters2.Add(RealRead(42, (ushort)DBNumber.P70_wor));             // 42.0 中老化最大电压3
                parameters2.Add(RealRead(46, (ushort)DBNumber.P70_wor));             // 46.0 中老化最小电压3
                parameters2.Add(RealRead(50, (ushort)DBNumber.P70_wor));             // 50.0 中老化平均电压3
                parameters2.Add(RealRead(54, (ushort)DBNumber.P70_wor));             // 54.0 中老化最大电流3
                parameters2.Add(RealRead(58, (ushort)DBNumber.P70_wor));             // 58.0 中老化最小电流3
                parameters2.Add(RealRead(62, (ushort)DBNumber.P70_wor));             // 62.0 中老化平均电流3
                parameters2.Add(RealRead(68, (ushort)DBNumber.P70_wor));               // 68.0 左老化最大电压4
                parameters2.Add(RealRead(72, (ushort)DBNumber.P70_wor));               // 72.0 左老化最小电压4
                parameters2.Add(RealRead(76, (ushort)DBNumber.P70_wor));               // 76.0 左老化平均电压4
                parameters2.Add(RealRead(80, (ushort)DBNumber.P70_wor));               // 80.0 左老化最大电流4
                parameters2.Add(RealRead(84, (ushort)DBNumber.P70_wor));               // 84.0 左老化最小电流4
                parameters2.Add(RealRead(88, (ushort)DBNumber.P70_wor));               // 88.0 左老化平均电流4

                // 执行第二次读取
                var result2 = ReadDataParameters_Wor_P70(parameters2);
                // 解析第二次读取结果
                int index2 = 0;
                returnData.ReadFlag3 = (bool)parameters2[index2++].Datas[0];                        // 40.0 读取标志位3
                returnData.ReadFlag4 = (bool)parameters2[index2++].Datas[0];                        // 66.0 读取标志位4

                // 解析后半部分浮点型数据 (28.0, 32.0, 36.0, 42.0, 46.0, 50.0, 54.0, 58.0, 62.0, 68.0, 72.0, 76.0, 80.0, 84.0, 88.0)
                returnData.RightAgingMaxCurrent2 = (float)parameters2[index2++].Datas[0];              // 28.0 右老化最大电流2
                returnData.RightAgingMinCurrent2 = (float)parameters2[index2++].Datas[0];              // 32.0 右老化最小电流2
                returnData.RightAgingAvgCurrent2 = (float)parameters2[index2++].Datas[0];              // 36.0 右老化平均电流2
                returnData.MiddleAgingMaxVoltage3 = (float)parameters2[index2++].Datas[0];             // 42.0 中老化最大电压3
                returnData.MiddleAgingMinVoltage3 = (float)parameters2[index2++].Datas[0];             // 46.0 中老化最小电压3
                returnData.MiddleAgingAvgVoltage3 = (float)parameters2[index2++].Datas[0];             // 50.0 中老化平均电压3
                returnData.MiddleAgingMaxCurrent3 = (float)parameters2[index2++].Datas[0];             // 54.0 中老化最大电流3
                returnData.MiddleAgingMinCurrent3 = (float)parameters2[index2++].Datas[0];             // 58.0 中老化最小电流3
                returnData.MiddleAgingAvgCurrent3 = (float)parameters2[index2++].Datas[0];             // 62.0 中老化平均电流3
                returnData.LeftAgingMaxVoltage4 = (float)parameters2[index2++].Datas[0];               // 68.0 左老化最大电压4
                returnData.LeftAgingMinVoltage4 = (float)parameters2[index2++].Datas[0];               // 72.0 左老化最小电压4
                returnData.LeftAgingAvgVoltage4 = (float)parameters2[index2++].Datas[0];               // 76.0 左老化平均电压4
                returnData.LeftAgingMaxCurrent4 = (float)parameters2[index2++].Datas[0];               // 80.0 左老化最大电流4
                returnData.LeftAgingMinCurrent4 = (float)parameters2[index2++].Datas[0];               // 84.0 左老化最小电流4
                returnData.LeftAgingAvgCurrent4 = (float)parameters2[index2++].Datas[0];               // 88.0 左老化平均电流4
                                                                                                       //读取主条码 DB1000.6 长度50
                var parameters3 = new List<DataParameter>();
                parameters3.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 1500,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 8,
                    Count = 48,
                    DataType = typeof(string)
                });
                var result3 = ReadDataParameters_Wor_P70(parameters3);
                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取DB3003数据失败: {ex.Message}", ex);
            }
        }
        public List<DataParameter> ReadDataParameters_Wor_P70(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public P60DB3000ReturnData ReadDB3000Data_wor_P60()
        {
            try
            {
                var returnData = new P60DB3000ReturnData();
                // 创建所有需要读取的参数
                var parameters = new List<DataParameter>();

                // 添加读取参数
                parameters.Add(BitRead(0, 0,(ushort)DBNumber.P60_wor));              // 0.0 读取标志位

                // 添加字符串数据读取参数 (2.0, 20.0)
                parameters.Add(StringRead(4, 16, (ushort)DBNumber.P60_wor));         // 2.0 时间日期
                parameters.Add(StringRead(22, 8, (ushort)DBNumber.P60_wor));         // 20.0 泄露率

                // 执行读取
                var result = ReadDataParameters_wor_P60(parameters);
                // 解析读取结果
                int index = 0;
                returnData.ReadFlag = (bool)parameters[index++].Datas[0];              // 0.0 读取标志位

                // 解析字符串数据 (2.0, 20.0)
                returnData.TimeDate = (string)parameters[index++].Datas[0];         // 2.0 时间日期
                returnData.LeakRate = (string)parameters[index++].Datas[0];         // 20.0 泄露率
                                                                                    //读取主条码 DB1000.6 长度50
                var parameters3 = new List<DataParameter>();
                parameters3.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 1000,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 8,
                    Count = 48,
                    DataType = typeof(string)
                });

                parameters3.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 1000,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 114,
                    Count = 8,
                    DataType = typeof(string)
                });
                var result3 = ReadDataParameters_wor_P60(parameters3);
                //读取成功
                returnData.Product_BarCode = ((string)parameters3[0].Datas[0]).TrimEnd('\r','\n','\0');
                returnData.LeakRate = (string)parameters3[1].Datas[0];         // 20.0 泄露率

                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取DB3000数据失败: {ex.Message}", ex);
            }
        }
        public P50DB3002ReturnData ReadDB3002Data_workData_P50()//zhan50站磁铁安装获取反馈数据
        {
            try
            {
                var returnData = new P50DB3002ReturnData();
                // 第一次读取：读取布尔信号和前半部分数据 (0.0, 2.0, 10.0, 18.0, 26.0, 34.0, 6.0, 14.0, 22.0, 30.0)
                var parameters1 = new List<DataParameter>();

                // 添加读取参数
                parameters1.Add(BitRead(0, 0,(ushort)DBNumber.P50_wor));                           // 0.0 读取标志位

                // 添加前半部分浮点型数据读取参数 (2.0, 10.0, 18.0, 26.0, 34.0)
                parameters1.Add(RealRead(2, (ushort)DBNumber.P50_wor));                    // 2.0 完成扭矩1
                parameters1.Add(RealRead(10, (ushort)DBNumber.P50_wor));                   // 10.0 完成扭矩2
                parameters1.Add(RealRead(18, (ushort)DBNumber.P50_wor));                   // 18.0 完成扭矩3
                parameters1.Add(RealRead(26, (ushort)DBNumber.P50_wor));                   // 26.0 完成扭矩4
                parameters1.Add(RealRead(34, (ushort)DBNumber.P50_wor));                   // 34.0 完成扭矩5

                // 添加前半部分双整型数据读取参数 (6.0, 14.0, 22.0, 30.0)
                parameters1.Add(DIntRead(6, (ushort)DBNumber.P50_wor));                     // 6.0 完成角度1
                parameters1.Add(DIntRead(14, (ushort)DBNumber.P50_wor));                    // 14.0 完成角度2
                parameters1.Add(DIntRead(22, (ushort)DBNumber.P50_wor));                    // 22.0 完成角度3
                parameters1.Add(DIntRead(30, (ushort)DBNumber.P50_wor));                    // 30.0 完成角度4

                // 执行第一次读取
                var result1 = ReadDataParameters_wor_P50(parameters1);
                // 解析第一次读取结果
                returnData.ReadFlag = (bool)parameters1[0].Datas[0];                           // 0.0 读取标志位

                // 解析前半部分浮点型数据 (2.0, 10.0, 18.0, 26.0, 34.0)
                returnData.CompletedTorque1 = (float)parameters1[1].Datas[0];                    // 2.0 完成扭矩1
                returnData.CompletedTorque2 = (float)parameters1[2].Datas[0];                   // 10.0 完成扭矩2
                returnData.CompletedTorque3 = (float)parameters1[3].Datas[0];                   // 18.0 完成扭矩3
                returnData.CompletedTorque4 = (float)parameters1[4].Datas[0];                   // 26.0 完成扭矩4
                returnData.CompletedTorque5 = (float)parameters1[5].Datas[0];                   // 34.0 完成扭矩5

                // 解析前半部分双整型数据 (6.0, 14.0, 22.0, 30.0)
                returnData.CompletedAngle1 = (int)parameters1[6].Datas[0];                     // 6.0 完成角度1
                returnData.CompletedAngle2 = (int)parameters1[7].Datas[0];                    // 14.0 完成角度2
                returnData.CompletedAngle3 = (int)parameters1[8].Datas[0];                    // 22.0 完成角度3
                returnData.CompletedAngle4 = (int)parameters1[9].Datas[0];                    // 30.0 完成角度4

                // 第二次读取：读取后半部分数据 (42.0, 50.0, 58.0, 66.0, 38.0, 46.0, 54.0, 62.0, 70.0)
                var parameters2 = new List<DataParameter>();

                // 添加后半部分浮点型数据读取参数 (42.0, 50.0, 58.0, 66.0)
                parameters2.Add(RealRead(42, (ushort)DBNumber.P50_wor));                   // 42.0 完成扭矩6
                parameters2.Add(RealRead(50, (ushort)DBNumber.P50_wor));                   // 50.0 完成扭矩7
                parameters2.Add(RealRead(58, (ushort)DBNumber.P50_wor));                   // 58.0 完成扭矩8
                parameters2.Add(RealRead(66, (ushort)DBNumber.P50_wor));                   // 66.0 完成扭矩9

                // 添加后半部分双整型数据读取参数 (38.0, 46.0, 54.0, 62.0, 70.0)
                parameters2.Add(DIntRead(38, (ushort)DBNumber.P50_wor));                    // 38.0 完成角度5
                parameters2.Add(DIntRead(46, (ushort)DBNumber.P50_wor));                    // 46.0 完成角度6
                parameters2.Add(DIntRead(54, (ushort)DBNumber.P50_wor));                    // 54.0 完成角度7
                parameters2.Add(DIntRead(62, (ushort)DBNumber.P50_wor));                    // 62.0 完成角度8
                parameters2.Add(DIntRead(70, (ushort)DBNumber.P50_wor));                    // 70.0 完成角度9

                // 执行第二次读取
                var result2 = ReadDataParameters_wor_P50(parameters2);

                // 解析后半部分浮点型数据 (42.0, 50.0, 58.0, 66.0)
                returnData.CompletedTorque6 = (float)parameters2[0].Datas[0];                   // 42.0 完成扭矩6
                returnData.CompletedTorque7 = (float)parameters2[1].Datas[0];                   // 50.0 完成扭矩7
                returnData.CompletedTorque8 = (float)parameters2[2].Datas[0];                   // 58.0 完成扭矩8
                returnData.CompletedTorque9 = (float)parameters2[3].Datas[0];                   // 66.0 完成扭矩9

                returnData.CompletedTorque8 = 0;                   // 58.0 完成扭矩8
                returnData.CompletedTorque9 = 0;                   // 66.0 完成扭矩9

                // 解析后半部分双整型数据 (38.0, 46.0, 54.0, 62.0, 70.0)
                returnData.CompletedAngle5 = (int)parameters2[4].Datas[0];                    // 38.0 完成角度5
                returnData.CompletedAngle6 = (int)parameters2[5].Datas[0];                    // 46.0 完成角度6
                returnData.CompletedAngle7 = (int)parameters2[6].Datas[0];                    // 54.0 完成角度7
                returnData.CompletedAngle8 = (int)parameters2[7].Datas[0];                    // 62.0 完成角度8
                returnData.CompletedAngle9 = (int)parameters2[8].Datas[0];                    // 70.0 完成角度9

                returnData.CompletedAngle8 = 0;                    // 62.0 完成角度8
                returnData.CompletedAngle9 = 0;                    // 70.0 完成角度9
                                                                   //读取主条码 DB1000.6 长度50
                var parameters3 = new List<DataParameter>();
                parameters3.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 2000,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 8,
                    Count = 48,
                    DataType = typeof(string)
                });
                var result3 = ReadDataParameters_wor_P50(parameters3);

                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取DB3002数据失败: {ex.Message}", ex);
            }
        }
        public P80UploadData ReadPlcData_ope_P80()
        {
            // 创建所有需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取布尔信号 (0.0~1.0)
            for (int bit = 0; bit <= 7; bit++)
            {
                parameters.Add(BitRead(0, (byte)bit,(ushort)DBNumber.P80_ope));
            }
            parameters.Add(BitRead(1, 0, (ushort)DBNumber.P80_ope)); // 1.0

            // 读取整型数据 (2.0, 4.0)
            parameters.Add(WordRead(2, (ushort)DBNumber.P80_ope)); // 2.0 工站状态
            parameters.Add(WordRead(4, (ushort)DBNumber.P80_ope)); // 4.0 报警字

            // 读取字符串数据 (6.0)
            parameters.Add(StringRead(8, 48, (ushort)DBNumber.P80_ope)); // 6.0 工件条码信息

            // 读取浮点型数据 (58.0, 62.0)
            parameters.Add(RealRead(58, (ushort)DBNumber.P80_ope)); // 58.0 电压数据
            parameters.Add(RealRead(62, (ushort)DBNumber.P80_ope)); // 62.0 电流数据

            // 执行读取
            var result = ReadDataParameters_ope_P80(parameters);
            // 解析数据
            return new P80UploadData
            {
                // 布尔信号 (0.0~1.0)
                Heartbeat = (bool)parameters[0].Datas[0],                    // 0.0 心跳
                WorkpieceOnlineRequest = (bool)parameters[1].Datas[0],       // 0.1 工件上线申请
                ReceivedOnlineInstruction = (bool)parameters[2].Datas[0],    // 0.2 收到上线指令
                StationProcessFinished = (bool)parameters[3].Datas[0],       // 0.3 工站流程结束
                ReceivedFinishInstruction = (bool)parameters[4].Datas[0],    // 0.4 收到结束指令
                ManualNGOffline = (bool)parameters[5].Datas[0],              // 0.5 产品手动NG下线
                ReceivedOfflineInstruction = (bool)parameters[6].Datas[0],   // 0.6 收到下线指令
                NoisePowerStarted = (bool)parameters[7].Datas[0],            // 0.7 噪音电源开始工作
                StationWorkingStarted = (bool)parameters[8].Datas[0],        // 1.0 工站开始工作

                // 整型数据 (2.0~4.0)
                StationStatus = (short)parameters[9].Datas[0],               // 2.0 工站状态
                AlarmWord = (short)parameters[10].Datas[0],                  // 4.0 报警字

                // 字符串数据 (6.0)
                Barcode = ((string)parameters[11].Datas[0]).TrimEnd('\r','\n','\0'),                   // 6.0 工件条码信息

                // 浮点型数据 (58.0, 62.0)
                Voltage = (float)parameters[12].Datas[0],                    // 58.0 电压数据
                Current = (float)parameters[13].Datas[0]                     // 62.0 电流数据
            };
        }
        public List<DataParameter> ReadDataParameters_ope_P80(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public P70UploadData ReadPlcData_ope_P70()
        {
            // 创建数据对象
            var data = new P70UploadData();

            // 分6次读取数据
            ReadPlcDataBatch1(data);
            ReadPlcDataBatch2(data);
            ReadPlcDataBatch3(data);
            ReadPlcDataBatch4(data);
            ReadPlcDataBatch5(data);
            ReadPlcDataBatch6(data);

            // 设置信号状态字
            data.SignalStatus = (ushort)(
                ((data.CalibrationWorkpieceOnlineRequest ? 1 : 0) << 1) |
                ((data.CalibrationReceivedOnlineCommand ? 1 : 0) << 2) |
                ((data.CalibrationWorkstationProcessEnd ? 1 : 0) << 3) |
                ((data.CalibrationReceivedEndCommand ? 1 : 0) << 4) |
                ((data.CalibrationProductNGDownline ? 1 : 0) << 5) |
                ((data.CalibrationReceivedDownlineCommand ? 1 : 0) << 6)
            );

            return data;
        }
        private void ReadPlcDataBatch2(P70UploadData data)
        {
            // 创建需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取标定相关数据
            parameters.Add(BitRead(0, 7, (ushort)DBNumber.P70_ope)); // 0.7 标定电源开始工作
            parameters.Add(BitRead(1, 0, (ushort)DBNumber.P70_ope)); // 1.0 标定工站开始工作
            parameters.Add(WordRead(2, (ushort)DBNumber.P70_ope));   // 2.0 标定工站状态
            parameters.Add(WordRead(4, (ushort)DBNumber.P70_ope));   // 4.0 标定报警字
            parameters.Add(StringRead(8, 48, (ushort)DBNumber.P70_ope)); // 6.0 标定工件条码信息

            // 执行读取
            var result = ReadDataParameters_ReadPlcDataBatch_P70(parameters);
            // 解析数据
            data.CalibrationPowerStarted = (bool)parameters[0].Datas[0];
            data.CalibrationStationWorkingStarted = (bool)parameters[1].Datas[0];
            data.CalibrationStationStatus = (short)parameters[2].Datas[0];
            data.CalibrationAlarmWord = (short)parameters[3].Datas[0];
            data.CalibrationBarcode = ((string)parameters[4].Datas[0]).TrimEnd('\r', '\n', '\0');
        }

        /// <summary>
        /// 分批读取PLC数据 - 第3批
        /// </summary>
        /// <param name="data">数据对象</param>
        private void ReadPlcDataBatch3(P70UploadData data)
        {
            // 创建需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取标定位置和参数
            parameters.Add(BitRead(58, 0, (ushort)DBNumber.P70_ope)); // 58.0 申请启动
            parameters.Add(WordRead(60, (ushort)DBNumber.P70_ope));  // 60.0 标定位置
            parameters.Add(RealRead(62, (ushort)DBNumber.P70_ope));  // 62.0 总行程长度
            parameters.Add(RealRead(66, (ushort)DBNumber.P70_ope));  // 66.0 标定站电压
            parameters.Add(RealRead(70, (ushort)DBNumber.P70_ope));  // 70.0 标定站电流

            // 执行读取
            var result = ReadDataParameters_ReadPlcDataBatch_P70(parameters);
            // 解析数据
            data.CalibrationStartRequest = (bool)parameters[0].Datas[0];
            data.CalibrationPosition = (short)parameters[1].Datas[0];
            data.TotalStrokeLength = (float)parameters[2].Datas[0];
            data.CalibrationVoltage = (float)parameters[3].Datas[0];
            data.CalibrationCurrent = (float)parameters[4].Datas[0];

        }

        /// <summary>
        /// 分批读取PLC数据 - 第4批
        /// </summary>
        /// <param name="data">数据对象</param>
        private void ReadPlcDataBatch4(P70UploadData data)
        {
            // 创建需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取老化2相关数据 (74.0-90.0)
            for (int bit = 0; bit <= 7; bit++)
            {
                parameters.Add(BitRead(74, (byte)bit, (ushort)DBNumber.P70_ope));
            }
            parameters.Add(WordRead(76, (ushort)DBNumber.P70_ope));  // 76.0 老化2运行次数
            parameters.Add(RealRead(78, (ushort)DBNumber.P70_ope));  // 78.0 老化2运行时间
            parameters.Add(RealRead(82, (ushort)DBNumber.P70_ope));  // 82.0 老化2电压
            parameters.Add(RealRead(86, (ushort)DBNumber.P70_ope));  // 86.0 老化2电流
            parameters.Add(StringRead(90, 50, (ushort)DBNumber.P70_ope)); // 6.0 标定工件条码信息

            // 执行读取
            var result = ReadDataParameters_ReadPlcDataBatch_P70(parameters);

            data.AgingOnlineRequest2 = (bool)parameters[0].Datas[0];
            data.AgingReceivedOnlineCommand2 = (bool)parameters[1].Datas[0];
            data.AgingProcessEnded2 = (bool)parameters[2].Datas[0];
            data.AgingReceivedEndCommand2 = (bool)parameters[3].Datas[0];
            data.AgingManualNgOffline2 = (bool)parameters[4].Datas[0];
            data.AgingReceivedOfflineCommand2 = (bool)parameters[5].Datas[0];
            data.AgingPowerStarted2 = (bool)parameters[6].Datas[0];
            data.AgingStationWorkingStarted2 = (bool)parameters[7].Datas[0];
            data.AgingRunCount2 = (short)parameters[8].Datas[0];
            data.AgingRunTime2 = (float)parameters[9].Datas[0];
            data.AgingVoltage2 = (float)parameters[10].Datas[0];
            data.AgingCurrent2 = (float)parameters[11].Datas[0];
            data.AgingBarcode2 = ((string)parameters[12].Datas[0]).TrimEnd('\r','\n','\0');
        }

        /// <summary>
        /// 分批读取PLC数据 - 第5批
        /// </summary>
        /// <param name="data">数据对象</param>
        private void ReadPlcDataBatch5(P70UploadData data)
        {
            // 创建需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取老化3相关数据 (142.0-158.0)
            for (int bit = 0; bit <= 7; bit++)
            {
                parameters.Add(BitRead(142, (byte)bit, (ushort)DBNumber.P70_ope));
            }
            parameters.Add(WordRead(144, (ushort)DBNumber.P70_ope)); // 144.0 老化3运行次数
            parameters.Add(RealRead(146, (ushort)DBNumber.P70_ope)); // 146.0 老化3运行时间
            parameters.Add(RealRead(150, (ushort)DBNumber.P70_ope)); // 150.0 老化3电压
            parameters.Add(RealRead(154, (ushort)DBNumber.P70_ope)); // 154.0 老化3电流

            // 执行读取
            var result = ReadDataParameters_ReadPlcDataBatch_P70(parameters);
            // 解析数据
            data.AgingOnlineRequest3 = (bool)parameters[0].Datas[0];
            data.AgingReceivedOnlineCommand3 = (bool)parameters[1].Datas[0];
            data.AgingProcessEnded3 = (bool)parameters[2].Datas[0];
            data.AgingReceivedEndCommand3 = (bool)parameters[3].Datas[0];
            data.AgingManualNgOffline3 = (bool)parameters[4].Datas[0];
            data.AgingReceivedOfflineCommand3 = (bool)parameters[5].Datas[0];
            data.AgingPowerStarted3 = (bool)parameters[6].Datas[0];
            data.AgingStationWorkingStarted3 = (bool)parameters[7].Datas[0];
            data.AgingRunCount3 = (short)parameters[8].Datas[0];
            data.AgingRunTime3 = (float)parameters[9].Datas[0];
            data.AgingVoltage3 = (float)parameters[10].Datas[0];
            data.AgingCurrent3 = (float)parameters[11].Datas[0];
        }

        /// <summary>
        /// 分批读取PLC数据 - 第6批
        /// </summary>
        /// <param name="data">数据对象</param>
        private void ReadPlcDataBatch6(P70UploadData data)
        {
            // 创建需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取老化4相关数据(210.0-226.0)
            for (int bit = 0; bit <= 7; bit++)
            {
                parameters.Add(BitRead(210, (byte)bit, (ushort)DBNumber.P70_ope));
            }
            parameters.Add(WordRead(212, (ushort)DBNumber.P70_ope)); // 212.0 老化4运行次数
            parameters.Add(RealRead(214, (ushort)DBNumber.P70_ope)); // 214.0 老化4运行时间
            parameters.Add(RealRead(218, (ushort)DBNumber.P70_ope)); // 218.0 老化4电压
            parameters.Add(RealRead(222, (ushort)DBNumber.P70_ope)); // 222.0 老化4电流
            parameters.Add(StringRead(226, 50, (ushort)DBNumber.P70_ope)); // 226.0 老化4条码

            parameters.Add(BitRead(294, (byte)0,(ushort)DBNumber.P70_ope)); // 294.0 负载测试开始通知


            // 执行读取
            var result = ReadDataParameters_ReadPlcDataBatch_P70(parameters);
            // 解析数据
            data.AgingOnlineRequest4 = (bool)parameters[0].Datas[0];
            data.AgingReceivedOnlineCommand4 = (bool)parameters[1].Datas[0];
            data.AgingProcessEnded4 = (bool)parameters[2].Datas[0];
            data.AgingReceivedEndCommand4 = (bool)parameters[3].Datas[0];
            data.AgingManualNgOffline4 = (bool)parameters[4].Datas[0];
            data.AgingReceivedOfflineCommand4 = (bool)parameters[5].Datas[0];
            data.AgingPowerStarted4 = (bool)parameters[6].Datas[0];
            data.AgingStationWorkingStarted4 = (bool)parameters[7].Datas[0];
            data.AgingRunCount4 = (short)parameters[8].Datas[0];
            data.AgingRunTime4 = (float)parameters[9].Datas[0];
            data.AgingVoltage4 = (float)parameters[10].Datas[0];
            data.AgingCurrent4 = (float)parameters[11].Datas[0];
            data.AgingBarcode4 = ((string)parameters[12].Datas[0]).TrimEnd('\r', '\n', '\0');

            data.LoadTestNotification = (bool)parameters[13].Datas[0];
        }
        private void ReadPlcDataBatch1(P70UploadData data)
        {
            // 创建需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取基本信号 (0.0~1.0)
            parameters.Add(BitRead(0, 0,(ushort)DBNumber.P70_ope)); // 0.0 心跳
            parameters.Add(BitRead(0, 1, (ushort)DBNumber.P70_ope)); // 0.1 工件上线申请
            parameters.Add(BitRead(0, 2, (ushort)DBNumber.P70_ope)); // 0.2 收到上线指令
            parameters.Add(BitRead(0, 3, (ushort)DBNumber.P70_ope)); // 0.3 工站流程结束
            parameters.Add(BitRead(0, 4, (ushort)DBNumber.P70_ope)); // 0.4 收到结束指令
            parameters.Add(BitRead(0, 5, (ushort)DBNumber.P70_ope)); // 0.5 产品NG下线
            parameters.Add(BitRead(0, 6, (ushort)DBNumber.P70_ope)); // 0.6 收到下线指令
            // 执行读取
            var result = ReadDataParameters_ReadPlcDataBatch_P70(parameters);
            // 解析数据
            data.Heartbeat = (bool)parameters[0].Datas[0];
            data.WorkpieceOnlineRequest = (bool)parameters[1].Datas[0];
            data.ReceivedOnlineInstruction = (bool)parameters[2].Datas[0];
            data.StationProcessFinished = (bool)parameters[3].Datas[0];
            data.ReceivedFinishInstruction = (bool)parameters[4].Datas[0];
            data.ManualNGOffline = (bool)parameters[5].Datas[0];
            data.ReceivedOfflineInstruction = (bool)parameters[6].Datas[0];
        }
        public List<DataParameter> ReadDataParameters_ReadPlcDataBatch_P70(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public P60UploadData ReadPlcData_ope_P60()
        {
            // 创建所有需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取布尔信号 (0.0~1.0)
            // 0.0 心跳
            parameters.Add(BitRead(0, 0,(ushort)DBNumber.P60_ope));
            // 0.1 工件上线申请
            parameters.Add(BitRead(0, 1, (ushort)DBNumber.P60_ope));
            // 0.2 收到上线指令
            parameters.Add(BitRead(0, 2, (ushort)DBNumber.P60_ope));
            // 0.3 工站流程结束
            parameters.Add(BitRead(0, 3, (ushort)DBNumber.P60_ope));
            // 0.4 收到结束指令
            parameters.Add(BitRead(0, 4, (ushort)DBNumber.P60_ope));
            // 0.5 产品NG下线
            parameters.Add(BitRead(0, 5, (ushort)DBNumber.P60_ope));
            // 0.6 收到下线指令
            parameters.Add(BitRead(0, 6, (ushort)DBNumber.P60_ope));
            // 0.7 气密仪开始工作
            parameters.Add(BitRead(0, 7, (ushort)DBNumber.P60_ope));
            // 1.0 工站开始工作
            parameters.Add(BitRead(1, 0, (ushort)DBNumber.P60_ope));

            // 读取整型数据 (2.0, 4.0)
            parameters.Add(WordRead(2, (ushort)DBNumber.P60_ope)); // 2.0 工站状态 (布尔类型)
            parameters.Add(WordRead(4, (ushort)DBNumber.P60_ope)); // 4.0 报警字

            // 读取整型数据 (6.0)
            parameters.Add(StringRead(6, 50, (ushort)DBNumber.P60_ope)); // 6.0 工件条码信息
            // 执行读取
            var result = ReadDataParameters_ope_P60(parameters);
            // 解析数据
            return new P60UploadData
            {
                // 布尔信号 (0.0~1.0)
                Heartbeat = (bool)parameters[0].Datas[0],                    // 0.0 心跳
                WorkpieceOnlineRequest = (bool)parameters[1].Datas[0],       // 0.1 工件上线申请
                ReceivedOnlineInstruction = (bool)parameters[2].Datas[0],    // 0.2 收到上线指令
                StationProcessFinished = (bool)parameters[3].Datas[0],    // 0.3
                ReceivedFinishInstruction = (bool)parameters[4].Datas[0],    // 0.4 收到结束指令
                ManualNGOffline = (bool)parameters[5].Datas[0],              // 0.5 产品NG下线
                ReceivedOfflineInstruction = (bool)parameters[6].Datas[0],   // 0.6 收到下线指令
                LeakTesterStarted = (bool)parameters[7].Datas[0],            // 0.7 气密仪开始工作
                StationWorkingStarted = (bool)parameters[8].Datas[0],        // 1.0 工站开始工作

                // 整型数据 (2.0~4.0)
                StationStatus = (short)parameters[9].Datas[0],                // 2.0 工站状态
                AlarmWord = (short)parameters[10].Datas[0],                  // 4.0 报警字

                // 整型数据 (6.0)
                BarcodeInfo = ((string)parameters[11].Datas[0]).TrimEnd('\r','\n','\0'),                // 6.0 工件条码信息
            };
        }
        public List<DataParameter> ReadDataParameters_ope_P60(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public P50UploadData ReadPlcData_ope_P50()
        {
            // 创建所有需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取布尔信号 (0.0~1.0)
            for (int bit = 0; bit <= 7; bit++)
            {
                parameters.Add(BitRead(0, (byte)bit,(ushort)DBNumber.P50_ope));
            }
            parameters.Add(BitRead(1, 0, (ushort)DBNumber.P50_ope)); // 1.0

            // 读取整型数据 (2.0, 4.0)
            parameters.Add(WordRead(2, (ushort)DBNumber.P50_ope)); // 2.0 工站状态
            parameters.Add(WordRead(4, (ushort)DBNumber.P50_ope)); // 4.0 报警字

            // 读取字符串数据 (6.0)
            parameters.Add(StringRead(8, 48, (ushort)DBNumber.P50_ope)); // 6.0 工件条码信息

            // 读取整型数据 (58.0, 60.0)
            parameters.Add(WordRead(58, (ushort)DBNumber.P50_ope)); // 58.0 拧紧枪程序号
            parameters.Add(WordRead(60, (ushort)DBNumber.P50_ope)); // 60.0 拧螺丝次数
            // 读取浮点型数据 (62.0, 66.0, 70.0, 74.0)
            parameters.Add(RealRead(62, (ushort)DBNumber.P50_ope)); // 62.0 拧紧枪实时扭矩
            parameters.Add(RealRead(66, (ushort)DBNumber.P50_ope)); // 66.0 拧紧枪完成扭矩
            parameters.Add(WordRead(70, (ushort)DBNumber.P50_ope)); // 70.0 拧紧枪实时角度
            parameters.Add(WordRead(72, (ushort)DBNumber.P50_ope)); // 74.0 拧紧枪完成角度

            // 执行读取
            var result = ReadDataParameters_ope_P50(parameters);

            // 解析数据
            return new P50UploadData
            {
                // 布尔信号 (0.0~1.0)
                Heartbeat = (bool)parameters[0].Datas[0],                    // 0.0 心跳
                WorkpieceOnlineRequest = (bool)parameters[1].Datas[0],       // 0.1 工件上线申请
                ReceivedOnlineInstruction = (bool)parameters[2].Datas[0],    // 0.2 收到上线指令
                StationProcessFinished = (bool)parameters[3].Datas[0],       // 0.3 工站流程结束
                ReceivedFinishInstruction = (bool)parameters[4].Datas[0],    // 0.4 收到结束指令
                ManualNGOffline = (bool)parameters[5].Datas[0],              // 0.5 产品手动NG下线
                ReceivedOfflineInstruction = (bool)parameters[6].Datas[0],   // 0.6 收到下线指令
                PressStarted = (bool)parameters[7].Datas[0],                 // 0.7 压机开始工作
                StationWorkingStarted = (bool)parameters[8].Datas[0],        // 1.0 工站开始工作

                // 整型数据 (2.0~4.0)
                StationStatus = (short)parameters[9].Datas[0],               // 2.0 工站状态
                AlarmWord = (short)parameters[10].Datas[0],                  // 4.0 报警字

                // 字符串数据 (6.0)
                Barcode = ((string)parameters[11].Datas[0]).TrimEnd('\r','\n','\0'),                   // 6.0 工件条码信息
                // 整型数据 (58.0~60.0)
                TighteningProgramNumber = (short)parameters[12].Datas[0],             // 58.0 拧紧枪程序号
                ScrewTighteningCount = (short)parameters[13].Datas[0],                 // 60.0 拧螺丝次数
                // 浮点型数据 (62.0~74.0)
                RealtimeTorque = (float)parameters[14].Datas[0],      // 62.0 拧紧枪实时扭矩
                CompletedTorque = (float)parameters[15].Datas[0],         // 66.0 拧紧枪完成扭矩
                RealtimeAngle = (short)parameters[16].Datas[0],        // 70.0 拧紧枪实时角度
                CompletedAngle = (short)parameters[17].Datas[0]            // 74.0 拧紧枪完成角度
            };
        }
        public P40DB3001ReturnData ReadDB3001Data_WorkingData_P40()
        {
            try
            {
                var returnData = new P40DB3001ReturnData();
                // 第一次读取：读取布尔信号和前半部分数据 (0.0, 2.0, 10.0, 18.0, 26.0, 34.0, 6.0, 14.0, 22.0, 30.0)
                var parameters1 = new List<DataParameter>();
                // 添加读取参数
                parameters1.Add(BitRead(0, 0,(ushort)DBNumber.P40_wor));                           // 0.0 读取标志位
                // 添加前半部分浮点型数据读取参数 (2.0, 10.0, 18.0, 26.0, 34.0)
                parameters1.Add(RealRead(2, (ushort)DBNumber.P40_wor));                    // 2.0 完成扭矩1
                parameters1.Add(RealRead(10, (ushort)DBNumber.P40_wor));                   // 10.0 完成扭矩2
                parameters1.Add(RealRead(18, (ushort)DBNumber.P40_wor));                   // 18.0 完成扭矩3
                parameters1.Add(RealRead(26, (ushort)DBNumber.P40_wor));                   // 26.0 完成扭矩4
                parameters1.Add(RealRead(34, (ushort)DBNumber.P40_wor));                   // 34.0 完成扭矩5

                // 添加前半部分双整型数据读取参数 (6.0, 14.0, 22.0, 30.0)
                parameters1.Add(DIntRead(6, (ushort)DBNumber.P40_wor));                     // 6.0 完成角度1
                parameters1.Add(DIntRead(14, (ushort)DBNumber.P40_wor));                    // 14.0 完成角度2
                parameters1.Add(DIntRead(22, (ushort)DBNumber.P40_wor));                    // 22.0 完成角度3
                parameters1.Add(DIntRead(30, (ushort)DBNumber.P40_wor));                    // 30.0 完成角度4

                // 执行第一次读取
                var result1 = ReadDataParameters_workingData_P40(parameters1);

                // 解析第一次读取结果
                returnData.ReadFlag = (bool)parameters1[0].Datas[0];                           // 0.0 读取标志位

                // 解析前半部分浮点型数据 (2.0, 10.0, 18.0, 26.0, 34.0)
                returnData.CompletedTorque1 = (float)parameters1[1].Datas[0];                    // 2.0 完成扭矩1
                returnData.CompletedTorque2 = (float)parameters1[2].Datas[0];                   // 10.0 完成扭矩2
                returnData.CompletedTorque3 = (float)parameters1[3].Datas[0];                   // 18.0 完成扭矩3
                returnData.CompletedTorque4 = (float)parameters1[4].Datas[0];                   // 26.0 完成扭矩4
                returnData.CompletedTorque5 = (float)parameters1[5].Datas[0];                   // 34.0 完成扭矩5

                // 解析前半部分双整型数据 (6.0, 14.0, 22.0, 30.0)
                returnData.CompletedAngle1 = (int)parameters1[6].Datas[0];                     // 6.0 完成角度1
                returnData.CompletedAngle2 = (int)parameters1[7].Datas[0];                    // 14.0 完成角度2
                returnData.CompletedAngle3 = (int)parameters1[8].Datas[0];                    // 22.0 完成角度3
                returnData.CompletedAngle4 = (int)parameters1[9].Datas[0];                    // 30.0 完成角度4

                // 第二次读取：读取后半部分数据 (42.0, 50.0, 58.0, 66.0, 38.0, 46.0, 54.0, 62.0, 70.0)
                var parameters2 = new List<DataParameter>();

                // 添加后半部分浮点型数据读取参数 (42.0, 50.0, 58.0, 66.0)
                parameters2.Add(RealRead(42, (ushort)DBNumber.P40_wor));                   // 42.0 完成扭矩6
                parameters2.Add(RealRead(50, (ushort)DBNumber.P40_wor));                   // 50.0 完成扭矩7
                parameters2.Add(RealRead(58, (ushort)DBNumber.P40_wor));                   // 58.0 完成扭矩8
                parameters2.Add(RealRead(66, (ushort)DBNumber.P40_wor));                   // 66.0 完成扭矩9

                // 添加后半部分双整型数据读取参数 (38.0, 46.0, 54.0, 62.0, 70.0)
                parameters2.Add(DIntRead(38, (ushort)DBNumber.P40_wor));                    // 38.0 完成角度5
                parameters2.Add(DIntRead(46, (ushort)DBNumber.P40_wor));                    // 46.0 完成角度6
                parameters2.Add(DIntRead(54, (ushort)DBNumber.P40_wor));                    // 54.0 完成角度7
                parameters2.Add(DIntRead(62, (ushort)DBNumber.P40_wor));                    // 62.0 完成角度8
                parameters2.Add(DIntRead(70, (ushort)DBNumber.P40_wor));                    // 70.0 完成角度9
                parameters2.Add(RealRead(178, (ushort)DBNumber.P40_wor));                    // 178.0 齿轮高度1
                parameters2.Add(RealRead(182, (ushort)DBNumber.P40_wor));                    // 182.0 齿轮高度2

                // 执行第二次读取
                var result2 = ReadDataParameters_workingData_P40(parameters2);

                // 解析后半部分浮点型数据 (42.0, 50.0, 58.0, 66.0)
                returnData.CompletedTorque6 = (float)parameters2[0].Datas[0];                   // 42.0 完成扭矩6
                returnData.CompletedTorque7 = (float)parameters2[1].Datas[0];                   // 50.0 完成扭矩7
                returnData.CompletedTorque8 = (float)parameters2[2].Datas[0];                   // 58.0 完成扭矩8
                returnData.CompletedTorque9 = (float)parameters2[3].Datas[0];                   // 66.0 完成扭矩9

                // 解析后半部分双整型数据 (38.0, 46.0, 54.0, 62.0, 70.0)
                returnData.CompletedAngle5 = (int)parameters2[4].Datas[0];                    // 38.0 完成角度5
                returnData.CompletedAngle6 = (int)parameters2[5].Datas[0];                    // 46.0 完成角度6
                returnData.CompletedAngle7 = (int)parameters2[6].Datas[0];                    // 54.0 完成角度7
                returnData.CompletedAngle8 = (int)parameters2[7].Datas[0];                    // 62.0 完成角度8
                returnData.CompletedAngle9 = (int)parameters2[8].Datas[0];                    // 70.0 完成角度9
                returnData.GearHeight1 = (float)parameters2[9].Datas[0];                    // 70.0 齿轮高度1

                returnData.GearHeight2 = (float)parameters2[10].Datas[0];                    // 70.0 齿轮高度2

                //读取主条码 DB1000.6 长度50
                var parameters3 = new List<DataParameter>();
                parameters3.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 1500,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 8,
                    Count = 48,
                    DataType = typeof(string)
                });
                var result3 = ReadDataParameters_workingData_P40(parameters3);
                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取DB3001数据失败: {ex.Message}", ex);
            }
        }
        public List<DataParameter> ReadDataParameters_workingData_P40(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public bool GetCanToDo(string code)
        {
            string codeCreateDT = "";
            using (var helper = new SqlConnectionHelper())
            {
                string sqlSelect = "SELECT * FROM P30DB3000ReturnData WHERE Product_BarCode = @Product_BarCode";
                var parameters = new[]
                {
                        SqlConnectionHelper.CreateParameter("@Product_BarCode", code)
                };
                DataTable result = helper.ExecuteDataTable(sqlSelect, parameters);
                if (result != null && result.Rows.Count > 0)
                {
                    DataRow row = result.Rows[0];
                    codeCreateDT = !Convert.IsDBNull(row["CreatedTime"]) ? Convert.ToDateTime(row["CreatedTime"]).ToString("yyyy-MM-dd HH:mm:ss") : "";
                }
            }
            if (string.IsNullOrEmpty(codeCreateDT))
            {
                return false;
            }
            else
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM GlueCuringSettingTime";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                var models = new List<GlueCuringItem>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string minsStr = !Convert.IsDBNull(row["GlueCuringSettingTimeMins"]) ? row["GlueCuringSettingTimeMins"].ToString() : "0";
                        int.TryParse(minsStr, out int mins);
                        models.Add(new GlueCuringItem
                        {
                            GlueCuringSettingTimeMins = mins,
                            CreateDatetime = !Convert.IsDBNull(row["CreateDatetime"]) ? Convert.ToDateTime(row["CreateDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") : ""
                        });
                    }
                }
                if (models.Count > 0)
                {
                    if (DateTime.TryParse(codeCreateDT,out DateTime dateTimeDt))
                    {
                        double totalMinutes = (DateTime.Now - dateTimeDt).TotalMinutes;
                        int ceilMinutes = (int)Math.Ceiling(totalMinutes);
                        if (ceilMinutes > models[0].GlueCuringSettingTimeMins)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        public P40UploadData ReadOperateData_P40()
        {
            // 创建所有需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取布尔信号 (0.0~1.0)
            for (int bit = 0; bit <= 7; bit++)
            {
                parameters.Add(BitRead(0, (byte)bit, (ushort)DBNumber.P40_ope));
            }
            parameters.Add(BitRead(1, 0, (ushort)DBNumber.P40_ope)); // 1.0

            // 读取整型数据 (2.0, 4.0)
            parameters.Add(WordRead(2, (ushort)DBNumber.P40_ope)); // 2.0 工站状态
            parameters.Add(WordRead(4, (ushort)DBNumber.P40_ope)); // 4.0 报警字

            // 读取字符串数据 (6.0)
            parameters.Add(StringRead(6, 50, (ushort)DBNumber.P40_ope)); // 6.0 工件条码信息

            // 读取整型数据 (58.0, 60.0)
            parameters.Add(WordRead(58, (ushort)DBNumber.P40_ope)); // 58.0 拧紧枪程序号
            parameters.Add(WordRead(60, (ushort)DBNumber.P40_ope)); // 60.0 拧螺丝次数
            // 读取浮点型数据 (62.0, 66.0, 70.0, 74.0)
            parameters.Add(RealRead(62, (ushort)DBNumber.P40_ope)); // 62.0 拧紧枪实时扭矩
            parameters.Add(RealRead(66, (ushort)DBNumber.P40_ope)); // 66.0 拧紧枪完成扭矩
            parameters.Add(WordRead(70, (ushort)DBNumber.P40_ope)); // 70.0 拧紧枪实时角度
            parameters.Add(WordRead(72, (ushort)DBNumber.P40_ope)); // 74.0 拧紧枪完成角度
            // 执行读取
            var result = ReadOperate_P40(parameters);
            // 解析数据
            return new P40UploadData
            {
                // 布尔信号 (0.0~1.0)
                Heartbeat = (bool)parameters[0].Datas[0],                    // 0.0 心跳
                WorkpieceOnlineRequest = (bool)parameters[1].Datas[0],       // 0.1 工件上线申请
                ReceivedOnlineInstruction = (bool)parameters[2].Datas[0],    // 0.2 收到上线指令
                StationProcessFinished = (bool)parameters[3].Datas[0],       // 0.3 工站流程结束
                ReceivedFinishInstruction = (bool)parameters[4].Datas[0],    // 0.4 收到结束指令
                ManualNGOffline = (bool)parameters[5].Datas[0],              // 0.5 产品手动NG下线
                ReceivedOfflineInstruction = (bool)parameters[6].Datas[0],   // 0.6 收到下线指令
                PressStarted = (bool)parameters[7].Datas[0],                 // 0.7 压机开始工作
                StationWorkingStarted = (bool)parameters[8].Datas[0],        // 1.0 工站开始工作

                // 整型数据 (2.0~4.0)
                StationStatus = (short)parameters[9].Datas[0],               // 2.0 工站状态
                AlarmWord = (short)parameters[10].Datas[0],                  // 4.0 报警字

                // 字符串数据 (6.0)
                Barcode = ((string)parameters[11].Datas[0]).TrimEnd('\r','\n','\0'),                   // 6.0 工件条码信息
                // 整型数据 (58.0~60.0)
                TighteningProgramNumber = (short)parameters[12].Datas[0],             // 58.0 拧紧枪程序号
                ScrewTighteningCount = (short)parameters[13].Datas[0],                 // 60.0 拧螺丝次数
                // 浮点型数据 (62.0~74.0)
                RealtimeTorque = (float)parameters[14].Datas[0],      // 62.0 拧紧枪实时扭矩
                CompletedTorque = (float)parameters[15].Datas[0],         // 66.0 拧紧枪完成扭矩
                RealtimeAngle = (short)parameters[16].Datas[0],        // 70.0 拧紧枪实时角度
                CompletedAngle = (short)parameters[17].Datas[0]            // 74.0 拧紧枪完成角度
            };
        }
        public List<DataParameter> ReadOperate_P40(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public P30DB3000ReturnData ReadDB3000Data_P30()
        {
            try
            {
                var returnData = new P30DB3000ReturnData();

                // 创建所有需要读取的参数
                var parameters = new List<DataParameter>();

                // 添加读取参数
                parameters.Add(BitRead(0, 0,(ushort)DBNumber.P30_wor));                           // 0.0 读取标志位

                // 添加浮点型数据读取参数 (2.0, 10.0, 18.0, 26.0, 34.0, 42.0, 50.0, 58.0, 66.0)
                parameters.Add(RealRead(2, (ushort)DBNumber.P30_wor));                    // 2.0 完成扭矩1
                parameters.Add(RealRead(10, (ushort)DBNumber.P30_wor));                   // 10.0 完成扭矩2
                parameters.Add(RealRead(18, (ushort)DBNumber.P30_wor));                   // 18.0 完成扭矩3
                parameters.Add(RealRead(26, (ushort)DBNumber.P30_wor));                   // 26.0 完成扭矩4
                parameters.Add(RealRead(34, (ushort)DBNumber.P30_wor));                   // 34.0 完成扭矩5
                parameters.Add(RealRead(42, (ushort)DBNumber.P30_wor));                   // 42.0 完成扭矩6
                parameters.Add(RealRead(50, (ushort)DBNumber.P30_wor));                   // 50.0 完成扭矩7
                parameters.Add(RealRead(58, (ushort)DBNumber.P30_wor));                   // 58.0 完成扭矩8
                parameters.Add(RealRead(66, (ushort)DBNumber.P30_wor));                   // 66.0 完成扭矩9

                // 添加双整型数据读取参数 (6.0, 14.0, 22.0, 30.0, 38.0, 46.0, 54.0, 62.0, 70.0)
                parameters.Add(DIntRead(6, (ushort)DBNumber.P30_wor));                     // 6.0 完成角度1
                parameters.Add(DIntRead(14, (ushort)DBNumber.P30_wor));                    // 14.0 完成角度2
                parameters.Add(DIntRead(22, (ushort)DBNumber.P30_wor));                    // 22.0 完成角度3
                parameters.Add(DIntRead(30, (ushort)DBNumber.P30_wor));                    // 30.0 完成角度4
                parameters.Add(DIntRead(38, (ushort)DBNumber.P30_wor));                    // 38.0 完成角度5
                parameters.Add(DIntRead(46, (ushort)DBNumber.P30_wor));                    // 46.0 完成角度6
                parameters.Add(DIntRead(54, (ushort)DBNumber.P30_wor));                    // 54.0 完成角度7
                parameters.Add(DIntRead(62, (ushort)DBNumber.P30_wor));                    // 62.0 完成角度8
                parameters.Add(DIntRead(70, (ushort)DBNumber.P30_wor));                    // 70.0 完成角度9

                // 执行读取
                var result = ReadData_P30(parameters);
                // 解析读取结果
                returnData.ReadFlag = (bool)parameters[0].Datas[0];                           // 0.0 读取标志位

                // 解析浮点型数据 (2.0, 10.0, 18.0, 26.0, 34.0, 42.0, 50.0, 58.0, 66.0)
                returnData.CompletedTorque1 = (float)parameters[1].Datas[0];                    // 2.0 完成扭矩1
                returnData.CompletedTorque2 = (float)parameters[2].Datas[0];                   // 10.0 完成扭矩2
                returnData.CompletedTorque3 = (float)parameters[3].Datas[0];                   // 18.0 完成扭矩3
                returnData.CompletedTorque4 = (float)parameters[4].Datas[0];                   // 26.0 完成扭矩4
                returnData.CompletedTorque5 = (float)parameters[5].Datas[0];                   // 34.0 完成扭矩5
                returnData.CompletedTorque6 = (float)parameters[6].Datas[0];                   // 42.0 完成扭矩6
                returnData.CompletedTorque7 = (float)parameters[7].Datas[0];                   // 50.0 完成扭矩7
                returnData.CompletedTorque8 = (float)parameters[8].Datas[0];                   // 58.0 完成扭矩8
                returnData.CompletedTorque9 = (float)parameters[9].Datas[0];                   // 66.0 完成扭矩9

                // 解析双整型数据 (6.0, 14.0, 22.0, 30.0, 38.0, 46.0, 54.0, 62.0, 70.0)
                returnData.CompletedAngle1 = (int)parameters[10].Datas[0];                     // 6.0 完成角度1
                returnData.CompletedAngle2 = (int)parameters[11].Datas[0];                    // 14.0 完成角度2
                returnData.CompletedAngle3 = (int)parameters[12].Datas[0];                    // 22.0 完成角度3
                returnData.CompletedAngle4 = (int)parameters[13].Datas[0];                    // 30.0 完成角度4
                returnData.CompletedAngle5 = (int)parameters[14].Datas[0];                    // 38.0 完成角度5
                returnData.CompletedAngle6 = (int)parameters[15].Datas[0];                    // 46.0 完成角度6
                returnData.CompletedAngle7 = (int)parameters[16].Datas[0];                    // 54.0 完成角度7
                returnData.CompletedAngle8 = (int)parameters[17].Datas[0];                    // 62.0 完成角度8
                returnData.CompletedAngle9 = (int)parameters[18].Datas[0];                    // 70.0 完成角度9
                                                                                              //读取主条码 DB1000.6 长度50
                var parameters1 = new List<DataParameter>();
                parameters1.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 1000,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 8,
                    Count = 48,
                    DataType = typeof(string)
                });
                var result1 = ReadData_P30(parameters1);
                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取DB3000数据失败: {ex.Message}", ex);
            }
        }
        public P30DB3000ReturnData ReadDB3000Data_P35()
        {
            try
            {
                var returnData = new P30DB3000ReturnData();

                // 创建所有需要读取的参数
                var parameters = new List<DataParameter>();

                // 添加读取参数
                parameters.Add(BitRead(0, 0, (ushort)DBNumber.P35_wor));                           // 0.0 读取标志位

                // 添加浮点型数据读取参数 (2.0, 10.0, 18.0, 26.0, 34.0, 42.0, 50.0, 58.0, 66.0)
                parameters.Add(RealRead(2, (ushort)DBNumber.P35_wor));                    // 2.0 完成扭矩1
                parameters.Add(RealRead(10, (ushort)DBNumber.P35_wor));                   // 10.0 完成扭矩2
                parameters.Add(RealRead(18, (ushort)DBNumber.P35_wor));                   // 18.0 完成扭矩3
                parameters.Add(RealRead(26, (ushort)DBNumber.P35_wor));                   // 26.0 完成扭矩4
                parameters.Add(RealRead(34, (ushort)DBNumber.P35_wor));                   // 34.0 完成扭矩5
                parameters.Add(RealRead(42, (ushort)DBNumber.P35_wor));                   // 42.0 完成扭矩6
                parameters.Add(RealRead(50, (ushort)DBNumber.P35_wor));                   // 50.0 完成扭矩7
                parameters.Add(RealRead(58, (ushort)DBNumber.P35_wor));                   // 58.0 完成扭矩8
                parameters.Add(RealRead(66, (ushort)DBNumber.P35_wor));                   // 66.0 完成扭矩9

                // 添加双整型数据读取参数 (6.0, 14.0, 22.0, 30.0, 38.0, 46.0, 54.0, 62.0, 70.0)
                parameters.Add(DIntRead(6, (ushort)DBNumber.P35_wor));                     // 6.0 完成角度1
                parameters.Add(DIntRead(14, (ushort)DBNumber.P35_wor));                    // 14.0 完成角度2
                parameters.Add(DIntRead(22, (ushort)DBNumber.P35_wor));                    // 22.0 完成角度3
                parameters.Add(DIntRead(30, (ushort)DBNumber.P35_wor));                    // 30.0 完成角度4
                parameters.Add(DIntRead(38, (ushort)DBNumber.P35_wor));                    // 38.0 完成角度5
                parameters.Add(DIntRead(46, (ushort)DBNumber.P35_wor));                    // 46.0 完成角度6
                parameters.Add(DIntRead(54, (ushort)DBNumber.P35_wor));                    // 54.0 完成角度7
                parameters.Add(DIntRead(62, (ushort)DBNumber.P35_wor));                    // 62.0 完成角度8
                parameters.Add(DIntRead(70, (ushort)DBNumber.P35_wor));                    // 70.0 完成角度9

                // 执行读取
                var result = ReadData_P30(parameters);
                // 解析读取结果
                returnData.ReadFlag = (bool)parameters[0].Datas[0];                           // 0.0 读取标志位

                // 解析浮点型数据 (2.0, 10.0, 18.0, 26.0, 34.0, 42.0, 50.0, 58.0, 66.0)
                returnData.CompletedTorque1 = (float)parameters[1].Datas[0];                    // 2.0 完成扭矩1
                returnData.CompletedTorque2 = (float)parameters[2].Datas[0];                   // 10.0 完成扭矩2
                returnData.CompletedTorque3 = (float)parameters[3].Datas[0];                   // 18.0 完成扭矩3
                returnData.CompletedTorque4 = (float)parameters[4].Datas[0];                   // 26.0 完成扭矩4
                returnData.CompletedTorque5 = (float)parameters[5].Datas[0];                   // 34.0 完成扭矩5
                returnData.CompletedTorque6 = (float)parameters[6].Datas[0];                   // 42.0 完成扭矩6
                returnData.CompletedTorque7 = (float)parameters[7].Datas[0];                   // 50.0 完成扭矩7
                returnData.CompletedTorque8 = (float)parameters[8].Datas[0];                   // 58.0 完成扭矩8
                returnData.CompletedTorque9 = (float)parameters[9].Datas[0];                   // 66.0 完成扭矩9

                // 解析双整型数据 (6.0, 14.0, 22.0, 30.0, 38.0, 46.0, 54.0, 62.0, 70.0)
                returnData.CompletedAngle1 = (int)parameters[10].Datas[0];                     // 6.0 完成角度1
                returnData.CompletedAngle2 = (int)parameters[11].Datas[0];                    // 14.0 完成角度2
                returnData.CompletedAngle3 = (int)parameters[12].Datas[0];                    // 22.0 完成角度3
                returnData.CompletedAngle4 = (int)parameters[13].Datas[0];                    // 30.0 完成角度4
                returnData.CompletedAngle5 = (int)parameters[14].Datas[0];                    // 38.0 完成角度5
                returnData.CompletedAngle6 = (int)parameters[15].Datas[0];                    // 46.0 完成角度6
                returnData.CompletedAngle7 = (int)parameters[16].Datas[0];                    // 54.0 完成角度7
                returnData.CompletedAngle8 = (int)parameters[17].Datas[0];                    // 62.0 完成角度8
                returnData.CompletedAngle9 = (int)parameters[18].Datas[0];                    // 70.0 完成角度9
                                                                                              //读取主条码 DB1000.6 长度50
                var parameters1 = new List<DataParameter>();
                parameters1.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 1000,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 8,
                    Count = 48,
                    DataType = typeof(string)
                });
                var result1 = ReadData_P30(parameters1);
                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取DB3000数据失败: {ex.Message}", ex);
            }
        }
        public List<DataParameter> ReadData_P30(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public P30UploadData ReadData_P30()
        {
            // 创建所有需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取布尔信号 (0.0~1.0)
            for (int bit = 0; bit <= 7; bit++)
            {
                parameters.Add(BitRead(0, (byte)bit, (ushort)DBNumber.P30_ope));
            }
            parameters.Add(BitRead(1, 0, (ushort)DBNumber.P30_ope)); // 1.0

            // 读取整型数据 (2.0, 4.0)
            parameters.Add(WordRead(2, (ushort)DBNumber.P30_ope)); // 2.0 工站状态
            parameters.Add(WordRead(4, (ushort)DBNumber.P30_ope)); // 4.0 报警字

            // 读取字符串数据 (6.0)
            parameters.Add(StringRead(8, 48, (ushort)DBNumber.P30_ope)); // 6.0 工件条码信息

            //读取整型数据(58.0, 60.0)
            parameters.Add(WordRead(58, (ushort)DBNumber.P30_ope)); // 58.0 拧紧枪程序号
            parameters.Add(WordRead(60, (ushort)DBNumber.P30_ope)); // 60.0 拧螺丝次数
                                          // 读取浮点型数据(62.0, 66.0, 70.0, 74.0)
            parameters.Add(RealRead(62, (ushort)DBNumber.P30_ope)); // 62.0 拧紧枪实时扭矩
            parameters.Add(RealRead(66, (ushort)DBNumber.P30_ope)); // 66.0 拧紧枪完成扭矩
            parameters.Add(WordRead(70, (ushort)DBNumber.P30_ope)); // 70.0 拧紧枪实时角度
            parameters.Add(WordRead(72, (ushort)DBNumber.P30_ope)); // 74.0 拧紧枪完成角度

            parameters.Add(StringRead(76, 21, (ushort)DBNumber.P30_ope)); // 76.0 转头二维码
            // 执行读取
            var result = ReadDataParameters_P30(parameters);

            // 解析数据
            return new P30UploadData
            {
                // 布尔信号 (0.0~1.0)
                Heartbeat = (bool)parameters[0].Datas[0],                    // 0.0 心跳
                WorkpieceOnlineRequest = (bool)parameters[1].Datas[0],       // 0.1 工件上线申请
                ReceivedOnlineInstruction = (bool)parameters[2].Datas[0],    // 0.2 收到上线指令
                StationProcessFinished = (bool)parameters[3].Datas[0],       // 0.3 工站流程结束
                ReceivedFinishInstruction = (bool)parameters[4].Datas[0],    // 0.4 收到结束指令
                ManualNGOffline = (bool)parameters[5].Datas[0],              // 0.5 产品手动NG下线
                ReceivedOfflineInstruction = (bool)parameters[6].Datas[0],   // 0.6 收到下线指令
                PressStarted = (bool)parameters[7].Datas[0],                 // 0.7 压机开始工作
                StationWorkingStarted = (bool)parameters[8].Datas[0],        // 1.0 工站开始工作

                // 整型数据 (2.0~4.0)
                StationStatus = (short)parameters[9].Datas[0],               // 2.0 工站状态
                AlarmWord = (short)parameters[10].Datas[0],                  // 4.0 报警字

                // 字符串数据 (6.0)
                Barcode = ((string)parameters[11].Datas[0]).TrimEnd('\r','\n','\0'),                   // 6.0 工件条码信息

                // 整型数据 (58.0~60.0)
                TighteningProgramNumber = (short)parameters[12].Datas[0],             // 58.0 拧紧枪程序号
                ScrewTighteningCount = (short)parameters[13].Datas[0],                 // 60.0 拧螺丝次数
                // 浮点型数据 (62.0~74.0)
                RealtimeTorque = (float)parameters[14].Datas[0],      // 62.0 拧紧枪实时扭矩
                CompletedTorque = (float)parameters[15].Datas[0],         // 66.0 拧紧枪完成扭矩
                RealtimeAngle = (short)parameters[16].Datas[0],        // 70.0 拧紧枪实时角度
                CompletedAngle = (short)parameters[17].Datas[0],            // 74.0 拧紧枪完成角度
                TurnHeadCode = ((string)parameters[18].Datas[0]).TrimEnd('\r', '\n', '\0') // 76.0 转头二维码
            };
        }
        public P30UploadData ReadData_P35()
        {
            // 创建所有需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取布尔信号 (0.0~1.0)
            for (int bit = 0; bit <= 7; bit++)
            {
                parameters.Add(BitRead(0, (byte)bit, (ushort)DBNumber.P35_ope));
            }
            parameters.Add(BitRead(1, 0, (ushort)DBNumber.P35_ope)); // 1.0

            // 读取整型数据 (2.0, 4.0)
            parameters.Add(WordRead(2, (ushort)DBNumber.P35_ope)); // 2.0 工站状态
            parameters.Add(WordRead(4, (ushort)DBNumber.P35_ope)); // 4.0 报警字

            // 读取字符串数据 (6.0)
            parameters.Add(StringRead(8, 48, (ushort)DBNumber.P35_ope)); // 6.0 工件条码信息

            //读取整型数据(58.0, 60.0)
            parameters.Add(WordRead(58, (ushort)DBNumber.P35_ope)); // 58.0 拧紧枪程序号
            parameters.Add(WordRead(60, (ushort)DBNumber.P35_ope)); // 60.0 拧螺丝次数
                                                                    // 读取浮点型数据(62.0, 66.0, 70.0, 74.0)
            parameters.Add(RealRead(62, (ushort)DBNumber.P35_ope)); // 62.0 拧紧枪实时扭矩
            parameters.Add(RealRead(66, (ushort)DBNumber.P35_ope)); // 66.0 拧紧枪完成扭矩
            parameters.Add(WordRead(70, (ushort)DBNumber.P35_ope)); // 70.0 拧紧枪实时角度
            parameters.Add(WordRead(72, (ushort)DBNumber.P35_ope)); // 74.0 拧紧枪完成角度

            // 执行读取
            var result = ReadDataParameters_P30(parameters);

            // 解析数据
            return new P30UploadData
            {
                // 布尔信号 (0.0~1.0)
                Heartbeat = (bool)parameters[0].Datas[0],                    // 0.0 心跳
                WorkpieceOnlineRequest = (bool)parameters[1].Datas[0],       // 0.1 工件上线申请
                ReceivedOnlineInstruction = (bool)parameters[2].Datas[0],    // 0.2 收到上线指令
                StationProcessFinished = (bool)parameters[3].Datas[0],       // 0.3 工站流程结束
                ReceivedFinishInstruction = (bool)parameters[4].Datas[0],    // 0.4 收到结束指令
                ManualNGOffline = (bool)parameters[5].Datas[0],              // 0.5 产品手动NG下线
                ReceivedOfflineInstruction = (bool)parameters[6].Datas[0],   // 0.6 收到下线指令
                PressStarted = (bool)parameters[7].Datas[0],                 // 0.7 压机开始工作
                StationWorkingStarted = (bool)parameters[8].Datas[0],        // 1.0 工站开始工作

                // 整型数据 (2.0~4.0)
                StationStatus = (short)parameters[9].Datas[0],               // 2.0 工站状态
                AlarmWord = (short)parameters[10].Datas[0],                  // 4.0 报警字

                // 字符串数据 (6.0)
                Barcode = ((string)parameters[11].Datas[0]).TrimEnd('\r', '\n', '\0'),                   // 6.0 工件条码信息

                // 整型数据 (58.0~60.0)
                TighteningProgramNumber = (short)parameters[12].Datas[0],             // 58.0 拧紧枪程序号
                ScrewTighteningCount = (short)parameters[13].Datas[0],                 // 60.0 拧螺丝次数
                // 浮点型数据 (62.0~74.0)
                RealtimeTorque = (float)parameters[14].Datas[0],      // 62.0 拧紧枪实时扭矩
                CompletedTorque = (float)parameters[15].Datas[0],         // 66.0 拧紧枪完成扭矩
                RealtimeAngle = (short)parameters[16].Datas[0],        // 70.0 拧紧枪实时角度
                CompletedAngle = (short)parameters[17].Datas[0]            // 74.0 拧紧枪完成角度

            };
        }

        public List<DataParameter> ReadDataParameters_P30(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public P20DB2000ReturnData ReadDB2000Data_P20()
        {
            try
            {
                var returnData = new P20DB2000ReturnData();

                // 创建所有需要读取的参数
                var parameters = new List<DataParameter>();

                // 添加读取参数
                parameters.Add(BitRead(0, 0, (ushort)DBNumber.P20_wor));                           // 0.0 读取标志位

                // 添加浮点型数据读取参数 (2.0~22.0)
                parameters.Add(RealRead(2, (ushort)DBNumber.P20_wor));             // 2.0 轮次1压头1压装高度
                parameters.Add(RealRead(6, (ushort)DBNumber.P20_wor));           // 6.0 轮次1压头1最终压力
                parameters.Add(RealRead(10, (ushort)DBNumber.P20_wor));            // 10.0 轮次2压头2压装高度
                parameters.Add(RealRead(14, (ushort)DBNumber.P20_wor));          // 14.0 轮次2压头2最终压力
                parameters.Add(RealRead(18, (ushort)DBNumber.P20_wor));            // 18.0 轮次3压头3压装高度
                parameters.Add(RealRead(22, (ushort)DBNumber.P20_wor));          // 22.0 轮次3压头3最终压力

                ReadDB2000Data_P20(parameters);
                // 解析读取结果
                returnData.ReadFlag = (bool)parameters[0].Datas[0];                           // 0.0 读取标志位

                // 解析浮点型数据 (2.0~22.0)
                returnData.Cycle1Head1PressHeight = (float)parameters[1].Datas[0];             // 2.0 轮次1压头1压装高度
                returnData.Cycle1Head1FinalPressure = (float)parameters[2].Datas[0];           // 6.0 轮次1压头1最终压力
                returnData.Cycle2Head2PressHeight = (float)parameters[3].Datas[0];            // 10.0 轮次2压头2压装高度
                returnData.Cycle2Head2FinalPressure = (float)parameters[4].Datas[0];          // 14.0 轮次2压头2最终压力
                returnData.Cycle3Head3PressHeight = (float)parameters[5].Datas[0];            // 18.0 轮次3压头3压装高度
                returnData.Cycle3Head3FinalPressure = (float)parameters[6].Datas[0];          // 22.0 轮次3压头3最终压力
                //读取主条码 DB1000.6 长度50
                var parameters1 = new List<DataParameter>();
                parameters1.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 1000,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 8,
                    Count = 48,
                    DataType = typeof(string)
                });
                ReadDB2000Data_P20(parameters1);
                return returnData;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取DB2000数据失败: {ex.Message}", ex);
            }
        }
        public List<DataParameter> ReadDB2000Data_P20(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        private P20UploadData ReadPlcDataForP20()
        {
            // 创建所有需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取布尔信号 (0.0~1.0)
            for (int bit = 0; bit <= 7; bit++)
            {
                parameters.Add(BitRead(0, (byte)bit, (ushort)DBNumber.P20_ope));
            }
            parameters.Add(BitRead(1, 0, (ushort)DBNumber.P20_ope)); // 1.0

            // 读取整型数据 (2.0, 4.0)
            parameters.Add(WordRead(2, (ushort)DBNumber.P20_ope)); // 2.0 工站状态
            parameters.Add(WordRead(4, (ushort)DBNumber.P20_ope)); // 4.0 报警字

            // 读取字符串数据 (6.0)
            parameters.Add(StringRead(8, 48, (ushort)DBNumber.P20_ope)); // 6.0 工件条码信息

            // 读取整型数据 (58.0, 60.0)
            parameters.Add(WordRead(58, (ushort)DBNumber.P20_ope)); // 58.0 压机程序号
            parameters.Add(WordRead(60, (ushort)DBNumber.P20_ope)); // 60.0 压装次数

            // 读取浮点型数据 (62.0, 66.0, 70.0, 74.0)
            parameters.Add(RealRead(62, (ushort)DBNumber.P20_ope)); // 62.0 压机实时压力
            parameters.Add(RealRead(66, (ushort)DBNumber.P20_ope)); // 66.0 压机完成压力
            parameters.Add(RealRead(70, (ushort)DBNumber.P20_ope)); // 70.0 压机实时高度
            parameters.Add(RealRead(74, (ushort)DBNumber.P20_ope)); // 74.0 压机完成高度

            // 执行读取
            var result = ReadDataParametersForP20(parameters);
            // 解析数据
            return new P20UploadData
            {
                // 布尔信号 (0.0~1.0)
                Heartbeat = (bool)parameters[0].Datas[0],                    // 0.0 心跳
                WorkpieceOnlineRequest = (bool)parameters[1].Datas[0],       // 0.1 工件上线申请
                ReceivedOnlineInstruction = (bool)parameters[2].Datas[0],    // 0.2 收到上线指令
                StationProcessFinished = (bool)parameters[3].Datas[0],       // 0.3 工站流程结束
                ReceivedFinishInstruction = (bool)parameters[4].Datas[0],    // 0.4 收到结束指令
                ManualNGOffline = (bool)parameters[5].Datas[0],              // 0.5 产品手动NG下线
                ReceivedOfflineInstruction = (bool)parameters[6].Datas[0],   // 0.6 收到下线指令
                PressStarted = (bool)parameters[7].Datas[0],                 // 0.7 压机开始工作
                StationWorkingStarted = (bool)parameters[8].Datas[0],        // 1.0 工站开始工作

                // 整型数据 (2.0~4.0)
                StationStatus = (short)parameters[9].Datas[0],               // 2.0 工站状态
                AlarmWord = (short)parameters[10].Datas[0],                  // 4.0 报警字

                // 字符串数据 (6.0)
                Barcode = ((string)parameters[11].Datas[0]).TrimEnd('\r','\n','\0'),                   // 6.0 工件条码信息

                // 整型数据 (58.0~60.0)
                PressProgramNo = (short)parameters[12].Datas[0],             // 58.0 压机程序号
                PressCount = (short)parameters[13].Datas[0],                 // 60.0 压装次数

                // 浮点型数据 (62.0~74.0)
                PressRealtimePressure = (float)parameters[14].Datas[0],      // 62.0 压机实时压力
                PressFinalPressure = (float)parameters[15].Datas[0],         // 66.0 压机完成压力
                PressRealtimeHeight = (float)parameters[16].Datas[0],        // 70.0 压机实时高度
                PressFinalHeight = (float)parameters[17].Datas[0]            // 74.0 压机完成高度
            };
        }
        private List<DataParameter> ReadDataParametersForP20(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }
        public P10UploadData ReadPlcData_P10()
        {
            // 创建所有需要读取的参数
            var parameters = new List<DataParameter>();

            // 读取布尔信号 (0.0~1.0)
            for (int bit = 0; bit <= 7; bit++)
            {
                parameters.Add(BitRead(0, (byte)bit, (ushort)DBNumber.P10_ope));
            }
            parameters.Add(BitRead(1, 0, (ushort)DBNumber.P10_ope)); // 1.0

            // 读取整型数据 (2.0, 4.0)
            parameters.Add(WordRead(2, (ushort)DBNumber.P10_ope)); // 2.0 工站状态
            parameters.Add(WordRead(4, (ushort)DBNumber.P10_ope)); // 4.0 报警字
            // 读取字符串数据 (6.0)
            parameters.Add(StringRead(8, 48, (ushort)DBNumber.P10_ope)); // 6.0 工件条码信息
            // 读取整型数据 (58.0, 60.0)
            parameters.Add(WordRead(58, (ushort)DBNumber.P10_ope)); // 58.0 压机程序号
            parameters.Add(WordRead(60, (ushort)DBNumber.P10_ope)); // 60.0 压装次数

            // 读取浮点型数据 (62.0, 66.0, 70.0, 74.0)
            parameters.Add(RealRead(62, (ushort)DBNumber.P10_ope)); // 62.0 压机实时压力
            parameters.Add(RealRead(66, (ushort)DBNumber.P10_ope)); // 66.0 压机完成压力
            parameters.Add(RealRead(70, (ushort)DBNumber.P10_ope)); // 70.0 压机实时高度
            parameters.Add(RealRead(74, (ushort)DBNumber.P10_ope)); // 74.0 压机完成高度

            // 执行读取
            ReadDataParameters(parameters);
            // 解析数据
            return new P10UploadData
            {
                // 布尔信号 (0.0~1.0)
                Heartbeat = (bool)parameters[0].Datas[0],                    // 0.0 心跳
                WorkpieceOnlineRequest = (bool)parameters[1].Datas[0],       // 0.1 工件上线申请
                ReceivedOnlineInstruction = (bool)parameters[2].Datas[0],    // 0.2 收到上线指令
                StationProcessFinished = (bool)parameters[3].Datas[0],       // 0.3 工站流程结束
                ReceivedFinishInstruction = (bool)parameters[4].Datas[0],    // 0.4 收到结束指令
                ManualNGOffline = (bool)parameters[5].Datas[0],              // 0.5 产品手动NG下线
                ReceivedOfflineInstruction = (bool)parameters[6].Datas[0],   // 0.6 收到下线指令
                PressStarted = (bool)parameters[7].Datas[0],                 // 0.7 压机开始工作
                StationWorkingStarted = (bool)parameters[8].Datas[0],        // 1.0 工站开始工作

                // 整型数据 (2.0~4.0)
                StationStatus = (short)parameters[9].Datas[0],               // 2.0 工站状态
                AlarmWord = (short)parameters[10].Datas[0],                  // 4.0 报警字

                // 字符串数据 (6.0)
                Barcode = ((string)parameters[11].Datas[0]).TrimEnd('\r','\n','\0'),                   // 6.0 工件条码信息

                // 整型数据 (58.0~60.0)
                PressProgramNo = (short)parameters[12].Datas[0],             // 58.0 压机程序号
                PressCount = (short)parameters[13].Datas[0],                 // 60.0 压装次数

                // 浮点型数据 (62.0~74.0)
                PressRealtimePressure = (float)parameters[14].Datas[0],      // 62.0 压机实时压力
                PressFinalPressure = (float)parameters[15].Datas[0],         // 66.0 压机完成压力
                PressRealtimeHeight = (float)parameters[16].Datas[0],        // 70.0 压机实时高度
                PressFinalHeight = (float)parameters[17].Datas[0]            // 74.0 压机完成高度
            };
        }
        //读取DB2000数据
        public P10DB2000ReturnData ReadDB2000Data()
        {
            var returnData = new P10DB2000ReturnData();
            try
            {
                // 创建所有需要读取的参数
                var parameters = new List<DataParameter>();
                // 添加读取参数
                parameters.Add(BitRead(0, 0, (ushort)DBNumber.P10_wor));                           // 0.0 读取标志位
                // 添加浮点型数据读取参数 (2.0~70.0)
                parameters.Add(RealRead(2, (ushort)DBNumber.P10_wor));             // 2.0 轮次1压头1压装高度1
                parameters.Add(RealRead(6, (ushort)DBNumber.P10_wor));           // 6.0 轮次1压头1最终压力1
                parameters.Add(RealRead(10, (ushort)DBNumber.P10_wor));            // 10.0 轮次1压头1压装高度2
                parameters.Add(RealRead(14, (ushort)DBNumber.P10_wor));          // 14.0 轮次1压头1最终压力2
                parameters.Add(RealRead(18, (ushort)DBNumber.P10_wor));             // 18.0 轮次1压头2压装高度
                parameters.Add(RealRead(22, (ushort)DBNumber.P10_wor));           // 22.0 轮次1压头2最终压力
                parameters.Add(RealRead(26, (ushort)DBNumber.P10_wor));             // 26.0 轮次1压头3压装高度
                parameters.Add(RealRead(30, (ushort)DBNumber.P10_wor));           // 30.0 轮次1压头3最终压力
                parameters.Add(RealRead(34, (ushort)DBNumber.P10_wor));             // 34.0 轮次1压头4压装高度
                parameters.Add(RealRead(38, (ushort)DBNumber.P10_wor));           // 38.0 轮次1压头4最终压力
                parameters.Add(RealRead(42, (ushort)DBNumber.P10_wor));             // 42.0 轮次2压头2压装高度
                parameters.Add(RealRead(46, (ushort)DBNumber.P10_wor));           // 46.0 轮次2压头2最终压力
                parameters.Add(RealRead(50, (ushort)DBNumber.P10_wor));             // 50.0 轮次2压头3压装高度
                parameters.Add(RealRead(54, (ushort)DBNumber.P10_wor));           // 54.0 轮次2压头3最终压力
                parameters.Add(RealRead(58, (ushort)DBNumber.P10_wor));             // 58.0 轮次2压头4压装高度
                parameters.Add(RealRead(62, (ushort)DBNumber.P10_wor));           // 62.0 轮次2压头4最终压力
                parameters.Add(RealRead(66, (ushort)DBNumber.P10_wor));             // 66.0 轮次3压头2压装高度
                parameters.Add(RealRead(70, (ushort)DBNumber.P10_wor));           // 70.0 轮次3压头2最终压力

                // 执行读取
                ReadDataParameters_WorkingData(parameters);
                // 解析读取结果
                returnData.ReadFlag = (bool)parameters[0].Datas[0];                           // 0.0 读取标志位

                // 解析浮点型数据 (2.0~70.0)
                returnData.Cycle1Head1PressHeight1 = (float)parameters[1].Datas[0];             // 2.0 轮次1压头1压装高度1
                returnData.Cycle1Head1FinalPressure1 = (float)parameters[2].Datas[0];           // 6.0 轮次1压头1最终压力1
                returnData.Cycle1Head1PressHeight2 = (float)parameters[3].Datas[0];            // 10.0 轮次1压头1压装高度2
                returnData.Cycle1Head1FinalPressure2 = (float)parameters[4].Datas[0];          // 14.0 轮次1压头1最终压力2
                returnData.Cycle1Head2PressHeight = (float)parameters[5].Datas[0];             // 18.0 轮次1压头2压装高度
                returnData.Cycle1Head2FinalPressure = (float)parameters[6].Datas[0];           // 22.0 轮次1压头2最终压力
                returnData.Cycle1Head3PressHeight = (float)parameters[7].Datas[0];             // 26.0 轮次1压头3压装高度
                returnData.Cycle1Head3FinalPressure = (float)parameters[8].Datas[0];           // 30.0 轮次1压头3最终压力
                returnData.Cycle1Head4PressHeight = (float)parameters[9].Datas[0];             // 34.0 轮次1压头4压装高度
                returnData.Cycle1Head4FinalPressure = (float)parameters[10].Datas[0];           // 38.0 轮次1压头4最终压力
                returnData.Cycle2Head2PressHeight = (float)parameters[11].Datas[0];             // 42.0 轮次2压头2压装高度
                returnData.Cycle2Head2FinalPressure = (float)parameters[12].Datas[0];           // 46.0 轮次2压头2最终压力
                returnData.Cycle2Head3PressHeight = (float)parameters[13].Datas[0];             // 50.0 轮次2压头3压装高度
                returnData.Cycle2Head3FinalPressure = (float)parameters[14].Datas[0];           // 54.0 轮次2压头3最终压力
                returnData.Cycle2Head4PressHeight = (float)parameters[15].Datas[0];             // 58.0 轮次2压头4压装高度
                returnData.Cycle2Head4FinalPressure = (float)parameters[16].Datas[0];           // 62.0 轮次2压头4最终压力
                returnData.Cycle3Head2PressHeight = (float)parameters[17].Datas[0];             // 66.0 轮次3压头2压装高度
                returnData.Cycle3Head2FinalPressure = (float)parameters[18].Datas[0];           // 70.0 轮次3压头2最终压力
                //读取主条码 DB1000.6 长度50
                var parameters1 = new List<DataParameter>();
                parameters1.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 1000,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 8,
                    Count = 48,
                    DataType = typeof(string)
                });
                ReadDataParameters_WorkingData(parameters1);
                returnData.Product_BarCode = ((string)parameters1[0].Datas[0]).TrimEnd('\r','\n','\0');
            }
            catch (Exception)
            {
            }
            return returnData;
        }
        //采集工作数据
        public List<DataParameter> ReadDataParameters_WorkingData(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }

        private short GetCommandCode(string commandName)
        {
            if (Enum.TryParse<CommandType>(commandName, out CommandType command))
            {
                return (short)command;
            }
            return 0;
        }
        //类型转换辅助方法
        private object ConvertValue(object value, Type targetType)
        {
            if (value == null || targetType == null)
                return value;

            try
            {
                // 如果类型匹配，直接返回
                if (value.GetType() == targetType)
                    return value;

                // 处理ushort→short转换
                if (targetType == typeof(short) && value is ushort)
                    return unchecked((short)(ushort)value);

                // 处理uint→int转换
                if (targetType == typeof(int) && value is uint)
                    return unchecked((int)(uint)value);

                // 处理字符串转换
                if (targetType == typeof(string))
                {
                    string result;
                    if (value is byte)
                        result = ((byte)value).ToString();
                    else if (value is byte[])
                        result = System.Text.Encoding.ASCII.GetString((byte[])value);
                    else
                        result = value.ToString();

                    // 去除字符串中的空格、回车等空白字符
                    return result.Trim();
                }

                // 其他情况使用Convert
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                // 转换失败，返回原值
                return value;
            }
        }
        //转换PValueSize枚举为VarType枚举
        private VarType ConvertPValueSizeToVarType(PValueSize size)
        {
            return size switch
            {
                PValueSize.BIT => VarType.Bit,
                PValueSize.BYTE => VarType.Byte,
                PValueSize.CHAR => VarType.Byte,
                PValueSize.WORD => VarType.Word,
                PValueSize.INTERGER => VarType.Int,
                PValueSize.DWORD => VarType.DWord,
                PValueSize.DINT => VarType.DInt,
                PValueSize.REAL => VarType.Real,
                PValueSize.DATE => VarType.Byte,
                _ => VarType.Byte
            };
        }
        //转换Areas枚举为DataType枚举
        private DataType ConvertAreasToDataType(Areas area)
        {
            return area switch
            {
                Areas.Input => DataType.Input,
                Areas.Output => DataType.Output,
                Areas.Memory => DataType.Memory,
                Areas.DataBlock => DataType.DataBlock,
                _ => DataType.DataBlock
            };
        }
        /// <summary>
        /// 读取字节数据
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        /// <param name="count">读取字节数</param>
        /// <returns>字节数组</returns>
        public byte[] ReadBytes(DataType dataType, int db, int startByteAdr, int count)
        {
            return _plc.ReadBytes(dataType, db, startByteAdr, count);
        }
        /// <summary>
        /// 读取指定类型的数据
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        /// <param name="varType">变量类型</param>
        /// <param name="varCount">变量数量</param>
        /// <returns>解码后的数据</returns>
        public object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount)
        {
            return _plc.Read(dataType, db, startByteAdr, varType, varCount);
        }
        /// <summary>
        /// 通过地址字符串读取数据
        /// </summary>
        /// <param name="variable">地址字符串，如"DB1.DBW20"</param>
        /// <returns>读取的数据</returns>
        public object Read(string variable)
        {
            return _plc.Read(variable);
        }

        /// <summary>
        /// 写入字节数据
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        /// <param name="value">要写入的字节数组</param>
        public void WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value)
        {

            _plc.WriteBytes(dataType, db, startByteAdr, value);

        }

        /// <summary>
        /// 写入指定类型的数据
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        /// <param name="value">要写入的数据</param>
        public void Write(DataType dataType, int db, int startByteAdr, object value)
        {

            _plc.Write(dataType, db, startByteAdr, value);

        }

        /// <summary>
        /// 通过地址字符串写入数据
        /// </summary>
        /// <param name="variable">地址字符串，如"DB1.DBW20"</param>
        /// <param name="value">要写入的数据</param>
        public void Write(string variable, object value)
        {
            try
            {
                _plc.Write(variable, value);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">地址，如"DB1.DBB0"</param>
        /// <param name="length">字符串长度</param>
        /// <returns>字符串值</returns>
        public string ReadString(string address, int length)
        {
            var bytes = ReadBytes(DataType.DataBlock,
                int.Parse(address.Split('.')[0].Substring(2)),
                int.Parse(address.Split('.')[1].Substring(3)),
                length);
            return S7String.FromByteArray(bytes);
        }
        /// <summary>
        /// 写入布尔值
        /// </summary>
        /// <param name="address">地址，如"DB1.DBX0.5"</param>
        /// <param name="value">布尔值</param>
        public void WriteBool(string address, bool value)
        {
            Write(address, value);
        }
        /// <summary>
        /// 写入整数
        /// </summary>
        /// <param name="address">地址，如"DB1.DBW0"</param>
        /// <param name="value">整数值</param>
        public void WriteInt(string address, short value)
        {
            Write(address, value);
        }

        private DataParameter RealWrite(int byteAddr, float value,ushort dbNumber) => new DataParameter
        {
            Area = Areas.DataBlock,
            DBNumber = dbNumber,
            PValueSize = PValueSize.REAL,
            DValueSize = DValueSize.OCTETSTRING,
            ByteAddress = byteAddr,
            Count = 1,
            DataType = typeof(float),
            Datas = new List<object> { value }
        };


        private DataParameter BitRead(int byteAddr, byte bitAddr, ushort dBNumber) => new DataParameter
        {
            Area = Areas.DataBlock,
            DBNumber = dBNumber,
            PValueSize = PValueSize.BIT,
            DValueSize = DValueSize.BIT,
            ByteAddress = byteAddr,
            BitAddress = bitAddr,
            Count = 1,
            DataType = typeof(bool)
        };
        private DataParameter WordRead(int byteAddr, ushort dBNumber) => new DataParameter
        {
            Area = Areas.DataBlock,
            DBNumber = dBNumber,
            PValueSize = PValueSize.WORD,
            DValueSize = DValueSize.BWD,
            ByteAddress = byteAddr,
            Count = 1,
            DataType = typeof(short)
        };
        private DataParameter StringRead(int byteAddr, int length, ushort dBNumber) => new DataParameter
        {
            Area = Areas.DataBlock,
            DBNumber = dBNumber,
            PValueSize = PValueSize.BYTE,
            DValueSize = DValueSize.OCTETSTRING,
            ByteAddress = byteAddr,
            Count = (ushort)length,
            DataType = typeof(string)
        };
        private DataParameter RealRead(int byteAddr, ushort dBNumber) => new DataParameter
        {
            Area = Areas.DataBlock,
            DBNumber = dBNumber,
            PValueSize = PValueSize.REAL,
            DValueSize = DValueSize.OCTETSTRING,
            ByteAddress = byteAddr,
            Count = 1,
            DataType = typeof(float)
        };
        private DataParameter DIntRead(int byteAddr, ushort dBNumber) => new DataParameter
        {
            Area = Areas.DataBlock,
            DBNumber = dBNumber,
            PValueSize = PValueSize.DWORD,
            DValueSize = DValueSize.DWORD,
            ByteAddress = byteAddr,
            Count = 1,
            DataType = typeof(int)
        };

        public List<DataParameter> ReadDataParameters(List<DataParameter> parameters)
        {
            var result = new List<DataParameter>();
            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }

        public Result Write_readClose_P(DataType dataType, int db, int startByteAdr, object value, byte bitAddr = 0)
        {

            try
            {
                if (bitAddr > 0 && value is bool boolValue)
                {
                    // 位写入
                    var bytes = _plc.ReadBytes(dataType, db, startByteAdr, 1);
                    if (boolValue)
                        bytes[0] |= (byte)(1 << bitAddr);
                    else
                        bytes[0] &= (byte)~(1 << bitAddr);
                    _plc.WriteBytes(dataType, db, startByteAdr, bytes);
                }
                else
                {
                    _plc.Write(dataType, db, startByteAdr, value);
                }

                return new Result { Status = true };
            }
            catch (Exception ex)
            {
                return new Result { Status = false, Message = $"写入数据异常: {ex.Message}" };
            }

        }
        public void WriteDB3001Data_readClose_P80(P80DB3004ReturnData returnData)
        {
            try
            {
                // 创建所有需要写入的参数
                var parameters = new List<DataParameter>();
                // 写入布尔信号 (0.0)
                parameters.Add(CreateBitWriteParameter(0, 0, returnData.ReadFlag,(ushort)DBNumber.P80_wor));                           // 0.0 读取标志位
                var result = Write_readClose_P(parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"写入DB3001数据失败: {ex.Message}", ex);
            }
        }
        public void WriteDB3001Data_readClose_P70(P70DB3003ReturnData returnData)
        {
            try
            {
                // 创建所有需要写入的参数
                var parameters = new List<DataParameter>();
                // 写入布尔信号 (0.0, 0.1, 14.0, 40.0, 66.0)
                parameters.Add(CreateBitWriteParameter(14, 0, returnData.ReadFlag1, (ushort)DBNumber.P70_wor));                          // 0.0 读取标志位1
                parameters.Add(CreateBitWriteParameter(40, 0, returnData.ReadFlag1, (ushort)DBNumber.P70_wor));                          // 0.0 读取标志位1
                parameters.Add(CreateBitWriteParameter(66, 0, returnData.ReadFlag1, (ushort)DBNumber.P70_wor));                          // 0.0 读取标志位1
                // 执行写入
                var result = Write_readClose_P(parameters);
            }
            catch (Exception ex)
            {
            }
        }
        public Result Write_readClose_P(List<DataParameter> parameters)
        {
            try
            {
                Result result = new Result { Status = true };
                // 使用EnhancedS7Comm逐个写入参数
                foreach (var param in parameters)
                {
                    try
                    {
                        var dataType = ConvertAreasToDataType(param.Area);
                        var varType = ConvertPValueSizeToVarType(param.PValueSize);

                        // 位写入
                        if (param.PValueSize == PValueSize.BIT)
                        {
                            bool writeSuccess = true;
                            foreach (var data in param.Datas)
                            {
                                var writeResult = Write_readClose_P(dataType, param.DBNumber, param.ByteAddress, (bool)data, param.BitAddress);
                                if (!writeResult.Status)
                                {
                                    param.Status = false;
                                    param.Error = writeResult.Message;
                                    result.Status = false;
                                    writeSuccess = false;
                                    break;
                                }
                            }
                            if (writeSuccess)
                            {
                                param.Status = true;
                            }
                        }
                        // 数组写入
                        else if (param.Datas.Count > 1)
                        {
                            bool writeSuccess = true;
                            for (int i = 0; i < param.Datas.Count; i++)
                            {
                                int offset = param.ByteAddress + i * GetTypeSize_readClose_P70(param.DataType);
                                var writeResult = Write_readClose_P(dataType, param.DBNumber, offset, param.Datas[i], 0);

                                if (!writeResult.Status)
                                {
                                    param.Status = false;
                                    param.Error = writeResult.Message;
                                    result.Status = false;
                                    writeSuccess = false;
                                    break;
                                }
                            }
                            if (writeSuccess)
                            {
                                param.Status = true;
                            }
                        }
                        // 单个值写入
                        else if (param.Datas.Count == 1)
                        {
                            var writeResult = Write_readClose_P(dataType, param.DBNumber, param.ByteAddress, param.Datas[0], 0);
                            if (writeResult.Status)
                            {
                                param.Status = true;
                            }
                            else
                            {
                                param.Status = false;
                                param.Error = writeResult.Message;
                                result.Status = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        param.Status = false;
                        param.Error = $"写入参数异常: {ex.Message}";
                        result.Status = false;
                    }
                }

                // 检查是否有失败的参数
                var failedParams = parameters.Where(p => !p.Status).ToList();
                if (failedParams.Count > 0)
                {
                    result.Status = false;
                    result.Message = $"部分参数写入失败: {string.Join(", ", failedParams.Select(p => p.Error))}";
                }

                return result;
            }
            catch (Exception ex)
            {
                return new Result { Status = false, Message = $"写入PLC数据异常: {ex.Message}" };
            }

        }
        private int GetTypeSize_readClose_P70(Type dataType)
        {
            if (dataType == typeof(bool)) return 1;
            if (dataType == typeof(byte)) return 1;
            if (dataType == typeof(short)) return 2;
            if (dataType == typeof(ushort)) return 2;
            if (dataType == typeof(int)) return 4;
            if (dataType == typeof(uint)) return 4;
            if (dataType == typeof(float)) return 4;
            if (dataType == typeof(double)) return 8;
            if (dataType == typeof(string)) return 1;
            return Marshal.SizeOf(dataType);
        }
        private DataParameter CreateBitWriteParameter(int byteAddr, byte bitAddr, bool value,ushort dbNumber)
        {
            var param = new DataParameter
            {
                Area = Areas.DataBlock,
                DBNumber = dbNumber,
                PValueSize = PValueSize.BIT,
                DValueSize = DValueSize.BIT,
                ByteAddress = byteAddr,
                BitAddress = bitAddr,
                Count = 1,
                DataType = typeof(bool)
            };
            param.Datas.Add(value);
            return param;
        }
        public void WriteDB3000Data_readClose_P60(P60DB3000ReturnData returnData)
        {
            try
            {
                // 创建所有需要写入的参数
                var parameters = new List<DataParameter>();
                // 写入布尔信号 (0.0)
                parameters.Add(CreateBitWriteParameter(0, 0, returnData.ReadFlag, (ushort)DBNumber.P60_wor));              // 0.0 读取标志位
                // 执行写入
                var result = Write_readClose_P(parameters);
                if (!result.Status)
                {
                    throw new Exception($"写入PLC数据失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"写入DB3000数据失败: {ex.Message}", ex);
            }
        }
        public void WriteDB3001Data_readClose_P40(P40DB3001ReturnData returnData)
        {
            try
            {
                // 创建所有需要写入的参数
                var parameters = new List<DataParameter>();
                // 写入布尔信号 (0.0)
                parameters.Add(CreateBitWriteParameter(0, 0, returnData.ReadFlag, (ushort)DBNumber.P40_wor));                           // 0.0 读取标志位
                // 执行写入
                var result = Write_readClose_P(parameters);
                if (!result.Status)
                {
                    throw new Exception($"写入PLC数据失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"写入DB3001数据失败: {ex.Message}", ex);
            }
        }
        public void WriteDB3002Data_readClose_P50(P50DB3002ReturnData returnData)
        {
            try
            {
                // 创建所有需要写入的参数
                var parameters = new List<DataParameter>();
                // 写入布尔信号 (0.0)
                parameters.Add(CreateBitWriteParameter(0, 0, returnData.ReadFlag,(ushort)DBNumber.P50_wor));                           // 0.0 读取标志位
                // 执行写入
                var result = Write_readClose_P(parameters);
                if (!result.Status)
                {
                    throw new Exception($"写入PLC数据失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"写入DB3002数据失败: {ex.Message}", ex);
            }
        }
        public void WriteDB3000Data_readClose_P30(P30DB3000ReturnData returnData)
        {
            try
            {
                // 创建所有需要写入的参数
                var parameters = new List<DataParameter>();
                // 写入布尔信号 (0.0)
                parameters.Add(CreateBitWriteParameter(0, 0, returnData.ReadFlag,(ushort)DBNumber.P30_wor));                           // 0.0 读取标志位
                // 执行写入
                var result = Write_readClose_P(parameters);
                if (!result.Status)
                {
                    throw new Exception($"写入PLC数据失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"写入DB3000数据失败: {ex.Message}", ex);
            }
        }
        public void WriteDB3000Data_readClose_P35(P30DB3000ReturnData returnData)
        {
            try
            {
                // 创建所有需要写入的参数
                var parameters = new List<DataParameter>();
                // 写入布尔信号 (0.0)
                parameters.Add(CreateBitWriteParameter(0, 0, returnData.ReadFlag, (ushort)DBNumber.P35_wor));                           // 0.0 读取标志位
                // 执行写入
                var result = Write_readClose_P(parameters);
                if (!result.Status)
                {
                    throw new Exception($"写入PLC数据失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"写入DB3000数据失败: {ex.Message}", ex);
            }
        }
        public void WriteDB2000Data_readClose_P20(P20DB2000ReturnData returnData)
        {
            try
            {
                // 创建所有需要写入的参数
                var parameters = new List<DataParameter>();
                // 写入布尔信号 (0.0)
                parameters.Add(CreateBitWriteParameter(0, 0, returnData.ReadFlag,(ushort)DBNumber.P20_wor));                           // 0.0 读取标志位
                var result = Write_readClose_P(parameters);
                if (!result.Status)
                {
                    throw new Exception($"写入PLC数据失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"写入DB2000数据失败: {ex.Message}", ex);
            }
        }
        public void WriteDB2000Data_readClose_P10(P10DB2000ReturnData returnData)
        {
            try
            {
                // 第一次写入：写入布尔信号和前半部分浮点型数据 (0.0~38.0)
                var parameters1 = new List<DataParameter>();
                // 写入布尔信号 (0.0)
                parameters1.Add(CreateBitWriteParameter(0, 0, returnData.ReadFlag,(ushort)DBNumber.P10_wor));                           // 0.0 读取标志位
                var result1 = Write_readClose_P(parameters1);
            }
            catch (Exception ex)
            {
                throw new Exception($"写入DB2000数据失败: {ex.Message}", ex);
            }
        }
        public List<DataParameter> ReadDataParameters_P70_biaoding(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }
            return result;
        }
        public P70DB3001UploadData2 ReadDB3001Data2_P70_fuzai()
        {
            try
            {
                var uploadData = new P70DB3001UploadData2();
                // 先读取MES可读标志
                var flagParam = BitRead(142, 0,(ushort)DBNumber.P70_wor);
                var flagResult = ReadDataParameters(new List<DataParameter> { flagParam });

                uploadData.MESCanRead = (bool)flagParam.Datas[0];
                // 创建所有需要读取的参数
                var parameters = new List<DataParameter>();
                // 添加浮点型数据读取参数 
                parameters.Add(RealRead(208, (ushort)DBNumber.P70_wor));                   // 2.0 500N力电流
                parameters.Add(RealRead(212, (ushort)DBNumber.P70_wor));                   // 6.0 600N力电流
                parameters.Add(RealRead(216, (ushort)DBNumber.P70_wor));                 // 10.0 最大电流
                parameters.Add(RealRead(220, (ushort)DBNumber.P70_wor));                 // 14.0 最大电压
                parameters.Add(RealRead(224, (ushort)DBNumber.P70_wor));                 // 18.0 最大扭力
                parameters.Add(RealRead(228, (ushort)DBNumber.P70_wor));                // 22.0 最大压力 
                parameters.Add(RealRead(244, (ushort)DBNumber.P70_wor));                // 30.0 最大压力
                parameters.Add(RealRead(248, (ushort)DBNumber.P70_wor));                // 34.0 最大压力
                parameters.Add(RealRead(252, (ushort)DBNumber.P70_wor));                // 34.0 最大压力
                parameters.Add(RealRead(240, (ushort)DBNumber.P70_wor));                // 26.0 最大压力
                // 执行读取
                var result = ReadDataParameters(parameters);
                // 解析读取结果
                int index = 0;
                // 解析浮点型数据 (2.0, 6.0, 10.0, 14.0, 18.0, 22.0, 26.0, 30.0, 34.0, 38.0, 42.0, 46.0, 50.0)
                uploadData.ForceCurrent500n = (float)parameters[index++].Datas[0];                   // 2.0 31HZ声压值
                uploadData.ForceCurrent600n = (float)parameters[index++].Datas[0];                   // 6.0 63HZ声压值
                uploadData.MaxCurrent = (float)parameters[index++].Datas[0];                 // 10.0 125HZ声压值
                uploadData.MaxVoltage = (float)parameters[index++].Datas[0];                 // 14.0 250HZ声压值
                uploadData.MaxTorque = (float)parameters[index++].Datas[0];                 // 18.0 500HZ声压值
                uploadData.MaxPressure = (float)parameters[index++].Datas[0];                 // 18.0 500HZ声压值

                uploadData.Reserve1 = (float)parameters[index++].Datas[0];                 // 10.0 125HZ声压值
                uploadData.Reserve2 = (float)parameters[index++].Datas[0];                 // 14.0 250HZ声压值
                uploadData.Reserve3 = (float)parameters[index++].Datas[0];                 // 18.0 500HZ声压值
                uploadData.Reserve4 = (float)parameters[index++].Datas[0];                 // 18.0 500HZ声压值
                //读取主条码 DB1000.6 长度50
                var parameters3 = new List<DataParameter>();
                parameters3.Add(new DataParameter
                {
                    Area = Areas.DataBlock,
                    DBNumber = 1500,
                    PValueSize = PValueSize.CHAR,
                    DValueSize = DValueSize.OCTETSTRING,
                    ByteAddress = 8,
                    Count = 48,
                    DataType = typeof(string)
                });
                var result3 = ReadDataParameters(parameters3);
                return uploadData;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取DB3001数据失败: {ex.Message}", ex);
            }
        }
        public P70DB3001UploadData ReadDB3001Data_P70_biaoding()
        {
            try
            {
                var uploadData = new P70DB3001UploadData();
                // 先读取MES可读标志
                var flagParam = BitRead(143, 0,(ushort)DBNumber.P70_wor);
                var flagResult = ReadDataParameters_P70_biaoding(new List<DataParameter> { flagParam });

                uploadData.MESCanRead = (bool)flagParam.Datas[0];

                ReadDataBatch1(uploadData);
                ReadDataBatch2(uploadData);
                ReadDataBatch3(uploadData);

                return uploadData;
            }
            catch (Exception ex)
            {
                throw new Exception($"读取DB3001数据失败: {ex.Message}", ex);
            }
        }
        private void ReadDataBatch1(P70DB3001UploadData data)
        {
            var parameters = new List<DataParameter>();

            // 读取浮点数 (2.0, 6.0, 10.0)
            parameters.Add(RealRead(2,(ushort)DBNumber.P70_wor));        // 2.0 实际距离     // 最大长度  
            parameters.Add(RealRead(6, (ushort)DBNumber.P70_wor));        // 6.0 MAG_LOW值C   // MAG_LOW值C
            parameters.Add(RealRead(10, (ushort)DBNumber.P70_wor));       // 10.0 MAG_LOW值A  // 10.0 MAG_LOW值A 

            var result = ReadDataParameters_P70_biaoding(parameters);

            int index = 0;
            data.ActualDistance = (float)parameters[index++].Datas[0];
            data.MagLowValueC = (float)parameters[index++].Datas[0];
            data.MagLowValueA = Convert.ToInt32(parameters[index++].Datas[0]);
        }
        private void ReadDataBatch2(P70DB3001UploadData data)
        {
            var parameters = new List<DataParameter>();

            // 读取浮点数 (144.0-168.0)
            parameters.Add(RealRead(248, (ushort)DBNumber.P70_wor));      // 144.0 C SLOT   //  C_SLOT
            parameters.Add(RealRead(244, (ushort)DBNumber.P70_wor));      // 146.0 B SLOT   //  B_SL0T
            parameters.Add(RealRead(240, (ushort)DBNumber.P70_wor));      // 148.0 A SLOT   //  A_SLOT
            parameters.Add(RealRead(252, (ushort)DBNumber.P70_wor));      // 150.0 A倒数第一个点
            parameters.Add(RealRead(256, (ushort)DBNumber.P70_wor));      // 152.0 A倒数第二个点
            parameters.Add(RealRead(260, (ushort)DBNumber.P70_wor));      // 154.0 B倒数第一个点
            parameters.Add(RealRead(272, (ushort)DBNumber.P70_wor));      // 156.0 B倒数第二个点
            parameters.Add(RealRead(264, (ushort)DBNumber.P70_wor));      // 158.0 C倒数第一个点
            parameters.Add(RealRead(268, (ushort)DBNumber.P70_wor));      // 160.0 C倒数第二个点
            parameters.Add(RealRead(164, (ushort)DBNumber.P70_wor));      // 164.0 MAG_LOW值B
            parameters.Add(RealRead(168, (ushort)DBNumber.P70_wor));      // 168.0 AMPLITUDE值C

            var result = ReadDataParameters_P70_biaoding(parameters);

            int index = 0;
            data.CSLOT = Convert.ToInt32(parameters[index++].Datas[0]);
            data.BSLOT = Convert.ToInt32(parameters[index++].Datas[0]);
            data.ASLOT = Convert.ToInt32(parameters[index++].Datas[0]);
            data.APenultimatePoint = (float)parameters[index++].Datas[0];
            data.AThirdLastPoint = (float)parameters[index++].Datas[0];
            data.BPenultimatePoint = (float)parameters[index++].Datas[0];
            data.BThirdLastPoint = (float)parameters[index++].Datas[0];
            data.CPenultimatePoint = (float)parameters[index++].Datas[0];
            data.CThirdLastPoint = (float)parameters[index++].Datas[0];
            data.MagLowValueB = (float)parameters[index++].Datas[0];
            data.AmplitudeValueC = Convert.ToInt32(parameters[index++].Datas[0]);
        }

        /// <summary>
        /// 第三批读取：后面部分数据 (172.0-204.0)
        /// </summary>
        private void ReadDataBatch3(P70DB3001UploadData data)
        {
            var parameters = new List<DataParameter>();

            // 读取浮点数 (172.0-204.0)
            parameters.Add(RealRead(172, (ushort)DBNumber.P70_wor));      // 172.0 AMPLITUDE值A
            parameters.Add(RealRead(176, (ushort)DBNumber.P70_wor));      // 176.0 AMPLITUDE值B
            parameters.Add(RealRead(180, (ushort)DBNumber.P70_wor));      // 180.0 感应距离
            parameters.Add(RealRead(184, (ushort)DBNumber.P70_wor));      // 184.0 A第一个点
            parameters.Add(RealRead(188, (ushort)DBNumber.P70_wor));      // 188.0 A第二个点
            parameters.Add(RealRead(192, (ushort)DBNumber.P70_wor));      // 192.0 B第一个点
            parameters.Add(RealRead(196, (ushort)DBNumber.P70_wor));      // 196.0 B第二个点
            parameters.Add(RealRead(200, (ushort)DBNumber.P70_wor));      // 200.0 C第一个点
            parameters.Add(RealRead(204, (ushort)DBNumber.P70_wor));      // 204.0 C第二个点

            parameters.Add(RealRead(236, (ushort)DBNumber.P70_wor));      // 204.0 最外距离

            parameters.Add(RealRead(232, (ushort)DBNumber.P70_wor));      // 204.0 最内距离

            var result = ReadDataParameters_P70_biaoding(parameters);

            int index = 0;
            data.AmplitudeValueA = Convert.ToInt32(parameters[index++].Datas[0]);
            data.AmplitudeValueB = Convert.ToInt32(parameters[index++].Datas[0]);
            data.SensingDistance = (float)parameters[index++].Datas[0];
            data.AFirstPoint = (float)parameters[index++].Datas[0];
            data.ASecondPoint = (float)parameters[index++].Datas[0];
            data.BFirstPoint = (float)parameters[index++].Datas[0];
            data.BSecondPoint = (float)parameters[index++].Datas[0];
            data.CFirstPoint = (float)parameters[index++].Datas[0];
            data.CSecondPoint = (float)parameters[index++].Datas[0];
            data.OutermostDistance = (float)parameters[index++].Datas[0];
            data.InnermostDistance = (float)parameters[index++].Datas[0];
        }
        public void WriteDB3001Data_P70_readClose(P70DB3001UploadData uploadData)
        {
            try
            {
                var parameters = new List<DataParameter>();

                // 写入布尔值
                parameters.Add(CreateBitWriteParameter(143, 0, uploadData.MESCanRead,(ushort)DBNumber.P70_wor));

                var result = Write_readClose_P(parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"写入DB3001数据失败: {ex.Message}", ex);
            }
        }
        public void WriteDB3001Data2_P70_fuzai(P70DB3001UploadData2 uploadData)
        {
            try
            {
                var parameters = new List<DataParameter>();
                // 写入布尔值
                parameters.Add(CreateBitWriteParameter(142, 0, uploadData.MESCanRead, (ushort)DBNumber.P70_wor));
                var result = Write_readClose_P(parameters);
                if (!result.Status)
                {
                    throw new Exception($"写入PLC数据失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"写入DB3001数据失败: {ex.Message}", ex);
            }
        }

        public void LickMicosOrder(string tableName,string Barcode)
        {
            string sqlDel = "delete from "+ tableName + " where ProductBarCode='" + Barcode + "'";
            string sqlInsert = "insert into "+ tableName + "(CreatedTime,ProductBarCode)" +
                "values('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + Barcode + "')";
            GlobalSqlExecute.sGlobalSqlExecute.AddExecSql(new SqlItemObj()
            {
                IsProcedure = true,
                ProSqlString = new List<string>() { sqlDel, sqlInsert }
            });
        }
        #endregion
    }
     
    public enum DBNumber
    {
        P10_ope =1000,
        P10_wor = 2000,
        P20_ope =1000,
        P20_wor = 2000,
        P30_ope =1000,
        P30_wor = 3000,
        P35_ope = 1500,
        P35_wor = 3000,
        P40_ope =1500,
        P40_wor = 3001,
        P50_ope=2000,
        P50_wor = 3002,
        P60_ope = 1000,
        P60_wor = 3000,
        P70_ope = 1500,
        P70_wor = 3001,
        P80_ope = 1000,
        P80_wor = 3001,
    }
    public enum CommandType
    {
        机械臂右 = 3,
        机械臂左 = 4,
        起落架右 = 5,
        起落架左 = 6
    }
}
