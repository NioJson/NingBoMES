using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Common
{
    /// <summary>
    /// 日志操作类
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// 文件路劲
        /// </summary>
        private string _logPath;
        /// <summary>
        /// 日志记录队列
        /// </summary>
        private ConcurrentQueue<LoggerObj> _loggerObjs;
        /// <summary>
        /// 单例
        /// </summary>
        public static readonly Logger sLogger = new Logger();
        public Logger()
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _loggerObjs = new ConcurrentQueue<LoggerObj>();
            Thread td = new Thread(new ThreadStart(ThreadWorker));
            td.Start();
        }
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="loggerObj">日志内容</param>
        public void InsertLog(LoggerObj loggerObj)
        {
            _loggerObjs.Enqueue(loggerObj);
        }
        /// <summary>
        /// 执行写操作的线程
        /// </summary>
        private void ThreadWorker()
        {
            while (!GlobalProperty.sGlobalProperty.GetMainFormCloseStatus())
            {
                if (_loggerObjs.TryDequeue(out LoggerObj loggerObj))
                {
                    if (loggerObj != null)
                    {
                        switch (loggerObj.LoggerType)
                        {
                            case LoggerType.Information:
                                InputLogInfor("Infor", loggerObj);
                                break;
                            case LoggerType.Error:
                                InputLogInfor("Error", loggerObj);
                                break;
                            default:
                                break;
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// 写入日志信息
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="loggerObj">日志内容</param>
        private void InputLogInfor(string fileName, LoggerObj loggerObj)
        {
            try
            {
                string logFileName = fileName + $"_{DateTime.Now:yyyyMMdd}.txt";
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }
                string logFilePath = Path.Combine(_logPath, logFileName);
                File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}" + Environment.NewLine);
                File.AppendAllText(logFilePath, loggerObj.Content + Environment.NewLine + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }
    }
    /// <summary>
    /// 日志实体类
    /// </summary>
    public class LoggerObj
    {
        public string Content { get; set; }
        public LoggerType LoggerType { get; set; }
    }
    /// <summary>
    /// 日志类型枚举
    /// </summary>
    public enum LoggerType
    {
        Information,
        Error
    }
}
