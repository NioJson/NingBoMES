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
    /// 特殊卡片数据类
    /// </summary>
    public class SpecialCard : ICardItem, INotifyPropertyChanged
    {
        private string _Title;
        private string _Description;
        private string _ExtraInfo;
        private string _IconColor;
        private string _ButtonText;
        private string _WorkingTime;
        private List<BoxItem> _BoxItems;
        private RightCardType _CardType;

        public string Title
        {
            get => _Title;
            set
            {
                _Title = value;
                OnPropertyChanged();
            }
        }
        public string Description
        {
            get => _Description;
            set
            {
                _Description = value;
                OnPropertyChanged();
            }
        }
        public string ExtraInfo
        {
            get => _ExtraInfo;
            set
            {
                _ExtraInfo = value;
                OnPropertyChanged();
            }
        }
        public string IconColor
        {
            get => _IconColor;
            set
            {
                _IconColor = value;
                OnPropertyChanged();
            }
        }
        public string ButtonText
        {
            get => _ButtonText;
            set
            {
                _ButtonText = value;
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
        public List<BoxItem> BoxItems
        {
            get => _BoxItems;
            set
            {
                _BoxItems = value;
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
    // 方框项类
    public class BoxItem: INotifyPropertyChanged
    {
        public int DevId { get; set; }

        private string _Value;
        private string _Label;
        private string _TopRightColor;
        public string Value// 主要显示值，如数字或文本
        {
            get => _Value;
            set
            {
                _Value = value;
                OnPropertyChanged();
            }
        }
        public string Label// 标签文本
        {
            get => _Label;
            set
            {
                _Label = value;
                OnPropertyChanged();
            }
        }
        public string TopRightColor// 右上角角标背景颜色
        {
            get => _TopRightColor;
            set
            {
                _TopRightColor = value;
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
