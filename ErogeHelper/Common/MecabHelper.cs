
using MeCab;
using System.Collections.Generic;
using WanaKanaSharp;

namespace ErogeHelper.Common
{
    class MecabHelper
    {
        private readonly MeCabParam parameter;
        private readonly MeCabTagger tagger;

        public MecabHelper()
        {
            parameter = new MeCabParam();
            tagger = MeCabTagger.Create(parameter);
        }

        /// <summary>
        /// Progress sentence
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<MecabWordInfo> SentenceHandle(string sentence)
        {

            List<MecabWordInfo> ret = new List<MecabWordInfo>();

            foreach (var node in tagger.ParseToNodes(sentence))
            {
                if (node.CharType > 0)
                {
                    var features = node.Feature.Split(',');

                    #region 填充 MecabWordInfo 各项 Property
                    MecabWordInfo word = new MecabWordInfo
                    {
                        Word = node.Surface,
                        PartOfSpeech = features[0],
                        Description = features[1],
                        Feature = node.Feature
                    };
                    // 加这一步是为了防止乱码进入分词导致无法读取假名
                    if (features.Length >= 8)
                    {
                        word.Kana = features[7];
                    }
                    // 清理不需要的假名
                    if (word.PartOfSpeech == "記号")
                    {
                        word.Kana = "";
                    }

                    if (WanaKana.IsHiragana(node.Surface))
                    {
                        word.Kana = "";
                    }
                    if (WanaKana.IsKatakana(node.Surface))
                    {
                        word.Kana = "";
                    }
                    #endregion

                    ret.Add(word);
                }
            }

            return ret;
        }
    }

    public struct MecabWordInfo
    {

        /// <summary>
        /// 单词
        /// </summary>
        public string Word;

        /// <summary>
        /// 品詞（ひんし）
        /// 诸如 名詞 助詞 動詞 助動詞 記号 副詞
        /// </summary>
        public string PartOfSpeech;

        /// <summary>
        /// 词性说明
        /// </summary>
        public string Description;

        /// <summary>
        /// 假名
        /// </summary>
        public string Kana;

        /// <summary>
        /// Mecab能提供的关于这个词的详细信息 CSV表示
        /// </summary>
        public string Feature;
    }

    public enum Hinshi
    {
        名詞,
        助詞,
        動詞,
        助動詞,
        記号,
        副詞
    }
}
