using ErogeHelper.Models;
using ErogeHelper.Utils;

namespace ErogeHelper.ViewModels
{
    class TextWindowViewModel : ObservableObject
    {
        #region Private Members
        private MainText _mainText = new MainText();
        private string _word = null;
        private string _pron = null;
        private string _trans = null;
        #endregion

        public TextWindowViewModel()
        {
            _mainText.Text = "init";
            Textractor.SelectedDataEvent += DataRecvEventHandler;
        }

        private void DataRecvEventHandler(object sender, HookParam e)
        {
            Text = e.text;
        }

        public string Text
        {
            get => _mainText.Text;
            set
            {
                _mainText.Text = value;
                RaisePropertyChangedEvent("Text");
            }
        }

        // TODO: Change to WordCardModel
        public string Word
        {
            get => _word;
            set
            {
                _word = value;
                RaisePropertyChangedEvent("Word");
            }
        }

        public string Pron
        {
            get => _pron;
            set
            {
                _pron = value;
                RaisePropertyChangedEvent("Pron");
            }
        }

        public string Trans
        {
            get => _trans;
            set
            {
                _trans = value;
                RaisePropertyChangedEvent("Trans");
            }
        }
    }
}
