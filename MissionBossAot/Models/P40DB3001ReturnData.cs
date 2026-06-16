using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    public class P40DB3001ReturnData
    {
        /// <summary>
        /// 读取标志位 (0.0)
        /// </summary>
        public bool ReadFlag { get; set; }

        /// <summary>
        /// 完成扭矩1 (2.0)
        /// </summary>
        public float CompletedTorque1 { get; set; }

        /// <summary>
        /// 完成角度1 (6.0)
        /// </summary>
        public int CompletedAngle1 { get; set; }

        /// <summary>
        /// 完成扭矩2 (10.0)
        /// </summary>
        public float CompletedTorque2 { get; set; }

        /// <summary>
        /// 完成角度2 (14.0)
        /// </summary>
        public int CompletedAngle2 { get; set; }

        /// <summary>
        /// 完成扭矩3 (18.0)
        /// </summary>
        public float CompletedTorque3 { get; set; }

        /// <summary>
        /// 完成角度3 (22.0)
        /// </summary>
        public int CompletedAngle3 { get; set; }

        /// <summary>
        /// 完成扭矩4 (26.0)
        /// </summary>
        public float CompletedTorque4 { get; set; }

        /// <summary>
        /// 完成角度4 (30.0)
        /// </summary>
        public int CompletedAngle4 { get; set; }

        /// <summary>
        /// 完成扭矩5 (34.0)
        /// </summary>
        public float CompletedTorque5 { get; set; }

        /// <summary>
        /// 完成角度5 (38.0)
        /// </summary>
        public int CompletedAngle5 { get; set; }

        /// <summary>
        /// 完成扭矩6 (42.0)
        /// </summary>
        public float CompletedTorque6 { get; set; }

        /// <summary>
        /// 完成角度6 (46.0)
        /// </summary>
        public int CompletedAngle6 { get; set; }

        /// <summary>
        /// 完成扭矩7 (50.0)
        /// </summary>
        public float CompletedTorque7 { get; set; }

        /// <summary>
        /// 完成角度7 (54.0)
        /// </summary>
        public int CompletedAngle7 { get; set; }

        /// <summary>
        /// 完成扭矩8 (58.0)
        /// </summary>
        public float CompletedTorque8 { get; set; }

        /// <summary>
        /// 完成角度8 (62.0)
        /// </summary>
        public int CompletedAngle8 { get; set; }

        /// <summary>
        /// 完成扭矩9 (66.0)
        /// </summary>
        public float CompletedTorque9 { get; set; }

        /// <summary>
        /// 完成角度9 (70.0)
        /// </summary>
        public int CompletedAngle9 { get; set; }


        /// <summary>
        /// 齿轮高度1 (178.0)
        /// </summary>
        public float GearHeight1 { get; set; }

        /// <summary>
        /// 齿轮高度2 (182.0)
        /// </summary>
        public float GearHeight2 { get; set; }



        /// <summary>
        /// 主条码（进站时扫描的条码）
        /// </summary>
        public string Product_BarCode { get; set; } = string.Empty;
        public string MotorCode { get; set; }
        public bool IsNG {  get; set; }
    }
}
