using ErogeHelper.Common;
using System.Windows.Media;

namespace ErogeHelper.Model
{
    public class SingleTextItem
    {
        //Properties for Binding
        public string RubyText { get; set; }
        public string Text { get; set; }
        private string _partOfSpeed;
        public string PartOfSpeed
        {
            get => _partOfSpeed;
            set
            {
                switch (value.ToString())
                {
                    case "名詞":
                        SubMarkColor = Utils.LoadBitmapFromResource("Resource/yellow.png");
                        break;
                    case "助詞":
                        //SubMarkColor = Utils.LoadBitmapFromResource("Resource/aqua_green.png");
                        SubMarkColor = Utils.LoadBitmapFromResource("Resource/transparent.png");
                        // 不能点击的单词就不要上色了
                        break;
                    case "動詞":
                    case "感動詞":
                        SubMarkColor = Utils.LoadBitmapFromResource("Resource/green.png");
                        break;
                    case "副詞":
                        SubMarkColor = Utils.LoadBitmapFromResource("Resource/purple.png");
                        break;
                    case "形容詞":
                        SubMarkColor = Utils.LoadBitmapFromResource("Resource/pink.png");
                        break;
                    default:
                        SubMarkColor = Utils.LoadBitmapFromResource("Resource/transparent.png");
                        break;
                }

                _partOfSpeed = value;
            }
        }
        public TextTemplateType TextTemplateType { get; set; }
        public ImageSource SubMarkColor { get; set; }
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
