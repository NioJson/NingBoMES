using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }
        public string WorkOrderNo { get; set; }          // 工单号
        public string Model { get; set; }                 // 型号
        public string IssueSerialNo { get; set; }         // 下发流水号
        public string ProcessRoute { get; set; }          // 工艺路线
        public string ProductName { get; set; }           // 产品名称
        public int OrderQuantity { get; set; }            // 订单数量
        public string OfflineSerialNo { get; set; }       // 下线流水号
        public string ReworkCode { get; set; }            // 返工码
        public DateTime StartTime { get; set; }           // 工单开始时间
        public DateTime PlanCompleteTime { get; set; }    // 计划完成时间
        public string FormulaName { get; set; }           // 配方名称
        public string ProductionTeam { get; set; }        // 生产班组
        public string Remarks { get; set; }               // 备注
        public string ExecuteStation { get; set; }        // 执行工站（多选，用逗号分隔）
        public string Status { get; set; }                // 状态：未下发、已下发
        public DateTime CreateTime { get; set; }          // 创建时间
    }
}
