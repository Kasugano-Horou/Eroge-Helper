using ErogeHelper.Common;
using ErogeHelper.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using System;
using System.Collections.Generic;
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

namespace ErogeHelper.View
{
    /// <summary>
    /// PreferenceView.xaml 的交互逻辑
    /// </summary>
    public partial class PreferenceView : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PreferenceView));

        public PreferenceView()
        {
            InitializeComponent();
        }

        // 在DataGrid Loading 完成后，把每一行的鼠标滑过事件订阅到Row_MouseEnter上去
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseEnter += Row_MouseEnter;
        }

        void Row_MouseEnter(object sender, MouseEventArgs e)
        {
            DataGridRow row = (DataGridRow)sender;
            Messenger.Default.Send(row, "PreferenceReceivedRow");
        }
    }
}
