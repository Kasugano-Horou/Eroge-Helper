using ErogeHelper.Model;
using System.Windows;
using System.Windows.Controls;

namespace ErogeHelper.Common.Selector
{
    class TextTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextDefaultTemplate { get; set; }
        public DataTemplate TextKanaTopTemplate { get; set; }
        public DataTemplate TextKanaBottomTemplate { get; set; }
        public DataTemplate OutLineDefaultTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // 可以通过container找keyname，也可以通过绑定的template直接返回
            if (container is FrameworkElement && item != null && item is SingleTextItem)
            {
                SingleTextItem textItem = item as SingleTextItem;

                switch (textItem.TextTemplateType)
                {
                    case TextTemplateType.Default:
                        return TextDefaultTemplate;
                    case TextTemplateType.KanaTop:
                        return TextKanaTopTemplate;
                    case TextTemplateType.KanaBottom:
                        return TextKanaBottomTemplate;
                    case TextTemplateType.OutLine:
                        return OutLineDefaultTemplate;
                }

            }

            return null;
        }
    }
}
