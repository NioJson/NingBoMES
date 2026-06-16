using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    /// <summary>
    /// 数据参数类，用于PLC通信中的数据读写参数
    /// </summary>
    public class DataParameter
    {
        /// <summary>
        /// 参数唯一标识符
        /// </summary>
        public string id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 存储区
        /// </summary>
        public Areas Area { get; set; }

        /// <summary>
        /// DB块编号
        /// </summary>
        public ushort DBNumber { get; set; } = 0;

        /// <summary>
        /// 参数值大小（读取时使用）
        /// </summary>
        public PValueSize PValueSize { get; set; }

        /// <summary>
        /// 数据值大小（写入时使用）
        /// </summary>
        public DValueSize DValueSize { get; set; }

        /// <summary>
        /// 字节地址
        /// </summary>
        public int ByteAddress { get; set; } = 0;

        /// <summary>
        /// 位地址
        /// </summary>
        public byte BitAddress { get; set; } = 0;

        /// <summary>
        /// 数据数量
        /// </summary>
        public ushort Count { get; set; } = 1;

        /// <summary>
        /// 数据类型
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// 数据集合
        /// 读取时：填充返回的结果
        /// 写入时：填充将写入的数据
        /// </summary>
        public List<object> Datas { get; set; } = new List<object>();

        /// <summary>
        /// 状态标识
        /// </summary>
        public bool Status { get; set; } = true;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }
    }
}
