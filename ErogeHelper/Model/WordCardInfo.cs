using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace ErogeHelper.Model
{
    public class WordCardInfo : ViewModelBase
    {
        // Properties
        private bool isProcess; // procssing flag
        private string ruby;
        private string hinshi; // 品詞
        private string word;
        private ObservableCollection<string> kaisetsu = new ObservableCollection<string>();

        public string Word { get => word; set { word = value; RaisePropertyChanged(nameof(word)); } }
        public string Hinshi { get => hinshi; set { hinshi = value; RaisePropertyChanged(nameof(hinshi)); } }
        public string Ruby { get => ruby; set { ruby = value; RaisePropertyChanged(nameof(ruby)); } }
        public ObservableCollection<string> Kaisetsu { get => kaisetsu; set { kaisetsu = value; RaisePropertyChanged(nameof(kaisetsu)); } }
        public bool IsProcess { get => isProcess; set { isProcess = value; RaisePropertyChanged(nameof(isProcess)); } }
    }
}
