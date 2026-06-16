using MissionBossAot.Common;
using MissionBossAot.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
using System.Xml.Linq;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace MissionBossAot.Views
{
    /// <summary>
    /// OrderModelSetting.xaml 的交互逻辑
    /// </summary>
    public partial class OrderModelSetting : Window
    {
        public OrderModelSetting()
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
        }
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            ModelItem selectedItem = dgModels.SelectedItem as ModelItem;
            if (selectedItem != null)
            {
                txtCode.Clear();
                txtCode.IsEnabled = false;
                txtName.Clear();
                txtCode.Text = selectedItem.Code;
                txtName.Text = selectedItem.Name;
                bool b = selectedItem.IsEnabled == "是" ?true:false;
                chkIsEnabled.IsChecked = b;
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
            string enable = (bool)chkIsEnabled.IsChecked ? "T" : "F";
            if (txtCode.IsEnabled)
            {
                using (var helper = new SqlConnectionHelper())
                {
                    string sqlSelect = "SELECT * FROM OrderModelSetting WHERE ModelValue = @ModelValue";
                    var parameters = new[]
                    {
                        SqlConnectionHelper.CreateParameter("@ModelValue", code)
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

            if (txtCode.IsEnabled)
            {
                string successRes = "未提交保存";
                using (var helper = new SqlConnectionHelper())
                {
                    try
                    {
                        string sql = "insert into OrderModelSetting(ModelValue,ModelText,EnableStatus,CreaterAcc,CreaterName,CreateDatetime)" +
                                "values(@ModelValue,@ModelText,@EnableStatus,@CreaterAcc,@CreaterName,@CreateDatetime)";
                        var parametersInsert = new[]
                            {
                                SqlConnectionHelper.CreateParameter("@ModelValue", code),
                                SqlConnectionHelper.CreateParameter("@ModelText", name),
                                SqlConnectionHelper.CreateParameter("@EnableStatus", enable),
                                SqlConnectionHelper.CreateParameter("@CreaterAcc", userCurrent.UserAccount),
                                SqlConnectionHelper.CreateParameter("@CreaterName", userCurrent.UserName),
                                SqlConnectionHelper.CreateParameter("@CreateDatetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                            };
                        int resInt = helper.ExecuteNonQuery(sql, parametersInsert);
                        if (resInt > 0)
                        {
                            successRes = "保存成功";
                            txtCode.Clear();
                            txtCode.IsEnabled = true;
                            txtName.Clear();
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
                        string sql = "update OrderModelSetting set ModelText=@ModelText,EnableStatus=@EnableStatus," +
                            "UpdaterAcc=@UpdaterAcc,UpdaterName=@UpdaterName,UpdateDatetime=@UpdateDatetime" +
                            " where ModelValue=@ModelValue";
                        var parametersInsert = new[]
                            {
                                SqlConnectionHelper.CreateParameter("@ModelValue", code),
                                SqlConnectionHelper.CreateParameter("@ModelText", name),
                                SqlConnectionHelper.CreateParameter("@EnableStatus", enable),
                                SqlConnectionHelper.CreateParameter("@UpdaterAcc", userCurrent.UserAccount),
                                SqlConnectionHelper.CreateParameter("@UpdaterName", userCurrent.UserName),
                                SqlConnectionHelper.CreateParameter("@UpdateDatetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            };
                        int resInt = helper.ExecuteNonQuery(sql, parametersInsert);
                        if (resInt > 0)
                        {
                            successRes = "保存成功";
                            txtCode.Clear();
                            txtCode.IsEnabled = true;
                            txtName.Clear();
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
                string sqlSelect = "SELECT * FROM OrderModelSetting order by EnableStatus desc,CreateDatetime desc";
                dt = helper.ExecuteDataTable(sqlSelect);
            }
            var models = new List<ModelItem>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    models.Add(new ModelItem
                    {
                        Code = !Convert.IsDBNull(row["ModelValue"]) ? row["ModelValue"].ToString():"",
                        Name = !Convert.IsDBNull(row["ModelText"]) ? row["ModelText"].ToString() : "",
                        IsEnabled = !Convert.IsDBNull(row["EnableStatus"]) ? (row["EnableStatus"].ToString() == "T" ? "是" : "否") : "",
                        CreateTime = !Convert.IsDBNull(row["CreateDatetime"]) ? Convert.ToDateTime(row["CreateDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") :""
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
        }
        private void DgModels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //_selectedModel = dgModels.SelectedItem as ModelInfo;
            //if (_selectedModel != null)
            //{
            //    txtCode.Text = _selectedModel.Code;
            //    txtName.Text = _selectedModel.Name;
            //    chkIsEnabled.IsChecked = _selectedModel.IsEnabled;
            //}
        }
    }
    public class ModelItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string IsEnabled { get; set; }
        public string CreateTime { get; set; }
    }
}
