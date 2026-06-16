using MissionBossAot.Common;
using MissionBossAot.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// WorkStation.xaml 的交互逻辑
    /// </summary>
    public partial class WorkStation : Window
    {
        public WorkStation()
        {
            InitializeComponent();
            LoadDataFromDatabase();
        }
        // 关闭按钮
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            txtCode.Clear();
            txtCode.IsEnabled = true;
            txtName.Clear();
            chkIsEnabled.IsChecked = true;
            txtPlcIp.Clear();
            txtPlcPort.Clear();
            txtExecutionOrder.Clear();
        }
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            WorkStationItem selectedItem = dgModels.SelectedItem as WorkStationItem;
            if (selectedItem != null)
            {
                txtCode.Clear();
                txtCode.IsEnabled = false;
                txtName.Clear();
                txtPlcIp.Clear();
                txtPlcPort.Clear();
                txtExecutionOrder.Clear();

                txtPlcIp.Text = selectedItem.PlcIp;
                txtPlcPort.Text = selectedItem.PlcPort;
                txtCode.Text = selectedItem.Code;
                txtName.Text = selectedItem.Name;
                bool b = selectedItem.IsEnabled == "是" ? true : false;
                chkIsEnabled.IsChecked = b;
                txtExecutionOrder.Text = selectedItem.ExecutionOrder+"";
            }
            else
            {
                MessageResult resultMsgCode = GlobalMessageDialog.Show(
                               "请选择表格里面的数据！",
                               "提示",
                               MessageType.Error,
                               MessageMode.Confirm,
                               3,
                               this);
                return;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            UserParameters userCurrent = GlobalProperty.sGlobalProperty.GetCurrUser();
            if (userCurrent == null || string.IsNullOrEmpty(userCurrent.UserAccount))
            {
                MessageResult resultMsgCode = GlobalMessageDialog.Show(
                               "当前登录账号不存在！",
                               "提示",
                               MessageType.Error,
                               MessageMode.Confirm,
                               3,
                               this);
                return;
            }
            string code = txtCode.Text;
            if (string.IsNullOrEmpty(code))
            {
                MessageResult resultMsgCode = GlobalMessageDialog.Show(
                               "请输入编码！",
                               "提示",
                               MessageType.Error,
                               MessageMode.Confirm,
                               3,
                               this);
                return;
            }
            if (!Regex.IsMatch(code[0].ToString(), @"^[a-zA-Z]$"))
            {
                MessageResult resultMsgCode = GlobalMessageDialog.Show(
                               "编码 的第一个字符必须是 字母！",
                               "提示",
                               MessageType.Error,
                               MessageMode.Confirm,
                               3,
                               this);
                return;
            }
            string name = txtName.Text;
            if (string.IsNullOrEmpty(name))
            {
                MessageResult resultMsgName = GlobalMessageDialog.Show(
                              "请输入名称！",
                              "提示",
                              MessageType.Error,
                              MessageMode.Confirm,
                              3,
                              this);
                return;
            }
            string sort = txtExecutionOrder.Text;
            if (string.IsNullOrEmpty(sort))
            {
                MessageResult resultMsgName = GlobalMessageDialog.Show(
                              "请输入执行顺序！",
                              "提示",
                              MessageType.Error,
                              MessageMode.Confirm,
                              3,
                              this);
                return;
            }
            if(!int.TryParse(sort, out int resi))
            {
                MessageResult resultMsgName = GlobalMessageDialog.Show(
                              "请输入正确的执行顺序（必须是正整数）！",
                              "提示",
                              MessageType.Error,
                              MessageMode.Confirm,
                              3,
                              this);
                return;
            }

            if (!string.IsNullOrEmpty(txtPlcPort.Text))
            {
                if (!int.TryParse(txtPlcPort.Text, out int porti))
                {
                    MessageResult resultMsgName = GlobalMessageDialog.Show(
                                  "请输入正确的端口（必须是正整数）！",
                                  "提示",
                                  MessageType.Error,
                                  MessageMode.Confirm,
                                  3,
                                  this);
                    return;
                }
            }
            

            string enable = (bool)chkIsEnabled.IsChecked ? "T" : "F";
            if (txtCode.IsEnabled)
            {
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM WorkStation WHERE StationCode = @StationCode";
                    var parameters = new[]
                    {
                        SqlConnectionHelper.CreateParameter("@StationCode", code)
                    };

                    DataTable result = helper.ExecuteDataTable(sqlSelect, parameters);
                    if (result != null && result.Rows.Count > 0)
                    {
                        MessageResult resultMsgExists = GlobalMessageDialog.Show(
                                                  "已经存在相同编码",
                                                  "提示",
                                                  MessageType.Error,
                                                  MessageMode.Confirm,
                                                  3,
                                                  this);
                        return;
                    }
                }
            }
            string txtPlcIps = txtPlcIp.Text;
            string txtPlcPorts = txtPlcPort.Text;
            string connectType=(cboPlcConnectionType.SelectedItem as ComboBoxItem).Content.ToString();
            if (txtCode.IsEnabled)
            {
                string successRes = "未提交保存";
                using (var helper = new SqlConnectionHelper())
                {
                    try
                    {
                        string sql = "insert into WorkStation(StationCode,StationName,EnableStatus,CreaterAcc,CreaterName,CreateDatetime," +
                            "PlcIp,PlcPort,ExecutionOrder,PlcConnectType)" +
                                "values(@StationCode,@StationName,@EnableStatus,@CreaterAcc,@CreaterName,@CreateDatetime," +
                                "@PlcIp,@PlcPort,@ExecutionOrder,@PlcConnectType)";
                        var parametersInsert = new[]
                            {
                                SqlConnectionHelper.CreateParameter("@StationCode", code),
                                SqlConnectionHelper.CreateParameter("@StationName", name),
                                SqlConnectionHelper.CreateParameter("@PlcIp", txtPlcIps),
                                SqlConnectionHelper.CreateParameter("@PlcPort", txtPlcPorts),
                                SqlConnectionHelper.CreateParameter("@EnableStatus", enable),
                                SqlConnectionHelper.CreateParameter("@CreaterAcc", userCurrent.UserAccount),
                                SqlConnectionHelper.CreateParameter("@CreaterName", userCurrent.UserName),
                                SqlConnectionHelper.CreateParameter("@ExecutionOrder", sort),
                                SqlConnectionHelper.CreateParameter("@PlcConnectType", connectType),
                                SqlConnectionHelper.CreateParameter("@CreateDatetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            };
                        int resInt = helper.ExecuteNonQuery(sql, parametersInsert);
                        if (resInt > 0)
                        {
                            successRes = "保存成功";
                            txtCode.Clear();
                            txtCode.IsEnabled = true;
                            txtName.Clear();

                            chkIsEnabled.IsChecked = true;
                            txtPlcIp.Clear();
                            txtPlcPort.Clear();
                            txtExecutionOrder.Clear();
                            LoadDataFromDatabase();
                        }
                        else
                        {
                            successRes = "保存失败";
                        }
                    }
                    catch (Exception ex)
                    {
                        successRes = "保存失败";
                    }
                }
                MessageResult resultMsg = GlobalMessageDialog.Show(
                                                  successRes,
                                                  "提示",
                                                  MessageType.Error,
                                                  MessageMode.Confirm,
                                                  3,
                                                  this);
            }
            else
            {
                string successRes = "未提交保存";
                using (var helper = new SqlConnectionHelper())
                {
                    try
                    {
                        string sql = "update WorkStation set StationName=@StationName,EnableStatus=@EnableStatus," +
                            "UpdaterAcc=@UpdaterAcc,UpdaterName=@UpdaterName,UpdateDatetime=@UpdateDatetime,PlcIp=@PlcIp,PlcPort=@PlcPort," +
                            "ExecutionOrder=@ExecutionOrder,PlcConnectType=@PlcConnectType" +
                            " where StationCode=@StationCode";
                        var parametersInsert = new[]
                            {
                                SqlConnectionHelper.CreateParameter("@StationCode", code),
                                SqlConnectionHelper.CreateParameter("@StationName", name),
                                SqlConnectionHelper.CreateParameter("@PlcIp", txtPlcIps),
                                SqlConnectionHelper.CreateParameter("@PlcPort", txtPlcPorts),
                                SqlConnectionHelper.CreateParameter("@EnableStatus", enable),
                                SqlConnectionHelper.CreateParameter("@UpdaterAcc", userCurrent.UserAccount),
                                SqlConnectionHelper.CreateParameter("@UpdaterName", userCurrent.UserName),
                                SqlConnectionHelper.CreateParameter("@ExecutionOrder", sort),
                                SqlConnectionHelper.CreateParameter("@PlcConnectType", connectType),
                                SqlConnectionHelper.CreateParameter("@UpdateDatetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            };
                        int resInt = helper.ExecuteNonQuery(sql, parametersInsert);
                        if (resInt > 0)
                        {
                            successRes = "保存成功";
                            txtCode.Clear();
                            txtCode.IsEnabled = true;
                            txtName.Clear();

                            chkIsEnabled.IsChecked = true;
                            txtPlcIp.Clear();
                            txtPlcPort.Clear();
                            txtExecutionOrder.Clear();
                            LoadDataFromDatabase();
                        }
                        else
                        {
                            successRes = "保存失败";
                        }
                    }
                    catch (Exception ex)
                    {
                        successRes = "保存失败";
                    }
                }
                MessageResult resultMsg = GlobalMessageDialog.Show(
                                                  successRes,
                                                  "提示",
                                                  MessageType.Error,
                                                  MessageMode.Confirm,
                                                  3,
                                                  this);
            }
        }
        private void LoadDataFromDatabase()
        {
            DataTable dt = null;
            using (var helper = new SqlConnectionHelper())
            {
                string sqlSelect = "SELECT * FROM WorkStation order by EnableStatus desc,CreateDatetime desc";
                dt = helper.ExecuteDataTable(sqlSelect);
            }
            var models = new List<WorkStationItem>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    models.Add(new WorkStationItem
                    {
                        ExecutionOrder = !Convert.IsDBNull(row["ExecutionOrder"]) ? int.Parse(row["ExecutionOrder"].ToString()) : 0,
                        Code = !Convert.IsDBNull(row["StationCode"]) ? row["StationCode"].ToString() : "",
                        Name = !Convert.IsDBNull(row["StationName"]) ? row["StationName"].ToString() : "",
                        PlcIp = !Convert.IsDBNull(row["PlcIp"]) ? row["PlcIp"].ToString() : "",
                        PlcPort = !Convert.IsDBNull(row["PlcPort"]) ? row["PlcPort"].ToString() : "",
                        IsEnabled = !Convert.IsDBNull(row["EnableStatus"]) ? (row["EnableStatus"].ToString() == "T" ? "是" : "否") : "",
                        CreateTime = !Convert.IsDBNull(row["CreateDatetime"]) ? Convert.ToDateTime(row["CreateDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    });
                }
            }
            dgModels.ItemsSource = models;
        }
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtCode.Clear();
            txtCode.IsEnabled = true;
            txtName.Clear();
            chkIsEnabled.IsChecked = true;
            txtPlcIp.Clear();
            txtPlcPort.Clear();
            txtExecutionOrder.Clear();
        }
    }
    public class WorkStationItem
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string IsEnabled { get; set; }
        public string CreateTime { get; set; }
        public string PlcIp { get; set; }
        public string PlcPort { get; set; }
        public int ExecutionOrder { get; set; }
        public string ConnectType { get; set; }
    }
}
