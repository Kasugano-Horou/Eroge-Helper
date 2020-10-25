using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Repository
{
    public abstract class BaseTranslator
    {
        protected static CancellationTokenSource _cancelToken ;

        public abstract Task<string> Translate(string sourceText, Language srcLang, Language desLang);
    }
}
