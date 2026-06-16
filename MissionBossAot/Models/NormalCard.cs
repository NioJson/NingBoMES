using MissionBossAot.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    /// <summary>
    /// 普通卡片数据类
    /// </summary>
    public class NormalCard : ICardItem, INotifyPropertyChanged
    {
        private string _Title;
        private string _Subtitle;
        private string _Color;
        private string _StationCount;
        private List<StatusItem> _StatusList;
        private string _MissionType;
        private string _StationType;
        private string _WorkingTime;
        private RightCardType _CardType;

        public int WorkOrderId {  get; set; }
        public string Title
        {
            get => _Title;
            set
            {
                _Title = value;
                OnPropertyChanged();
            }
        }
        public string Subtitle
        {
            get => _Subtitle;
            set
            {
                _Subtitle = value;
                OnPropertyChanged();
            }
        }
        public string Color
        {
            get => _Color;
            set
            {
                _Color = value;
                OnPropertyChanged();
            }
        }
        public string StationCount
        {
            get => _StationCount;
            set
            {
                _StationCount = value;
                OnPropertyChanged();
            }
        }
        public List<StatusItem> StatusList
        {
            get => _StatusList;
            set
            {
                _StatusList = value;
                OnPropertyChanged();
            }
        }
        public string MissionType
        {
            get => _MissionType;
            set
            {
                _MissionType = value;
                OnPropertyChanged();
            }
        }
        public string StationType
        {
            get => _StationType;
            set
            {
                _StationType = value;
                OnPropertyChanged();
            }
        }
        public string WorkingTime
        {
            get => _WorkingTime;
            set
            {
                _WorkingTime = value;
                OnPropertyChanged();
            }
        }
        public RightCardType CardType
        {
            get => _CardType;
            set
            {
                _CardType = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
