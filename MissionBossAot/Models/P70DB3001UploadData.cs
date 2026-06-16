using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    public class P70DB3001UploadData
    {
        /// <summary>
        /// MES可读 (0.0)
        /// </summary>
        public bool MESCanRead { get; set; }

        /// <summary>
        /// A SLOT (148.0)
        /// </summary>
        public int ASLOT { get; set; }

        /// <summary>
        /// B SLOT (146.0)
        /// </summary>
        public int BSLOT { get; set; }

        /// <summary>
        /// C SLOT (144.0)
        /// </summary>
        public int CSLOT { get; set; }

        /// <summary>
        /// AMPLITUDE值A (172.0)
        /// </summary>
        public int AmplitudeValueA { get; set; }

        /// <summary>
        /// AMPLITUDE值B (176.0)
        /// </summary>
        public int AmplitudeValueB { get; set; }

        /// <summary>
        /// AMPLITUDE值C (168.0)
        /// </summary>
        public int AmplitudeValueC { get; set; }

        /// <summary>
        /// MAG_LOW值A (10.0)
        /// </summary>
        public int MagLowValueA { get; set; }

        /// <summary>
        /// MAG_LOW值B (164.0)
        /// </summary>
        public float MagLowValueB { get; set; }

        /// <summary>
        /// MAG_LOW值C (6.0)
        /// </summary>
        public float MagLowValueC { get; set; }

        /// <summary>
        /// A倒数第一个点 (150.0)
        /// </summary>
        public float APenultimatePoint { get; set; }

        /// <summary>
        /// A倒数第二个点 (152.0)
        /// </summary>
        public float AThirdLastPoint { get; set; }

        /// <summary>
        /// B倒数第一个点 (154.0)
        /// </summary>
        public float BPenultimatePoint { get; set; }

        /// <summary>
        /// B倒数第二个点 (156.0)
        /// </summary>
        public float BThirdLastPoint { get; set; }

        /// <summary>
        /// C倒数第一个点 (158.0)
        /// </summary>
        public float CPenultimatePoint { get; set; }

        /// <summary>
        /// C倒数第二个点 (160.0)
        /// </summary>
        public float CThirdLastPoint { get; set; }

        /// <summary>
        /// A第一个点 (184.0)
        /// </summary>
        public float AFirstPoint { get; set; }

        /// <summary>
        /// A第二个点 (188.0)
        /// </summary>
        public float ASecondPoint { get; set; }

        /// <summary>
        /// B第一个点 (192.0)
        /// </summary>
        public float BFirstPoint { get; set; }

        /// <summary>
        /// B第二个点 (196.0)
        /// </summary>
        public float BSecondPoint { get; set; }

        /// <summary>
        /// C第一个点 (200.0)
        /// </summary>
        public float CFirstPoint { get; set; }

        /// <summary>
        /// C第二个点 (204.0)
        /// </summary>
        public float CSecondPoint { get; set; }

        /// <summary>
        /// 感应距离 (180.0)
        /// </summary>
        public float SensingDistance { get; set; }



        /// <summary>
        /// C第二个点 (204.0)
        /// </summary>
        public float OutermostDistance { get; set; }

        /// <summary>
        /// 感应距离 (180.0)
        /// </summary>
        public float InnermostDistance { get; set; }



        /// <summary>
        /// 实际距离 (2.0)
        /// </summary>
        public float ActualDistance { get; set; }
        public string ProductBarCode { get; set; }
        public bool IsNG {  get; set; }
    }



    /// <summary>
    /// P70工站DB3001上传数据模型（PLC→上位机）
    /// 字段与signal_data2数据表完全对应
    /// IP: 192.168.0.40
    /// DB: 3001
    /// </summary>
    public class P70DB3001UploadData2
    {
        /// <summary>
        /// MES可读 (0.0)
        /// </summary>
        public bool MESCanRead { get; set; }


        /// <summary>
        /// 500N力电流（单位：A）
        /// 对应表字段：force_current_500n
        /// </summary>
        public float ForceCurrent500n { get; set; }

        /// <summary>
        /// 600N力电流（单位：A）
        /// 对应表字段：force_current_600n
        /// </summary>
        public float ForceCurrent600n { get; set; }

        /// <summary>
        /// 最大电流（单位：A）
        /// 对应表字段：max_current
        /// </summary>
        public float MaxCurrent { get; set; }

        /// <summary>
        /// 最大电压（单位：V）
        /// 对应表字段：max_voltage
        /// </summary>
        public float MaxVoltage { get; set; }

        /// <summary>
        /// 最大扭力（单位：N·m）
        /// 对应表字段：max_torque
        /// </summary>
        public float MaxTorque { get; set; }

        /// <summary>
        /// 最大压力（单位：Pa）
        /// 对应表字段：max_pressure
        /// </summary>
        public float MaxPressure { get; set; }

        /// <summary>
        /// 预留字段1
        /// 对应表字段：reserve1
        /// </summary>
        public float Reserve1 { get; set; }

        /// <summary>
        /// 预留字段2
        /// 对应表字段：reserve2
        /// </summary>
        public float Reserve2 { get; set; }

        /// <summary>
        /// 预留字段3
        /// 对应表字段：reserve3
        /// </summary>
        public float Reserve3 { get; set; }

        /// <summary>
        /// 预留字段4
        /// 对应表字段：reserve4
        /// </summary>
        public float Reserve4 { get; set; }

        /// <summary>
        /// 数据创建时间（数据库自动赋值，PLC侧无需赋值）
        /// 对应表字段：create_time
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 产品条码
        /// 对应表字段：ProductBarCode
        /// </summary>
        public string ProductBarCode { get; set; }
        public bool IsNG { get; set; }
    }
}
