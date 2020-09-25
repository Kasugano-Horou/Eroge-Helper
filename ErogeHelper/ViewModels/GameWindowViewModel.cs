using ErogeHelper.Models;
using ErogeHelper.Utils;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ErogeHelper.ViewModels
{
    class GameWindowViewModel : ObservableObject
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TextWindow));

        private GameInfo gameInfo = GameInfo.Instance;

        /// <summary>
        /// Constructor of GameWindowViewModel, set hook for the window
        /// </summary>
        public GameWindowViewModel()
        {
            TopMost = true;

            SetGameWindowHook();

            DisplayTextBlock = new ObservableCollection<RubyTextViewModel>();

            Textractor.SelectedDataEvent += SelectedDataEventHandler;

        }

        private MecabHelper _mecabHelper = new MecabHelper();

        private void SelectedDataEventHandler(object sender, HookParam hp)
        {
            SynchronizationContext.SetSynchronizationContext(new
                System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
            SynchronizationContext.Current.Post(pl =>
            {
                LineHeightOfText = hp.text.Length;

                DisplayTextBlock.Clear();

                var mecabWordList = _mecabHelper.SentenceHandle(hp.text);
                foreach (MecabWordInfo mecabWord in mecabWordList)
                {
                    if (mecabWord.PartOfSpeech == "名詞" ||
                        mecabWord.PartOfSpeech == "助詞" ||
                        mecabWord.PartOfSpeech == "動詞") // bad
                    {
                        DisplayTextBlock.Add(new RubyTextViewModel { RubyText = mecabWord.Kana, 
                                                                     Text = mecabWord.Word,
                                                                     canSearch = true});

                    }
                    else
                    {
                        DisplayTextBlock.Add(new RubyTextViewModel { RubyText = "", 
                                                                     Text = mecabWord.Word,
                                                                     canSearch = false});
                    }
                }

            }, null);
        }

        #region Window Follow Hook
        protected Hook.WinEventDelegate WinEventDelegate;
        private static GCHandle GCSafetyHandle; // 需回收释放
        private IntPtr hWinEventHook;   // 同上

        private void SetGameWindowHook()
        {
            WinEventDelegate = new Hook.WinEventDelegate(WinEventCallback);
            GCSafetyHandle = GCHandle.Alloc(WinEventDelegate);
            try
            {
                log.Info("Start find game's window.");

                // FIXME: 这一句引发 xaml DataContext 的“未将对象应用设置到实例”
                //Process targetProc = gameInfo.ProcList[0];
                Process targetProc = null;
                foreach (Process proc in gameInfo.ProcList)
                {
                    if (!proc.HasExited)
                    {
                        targetProc = proc;
                    }
                }
                //targetProc.WaitForInputIdle(7000);

                // 设置进程终止时触发的事件
                targetProc.EnableRaisingEvents = true;
                targetProc.Exited += new EventHandler(GameExited);

                // 设置窗口跟随相关钩子
                if (targetProc != null)
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    while (targetProc.MainWindowHandle == IntPtr.Zero)
                        Thread.Sleep(100);
                    //targetProc.Refresh(); 丢弃数据
                    log.Info($"Spend {sw.Elapsed.TotalSeconds}s to find game window's handle");
                    sw.Stop();

                    gameInfo.hWnd = targetProc.MainWindowHandle;
                    Debug.Assert(gameInfo.hWnd != (IntPtr)0);
                    uint targetThreadId = Hook.GetWindowThread(gameInfo.hWnd);

                    if (gameInfo.hWnd != IntPtr.Zero)
                    {
                        // 调用 SetWinEventHook 传入 WinEventDelegate 回调函数
                        hWinEventHook = Hook.WinEventHookOne(Hook.SWEH_Events.EVENT_OBJECT_LOCATIONCHANGE,
                                                             WinEventDelegate,
                                                             (uint)targetProc.Id,
                                                             targetThreadId);
                        // 首次设置窗口位置
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

        private void SetLocation()
        {
            var rect = Hook.GetWindowRect(gameInfo.hWnd);
            var rectClient = Hook.GetClientRect(gameInfo.hWnd);
            gameInfo.Width = rect.Right - rect.Left;  // rectClient.Right + shadow*2
            gameInfo.Height = rect.Bottom - rect.Top; // rectClient.Bottom + shadow + title

            WinShadow = (gameInfo.Width - rectClient.Right) / 2;
            var wholeHeight = rect.Bottom - rect.Top;
            WinTitleHeight = wholeHeight - rectClient.Bottom - WinShadow;

            ClientAreaMargin = new Thickness(WinShadow, WinTitleHeight, WinShadow, WinShadow);

            WinLeft = rect.Left;
            WinTop = rect.Top;
            WinWidth = gameInfo.Width;
            WinHeight = gameInfo.Height;
        }

        /// <summary>
        /// WinEventDelegate 委托回调的实现
        /// </summary>
        protected void WinEventCallback(IntPtr hWinEventHook,
                                    Hook.SWEH_Events eventType,
                                    IntPtr hWnd,
                                    Hook.SWEH_ObjectId idObject,
                                    long idChild,
                                    uint dwEventThread,
                                    uint dwmsEventTime)
        {
            // 仅获取焦点时调用
            //if (hWnd == GameInfo.Instance.hWnd &&
            //    eventType == Hook.SWEH_Events.EVENT_OBJECT_FOCUS)
            //{
            //    Console.WriteLine("get foucus");
            //}
            // 更新窗口信息
            if (hWnd == GameInfo.Instance.hWnd &&
                eventType == Hook.SWEH_Events.EVENT_OBJECT_LOCATIONCHANGE &&
                idObject == (Hook.SWEH_ObjectId)Hook.SWEH_CHILDID_SELF)
            {
                SetLocation();
            }
        }
        #endregion

        /// <summary>
        /// 游戏窗口关闭时调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameExited(object sender, EventArgs e)
        {
            log.Info("Detected game quit.");
            GCSafetyHandle.Free();
            Hook.WinEventUnhook(hWinEventHook);

            //Dispatcher.BeginInvoke(new Action(() => { window.Close(); }));
            //Window window = Application.Current.Windows.OfType<Window>().Where(w => w.Name == "GameWindow").FirstOrDefault();
            WindowSwitch = false;
        }

        #region Properties
        private int _winLeft;
        public int WinLeft 
        {
            get => _winLeft;
            set
            {
                _winLeft = value;
                RaisePropertyChangedEvent(nameof(WinLeft));
            } 
        }
        private int _winTop;
        public int WinTop
        {
            get => _winTop;
            set
            {
                _winTop = value;
                RaisePropertyChangedEvent(nameof(WinTop));
            }
        }
        private int _winWidth;
        public int WinWidth
        {
            get => _winWidth;
            set
            {
                _winWidth = value;
                RaisePropertyChangedEvent(nameof(WinWidth));
            }
        }
        private int _winHeight;
        public int WinHeight
        {
            get => _winHeight;
            set
            {
                _winHeight = value;
                RaisePropertyChangedEvent(nameof(WinHeight));
            }
        }

        private bool _isEnable = true;
        public bool WindowSwitch
        {
            get
            {
                return _isEnable;
            }

            set
            {
                _isEnable = value;
                RaisePropertyChangedEvent(nameof(WindowSwitch));
            }
        }

        private int _winTitleHeight;
        public int WinTitleHeight 
        {
            get => _winTitleHeight; 
            set
            {
                _winTitleHeight = value;
                RaisePropertyChangedEvent(nameof(WinTitleHeight));
            }
        }
        private int _winShadow;
        public int WinShadow 
        {
            get => _winShadow;
            set
            {
                _winShadow = value;
                RaisePropertyChangedEvent(nameof(WinShadow));
            }
        }
        private Thickness _clientAreaMargin;
        public Thickness ClientAreaMargin
        {
            get => _clientAreaMargin;
            set
            {
                _clientAreaMargin = value;
                RaisePropertyChangedEvent(nameof(ClientAreaMargin));
            }
        }
        private bool _topMost;
        public bool TopMost
        {
            get => _topMost;
            set
            {
                _topMost = value;
                RaisePropertyChangedEvent(nameof(TopMost));
            }
        }
        #endregion

        public ObservableCollection<RubyTextViewModel> DisplayTextBlock { get; set; }
        public ICommand AddText { get; }

        private int _lineHeightOfText;
        public int LineHeightOfText 
        {
            get => _lineHeightOfText;
            set
            {
                // value is lenth of text
                int line = value / 27; // line == 0 => 1 line
                                       // line == 1 => 2 line
                int height = 45 + line * 55;


                _lineHeightOfText = height;
                RaisePropertyChangedEvent(nameof(LineHeightOfText));
            } 
        }

        public ICommand WordQuery
        {
            get
            {
                return new DelegateCommand(WordSearch);
            }
        }

        private void WordSearch()
        {
            log.Info("查询单词");
        }
    }

    public class RubyTextViewModel
    {
        //Properties for Binding to Combobox and Textbox goes here
        public string RubyText { get; set; }
        public string Text { get; set; }
        public bool canSearch { get; set; }
    }
}
