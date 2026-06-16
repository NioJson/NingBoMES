using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    public class P70DB3003ReturnData
    {
        /// <summary>
        /// 读取标志位1 (0.0)
        /// </summary>
        public bool ReadFlag1 { get; set; }

        /// <summary>
        /// 标定结果1 (0.1)
        /// </summary>
        public bool CalibrationResult1 { get; set; }

        /// <summary>
        /// 最大长度1 (2.0)
        /// </summary>
        public float MaxLength1 { get; set; }

        /// <summary>
        /// AMPLITUDE值1 (6.0)
        /// </summary>
        public float AmplitudeValue1 { get; set; }

        /// <summary>
        /// MAG_LOW值1 (10.0)
        /// </summary>
        public float MagLowValue1 { get; set; }

        /// <summary>
        /// 读取标志位2 (14.0)
        /// </summary>
        public bool ReadFlag2 { get; set; }

        /// <summary>
        /// 右老化最大电压2 (16.0)
        /// </summary>
        public float RightAgingMaxVoltage2 { get; set; }

        /// <summary>
        /// 右老化最小电压2 (20.0)
        /// </summary>
        public float RightAgingMinVoltage2 { get; set; }

        /// <summary>
        /// 右老化平均电压2 (24.0)
        /// </summary>
        public float RightAgingAvgVoltage2 { get; set; }

        /// <summary>
        /// 右老化最大电流2 (28.0)
        /// </summary>
        public float RightAgingMaxCurrent2 { get; set; }

        /// <summary>
        /// 右老化最小电流2 (32.0)
        /// </summary>
        public float RightAgingMinCurrent2 { get; set; }

        /// <summary>
        /// 右老化平均电流2 (36.0)
        /// </summary>
        public float RightAgingAvgCurrent2 { get; set; }

        /// <summary>
        /// 读取标志位3 (40.0)
        /// </summary>
        public bool ReadFlag3 { get; set; }

        /// <summary>
        /// 中老化最大电压3 (42.0)
        /// </summary>
        public float MiddleAgingMaxVoltage3 { get; set; }

        /// <summary>
        /// 中老化最小电压3 (46.0)
        /// </summary>
        public float MiddleAgingMinVoltage3 { get; set; }

        /// <summary>
        /// 中老化平均电压3 (50.0)
        /// </summary>
        public float MiddleAgingAvgVoltage3 { get; set; }

        /// <summary>
        /// 中老化最大电流3 (54.0)
        /// </summary>
        public float MiddleAgingMaxCurrent3 { get; set; }

        /// <summary>
        /// 中老化最小电流3 (58.0)
        /// </summary>
        public float MiddleAgingMinCurrent3 { get; set; }

        /// <summary>
        /// 中老化平均电流3 (62.0)
        /// </summary>
        public float MiddleAgingAvgCurrent3 { get; set; }

        /// <summary>
        /// 读取标志位4 (66.0)
        /// </summary>
        public bool ReadFlag4 { get; set; }

        /// <summary>
        /// 左老化最大电压4 (68.0)
        /// </summary>
        public float LeftAgingMaxVoltage4 { get; set; }

        /// <summary>
        /// 左老化最小电压4 (72.0)
        /// </summary>
        public float LeftAgingMinVoltage4 { get; set; }

        /// <summary>
        /// 左老化平均电压4 (76.0)
        /// </summary>
        public float LeftAgingAvgVoltage4 { get; set; }

        /// <summary>
        /// 左老化最大电流4 (80.0)
        /// </summary>
        public float LeftAgingMaxCurrent4 { get; set; }

        /// <summary>
        /// 左老化最小电流4 (84.0)
        /// </summary>
        public float LeftAgingMinCurrent4 { get; set; }

        /// <summary>
        /// 左老化平均电流4 (88.0)
        /// </summary>
        public float LeftAgingAvgCurrent4 { get; set; }

        /// <summary>
        /// 主条码（进站时扫描的条码）
        /// </summary>
        public string Product_BarCode { get; set; } = string.Empty;
        public bool IsNG {  get; set; }
    }
}
