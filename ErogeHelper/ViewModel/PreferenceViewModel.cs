using ErogeHelper.Common;
using ErogeHelper.Model.Singleton;
using ErogeHelper.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using log4net;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace ErogeHelper.ViewModel
{
    public class PreferenceViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PreferenceViewModel));

        private bool noFocusToggel;

        public bool NoFocusToggel
        {
            get
            {
                // this try catch do upward compatible?
                // No, must also do it with GameView code behind OnSourceInitialized
                try
                {
                    var ret = EHConfig.GetValue(EHNode.NoFocus);
                    noFocusToggel = bool.Parse(ret);
                }
                catch (NullReferenceException)
                {
                    // create the node
                    noFocusToggel = false;
                    EHConfig.SetValue(EHNode.NoFocus, noFocusToggel.ToString());
                }
                return noFocusToggel;
            }
            set
            {
                var window = Application.Current.Windows.OfType<GameView>().FirstOrDefault();
                var interopHelper = new WindowInteropHelper(window);
                int exStyle = Hook.GetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE);
                Hook.SetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE, value ? exStyle | Hook.WS_EX_NOACTIVATE
                                                                                 : exStyle & ~Hook.WS_EX_NOACTIVATE);
                noFocusToggel = value;
                EHConfig.SetValue(EHNode.NoFocus, value.ToString());
                RaisePropertyChanged(() => NoFocusToggel);
                log.Info($"Set TextWindow statu to {(noFocusToggel ? "NoFocus" : "Normal")}");
            }
        }

        public AppSetting Setting { get; set; } = SimpleIoc.Default.GetInstance<AppSetting>();

        // 拨动开关 先执行set 后 get
        public bool MachineTransMode 
        {
            get 
            {
                if (Setting.MachineTransleVisible == Visibility.Visible) return false;
                else return true;
            }
            set 
            {
                Setting.MachineTransleVisible = value ? Visibility.Collapsed : Visibility.Visible;
                NoFocusToggel = value;
                log.Info($"MachineTranslate Only: {value}");
            }
        }
    }
}
