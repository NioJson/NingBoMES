using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    public class P70UploadData : PlcUploadData
    {

        public bool WorkpieceOnlineRequest { get; set; }     // 0.1 工件上线申请
        public bool ReceivedOnlineInstruction { get; set; }  // 0.2 收到上线指令
        public bool StationProcessFinished { get; set; }     // 0.3 工站流程结束
        public bool ReceivedFinishInstruction { get; set; }  // 0.4 收到结束指令
        public bool ManualNGOffline { get; set; }            // 0.5 产品手动NG下线
        public bool ReceivedOfflineInstruction { get; set; } // 0.6 收到下线指令

        public bool CalibrationPowerStarted { get; set; }            // 0.7 标定电源开始工作（Bool）
        public bool CalibrationStationWorkingStarted { get; set; }   // 1.0 标定工站开始工作（Bool）
        public short CalibrationStationStatus { get; set; }          // 2.0 标定工站状态（Int16）
        public short CalibrationAlarmWord { get; set; }              // 4.0 标定报警字（Int16）
        public string CalibrationBarcode { get; set; } = "";         // 6.0 标定工件条码信息（String[50]）
        public bool CalibrationStartRequest { get; set; }            // 58.0 申请启动（Bool）
        public short CalibrationPosition { get; set; }               // 60.0 标定位置（Int16）
        public float TotalStrokeLength { get; set; }                 // 62.0 总行程长度（Real）
        public float CalibrationVoltage { get; set; }                // 66.0 标定站电压（Real）
        public float CalibrationCurrent { get; set; }                // 70.0 标定站电流（Real）

        // 老化2（Aging #2）
        public bool AgingOnlineRequest2 { get; set; }                // 74.0
        public bool AgingReceivedOnlineCommand2 { get; set; }        // 74.1
        public bool AgingProcessEnded2 { get; set; }                 // 74.2
        public bool AgingReceivedEndCommand2 { get; set; }           // 74.3
        public bool AgingManualNgOffline2 { get; set; }              // 74.4
        public bool AgingReceivedOfflineCommand2 { get; set; }       // 74.5
        public bool AgingPowerStarted2 { get; set; }                 // 74.6
        public bool AgingStationWorkingStarted2 { get; set; }        // 74.7
        public short AgingRunCount2 { get; set; }                    // 76.0（Int16）
        public float AgingRunTime2 { get; set; }                     // 78.0（Real）
        public float AgingVoltage2 { get; set; }                     // 82.0（Real）
        public float AgingCurrent2 { get; set; }                     // 86.0（Real）
        public string AgingBarcode2 { get; set; } = "";              // 90.0（String[50]）

        // 老化3（Aging #3）
        public bool AgingOnlineRequest3 { get; set; }                // 142.0
        public bool AgingReceivedOnlineCommand3 { get; set; }        // 142.1
        public bool AgingProcessEnded3 { get; set; }                 // 142.2
        public bool AgingReceivedEndCommand3 { get; set; }           // 142.3
        public bool AgingManualNgOffline3 { get; set; }              // 142.4
        public bool AgingReceivedOfflineCommand3 { get; set; }       // 142.5
        public bool AgingPowerStarted3 { get; set; }                 // 142.6
        public bool AgingStationWorkingStarted3 { get; set; }        // 142.7
        public short AgingRunCount3 { get; set; }                    // 144.0（Int16）
        public float AgingRunTime3 { get; set; }                     // 146.0（Real）
        public float AgingVoltage3 { get; set; }                     // 150.0（Real）
        public float AgingCurrent3 { get; set; }                     // 154.0（Real）
        public string AgingBarcode3 { get; set; } = "";              // 158.0（String[50]）

        // 老化4（Aging #4）
        public bool AgingOnlineRequest4 { get; set; }                // 210.0
        public bool AgingReceivedOnlineCommand4 { get; set; }        // 210.1
        public bool AgingProcessEnded4 { get; set; }                 // 210.2
        public bool AgingReceivedEndCommand4 { get; set; }           // 210.3
        public bool AgingManualNgOffline4 { get; set; }              // 210.4
        public bool AgingReceivedOfflineCommand4 { get; set; }       // 210.5
        public bool AgingPowerStarted4 { get; set; }                 // 210.6
        public bool AgingStationWorkingStarted4 { get; set; }        // 210.7
        public short AgingRunCount4 { get; set; }                    // 212.0（Int16）
        public float AgingRunTime4 { get; set; }                     // 214.0（Real）
        public float AgingVoltage4 { get; set; }                     // 218.0（Real）
        public float AgingCurrent4 { get; set; }                     // 222.0（Real）
        public string AgingBarcode4 { get; set; } = "";              // 226.0（String[50]）

        public bool LoadTestNotification { get; set; }                // 294.0
        // ========== 信号状态字 - 使用位操作压缩存储 ==========

        /// <summary>
        /// 信号状态字 - 16位无符号整数
        /// 每个位对应一个Bool信号
        /// </summary>
        public ushort SignalStatus { get; set; }

        // ========== Bool信号属性 - 通过位操作访问 ==========

        /// <summary>
        /// 标定工件上线申请 (地址: 0.1)
        /// 人工手持扫码之后向上位机发送上线申请
        /// 接收到上位机合格或NG指令后关闭
        /// </summary>
        public bool CalibrationWorkpieceOnlineRequest
        {
            get => GetBit(SignalStatus, 1);
            set => SignalStatus = SetBit(SignalStatus, 1, value);
        }

        /// <summary>
        /// 标定收到上线指令 (地址: 0.2)
        /// 接收到上位机合格或NG指令后触发
        /// 上位机接收到此指令后关闭合格或NG指令
        /// </summary>
        public bool CalibrationReceivedOnlineCommand
        {
            get => GetBit(SignalStatus, 2);
            set => SignalStatus = SetBit(SignalStatus, 2, value);
        }

        /// <summary>
        /// 标定工站流程结束 (地址: 0.3)
        /// PLC当前工站动作完成之后向上位机发送结束指令
        /// 收到上位机允许指令后关闭
        /// </summary>
        public bool CalibrationWorkstationProcessEnd
        {
            get => GetBit(SignalStatus, 3);
            set => SignalStatus = SetBit(SignalStatus, 3, value);
        }

        /// <summary>
        /// 标定收到结束指令 (地址: 0.4)
        /// 接收到上位机允许指令后触发
        /// 上位机接收到此指令后关闭收到下线指令
        /// </summary>
        public bool CalibrationReceivedEndCommand
        {
            get => GetBit(SignalStatus, 4);
            set => SignalStatus = SetBit(SignalStatus, 4, value);
        }

        /// <summary>
        /// 标定产品NG下线 (地址: 0.5)
        /// 工件因意外中途下线，由人工在触摸屏点击按钮向上位机发送此指令
        /// 接收到上位机收到指令后关闭
        /// </summary>
        public bool CalibrationProductNGDownline
        {
            get => GetBit(SignalStatus, 5);
            set => SignalStatus = SetBit(SignalStatus, 5, value);
        }

        /// <summary>
        /// 标定收到下线指令 (地址: 0.6)
        /// 接收到上位机允许指令后触发
        /// 上位机接收到此指令后关闭收到下线指令
        /// </summary>
        public bool CalibrationReceivedDownlineCommand
        {
            get => GetBit(SignalStatus, 6);
            set => SignalStatus = SetBit(SignalStatus, 6, value);
        }

        // ========== 辅助方法 ==========

        /// <summary>
        /// 获取指定位的值
        /// </summary>
        /// <param name="value">原始值</param>
        /// <param name="bitPosition">位位置 (0-15)</param>
        /// <returns>位的布尔值</returns>
        private static bool GetBit(ushort value, int bitPosition)
        {
            if (bitPosition < 0 || bitPosition > 15)
                throw new ArgumentOutOfRangeException(nameof(bitPosition), "位位置必须在0-15之间");

            return (value & (1 << bitPosition)) != 0;
        }

        /// <summary>
        /// 设置指定位的值
        /// </summary>
        /// <param name="value">原始值</param>
        /// <param name="bitPosition">位位置 (0-15)</param>
        /// <param name="bitValue">要设置的布尔值</param>
        /// <returns>设置后的值</returns>
        private static ushort SetBit(ushort value, int bitPosition, bool bitValue)
        {
            if (bitPosition < 0 || bitPosition > 15)
                throw new ArgumentOutOfRangeException(nameof(bitPosition), "位位置必须在0-15之间");

            if (bitValue)
                return (ushort)(value | (1 << bitPosition));
            else
                return (ushort)(value & ~(1 << bitPosition));
        }

        /// <summary>
        /// 重置所有Bool信号
        /// </summary>
        public void ResetAllSignals()
        {
            SignalStatus = 0;
        }

        /// <summary>
        /// 获取信号状态的详细字符串描述
        /// </summary>
        /// <returns>信号状态字符串</returns>
        public string GetSignalStatusDescription()
        {
            return $"上线申请:{CalibrationWorkpieceOnlineRequest} | " +
                   $"上线指令:{CalibrationReceivedOnlineCommand} | " +
                   $"流程结束:{CalibrationWorkstationProcessEnd} | " +
                   $"结束指令:{CalibrationReceivedEndCommand} | " +
                   $"NG下线:{CalibrationProductNGDownline} | " +
                   $"下线指令:{CalibrationReceivedDownlineCommand}";
        }

        /// <summary>
        /// 检查是否处于上线申请状态
        /// </summary>
        /// <returns>是否正在申请上线</returns>
        public bool IsOnlineRequesting()
        {
            return CalibrationWorkpieceOnlineRequest && !CalibrationReceivedOnlineCommand;
        }

        /// <summary>
        /// 检查是否处于流程结束状态
        /// </summary>
        /// <returns>是否流程结束等待确认</returns>
        public bool IsProcessEnding()
        {
            return CalibrationWorkstationProcessEnd && !CalibrationReceivedEndCommand;
        }

        /// <summary>
        /// 检查是否处于NG下线状态
        /// </summary>
        /// <returns>是否NG下线等待确认</returns>
        public bool IsNGDownlining()
        {
            return CalibrationProductNGDownline && !CalibrationReceivedDownlineCommand;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{");

            // 基类属性
            sb.Append($"\"Heartbeat(0.0 心跳)\":{Heartbeat.ToString().ToLower()},");

            // 标定（Calibration）
            sb.Append($"\"CalibrationPowerStarted(0.7 标定电源开始工作)\":{CalibrationPowerStarted.ToString().ToLower()},");
            sb.Append($"\"CalibrationStationWorkingStarted(1.0 标定工站开始工作)\":{CalibrationStationWorkingStarted.ToString().ToLower()},");
            sb.Append($"\"CalibrationStationStatus(2.0 标定工站状态)\":{CalibrationStationStatus},");
            sb.Append($"\"CalibrationAlarmWord(4.0 标定报警字)\":{CalibrationAlarmWord},");
            sb.Append($"\"CalibrationBarcode(6.0 标定工件条码信息)\":\"{CalibrationBarcode}\",");
            sb.Append($"\"CalibrationStartRequest(58.0 申请启动)\":{CalibrationStartRequest.ToString().ToLower()},");
            sb.Append($"\"CalibrationPosition(60.0 标定位置)\":{CalibrationPosition},");
            sb.Append($"\"TotalStrokeLength(62.0 总行程长度)\":{TotalStrokeLength},");
            sb.Append($"\"CalibrationVoltage(66.0 标定站电压)\":{CalibrationVoltage},");
            sb.Append($"\"CalibrationCurrent(70.0 标定站电流)\":{CalibrationCurrent},");

            // 老化2（Aging #2）
            sb.Append($"\"AgingOnlineRequest2(74.0)\":{AgingOnlineRequest2.ToString().ToLower()},");
            sb.Append($"\"AgingReceivedOnlineCommand2(74.1)\":{AgingReceivedOnlineCommand2.ToString().ToLower()},");
            sb.Append($"\"AgingProcessEnded2(74.2)\":{AgingProcessEnded2.ToString().ToLower()},");
            sb.Append($"\"AgingReceivedEndCommand2(74.3)\":{AgingReceivedEndCommand2.ToString().ToLower()},");
            sb.Append($"\"AgingManualNgOffline2(74.4)\":{AgingManualNgOffline2.ToString().ToLower()},");
            sb.Append($"\"AgingReceivedOfflineCommand2(74.5)\":{AgingReceivedOfflineCommand2.ToString().ToLower()},");
            sb.Append($"\"AgingPowerStarted2(74.6)\":{AgingPowerStarted2.ToString().ToLower()},");
            sb.Append($"\"AgingStationWorkingStarted2(74.7)\":{AgingStationWorkingStarted2.ToString().ToLower()},");
            sb.Append($"\"AgingRunCount2(76.0)\":{AgingRunCount2},");
            sb.Append($"\"AgingRunTime2(78.0)\":{AgingRunTime2},");
            sb.Append($"\"AgingVoltage2(82.0)\":{AgingVoltage2},");
            sb.Append($"\"AgingCurrent2(86.0)\":{AgingCurrent2},");
            sb.Append($"\"AgingBarcode2(90.0)\":\"{AgingBarcode2}\",");

            // 老化3（Aging #3）
            sb.Append($"\"AgingOnlineRequest3(142.0)\":{AgingOnlineRequest3.ToString().ToLower()},");
            sb.Append($"\"AgingReceivedOnlineCommand3(142.1)\":{AgingReceivedOnlineCommand3.ToString().ToLower()},");
            sb.Append($"\"AgingProcessEnded3(142.2)\":{AgingProcessEnded3.ToString().ToLower()},");
            sb.Append($"\"AgingReceivedEndCommand3(142.3)\":{AgingReceivedEndCommand3.ToString().ToLower()},");
            sb.Append($"\"AgingManualNgOffline3(142.4)\":{AgingManualNgOffline3.ToString().ToLower()},");
            sb.Append($"\"AgingReceivedOfflineCommand3(142.5)\":{AgingReceivedOfflineCommand3.ToString().ToLower()},");
            sb.Append($"\"AgingPowerStarted3(142.6)\":{AgingPowerStarted3.ToString().ToLower()},");
            sb.Append($"\"AgingStationWorkingStarted3(142.7)\":{AgingStationWorkingStarted3.ToString().ToLower()},");
            sb.Append($"\"AgingRunCount3(144.0)\":{AgingRunCount3},");
            sb.Append($"\"AgingRunTime3(146.0)\":{AgingRunTime3},");
            sb.Append($"\"AgingVoltage3(150.0)\":{AgingVoltage3},");
            sb.Append($"\"AgingCurrent3(154.0)\":{AgingCurrent3},");
            sb.Append($"\"AgingBarcode3(158.0)\":\"{AgingBarcode3}\",");

            // 老化4（Aging #4）
            sb.Append($"\"AgingOnlineRequest4(210.0)\":{AgingOnlineRequest4.ToString().ToLower()},");
            sb.Append($"\"AgingReceivedOnlineCommand4(210.1)\":{AgingReceivedOnlineCommand4.ToString().ToLower()},");
            sb.Append($"\"AgingProcessEnded4(210.2)\":{AgingProcessEnded4.ToString().ToLower()},");
            sb.Append($"\"AgingReceivedEndCommand4(210.3)\":{AgingReceivedEndCommand4.ToString().ToLower()},");
            sb.Append($"\"AgingManualNgOffline4(210.4)\":{AgingManualNgOffline4.ToString().ToLower()},");
            sb.Append($"\"AgingReceivedOfflineCommand4(210.5)\":{AgingReceivedOfflineCommand4.ToString().ToLower()},");
            sb.Append($"\"AgingPowerStarted4(210.6)\":{AgingPowerStarted4.ToString().ToLower()},");
            sb.Append($"\"AgingStationWorkingStarted4(210.7)\":{AgingStationWorkingStarted4.ToString().ToLower()},");
            sb.Append($"\"AgingRunCount4(212.0)\":{AgingRunCount4},");
            sb.Append($"\"AgingRunTime4(214.0)\":{AgingRunTime4},");
            sb.Append($"\"AgingVoltage4(218.0)\":{AgingVoltage4},");
            sb.Append($"\"AgingCurrent4(222.0)\":{AgingCurrent4},");
            sb.Append($"\"AgingBarcode4(226.0)\":\"{AgingBarcode4}\",");

            // 信号状态字
            sb.Append($"\"SignalStatus(信号状态字)\":{SignalStatus},");

            // Bool信号属性
            sb.Append($"\"CalibrationWorkpieceOnlineRequest(标定工件上线申请 0.1)\":{CalibrationWorkpieceOnlineRequest.ToString().ToLower()},");
            sb.Append($"\"CalibrationReceivedOnlineCommand(标定收到上线指令 0.2)\":{CalibrationReceivedOnlineCommand.ToString().ToLower()},");
            sb.Append($"\"CalibrationWorkstationProcessEnd(标定工站流程结束 0.3)\":{CalibrationWorkstationProcessEnd.ToString().ToLower()},");
            sb.Append($"\"CalibrationReceivedEndCommand(标定收到结束指令 0.4)\":{CalibrationReceivedEndCommand.ToString().ToLower()},");
            sb.Append($"\"CalibrationProductNGDownline(标定产品NG下线 0.5)\":{CalibrationProductNGDownline.ToString().ToLower()},");
            sb.Append($"\"CalibrationReceivedDownlineCommand(标定收到下线指令 0.6)\":{CalibrationReceivedDownlineCommand.ToString().ToLower()}");

            sb.Append("}");
            return sb.ToString();
        }
    }
}
