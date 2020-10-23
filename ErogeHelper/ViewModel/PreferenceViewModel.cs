using ErogeHelper.Common;
using ErogeHelper.Model.Singleton;
using ErogeHelper.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ErogeHelper.ViewModel
{
    public class PreferenceViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PreferenceViewModel));

        public AppSetting Setting { get; set; } = SimpleIoc.Default.GetInstance<AppSetting>();

        public PreferenceViewModel()
        {
            Messenger.Default.Register<DataGridRow>(this, "PreferenceReceivedRow", e => ReceivedRow = e);

            TranslatorList = new ObservableCollection<TranslatorModel>
            {
                new TranslatorModel
                {
                    IsSelected = true,
                    TranslatorName = "百度",
                    SourceLanguage = "日语",
                    DestLanguage = "中文",
                },
                new TranslatorModel
                {
                    TranslatorName = "腾讯",
                    SourceLanguage = "日语",
                    DestLanguage = "中文"
                },
                new TranslatorModel
                {
                    TranslatorName = "有道",
                    SourceLanguage = "日语",
                    DestLanguage = "中文"
                }
            }; ;
        }

        private bool _noFocusToggel;

        public bool NoFocusToggel
        {
            get
            {
                // this try catch do upward compatible?
                // No, must also do it with GameView code behind OnSourceInitialized
                try
                {
                    var ret = EHConfig.GetValue(EHNode.NoFocus);
                    _noFocusToggel = bool.Parse(ret);
                }
                catch (NullReferenceException)
                {
                    // create the node
                    _noFocusToggel = false;
                    EHConfig.SetValue(EHNode.NoFocus, _noFocusToggel.ToString());
                }
                return _noFocusToggel;
            }
            set
            {
                var window = Application.Current.Windows.OfType<GameView>().FirstOrDefault();
                var interopHelper = new WindowInteropHelper(window);
                int exStyle = Hook.GetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE);
                Hook.SetWindowLong(interopHelper.Handle, Hook.GWL_EXSTYLE, value ? exStyle | Hook.WS_EX_NOACTIVATE
                                                                                 : exStyle & ~Hook.WS_EX_NOACTIVATE);
                _noFocusToggel = value;
                EHConfig.SetValue(EHNode.NoFocus, value.ToString());
                RaisePropertyChanged(() => NoFocusToggel);
                log.Info($"Set TextWindow statu to {(_noFocusToggel ? "NoFocus" : "Normal")}");
            }
        }

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

        public ObservableCollection<TranslatorModel> TranslatorList { get; }
        public TranslatorModel SelectedItem { get; set; }
        public DataGridRow ReceivedRow { get; set; }
        public bool ComboxPopStatu = false;
        public IEnumerable<string> SourceLanguages
        {
            get
            {
                if (ReceivedRow != null)
                {
                    TranslatorModel item = (TranslatorModel)ReceivedRow.Item;

                    string ret = ComboxPopStatu == false ? item.TranslatorName : SelectedItem.TranslatorName;
                    ComboxPopStatu = !ComboxPopStatu;
                    return ret switch
                    {
                        "百度" => new[] { "日语", "英语" },
                        "腾讯" => new[] { "日语" },
                        "有道" => new[] { "日语" },
                        _ => new[] { "" },
                    };
                }
                // DataGrid init each row
                return new[] { "日语" };
            }
        }
        public IEnumerable<string> DestLanguages 
        {
            get
            {
                if (ReceivedRow != null)
                {
                    TranslatorModel item = (TranslatorModel)ReceivedRow.Item;

                    string ret = ComboxPopStatu == false ? item.TranslatorName : SelectedItem.TranslatorName;
                    ComboxPopStatu = !ComboxPopStatu;
                    return ret switch
                    {
                        "百度" => new[] { "中文" },
                        "腾讯" => new[] { "中文" },
                        "有道" => new[] { "中文" },
                        _ => new[] { "" },
                    };
                }
                // DataGrid init each row
                return new[] { "中文" };
            }
        }
    }

    public class TranslatorModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TranslatorModel));

        private bool _isSelected;
        private string _translatorName;
        private string _sourceLaguages;
        private string _destLaguages;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
                log.Info("选择状态改变了。。");
            }
        }

        public string TranslatorName
        {
            get => _translatorName;
            set
            {
                _translatorName = value;
                RaisePropertyChanged(() => TranslatorName);
            }
        }

        public string SourceLanguage
        {
            get => _sourceLaguages;
            set
            {
                if (_sourceLaguages == value) return;
                _sourceLaguages = value;
                RaisePropertyChanged(() => SourceLanguage);
                log.Info("源语言改变了。。");
            }
        }

        public string DestLanguage
        {
            get => _destLaguages;
            set
            {
                if (_destLaguages == value) return;
                _destLaguages = value;
                RaisePropertyChanged(() => DestLanguage);
                log.Info("目标语言改变了。。");
            }
        }
    }
}
