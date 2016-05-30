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

        internal static IEnumerable<IEnumerable<T>> ToEnumerable<T>(this T[,] array)
        {
            List<List<T>> result = new List<List<T>>();
            for(int i = 0; i < array.GetLength(0); i++)
            {
                List<T> row = new List<T>();
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    row.Add(array[i, j]);
                }
                result.Add(row);
            }
            return result;
        }
    }
}
