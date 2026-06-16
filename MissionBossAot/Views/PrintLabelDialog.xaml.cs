using MissionBossAot.Common;
using MissionBossAot.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
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
using MessageBox = System.Windows.MessageBox;
using Window = System.Windows.Window;

namespace MissionBossAot.Views
{
    /// <summary>
    /// PrintLabelDialog.xaml 的交互逻辑
    /// </summary>
    public partial class PrintLabelDialog : Window
    {
        public PrintLabelDialog(int orderId)
        {
            InitializeComponent();
            this.DataContext = this;
            LoadData(orderId);
        }
        private void LoadData(int orderId)
        {
            List<PrintItem> _printDataTable = new List<PrintItem>();
            using (var helper = new SqlConnectionHelper())
            {
                string sqlSelect = "SELECT ProcessCode FROM WorkOrderNewProductCode where WorkOrderId=" + orderId;
                DataTable dtWorkOrder = helper.ExecuteDataTable(sqlSelect);
                if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                {
                    for (int i = 0; i < dtWorkOrder.Rows.Count; i++)
                    {
                        DataRow dataRow = dtWorkOrder.Rows[i];
                        _printDataTable.Add(new PrintItem()
                        {
                            CodeString = !Convert.IsDBNull(dataRow["ProcessCode"]) ? dataRow["ProcessCode"].ToString() : "",
                            IsSelected = false
                        }); 
                    }
                }
            }
            PrintDataGrid.ItemsSource = _printDataTable;
        }
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = PrintDataGrid.ItemsSource
                .Cast<PrintItem>()
                .Where(item => item.IsSelected)
                .ToList();
            if (selectedItems != null && selectedItems.Count > 0)
            {
                List<PrintParameter> printParameters = new List<PrintParameter>();
                for (int i = 0; i < selectedItems.Count; i++)
                {
                    string code = selectedItems[i].CodeString;
                    if (code.Length > 20)
                    {
                        try
                        {
                            string typeString = code.Substring(0, 9);
                            string dateStr = code.Substring(9, 8);
                            string indexStr = code.Substring(17);
                            int.TryParse(indexStr, out int startIndex);
                            printParameters.Add(new PrintParameter()
                            {
                                 DateString = dateStr,StartIndex = startIndex, TypeString = typeString,
                            });
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                var result = printParameters.OrderBy(x => x.StartIndex)
                    .Select((item, index) => new { item, diff = item.StartIndex - index })
                    .GroupBy(x => x.diff)
                    .Select(g => new PrintParameter
                    {
                        StartIndex = g.Min(x => x.item.StartIndex),
                        Count = g.Count(),  // 新增：统计每组有多少个元素
                        TypeString = g.First().item.TypeString,
                        DateString = g.First().item.DateString
                    })
                    .ToList();
                foreach (var item in result)
                {
                    string message = item.StartIndex + ","+item.Count+"," + item.TypeString + item.DateString;
                    SendData(message);
                    Thread.Sleep(1000);
                }
            }
            else
            {
                MessageBox.Show("请选择需要打印的数据！");
                return;
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
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Information, Content = "连接打印机错误，" + ex.Message });
                return false;
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType = LoggerType.Information, Content = "连接打印机错误，出现异常" + ex.Message });
                return false;
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
    public class PrintItem
    {
        public bool IsSelected { get; set; }
        public string CodeString { get; set; }
    }
    public class PrintParameter
    {
        public int StartIndex { get; set; }
        public string TypeString { get; set; }
        public string DateString { get; set; }
        public int Count { get; set; }
    }
}
