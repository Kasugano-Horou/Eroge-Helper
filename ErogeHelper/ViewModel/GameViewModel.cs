using ErogeHelper.Common;
using ErogeHelper.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using System.Collections.ObjectModel;
using System.Windows;

namespace ErogeHelper.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class GameViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public GameViewModel()
        {
            DisplayTextCollection = new ObservableCollection<SingleTextItem>();
            TextTemplateConfig = TextTemplateType.KanaBottom;

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                ClientAreaMargin = new Thickness(10, 30, 10, 10);
                TextAreaVisibility = Visibility.Visible;
                // 悠真(ユウマ)くんを攻略(コウリャク)すれば２１０円(エン)か。なるほどなぁ…
                #region Render Model
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "ユウマ",
                    Text = "悠真",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "名詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "くん",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "名詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "を",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "助詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "コウリャク",
                    Text = "攻略",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "名詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "すれ",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "動詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "ば",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "助詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "２",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "名詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "１",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "名詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "０",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "名詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "エン",
                    Text = "円",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "名詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "か",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "助詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "。",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "記号"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "なるほど",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "感動詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "なぁ",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "助詞"
                });
                DisplayTextCollection.Add(new SingleTextItem
                {
                    RubyText = "",
                    Text = "…",
                    TextTemplateType = TextTemplateConfig,
                    PartOfSpeed = "記号"
                });
                #endregion
            }
            else
            {
                // Code runs "for real"
                TextAreaVisibility = Visibility.Collapsed;
                Topmost = true;

                Textractor.SelectedDataEvent += SelectedDataEventHandler;
                _mecabHelper = new MecabHelper();

            }
        }

        private readonly MecabHelper _mecabHelper;

        private void SelectedDataEventHandler(object sender, HookParam hp)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                DisplayTextCollection.Clear();

                var mecabWordList = _mecabHelper.SentenceHandle(hp.Text);
                foreach (MecabWordInfo mecabWord in mecabWordList)
                {
                    DisplayTextCollection.Add(new SingleTextItem
                    {
                        Text = mecabWord.Word,
                        RubyText = mecabWord.Kana,
                        PartOfSpeed = mecabWord.PartOfSpeech,
                        TextTemplateType = TextTemplateConfig
                    });
                }
            });
        }

        public bool Topmost { get; set; }
        public Thickness ClientAreaMargin { get; set; }
        public Visibility TextAreaVisibility { get; set; }

        public ObservableCollection<SingleTextItem> DisplayTextCollection { get; set; }
        public TextTemplateType TextTemplateConfig { get; set; } = TextTemplateType.Default;

    }
}