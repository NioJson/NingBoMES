using MissionBossAot.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Xml.Linq;

namespace MissionBossAot.Views
{
    /// <summary>
    /// GlueCuringSettingTime.xaml 的交互逻辑
    /// </summary>
    public partial class GlueCuringSettingTime : Window
    {
        public GlueCuringSettingTime()
        {
            InitializeComponent();
            LoadDataFromDatabase();
        }
        private void LoadDataFromDatabase()
        {
            DataTable dt = null;
            using (var helper = new SqlConnectionHelper())
            {
                string sqlSelect = "SELECT * FROM GlueCuringSettingTime";
                dt = helper.ExecuteDataTable(sqlSelect);
            }
            var models = new List<GlueCuringItem>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string minsStr = !Convert.IsDBNull(row["GlueCuringSettingTimeMins"]) ? row["GlueCuringSettingTimeMins"].ToString() : "0";
                    int.TryParse(minsStr,out int mins);
                    models.Add(new GlueCuringItem
                    {
                        GlueCuringSettingTimeMins = mins,
                        CreateDatetime = !Convert.IsDBNull(row["CreateDatetime"]) ? Convert.ToDateTime(row["CreateDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") : ""
                    });
                }
            }
            dgModels.ItemsSource = models;
        }
        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            LoadDataFromDatabase();
        }
         

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string txtMinss = txtMins.Text;
            if (string.IsNullOrEmpty(txtMinss))
            {
                MessageResult resultMsgName = GlobalMessageDialog.Show(
                              "请输入分钟数！",
                              "提示",
                              MessageType.Error,
                              MessageMode.Confirm,
                              3,
                              this);
                return;
            }
            if (!uint.TryParse(txtMinss,out uint mins))
            {
                MessageResult resultMsgName = GlobalMessageDialog.Show(
                             "分钟数 请输入正整数！",
                             "提示",
                             MessageType.Error,
                             MessageMode.Confirm,
                             3,
                             this);
                return;
            }
            using (var helper = new SqlConnectionHelper())
            {
                try
                {
                    helper.BeginTransaction();
                    string sqlDel = "delete from GlueCuringSettingTime";
                    helper.ExecuteNonQuery(sqlDel);

                    string sql = "insert into GlueCuringSettingTime(GlueCuringSettingTimeMins,CreateDatetime)values" +
                        "(@GlueCuringSettingTimeMins,@CreateDatetime)";
                    var parametersInsert = new[]
                        {
                                SqlConnectionHelper.CreateParameter("@GlueCuringSettingTimeMins", txtMinss),
                                SqlConnectionHelper.CreateParameter("@CreateDatetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                            };
                    helper.ExecuteNonQuery(sql, parametersInsert);


                    helper.CommitTransaction();
                    MessageBox.Show("保存成功");
                    LoadDataFromDatabase();
                }
                catch (Exception ex)
                {
                    helper.RollbackTransaction();
                    MessageBox.Show("保存失败");
                }
            }
        }

          
        // 关闭按钮
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
    public class GlueCuringItem
    {
        public int GlueCuringSettingTimeMins { get; set; }
        public string CreateDatetime { get; set; }
    }
}
