using CommunityToolkit.Mvvm.Input;
using HandyControl.Expression.Shapes;
using HandyControl.Tools.Extension;
using MissionBossAot.Common;
using MissionBossAot.Models;
using MissionBossAot.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MissionBossAot.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private Window loginForm;
        private string userAccount;
        private string userPassword;
        /// <summary>
        /// 异步加载登录窗口的账号信息
        /// </summary>
        private bool _isLoadingUserParameters;
        public event PropertyChangedEventHandler? PropertyChanged;
        public IRelayCommand LoginFormCloseCommand { get; }
        public IRelayCommand LoginForgetUserPasswordCommand { get; }
        public IRelayCommand WindowMoveCommand { get; }

        public LoginViewModel(Window loginForm)
        {
            this.loginForm = loginForm;
            LoginFormCloseCommand = new LoginRelayCommand(
                () =>
                {
                    Environment.Exit(0);
                },
                "关闭登录窗口",
                "用户关闭登录窗口"
            );
            LoginForgetUserPasswordCommand = new LoginRelayCommand(() => { Environment.Exit(0); },
                "打开找回密码窗口",
                "用户通过此窗口修改密码");

            WindowMoveCommand = new LoginRelayCommand(() =>
            {
                try
                {
                    loginForm.DragMove();
                }
                catch { }
            },
                "拖动登录窗体",
                "按住鼠标左键拖动登录窗口");
        }
        #region
        public SolidColorBrush LoginTheme { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#5A88D3");
        public CornerRadius ElementCornerRadius { get; set; } = new CornerRadius(0, 0, 0, 0);
        public CornerRadius ElementCornerRadiusCloseButton { get; set; } = new CornerRadius(0, 0, 0, 0);
        public CornerRadius SystemFrameworkElementCornerRadius { get; set; } = new CornerRadius(0, 0, 0, 0);
        public SolidColorBrush UserLoginFormUserName { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#5D6B99");
        /// <summary>
        /// 初始化登录窗口下拉账号数据
        /// </summary>
        public Dictionary<string, UserParameters> DictionaryUserParemeters
        {
            get
            {
                Dictionary<string, UserParameters> pairs = new Dictionary<string, UserParameters>();
                try
                {
                    using (var helper = new SqlConnectionHelper())
                    {
                        string sql = "SELECT UserAccount,UserName FROM Users";
                        DataTable result = helper.ExecuteDataTable(sql);
                        if (result != null && result.Rows != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                for (int i = 0; i < result.Rows.Count; i++)
                                {
                                    string acc = null;
                                    string userName = null;
                                    if (result.Rows[i]["UserAccount"] != null)
                                    {
                                        acc = result.Rows[i]["UserAccount"].ToString();
                                    }
                                    if (result.Rows[i]["UserName"] != null)
                                    {
                                        userName = result.Rows[i]["UserName"].ToString();
                                    }
                                    if (!string.IsNullOrEmpty(acc) && !string.IsNullOrEmpty(userName))
                                    {
                                        pairs.Add(acc, new UserParameters() { UserAccount = acc, UserName = userName });
                                    }
                                }
                            }
                            else
                            {
                                MessageResult resultLoginFail = GlobalMessageDialog.Show(
                                "无账号可用！",
                                "提示",
                                MessageType.Error,
                                MessageMode.AutoClose,
                                3,
                                this.loginForm);
                            }
                        }
                        else
                        {
                            MessageResult resultLoginFail = GlobalMessageDialog.Show(
                            "连接SqlServer错误！",
                            "提示",
                            MessageType.Error,
                            MessageMode.Confirm,
                            3,
                            this.loginForm);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageResult resultLoginFail = GlobalMessageDialog.Show(
                            "连接SqlServer 获取账号信息异常错误！",
                            "提示",
                            MessageType.Error,
                            MessageMode.Confirm,
                            3,
                            this.loginForm);
                    Logger.sLogger.InsertLog(new LoggerObj()
                    {
                        Content = "系统获取登录账号异常*****>>" + ex.Message,
                        LoggerType = LoggerType.Error
                    });
                }
                // 在加载 DictionaryUserParemeters 数据后
                if (pairs?.Keys?.Any() == true)
                {
                    UserAccount = pairs.Keys.First();
                }
                return pairs;
            }
        }

        public string UserAccount
        {
            get { return userAccount; }
            set { userAccount = value; }
        }
        public string Password
        {
            get { return userPassword; }
            set { userPassword = value; }
        }
        #endregion
        #region
        
        #endregion
    }

    public class LoginRelayCommand : IRelayCommand
    {
        private readonly IRelayCommand _innerCommand;
        private readonly string _commandName;
        private readonly string _description;
        public LoginRelayCommand(Action execute, string commandName, string description = "", Func<bool> canExecute = null)
        {
            _innerCommand = canExecute == null
               ? new RelayCommand(execute)
               : new RelayCommand(execute, canExecute);
            _commandName = commandName;
            _description = description;
        }
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _innerCommand.CanExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _innerCommand.Execute(parameter);
        }

        public void NotifyCanExecuteChanged()
        {
            _innerCommand.NotifyCanExecuteChanged();
        }
    }
}
