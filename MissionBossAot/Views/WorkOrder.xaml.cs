using MissionBossAot.Common;
using MissionBossAot.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MissionBossAot.Views
{
    public partial class WorkOrder : Window
    {
        private ObservableCollection<WorkOrderModel> _allWorkOrders;
        private WorkOrderModel _selectedWorkOrder;
        private Window _mainForm;

        public WorkOrder(Window mainWindow)
        {
            InitializeComponent();
            this._mainForm = mainWindow;
            InitializeData();
            LoadData();
        }
        private void Window_Activated(object sender, EventArgs e)
        {
        }
        private void InitializeData()
        {
            _allWorkOrders = new ObservableCollection<WorkOrderModel>();
            dgWorkOrders.ItemsSource = _allWorkOrders;

            // 初始化默认日期
            dpStartDate.SelectedDate = DateTime.Now;
            txtStartTime.Text = DateTime.Now.ToString("HH:mm:ss");
            dpPlanCompleteDate.SelectedDate = DateTime.Now;
            txtPlanCompleteTime.Text = DateTime.Now.AddHours(6).ToString("HH:mm:ss");
        }

        private void LoadData()
        {
            LoadModelItems();
            LoadMockData(null,null,null);
            LoadStations();
        }
        //获取所有工站
        private List<WorkStationItem> GetWorkStations()
        {
            var modelsWorkStation = new List<WorkStationItem>();
            DataTable dtWorkStation = null;
            try
            {
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM WorkStation where EnableStatus='T' order by ExecutionOrder";
                    dtWorkStation = helper.ExecuteDataTable(sqlSelect);
                }
                if (dtWorkStation != null && dtWorkStation.Rows.Count > 0)
                {
                    foreach (DataRow row in dtWorkStation.Rows)
                    {
                        int.TryParse(row["ID"].ToString(), out int idd);
                        modelsWorkStation.Add(new WorkStationItem
                        {
                            Id = idd,
                            Code = !Convert.IsDBNull(row["StationCode"]) ? row["StationCode"].ToString() : "",
                            Name = !Convert.IsDBNull(row["StationName"]) ? row["StationName"].ToString() : "",
                            PlcIp = !Convert.IsDBNull(row["PlcIp"]) ? row["PlcIp"].ToString() : "",
                            PlcPort = !Convert.IsDBNull(row["PlcPort"]) ? row["PlcPort"].ToString() : "",
                            IsEnabled = !Convert.IsDBNull(row["EnableStatus"]) ? row["EnableStatus"].ToString() : "",
                            CreateTime = !Convert.IsDBNull(row["CreateDatetime"]) ? Convert.ToDateTime(row["CreateDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") : ""
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return modelsWorkStation;
        }
        //初始化工站数据
        private void LoadStations()
        {
            wpExecutionStation.Children.Clear();
            var modelsWorkStation = GetWorkStations();
            foreach (WorkStationItem stationName in modelsWorkStation)
            {
                CheckBox chk = new CheckBox
                {
                    Content = stationName.Name,
                    Name = stationName.Code,
                    Width = 80,
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center,
                    IsChecked = true
                };
                wpExecutionStation.Children.Add(chk);
            }
        }
        private void cmbModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 获取选中项
            var selectedItem = cmbModel.SelectedItem;

            if (selectedItem != null)
            {
                // 获取选中项的名称（因为设置了 DisplayMemberPath="Name"）
                string selectedName = selectedItem.GetType().GetProperty("Name")?.GetValue(selectedItem)?.ToString();
                if (!string.IsNullOrEmpty(selectedName))
                {
                    switch (selectedName)
                    {
                        case "机械臂右":
                            txtProductName.Text = "机臂展开电动锁止机构1";
                            break;
                        case "机械臂左":
                            txtProductName.Text = "机臂展开电动锁止机构2";
                            break;
                        case "起落架右":
                            txtProductName.Text = "起落架收放电动锁止机构右件";
                            break;
                        case "起落架左":
                            txtProductName.Text = "起落架收放电动锁止机构左件";
                            break;
                        default:
                            break;
                    }
                    for (int i = 0; i < cmbProcessRoute.Items.Count; i++)
                    {
                        var item = cmbProcessRoute.Items[i];
                        string name = item.GetType().GetProperty("Name")?.GetValue(item)?.ToString();
                        if (name == selectedName)
                        {
                            cmbProcessRoute.SelectedIndex = i;
                            break;
                        }
                    }
                    for (int i = 0; i < txtFormulaName.Items.Count; i++)
                    {
                        var item = txtFormulaName.Items[i];
                        string name = item.GetType().GetProperty("Name")?.GetValue(item)?.ToString();
                        if (name == selectedName)
                        {
                            txtFormulaName.SelectedIndex = i;
                            break;
                        }
                    }
                    for (int i = 0; i < cmbProcessRoute.Items.Count; i++)
                    {
                        var item = cmbProcessRoute.Items[i];
                        string name = item.GetType().GetProperty("Name")?.GetValue(item)?.ToString();
                        if (name == selectedName)
                        {
                            cmbProcessRoute.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }
        //初始化 型号 工艺路线 执行工站 数据 
        private void LoadModelItems()
        {
            DataTable dt = null;
            using (var helper = new SqlConnectionHelper())
            {
                string sqlSelect = "SELECT * FROM OrderModelSetting where EnableStatus='T' order by CreateDatetime desc";
                dt = helper.ExecuteDataTable(sqlSelect);
            }
            var models = new List<ModelItem>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    models.Add(new ModelItem
                    {
                        Code = !Convert.IsDBNull(row["ModelValue"]) ? row["ModelValue"].ToString() : "",
                        Name = !Convert.IsDBNull(row["ModelText"]) ? row["ModelText"].ToString() : "",
                        IsEnabled = !Convert.IsDBNull(row["EnableStatus"]) ? row["EnableStatus"].ToString() : "",
                        CreateTime = !Convert.IsDBNull(row["CreateDatetime"]) ? Convert.ToDateTime(row["CreateDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    });
                }
            }
            cmbModel.ItemsSource = models;//型号设置

            DataTable dtRouting = null;
            using (var helper = new SqlConnectionHelper())
            {
                string sqlSelect = "SELECT * FROM OrderRouting where EnableStatus='T' order by CreateDatetime desc";
                dtRouting = helper.ExecuteDataTable(sqlSelect);
            }
            var modelsRouting = new List<ModelItem>();
            if (dtRouting != null && dtRouting.Rows.Count > 0)
            {
                foreach (DataRow row in dtRouting.Rows)
                {
                    modelsRouting.Add(new ModelItem
                    {
                        Code = !Convert.IsDBNull(row["RoutingValue"]) ? row["RoutingValue"].ToString() : "",
                        Name = !Convert.IsDBNull(row["RoutingText"]) ? row["RoutingText"].ToString() : "",
                        IsEnabled = !Convert.IsDBNull(row["EnableStatus"]) ? row["EnableStatus"].ToString() : "",
                        CreateTime = !Convert.IsDBNull(row["CreateDatetime"]) ? Convert.ToDateTime(row["CreateDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    });
                }
            }
            cmbProcessRoute.ItemsSource = modelsRouting;//工艺路线设置

            DataTable dtWorkTeam = null;
            using (var helper = new SqlConnectionHelper())
            {
                string sqlSelect = "SELECT * FROM WorkTeam where EnableStatus='T' order by CreateDatetime desc";
                dtWorkTeam = helper.ExecuteDataTable(sqlSelect);
            }
            var modelsWorkTeam = new List<ModelItem>();
            if (dtWorkTeam != null && dtWorkTeam.Rows.Count > 0)
            {
                foreach (DataRow row in dtWorkTeam.Rows)
                {
                    modelsWorkTeam.Add(new ModelItem
                    {
                        Code = !Convert.IsDBNull(row["ItemValue"]) ? row["ItemValue"].ToString() : "",
                        Name = !Convert.IsDBNull(row["ItemText"]) ? row["ItemText"].ToString() : "",
                        IsEnabled = !Convert.IsDBNull(row["EnableStatus"]) ? row["EnableStatus"].ToString() : "",
                        CreateTime = !Convert.IsDBNull(row["CreateDatetime"]) ? Convert.ToDateTime(row["CreateDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    });
                }
            }
            cmbProductionTeam.ItemsSource = modelsWorkTeam;//生产班组设置

            DataTable dtFormula = null;
            using (var helper = new SqlConnectionHelper())
            {
                string sqlSelect = "SELECT * FROM Formula where EnableStatus='T' order by CreateDatetime desc";
                dtFormula = helper.ExecuteDataTable(sqlSelect);
            }
            var modelsFormula = new List<ModelItem>();
            if (dtFormula != null && dtFormula.Rows.Count > 0)
            {
                foreach (DataRow row in dtFormula.Rows)
                {
                    modelsFormula.Add(new ModelItem
                    {
                        Code = !Convert.IsDBNull(row["ItemValue"]) ? row["ItemValue"].ToString() : "",
                        Name = !Convert.IsDBNull(row["ItemText"]) ? row["ItemText"].ToString() : "",
                        IsEnabled = !Convert.IsDBNull(row["EnableStatus"]) ? row["EnableStatus"].ToString() : "",
                        CreateTime = !Convert.IsDBNull(row["CreateDatetime"]) ? Convert.ToDateTime(row["CreateDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    });
                }
            }
            txtFormulaName.ItemsSource = modelsFormula;//配方设置
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // 只允许输入数字
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void WorkOrderPriorityComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!e.Handled && sender is ComboBox comboBox)
            {
                //WorkOrderViewModel? workOrderViewModel = this.DataContext as WorkOrderViewModel;
                ModelItem selectedItem = comboBox.SelectedItem as ModelItem;
            }

            e.Handled = true;
        }

        //通过ID查询工单
        private WorkOrderModel GetWorkOrderModel(int Id)
        {
            List<WorkOrderModel> dgWorkOrders =GlobalBaseData.sGlobalBaseData.GetWorkOrders(Id);
            if (dgWorkOrders != null && dgWorkOrders.Count > 0) 
            {
                return dgWorkOrders[0];
            }
            else
            {
                return null;
            }
        }
        private void LoadMockData(string currentStatus,string orderId,string productName)
        {
            _allWorkOrders.Clear();
            DataTable dtWorkOrder = null;
            using (var helper = new SqlConnectionHelper())
            {
                string _currentStatus = "";
                string _orderId = "";
                string _productName = "";
                if (!string.IsNullOrEmpty(currentStatus))
                {
                    _currentStatus = " and EnableStatus='"+ currentStatus + "'  ";
                }
                if (!string.IsNullOrEmpty(orderId))
                {
                    _orderId = " and OrderId like '%" + orderId + "%'";
                }
                if (!string.IsNullOrEmpty(productName))
                {
                    _productName = " and ProductName like '%" + productName + "%'";
                }
                string sqlSelect = "SELECT * FROM WorkOrderNew where Deleted='F' " + _currentStatus + _orderId+ _productName +
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
                        RowNumber = i+1,
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
         
        // 关闭按钮
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        // 清除数据
        private void ClearData_Click(object sender, RoutedEventArgs e)
        {
            txtWorkOrderNo.Text = "";
            cmbModel.SelectedIndex = -1;
            txtIssueSerialNo.Text = "";
            cmbProcessRoute.SelectedIndex = -1;
            txtProductName.Text = "";
            txtOrderQuantity.Text = "";
            txtOfflineSerialNo.Text = "";
            txtReworkCode.Text = "";
            txtFormulaName.Text = "";
            cmbProductionTeam.SelectedIndex = -1;
            txtRemark.Text = "";

            ClearSelectedStations();
        }
        // 执行工站默认多选
        private void ClearSelectedStations()
        {
            foreach (var item in wpExecutionStation.Children)
            {
                CheckBox station = item as CheckBox;
                station.IsChecked = true;
            }
        }
        private void ReseachSettingData_Click(object sender, RoutedEventArgs e)
        {
            LoadData();

            
        }
        private bool GetPrintSettingData(PrintData printData)
        {
            try
            {
                string pt = "";
                switch (printData.PrintType)
                {
                    case "机械臂左":
                        pt = "11DS001AA";
                        break;
                    case "机械臂右":
                        pt = "11DS001BA";
                        break;
                    case "起落架左":
                        pt = "11DS002AA";
                        break;
                    case "起落架右":
                        pt = "11DS002BA";
                        break;
                    default:
                        break;
                }
                printData.PrintType = pt;
                string dateStr = DateTime.Now.ToString("yyyyMMdd");
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM TimeCount where DateString='" + dateStr + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    int.TryParse(row["StartIndex"].ToString(), out int iddS);
                    int.TryParse(row["EndIndex"].ToString(), out int iddE);
                    printData.DateString = dateStr;
                    printData.StartIndex = iddE + 1;
                    printData.EndIndex = iddE + printData.PrintCount;
                    string updateSql = "update TimeCount set StartIndex=" + printData.StartIndex + ",EndIndex=" + printData.EndIndex + " where DateString='" + dateStr + "'";
                    GlobalSqlExecute.sGlobalSqlExecute.AddExecSql(new SqlItemObj() { Sql = updateSql });
                }
                else
                {
                    //如果没有数据就创建新的
                    printData.DateString = dateStr;
                    printData.StartIndex = 1;
                    printData.EndIndex = printData.PrintCount;
                    string insertSql = "insert into TimeCount(StartIndex,EndIndex,DateString,CreateTime)" +
                        "values(1," + printData.PrintCount + ",'" + dateStr + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                    GlobalSqlExecute.sGlobalSqlExecute.AddExecSql(new SqlItemObj() { Sql = insertSql });
                }
                if (printData.PrintResultCode == null)
                {
                    printData.PrintResultCode = new List<string>();
                }
                for (int i = 0; i < printData.PrintCount; i++)
                {
                    string resultSS = (printData.StartIndex + i).ToString("D4");
                    string code = printData.PrintType + dateStr + resultSS;
                    printData.PrintResultCode.Add(code);
                }
                string message = printData.StartIndex + "," + printData.PrintCount + "," + printData.PrintType + dateStr;
                return SendData(message);
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool SendData(string message)
        {
            string serverIp = "192.168.0.183";
            int port = 15001;
            try
            {
                // 连接服务端
                TcpClient client = new TcpClient(serverIp, port);
                NetworkStream stream = client.GetStream();
                // 发送消息
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // ========== 新增：接收服务端的确认消息 ==========
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string ackMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // 关闭连接
                stream.Close();
                client.Close();
                return true;
            }
            catch (SocketException ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Information, Content="连接打印机错误，"+ex.Message});
                return false;
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Information, Content = "连接打印机错误，出现异常" + ex.Message });
                return false;
            }
        }
        // 保存数据
        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(txtWorkOrderNo.Text))
            {
                MessageBox.Show("请输入工单号！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtOrderQuantity.Text))
            {
                MessageBox.Show("请输入工单数量！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(txtOrderQuantity.Text,out int result))
            {
                MessageBox.Show("工单数量请输入整数！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cmbModel.SelectedItem == null)
            {
                MessageBox.Show("请选择型号！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var bindingExpression = txtStartTime.GetBindingExpression(TextBox.TextProperty);
            bindingExpression?.UpdateSource();

            ModelItem model = cmbModel.SelectedItem as ModelItem;//选中型号
            ModelItem routing = null;//选择工艺
            if (cmbProcessRoute.SelectedItem != null)
            {
                routing = cmbProcessRoute.SelectedItem as ModelItem;
            }
            ModelItem formula = null;//选择配方
            if (txtFormulaName.SelectedItem != null)
            {
                formula = txtFormulaName.SelectedItem as ModelItem;
            }
            ModelItem workTeam = null;//选择班组
            if (cmbProductionTeam.SelectedItem != null)
            {
                workTeam = cmbProductionTeam.SelectedItem as ModelItem;
            }

            UserParameters userCurrent = GlobalProperty.sGlobalProperty.GetCurrUser();
            if (string.IsNullOrEmpty(txtStartTime.Text) 
                || !DateTime.TryParse((dpStartDate.SelectedDate ?? DateTime.Now).ToString("yyyy-MM-dd ") + txtStartTime.Text,out DateTime res))
            {
                MessageBox.Show("订单开始时间 格式不对，请确认！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txtStartTime.Text) 
                || !DateTime.TryParse((dpPlanCompleteDate.SelectedDate ?? DateTime.Now).ToString("yyyy-MM-dd ") + txtPlanCompleteTime.Text, out DateTime resComp))
            {
                MessageBox.Show("计划完成时间 格式不对，请确认！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // 创建新工单
            var newWorkOrder = new WorkOrderModel
            {
                OrderId = txtWorkOrderNo.Text,
                ModelValue = model.Code,
                ModelText = model.Name,
                FlowOrderId = txtIssueSerialNo.Text,
                RoutingValue = routing == null ? "" : routing.Code,
                RoutingText = routing == null ? "" : routing.Name,
                ProductName = txtProductName.Text,
                OrderAmount = int.TryParse(txtOrderQuantity.Text, out int qty) ? qty : 0,
                OffLineFlowId = txtOfflineSerialNo.Text,
                WorkBackNo = txtReworkCode.Text,
                FormulaValue = formula == null ? "" : formula.Code,
                FormulaText = formula == null ? "" : formula.Name,
                WorkTeamValue = workTeam == null ? "" : workTeam.Code,
                WorkTeamText = workTeam == null ? "" : workTeam.Name,
                Remark = txtRemark.Text,
                OrderStartDatetime = (dpStartDate.SelectedDate ?? DateTime.Now).ToString("yyyy-MM-dd ") + txtStartTime.Text,
                PlanedCompleteDatetime = (dpPlanCompleteDate.SelectedDate ?? DateTime.Now).ToString("yyyy-MM-dd ") + txtPlanCompleteTime.Text,
                CreaterAcc = userCurrent.UserAccount,
                CreaterName = userCurrent.UserName,
            };
            var modelsWorkStation = GetWorkStations();
            List<int> stationId = new List<int>();
            foreach (UIElement element in wpExecutionStation.Children)
            {
                if (element is CheckBox chk && chk.IsChecked == true)
                {
                    var s = modelsWorkStation.FindAll(a => a.Code == chk.Name).FirstOrDefault();
                    if (s != null) 
                    {
                        stationId.Add(s.Id);
                    }
                }
            }
            if (stationId.Count == 0)
            {
                MessageBox.Show("未选择执行工站，请确认！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Guid newGuid = Guid.NewGuid();
            string uuidString = newGuid.ToString().Replace("-","");
            using (var helper = new SqlConnectionHelper())
            {
                try
                {
                    helper.BeginTransaction();

                    string sql = "insert into WorkOrderNew(OrderId,ModelValue,ModelText,CreaterAcc,CreaterName,CreateDatetime," +
                        "RoutingValue,RoutingText,EnableStatus,FlowOrderId,ProductName,OrderAmount,OffLineFlowId,WorkBackNo,FormulaValue," +
                        "FormulaText,WorkTeamValue,WorkTeamText,Remark,OrderStartDatetime,PlanedCompleteDatetime,WorkOrderUUID,Deleted)" +
                            "values(@OrderId,@ModelValue,@ModelText,@CreaterAcc,@CreaterName,@CreateDatetime," +
                            "@RoutingValue,@RoutingText,'未下发',@FlowOrderId,@ProductName,@OrderAmount,@OffLineFlowId,@WorkBackNo,@FormulaValue," +
                            "@FormulaText,@WorkTeamValue,@WorkTeamText,@Remark,@OrderStartDatetime,@PlanedCompleteDatetime,@WorkOrderUUID,'F')";
                    var parametersInsert = new[]
                        {
                                SqlConnectionHelper.CreateParameter("@OrderId", newWorkOrder.OrderId),
                                SqlConnectionHelper.CreateParameter("@ModelValue", newWorkOrder.ModelValue),
                                SqlConnectionHelper.CreateParameter("@ModelText", newWorkOrder.ModelText),
                                SqlConnectionHelper.CreateParameter("@RoutingValue", newWorkOrder.RoutingValue),
                                SqlConnectionHelper.CreateParameter("@RoutingText", newWorkOrder.RoutingText),
                                SqlConnectionHelper.CreateParameter("@FlowOrderId", newWorkOrder.FlowOrderId),
                                SqlConnectionHelper.CreateParameter("@ProductName", newWorkOrder.ProductName),
                                SqlConnectionHelper.CreateParameter("@OrderAmount", newWorkOrder.OrderAmount),
                                SqlConnectionHelper.CreateParameter("@OffLineFlowId", newWorkOrder.OffLineFlowId),
                                SqlConnectionHelper.CreateParameter("@WorkBackNo", newWorkOrder.WorkBackNo),
                                SqlConnectionHelper.CreateParameter("@FormulaValue", newWorkOrder.FormulaValue),
                                SqlConnectionHelper.CreateParameter("@FormulaText", newWorkOrder.FormulaText),
                                SqlConnectionHelper.CreateParameter("@WorkTeamValue", newWorkOrder.WorkTeamValue),
                                SqlConnectionHelper.CreateParameter("@WorkTeamText", newWorkOrder.WorkTeamText),
                                SqlConnectionHelper.CreateParameter("@Remark", newWorkOrder.Remark),
                                SqlConnectionHelper.CreateParameter("@OrderStartDatetime", newWorkOrder.OrderStartDatetime),
                                SqlConnectionHelper.CreateParameter("@PlanedCompleteDatetime", newWorkOrder.PlanedCompleteDatetime),
                                SqlConnectionHelper.CreateParameter("@CreaterAcc", userCurrent.UserAccount),
                                SqlConnectionHelper.CreateParameter("@CreaterName", userCurrent.UserName),
                                SqlConnectionHelper.CreateParameter("@WorkOrderUUID", uuidString),
                                SqlConnectionHelper.CreateParameter("@CreateDatetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            };
                    helper.ExecuteNonQuery(sql, parametersInsert);

                    if (stationId != null && stationId.Count > 0)
                    {
                        string sqlStation = "insert into WorkOrderStation(WorkOrderUUID,StationId)" +
                            "values";
                        for (int i = 0; i < stationId.Count; i++)
                        {
                            sqlStation += "('"+ uuidString + "'," + stationId[i]+")";
                            if (i != (stationId.Count-1))
                            {
                                sqlStation += ",";
                            }
                        }
                        helper.ExecuteNonQuery(sqlStation);
                    }

                    helper.CommitTransaction();
                    MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearData_Click(sender, e);
                    LoadData();
                }
                catch (Exception ex)
                {
                    helper.RollbackTransaction();
                    MessageBox.Show("保存失败！异常信息:" + ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 查询
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string status = "";
            string orderId = "";
            string productName = "";
            if (cmbStatus.SelectedItem is ComboBoxItem selectedItem)
            {
                status = selectedItem.Content.ToString();
            }
            if (status == "全部")
            {
                status="";
            }
            if (!string.IsNullOrEmpty(txtWorkOrderNo_Search.Text))
            {
                orderId = txtWorkOrderNo_Search.Text;
            }
            if (!string.IsNullOrEmpty(txtProductName_Search.Text))
            {
                productName = txtProductName_Search.Text;
            }
            LoadMockData(status,orderId,productName);
        }

        // 选中行变更
        private void WorkOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedWorkOrder = dgWorkOrders.SelectedItem as WorkOrderModel;
            bool hasSelection = _selectedWorkOrder != null;

            btnIssue.IsEnabled = hasSelection;
            //btnModify.IsEnabled = hasSelection;
            btnDelete.IsEnabled = hasSelection;
        }

        // 下发工单
        private void IssueWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWorkOrder != null )
            {
                var result = MessageBox.Show($"确定要下发工单 {_selectedWorkOrder.OrderId} 吗？",
                    "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    UserParameters userCurrent = GlobalProperty.sGlobalProperty.GetCurrUser();
                    WorkOrderModel orderModel = GetWorkOrderModel(_selectedWorkOrder.Id);
                    if (orderModel.EnableStatus == "未下发")
                    {
                        string startWorkTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        using (var helper = new SqlConnectionHelper())
                        {
                            try
                            {
                                helper.BeginTransaction();

                                string sql = "update WorkOrderNew set EnableStatus=@EnableStatus" +
                                        " where ID=@Id";
                                var parametersInsert = new[]
                                    {
                                        SqlConnectionHelper.CreateParameter("@EnableStatus", "已下发"),
                                        SqlConnectionHelper.CreateParameter("@Id",orderModel.Id)
                                    };
                                helper.ExecuteNonQuery(sql, parametersInsert);

                                string sqlSelectStation = @"SELECT DISTINCT ws.* 
                                        FROM WorkStation ws
                                        INNER JOIN WorkOrderStation wos ON ws.id = wos.StationId
                                        INNER JOIN WorkOrderNew won ON wos.WorkOrderUUID = won.WorkOrderUUID
                                        WHERE won.id = "+ orderModel.Id;
                                DataTable dt = helper.ExecuteDataTable(sqlSelectStation);

                                List<StatusItem> statusItems = new List<StatusItem>();
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    string insertOrderStation = "insert into WorkOrderMissionFlow(OrderID,StationID,CurrStationStatus," +
                                        "StationExecuteSort,CreateDatetime,CreaterAcc,CreaterName)values";
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        DataRow row = dt.Rows[i];
                                        int ses = (!Convert.IsDBNull(row["ExecutionOrder"]) ? int.Parse(row["ExecutionOrder"].ToString()) : 0);
                                        insertOrderStation += "(" +
                                            orderModel.Id + "," +
                                            (!Convert.IsDBNull(row["ID"]) ? int.Parse(row["ID"].ToString()) : 0) + "," +
                                            "'None'," +
                                             ses + "," +
                                            "'"+ startWorkTime + "'," +
                                            "'"+ userCurrent.UserAccount + "'," +
                                            "'"+ userCurrent.UserName + "'" +
                                            ")";
                                        if (i != (dt.Rows.Count-1))
                                        {
                                            insertOrderStation += ",";
                                        }

                                        statusItems.Add(new StatusItem() { Color = "#95A5A6", Value = "" + ses });//半路插进来的 默认 未开始 
                                    }
                                    helper.ExecuteNonQuery(insertOrderStation);
                                }

                                PrintData printData = new PrintData();
                                printData.PrintCount = orderModel.OrderAmount;
                                printData.PrintType = orderModel.FormulaText;
                                if (GetPrintSettingData(printData))//GetPrintSettingData(printData)
                                {
                                    if (printData.PrintResultCode != null && printData.PrintResultCode.Count > 0)
                                    {
                                        string inputCode = "insert into WorkOrderNewProductCode(WorkOrderId,ProcessCode) values ";
                                        for (int i = 0; i < printData.PrintResultCode.Count; i++) 
                                        {
                                            inputCode += "("+ orderModel.Id + ",'"+ printData.PrintResultCode[i] + "'" +
                                                ")";
                                            if ((printData.PrintResultCode.Count-1) != i)
                                            {
                                                inputCode += ",";
                                            }
                                        }
                                        helper.ExecuteNonQuery(inputCode);
                                    }

                                    helper.CommitTransaction();
                                    MessageBox.Show("下发成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                    GlobalProperty.sGlobalProperty.AddMissionList(
                                        new NormalCard
                                        {
                                            Title = "工单号：" + orderModel.OrderId,
                                            Subtitle = startWorkTime,
                                            Color = "#3498DB",
                                            StationCount = orderModel.WorkOrderMissionStation != null ? orderModel.WorkOrderMissionStation.Count + "" : "0",
                                            StatusList = statusItems,
                                            WorkingTime = startWorkTime,
                                            MissionType = "Visible",
                                            StationType = "Collapsed",
                                            CardType = RightCardType.NormalCard,
                                            WorkOrderId = orderModel.Id
                                        },
                                        _mainForm
                                        );
                                    Search_Click(sender, e);
                                }
                                else
                                {
                                    helper.RollbackTransaction();
                                    MessageBox.Show("标签打印失败，请重试");
                                }
                            }
                            catch (Exception ex)
                            {
                                helper.RollbackTransaction();
                                MessageBox.Show("保存失败！异常信息:" + ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("当前工单不是 [未下发] 状态！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        // 修改工单
        private void ModifyWorkOrder_Click(object sender, RoutedEventArgs e)
        {
        }

        // 删除工单
        private void DeleteWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWorkOrder != null)
            {
                var result = MessageBox.Show($"确定要删除工单 {_selectedWorkOrder.OrderId} 吗？",
                    "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    WorkOrderModel orderModel = GetWorkOrderModel(_selectedWorkOrder.Id);
                    if (orderModel != null) 
                    {
                        if (orderModel.EnableStatus == "未下发")
                        {
                            using (var helper = new SqlConnectionHelper())
                            {
                                try
                                {
                                    string sql = "update WorkOrderNew set Deleted='T' where ID=@ID";
                                    var parametersInsert = new[]
                                        {
                                            SqlConnectionHelper.CreateParameter("@ID", _selectedWorkOrder.Id)
                                        };
                                    int resInt = helper.ExecuteNonQuery(sql, parametersInsert);
                                    if (resInt > 0)
                                    {
                                        MessageBox.Show("删除成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                        Search_Click(sender,e);
                                    }
                                    else
                                    {
                                        MessageBox.Show("删除失败！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("操作异常，请重试！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("工单不是 [未下发]，不允许删除！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("工单不存在，无法操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
    }
    public class WorkOrderLinkStaion
    {
        public int Id { get; set; }
        public int OrderID { get; set; }
        public int StationID { get; set; }
        public string CurrStationStatus { get; set; }
        public int StationExecuteSort { get; set; }
        public string CreateDatetime { get; set; }
        public string StationCode { get; set; }
        public string StationName { get; set; }
    }
    // 工单数据模型
    public class WorkOrderModel
    {
        public int RowNumber { get; set; }
        public int Id { get; set; }
        /// <summary>
        /// 工单号
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 型号值
        /// </summary>
        public string ModelValue { get; set; }
        /// <summary>
        /// 型号名称
        /// </summary>
        public string ModelText { get; set; }
        /// <summary>
        /// 下发流水号
        /// </summary>
        public string FlowOrderId { get; set; }
        /// <summary>
        /// 工艺路线值
        /// </summary>
        public string RoutingValue { get; set; }
        /// <summary>
        /// 工艺路线名称
        /// </summary>
        public string RoutingText { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 订单数量
        /// </summary>
        public int OrderAmount { get; set; }
        /// <summary>
        /// 下线流水号
        /// </summary>
        public string OffLineFlowId { get; set; }
        /// <summary>
        /// 返工码
        /// </summary>
        public string WorkBackNo { get; set; }
        /// <summary>
        /// 配方值
        /// </summary>
        public string FormulaValue { get; set; }
        /// <summary>
        /// 配方名称
        /// </summary>
        public string FormulaText { get; set; }
        /// <summary>
        /// 班组值
        /// </summary>
        public string WorkTeamValue { get; set; }
        /// <summary>
        /// 班组名称
        /// </summary>
        public string WorkTeamText { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 写入时间
        /// </summary>
        public string CreateDatetime { get; set; }
        /// <summary>
        /// 订单开始时间
        /// </summary>
        public string OrderStartDatetime { get; set; }
        /// <summary>
        /// 计划完成时间
        /// </summary>
        public string PlanedCompleteDatetime { get; set; }
        /// <summary>
        /// 创建人账号
        /// </summary>
        public string CreaterAcc { get; set; }
        /// <summary>
        /// 创建人姓名
        /// </summary>
        public string CreaterName { get; set; }
        /// <summary>
        /// 工单状态
        /// </summary>
        public string EnableStatus { get; set; }
        /// <summary>
        /// 工单uuid
        /// </summary>
        public string WorkOrderUUID { get; set; }
        /// <summary>
        /// 工单任务对应的工站
        /// </summary>
        public List<WorkOrderLinkStaion> WorkOrderMissionStation { get; set; }
    }
    public class PrintData
    {
        public string DateString { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int PrintCount { get; set; }
        public string PrintType { get; set; }
        public List<string> PrintResultCode { get; set; }
    }
   
}