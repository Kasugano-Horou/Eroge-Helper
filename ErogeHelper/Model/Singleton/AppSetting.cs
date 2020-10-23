using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ErogeHelper.Model.Singleton
{
    public class AppSetting : ViewModelBase
    {
        public Visibility MachineTransleVisible 
        {
            get 
            {
                if (Properties.Settings.Default.OnlyMachineTranslation) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
            set
            {
                if (value == Visibility.Visible)
                    Properties.Settings.Default.OnlyMachineTranslation = false;
                else
                    Properties.Settings.Default.OnlyMachineTranslation = true;
                Properties.Settings.Default.Save();
                RaisePropertyChanged(()=>MachineTransleVisible);
            }
        }
    }
}
