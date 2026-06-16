using HandyControl.Tools.Extension;
using MissionBossAot.Models;
using MissionBossAot.ViewModels;
using MissionBossAot.Views;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MissionBossAot.Common
{
    /// <summary>
    /// 系统全局属性
    /// </summary>
    public class GlobalProperty
    {
        //申请上线的
        private ApplyStartWorkingObj MicoObj = new ApplyStartWorkingObj();
        /// <summary>
        /// 是否关闭主窗体
        /// </summary>
        private bool _mainFormCloseStatus = false;
        private List<OpenShowedForm> openShowedForms;
        //工单任务列表
        private ObservableCollection<ICardItem> orderMissionList;
        //当前登录用户
        private UserParameters _userCurrent;
        //通过列表更新右边任务栏状态
        private ConcurrentQueue<RightCardUpdateStatus> RightCardUpdateStatusList = new ConcurrentQueue<RightCardUpdateStatus>();
        public static GlobalProperty sGlobalProperty = new GlobalProperty();
        public GlobalProperty() 
        {
            // 每5秒钟刷新一下任务列表
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, e) => UpdateControlStatus();
            timer.Start();

            Thread td = new Thread(new ThreadStart(UpdateControlStatusThread));
            td.Start();
        } 
        //设置上线申请的过程码
        public void SetCurrWorkOrder(int id)
        {
            MicoObj.WorkOrderId = id;
        }
        //获取当前申请上线的过程码
        public ApplyStartWorkingObj GetCurrWorkOrder()
        {
            return MicoObj;
        }
        public void SetMainFormCloseStatus(bool act)
        {
            _mainFormCloseStatus = act;
        }
        //获取当前主窗体状态值
        public bool GetMainFormCloseStatus()
        {
            return _mainFormCloseStatus;
        }
        public void SetCurrUser(UserParameters u)
        {
            this._userCurrent = u;
        }
        public UserParameters GetCurrUser()
        {
            return this._userCurrent;
        }
        public bool AddForm(OpenShowedForm showedForm, Window mainForm)
        {
            try
            {
                if (string.IsNullOrEmpty(showedForm.FormName))
                {
                    MessageResult result = GlobalMessageDialog.Show(
                    "窗体Name 为空，不能打开！",
                    "提示",
                    MessageType.Warning,
                    MessageMode.Confirm,
                    3,
                    mainForm);
                    return false;
                }
                if (openShowedForms == null)
                {
                    openShowedForms = new List<OpenShowedForm>();
                }
                openShowedForms.Insert(0, showedForm);
                MainWindow _mainForm = mainForm as MainWindow;

                List<WindowItem> openWindows = new List<WindowItem>();
                foreach (OpenShowedForm item in openShowedForms)
                {
                    openWindows.Add(new WindowItem 
                    { 
                        Title = item.FormTitle, 
                        Name = item.FormName,
                        Command = new MainFormRelayCommand(ExecuteControlClick),
                        CommandParameter = new CommandParameterData() { ActName = item.FormName }
                    });
                }
                _mainForm.OpenWindowsList.ItemsSource = openWindows;

                return true;
            }
            catch (Exception ex)
            {
                Logger.sLogger.InsertLog(new LoggerObj() { LoggerType= LoggerType.Error,
                 Content="打开新窗体异常，具体信息："+ ex.Message});
                return false;
            }
        }
        public OpenShowedForm GetOpenShowedForm(string formName)
        {
            if (openShowedForms != null) 
            {
                return openShowedForms.FindAll(a => a.FormName == formName).FirstOrDefault();
            }
            else { return null; }
        }
        public List<OpenShowedForm> GetOpenShowedForms()
        {
            return openShowedForms;
        }
        //主窗体右边任务栏插入新的内容
        public void AddMissionList(NormalCard normalCard, Window mainForm)
        {
            orderMissionList.Insert(0, normalCard);
            MissionListInit(mainForm);
        }
        //主窗体右边任务栏初始化
        public void MissionListInit(Window mainForm)
        {
            var orderMissionListTemp = UpdateRightItemBoard();
            orderMissionListTemp = orderMissionListTemp.OrderByDescending(a => a.WorkingTime).ToList();
            orderMissionList = new ObservableCollection<ICardItem>(orderMissionListTemp);
            MainWindow _mainForm = mainForm as MainWindow;
            _mainForm.RightCardsList.ItemsSource = orderMissionList;
        }
        //更新右边任务列表的状态
        private void UpdateControlStatus()
        {
            RightCardUpdateStatusList.Enqueue(new RightCardUpdateStatus());
        }
        private void UpdateControlStatusThread()
        {
            while (!GlobalProperty.sGlobalProperty.GetMainFormCloseStatus())
            {
                if (RightCardUpdateStatusList.TryDequeue(out RightCardUpdateStatus result))
                {
                    if (orderMissionList != null && orderMissionList.Count > 0)
                    {
                        foreach (var item in orderMissionList)
                        {
                            switch (item.CardType)
                            {
                                case RightCardType.None:
                                    break;
                                case RightCardType.NormalCard:
                                    NormalCard nc = item as NormalCard;
                                    break;
                                case RightCardType.SpecialCard:
                                    SpecialCard sc = item as SpecialCard;
                                    var devCard = sc.BoxItems;
                                    List<DeviceInfo> deviceInfos = GlobalDevice.sGlobalDevice.GetDeviceInfos();
                                    if (devCard != null && devCard.Count > 0)
                                    {
                                        foreach (var itemdc in devCard)
                                        {
                                            var x = deviceInfos.FindAll(a => a.Id == itemdc.DevId).FirstOrDefault();
                                            if (x != null)
                                            {
                                                if (x.DeviceConnectStatus)
                                                {
                                                    itemdc.TopRightColor = "#4CAF50";
                                                }
                                                else
                                                {
                                                    itemdc.TopRightColor = "red";
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
        //初始化右侧卡片数据
        private List<ICardItem> UpdateRightItemBoard()
        {
            List<ICardItem> rightCards = new List<ICardItem>();
            try
            {
                //{
                //    new RightCardItem { Title = "最近文档", Subtitle = "project_specification.docx", Color = "#3498DB" },
                //    new RightCardItem { Title = "任务提醒", Subtitle = "完成代码审查 - 今天", Color = "#E74C3C" },
                //    new RightCardItem { Title = "消息通知", Subtitle = "有新的团队成员加入", Color = "#2ECC71" },
                //    new RightCardItem { Title = "系统更新", Subtitle = "新版本v2.0可用", Color = "#F39C12" },
                //    new RightCardItem { Title = "备份状态", Subtitle = "上次备份: 今天 10:30", Color = "#9B59B6" }
                //};
                List<WorkOrderModel> orderModels = GetWorkOrderModelInMission();
                if (orderModels != null && orderModels.Count > 0)
                {
                    foreach (var item in orderModels)
                    {
                        List<StatusItem> statusItems = new List<StatusItem>();
                        var meme = item.WorkOrderMissionStation != null ? item.WorkOrderMissionStation.OrderBy(a => a.StationExecuteSort).ToList() : new List<WorkOrderLinkStaion>();
                        string downWorkTime = "";
                        foreach (var itemSub in meme)
                        {
                            string colorString = "";
                            switch (itemSub.CurrStationStatus)
                            {
                                case "None"://未开始
                                    colorString = "#95A5A6";
                                    break;
                                case "Running"://进行中
                                    colorString = "#0078D7";
                                    break;
                                case "Pause"://暂停
                                    colorString = "#FFC000";
                                    break;
                                case "Stop"://停止
                                    colorString = "#E31B23";
                                    break;
                                case "Succeed"://完成
                                    colorString = "#00B159";
                                    break;
                                default:
                                    break;
                            }
                            if (string.IsNullOrEmpty(downWorkTime))
                            {
                                downWorkTime = itemSub.CreateDatetime;
                            }
                            statusItems.Add(new StatusItem() { Color = colorString, Value = "" + itemSub.StationExecuteSort });
                        }
                         
                        rightCards.Add(new NormalCard
                        {
                            Title = "工单号："+item.OrderId,
                            Subtitle = downWorkTime,
                            Color = "#3498DB",
                            StationCount = item.WorkOrderMissionStation != null ? item.WorkOrderMissionStation.Count + "" : "0",
                            StatusList = statusItems,
                            WorkingTime = downWorkTime,
                            MissionType = "Visible",
                            StationType = "Collapsed",
                            CardType = RightCardType.NormalCard,
                            WorkOrderId = item.Id
                        }
                        );
                    }
                }
                List<BoxItem> boxx = GlobalDevice.sGlobalDevice.GetDeviceCardList();
                rightCards.Add(new SpecialCard
                {
                    Title = "工站信息列表--设备通讯详情",
                    WorkingTime = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    BoxItems = boxx,
                    CardType = RightCardType.SpecialCard
                });
            }
            catch (Exception)
            {
            }
            return rightCards;
        }
        //获取所有任务列表中的工站信息
        private List<WorkOrderLinkStaion> GetWorkOrderModelInMissionStation(List<int> orderIds)
        {
            List<WorkOrderLinkStaion> workOrderStaModels = new List<WorkOrderLinkStaion>();
            try
            {
                using (var helper = new SqlConnectionHelper())
                {
                    DataTable dtOrderSta = null;
                    string sqlSelect = "SELECT * FROM WorkOrderMissionFlow where OrderID in(";
                    for (int i = 0; i < orderIds.Count; i++)
                    {
                        sqlSelect += orderIds[i];
                        if (i != (orderIds.Count - 1))
                        {
                            sqlSelect += ",";
                        }
                    }
                    sqlSelect += ")";

                    dtOrderSta = helper.ExecuteDataTable(sqlSelect);
                    if (dtOrderSta != null && dtOrderSta.Rows.Count > 0)
                    {
                        List<WorkStationItem> workStationItems = GlobalBaseData.sGlobalBaseData.GetWorkOrderStation();
                        foreach (DataRow row in dtOrderSta.Rows)
                        {
                            string orId = !Convert.IsDBNull(row["OrderID"]) ? row["OrderID"].ToString() : "";
                            int.TryParse(orId, out int orderId);

                            string idInt = !Convert.IsDBNull(row["ID"]) ? row["ID"].ToString() : "";
                            int.TryParse(idInt, out int idI);

                            string staId = !Convert.IsDBNull(row["StationID"]) ? row["StationID"].ToString() : "";
                            int.TryParse(staId, out int sId);

                            string staExecId = !Convert.IsDBNull(row["StationExecuteSort"]) ? row["StationExecuteSort"].ToString() : "";
                            int.TryParse(staExecId, out int execId);

                            var sta = workStationItems.FindAll(a => a.Id == sId).FirstOrDefault();
                            workOrderStaModels.Add(new WorkOrderLinkStaion
                            {
                                Id = idI,
                                OrderID = orderId,
                                StationID = sId,
                                CurrStationStatus = !Convert.IsDBNull(row["CurrStationStatus"]) ? row["CurrStationStatus"].ToString() : "",
                                CreateDatetime = !Convert.IsDBNull(row["CreateDatetime"]) ? row["CreateDatetime"].ToString() : "",
                                StationExecuteSort = execId,
                                StationCode = sta!=null? sta.Code:"无",
                                StationName = sta != null ? sta.Name:"无"
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return workOrderStaModels;
        }
        //查询已下发的工单
        private List<WorkOrderModel> GetWorkOrderModelInMission()
        {
            List<WorkOrderModel> workOrderModels = new List<WorkOrderModel>();
            try
            {
                using (var helper = new SqlConnectionHelper())
                {
                    DataTable dtWorkOrder = null;
                    string sqlSelect = "SELECT * FROM WorkOrderNew where EnableStatus = '已下发'";
                    dtWorkOrder = helper.ExecuteDataTable(sqlSelect);

                    if (dtWorkOrder != null && dtWorkOrder.Rows.Count > 0)
                    {
                        List<int> ints = new List<int>();
                        foreach (DataRow row in dtWorkOrder.Rows)
                        {
                            string amount = !Convert.IsDBNull(row["OrderAmount"]) ? row["OrderAmount"].ToString() : "";
                            int.TryParse(amount, out int resInt);

                            string idInt = !Convert.IsDBNull(row["ID"]) ? row["ID"].ToString() : "";
                            int.TryParse(idInt, out int idI);
                            ints.Add(idI);
                            workOrderModels.Add(new WorkOrderModel
                            {
                                WorkOrderUUID = !Convert.IsDBNull(row["WorkOrderUUID"]) ? row["WorkOrderUUID"].ToString() : "",
                                Id = idI,
                                OrderId = !Convert.IsDBNull(row["OrderId"]) ? row["OrderId"].ToString() : "",
                                ModelText = !Convert.IsDBNull(row["ModelText"]) ? row["ModelText"].ToString() : "",
                                FlowOrderId = !Convert.IsDBNull(row["FlowOrderId"]) ? row["FlowOrderId"].ToString() : "",
                                RoutingText = !Convert.IsDBNull(row["RoutingText"]) ? row["RoutingText"].ToString() : "",
                                ProductName = !Convert.IsDBNull(row["ProductName"]) ? row["ProductName"].ToString() : "",
                                OrderAmount = resInt,
                                WorkBackNo = !Convert.IsDBNull(row["WorkBackNo"]) ? row["WorkBackNo"].ToString() : "",
                                EnableStatus = !Convert.IsDBNull(row["EnableStatus"]) ? row["EnableStatus"].ToString() : "",
                                CreateDatetime = !Convert.IsDBNull(row["CreateDatetime"]) ? row["CreateDatetime"].ToString() : "",
                                OrderStartDatetime = !Convert.IsDBNull(row["OrderStartDatetime"]) ? row["OrderStartDatetime"].ToString() : "",
                                PlanedCompleteDatetime = !Convert.IsDBNull(row["OrderStartDatetime"]) ? row["OrderStartDatetime"].ToString() : ""
                            });
                        }
                        List<WorkOrderLinkStaion> miss = GetWorkOrderModelInMissionStation(ints);
                        foreach (var item in workOrderModels)
                        {
                            item.WorkOrderMissionStation = miss.FindAll(a => a.OrderID == item.Id);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return workOrderModels;
        }
        private void ExecuteControlClick(object parameter)
        {
            CommandParameterData action = parameter as CommandParameterData;
            if (action != null)
            {
                OpenShowedForm form = GetOpenShowedForm(action.ActName);
                if (form != null)
                {
                    form.CurrForm.Show();
                    form.CurrForm.Activate();
                }
            }
        }
    }
    public class OpenShowedForm
    {
        public string FormTitle {  get; set; }
        public string FormName { get; set; }
        public Window CurrForm { get; set; }
    }
    public class RightCardUpdateStatus()
    {

    }
    public class ApplyStartWorkingObj()
    {
        public int WorkOrderId { get; set; }
    }
}
