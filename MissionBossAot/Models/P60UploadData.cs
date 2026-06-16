using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    public class P60UploadData : PlcUploadData
    {

        public bool WorkpieceOnlineRequest { get; set; }     // 0.1 工件上线申请
        public bool ReceivedOnlineInstruction { get; set; }  // 0.2 收到上线指令
        public bool StationProcessFinished { get; set; }     // 0.3 工站流程结束
        public bool ReceivedFinishInstruction { get; set; }  // 0.4 收到结束指令
        public bool ManualNGOffline { get; set; }            // 0.5 产品NG下线
        public bool ReceivedOfflineInstruction { get; set; } // 0.6 收到下线指令
        public bool LeakTesterStarted { get; set; }          // 0.7 气密仪开始工作
        public bool StationWorkingStarted { get; set; }      // 1.0 工站开始工作

        // 整型数据 (2.0~4.0)
        public short StationStatus { get; set; }              // 2.0 工站状态 (注意：您提供的表格中为Bool类型)
        public short AlarmWord { get; set; }                 // 4.0 报警字

        // 整型数据 (6.0)
        public string BarcodeInfo { get; set; }               // 6.0 工件条码信息 (注意：您提供的表格中为Int类型)

        // 字符串数据 (58.0~154.0)
        public string DateTimeInfo { get; set; } = "";       // 58.0 时间日期 String[50]
        public string Temperature { get; set; } = "";        // 76.0 温度 String[16]
        public string RunCount { get; set; } = "";           // 84.0 运行数 String[5]
        public string ProgramNumber { get; set; } = "";      // 90.0 程序编号 String[4]
        public string TestPressure { get; set; } = "";       // 96.0 测试压力 String[3]
        public string TestPressureUnit { get; set; } = "";   // 106.0 测试压力单位 String[8]
        public string PressureDrop { get; set; } = "";       // 112.0 压降 String[4]
        public string PressureDropUnit { get; set; } = "";   // 122.0 压降单位 String[8]
        public string LeakRate { get; set; } = "";           // 128.0 泄露率 String[4]
        public string LeakRateUnit { get; set; } = "";       // 138.0 泄露率单位 String[8]
        public string MeaningCode { get; set; } = "";        // 148.0 含义代码 String[7]

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
            sb.Append($"\"ManualNGOffline(0.5 产品NG下线)\":{ManualNGOffline.ToString().ToLower()},");
            sb.Append($"\"ReceivedOfflineInstruction(0.6 收到下线指令)\":{ReceivedOfflineInstruction.ToString().ToLower()},");
            sb.Append($"\"LeakTesterStarted(0.7 气密仪开始工作)\":{LeakTesterStarted.ToString().ToLower()},");
            sb.Append($"\"StationWorkingStarted(1.0 工站开始工作)\":{StationWorkingStarted.ToString().ToLower()},");

            // 整型数据 (2.0~4.0)
            sb.Append($"\"StationStatus(2.0 工站状态)\":{StationStatus},");
            sb.Append($"\"AlarmWord(4.0 报警字)\":{AlarmWord},");

            // 整型数据 (6.0)
            sb.Append($"\"BarcodeInfo(6.0 工件条码信息)\":{BarcodeInfo},");

            // 字符串数据 (58.0~154.0)
            sb.Append($"\"DateTimeInfo(58.0 时间日期 String[50])\":\"{DateTimeInfo}\",");
            sb.Append($"\"Temperature(76.0 温度 String[16])\":\"{Temperature}\",");
            sb.Append($"\"RunCount(84.0 运行数 String[5])\":\"{RunCount}\",");
            sb.Append($"\"ProgramNumber(90.0 程序编号 String[4])\":\"{ProgramNumber}\",");
            sb.Append($"\"TestPressure(96.0 测试压力 String[3])\":\"{TestPressure}\",");
            sb.Append($"\"TestPressureUnit(106.0 测试压力单位 String[8])\":\"{TestPressureUnit}\",");
            sb.Append($"\"PressureDrop(112.0 压降 String[4])\":\"{PressureDrop}\",");
            sb.Append($"\"PressureDropUnit(122.0 压降单位 String[8])\":\"{PressureDropUnit}\",");
            sb.Append($"\"LeakRate(128.0 泄露率 String[4])\":\"{LeakRate}\",");
            sb.Append($"\"LeakRateUnit(138.0 泄露率单位 String[8])\":\"{LeakRateUnit}\",");
            sb.Append($"\"MeaningCode(148.0 含义代码 String[7])\":\"{MeaningCode}\"");

            sb.Append("}");
            return sb.ToString();
        }
    }
}
