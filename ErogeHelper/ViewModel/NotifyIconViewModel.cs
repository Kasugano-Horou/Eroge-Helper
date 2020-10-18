using ErogeHelper.Model;
using ErogeHelper.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using System.IO;
using System.Linq;
using System.Windows;

namespace ErogeHelper.ViewModel
{
    public class NotifyIconViewModel : ViewModelBase
    {
        public NotifyIconViewModel()
        {
            ShowPreferenceCommand = new RelayCommand(() =>
            {
                var window = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault();
                if (window == null)
                    new PreferenceView().Show();
                else
                {
                    window.Activate();
                }
            }, File.Exists(SimpleIoc.Default.GetInstance<GameInfo>().ConfigPath));
        }

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

        public RelayCommand ShowPreferenceCommand { set; get; }

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
