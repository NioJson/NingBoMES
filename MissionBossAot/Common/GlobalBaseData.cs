using MissionBossAot.Models;
using MissionBossAot.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Common
{
    /// <summary>
    /// 全局基础数据查询
    /// </summary>
    public class GlobalBaseData
    {
        public static GlobalBaseData sGlobalBaseData=new GlobalBaseData();
        public GlobalBaseData() { }

        //通过表名过程码查询数据
        public DataTable GetStationData(string tableName, string proCode)
        {
            DataTable resDT = null;
            try
            {
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM " + tableName + " WHERE Product_BarCode='" + proCode + "' order by CreatedTime desc";
                    resDT = helper.ExecuteDataTable(sqlSelect);
                }
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { Content = "GetStationData 查询操作失败，具体信息:" + ex.Message });
            }
            if (resDT != null)
            {
                DataExport.SetColumnCaptions(resDT);
            }
            else
            {
                resDT = new DataTable();
            }
            return resDT;
        }

        public WorkOrderLinkCode GetWorkOrderLinkCode(string code)
        {
            WorkOrderLinkCode workOrderLinkCode = null;
            DataTable dt = null;
            try
            {
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM WorkOrderNewProductCode where ProcessCode='"+ code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    workOrderLinkCode = new WorkOrderLinkCode();
                    DataRow row = dt.Rows[0];
                    string idInt = !Convert.IsDBNull(row["WorkOrderId"]) ? row["WorkOrderId"].ToString() : "";
                    int.TryParse(idInt, out int idI);
                    workOrderLinkCode.WorkOrderId = idI;
                    workOrderLinkCode.ProcessCode = code;
                }
            }
            catch (Exception)
            {
            }
            return workOrderLinkCode;
        }
        //查询工单工站绑定关系
        public List<WorkOrderStation> GetWorkOrderLinkStation(int staId = -1)
        {
            List<WorkOrderStation> workOrderSta = new List<WorkOrderStation>();
            DataTable dt = null;
            try
            {
                string sqlSub = "";
                if (staId > 0)
                {
                    sqlSub = " and StationId=" + staId;
                }
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM WorkOrderStation where 1=1 " + sqlSub;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string idInt = !Convert.IsDBNull(row["ID"]) ? row["ID"].ToString() : "";
                        int.TryParse(idInt, out int idI);
                        string staIdInt = !Convert.IsDBNull(row["StationId"]) ? row["StationId"].ToString() : "";
                        int.TryParse(staIdInt, out int staIdInti);

                        workOrderSta.Add(new WorkOrderStation()
                        {
                            OrderUUID = !Convert.IsDBNull(row["WorkOrderUUID"]) ? row["WorkOrderUUID"].ToString() : "",
                            Id = idI,
                            StationId = staIdInti
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return workOrderSta;
        }
        public bool GetP70UpStatus(string code)
        {
            DataTable dt = null;
            try
            {
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P60DB3000ReturnData_Detail where Product_BarCode='"+ code + "' order by CreatedTime desc";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    string isNg = !Convert.IsDBNull(row["IsNG"]) ? row["IsNG"].ToString() : "";
                    if (isNg != "0")
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
        //查询工单信息
        public List<WorkOrderModel> GetWorkOrders(int woId = -1,string productCode="",List<string> uuidList=null)
        {
            List<WorkOrderModel> workOrders = new List<WorkOrderModel>();
            DataTable dt = null;
            try
            {
                string sqlSub = "";
                if (woId > 0)
                {
                    sqlSub = " and ID=" + woId;
                }
                if (!string.IsNullOrEmpty(productCode))
                {
                    sqlSub += " and ProductCode='"+ productCode + "'";
                }
                if (uuidList != null && uuidList.Count > 0) 
                {
                    sqlSub += " and WorkOrderUUID in(";
                    for (int i = 0; i < uuidList.Count; i++) 
                    {
                        sqlSub += "'"+ uuidList[i] + "'";
                        if (i != (uuidList.Count-1))
                        {
                            sqlSub += ",";
                        }
                    }
                    sqlSub += ")";
                }
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM WorkOrderNew where 1=1 " + sqlSub;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows) 
                    {
                        string amount = !Convert.IsDBNull(row["OrderAmount"]) ? row["OrderAmount"].ToString() : "";
                        int.TryParse(amount, out int resInt);

                        string idInt = !Convert.IsDBNull(row["ID"]) ? row["ID"].ToString() : "";
                        int.TryParse(idInt, out int idI);
                        
                        workOrders.Add(new WorkOrderModel()
                        {
                            WorkOrderUUID = !Convert.IsDBNull(row["WorkOrderUUID"]) ? row["WorkOrderUUID"].ToString() : "",
                            Id = idI,
                            OrderId = !Convert.IsDBNull(row["OrderId"]) ? row["OrderId"].ToString() : "",
                            ModelText = !Convert.IsDBNull(row["ModelText"]) ? row["ModelText"].ToString() : "",
                            ModelValue = !Convert.IsDBNull(row["ModelValue"]) ? row["ModelValue"].ToString() : "",
                            FlowOrderId = !Convert.IsDBNull(row["FlowOrderId"]) ? row["FlowOrderId"].ToString() : "",
                            RoutingText = !Convert.IsDBNull(row["RoutingText"]) ? row["RoutingText"].ToString() : "",
                            ProductName = !Convert.IsDBNull(row["ProductName"]) ? row["ProductName"].ToString() : "",
                            OrderAmount = resInt,
                            WorkBackNo = !Convert.IsDBNull(row["WorkBackNo"]) ? row["WorkBackNo"].ToString() : "",
                            EnableStatus = !Convert.IsDBNull(row["EnableStatus"]) ? row["EnableStatus"].ToString() : "",
                            CreateDatetime = !Convert.IsDBNull(row["CreateDatetime"]) ? row["CreateDatetime"].ToString() : "",
                            OrderStartDatetime = !Convert.IsDBNull(row["OrderStartDatetime"]) ? row["OrderStartDatetime"].ToString() : "",
                            PlanedCompleteDatetime = !Convert.IsDBNull(row["OrderStartDatetime"]) ? row["OrderStartDatetime"].ToString() : "",
                            FormulaText = !Convert.IsDBNull(row["FormulaText"]) ? row["FormulaText"].ToString() : "",
                            FormulaValue = !Convert.IsDBNull(row["FormulaValue"]) ? row["FormulaValue"].ToString() : ""
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return workOrders;
        }
        /// <summary>
        /// 查询所有工站信息
        /// </summary>
        /// <returns></returns>
        public List<WorkStationItem> GetWorkOrderStation(bool enableSta = false)
        {
            List<WorkStationItem> workStationItems = new List<WorkStationItem>();
            DataTable dt = null;
            try
            {
                string sqlSub = "";
                if (enableSta)
                {
                    sqlSub = " and EnableStatus='T'";
                }
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM WorkStation where 1=1" + sqlSub;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        workStationItems.Add(new WorkStationItem
                        {
                            ConnectType= !Convert.IsDBNull(row["PlcConnectType"]) ? row["PlcConnectType"].ToString() : "",
                            Id = !Convert.IsDBNull(row["ID"]) ? int.Parse(row["ID"].ToString()) : 0,
                            ExecutionOrder = !Convert.IsDBNull(row["ExecutionOrder"]) ? int.Parse(row["ExecutionOrder"].ToString()) : 0,
                            Code = !Convert.IsDBNull(row["StationCode"]) ? row["StationCode"].ToString() : "",
                            Name = !Convert.IsDBNull(row["StationName"]) ? row["StationName"].ToString() : "",
                            PlcIp = !Convert.IsDBNull(row["PlcIp"]) ? row["PlcIp"].ToString() : "",
                            PlcPort = !Convert.IsDBNull(row["PlcPort"]) ? row["PlcPort"].ToString() : "",
                            IsEnabled = !Convert.IsDBNull(row["EnableStatus"]) ? (row["EnableStatus"].ToString() == "T" ? "是" : "否") : "",
                            CreateTime = !Convert.IsDBNull(row["CreateDatetime"]) ? Convert.ToDateTime(row["CreateDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") : ""
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return workStationItems;
        }
        public WorkOrderStationResultStatus GetWorkOrderMissionStatus(string tableName, string tableNameXcode, string tableNameMico)
        {
            WorkOrderStationResultStatus workOrderStationResultStatus = new WorkOrderStationResultStatus();
            DataTable dt = null;
            try
            {
                List<WorkOrderLinkCode> works = new List<WorkOrderLinkCode>();
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM "+ tableName + " where CreatedTime >= CAST(GETDATE() AS DATE)AND CreatedTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE))";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    workOrderStationResultStatus.ProductionCount = dt.Rows.Count;
                    int ngint = 0;
                    int okint = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        string ngString = !Convert.IsDBNull(row["IsNG"]) ? row["IsNG"].ToString() : "";
                        if (ngString == "NG")
                        {
                            ngint++;
                        }
                        else
                        {
                            okint++;
                        }
                        string codeString = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                        var dsx = works.FindAll(a => a.ProcessCode == codeString).FirstOrDefault();
                        if (dsx == null)
                        {
                            works.Add(new WorkOrderLinkCode() { ProcessCode = codeString });
                        }
                    }
                    workOrderStationResultStatus.OkCount = okint;
                    workOrderStationResultStatus.NgCount = ngint;
                }


                if (works.Count > 0)
                {
                    string sqlx = "(";
                    for (int i = 0; i < works.Count; i++)
                    {
                        sqlx += "'"+ works[i].ProcessCode + "'";
                        if (i != (works.Count - 1))
                        {
                            sqlx += ",";
                        }
                    }
                    sqlx += ")";


                    DataTable dtMico = null;
                    using (var helper = new SqlConnectionHelper())
                    {
                        string sqlSelect = "SELECT * FROM " + tableNameMico + " where Product_BarCode in " + sqlx;
                        dtMico = helper.ExecuteDataTable(sqlSelect);
                    }
                    List<P10DB2000ReturnData> daxd = new List<P10DB2000ReturnData>();
                    if (dtMico != null && dtMico.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtMico.Rows)
                        {
                            string codeString = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                            string createdTime = !Convert.IsDBNull(row["CreatedTime"]) ? row["CreatedTime"].ToString() : "";
                            if (DateTime.TryParse(createdTime,out DateTime dateTimeSet))
                            {
                                daxd.Add(new P10DB2000ReturnData() { Product_BarCode = codeString, CreatedTime = dateTimeSet });
                            }
                        }
                        
                    }

                    if (daxd.Count > 0)
                    {
                        DataTable dtXcode = null;
                        using (var helper = new SqlConnectionHelper())
                        {
                            string sqlSelect = "SELECT * FROM " + tableNameXcode + " where ProductBarCode in " + sqlx;
                            dtXcode = helper.ExecuteDataTable(sqlSelect);
                        }
                        List<WorkOrderXcodeBock> workOrders = new List<WorkOrderXcodeBock>();
                        if (dtXcode != null && dtXcode.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtXcode.Rows)
                            {
                                string codeString = !Convert.IsDBNull(row["ProductBarCode"]) ? row["ProductBarCode"].ToString() : "";
                                string createdTime = !Convert.IsDBNull(row["CreatedTime"]) ? row["CreatedTime"].ToString() : "";
                                if (DateTime.TryParse(createdTime, out DateTime dateTimeSet))
                                {
                                    workOrders.Add(new WorkOrderXcodeBock() { ProductBarCode = codeString, CreatedTime = dateTimeSet });
                                }
                            }
                        }
                        if (workOrders.Count > 0)
                        {
                            int countdox = 0;
                            foreach (WorkOrderXcodeBock item in workOrders)
                            {
                                var dxdll=daxd.FindAll(a => a.Product_BarCode == item.ProductBarCode).FirstOrDefault();
                                if (dxdll != null)
                                {
                                    double totalMinutes = (dxdll.CreatedTime - item.CreatedTime).TotalSeconds;
                                    countdox++;
                                    workOrderStationResultStatus.timeClockCount += (float)totalMinutes;
                                }
                            }
                            if (countdox >0)
                            {
                                workOrderStationResultStatus.timeClockCountPer = (float)Math.Round(workOrderStationResultStatus.timeClockCount / countdox, 2);
                                
                            }
                        }
                    }
                   

                    
                }




                
            }
            catch (Exception)
            {
            }
            return workOrderStationResultStatus;
        }
    }
    public class WorkOrderStationResultStatus
    {
        public int OkCount { get; set; }
        public int NgCount { get; set; }
        public int ProductionCount { get; set; }
        public float timeClockCount { get; set; }
        public float timeClockCountPer { get; set; }
    }
    public class WorkOrderStation
    {
        public int Id { get; set; }
        public string OrderUUID { get; set; }
        public int StationId { get; set; }
    }
    public class WorkOrderLinkCode
    {
        public int WorkOrderId { get; set; }
        public string ProcessCode { get; set; }
    }
    public class WorkOrderXcodeBock
    {
        public DateTime CreatedTime { get; set; }
        public string ProductBarCode { get; set; }
    }
}
