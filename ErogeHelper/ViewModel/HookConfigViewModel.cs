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

            }
            else
            {
                HookMapData = new HookBindingList<long, HookParam>(p => p.Handle);
                SubmitCommand = new RelayCommand(SubmitMessage, CanSubmitMessage);

                Textractor.DataEvent += DataRecvEventHandler;
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
            // FIXME: 用了try catch的地方
            try
            {
                EHConfig.WriteConfig(gameInfo.ConfigPath, new EHProfile()
                {
                    HookCode = SelectedHook.Hookcode,
                    MD5 = gameInfo.MD5,
                    ThreadContext = SelectedHook.Ctx,
                    Name = gameInfo.ProcessName + ".eh.config",
                    RepeatType = "",
                    RepeatTime = 0
                });
            }
            catch (Exception)
            {
                throw;
            }

            gameInfo.HookCode = SelectedHook.Hookcode;
            gameInfo.ThreadContext = SelectedHook.Ctx;

            Messenger.Default.Send(new NotificationMessage("ShowGameView"));
        }
    }
}
