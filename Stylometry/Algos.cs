using OpenNLP.Tools.Tokenize;
using OpenNLP.Tools.PosTagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using edu.stanford.nlp.tagger.maxent;

namespace Stylometry
{
    class Algos
    {
        //public const string POS_ST_PATH = @"C:\Users\Andrej\source\repos\Stylometry\Stylometry\Dictionary\english-caseless-left3words-distsim.tagger";
        //static MaxentTagger maxentTagger = new MaxentTagger(POS_ST_PATH);

        const string POS_MODELPATH = @"C:\Users\Andrej\source\repos\Stylometry\Stylometry\Dictionary\EnglishPOS.nbin";
        static EnglishMaximumEntropyPosTagger posTagger = new EnglishMaximumEntropyPosTagger(POS_MODELPATH);
        static EnglishRuleBasedTokenizer tokenizer = new EnglishRuleBasedTokenizer(false);
        /// <summary>
        /// <para>1 = Hunspell</para>
        /// <para>Hunspell cannot parse emojis correctly and will throw <see cref="AccessViolationException"/>(s)</para>
        /// <para>2 = Symspell</para>
        /// </summary>
        public static int SpellerId = 1;

        // Set containing all found swapped letters from the keyboard
        public static HashSet<string> FoundDirectionErrors = new HashSet<string>();

