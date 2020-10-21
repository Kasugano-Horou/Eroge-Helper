using ErogeHelper.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Service
{
    class DesignGameDataService : IGameDataService
    {
        public ObservableCollection<SingleTextItem> InitTextData(TextTemplateType type)
        {
            var DisplayTextCollection = new ObservableCollection<SingleTextItem>();

            // 悠真(ユウマ)くんを攻略(コウリャク)すれば２１０円(エン)か。なるほどなぁ…
            #region Render Model
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "ユウマ",
                Text = "悠真",
                TextTemplateType = type,
                PartOfSpeed = "名詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "くん",
                TextTemplateType = type,
                PartOfSpeed = "名詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "を",
                TextTemplateType = type,
                PartOfSpeed = "助詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "コウリャク",
                Text = "攻略",
                TextTemplateType = type,
                PartOfSpeed = "名詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "すれ",
                TextTemplateType = type,
                PartOfSpeed = "動詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "ば",
                TextTemplateType = type,
                PartOfSpeed = "助詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "２",
                TextTemplateType = type,
                PartOfSpeed = "名詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "１",
                TextTemplateType = type,
                PartOfSpeed = "名詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "０",
                TextTemplateType = type,
                PartOfSpeed = "名詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "エン",
                Text = "円",
                TextTemplateType = type,
                PartOfSpeed = "名詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "か",
                TextTemplateType = type,
                PartOfSpeed = "助詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "。",
                TextTemplateType = type,
                PartOfSpeed = "記号"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "なるほど",
                TextTemplateType = type,
                PartOfSpeed = "感動詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "なぁ",
                TextTemplateType = type,
                PartOfSpeed = "助詞"
            });
            DisplayTextCollection.Add(new SingleTextItem
            {
                RubyText = "",
                Text = "…",
                TextTemplateType = type,
                PartOfSpeed = "記号"
            });
            #endregion

            return DisplayTextCollection;
        }
    }
}
