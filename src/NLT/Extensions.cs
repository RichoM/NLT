using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalLanguageTranslator
{
    public static class Extensions
    {
        public static string Translated(this string original)
        {
            return NLT.Default.Translate(original);
        }
    }
}
