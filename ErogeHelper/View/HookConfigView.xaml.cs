using ErogeHelper.Service;
using GalaSoft.MvvmLight.Ioc;
using System.Linq;
using System.Windows;

namespace ErogeHelper.View
{
    /// <summary>
    /// HookConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class HookConfigView : Window, IWindowService
    {
        public HookConfigView()
        {
            InitializeComponent();
            
            if (!SimpleIoc.Default.IsRegistered<IWindowService>())
                SimpleIoc.Default.Register<IWindowService>(() => this);
        }

        /// <summary>
        /// 打开主窗口
        /// </summary>
        public void OpenWindow()
        {
            var window = Application.Current.Windows.OfType<GameView>().FirstOrDefault();
            if (window == null)
                new GameView().Show();
            Close();
            SimpleIoc.Default.Unregister<IWindowService>();
        }
    }
}
