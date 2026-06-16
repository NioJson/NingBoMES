using CommunityToolkit.Mvvm.Input;
using MissionBossAot.Common;
using MissionBossAot.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MissionBossAot.ViewModels
{
    /// <summary>
    /// 首页ViewModel
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private Window _mainForm;
        public List<CardItem> CenterCardsList { get; set; }

        public MainWindowViewModel(Window mainForm)
        {
            this._mainForm = mainForm;

            // 初始化数据
            CenterCardsList = new List<CardItem>
            {
                new CardItem
                {
                    Title = "工单管理",
                    Color = "#3498DB",
                    Buttons = new List<ButtonItem>
                    {
                        new ButtonItem
                        {
                            Text = "生产工单",
                            Color = "#3498DB",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "WorkOrderManager"}
                        },
                        new ButtonItem
                        {
                            Text = "查询工单",
                            Color = "#3498DB",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "WorkingOrderQuery"}
                        },
                        new ButtonItem
                        {
                            Text = "工单标签补打",
                            Color = "#3498DB",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "WorkOrderCodeReprintForm"}
                        }
                    }
                },
                new CardItem { 
                    Title ="数据管理",
                    Color = "#4ecdc4",
                    Buttons = new List<ButtonItem>
                    {
                        new ButtonItem
                        {
                            Text = "工站数据",
                            Color = "#4ecdc4",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "StationDataCollect"}
                        },
                        new ButtonItem
                        {
                            Text = "追溯管理",
                            Color = "#4ecdc4",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "TraceabilityManagementForm"}
                        }
                    }
                },
                new CardItem
                {
                    Title = "系统设置",
                    Color = "#2ECC71",
                    Buttons = new List<ButtonItem>
                    {
                        new ButtonItem { Text = "工站管理",
                            Color = "#2ECC71",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "WorkStation"}},
                        new ButtonItem { Text = "工单型号",
                            Color = "#2ECC71",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "OrderModel"}},
                        new ButtonItem { Text = "工单工艺路线",
                            Color = "#2ECC71",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "Routing"}},
                        new ButtonItem { Text = "工单生产班组",
                            Color = "#2ECC71",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "WorkTeam"}},
                        new ButtonItem { Text = "P30 胶水固化时间设置",
                            Color = "#2ECC71",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "GlueCuringSettingTime"}},
                        new ButtonItem { Text = "P30 转头二维码打印",
                            Color = "#2ECC71",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "TurnHeadQrCodeForm"}},
                        new ButtonItem { Text = "配方管理",
                            Color = "#2ECC71",
                            Command = new MainFormRelayCommand(ExecuteButtonClick),
                            CommandParameter = new CommandParameterData(){ ActName = "Formula"}}
                    }
                }
            };
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        //系统标题
        public string SystemTitle { get; set; } = "IWTCAMWM";
        public CornerRadius SystemFrameworkElementCornerRadiusLeft
        {
            get { return new CornerRadius(0); }
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SystemFrameworkElementCornerRadiusLeft)));
            }
        }
        public double SystemFrameworkElementCornerRadius
        {
            get { return 0; }
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SystemFrameworkElementCornerRadius)));
            }
        }
        private void ExecuteButtonClick(object parameter)
        {
            CommandParameterData action = parameter as CommandParameterData;
            if (action != null) 
            {
                switch (action.ActName)
                {
                    case "WorkOrderCodeReprintForm"://工单标签补打
                        OpenWorkOrderCodeReprintFormForm("WorkOrderCodeReprintForm");
                        break;
                    case "TraceabilityManagementForm"://追溯管理
                        OpenTraceabilityManagementForm("TraceabilityManagementForm");
                        break;
                    case "StationDataCollect"://工站数据 
                        OpenStationDataForm("StationDataCollect");
                        break;
                    case "WorkOrderManager"://生产工单
                        OpenWorkOrderForm("WorkOrderManager");
                        break;
                    case "WorkingOrderQuery"://工单查询
                        OpenWorkingOrderQueryForm("WorkingOrderQuery");
                        break;
                    case "OrderModel"://工单型号
                        OpenOrderModelForm();
                        break;
                    case "Routing"://工单工艺路线
                        OpenRoutingForm();
                        break;
                    case "WorkTeam"://工单生产班组
                        OpenWorkTeamForm();
                        break;
                    case "WorkStation"://工站管理 
                        OpenWorkStationForm();
                        break;
                    case "Formula"://配方管理
                        OpenFormulaForm();
                        break;
                    case "GlueCuringSettingTime"://胶水固化时间设置
                        OpenGlueCuringSettingTimeForm();
                        break;
                    case "TurnHeadQrCodeForm"://转头二维码打印
                        OpenTurnHeadQrCodeForm();
                        break;
                }
            }
        }
        private void OpenTurnHeadQrCodeForm()
        {
            TurnHeadQrCodePrint turnHeadQrCodePrint = new TurnHeadQrCodePrint();
            turnHeadQrCodePrint.Show();
        }
        //打开 胶水固化时间设置 编辑页面
        private void OpenGlueCuringSettingTimeForm()
        {
            GlueCuringSettingTime glueCuringSetting = new GlueCuringSettingTime();
            glueCuringSetting.Show();
        }
        //打开 配方管理 编辑页面
        private void OpenFormulaForm()
        {
            Formula formulaManage = new Formula();
            formulaManage.Show();
        }
        //打开 工站管理 编辑页面
        private void OpenWorkStationForm()
        {
            WorkStation workStationManage = new WorkStation();
            workStationManage.Show();
        }
        //打开 工单 生产班组 编辑页面
        private void OpenWorkTeamForm()
        {
            WorkTeam workTeamSetting = new WorkTeam();
            workTeamSetting.Show();
        }
        //打开 工单 工艺路线 编辑页面
        private void OpenRoutingForm()
        {
            RoutingSetting routingSetting = new RoutingSetting();
            routingSetting.Show();
        }
        //打开 工单型号编辑页面
        private void OpenOrderModelForm()
        {
            OrderModelSetting orderModelSetting = new OrderModelSetting();
            orderModelSetting.Show();
        }
        private void OpenWorkOrderCodeReprintFormForm(string formName)
        {
            OpenShowedForm showedForm = GlobalProperty.sGlobalProperty.GetOpenShowedForm(formName);
            if (showedForm != null) 
            {
                WorkOrderCodeReprint orderCodeReprint = showedForm.CurrForm as WorkOrderCodeReprint;
                orderCodeReprint.Show();
                orderCodeReprint.Activate();
            }
            else
            {
                WorkOrderCodeReprint workOrderCodeReprint = new WorkOrderCodeReprint();
                if (GlobalProperty.sGlobalProperty.AddForm(new OpenShowedForm() { CurrForm= workOrderCodeReprint,
                 FormName = workOrderCodeReprint.Name, FormTitle= workOrderCodeReprint.Title}, this._mainForm))
                {
                    workOrderCodeReprint.Show();
                }
            }
        }
        private void OpenTraceabilityManagementForm(string formName)
        {
            OpenShowedForm showedForm = GlobalProperty.sGlobalProperty.GetOpenShowedForm(formName);
            if (showedForm != null) 
            {
                TraceabilityManagement traceability = showedForm.CurrForm as TraceabilityManagement;
                traceability.Show();
                traceability.Activate();
            }
            else
            {
                TraceabilityManagement traceabilityManagement = new TraceabilityManagement(this._mainForm);
                if (GlobalProperty.sGlobalProperty.AddForm(new OpenShowedForm() { CurrForm = traceabilityManagement,FormName= traceabilityManagement.Name,FormTitle= traceabilityManagement.Title },this._mainForm))
                {
                    traceabilityManagement.Show();
                }
            }
        }
        private void OpenStationDataForm(string formName)
        {
            OpenShowedForm showedForm = GlobalProperty.sGlobalProperty.GetOpenShowedForm(formName);
            if (showedForm != null)
            {
                StationData stationDataCurr = showedForm.CurrForm as StationData;
                stationDataCurr.Show();
                stationDataCurr.Activate();      // 激活窗口
            }
            else
            {
                StationData stationData = new StationData();
                if (GlobalProperty.sGlobalProperty.AddForm(new OpenShowedForm() { CurrForm = stationData, FormName = stationData.Name, FormTitle = stationData.Title }, this._mainForm))
                {
                    stationData.Show();
                }
            }

        }
        private void OpenWorkingOrderQueryForm(string formName)
        {
            OpenShowedForm showedForm = GlobalProperty.sGlobalProperty.GetOpenShowedForm(formName);
            if(showedForm != null)
            {
                WorkOrderQuery workOrderQuery = showedForm.CurrForm as WorkOrderQuery;
                workOrderQuery.Show();
                workOrderQuery.Activate();
            }
            else
            {
                WorkOrderQuery workOrderQuery = new WorkOrderQuery();
                if (GlobalProperty.sGlobalProperty.AddForm(new OpenShowedForm() { CurrForm=workOrderQuery,FormName=formName,FormTitle=workOrderQuery.Title},this._mainForm))
                {
                    workOrderQuery.Show();
                }
            }
        }
        //打开 生产工单 页面
        private void OpenWorkOrderForm(string formName)
        {
            OpenShowedForm showedForm = GlobalProperty.sGlobalProperty.GetOpenShowedForm(formName);
            if (showedForm != null) 
            {
                WorkOrder workOrderCurr = showedForm.CurrForm as WorkOrder;
                workOrderCurr.Show();
                workOrderCurr.Activate();      // 激活窗口
            }
            else
            {
                WorkOrder workOrder = new WorkOrder(this._mainForm);
                if (GlobalProperty.sGlobalProperty.AddForm(new OpenShowedForm() { CurrForm = workOrder, FormName = workOrder.Name, FormTitle = workOrder.Title }, this._mainForm))
                {
                    workOrder.Show();
                }
            }
        }
        private bool CanExecuteButtonClick(object parameter)
        {
            // 返回按钮是否可用
            return false;
        }
    }
    public class MainFormRelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public MainFormRelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged;
    }
    public class CommandParameterData
    {
        public string ActName { get; set; }
    }
}
