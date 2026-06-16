using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    public class P80DB3004ReturnData
    {
        /// <summary>
        /// 读取标志位 (0.0)
        /// </summary>
        public bool ReadFlag { get; set; }

        /// <summary>
        /// 31HZ声压值 (2.0)
        /// </summary>
        public float SoundPressure31Hz { get; set; }

        /// <summary>
        /// 63HZ声压值 (6.0)
        /// </summary>
        public float SoundPressure63Hz { get; set; }

        /// <summary>
        /// 125HZ声压值 (10.0)
        /// </summary>
        public float SoundPressure125Hz { get; set; }

        /// <summary>
        /// 250HZ声压值 (14.0)
        /// </summary>
        public float SoundPressure250Hz { get; set; }

        /// <summary>
        /// 500HZ声压值 (18.0)
        /// </summary>
        public float SoundPressure500Hz { get; set; }

        /// <summary>
        /// 1000HZ声压值 (22.0)
        /// </summary>
        public float SoundPressure1000Hz { get; set; }

        /// <summary>
        /// 2000HZ声压值 (26.0)
        /// </summary>
        public float SoundPressure2000Hz { get; set; }

        /// <summary>
        /// 4000HZ声压值 (30.0)
        /// </summary>
        public float SoundPressure4000Hz { get; set; }

        /// <summary>
        /// 8000HZ声压值 (34.0)
        /// </summary>
        public float SoundPressure8000Hz { get; set; }

        /// <summary>
        /// 16000HZ声压值 (38.0)
        /// </summary>
        public float SoundPressure16000Hz { get; set; }

        /// <summary>
        /// APC声压值 (42.0)
        /// </summary>
        public float SoundPressureAPC { get; set; }

        /// <summary>
        /// APA声压值 (46.0)
        /// </summary>
        public float SoundPressureAPA { get; set; }

        /// <summary>
        /// APLIN声压值 (50.0)
        /// </summary>
        public float SoundPressureAPLIN { get; set; }

        /// <summary>
        /// 主条码（进站时扫描的条码）
        /// </summary>
        public string Product_BarCode { get; set; } = string.Empty;
        public bool IsNG {  get; set; }
    }
}
