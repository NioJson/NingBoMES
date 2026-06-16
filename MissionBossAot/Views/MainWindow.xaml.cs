using CommunityToolkit.Mvvm.Input;
using MissionBossAot.Common;
using MissionBossAot.Models;
using MissionBossAot.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

namespace MissionBossAot.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<CardItem> centerCards;
        private bool isMaximized = false;
        private MainWindowViewModel mainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            //主窗体开启状态
            GlobalProperty.sGlobalProperty.SetMainFormCloseStatus(false);
            this.mainWindowViewModel = new MainWindowViewModel(this);
            this.DataContext = this.mainWindowViewModel;


            InitializeData();
            UpdateDateTime();

            // 定时更新状态栏时间
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => UpdateDateTime();
            timer.Start();

            GlobalDevice.sGlobalDevice.DeviceListInit();
        }
        private void InitializeData()
        {
            //首页中间菜单栏
            CenterCardsList.ItemsSource = this.mainWindowViewModel.CenterCardsList;
            //首页右边任务栏初始化
            GlobalProperty.sGlobalProperty.MissionListInit(this);
        }
        
        private void UpdateDateTime()
        {
            DateTimeText.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private void NormalCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 获取被点击的 Border
            var border = sender as Border;
            if (border != null && border.DataContext is NormalCard card)
            {
                WorkOrderMission workOrderMission = new WorkOrderMission(card.WorkOrderId,this);
                workOrderMission.Owner = this;
                workOrderMission.ShowDialog();
            }
        }
        // 标题栏拖拽移动窗口
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeBtn_Click(sender, e);
            }
            else
            {
                this.DragMove();
            }
        }

        // 最小化
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 最大化/还原
        private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MaximizeBtn.Content = "□";
                isMaximized = false;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MaximizeBtn.Content = "❐";
                isMaximized = true;
            }
        }

        // 关闭窗口
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要退出程序吗？", "确认退出",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                //主窗体开启状态
                GlobalProperty.sGlobalProperty.SetMainFormCloseStatus(true);
                Application.Current.Shutdown();
            }
        }

        // 退出登录
        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要退出登录吗？", "确认退出",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // 这里可以添加退出登录的逻辑
                StatusText.Text = "已退出登录";
                UserInfoText.Text = "未登录";

                // 模拟重新登录对话框
                System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        var loginResult = MessageBox.Show("是否重新登录？", "登录",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (loginResult == MessageBoxResult.Yes)
                        {
                            UserInfoText.Text = "张三";
                            StatusText.Text = "登录成功";
                        }
                    });
                });
            }
        }

        private void AddCardBtn_Click(object sender, RoutedEventArgs e)
        {
            var newCard = new CardItem
            {
                Title = $"新卡片 {centerCards.Count + 1}",
               // Description = "这是一个新添加的卡片，包含示例内容。可以继续添加更多卡片来测试滚动效果。",
              //  Date = DateTime.Now.ToString("yyyy-MM-dd"),
                Color = GetRandomColor()
            };
            centerCards.Add(newCard);
            StatusText.Text = $"已添加卡片: {newCard.Title}";
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            GlobalProperty.sGlobalProperty.MissionListInit(this);
        }

        private string GetRandomColor()
        {
            string[] colors = { "#3498DB", "#E74C3C", "#2ECC71", "#F39C12", "#9B59B6", "#1ABC9C" };
            Random random = new Random();
            return colors[random.Next(colors.Length)];
        }
    }
    // 左侧窗体项数据模型
    public class WindowItem
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
    }
    // 右侧卡片数据模型
    public class RightCardItem
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Color { get; set; }
        public string StationCount { get; set; }
        public List<StatusItem> StatusList { get; set; }
        public string WorkingTime { get; set; }
        public string MissionType { get; set; }
        public string StationType { get; set; }
    }
    public class StatusItem
    {
        public string Value { get; set; }  // 显示文字：优、良、中、差等
        public string Color { get; set; }  // 背景颜色
    }
    // 中间卡片数据模型
    public class CardItem
    {
        public string Title { get; set; }
        public string Color { get; set; }
        public List<ButtonItem> Buttons { get; set; }
    }

    public class ButtonItem
    {
        public string Text { get; set; }
        public ICommand Command { get; set; }
        public string Color { get; set; }
        public object CommandParameter { get; set; }
    }
    // 基础接口或基类（可选）
    public interface ICardItem
    {
        string WorkingTime { get; set; }
        RightCardType CardType { get; set; }
    }
    public enum RightCardType
    {
        None,
        NormalCard,
        SpecialCard
    }
}
