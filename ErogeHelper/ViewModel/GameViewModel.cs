using ErogeHelper.Common;
using ErogeHelper.Model;
using ErogeHelper.Model.Singleton;
using ErogeHelper.Repository;
using ErogeHelper.Service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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
        private static readonly ILog log = LogManager.GetLogger(typeof(GameViewModel));

        public double MainHeight { get; set; }
        public double MainWidth { get; set; }
        public double MainLeft { get; set; }
        public double MainTop { get; set; }

        private readonly MecabHelper _mecabHelper;
        private readonly MojiDictApi _mojiHelper;
        private readonly BaiduWebTranslator _baiduHelper;

        private readonly IGameDataService _dataService;
        public ObservableCollection<SingleTextItem> DisplayTextCollection { get; set; }
        public TextTemplateType TextTemplateConfig { get; set; } = TextTemplateType.Default;

        // FIXME: learn new way
        // https://stackoverflow.com/questions/20099743/how-do-i-get-design-time-view-of-a-view-model-first-approach-with-ioc
        // This make design view but get wrong in Real mode
        //public GameViewModel() : this(new DesignGameDataService()) { }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public GameViewModel(IGameDataService dataService)
        {
            log.Info("Initialize");
            TextTemplateConfig = TextTemplateType.OutLineKanaBottom;

            _dataService = dataService;
            DisplayTextCollection = _dataService.InitTextData(TextTemplateConfig);

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                CardInfo = new WordCardInfo()
                {
                    Word = "買う",
                    Ruby = "かう",
                    IsProcess = false,
                    Hinshi = "動詞",
                    Kaisetsu = new ObservableCollection<string>()
                    {
                        "1. 多，多数，许多。（たくさん。多くのもの。）",
                        "2. 多半，大都。（ふつう。一般に。たいてい。）"
                    }
                };

                MainHeight = 800;
                MainWidth = 600;
            }
            else
            {
                // Code runs "for real"
                CardInfo = new WordCardInfo();
                _mecabHelper = new MecabHelper();
                _mojiHelper = new MojiDictApi();
                _baiduHelper = new BaiduWebTranslator();
                WordSearchCommand = new RelayCommand<SingleTextItem>(WordSearch, CanWordSearch);
                PopupCloseCommand = new RelayCommand(() => Messenger.Default.Send(new NotificationMessage("CloseCard")));
                PinCommand = new RelayCommand(() => TextPanelPin = !TextPanelPin);
                TranslateCommand = new RelayCommand(FakeDoTranslate);
                TranslateTextList = new ObservableCollection<string>();

                Textractor.SelectedDataEvent += SelectedDataEventHandler;
            }
        }

        #region Text Data Dispatch
        private string currentSentence = "";
        private string lastSentence = "";
        private void SelectedDataEventHandler(object sender, HookParam hp)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (!Properties.Settings.Default.OnlyMachineTranslation)
                {
                    DisplayTextCollection.Clear();

                    var pattern = SimpleIoc.Default.GetInstance<GameInfo>().Regexp;
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        var list = Regex.Split(hp.Text, pattern);
                        hp.Text = string.Join("", list);
                    }

                    if (hp.Text.Length > 80)
                    {
                        hp.Text = "长度大于80的文本自动跳过";
                    }

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

                    if (lastSentence == "")
                    {
                        currentSentence = hp.Text;
                    }
                    else
                    {
                        lastSentence = currentSentence;
                        currentSentence = hp.Text;
                    }

                    TransText = "";
                    TransTextVisible = Visibility.Collapsed;
                    DoPreTranslateAsync();
                }
                else
                {
                    TranslateTextList.Clear();
                    if (hp.Text.Length > 80)
                    {
                        TranslateTextList.Add("长度大于80的文本自动跳过");
                        return;
                    }

                    TrasnlateAllAsync(hp.Text, Utils.GetTranslatorList());
                }
            });
        }
        #endregion

        #region Right Toolbar
        // Can't be init in constructor
        public RelayCommand PinCommand { get; set; }
        private bool _textPanelPin;
        public bool TextPanelPin
        {
            get => _textPanelPin;
            set
            {
                if (value == true)
                {
                    Messenger.Default.Send(new NotificationMessage("MakeTextPanelPin"));
                }
                else
                {
                    Messenger.Default.Send(new NotificationMessage("CancelTextPanelPin"));
                }
                _textPanelPin = value;
            }
        }

        private Visibility transTextVisible;
        public Visibility TransTextVisible { get => transTextVisible; set { transTextVisible = value; RaisePropertyChanged(() => TransTextVisible); } }
        public RelayCommand TranslateCommand { get; set; }
        private string transText;
        public string TransText { get => transText; set { transText = value; RaisePropertyChanged(() => TransText); } }
        private void FakeDoTranslate()
        {
            //只做显示的操作
            TransTextVisible = Visibility.Visible;
        }
        private async void DoPreTranslateAsync()
        {
            // Make language dynamic, set by user, use setting properties?
            var result = await _baiduHelper.Translate(currentSentence, Language.Japenese, Language.ChineseSimplified);
            // Task canceled
            if (result == "") return;

            TransText = result == null ? _baiduHelper.GetLastError() : result;
            log.Info($"Get translate {TransText}");
        }
        #endregion

        #region MojiCard Search
        private WordCardInfo cardInfo;
        public WordCardInfo CardInfo { get => cardInfo; set { cardInfo = value; RaisePropertyChanged(nameof(cardInfo)); } }
        public RelayCommand<SingleTextItem> WordSearchCommand { get; private set; }
        public RelayCommand PopupCloseCommand { get; set; }

        private bool CanWordSearch(SingleTextItem item)
        {
            // If text background is transparent, then we don't click it 
            if (item.SubMarkColor.ToString()
                == Utils.LoadBitmapFromResource("Resource/transparent.png").ToString())
            {
                return false;
            }
            return true;
        }

        private async void WordSearch(SingleTextItem item)
        {
            log.Info($"Search \"{item.Text}\", partofspeech {item.PartOfSpeed} ");

            CardInfo = new WordCardInfo()
            {
                Word = item.Text,
                IsProcess = true
            };

            Messenger.Default.Send(new NotificationMessage("OpenCard"));

            var resp = await _mojiHelper.RequestAsync(item.Text);

            var result = resp.result;
            if (result != null)
            {
                log.Info($"Find explain <{result.word.excerpt}>");

                CardInfo.IsProcess = false;
                CardInfo.Word = result.word.spell;
                CardInfo.Hinshi = result.details[0].title;
                CardInfo.Ruby = result.word.pron;
                int count = 1;
                foreach (var subdetail in result.subdetails)
                {
                    CardInfo.Kaisetsu.Add($"{count++}. {subdetail.title}");
                }
            }
            else
            {
                CardInfo.IsProcess = false;
                CardInfo.Hinshi = "空空";
                CardInfo.Kaisetsu.Add("没有找到呀!");
            }
        }
        #endregion

        public AppSetting Setting { get; set; } = SimpleIoc.Default.GetInstance<AppSetting>();

        public ObservableCollection<string> TranslateTextList { get; set; }

        private async void TrasnlateAllAsync(string text, List<ITranslator> list)
        {
            await Task.Run(() => 
            { 
                foreach(var translator in list)
                {
                    TranslateAsync(translator, text);
                }
            });
        }

        private async void TranslateAsync(ITranslator translator, string text)
        {
            // 语言也通过properties获取？直接在内部
            var sw = new Stopwatch();
            sw.Start();
            var result = await translator.Translate(text, Language.Japenese, Language.ChineseSimplified);
            sw.Stop();
            if (result == "")
                return;
            //if (result == null)
            //    result = translator.GetLastError();
            log.Info($"[{sw.Elapsed.TotalSeconds:0.00}s][TranslatorName] {result}");

            DispatcherHelper.CheckBeginInvokeOnUI(() => TranslateTextList.Add(result) );
        }
    }
}