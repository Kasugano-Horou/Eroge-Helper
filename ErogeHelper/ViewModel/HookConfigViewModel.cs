using CommonServiceLocator;
using ErogeHelper.Common;
using ErogeHelper.Common.Validation;
using ErogeHelper.Model;
using ErogeHelper.Service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using log4net;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.TextFormatting;
using System.Xml.Linq;

namespace ErogeHelper.ViewModel
{
    public class HookConfigViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HookConfigViewModel));

        private readonly IHookConfigDataService _dataService;

        #region Constructor
        public HookConfigViewModel(IHookConfigDataService dataService)
        {
            _dataService = dataService;
            HookMapData = _dataService.GetHookMapData();

            if (IsInDesignMode)
            {
                InputCode = "/HS-10@21967:NUKITASHI2.EXE";
            }
            else
            {
                // initialize
                InsertCodeCommand = new RelayCommand(() => Textractor.InsertHook(InputCode), CanInsertCode);
                SelectedHookChangeCommand = new RelayCommand(() => {
                    log.Info($"Select hook {SelectedHook.Name}");
                    SelectedText = SelectedHook.Text;
                });
                SubmitCommand = new RelayCommand(SubmitMessage, CanSubmitMessage);

                Textractor.DataEvent += DataRecvEventHandler;
            }
        }
        #endregion

        #region HookCode
        public string InputCode { get; set; } = "";
        public RelayCommand InsertCodeCommand { get; private set; }
        public bool InvalidHookCood { get; set; }
        private bool CanInsertCode() => InputCode != "" && !InvalidHookCood;
        #endregion

        #region Regexp
        public string Regexp { get; set; } = SimpleIoc.Default.GetInstance<GameInfo>().Regexp;
        public bool InvalidRegexp { get; set; }
        private string selectedText;
        public string SelectedText
        { 
            get => selectedText;
            set 
            {
                string tmp = value;
                if (Regexp != null && !InvalidRegexp)
                {
                    // value with regexp
                    var list = Regex.Split(tmp, Regexp);
                    tmp = string.Join("", list);
                }
                if (tmp == "") return;

                selectedText = tmp;
                RaisePropertyChanged(() => SelectedText);
            }
        }
        #endregion

        #region HookParam Data
        public HookBindingList<long, HookParam> HookMapData { get; set; }
        private string consoleOutput;
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
                    string tmp = targetItem.TotalText + "\n\n" + hp.Text;

                    // dummy way with my TextBlock item
                    var count = tmp.Count(f => f == '\n');
                    if (count > 5)
                    {
                        var index = tmp.IndexOf('\n') + 2;
                        index = tmp.IndexOf('\n', index);
                        tmp = tmp.Substring(index);
                    }
                    targetItem.TotalText = tmp;
                }

                if (SelectedHook != null && hp.Handle == SelectedHook.Handle)
                {
                    SelectedText = hp.Text;
                }
            });
        }

        public HookParam SelectedHook { get; set; } = null;
        public RelayCommand SelectedHookChangeCommand { get; set; }
        #endregion

        #region SubmitCommand
        public RelayCommand SubmitCommand { get; private set; }

        private bool CanSubmitMessage() => SelectedHook != null && !InvalidRegexp;

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
                    Regexp = Regexp,
                });
            }
            else
            {
                // load and write
                var root = XElement.Load(gameInfo.ConfigPath).Elements("Profile");
                // update 4 node
                // TODO
                // write to file
                EHConfig.NewWriteConfig(root);
            }

            gameInfo.HookCode = SelectedHook.Hookcode;
            gameInfo.ThreadContext = SelectedHook.Ctx;
            gameInfo.SubThreadContext = SelectedHook.Ctx2;
            gameInfo.Regexp = Regexp;

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
