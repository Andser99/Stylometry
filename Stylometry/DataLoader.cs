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
            using var streamReader = new StreamReader(@"C:\Users\Andrej\source\repos\Stylometry\Stylometry\articles.csv");
            using var csvReader = new CsvReader(streamReader, System.Globalization.CultureInfo.InvariantCulture);
            var extractedText = csvReader.GetRecords<ArticleEntry>().ToList();
            foreach (var x in extractedText.Distinct())
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


            //TrainingArticles.First().Text;

        }
    }
}
