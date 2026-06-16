using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    /// <summary>
    /// P10工站DB2000返回数据模型（上位机→PLC）
    /// </summary>
    public class P10DB2000ReturnData
    {
        /// <summary>
        /// 读取标志位 (0.0)
        /// </summary>
        public bool ReadFlag { get; set; }

        /// <summary>
        /// 轮次1压头1压装高度1 (2.0)
        /// </summary>
        public float Cycle1Head1PressHeight1 { get; set; }

        /// <summary>
        /// 轮次1压头1最终压力1 (6.0)
        /// </summary>
        public float Cycle1Head1FinalPressure1 { get; set; }

        /// <summary>
        /// 轮次1压头1压装高度2 (10.0)
        /// </summary>
        public float Cycle1Head1PressHeight2 { get; set; }

        /// <summary>
        /// 轮次1压头1最终压力2 (14.0)
        /// </summary>
        public float Cycle1Head1FinalPressure2 { get; set; }

        /// <summary>
        /// 轮次1压头2压装高度 (18.0)
        /// </summary>
        public float Cycle1Head2PressHeight { get; set; }

        /// <summary>
        /// 轮次1压头2最终压力 (22.0)
        /// </summary>
        public float Cycle1Head2FinalPressure { get; set; }

        /// <summary>
        /// 轮次1压头3压装高度 (26.0)
        /// </summary>
        public float Cycle1Head3PressHeight { get; set; }

        /// <summary>
        /// 轮次1压头3最终压力 (30.0)
        /// </summary>
        public float Cycle1Head3FinalPressure { get; set; }

        /// <summary>
        /// 轮次1压头4压装高度 (34.0)
        /// </summary>
        public float Cycle1Head4PressHeight { get; set; }

        /// <summary>
        /// 轮次1压头4最终压力 (38.0)
        /// </summary>
        public float Cycle1Head4FinalPressure { get; set; }

        /// <summary>
        /// 轮次2压头2压装高度 (42.0)
        /// </summary>
        public float Cycle2Head2PressHeight { get; set; }

        /// <summary>
        /// 轮次2压头2最终压力 (46.0)
        /// </summary>
        public float Cycle2Head2FinalPressure { get; set; }

        /// <summary>
        /// 轮次2压头3压装高度 (50.0)
        /// </summary>
        public float Cycle2Head3PressHeight { get; set; }

        /// <summary>
        /// 轮次2压头3最终压力 (54.0)
        /// </summary>
        public float Cycle2Head3FinalPressure { get; set; }

        /// <summary>
        /// 轮次2压头4压装高度 (58.0)
        /// </summary>
        public float Cycle2Head4PressHeight { get; set; }

        /// <summary>
        /// 轮次2压头4最终压力 (62.0)
        /// </summary>
        public float Cycle2Head4FinalPressure { get; set; }

        /// <summary>
        /// 轮次3压头2压装高度 (66.0)
        /// </summary>
        public float Cycle3Head2PressHeight { get; set; }

        /// <summary>
        /// 轮次3压头2最终压力 (70.0)
        /// </summary>
        public float Cycle3Head2FinalPressure { get; set; }

        /// <summary>
        /// 主条码（进站时扫描的条码）
        /// </summary>
        public string Product_BarCode { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public bool IsNG { get; set; }
    }
}
