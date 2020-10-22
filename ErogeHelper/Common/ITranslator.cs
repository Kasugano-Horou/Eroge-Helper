using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.Common
{
    interface ITranslator
    {
        Task<string> Translate(string sourceText, string srcLang, string desLang, params string[] list);
    }
}
