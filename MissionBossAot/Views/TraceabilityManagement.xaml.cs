using MissionBossAot.Common;
using MissionBossAot.Models;
using System;
using System.Collections.Generic;
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
    /// TraceabilityManagement.xaml 的交互逻辑
    /// </summary>
    public partial class TraceabilityManagement : Window
    {
        private Window mainForm;
        private bool _isCopyModeActive = false;
        public TraceabilityManagement(Window main)
        {
            InitializeComponent();
            this.mainForm = main;
        }
        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            string txtMinss = FinallyCode.Text;
            if (string.IsNullOrEmpty(txtMinss))
            {
                MessageResult resultMsgName = GlobalMessageDialog.Show(
                              "请输入条码内容！",
                              "提示",
                              MessageType.Error,
                              MessageMode.Confirm,
                              3,
                              this);
                return;
            }
            DataTable dt = null;
            using (var helper = new SqlConnectionHelper())
            {
                string sqlSelect = "SELECT * FROM ProductRecord where ProductCode like '%"+ FinallyCode.Text + "%'";
                dt = helper.ExecuteDataTable(sqlSelect);
            }
            var models = new List<ProductRecordItem>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    models.Add(new ProductRecordItem
                    {
                        ProductCode = !Convert.IsDBNull(row["ProductCode"]) ? row["ProductCode"].ToString() : "",
                        MotorCode = !Convert.IsDBNull(row["MotorCode"]) ? row["MotorCode"].ToString() : "",
                        ProcessCode = !Convert.IsDBNull(row["ProcessCode"]) ? row["ProcessCode"].ToString() : ""
                    });
                }
            }
            DataGrid_DataBaseData.ItemsSource = models;
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<DeviceInfo> devs = GlobalDevice.sGlobalDevice.GetDeviceInfos();
                DeviceInfo d35 = devs.FindAll(a => a.ParentCode == "P10").FirstOrDefault();
                if (d35 != null)
                {
                    P10UploadData data = d35.s7Comm.ReadPlcData_P10();
                    if (data != null)
                    {
                        FinallyCode.Text = data.Barcode;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void NavigateToWorkOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProductRecordItem selectItem= DataGrid_DataBaseData.SelectedItem as ProductRecordItem;
                if (selectItem != null)
                {
                    if (string.IsNullOrEmpty(selectItem.ProcessCode))
                    {
                        MessageBox.Show("请选择过程码");
                    }
                    else
                    {
                        this.Hide();

                        string formName = "WorkingOrderQuery";
                        OpenShowedForm showedForm = GlobalProperty.sGlobalProperty.GetOpenShowedForm(formName);
                        if (showedForm != null)
                        {
                            WorkOrderQuery workOrderQuery = showedForm.CurrForm as WorkOrderQuery;
                            workOrderQuery.SetProcessCode(selectItem.ProcessCode);
                            workOrderQuery.Show();
                            workOrderQuery.Activate();
                        }
                        else
                        {
                            WorkOrderQuery workOrderQuery = new WorkOrderQuery();
                            if (GlobalProperty.sGlobalProperty.AddForm(new OpenShowedForm() { CurrForm = workOrderQuery, FormName = formName, FormTitle = workOrderQuery.Title }, this.mainForm))
                            {
                                workOrderQuery.SetProcessCode(selectItem.ProcessCode);
                                workOrderQuery.Show();
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("请选择过程码");
                }
            }
            catch (Exception)
            {
            }
        }
        private void StationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (!_isCopyModeActive)
            {
                // 启动复制模式
               // StartCopyMode();
            }
            else
            {
                // 取消复制模式
               // CancelCopyMode();
            }
        }
        //private void StartCopyMode()
        //{
        //    _isCopyModeActive = true;
        //    DataGrid_DataBaseData.ClipboardCopyMode = DataGridClipboardCopyMode.ExcludeHeader;
        //    DataGrid_DataBaseData.SelectionUnit = DataGridSelectionUnit.Cell;
        //    CopyButton.Content = "取消单元格复制";
        //    CopyButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E67E22"));
        //}
        //private void CancelCopyMode()
        //{
        //    _isCopyModeActive = false;
        //    DataGrid_DataBaseData.ClipboardCopyMode = DataGridClipboardCopyMode.None;
        //    DataGrid_DataBaseData.SelectionUnit = DataGridSelectionUnit.FullRow;
        //    CopyButton.Content = "点击开始单元格复制";
        //    CopyButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
        //}




        // 关闭按钮
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private void Window_Activated(object sender, EventArgs e)
        {
        }
    }
    public class ProductRecordItem
    {
        public string ProductCode { get; set; }
        public string ProcessCode { get; set; }
        public string MotorCode { get; set; }
    }
}
