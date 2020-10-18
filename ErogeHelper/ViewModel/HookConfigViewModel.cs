using CommonServiceLocator;
using ErogeHelper.Common;
using ErogeHelper.Model;
using ErogeHelper.Service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using log4net;

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

        public HookBindingList<long, HookParam> HookMapData { get; set; }

        private void DataRecvEventHandler(object sender, HookParam hp)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var targetItem = HookMapData.FastFind(hp.Handle);
                if (targetItem == null)
                {
                    HookMapData.Insert(0, hp);
                }
                else
                {
                    HookMapData.Remove(targetItem);
                    HookMapData.Insert(0, hp);
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

            // write xml file
            EHConfig.WriteConfig(gameInfo.ConfigPath, new EHProfile()
            {
                HookCode = SelectedHook.Hookcode,
                MD5 = gameInfo.MD5,
                ThreadContext = SelectedHook.Ctx,
                SubThreadContext = SelectedHook.Ctx2,
                Name = gameInfo.ProcessName + ".eh.config",
            });

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
