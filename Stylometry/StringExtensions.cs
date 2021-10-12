using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylometry
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to a <see cref="List{T}"/> of <see cref="Rune"/>s 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static List<Rune> ToRuneList(this string val)
        {
            var result = new List<Rune>();
            foreach (var x in val.EnumerateRunes())
            {
                result.Add(x);
            }
            return result;
        }
        public static List<Rune> ToRuneListWithDictionary(this string val, ref Dictionary<UnicodeCategory, int> dict)
        {
            var result = new List<Rune>();
            foreach(var x in Enum.GetValues(typeof(UnicodeCategory)))
            {
                dict[(UnicodeCategory)x] = 0;
            }
            foreach (var x in val.EnumerateRunes())
            {
                dict[CharUnicodeInfo.GetUnicodeCategory(x.Value)]++;
                result.Add(x);
            }
            return result;
        }

        /// <summary>
        /// Creates a shorthand form of string, removing any spaces and writing at most <paramref name="initialCount"/> characters
        /// after each capital letter (including)
        /// <para>throws <see cref="ArgumentException"/> if <paramref name="initialCount"/> is less than 1</para>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="initialCount"></param>
        /// <returns></returns>
        public static string ToInitials(this string val, int initialCount = 2)
        {
            if (string.IsNullOrEmpty(val)) return val;
            if (initialCount < 1) throw new ArgumentException("initialCount cannot be less than 1");
            var resultString = "";
            var lower = val.ToLower();
            if (val == lower) return val.Substring(0, val.Length < initialCount ? val.Length : initialCount);
            int toAppend = initialCount;
            for (int i = 0; i < val.Length; i++)
            {
                if (lower[i] != val[i])
                {
                    resultString += val[i];
                    toAppend--;
                    i++;
                    while(toAppend != 0 && i < val.Length && lower[i] == val[i])
                    {
                        if (val[i] != ' ')
                        {
                            toAppend--;
                            resultString += val[i];
                            i++;
                        } 
                        else
                        {
                            break;
                        }
                    }
                    toAppend = initialCount;
                }
            }
            return resultString;
        }
    }
}
