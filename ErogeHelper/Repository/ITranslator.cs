using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Repository
{
    public interface ITranslator
    {
        public string GetLastError();
        public string GetTranslatorName();
        /// <summary>
        /// <para>return <strong>string.Empty</strong> means this task has been canceled</para>
        /// <para>return <strong>null</strong> means error</para>
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="srcLang"></param>
        /// <param name="desLang"></param>
        /// <returns></returns>
        Task<string> Translate(string sourceText, Language srcLang, Language desLang);
    }

    public enum Language
    {
        Auto,
        ChineseSimplified,   
        Japenese,
    }
}
