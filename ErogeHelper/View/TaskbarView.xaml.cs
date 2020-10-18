using GalaSoft.MvvmLight.Messaging;
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
    /// TaskbarView.xaml 的交互逻辑
    /// </summary>
    public partial class TaskbarView : Window
    {
        public TaskbarView()
        {
            InitializeComponent();
        }

        private void HookConfigOpen(object sender, RoutedEventArgs e)
        {
            var window = Application.Current.Windows.OfType<HookConfigView>().FirstOrDefault();
            if (window == null)
                new HookConfigView().Show();
            else
            {
                window.Activate();
            }
        }
    }
}
