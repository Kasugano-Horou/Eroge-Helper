using GalaSoft.MvvmLight.Messaging;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ErogeHelper.View
{
    /// <summary>
    /// HookConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class HookConfigView : Window
    {
        public HookConfigView()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // FIXME: 部分奇葩引擎(ハミダシ)窗口会穿透
            var interopHelper = new WindowInteropHelper(this);
            int exStyle = Hook.GetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE);
            Hook.SetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE, exStyle | Hook.WS_EX_NOACTIVATE);

            DispatcherTimer timer = new DispatcherTimer();
            var pointer = new WindowInteropHelper(this);
            timer.Tick += (sender, _) =>
            {
                if (pointer.Handle == IntPtr.Zero)
                {
                    timer.Stop();
                }
                Hook.BringWindowToTop(pointer.Handle);
            };

            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void NotificationMessageReceived(NotificationMessage msg)
        {
            if (msg.Notification == "ShowGameView")
            {
                var window = Application.Current.Windows.OfType<GameView>().FirstOrDefault();
                if (window == null)
                    new GameView().Show();
                Close();
            }
        }
    }
}
