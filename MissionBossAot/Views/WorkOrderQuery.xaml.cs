using MissionBossAot.Common;
using MissionBossAot.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// WorkOrderQuery.xaml 的交互逻辑
    /// </summary>
    public partial class WorkOrderQuery : Window
    {
        private ObservableCollection<WorkOrderModel> _allRunningWorkOrders;
        public WorkOrderQuery()
        {
            InitializeComponent();            
            InitializeDateTimePickers();
            InitializeData();
        }
        public void SetProcessCode(string processCode = "")
        {
            ProcessCodeBox.Text = processCode;
        }
        private void LoadTestData(DataTable dtWorkOrder)
        {
            List<ProcessCodeModel> processListTemp = new List<ProcessCodeModel>();
            if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
            {
                foreach (DataRow row in dtWorkOrder.Rows)
                {
                    string code = !Convert.IsDBNull(row["ProcessCode"]) ? row["ProcessCode"].ToString() : "";
                    processListTemp.Add(
                            new ProcessCodeModel
                            {
                                ProcessName = code,
                                StatusColor = "#4CAF50",
                                TagColor = "#2196F3",
                                TagText = "进行中"
                            }
                        );
                }
            }
            // 创建测试数据集合
        //    var processList = new List<ProcessCodeModel>
        //{
        //    new ProcessCodeModel
        //    {
        //        ProcessName = "组装过程码 A01",
        //        StatusColor = "#4CAF50",
        //        TagColor = "#2196F3",
        //        TagText = "进行中"
        //    },
        //    new ProcessCodeModel
        //    {
        //        ProcessName = "测试过程码 B02",
        //        StatusColor = "#FF9800",
        //        TagColor = "#9C27B0",
        //        TagText = "待检"
        //    },
        //    new ProcessCodeModel
        //    {
        //        ProcessName = "包装过程码 C03",
        //        StatusColor = "#2196F3",
        //        TagColor = "#4CAF50",
        //        TagText = "已完成"
        //    }
        //};

            // 绑定到ItemsControl
            ProcessResultPanel.ItemsSource = processListTemp;
        }
        private void ProcessCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var processData = border?.DataContext as ProcessCodeModel; // 替换为你的数据类型
            if (processData != null)
            {
                // 初始化统计卡片数据
                var cards = new ObservableCollection<StatisticsCard>();

                ObservableCollection<TestItem> testItems_p10 = new ObservableCollection<TestItem>();
                cards.Add(new StatisticsCard
                {
                    CardTitle = "P10 齿轮、铜套压装",
                    HeaderColor = "#3498DB",
                    TestItems = testItems_p10
                });
                DataTable result_p10 = GlobalBaseData.sGlobalBaseData.GetStationData("P10DB2000ReturnData", processData.ProcessName);
                foreach (DataRow row in result_p10.Rows) 
                {
                    foreach(DataColumn col in result_p10.Columns)
                    {
                        string val = !Convert.IsDBNull(row[col.ColumnName]) ? row[col.ColumnName].ToString() : "";
                        testItems_p10.Add(new TestItem() 
                        {
                            Name = col.ColumnName,
                            Value = val
                        });
                    }
                }

                ObservableCollection<TestItem> testItems_p20 = new ObservableCollection<TestItem>();
                cards.Add(new StatisticsCard
                {
                    CardTitle = "P20 丝杆部件压装",
                    HeaderColor = "#3498DB",
                    TestItems = testItems_p20
                });
                DataTable result_p20 = GlobalBaseData.sGlobalBaseData.GetStationData("P20DB2000ReturnData", processData.ProcessName);
                foreach (DataRow row in result_p20.Rows)
                {
                    foreach (DataColumn col in result_p20.Columns)
                    {
                        string val = !Convert.IsDBNull(row[col.ColumnName]) ? row[col.ColumnName].ToString() : "";
                        testItems_p20.Add(new TestItem()
                        {
                            Name = col.ColumnName,
                            Value = val
                        });
                    }
                }

                ObservableCollection<TestItem> testItems_p30 = new ObservableCollection<TestItem>();
                cards.Add(new StatisticsCard
                {
                    CardTitle = "P30 活塞杆部件装配",
                    HeaderColor = "#3498DB",
                    TestItems = testItems_p30
                });
                DataTable result_p30 = GlobalBaseData.sGlobalBaseData.GetStationData("P30DB3000ReturnData", processData.ProcessName);
                foreach (DataRow row in result_p30.Rows)
                {
                    foreach (DataColumn col in result_p30.Columns)
                    {
                        string val = !Convert.IsDBNull(row[col.ColumnName]) ? row[col.ColumnName].ToString() : "";
                        testItems_p30.Add(new TestItem()
                        {
                            Name = col.ColumnName,
                            Value = val
                        });
                    }
                }

                ObservableCollection<TestItem> testItems_p40 = new ObservableCollection<TestItem>();
                cards.Add(new StatisticsCard
                {
                    CardTitle = "P40 电机、齿轮箱装配",
                    HeaderColor = "#3498DB",
                    TestItems = testItems_p40
                });
                DataTable result_p40 = GlobalBaseData.sGlobalBaseData.GetStationData("P40DB3001ReturnData", processData.ProcessName);
                foreach (DataRow row in result_p40.Rows)
                {
                    foreach (DataColumn col in result_p40.Columns)
                    {
                        string val = !Convert.IsDBNull(row[col.ColumnName]) ? row[col.ColumnName].ToString() : "";
                        testItems_p40.Add(new TestItem()
                        {
                            Name = col.ColumnName,
                            Value = val
                        });
                    }
                }

                ObservableCollection<TestItem> testItems_p50 = new ObservableCollection<TestItem>();
                cards.Add(new StatisticsCard
                {
                    CardTitle = "P50 传感器装配",
                    HeaderColor = "#3498DB",
                    TestItems = testItems_p50
                });
                DataTable result_p50 = GlobalBaseData.sGlobalBaseData.GetStationData("P50DB3002ReturnData", processData.ProcessName);
                foreach (DataRow row in result_p50.Rows)
                {
                    foreach (DataColumn col in result_p50.Columns)
                    {
                        string val = !Convert.IsDBNull(row[col.ColumnName]) ? row[col.ColumnName].ToString() : "";
                        testItems_p50.Add(new TestItem()
                        {
                            Name = col.ColumnName,
                            Value = val
                        });
                    }
                }

                ObservableCollection<TestItem> testItems_p60 = new ObservableCollection<TestItem>();
                cards.Add(new StatisticsCard
                {
                    CardTitle = "P60 气密性测试",
                    HeaderColor = "#3498DB",
                    TestItems = testItems_p60
                });
                DataTable result_p60 = GlobalBaseData.sGlobalBaseData.GetStationData("P60DB3000ReturnData", processData.ProcessName);
                foreach (DataRow row in result_p60.Rows)
                {
                    foreach (DataColumn col in result_p60.Columns)
                    {
                        string val = !Convert.IsDBNull(row[col.ColumnName]) ? row[col.ColumnName].ToString() : "";
                        testItems_p60.Add(new TestItem()
                        {
                            Name = col.ColumnName,
                            Value = val
                        });
                    }
                }

                ObservableCollection<TestItem> testItems_p70_lao = new ObservableCollection<TestItem>();
                cards.Add(new StatisticsCard
                {
                    CardTitle = "P70 老化、性能测试",
                    HeaderColor = "#3498DB",
                    TestItems = testItems_p70_lao
                });
                DataTable result_p70_lao = GlobalBaseData.sGlobalBaseData.GetStationData("P70DB3003ReturnData", processData.ProcessName);
                foreach (DataRow row in result_p70_lao.Rows)
                {
                    foreach (DataColumn col in result_p70_lao.Columns)
                    {
                        string val = !Convert.IsDBNull(row[col.ColumnName]) ? row[col.ColumnName].ToString() : "";
                        testItems_p70_lao.Add(new TestItem()
                        {
                            Name = col.ColumnName,
                            Value = val
                        });
                    }
                }

                ObservableCollection<TestItem> testItems_p70_biao = new ObservableCollection<TestItem>();
                cards.Add(new StatisticsCard
                {
                    CardTitle = "P70 标定",
                    HeaderColor = "#3498DB",
                    TestItems = testItems_p70_biao
                });
                DataTable result_p70_biao = GlobalBaseData.sGlobalBaseData.GetStationData("signal_data", processData.ProcessName);
                foreach (DataRow row in result_p70_biao.Rows)
                {
                    foreach (DataColumn col in result_p70_biao.Columns)
                    {
                        string val = !Convert.IsDBNull(row[col.ColumnName]) ? row[col.ColumnName].ToString() : "";
                        testItems_p70_biao.Add(new TestItem()
                        {
                            Name = col.ColumnName,
                            Value = val
                        });
                    }
                }

                ObservableCollection<TestItem> testItems_p70_la = new ObservableCollection<TestItem>();
                cards.Add(new StatisticsCard
                {
                    CardTitle = "P70 推拉力",
                    HeaderColor = "#3498DB",
                    TestItems = testItems_p70_la
                });
                DataTable result_p70_la = GlobalBaseData.sGlobalBaseData.GetStationData("signal_data2", processData.ProcessName);
                foreach (DataRow row in result_p70_la.Rows)
                {
                    foreach (DataColumn col in result_p70_la.Columns)
                    {
                        string val = !Convert.IsDBNull(row[col.ColumnName]) ? row[col.ColumnName].ToString() : "";
                        testItems_p70_la.Add(new TestItem()
                        {
                            Name = col.ColumnName,
                            Value = val
                        });
                    }
                }

                ObservableCollection<TestItem> testItems_p80 = new ObservableCollection<TestItem>();
                cards.Add(new StatisticsCard
                {
                    CardTitle = "P80 噪音测试",
                    HeaderColor = "#3498DB",
                    TestItems = testItems_p80
                });
                DataTable result_p80 = GlobalBaseData.sGlobalBaseData.GetStationData("P80DB3004ReturnData", processData.ProcessName);
                foreach (DataRow row in result_p80.Rows)
                {
                    foreach (DataColumn col in result_p80.Columns)
                    {
                        string val = !Convert.IsDBNull(row[col.ColumnName]) ? row[col.ColumnName].ToString() : "";
                        testItems_p80.Add(new TestItem()
                        {
                            Name = col.ColumnName,
                            Value = val
                        });
                    }
                }

                StatisticsCardsControl.ItemsSource = cards;
            }
        }
        
        private void InitializeData()
        {
            _allRunningWorkOrders = new ObservableCollection<WorkOrderModel>();
            DataGrid_DataBaseData.ItemsSource = _allRunningWorkOrders;
        }
        private void InitializeDateTimePickers()
        {
            // 设置默认日期为当天
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today;
        }
        private void ToggleCardExpand(object sender, MouseButtonEventArgs e)
        {
            // 获取实际的DataContext
            var border = sender as Border;
            var card = border?.DataContext as StatisticsCard;

            if (card != null)
            {
                card.IsExpanded = !card.IsExpanded;
                e.Handled = true;
            }
        }
         
        private void SearchWorkOrders_Click(object sender, RoutedEventArgs e)
        {
            // 验证时间输入
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("请选择开始和结束日期", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DateTime startTime = StartDatePicker.SelectedDate.Value;
            DateTime endTime = EndDatePicker.SelectedDate.Value;
            if (TimeSpan.TryParse(StartTimeBox.Text, out TimeSpan startSpan))
                startTime = startTime.Date + startSpan;
            if (TimeSpan.TryParse(EndTimeBox.Text, out TimeSpan endSpan))
                endTime = endTime.Date + endSpan;
            // 检查时间范围
            if (startTime > endTime)
            {
                MessageBox.Show("开始时间不能大于结束时间", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string workOrderNo = "";
            if (!string.IsNullOrEmpty(WorkOrderNoBox.Text))
            {
                workOrderNo = WorkOrderNoBox.Text;
            }
            LoadMockData(workOrderNo, startTime, endTime);
        }
        private void DataGrid_DataBaseData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 获取被选中的行（单行选中模式）
            var selectedItem = DataGrid_DataBaseData.SelectedItem as WorkOrderModel; // 替换为你的数据模型类型
            if (selectedItem != null)
            {
                // 处理选中逻辑
                int orderId = selectedItem.Id;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM WorkOrderNewProductCode where WorkOrderId="+ orderId;
                    DataTable dtWorkOrder = helper.ExecuteDataTable(sqlSelect);
                    LoadTestData(dtWorkOrder);
                }
            }
        }
        private void LoadMockData(string workOrderNo, DateTime startTime, DateTime endTime)
        {
            _allRunningWorkOrders.Clear();
            DataTable dtWorkOrder = null;
            using (var helper = new SqlConnectionHelper())
            {
                string _currentStatus = " and EnableStatus <> '未下发'";
                string _workOrderNoSql = "";
                if (!string.IsNullOrEmpty(workOrderNo))
                {
                    _workOrderNoSql = " and OrderId like '%" + workOrderNo + "%'";
                }
                string sqlSelect = "SELECT * FROM WorkOrderNew where Deleted='F' " + _currentStatus + _workOrderNoSql +
                    " and CreateDatetime > '" + startTime + "' and CreateDatetime < '"+ endTime + "'" +
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
                    _allRunningWorkOrders.Add(new WorkOrderModel
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
        private void SearchProcessCode_Click(object sender, RoutedEventArgs e)
        {
            string code = ProcessCodeBox.Text;
            WorkOrderLinkCode lc = GlobalBaseData.sGlobalBaseData.GetWorkOrderLinkCode(code);
            if (lc != null)
            {
                List<ProcessCodeModel> processListTemp = new List<ProcessCodeModel>();
                processListTemp.Add(
                               new ProcessCodeModel
                               {
                                   ProcessName = code,
                                   StatusColor = "#4CAF50",
                                   TagColor = "#2196F3",
                                   TagText = "进行中"
                               }
                           );
                ProcessResultPanel.ItemsSource = processListTemp;

                _allRunningWorkOrders.Clear();
                DataTable dtWorkOrder = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM WorkOrderNew where ID="+lc.WorkOrderId;
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
                        _allRunningWorkOrders.Add(new WorkOrderModel
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
        }
        // 关闭按钮
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
    
    public class TestItem
    {
        public string Name { get; set; }      // 指标名称
        public string Value { get; set; }     // 指标数值
    }
    public class StatisticsCard : INotifyPropertyChanged
    {
        public string CardTitle { get; set; }           // 卡片标题
        public string SubTitle { get; set; }            // 副标题
        public string Icon { get; set; }                // 图标字符
        public string HeaderColor { get; set; }         // 头部背景色
        public ObservableCollection<TestItem> TestItems { get; set; }  // 测试指标集合

        // 折叠/展开相关属性
        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(); OnPropertyChanged(nameof(ArrowIcon)); OnPropertyChanged(nameof(ContentVisibility)); }
        }
        public string ArrowIcon => IsExpanded ? "▼" : "▶";
        public Visibility ContentVisibility => IsExpanded ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HasSubTitle => string.IsNullOrEmpty(SubTitle) ? Visibility.Collapsed : Visibility.Visible;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public class ProcessCodeModel
    {
        public string ProcessName { get; set; }
        public string ProcessDescription { get; set; }
        public string LastUpdateTime { get; set; }
        public string StatusColor { get; set; }
        public string TagColor { get; set; }
        public string TagText { get; set; }
    }
}
