using MissionBossAot.Common;
using System;
using System.Collections.Generic;
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

namespace MissionBossAot.Views
{
    /// <summary>
    /// TurnHeadQrCodePrint.xaml 的交互逻辑
    /// </summary>
    public partial class TurnHeadQrCodePrint : Window
    {
        public TurnHeadQrCodePrint()
        {
            InitializeComponent();
        }
        private void PrintLandingGearBtn_Click(object sender, RoutedEventArgs e)
        {
            // 打印起落架转头标签的逻辑
            int startIndex = 1;
            int count = 1;
            string stype = "1";
            string dateString = "1";
            string message = startIndex + "," + count + "," + stype + dateString;
            SendData(message);
        }

        private void PrintRobotArmBtn_Click(object sender, RoutedEventArgs e)
        {
            // 打印机械臂转头标签的逻辑
            int startIndex = 2;
            int count = 1;
            string stype = "2";
            string dateString = "2";
            string message = startIndex + "," + count + "," + stype + dateString;
            SendData(message);
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
                MessageBox.Show("连接打印机错误");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("打印错误");
                return false;
            }
        }
        // 关闭按钮
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
