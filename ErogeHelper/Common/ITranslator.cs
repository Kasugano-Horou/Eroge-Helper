using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common
{
    interface ITranslator
    {
        Task<string> Translate(string sourceText, Language srcLang, Language desLang, params string[] list);
    }

    public enum Language
    {
        Auto,
        ChineseSimplified,   
        Japenese,
    }
}
