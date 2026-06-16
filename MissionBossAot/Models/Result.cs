using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    /// <summary>
    /// 泛型结果类，用于封装操作结果和数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// 操作状态
        /// </summary>
        public bool Status { get; set; } = true;

        /// <summary>
        /// 操作消息
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// 数据集合
        /// </summary>
        public List<T> Datas { get; set; } = new List<T>();

        /// <summary>
        /// 临时数据集合
        /// </summary>
        public List<object> Temp { get; set; } = new List<object>();

        /// <summary>
        /// 属性集合
        /// </summary>
        public ArrayList MyProperty { get; set; } = new ArrayList();

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public Result() : this(true, "OK") { }

        /// <summary>
        /// 带状态和消息的构造函数
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="msg">消息</param>
        public Result(bool state, string msg) : this(state, msg, new List<T>()) { }

        /// <summary>
        /// 带状态、消息和数据的构造函数
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="msg">消息</param>
        /// <param name="datas">数据</param>
        public Result(bool state, string msg, List<T> datas)
        {
            this.Status = state;
            this.Message = msg;
            this.Datas = datas;
        }
    }

    /// <summary>
    /// 结果类，用于封装操作结果
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 操作状态
        /// </summary>
        public bool Status { get; set; } = true;

        /// <summary>
        /// 操作消息
        /// </summary>
        public string Message { get; set; } = "";
    }
}
