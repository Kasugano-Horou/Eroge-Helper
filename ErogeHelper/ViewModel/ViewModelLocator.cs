/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:ErogeHelper"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using CommonServiceLocator;
using ErogeHelper.Model;
using ErogeHelper.Service;

namespace ErogeHelper.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
                SimpleIoc.Default.Register<IHookConfigDataService, DesignHookConfigDataService>();
                SimpleIoc.Default.Register<IGameDataService, DesignGameDataService>();
            }
            else
            {
                // Create run time view services and models
                SimpleIoc.Default.Register<IHookConfigDataService, HookConfigDataService>();
                SimpleIoc.Default.Register<IGameDataService, GameDataService>();
            }

            SimpleIoc.Default.Register<GameViewModel>();
            SimpleIoc.Default.Register<HookConfigViewModel>();
            SimpleIoc.Default.Register<NotifyIconViewModel>();
            SimpleIoc.Default.Register<PreferenceViewModel>();
        }

        public GameViewModel Game
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GameViewModel>();
            }
        }
        public HookConfigViewModel HookConfig
        {
            get
            {
                return ServiceLocator.Current.GetInstance<HookConfigViewModel>();
            }
        }
        public NotifyIconViewModel NotifyIcon
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NotifyIconViewModel>();
            }
        }
        public PreferenceViewModel Preference
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PreferenceViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}