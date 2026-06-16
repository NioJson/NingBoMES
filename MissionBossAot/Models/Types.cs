using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    /// <summary>
    /// PLC存储区域枚举
    /// </summary>
    public enum Areas
    {
        /// <summary>
        /// 输入区域
        /// </summary>
        Input = 0x81,

        /// <summary>
        /// 输出区域
        /// </summary>
        Output = 0x82,

        /// <summary>
        /// 内存区域
        /// </summary>
        Memory = 0x83,

        /// <summary>
        /// 数据块区域
        /// </summary>
        DataBlock = 0x84
    }

    /// <summary>
    /// 参数值大小枚举（用于读取操作）
    /// </summary>
    public enum PValueSize
    {
        /// <summary>
        /// 位
        /// </summary>
        BIT = 0x01,

        /// <summary>
        /// 字节
        /// </summary>
        BYTE = 0x02,

        /// <summary>
        /// 字符
        /// </summary>
        CHAR = 0x03,

        /// <summary>
        /// 字
        /// </summary>
        WORD = 0x04,

        /// <summary>
        /// 整数
        /// </summary>
        INTERGER = 0x05,

        /// <summary>
        /// 双字
        /// </summary>
        DWORD = 0x06,

        /// <summary>
        /// 双整数
        /// </summary>
        DINT = 0x07,

        /// <summary>
        /// 实数
        /// </summary>
        REAL = 0x08,

        /// <summary>
        /// 日期
        /// </summary>
        DATE = 0x09
    }

    /// <summary>
    /// 数据值大小枚举（用于写入操作）
    /// </summary>
    public enum DValueSize
    {
        /// <summary>
        /// 空值
        /// </summary>
        NULL = 0x00,

        /// <summary>
        /// 位
        /// </summary>
        BIT = 0x03,

        /// <summary>
        /// 字节/字/双字
        /// </summary>
        BWD = 0x04,

        /// <summary>
        /// 整数
        /// </summary>
        INTERGER = 0x05,

        /// <summary>
        /// 实数
        /// </summary>
        REAL = 0x07,
        DWORD = 0x06,

        /// <summary>
        /// 八位字节串
        /// </summary>
        OCTETSTRING = 0x09
    }
}
