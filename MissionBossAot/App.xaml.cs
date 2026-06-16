using MissionBossAot.Common;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace MissionBossAot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // <summary>
        /// 将可能藏在任务栏的窗口恢复到可视区域
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int Msg);
        /// <summary>
        /// 让用户立刻能看到并操作这个窗口
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        /// <summary>
        /// 验证是否已经启动同一个程序了
        /// </summary>
        private bool CheckAndActivateExistingInstance()
        {
            try
            {
                var current = Process.GetCurrentProcess();
                var existing = Process.GetProcessesByName(current.ProcessName)
                    .FirstOrDefault(p => p.Id != current.Id &&
                                         p.MainModule?.FileName == current.MainModule?.FileName);

                if (existing != null)
                {
                    ShowWindow(existing.MainWindowHandle, 1);
                    SetForegroundWindow(existing.MainWindowHandle);
                    return false; // 存在已有实例
                }
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj()
                {
                    Content = "系统启动出现异常*****>>" + ex.Message,
                    LoggerType = LoggerType.Error
                });
            }
            return true; // 没有已有实例，可以继续
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            // 在 OnStartup 最开始执行单例检查
            if (!CheckAndActivateExistingInstance())
            {
                // 已有实例并已激活，退出当前实例
                Environment.Exit(-1);
                return;
            }
            base.OnStartup(e);
            // 可以在这里进行全局初始化
            // 例如：加载配置、连接数据库等
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // 程序退出时的清理工作
        }
    }
}
