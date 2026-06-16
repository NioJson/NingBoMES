using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    public class P40UploadData : PlcUploadData
    {

        // 布尔信号 (0.0~1.0)
        public bool WorkpieceOnlineRequest { get; set; }     // 0.1 工件上线申请
        public bool ReceivedOnlineInstruction { get; set; }  // 0.2 收到上线指令
        public bool StationProcessFinished { get; set; }     // 0.3 工站流程结束
        public bool ReceivedFinishInstruction { get; set; }  // 0.4 收到结束指令
        public bool ManualNGOffline { get; set; }            // 0.5 产品手动NG下线
        public bool ReceivedOfflineInstruction { get; set; } // 0.6 收到下线指令
        public bool PressStarted { get; set; }               // 0.7 压机开始工作

        public bool StationWorkingStarted { get; set; }      // 1.0 工站开始工作

        // 整型数据 (2.0~4.0)
        public short StationStatus { get; set; }             // 2.0 工站状态
        public short AlarmWord { get; set; }                 // 4.0 报警字

        // 字符串数据 (6.0)
        public string Barcode { get; set; } = "";            // 6.0 工件条码信息 String[50]

        // 整型数据 (58.0~60.0)
        public int TighteningProgramNumber { get; set; }      //  拧紧枪程序号 58.0
        public int ScrewTighteningCount { get; set; }         // 拧螺丝次数 60.0
        public float RealtimeTorque { get; set; }             //拧紧枪实时扭矩 62.0
        public float CompletedTorque { get; set; }            //拧紧枪完成扭矩 66.0
        public int RealtimeAngle { get; set; }                //拧紧枪实时角度 70.0
        public int CompletedAngle { get; set; }               //拧紧枪完成角度 72.0

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
            sb.Append($"\"PressStarted(0.7 压机开始工作)\":{PressStarted.ToString().ToLower()},");
            sb.Append($"\"StationWorkingStarted(1.0 工站开始工作)\":{StationWorkingStarted.ToString().ToLower()},");

            // 整型数据 (2.0~4.0)
            sb.Append($"\"StationStatus(2.0 工站状态)\":{StationStatus},");
            sb.Append($"\"AlarmWord(4.0 报警字)\":{AlarmWord},");

            // 字符串数据 (6.0)
            sb.Append($"\"Barcode(6.0 工件条码信息 String[50])\":\"{Barcode}\",");

            // 整型数据 (58.0~60.0)
            sb.Append($"\"TighteningProgramNumber(拧紧枪程序号 58.0)\":{TighteningProgramNumber},");
            sb.Append($"\"ScrewTighteningCount(拧螺丝次数 60.0)\":{ScrewTighteningCount},");

            // 浮点型数据 (62.0~74.0)
            sb.Append($"\"RealtimeTorque(拧紧枪实时扭矩 62.0)\":{RealtimeTorque},");
            sb.Append($"\"CompletedTorque(拧紧枪完成扭矩 66.0)\":{CompletedTorque},");
            sb.Append($"\"RealtimeAngle(拧紧枪实时角度 70.0)\":{RealtimeAngle},");
            sb.Append($"\"CompletedAngle(拧紧枪完成角度 72.0)\":{CompletedAngle}");

            sb.Append("}");
            return sb.ToString();
        }
    }
}
