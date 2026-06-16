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
    /// WorkOrderDataDetail.xaml 的交互逻辑
    /// </summary>
    public partial class WorkOrderDataDetail : Window
    {
        public WorkOrderDataDetail(DataTable filteredData)
        {
            InitializeComponent();
            DataGrid_DataBaseData.ItemsSource = filteredData.DefaultView;
        }
    }
}