        public static char[,] Keyboard = new char [4, 11] {
            {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-'},
            {'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', '['},
            {'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', ';', '\''},
            {'z', 'x', 'c', 'v', 'b', 'n', 'm', ',', '.', '/', ' '}
        };

        public static char[] KeyboardChars = new char[44]
        {
            '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-',
            'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', '[',
            'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', ';', '\'',
            'z', 'x', 'c', 'v', 'b', 'n', 'm', ',', '.', '/', ' '
        };

        public static Dictionary<string, int> KeyboardDict = new Dictionary<string, int>();

        public static Dictionary<char, (int X, int Y)> KeyboardCoords = new Dictionary<char, (int, int)>();

        public static Dictionary<string, bool> IgnoredTokens = new Dictionary<string, bool> {
            { "'ll", true },
            { "'re", true },
            { "'ve", true },
        };

        /// <summary>
        /// Returns a <see cref="List{T}"/> containing a (WrongWords, Entry) pair with a 
        /// List of mispelled word pairs (Original, Fixed) for each <see cref="ExtendedArticleEntry"/>
        /// </summary>
        /// <param name="articleEntries"></param>
        /// <returns></returns>
        public static List<(List<(string Original, string Fixed)> WrongWords, ExtendedArticleEntry Entry)> GetErrors(List<ExtendedArticleEntry> articleEntries)
        {
            if (KeyboardDict.Count == 0) {
                int index = 0;
                foreach (var x in KeyboardChars)
                {
                    foreach (var y in KeyboardChars)
                    {
                        KeyboardDict.Add(string.Concat(x, "_", y), index);
                        index++;
                    }
                }
            }


            DateTime lastPrint = DateTime.Now;
            List<(List<(string, string)>, ExtendedArticleEntry)> errorList = new List<(List<(string, string)>, ExtendedArticleEntry)>(articleEntries.Count);
            foreach (var x in articleEntries)
            {
                if (DateTime.Now - lastPrint > TimeSpan.FromMilliseconds(2000))
                {
                    Console.WriteLine($"GetErrors {articleEntries.IndexOf(x) + 1}/{articleEntries.Count}");
                    lastPrint = DateTime.Now;
                }
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
                            if (spellChecked != token && spellChecked != token.ToLower())
                            {
                                errorPairsList.Add((token, spellChecked));
                            }
                        }
                    }
                    else if (SpellerId == 1)
                    {
                        // Ignore All Caps words
                        if (token.ToUpper() == token) continue;
                        if (IgnoredTokens.ContainsKey(token)) continue;
                        var suggestions = Spellcheck.symspell.Lookup(token, SymSpell.Verbosity.Closest, 3);
                        if (suggestions.Count == 0) suggestions = Spellcheck.symspell.LookupCompound(token);
                        if (suggestions[0].term != token && suggestions[0].term != token.ToLower())
                        {
                            suggestions = Spellcheck.symspell.LookupCompound(token);
                            bool isOk = false;
                            //Check for compound word formats separated with a hyphen or written together
                            //  and remove dots from end of words
                            //These should be treated as correct, but a feature can be extracted
                            if (suggestions[0].term.Replace(" ", "-") == token
                                || suggestions[0].term.Replace(" ", "") == token
                                || suggestions[0].term == token.Replace(".", "").ToLower())
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
                x.MisspelledWords = errorEntry.errorPairsList;
                errorList.Add(errorEntry);
            }
            Console.WriteLine($"GetErrors Done.");
            return errorList;
        }

        public static Dictionary<string, int> TagDict = new Dictionary<string, int>();

        static string[] TagSentence(string sentence, int type = 0)
        {
            if (type == 0)
            {
                return posTagger.TagSentence(sentence).Select(_ => _.Tag).ToArray();
            }
            else
            {
                var tokens = tokenizer.Tokenize(sentence);
                string[] tags = new string[tokens.Length];
                int index = 0;
                foreach (var token in tokens)
                {
                    //var tag = maxentTagger.tagString(token);
                    //tags[index] = tag;
                    index++;
                }
                return tags;
            }
        }


        public static int firstZeroTag;
        public static List<(int Index, double Count)> sortedTags;
        public static double[] allTagsCount;
        private static int ngrams = 3;
        /// <summary>
        /// Returns a dictionary with the frequency of a specific tag pair
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        public static double[] POSTag(ArticleEntry article)
        {
            if (TagDict.Count == 0)
            {
                int i = 0;
                foreach (var x in posTagger.AllTags())
                {
                    //if (x.Length < 2) continue;
                    //TagDict.Add(x, i);
                    //i++;
                    foreach (var y in posTagger.AllTags())
                    {
                        // Check whether a pair isn't already in the dictionary
                        //if (y.Length < 2) continue;
                        if (ngrams == 2)
                        {
                            if (!TagDict.ContainsKey(string.Concat(x.Shorten(2), "_", y.Shorten(2))))
                            {
                                // add it and assign a unique ID to it
                                TagDict.Add(string.Concat(x.Shorten(2), "_", y.Shorten(2)), i);
                                i++;
                            }
                        }
                        else if (ngrams == 3)
                        {
                            foreach (var z in posTagger.AllTags())
                            {
                                //if (z.Length < 2) continue;

                                if (!TagDict.ContainsKey(string.Concat(x.Shorten(2), "_", y.Shorten(2), "_", z.Shorten(2))))
                                {
                                    // add it and assign a unique ID to it
                                    TagDict.Add(string.Concat(x.Shorten(2), "_", y.Shorten(2), "_", z.Shorten(2)), i);
                                    i++;
                                }
                            }
                        }
                    }
                }
                allTagsCount = new double[TagDict.Count];
                Console.WriteLine($"Generated {TagDict.Count} POS combinations.");
            }

            double[] articleTags = new double[TagDict.Count];
            Array.Clear(articleTags, 0, articleTags.Length);

            // Tag the whole text as one sentence
            var tags = TagSentence(article.Text, 0);



            for (int i = 0; i < tags.Length - (ngrams - 1); i++)
            {
                //bool skip = false;
                //for (int j = 0; j < ngrams - 1; j++)
                //{
                //    if (tags[i + j].Length < 2) skip = true;
                //}
                //if (skip) continue;
                //if (tags[i].Length < 2 || tags[i+1].Length < 2) continue;
                // create a tag ngram
                var ngram = string.Concat(tags[i].Shorten(2), "_", tags[i + 1].Shorten(2), (ngrams > 2 ? "_"+tags[i + 2].Shorten(2) : ""));
                //var pair = tags[i].Tag;
                if (TagDict.ContainsKey(ngram))
                {
                    allTagsCount[TagDict[ngram]]++;
                    articleTags[TagDict[ngram]]++;
                }
                else
                {
                    Console.WriteLine($"Tag pair <{ngram}> wasn't generated, ignoring.");
                }
            }

            // Get the tag frequency among all tags
            for (int i = 0; i < articleTags.Length; i++)
            {
                articleTags[i] /= tags.Length;
            }

            return articleTags;

            //return posTagger.Tag(tokenizer.Tokenize(article.Text).Select(_ => Spellcheck.symspell.Lookup(_, SymSpell.Verbosity.Closest).FirstOrDefault);
        }

        /// <summary>
        /// Sorts all ngram tags according to their frequency from most frequent to least frequent.
        /// </summary>
        public static void SortTags()
        {
            List<(int Index, double Count)> tags = new List<(int Index, double Count)>();
            for (int i = 0; i < allTagsCount.Length; i++)
            {
                tags.Add((i, allTagsCount[i]));
            }
            sortedTags = tags.OrderByDescending(_ => _.Count).ToList();
            firstZeroTag = sortedTags.FindIndex(_ => _.Count == 0);
        }

        public static bool ShouldAddTag(int index)
        {
            // tag is in the top 1%;
            return sortedTags.FindIndex(_ => _.Index == index) < sortedTags.Count / 100;
        }


        public static List<(int Index, int Direction)> GetErrorDirection(string source, string target)
        {
            if (source.Length != target.Length) return null;
            List<(int Index, int Direction)> errorList = new List<(int Index, int Direction)>();
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != target[i])
                {
                    var direction = GetDirection(source[i], target[i]);
                    if (direction == -1) continue;
                    errorList.Add((i, direction));
                }
            }
            return errorList;
        }

        public static int GetDirection(char original, char error)
        {
            if (original == error) return -1;
            if (!KeyboardCoords.ContainsKey(original) || !KeyboardCoords.ContainsKey(error)) return -1;
            var dX = KeyboardCoords[original].X - KeyboardCoords[error].X;
            var dY = KeyboardCoords[original].Y - KeyboardCoords[error].Y;
            if (Math.Abs(dX) > Math.Abs(dY)) {
                return dX > 0 ? 0 : 2;  // up : down
            }
            return dY > 0 ? 3 : 1; // left : right
        }

        public static void InitializeKeyboardCoords()
        {
            var width = Keyboard.GetLength(1);
            var height = Keyboard.GetLength(0);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    KeyboardCoords.Add(Keyboard[i, j], (i, j));
                }
            }
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
