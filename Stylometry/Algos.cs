using OpenNLP.Tools.Tokenize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylometry
{
    class Algos
    {
        static EnglishRuleBasedTokenizer tokenizer = new EnglishRuleBasedTokenizer(false);
        /// <summary>
        /// <para>1 = Hunspell</para>
        /// <para>Hunspell cannot parse emojis correctly and will throw <see cref="AccessViolationException"/>(s)</para>
        /// <para>2 = Symspell</para>
        /// </summary>
        public static int SpellerId = 1;

        /// <summary>
        /// Returns a <see cref="List{T}"/> containing a (WrongWords, Entry) pair with a 
        /// List of mispelled word pairs (Original, Fixed) for each <see cref="ExtendedArticleEntry"/>
        /// </summary>
        /// <param name="articleEntries"></param>
        /// <returns></returns>
        public static List<(List<(string Original, string Fixed)> WrongWords, ExtendedArticleEntry Entry)> GetErrors(List<ExtendedArticleEntry> articleEntries)
        {
            List<(List<(string, string)>, ExtendedArticleEntry)> errorList = new List<(List<(string, string)>, ExtendedArticleEntry)>(articleEntries.Count);
            foreach (var x in articleEntries)
            {
                Console.WriteLine($"GetErrors {articleEntries.IndexOf(x)}/{articleEntries.Count}");
                var errorPairsList = new List<(string Original, string Fixed)>();
                var errorEntry = (errorPairsList, x);
                var tokens = tokenizer.Tokenize(x.Text);
                x.TokenCount = tokens.Length;
                foreach (var token in tokens)
                {
                    // Ignores spell checking on most function words and non-word tokens
                    if (token.Length < 3) continue;
                    x.WordCount++;
                    if (SpellerId == 0)
                    {
                        if (!Spellcheck.hunspell.Spell(token))
                        {
                            var suggestions = Spellcheck.hunspell.Suggest(token);

                            var spellChecked = suggestions.FirstOrDefault();
                            if (spellChecked != token)
                            {
                                errorPairsList.Add((token, spellChecked));
                            }
                        }
                    }
                    else if (SpellerId == 1)
                    {
                        var suggestions = Spellcheck.symspell.Lookup(token, SymSpell.Verbosity.Closest, 3);
                        if (suggestions.Count == 0) suggestions = Spellcheck.symspell.LookupCompound(token);
                        if (suggestions[0].term != token)
                        {
                            suggestions = Spellcheck.symspell.LookupCompound(token);
                            bool isOk = false;
                            //Check for compound word formats separated with a hyphen or written together
                            //These should be treated as correct, but a feature can be extracted
                            if (suggestions[0].term.Replace(" ", "-") == token
                                || suggestions[0].term.Replace(" ", "") == token)
                            {
                                isOk = true;
                            }
                            if (!isOk)
                            {
                                errorPairsList.Add((token, suggestions[0].term));
                            }
                        }
                    }
                }
                x.MisspellCount = errorPairsList.Count;
                errorList.Add(errorEntry);
            }
            return errorList;
        }
        static void Swap<T>(ref T arg1, ref T arg2)
        {
            T temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }
        /// <summary>
        /// Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
        /// integers, where each integer represents the code point of a character in the source string.
        /// Includes an optional threshhold which can be used to indicate the maximum allowable distance.
        /// </summary>
        /// <param name="source">An array of the code points of the first string</param>
        /// <param name="target">An array of the code points of the second string</param>
        /// <param name="threshold">Maximum allowable distance</param>
        /// <returns>Int.MaxValue if threshhold exceeded; otherwise the Damerau-Leveshteim distance between the strings</returns>
        public static int LevenstheinDistance(string source, string target, int threshold = 10)
        {

            int length1 = source.Length;
            int length2 = target.Length;

            // Return trivial case - difference in string lengths exceeds threshhold
            if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

            // Ensure arrays [i] / length1 use shorter length 
            if (length1 > length2)
            {
                Swap(ref target, ref source);
                Swap(ref length1, ref length2);
            }

            int maxi = length1;
            int maxj = length2;

            int[] dCurrent = new int[maxi + 1];
            int[] dMinus1 = new int[maxi + 1];
            int[] dMinus2 = new int[maxi + 1];
            int[] dSwap;

            for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

            int jm1 = 0, im1 = 0, im2 = -1;

            for (int j = 1; j <= maxj; j++)
            {

                // Rotate
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                int minDistance = int.MaxValue;
                dCurrent[0] = j;
                im1 = 0;
                im2 = -1;

                for (int i = 1; i <= maxi; i++)
                {

                    int cost = source[im1] == target[jm1] ? 0 : 1;

                    int del = dCurrent[im1] + 1;
                    int ins = dMinus1[i] + 1;
                    int sub = dMinus1[im1] + cost;

                    //Fastest execution for min value of 3 integers
                    int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                    if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                        min = Math.Min(min, dMinus2[im2] + cost);

                    dCurrent[i] = min;
                    if (min < minDistance) { minDistance = min; }
                    im1++;
                    im2++;
                }
                jm1++;
                if (minDistance > threshold) { return int.MaxValue; }
            }

            int result = dCurrent[maxi];
            return (result > threshold) ? int.MaxValue : result;
        }

    }
}
