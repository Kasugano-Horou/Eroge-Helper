using ErogeHelper.ViewModels;
using System.Windows;

namespace ErogeHelper.Views
{
    /// <summary>
    /// GameWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GameWindow : Window
    {
        public GameWindow()
        {
            InitializeComponent();

            DataContext = new GameWindowViewModel();
            IsEnabledChanged += GameWindow_IsEnabledChanged;
        }

        private void GameWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                Close();
            }
        }
    }
}
