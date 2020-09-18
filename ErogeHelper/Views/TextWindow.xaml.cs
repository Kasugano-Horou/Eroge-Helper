using ErogeHelper.Models;
using ErogeHelper.Utils;
using ErogeHelper.ViewModels;
using log4net;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace ErogeHelper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TextWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TextWindow));

        protected Hook.WinEventDelegate WinEventDelegate;
        private static GCHandle GCSafetyHandle; // 需回收释放
        private IntPtr hWinEventHook;   // 同上

        private GameInfo gameInfo = GameInfo.Instance;

        /// <summary>
        /// Constructor of TextWindow, set hook for the window
        /// </summary>
        public TextWindow()
        {
            InitializeComponent();

            WinEventDelegate = new Hook.WinEventDelegate(WinEventCallback);
            GCSafetyHandle = GCHandle.Alloc(WinEventDelegate);

            try
            {
                log.Info("Start find game's window.");

                // 获取游戏进程
                Process targetProc = gameInfo.ProcList[0];

                //设置进程终止时触发的事件
                targetProc.EnableRaisingEvents = true;
                // TODO: 失去焦点时 TextWindow 也失去置顶， 这些应该由ICommand 控制吧

                // 设置窗口跟随相关钩子
                if (targetProc != null)
                {
                    // didn't useful
                    targetProc.WaitForInputIdle(7000); // may throw error

                    var sw = new Stopwatch();
                    sw.Start();
                    while (targetProc.MainWindowHandle == IntPtr.Zero)
                        Thread.Sleep(100);
                    //targetProc.Refresh();
                    log.Info($"Spend {sw.Elapsed.TotalSeconds}s to find game window's handle");
                    sw.Stop();

                    gameInfo.hWnd = targetProc.MainWindowHandle;
                    Debug.Assert(gameInfo.hWnd != (IntPtr)0);
                    uint targetThreadId = Hook.GetWindowThread(gameInfo.hWnd);

                    if (gameInfo.hWnd != IntPtr.Zero)
                    {
                        // 调用 SetWinEventHook 传入 WinEventDelegate
                        hWinEventHook = Hook.WinEventHookOne(Hook.SWEH_Events.EVENT_OBJECT_LOCATIONCHANGE,
                                                             WinEventDelegate,
                                                             (uint)targetProc.Id,
                                                             targetThreadId);
                        // 首次设置窗口位置
                        var rect = Hook.GetWindowRect(gameInfo.hWnd);
                        var wholeHeight = rect.Bottom - rect.Top;
                        WinTitileHeight = wholeHeight - Hook.GetClientRect(gameInfo.hWnd).Bottom - WinShadow;

                        SetLocation();
                        log.Info("Begin to follow the window");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Debug($"Exception Occurrent! {ex.HResult}, {ex.Data}, {DateTime.Now}");
                throw ex;
            }
        }

        private const int WinShadow = 8;
        private readonly int WinTitileHeight;
        private void SetLocation()
        {
            gameInfo.Rect = Hook.GetWindowRect(gameInfo.hWnd);
            var rectClient = Hook.GetClientRect(gameInfo.hWnd);
            //gameInfo.Width = gameInfo.Rect.Right - gameInfo.Rect.Left;  // same as rectClient.Right + shadow*2
            //gameInfo.Height = gameInfo.Rect.Bottom - gameInfo.Rect.Top; // rectClient.Bottom + shadow + title

            if (gameInfo.Rect.Left == -32000 && gameInfo.Rect.Top == -32000)
            {
                this.Hide();
            }
            else
            {
                this.Show();
                //this.Activate();
                //this.WindowState = WindowState.Normal;
            }

            // window mode
            this.Left = gameInfo.Rect.Left + WinShadow;
            this.Top = gameInfo.Rect.Top + WinTitileHeight;
            this.Width = rectClient.Right;
            this.Height = rectClient.Bottom;
            //if (gameInfo.Height > WinTitileHeight + WinShadow)
            //    this.Height = gameInfo.Height - WinShadow - WinTitileHeight;
            // hand by hand locate position
            //this.Left = gameInfo.Rect.Left + WinShadow;
            //this.Top = gameInfo.Rect.Top + WinTitileHeight;
            //this.Width = gameInfo.Width - WinShadow * 2;
            //if (gameInfo.Height > WinTitileHeight + WinShadow)
            //    this.Height = gameInfo.Height - WinShadow - WinTitileHeight;
        }

        /// <summary>
        /// WinEventDelegate 委托回调的实现
        /// </summary>
        /// <param name="hWinEventHook"></param>
        /// <param name="eventType"></param>
        /// <param name="hWnd"></param>
        /// <param name="idObject"></param>
        /// <param name="idChild"></param>
        /// <param name="dwEventThread"></param>
        /// <param name="dwmsEventTime"></param>
        protected void WinEventCallback(IntPtr hWinEventHook,
                                        Hook.SWEH_Events eventType,
                                        IntPtr hWnd,
                                        Hook.SWEH_ObjectId idObject,
                                        long idChild,
                                        uint dwEventThread,
                                        uint dwmsEventTime)
        {
            // 找到指定hWnd
            if (hWnd == GameInfo.Instance.hWnd &&
                eventType == Hook.SWEH_Events.EVENT_OBJECT_LOCATIONCHANGE &&
                idObject == (Hook.SWEH_ObjectId)Hook.SWEH_CHILDID_SELF)
            {
                SetLocation();
            }
        }

        // 游戏窗口关闭时调用
        private void GameExited(object sender, EventArgs e)
        {
            GCSafetyHandle.Free();
            Hook.WinEventUnhook(hWinEventHook);
            Dispatcher.BeginInvoke(new Action(() => { this.Close(); }));

            log.Info("Detected game quit.");
        }

        // TODO: Should move to viewmodel, this maybe a command
        // command -> (viewmodel -> model -> viewmodel) -> view
        private async void TextInput_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DisplayTextBox.SelectedText.Trim() != "" && DisplayTextBox.SelectionLength != 0)
            {
                WordCard.IsOpen = true;

                WordInfo word = new WordInfo();

                word = await Translate.Search(DisplayTextBox.SelectedText.Trim());

                (DataContext as TextWindowViewModel).Pron = word.Pron;
                (DataContext as TextWindowViewModel).Word = word.Original;
                (DataContext as TextWindowViewModel).Trans = word.Trans;
            }
        }
    }
}
