using ErogeHelper.Common;
using ErogeHelper.Model;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using log4net;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace ErogeHelper.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GameView : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GameView));

        public IntPtr gameHWnd = IntPtr.Zero;

        public GameView()
        {
            log.Info("Initialize");
            InitializeComponent(); // VM Initialize -> Component Initialize

            SetGameWindowHook();
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);
            Unloaded += (sender, e) => Messenger.Default.Unregister(this);
        }

        #region Window Follow Game Initialize
        protected Hook.WinEventDelegate WinEventDelegate;
        private static GCHandle GCSafetyHandle;
        private IntPtr hWinEventHook;
        private void SetGameWindowHook()
        {
            WinEventDelegate = new Hook.WinEventDelegate(WinEventCallback);
            GCSafetyHandle = GCHandle.Alloc(WinEventDelegate);

            var gameInfo = (GameInfo)SimpleIoc.Default.GetInstance(typeof(GameInfo));
            var targetProc = gameInfo.HWndProc;

            targetProc.EnableRaisingEvents = true;
            targetProc.Exited += new EventHandler(Window_Closed);
            Closed += Window_Closed;

            gameHWnd = targetProc.MainWindowHandle;
            log.Info($"Set handle {gameHWnd} Title: {targetProc.MainWindowTitle}");
            uint targetThreadId = Hook.GetWindowThread(gameHWnd);
            dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;

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
        private double dpi;

        private void SetLocation()
        {
            var rect = Hook.GetWindowRect(gameHWnd, dpi);
            var rectClient = Hook.GetClientRect(gameHWnd, dpi);
            // 再把字体除以dpi好了嘛 不解决窗口大小随着字体变化，两个事情

            Width = rect.Right - rect.Left;  // rectClient.Right + shadow*2
            Height = rect.Bottom - rect.Top; // rectClient.Bottom + shadow + title

            winShadow = (Width - rectClient.Right) / 2;

            var wholeHeight = rect.Bottom - rect.Top;
            var winTitleHeight = wholeHeight - rectClient.Bottom - winShadow;

            ClientArea.Margin = new Thickness(winShadow, winTitleHeight, winShadow, winShadow);

            Left = rect.Left;
            Top = rect.Top;
        }

        protected override void OnDpiChanged(DpiScale oldDpiScaleInfo, DpiScale newDpiScaleInfo)
        {
            dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
            log.Info($"Current screen dpi {dpi*100}%");
        }

        protected void WinEventCallback(IntPtr hWinEventHook,
                                    Hook.SWEH_Events eventType,
                                    IntPtr hWnd,
                                    Hook.SWEH_ObjectId idObject,
                                    long idChild,
                                    uint dwEventThread,
                                    uint dwmsEventTime)
        {
            // 仅游戏窗口获取焦点时会调用
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
                Application.Current.Shutdown();
            });
        }

        #region When Window Loding
        // 设置窗口焦点状态与BringToTop计时器
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Do upward compatible
            var NoFocusFlag = false;
            try
            {
                var ret = EHConfig.GetValue(EHNode.NoFocus);
                NoFocusFlag = bool.Parse(ret);
            }
            catch (NullReferenceException)
            {
                // create the node
                EHConfig.SetValue(EHNode.NoFocus, NoFocusFlag.ToString());
            }

            // 处理全局开启了MTMode，在加载别的游戏时focus选项还没打开的情况
            if (Properties.Settings.Default.OnlyMachineTranslation && NoFocusFlag == false)
            {
                NoFocusFlag = true;
                EHConfig.SetValue(EHNode.NoFocus, NoFocusFlag.ToString());
            }

            if (NoFocusFlag)
            {
                var interopHelper = new WindowInteropHelper(this);
                int exStyle = Hook.GetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE);
                Hook.SetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE, exStyle | Hook.WS_EX_NOACTIVATE);
            }

            DispatcherTimer timer = new DispatcherTimer();
            var pointer = new WindowInteropHelper(this);
            timer.Tick += (sender, _) =>
            {
                if (pointer.Handle == IntPtr.Zero)
                {
                    timer.Stop();
                }
                // Still get a little bad exprience with right click taskbar icon
                if (gameHWnd == Hook.GetForegroundWindow())
                {
                    Hook.BringWindowToTop(pointer.Handle);
                }
            };

            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Clear red border in design mode
            WinArea.SetValue(StyleProperty, null);
            ClientArea.SetValue(StyleProperty, null);
            // Hide TextArea by default
            TextArea.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region TextWindow Pin
        private bool textPanelPin = false;

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
        #endregion

        private void NotificationMessageReceived(NotificationMessage obj)
        {
            // Must called after InitializeComponent() finished

            if (obj.Notification == "MakeTextPanelPin")
            {
                log.Info("Set TextPanel Pin");
                textPanelPin = true;
                TriggerPopupBorder.Visibility = Visibility.Collapsed;
                TextArea.Visibility = Visibility.Visible;
                TextArea.Background = Background;
            }
            if (obj.Notification == "CancelTextPanelPin")
            {
                log.Info("Cancel TextPanel Pin");
                textPanelPin = false;
                TriggerPopupBorder.Visibility = Visibility.Visible;
                TextArea.Visibility = Visibility.Collapsed;
                TextArea.Background = new SolidColorBrush(Colors.Black) { Opacity = 0.1 };
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
    }
}