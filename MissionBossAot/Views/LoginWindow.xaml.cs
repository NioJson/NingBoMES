using CommunityToolkit.Mvvm.Input;
using MissionBossAot.Common;
using MissionBossAot.Models;
using MissionBossAot.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
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
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// 登录窗口model
        /// </summary>
        private LoginViewModel userLoginWindowDc;
        public LoginWindow()
        {
            InitializeComponent();
            //加载mvvw 模块
            userLoginWindowDc = new LoginViewModel(this);
            this.DataContext = userLoginWindowDc;
            //密码输入框注册回车事件
            PasswordBox_UserPassword.KeyDown += TxtPassword_KeyDown;
        }
        //点击登录
        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            var acc = (string)ComboBox_UserName.SelectedValue;
            if (string.IsNullOrEmpty(acc))
            {
                MessageResult result = GlobalMessageDialog.Show(
                "请选择账号！",
                "提示",
                MessageType.Warning,
                MessageMode.Confirm,
                3,
                this);
                return;
            }

            string password = PasswordBox_UserPassword.Password;
            userLoginWindowDc.UserAccount = acc;
            userLoginWindowDc.Password = password;
            using (var helper = new SqlConnectionHelper())
            {
                string sql = "SELECT * FROM Users WHERE UserAccount = @acc";
                var parameters = new[]
                {
                        SqlConnectionHelper.CreateParameter("@acc", acc)
                    };

                DataTable result = helper.ExecuteDataTable(sql, parameters);
                if (result != null && result.Rows != null && result.Rows.Count == 1)
                {
                    string pwdDb = result.Rows[0]["UserPassword"].ToString();
                    if (!string.IsNullOrEmpty(pwdDb) && BCryptHelper.sBCryptHelper.VerifyPassword(password, pwdDb))
                    {
                        string username = result.Rows[0]["UserName"].ToString();
                        UserParameters userParameters = new UserParameters() { UserAccount=acc, UserName= username, UserPassword = password };
                        GlobalProperty.sGlobalProperty.SetCurrUser(userParameters);
                        // 打开主窗口
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();

                        // 关闭登录窗口
                        this.Close();
                    }
                    else
                    {
                        MessageResult resultLoginFail = GlobalMessageDialog.Show(
                                                "密码错误！",
                                                "提示",
                                                MessageType.Error,
                                                MessageMode.Confirm,
                                                3,
                                                this);
                    }
                }
                else
                {
                    MessageResult resultLoginFail = GlobalMessageDialog.Show(
                    "账号错误！",
                    "提示",
                    MessageType.Error,
                    MessageMode.Confirm,
                    3,
                    this);
                }
            }
        }
        //密码输入框回车执行方法
        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Confirm_Click(sender, e);
            }
        }
        private void Button_Close_MouseEnter(object sender, MouseEventArgs e)
        {
            Common.MouseEnterLeave.ButtonMouseEnterLightGray((Button)sender);
        }

        private void Button_Close_MouseLeave(object sender, MouseEventArgs e)
        {
            Common.MouseEnterLeave.ButtonMouseLeave((Button)sender);
        }
    }
}
