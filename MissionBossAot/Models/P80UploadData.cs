using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    public class P80UploadData : PlcUploadData
    {

        // 布尔信号 (0.0~1.0)
        public bool WorkpieceOnlineRequest { get; set; }     // 0.1 工件上线申请
        public bool ReceivedOnlineInstruction { get; set; }  // 0.2 收到上线指令
        public bool StationProcessFinished { get; set; }     // 0.3 工站流程结束
        public bool ReceivedFinishInstruction { get; set; }  // 0.4 收到结束指令
        public bool ManualNGOffline { get; set; }            // 0.5 产品手动NG下线
        public bool ReceivedOfflineInstruction { get; set; } // 0.6 收到下线指令
        public bool NoisePowerStarted { get; set; }          // 0.7 噪音电源开始工作（Bool）
        public bool StationWorkingStarted { get; set; }      // 1.0 工站开始工作（Bool）
        public short StationStatus { get; set; }             // 2.0 工站状态（Int16）
        public short AlarmWord { get; set; }                 // 4.0 报警字（Int16）
        public string Barcode { get; set; } = "";            // 6.0 工件条码信息（String[50]）
        public float Voltage { get; set; }                   // 58.0 电压数据（Real）
        public float Current { get; set; }                   // 62.0 电流数据（Real）

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{");

            // 基类属性
            sb.Append($"\"Heartbeat(0.0 心跳)\":{Heartbeat.ToString().ToLower()},");

            // 布尔信号 (0.1~1.0)
            sb.Append($"\"WorkpieceOnlineRequest(0.1 工件上线申请)\":{WorkpieceOnlineRequest.ToString().ToLower()},");
            sb.Append($"\"ReceivedOnlineInstruction(0.2 收到上线指令)\":{ReceivedOnlineInstruction.ToString().ToLower()},");
            sb.Append($"\"StationProcessFinished(0.3 工站流程结束)\":{StationProcessFinished.ToString().ToLower()},");
            sb.Append($"\"ReceivedFinishInstruction(0.4 收到结束指令)\":{ReceivedFinishInstruction.ToString().ToLower()},");
            sb.Append($"\"ManualNGOffline(0.5 产品手动NG下线)\":{ManualNGOffline.ToString().ToLower()},");
            sb.Append($"\"ReceivedOfflineInstruction(0.6 收到下线指令)\":{ReceivedOfflineInstruction.ToString().ToLower()},");
            sb.Append($"\"NoisePowerStarted(0.7 噪音电源开始工作)\":{NoisePowerStarted.ToString().ToLower()},");
            sb.Append($"\"StationWorkingStarted(1.0 工站开始工作)\":{StationWorkingStarted.ToString().ToLower()},");

            // 整型数据 (2.0~4.0)
            sb.Append($"\"StationStatus(2.0 工站状态)\":{StationStatus},");
            sb.Append($"\"AlarmWord(4.0 报警字)\":{AlarmWord},");

            // 字符串数据 (6.0)
            sb.Append($"\"Barcode(6.0 工件条码信息)\":\"{Barcode}\",");

            // 浮点型数据 (58.0~62.0)
            sb.Append($"\"Voltage(58.0 电压数据)\":{Voltage},");
            sb.Append($"\"Current(62.0 电流数据)\":{Current}");

            sb.Append("}");
            return sb.ToString();
        }
    }
}
