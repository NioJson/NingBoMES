using MissionBossAot.Common;
using MissionBossAot.Models;
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
    /// StationData.xaml 的交互逻辑
    /// </summary>
    public partial class StationData : Window
    {
        public StationData()
        {
            InitializeComponent();
            InitializeDateTimePickers();
            LoadStationData();
        }
        private void InitializeDateTimePickers()
        {
            // 设置默认日期为当天
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today;
        }
        private void LoadStationData()
        {
            var stationList = new[]
            {
                new { StationName = "P10 齿轮、铜套压装" },
                new { StationName = "P20 丝杆部件压装" },
                new { StationName = "P30 活塞杆部件装配" },
                new { StationName = "P40 电机、齿轮箱装配" },
                new { StationName = "P50 传感器装配" },
                new { StationName = "P60 气密性测试" },
                new { StationName = "P70 老化、性能测试" },
                new { StationName = "P70 标定" },
                new { StationName = "P70 推拉力" },
                new { StationName = "P80 噪音测试" }
            };
            StationComboBox.ItemsSource = stationList;
            StationComboBox.SelectedIndex = 0;
        }
        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {        
            try
            {
                // 验证时间输入
                if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("请选择开始和结束日期", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DateTime startTime = StartDatePicker.SelectedDate.Value;
                DateTime endTime = EndDatePicker.SelectedDate.Value;
                if (TimeSpan.TryParse(StartTimeTextBox.Text, out TimeSpan startSpan))
                    startTime = startTime.Date + startSpan;
                if (TimeSpan.TryParse(EndTimeTextBox.Text, out TimeSpan endSpan))
                    endTime = endTime.Date + endSpan;
                // 检查时间范围
                if (startTime > endTime)
                {
                    MessageBox.Show("开始时间不能大于结束时间", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DataTable filteredData = FilterDataFromDatabase(startTime, endTime, StationComboBox.SelectedValue?.ToString());
                DataGrid_DataBaseData.ItemsSource = filteredData.DefaultView;

                foreach (var column in DataGrid_DataBaseData.Columns)
                {
                    var textColumn = column as DataGridTextColumn;
                    if (textColumn != null && textColumn.Binding is System.Windows.Data.Binding binding)
                    {
                        // 假设日期属性名包含 "Date" 或 "Time"
                        if (binding.Path.Path.Contains("Date") ||
                            binding.Path.Path.Contains("Time") ||
                            binding.Path.Path.Contains("记录时间"))
                        {
                            textColumn.Binding.StringFormat = "yyyy-MM-dd HH:mm:ss";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private DataTable FilterDataFromDatabase(DateTime startTime, DateTime endTime, string stationName)
        {
            DataTable result = null;
            switch (stationName)
            {
                case "P10 齿轮、铜套压装":
                    result = GetStationData("P10DB2000ReturnData", startTime, endTime);
                    break;
                case "P20 丝杆部件压装":
                    result = GetStationData("P20DB2000ReturnData", startTime, endTime);
                    break;
                case "P30 活塞杆部件装配":
                    result = GetStationData("P30DB3000ReturnData", startTime, endTime);
                    break;
                case "P40 电机、齿轮箱装配":
                    result = GetStationData("P40DB3001ReturnData", startTime, endTime);
                    break;
                case "P50 传感器装配":
                    result = GetStationData("P50DB3002ReturnData", startTime, endTime);
                    break;
                case "P60 气密性测试":
                    result = GetStationData("P60DB3000ReturnData", startTime, endTime);
                    break;
                case "P70 老化、性能测试":
                    result = GetStationData("P70DB3003ReturnData", startTime, endTime);
                    break;
                case "P70 标定":
                    result = GetStationData("signal_data", startTime, endTime);
                    break;
                case "P70 推拉力":
                    result = GetStationData("signal_data2", startTime, endTime);
                    break;
                case "P80 噪音测试":
                    result = GetStationData("P80DB3004ReturnData", startTime, endTime);
                    break;
                default:
                    break;
            }
            if (result == null)
            {
                result = new DataTable();
            }
            return result;
        }
        private DataTable GetStationData(string tableName, DateTime startTime, DateTime endTime)
        {
            DataTable resDT = null;
            try
            {
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM "+ tableName + " WHERE CreatedTime > @startDt and CreatedTime < @endDt order by CreatedTime desc";
                    var parameters = new[]
                    {
                         SqlConnectionHelper.CreateParameter("@startDt", startTime),
                         SqlConnectionHelper.CreateParameter("@endDt", endTime)
                    };
                    resDT = helper.ExecuteDataTable(sqlSelect, parameters);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询出错，请联系管理员");
                Logger.sLogger.InsertLog(new LoggerObj() { Content = "GetStationData 查询操作失败，具体信息:" + ex.Message });
            }
            if (resDT != null)
            {
                DataExport.SetColumnCaptions(resDT);
            }
            else
            {
                resDT = new DataTable();
            }
            return resDT;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // 重置查询条件
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today;
            StartTimeTextBox.Text = "00:00:00";
            EndTimeTextBox.Text = "23:59:59";
            StationComboBox.SelectedIndex = 0;
            // 重新加载数据 
            QueryButton_Click(sender, e);
        }
        private void StationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void DetailButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var d = DataGrid_DataBaseData.SelectedItem;
                if (d != null)
                {
                    DataRowView dataRow = d as DataRowView;
                    string idStr = dataRow.Row.ItemArray[0].ToString();
                    int.TryParse(idStr,out int objId);
                    var xa = StationComboBox.SelectedValue;
                    switch (xa.ToString())
                    {
                        case "P10 齿轮、铜套压装":
                            string processCode = P10_Action(objId);
                            if (!string.IsNullOrEmpty(processCode))
                            {
                                P10_Action_Detail(processCode);
                            }
                            break;
                        case "P20 丝杆部件压装":
                            string processCodeP20 = P20_Action(objId);
                            if (!string.IsNullOrEmpty(processCodeP20))
                            {
                                P20_Action_Detail(processCodeP20);
                            }
                            break;
                        case "P30 活塞杆部件装配":
                            string processCodeP30 = P30_Action(objId);
                            if (!string.IsNullOrEmpty(processCodeP30))
                            {
                                P30_Action_Detail(processCodeP30);
                            }
                            break;
                        case "P40 电机、齿轮箱装配":
                            string processCodeP40 = P40_Action(objId);
                            if (!string.IsNullOrEmpty(processCodeP40))
                            {
                                P40_Action_Detail(processCodeP40);
                            }
                            break;
                        case "P50 传感器装配":
                            string processCodeP50 = P50_Action(objId);
                            if (!string.IsNullOrEmpty(processCodeP50))
                            {
                                P50_Action_Detail(processCodeP50);
                            }
                            break;
                        case "P60 气密性测试":
                            string processCodeP60 = P60_Action(objId);
                            if (!string.IsNullOrEmpty(processCodeP60))
                            {
                                P60_Action_Detail(processCodeP60);
                            }
                            break;
                        case "P70 老化、性能测试":
                            string processCodeP70Laohua = P70_Action_laohua(objId);
                            if (!string.IsNullOrEmpty(processCodeP70Laohua))
                            {
                                P70_Action_Detail_laohua(processCodeP70Laohua);
                            }
                            break;
                        case "P70 标定":
                            string processCodeP70biaoding = P70_Action_biaoding(objId);
                            if (!string.IsNullOrEmpty(processCodeP70biaoding))
                            {
                                P70_Action_Detail_biaoding(processCodeP70biaoding);
                            }
                            break;
                        case "P70 推拉力":
                            string processCodeP70fuzai = P70_Action_fuzai(objId);
                            if (!string.IsNullOrEmpty(processCodeP70fuzai))
                            {
                                P70_Action_Detail_fuzai(processCodeP70fuzai);
                            }
                            break;
                        case "P80 噪音测试":
                            string processCodeP80 = P80_Action(objId);
                            if (!string.IsNullOrEmpty(processCodeP80))
                            {
                                P80_Action_Detail(processCodeP80);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }
            //Button btn = sender as Button;
            //var dataItem = btn?.Tag;
            //if (dataItem != null)
            //{
            //    WorkOrderModel orderModel = dataItem as WorkOrderModel;
            //    PrintLabelDialog dialog = new PrintLabelDialog(orderModel.Id);
            //    dialog.Owner = this;
            //    dialog.ShowDialog();
            //}
        }
        private void P10_Action_Detail(string code)
        {
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P10DB2000ReturnData_Detail where Product_BarCode='"+ code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataExport.SetColumnCaptions(dt);
                    WorkOrderDataDetail detail = new WorkOrderDataDetail(dt);
                    detail.Owner = this;
                    detail.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }
        private void P20_Action_Detail(string code)
        {
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P20DB2000ReturnData_Detail where Product_BarCode='" + code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataExport.SetColumnCaptions(dt);
                    WorkOrderDataDetail detail = new WorkOrderDataDetail(dt);
                    detail.Owner = this;
                    detail.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }
        private void P30_Action_Detail(string code)
        {
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P30DB3000ReturnData_Detail where Product_BarCode='" + code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataExport.SetColumnCaptions(dt);
                    WorkOrderDataDetail detail = new WorkOrderDataDetail(dt);
                    detail.Owner = this;
                    detail.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }
        private void P40_Action_Detail(string code)
        {
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P40DB3001ReturnData_Detail where Product_BarCode='" + code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataExport.SetColumnCaptions(dt);
                    WorkOrderDataDetail detail = new WorkOrderDataDetail(dt);
                    detail.Owner = this;
                    detail.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }
        private void P50_Action_Detail(string code)
        {
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P50DB3002ReturnData_Detail where Product_BarCode='" + code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataExport.SetColumnCaptions(dt);
                    WorkOrderDataDetail detail = new WorkOrderDataDetail(dt);
                    detail.Owner = this;
                    detail.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }
        private void P60_Action_Detail(string code)
        {
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P60DB3000ReturnData_Detail where Product_BarCode='" + code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataExport.SetColumnCaptions(dt);
                    WorkOrderDataDetail detail = new WorkOrderDataDetail(dt);
                    detail.Owner = this;
                    detail.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }
        private void P70_Action_Detail_laohua(string code)
        {
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P70DB3003ReturnData_Detail where Product_BarCode='" + code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataExport.SetColumnCaptions(dt);
                    WorkOrderDataDetail detail = new WorkOrderDataDetail(dt);
                    detail.Owner = this;
                    detail.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }
        private void P70_Action_Detail_biaoding(string code)
        {
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM signal_data_Detail where Product_BarCode='" + code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataExport.SetColumnCaptions(dt);
                    WorkOrderDataDetail detail = new WorkOrderDataDetail(dt);
                    detail.Owner = this;
                    detail.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }
        private void P70_Action_Detail_fuzai(string code)
        {
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM signal_data2_Detail where Product_BarCode='" + code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataExport.SetColumnCaptions(dt);
                    WorkOrderDataDetail detail = new WorkOrderDataDetail(dt);
                    detail.Owner = this;
                    detail.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }
        private void P80_Action_Detail(string code)
        {
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P80DB3004ReturnData_Detail where Product_BarCode='" + code + "'";
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataExport.SetColumnCaptions(dt);
                    WorkOrderDataDetail detail = new WorkOrderDataDetail(dt);
                    detail.Owner = this;
                    detail.ShowDialog();
                }
            }
            catch (Exception)
            {
            }
        }
        private string P10_Action(int id)
        {
            string processCode = "";
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P10DB2000ReturnData where ID=" + id;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    processCode = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                }
            }
            catch (Exception)
            {
            }
            return processCode;
        }
        private string P20_Action(int id)
        {
            string processCode = "";
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P20DB2000ReturnData where ID=" + id;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    processCode = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                }
            }
            catch (Exception)
            {
            }
            return processCode;
        }
        private string P30_Action(int id)
        {
            string processCode = "";
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P30DB3000ReturnData where ID=" + id;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    processCode = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                }
            }
            catch (Exception)
            {
            }
            return processCode;
        }
        private string P40_Action(int id)
        {
            string processCode = "";
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P40DB3001ReturnData where ID=" + id;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    processCode = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                }
            }
            catch (Exception)
            {
            }
            return processCode;
        }
        private string P50_Action(int id)
        {
            string processCode = "";
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P50DB3002ReturnData where ID=" + id;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    processCode = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                }
            }
            catch (Exception)
            {
            }
            return processCode;
        }
        private string P60_Action(int id)
        {
            string processCode = "";
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P60DB3000ReturnData where ID=" + id;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    processCode = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                }
            }
            catch (Exception)
            {
            }
            return processCode;
        }
        private string P70_Action_laohua(int id)
        {
            string processCode = "";
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P70DB3003ReturnData where ID=" + id;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    processCode = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                }
            }
            catch (Exception)
            {
            }
            return processCode;
        }
        private string P70_Action_biaoding(int id)
        {
            string processCode = "";
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM signal_data where ID=" + id;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    processCode = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                }
            }
            catch (Exception)
            {
            }
            return processCode;
        }
        private string P70_Action_fuzai(int id)
        {
            string processCode = "";
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM signal_data2 where ID=" + id;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    processCode = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                }
            }
            catch (Exception)
            {
            }
            return processCode;
        }
        private string P80_Action(int id)
        {
            string processCode = "";
            try
            {
                DataTable dt = null;
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM P80DB3004ReturnData where ID=" + id;
                    dt = helper.ExecuteDataTable(sqlSelect);
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    processCode = !Convert.IsDBNull(row["Product_BarCode"]) ? row["Product_BarCode"].ToString() : "";
                }
            }
            catch (Exception)
            {
            }
            return processCode;
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
