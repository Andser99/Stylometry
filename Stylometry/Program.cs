using System;
using System.Collections.Generic;
using System.Linq;

namespace Stylometry
{
    class Program
    {
        static void Main(string[] args)
        {
            Spellcheck.Init();
            var dataLoader = new DataLoader();
            //var x = Algos.GetErrors(dataLoader.ArticleEntries.GetRange(0, 40));
            var authorsWithMultipleArticles = new List<ExtendedArticleEntry>();
            foreach (var authorArticles in dataLoader.AuthorArticles)
            {
                if (authorArticles.Value.Count > 4)
                {
                    authorsWithMultipleArticles.AddRange(authorArticles.Value);
                }
            }
            var x = Algos.GetErrors(authorsWithMultipleArticles);
            foreach (var err in x)
            {
                Console.WriteLine($"{err.Entry.Author.PadLeft(25)} : {err.Entry.MisspellRatio.ToString("0.000")} = {err.Entry.MisspellCount}/{err.Entry.WordCount} ");
            }
            //foreach (var err in x)
            //{
            //    Console.WriteLine("---BEGIN AUTHOR---");
            //    Console.WriteLine(err.Entry.Author);
            //    Console.WriteLine("---END AUTHOR---");
            //    Console.WriteLine("---BEGIN TEXT---");
            //    Console.WriteLine(err.Entry.Text);
            //    Console.WriteLine("---END TEXT---");
            //    Console.WriteLine("---BEGIN ERRORS---");
            //    Console.WriteLine($"COUNT ERRORS: {err.Entry.MisspellCount}/{err.Entry.WordCount} = {err.Entry.MisspellRatio} ");
            //    foreach (var error in err.WrongWords)
            //    {
            //        Console.WriteLine($"{error.Original.PadLeft(12)} | {error.Wrong}");
            //    }
            //    Console.WriteLine("---END ERRORS---");
            //}
        }
    }
}
