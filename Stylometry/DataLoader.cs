using CsvHelper;
using NHunspell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNLP.Tools;
using OpenNLP.Tools.Tokenize;

namespace Stylometry
{
    class DataLoader
    {
        public List<ExtendedArticleEntry> ArticleEntries = new List<ExtendedArticleEntry>();
        public Dictionary<string, List<ExtendedArticleEntry>> AuthorArticles = new Dictionary<string, List<ExtendedArticleEntry>>();
        public List<ExtendedArticleEntry> TrainingArticles = new List<ExtendedArticleEntry>();
        public List<ExtendedArticleEntry> TestingArticles = new List<ExtendedArticleEntry>();

        public DataLoader()
        {
            //using var streamReader = new StreamReader(@"C:\Users\Andrej\source\repos\Stylometry\Stylometry\articles.csv");
            using var streamReader = new StreamReader(@"S:\Datasets\blogtext.csv");
            using var csvReader = new CsvReader(streamReader, System.Globalization.CultureInfo.InvariantCulture);
            var extractedText = csvReader.GetRecords<ArticleEntry>().ToList();
            foreach (var x in extractedText)
            {
                ArticleEntries.Add(new ExtendedArticleEntry(x));
            }
            // Create a dictionary of articles for each author
            foreach (var x in ArticleEntries)
            {
                if (!AuthorArticles.ContainsKey(x.Author))
                {
                    AuthorArticles[x.Author] = new List<ExtendedArticleEntry>();
                }
                AuthorArticles[x.Author].Add(x);
            }
            // Get only authors with 2+ articles
            foreach (var x in AuthorArticles.Keys)
            {
                if (AuthorArticles[x].Count > 1)
                {
                    TrainingArticles.AddRange(AuthorArticles[x]);
                }
            }

        }

        /// <summary>
        /// Cuts ArticleEntries into a size 1/n while selecting random elements
        /// </summary>
        /// <param name="n"></param>
        public static void ShortenAndSave(List<ArticleEntry> articles, int n, string path)
        {
            using var streamWriter = new StreamWriter(path);
            using var csvWriter = new CsvWriter(streamWriter, System.Globalization.CultureInfo.InvariantCulture);
            csvWriter.WriteHeader<ArticleEntry>();
            var rnd = new Random();
            foreach (var x in articles)
            {
                if (rnd.Next(n) == 0)
                {
                    csvWriter.WriteRecord(x);
                }
            }
        }
    }
}
