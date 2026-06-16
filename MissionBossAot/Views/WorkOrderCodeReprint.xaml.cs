using MissionBossAot.Common;
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
    /// WorkOrderCodeReprint.xaml 的交互逻辑
    /// </summary>
    public partial class WorkOrderCodeReprint : Window
    {
        private ObservableCollection<WorkOrderModel> _allWorkOrders;
        public WorkOrderCodeReprint()
        {
            InitializeComponent();
            InitializeData();
        }
        private void InitializeData()
        {
            _allWorkOrders = new ObservableCollection<WorkOrderModel>();
            DataGrid_DataBaseData.ItemsSource = _allWorkOrders;
        }
        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            string workOrderNo = "";
            if (!string.IsNullOrEmpty(WorkOrderTextBox.Text))
            {
                workOrderNo = WorkOrderTextBox.Text;
            }
            LoadMockData(workOrderNo);
        }
        private void LoadMockData(string workOrderNo)
        {
            _allWorkOrders.Clear();
            DataTable dtWorkOrder = null;
            using (var helper = new SqlConnectionHelper())
            {
                string _currentStatus = " and EnableStatus='已下发'";
                string _workOrderNoSql = "";
                if (!string.IsNullOrEmpty(workOrderNo))
                {
                    _workOrderNoSql = " and OrderId like '%" + workOrderNo + "%'";
                }
                string sqlSelect = "SELECT * FROM WorkOrderNew where Deleted='F' " + _currentStatus + _workOrderNoSql +
                    " order by CreateDatetime desc";
                dtWorkOrder = helper.ExecuteDataTable(sqlSelect);
            }
            if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
            {
                for (int i = 0; i < dtWorkOrder.Rows.Count; i++)
                {
                    DataRow row = dtWorkOrder.Rows[i];
                    string amount = !Convert.IsDBNull(row["OrderAmount"]) ? row["OrderAmount"].ToString() : "";
                    int.TryParse(amount, out int resInt);

                    string idInt = !Convert.IsDBNull(row["ID"]) ? row["ID"].ToString() : "";
                    int.TryParse(idInt, out int idI);
                    _allWorkOrders.Add(new WorkOrderModel
                    {
                        Id = idI,
                        RowNumber = i + 1,
                        OrderId = !Convert.IsDBNull(row["OrderId"]) ? row["OrderId"].ToString() : "",
                        ModelText = !Convert.IsDBNull(row["ModelText"]) ? row["ModelText"].ToString() : "",
                        FlowOrderId = !Convert.IsDBNull(row["FlowOrderId"]) ? row["FlowOrderId"].ToString() : "",
                        RoutingText = !Convert.IsDBNull(row["RoutingText"]) ? row["RoutingText"].ToString() : "",
                        ProductName = !Convert.IsDBNull(row["ProductName"]) ? row["ProductName"].ToString() : "",
                        OrderAmount = resInt,
                        WorkBackNo = !Convert.IsDBNull(row["WorkBackNo"]) ? row["WorkBackNo"].ToString() : "",
                        EnableStatus = !Convert.IsDBNull(row["EnableStatus"]) ? row["EnableStatus"].ToString() : "",
                        CreateDatetime = !Convert.IsDBNull(row["CreateDatetime"]) ? row["CreateDatetime"].ToString() : "",
                        OrderStartDatetime = !Convert.IsDBNull(row["OrderStartDatetime"]) ? row["OrderStartDatetime"].ToString() : "",
                        PlanedCompleteDatetime = !Convert.IsDBNull(row["OrderStartDatetime"]) ? row["OrderStartDatetime"].ToString() : ""
                    });
                }
            }
        }
        private void PrintLabelButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var dataItem = btn?.Tag;
            if (dataItem != null)
            {
                WorkOrderModel orderModel = dataItem as WorkOrderModel;
                PrintLabelDialog dialog = new PrintLabelDialog(orderModel.Id);
                dialog.Owner = this;
                dialog.ShowDialog();
            }
        }
        // 关闭按钮
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private void Window_Activated(object sender, EventArgs e)
        {
        }
    }
}
