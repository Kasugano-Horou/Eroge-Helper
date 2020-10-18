using CommonServiceLocator;
using ErogeHelper.Common;
using ErogeHelper.Model;
using ErogeHelper.Service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using log4net;
using System.IO;
using System.Linq;
using System.Windows.Media.TextFormatting;

namespace ErogeHelper.ViewModel
{
    public class HookConfigViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HookConfigViewModel));

        private readonly IHookConfigDataService _dataService;

        public HookConfigViewModel(IHookConfigDataService dataService)
        {
            _dataService = dataService;
            HookMapData = _dataService.GetHookMapData();

            if (IsInDesignMode) { }
            else
            {
                // initialize
                SubmitCommand = new RelayCommand(SubmitMessage, CanSubmitMessage);

                Textractor.DataEvent += DataRecvEventHandler;
            }
        }

        private string consoleOutput;
        public HookBindingList<long, HookParam> HookMapData { get; set; }
        public string ConsoleOutput { get => consoleOutput; set { consoleOutput = value; RaisePropertyChanged(() => ConsoleOutput); } }
        public string ClipboardOutput { get; set; }

        private void DataRecvEventHandler(object sender, HookParam hp)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (hp.Name == "控制台")
                {
                    ConsoleOutput += "\n" + hp.Text;
                    return;
                }
                else if (hp.Name == "剪贴板")
                {
                    // ClipboardOutput += "\n" + hp.Text;
                    return;
                }

                var targetItem = HookMapData.FastFind(hp.Handle);
                if (targetItem == null)
                {
                    hp.TotalText = hp.Text;
                    HookMapData.Add(hp);
                }
                else
                {
                    if (hp.Text.Length > 80)
                    {
                        hp.Text = "String.Length > 80. Skip";
                    }
                    string tmp = targetItem.TotalText + "\n\n" + hp.Text;

                    // dummy way
                    var count = tmp.Count(f => f == '\n');
                    if (count > 5)
                    {
                        var index = tmp.IndexOf('\n') + 2;
                        tmp = tmp.Substring(index);
                    }
                    targetItem.TotalText = tmp;
                }
            });
        }

        public HookParam SelectedHook { get; set; } = null;

        #region SubmitCommand
        public RelayCommand SubmitCommand { get; private set; }

        private bool CanSubmitMessage() => SelectedHook != null;

        private void SubmitMessage()
        {
            log.Info($"Selected Hook: {SelectedHook.Hookcode}");

            var gameInfo = (GameInfo)SimpleIoc.Default.GetInstance(typeof(GameInfo));
            if (!File.Exists(gameInfo.ConfigPath))
            {
                EHConfig.WriteConfig(gameInfo.ConfigPath, new EHProfile()
                {
                    Name = gameInfo.ProcessName + ".eh.config",
                    MD5 = gameInfo.MD5,

                    HookCode = SelectedHook.Hookcode,
                    ThreadContext = SelectedHook.Ctx,
                    SubThreadContext = SelectedHook.Ctx2,
                });
            }
            else
            {
                // load and write
            }

            gameInfo.HookCode = SelectedHook.Hookcode;
            gameInfo.ThreadContext = SelectedHook.Ctx;
            gameInfo.SubThreadContext = SelectedHook.Ctx2;

            if (WindowService != null)
                WindowService.OpenWindow();
        }

        private IWindowService WindowService
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IWindowService>();
            }
        }
        #endregion
    }
}
