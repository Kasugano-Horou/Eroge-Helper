using ErogeHelper.Utils;
using ErogeHelper.ViewModels;
using log4net;
using System.Windows;
using System.Windows.Controls;

namespace ErogeHelper.Views
{
    /// <summary>
    /// GameWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GameWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GameWindow));

        public GameWindow()
        {
            InitializeComponent();

            DataContext = new GameWindowViewModel();
            IsEnabledChanged += GameWindow_IsEnabledChanged;

            mojiDict = new MojiDictApi();

        }

        private MojiDictApi mojiDict;

        private void GameWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                Close();
            }
        }

        private void TextPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextPanel.Visibility = Visibility.Collapsed;
            OpenPopupBorder.Visibility = Visibility.Visible;
        }

        private void OpenPopupBorder_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextPanel.Visibility = Visibility.Visible;
            OpenPopupBorder.Visibility = Visibility.Collapsed;
        }

        private async void TextBlock_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var control = (TextBlock)sender;
            
            log.Info($"点击 {control.Text}");
            WordCard.IsOpen = true;

            Word.Content = control.Text;
            Speech.Text = "";
            Pron.Text = "";
            Trans.Text = "";

            var mojiResp = await mojiDict.RequestAsync(control.Text);

            if (mojiResp.result != null)
            {
                Word.Content = $"{control.Text} => {mojiResp.result.word.spell}";
                Speech.Text = mojiResp.result.details[0].title;
                Pron.Text = mojiResp.result.word.pron;
                Trans.Text = mojiResp.result.subdetails[0].title;
            }
            else
            {
                Word.Content = $"{control.Text} 没有找到";
            }


            //WordInfo word = new WordInfo();

            //word = await Translate.Search(DisplayTextBox.SelectedText.Trim());

            //(DataContext as TextWindowViewModel).Pron = word.Pron;
            //(DataContext as TextWindowViewModel).Word = word.Original;
            //(DataContext as TextWindowViewModel).Trans = word.Trans;
        }
        // Will change all of this to MVVM in the future
    }
}
