using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MissionBossAot.Views
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// 消息模式
    /// </summary>
    public enum MessageMode
    {
        AutoClose,   // 自动关闭模式
        Confirm      // 确认关闭模式
    }

    /// <summary>
    /// 消息结果
    /// </summary>
    public enum MessageResult
    {
        AutoClosed,   // 自动关闭
        Confirmed,    // 确认关闭
        Cancelled     // 取消关闭
    }

    /// <summary>
    /// 全局消息弹窗
    /// </summary>
    public partial class GlobalMessageDialog : Window
    {
        private Timer _autoCloseTimer;
        private DispatcherTimer _progressTimer;
        private int _remainingSeconds;
        private int _durationSeconds;
        private MessageMode _currentMode;

        /// <summary>
        /// 对话框结果
        /// </summary>
        public MessageResult DialogResult { get; private set; } = MessageResult.Cancelled;

        public GlobalMessageDialog()
        {
            InitializeComponent();
            InitializeWindowEvents();
        }

        private void InitializeWindowEvents()
        {
            BtnClose.Click += (s, e) => CloseDialog(false);
            BtnConfirm.Click += (s, e) => CloseDialog(true);

            // 允许通过ESC键关闭确认弹窗
            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape && _currentMode == MessageMode.Confirm)
                {
                    CloseDialog(false);
                }
            };
        }

        /// <summary>
        /// 显示消息（同步方法）
        /// </summary>
        public static MessageResult Show(
            string message,
            string title = "提示",
            MessageType messageType = MessageType.Info,
            MessageMode mode = MessageMode.AutoClose,
            int autoCloseSeconds = 5,
            Window owner = null)
        {
            var dialog = new GlobalMessageDialog();

            // 设置所有者窗口
            if (owner != null)
            {
                dialog.Owner = owner;
            }

            // 配置对话框
            dialog.ConfigureDialog(message, title, messageType, mode, autoCloseSeconds);

            // 显示对话框（模态）
            dialog.ShowDialog();

            // 返回结果
            return dialog.DialogResult;
        }

        private void ConfigureDialog(string message, string title, MessageType messageType,
            MessageMode mode, int autoCloseSeconds)
        {
            _currentMode = mode;
            _durationSeconds = autoCloseSeconds;
            _remainingSeconds = autoCloseSeconds;

            // 设置标题和图标
            TitleText.Text = title;
            SetMessageIcon(messageType);

            // 设置消息内容
            TxtMessage.Text = message;

            // 根据模式显示不同的UI
            if (mode == MessageMode.AutoClose)
            {
                AutoClosePanel.Visibility = Visibility.Visible;
                ConfirmPanel.Visibility = Visibility.Collapsed;
                StartAutoClose();
            }
            else
            {
                AutoClosePanel.Visibility = Visibility.Collapsed;
                ConfirmPanel.Visibility = Visibility.Visible;
                BtnConfirm.Focus(); // 聚焦到确认按钮
            }
        }

        private void SetMessageIcon(MessageType messageType)
        {
            string iconData = string.Empty;

            switch (messageType)
            {
                case MessageType.Success:
                    // 成功图标（对勾）
                    iconData = "M0,5 L4,9 L10,0";
                    TitleIcon.Data = Geometry.Parse(iconData);
                    TitleIcon.Stroke = Brushes.White;
                    TitleIcon.StrokeThickness = 1.5;
                    TitleIcon.Fill = null;
                    break;

                case MessageType.Warning:
                    // 警告图标（感叹号三角形）
                    iconData = "M5,0 L10,10 L0,10 Z M5,3 L4,7 L6,7 Z M5,8 A0.8,0.8 0 1,0 5,9.6 A0.8,0.8 0 1,0 5,8";
                    TitleIcon.Data = Geometry.Parse(iconData);
                    TitleIcon.Fill = Brushes.White;
                    TitleIcon.Stroke = null;
                    break;

                case MessageType.Error:
                    // 错误图标（圆形叉号）
                    iconData = "M5,0 A5,5 0 1,0 5,10 A5,5 0 1,0 5,0 M3,3 L7,7 M7,3 L3,7";
                    TitleIcon.Data = Geometry.Parse(iconData);
                    TitleIcon.Stroke = Brushes.White;
                    TitleIcon.StrokeThickness = 1;
                    TitleIcon.Fill = null;
                    break;

                case MessageType.Info:
                default:
                    // 信息图标（圆形 i）
                    iconData = "M5,0 A5,5 0 1,0 5,10 A5,5 0 1,0 5,0 M5,3 L5,6 M5,7 L5,8";
                    TitleIcon.Data = Geometry.Parse(iconData);
                    TitleIcon.Stroke = Brushes.White;
                    TitleIcon.StrokeThickness = 1;
                    TitleIcon.Fill = null;
                    break;
            }
        }

        private void StartAutoClose()
        {
            // 启动进度条更新定时器
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();

            // 启动倒计时定时器
            _autoCloseTimer = new Timer(
                AutoCloseTimerCallback,
                null,
                1000,  // 1秒后开始
                1000   // 每秒执行一次
            );
        }

        private void AutoCloseTimerCallback(object state)
        {
            _remainingSeconds--;

            // 更新倒计时文本
            Dispatcher.Invoke(() =>
            {
                if (_remainingSeconds > 0)
                {
                    TxtCountdown.Text = $"{_remainingSeconds} 秒后自动关闭";
                }
                else
                {
                    // 倒计时结束，关闭窗口
                    CloseDialog(false);
                }
            });
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (_durationSeconds <= 0) return;

            // 计算进度百分比
            double percent = 1.0 - (double)_remainingSeconds / _durationSeconds;
            double maxWidth = (ProgressBar.Parent as FrameworkElement)?.ActualWidth ?? 300;
            ProgressBar.Width = maxWidth * percent;
        }

        private void CloseDialog(bool isConfirmed)
        {
            // 停止定时器
            _autoCloseTimer?.Dispose();
            _progressTimer?.Stop();

            // 设置结果
            DialogResult = _currentMode == MessageMode.AutoClose
                ? MessageResult.AutoClosed
                : (isConfirmed ? MessageResult.Confirmed : MessageResult.Cancelled);

            // 关闭窗口
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _autoCloseTimer?.Dispose();
            _progressTimer?.Stop();
            base.OnClosed(e);
        }
    }
}
