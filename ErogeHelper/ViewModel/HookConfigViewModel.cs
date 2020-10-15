using ErogeHelper.Common;
using ErogeHelper.Model;
using ErogeHelper.Model.Singleton;
using ErogeHelper.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using log4net;
using System;

namespace ErogeHelper.ViewModel
{
    public class HookConfigViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HookConfigViewModel));

        private static readonly GameInfo gameInfo = GameInfo.Instance;

        public HookConfigViewModel()
        {
            if (IsInDesignMode)
            {
                HookMapData = new HookBindingList<long, HookParam>(p => p.Handle);
                HookParam hp = new HookParam()
                {
                    Addr = 0,
                    Ctx = 0,
                    Ctx2 = 0,
                    Handle = 1,
                    Hookcode = "e@e.exe",
                    Name = "engine",
                    Pid = 10000,
                    Text = "Text is me"
                };
                HookMapData.Add(hp);
            }
            else
            {
                HookMapData = new HookBindingList<long, HookParam>(p => p.Handle);
                SubmitCommand = new RelayCommand(SubmitMessage, CanSubmitMessage);

                Textractor.DataEvent += DataRecvEventHandler;
                TextractorLib.DataEvent += DataRecvEventHandler;
            }

        }

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

        public HookBindingList<long, HookParam> HookMapData { get; set; }
        public HookParam SelectedHook { get; set; } = null;
        public RelayCommand SubmitCommand { get; private set; }

        private bool CanSubmitMessage() => SelectedHook != null;

        private void SubmitMessage()
        {
            log.Info($"Selected Hook: {SelectedHook.Hookcode}");

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

            Messenger.Default.Send(new NotificationMessage("ShowGameView"));
        }
    }
}
