using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylometry
{
    class ArticleEntry
    {
        public string Author { get; }
        public string Text { get; }
        public ArticleEntry(string author, string text)
        {
            this.Author = author;
            this.Text = text;
        }

        public override bool Equals(object obj)
        {
            return (obj as ArticleEntry).Author == Author && Text.GetHashCode() == (obj as ArticleEntry).Text.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Author.Length + Text.Length * 100;
        }
    }
    class ExtendedArticleEntry : ArticleEntry
    {
        private static List<string> _existingAuthors = new List<string>();
        public List<Rune> Runes { get; set; }
        public Dictionary<UnicodeCategory, int> UnicodeCategoryCounts;
        public int TokenCount { get; set; } = 0;
        public int WordCount { get; set; } = 0;
        /// <summary>
        /// Grammatical errors divided by the number of words in the text, only words longer than 2 are checked
        /// </summary>
        public double MisspellRatio { 
            get
            {
                return MisspellCount / (double)WordCount;
            }
        }

        public int MisspellCount { get; set; } = 0;
        public string AuthorId { get; set; }

        public List<(string Original, string Fixed)> MisspelledWords = new List<(string Original, string Fixed)>();

        public List<(string Original, string Fixed, int Direction, int Index)> MisspelledDirections = new List<(string Original, string Fixed, int Direction, int Index)>();

        public ExtendedArticleEntry(string author, string text) : base(author, text)
        {
            AuthorId = CheckAuthor(author);
        }

        public ExtendedArticleEntry(ArticleEntry entry) : base(entry.Author, entry.Text)
        {
            AuthorId = CheckAuthor(entry.Author);
        }
        /// <summary>
        /// Populates <see cref="Runes"/> with <see cref="Rune"/>(s) converted from its own <see cref="ArticleEntry.Text"/>
        /// and <see cref="UnicodeCategoryCounts"/> for each value in the <see cref="UnicodeCategory"/> enum
        /// <para><see cref="Runes"/> will be empty if <see cref="ArticleEntry.Text"/> is null or empty</para> 
        /// </summary>
        /// <param name="statsOnly"></param>
        public void PopulateRunes()
        {
            UnicodeCategoryCounts = new Dictionary<UnicodeCategory, int>();
            Runes = Text.ToRuneListWithDictionary(ref UnicodeCategoryCounts);
        }

        public void PopulateDirectionErrors()
        {
            foreach (var mPair in MisspelledWords)
            {
                var errors = Algos.GetErrorDirection(mPair.Original, mPair.Fixed);
                if (errors != null)
                {
                    foreach (var dir in errors)
                    {
                        MisspelledDirections.Add((mPair.Original, mPair.Fixed, dir.Direction, dir.Index));
                    }
                }
            }
        }

        public (int Direction, int Count) GetMostCommonDirection()
        {
            Dictionary<int, int> directionDict = new Dictionary<int, int>();
            var key = -1;
            var count = -1;
            foreach (var x in MisspelledDirections)
            {
                if (!directionDict.ContainsKey(x.Direction))
                {
                    directionDict[x.Direction] = 1;
                }
                else
                {
                    directionDict[x.Direction]++;
                }
                if (directionDict[x.Direction] > count)
                {
                    count = directionDict[x.Direction];
                    key = x.Direction;
                }
            }
            return (key, count);

        }

        private static string CheckAuthor(string author)
        {
            if (!_existingAuthors.Contains(author))
            {
                _existingAuthors.Add(author);
            }
            return _existingAuthors.IndexOf(author).ToString();
        }
    }
}
