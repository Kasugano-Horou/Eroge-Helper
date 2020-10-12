using System;
using System.Windows.Media;

namespace ErogeHelper.Model
{
    public class SingleTextItem
    {
        //Properties for Binding
        public string RubyText { get; set; }
        public string Text { get; set; }
        // 名詞 助詞 動詞 助動詞 記号 副詞
        private string _partOfSpeed;
        public string PartOfSpeed
        {
            get => _partOfSpeed;
            set
            {
                // 找到有哪些词性
                if (value.Equals(""))
                {
                    // TODO:
                    //SubMarkColor = new BitmapImage(new Uri(@"C:\Users\k1mlka\source\repos\luojunyuan\WpfPlayground\TextControlMVVMLight\Resources\notitle4.png", UriKind.Absolute));
                }

                _partOfSpeed = value;
            }
        }
        public TextTemplateType TextTemplateType { get; set; }
        public ImageSource SubMarkColor { get; internal set; }
    }

    public enum TextTemplateType
    {
        Default,
        KanaTop,
        KanaBottom,
        OutLine,
        OutLineKanaTop,
        OutLineKanaBottom
    }
}
