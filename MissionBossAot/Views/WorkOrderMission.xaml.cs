using MissionBossAot.Common;
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

namespace MissionBossAot.Views
{
    /// <summary>
    /// WorkOrderMission.xaml 的交互逻辑
    /// </summary>
    public partial class WorkOrderMission : Window
    {
        private int workOrderId;
        private Window mainForm;
        public WorkOrderMission(int id,Window main)
        {
            InitializeComponent();
            workOrderId = id;
            mainForm = main;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // 关闭窗口
        private void BtnWindowClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 确认按钮点击事件
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // 可以在这里获取 rbtnCompleted.IsChecked 和 rbtnEnd.IsChecked 的值
            string statusString = string.Empty;
            if ((bool)rbtnCompleted.IsChecked)
            {
                statusString = "已完成";
            }
            if ((bool)rbtnEnd.IsChecked)
            {
                statusString = "结束";
            }
            using (var helper = new SqlConnectionHelper())
            {
                try
                {
                    helper.BeginTransaction();

                    string sqlDel = "update WorkOrderNew set EnableStatus='"+ statusString + "' where ID="+ workOrderId;
                    helper.ExecuteNonQuery(sqlDel);

                    helper.CommitTransaction();

                    DialogResult = true;
                    this.Close();
                    GlobalProperty.sGlobalProperty.MissionListInit(mainForm);
                }
                catch (Exception)
                {
                    helper.RollbackTransaction();
                    MessageBox.Show("操作错误，请重试！");
                }
            }
        }

        // 关闭按钮点击事件
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
