using ErogeHelper.Model.Singleton;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using log4net;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ErogeHelper.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GameView : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GameView));

        private readonly GameInfo gameInfo = GameInfo.Instance;

        public IntPtr gameHWnd = IntPtr.Zero;
        private bool textPanelPin;

        public GameView()
        {
            log.Info("Initialize");
            textPanelPin = false;
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);

            InitializeComponent(); // VM Initialize -> Component Initialize

            SetGameWindowHook();
        }

        private void NotificationMessageReceived(NotificationMessage obj)
        {
            if (obj.Notification == "MakeTextPanelPin")
            {
                log.Info("Set TextPanel Pin");
                textPanelPin = true;
            }
            if (obj.Notification == "OpenCard")
            {
                WordCard.IsOpen = true;
            }
            if (obj.Notification == "CloseCard")
            {
                WordCard.IsOpen = false;
            }
        }

        #region Window Follow Game Initialize
        protected Hook.WinEventDelegate WinEventDelegate;
        private static GCHandle GCSafetyHandle;
        private IntPtr hWinEventHook;
        private void SetGameWindowHook()
        {
            WinEventDelegate = new Hook.WinEventDelegate(WinEventCallback);
            GCSafetyHandle = GCHandle.Alloc(WinEventDelegate);

            var targetProc = gameInfo.HWndProc;

            targetProc.EnableRaisingEvents = true;
            targetProc.Exited += new EventHandler(Window_Closed);
            Closed += Window_Closed;

            gameHWnd = targetProc.MainWindowHandle;
            uint targetThreadId = Hook.GetWindowThread(gameHWnd);

            if (gameHWnd != IntPtr.Zero)
            {
                // 调用 SetWinEventHook 传入 WinEventDelegate 回调函数
                hWinEventHook = Hook.WinEventHookOne(Hook.SWEH_Events.EVENT_OBJECT_LOCATIONCHANGE,
                                                     WinEventDelegate,
                                                     (uint)targetProc.Id,
                                                     targetThreadId);
                // 首次设置窗口位置
                SetLocation();
                log.Info("Begin to follow the window");
                return;
            }
        }

        private double winShadow;

        private void SetLocation()
        {
            var rect = Hook.GetWindowRect(gameHWnd);
            var rectClient = Hook.GetClientRect(gameHWnd);
            Width = rect.Right - rect.Left;  // rectClient.Right + shadow*2
            Height = rect.Bottom - rect.Top; // rectClient.Bottom + shadow + title

            winShadow = (Width - rectClient.Right) / 2;

            var wholeHeight = rect.Bottom - rect.Top;
            var winTitleHeight = wholeHeight - rectClient.Bottom - winShadow;

            ClientArea.Margin = new Thickness(winShadow, winTitleHeight, winShadow, winShadow);

            Left = rect.Left;
            Top = rect.Top;
        }

        protected void WinEventCallback(IntPtr hWinEventHook,
                                    Hook.SWEH_Events eventType,
                                    IntPtr hWnd,
                                    Hook.SWEH_ObjectId idObject,
                                    long idChild,
                                    uint dwEventThread,
                                    uint dwmsEventTime)
        {
            // 仅游戏窗口获取焦点时调用
            //if (hWnd == GameInfo.Instance.hWnd &&
            //    eventType == Hook.SWEH_Events.EVENT_OBJECT_FOCUS)
            //{
            //    log.Info("Game window get foucus");
            //}
            // 更新窗口信息
            if (hWnd == gameHWnd &&
                eventType == Hook.SWEH_Events.EVENT_OBJECT_LOCATIONCHANGE &&
                idObject == (Hook.SWEH_ObjectId)Hook.SWEH_CHILDID_SELF)
            {
                SetLocation();
            }
        }
        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            log.Info("Detected quit event");
            GCSafetyHandle.Free();
            Hook.WinEventUnhook(hWinEventHook);
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                Closed -= Window_Closed;
                Close();
            });
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var interopHelper = new WindowInteropHelper(this);
            int exStyle = Hook.GetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE);
            Hook.SetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE, exStyle | Hook.WS_EX_NOACTIVATE);
            // bool(exStyle & win32con.WS_EX_NOACTIVATE) 窗口忽视焦点开关状态
            // exStyle & ~win32con.WS_EX_NOACTIVATE 关闭忽视焦点（即默认状态）
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WinArea.SetValue(StyleProperty, null);
            ClientArea.SetValue(StyleProperty, null);
            if (textPanelPin == true)
            {
                TriggerPopupBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void TriggerPopupBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (textPanelPin == false)
            {
                TextArea.Visibility = Visibility.Visible;
                TriggerPopupBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void TextArea_MouseLeave(object sender, MouseEventArgs e)
        {
            if (textPanelPin == false)
            {
                TextArea.Visibility = Visibility.Collapsed;
                TriggerPopupBorder.Visibility = Visibility.Visible;
            }
        }
    }
}