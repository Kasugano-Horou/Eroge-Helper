using ErogeHelper.Common;
using ErogeHelper.View;
using GalaSoft.MvvmLight;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                // bool(exStyle & win32con.WS_EX_NOACTIVATE) 窗口忽视焦点开关状态
                if (value) HandleCheckedEvent();
                else HandleUnCheckedEvent();

                noFocusToggel = value;
                EHConfig.SetValue(EHNode.NoFocus, value.ToString());
                RaisePropertyChanged(() => NoFocusToggel);
                log.Info($"Set TextWindow to {noFocusToggel}");
            }
        }

        private void HandleUnCheckedEvent()
        {
            var window = Application.Current.Windows.OfType<GameView>().FirstOrDefault();

            var interopHelper = new WindowInteropHelper(window);
            int exStyle = Hook.GetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE);
            Hook.SetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE, exStyle & ~Hook.WS_EX_NOACTIVATE);
        }

        private void HandleCheckedEvent()
        {
            var window = Application.Current.Windows.OfType<GameView>().FirstOrDefault();

            var interopHelper = new WindowInteropHelper(window);
            int exStyle = Hook.GetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE);
            Hook.SetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE, exStyle | Hook.WS_EX_NOACTIVATE);
        }
    }
}
