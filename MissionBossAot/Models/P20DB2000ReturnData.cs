using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MissionBossAot.Models
{
    /// <summary>
    /// P20工站DB2000返回数据模型（上位机→PLC）
    /// </summary>
    public class P20DB2000ReturnData
    {
        /// <summary>
        /// 读取标志位 (0.0)
        /// </summary>
        public bool ReadFlag { get; set; }

        /// <summary>
        /// 轮次1压头1压装高度 (2.0)
        /// </summary>
        public float Cycle1Head1PressHeight { get; set; }

        /// <summary>
        /// 轮次1压头1最终压力 (6.0)
        /// </summary>
        public float Cycle1Head1FinalPressure { get; set; }

        /// <summary>
        /// 轮次2压头2压装高度 (10.0)
        /// </summary>
        public float Cycle2Head2PressHeight { get; set; }

        /// <summary>
        /// 轮次2压头2最终压力 (14.0)
        /// </summary>
        public float Cycle2Head2FinalPressure { get; set; }

        /// <summary>
        /// 轮次3压头3压装高度 (18.0)
        /// </summary>
        public float Cycle3Head3PressHeight { get; set; }

        /// <summary>
        /// 轮次3压头3最终压力 (22.0)
        /// </summary>
        public float Cycle3Head3FinalPressure { get; set; }

        /// <summary>
        /// 主条码（进站时扫描的条码）
        /// </summary>
        public string Product_BarCode { get; set; } = string.Empty;
        public bool IsNG {  get; set; }
    }
}
