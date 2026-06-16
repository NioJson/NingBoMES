using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    public class P60DB3000ReturnData
    {
        /// <summary>
        /// 读取标志位 (0.0)
        /// </summary>
        public bool ReadFlag { get; set; }

        /// <summary>
        /// 时间日期 (2.0)
        /// </summary>
        public string TimeDate { get; set; } = string.Empty;

        /// <summary>
        /// 泄露率 (20.0)
        /// </summary>
        public string LeakRate { get; set; } = string.Empty;

        /// <summary>
        /// 主条码（进站时扫描的条码）
        /// </summary>
        public string Product_BarCode { get; set; } = string.Empty;
        public bool IsNG {  get; set; }
    }
}
