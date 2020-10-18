using ErogeHelper.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Linq;
using System.Windows;

namespace ErogeHelper.ViewModel
{
    public class NotifyIconViewModel : ViewModelBase
    {
        public RelayCommand ShowHookConfigCommand
        {
            get
            {
                return new RelayCommand(() => {
                    var window = Application.Current.Windows.OfType<HookConfigView>().FirstOrDefault();
                    if (window == null)
                        new HookConfigView().Show();
                    else
                    {
                        window.Activate();
                    }
                });
            }
        }

        /// <summary>
        /// (No Need かも)Shuts down the application.
        /// </summary>
        public RelayCommand ExitApplicationCommand
        {
            get
            {
                return new RelayCommand(()=> {
                    Application.Current.Shutdown();
                });
            }
        }
    }
}
