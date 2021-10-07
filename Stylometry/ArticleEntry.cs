using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylometry
{
    class ArticleEntry
    {
        public string Author { get; }
        public string Claps { get; }
        public string ReadingTime { get; }
        public string Link { get; }
        public string Text { get; }
        public ArticleEntry(string author, string claps, string readingTime, string link, string text)
        {
            this.Author = author;
            this.Claps = claps;
            this.ReadingTime = readingTime;
            this.Link = link;
            this.Text = text;
        }

        public override bool Equals(object obj)
        {

            return (obj as ArticleEntry).Author == Author
                && (obj as ArticleEntry).Claps == Claps
                && (obj as ArticleEntry).ReadingTime == ReadingTime;
        }

        public override int GetHashCode()
        {
            return Author.Length + Text.Length * 100;
        }
    }
    class ExtendedArticleEntry : ArticleEntry
    {
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
        public ExtendedArticleEntry(string author, string claps, string readingTime, string link, string text) : base(author, claps, readingTime, link, text)
        {
        }

        public ExtendedArticleEntry(ArticleEntry entry) : base(entry.Author, entry.Claps, entry.ReadingTime, entry.Link, entry.Text)
        {
        }

    }
}
