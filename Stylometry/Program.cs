using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Stylometry
{
    class Program
    {
        static void Main(string[] args)
        {
            //Force decimal dot for easier csv readability
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            // Initialize Keyboard coord dictionary
            Algos.InitializeKeyboardCoords();

            var right = "autjor";
            var left = "autgor";
            var up = "autyor";
            var down = "autnor";
            var orig = "author";

            // 1 = right, 3 = left, 2 = down, 0 = up
            var r = Algos.GetErrorDirection(orig, right);
            var l = Algos.GetErrorDirection(orig, left);
            var d = Algos.GetErrorDirection(orig, down);
            var u = Algos.GetErrorDirection(orig, up);



            //Loads articles
            var dataLoader = new DataLoader();


            //Seed 5, 5, 2 was first used
            //Setting MOD to anything other than 1 will filter out a random number of articles to a size of 1/MOD * original size
            var SEED = 5;
            var MOD = 4;
            var COUNT = 6;
            var rnd = new Random(SEED);
            //
            // Gets the 2 longest article from each author that has 2+ articles
            var authorsWithNArticles = new List<ExtendedArticleEntry>();
            foreach (var authorArticles in dataLoader.AuthorArticles)
            {
                if (authorArticles.Value.Count > 1 && rnd.Next() % MOD == 0)
                {
                    if (authorArticles.Value.Count == COUNT)
                    {
                        authorsWithNArticles.AddRange(authorArticles.Value);
                    } 
                    else if(authorArticles.Value.Count > COUNT)
                    {
                        var ordered = authorArticles.Value.OrderByDescending(_ => _.Text);
                        for (int i = 0; i < COUNT; i++)
                        {
                            authorsWithNArticles.Add(ordered.ElementAt(i));
                        }
                    }
                }
            }

            //Get list of errors for each article
            var errorPairs = Algos.GetErrors(authorsWithNArticles);

            //Generate UnicodeCategory stats and Rune lists for each article
            foreach (var err in errorPairs)
            {
                err.Entry.PopulateRunes();
                err.Entry.PopulateDirectionErrors();
            }

            // Write Pair info and UnicodeCategory stats
            //foreach (var err in errorPairs)
            //{
            //    Console.WriteLine($"{err.Entry.Author.PadRight(18)} : {err.Entry.MisspellRatio.ToString("0.000")} = {err.Entry.MisspellCount}/{err.Entry.WordCount}, DIR {err.Entry.GetMostCommonDirection().Direction} ");
            //    foreach(var key in err.Entry.UnicodeCategoryCounts.Keys)
            //    {
            //        Console.Write($"{key.ToString().ToInitials(),6} {err.Entry.UnicodeCategoryCounts[key],5}, ");
            //    }
            //    Console.WriteLine();
            //}



            //Save data
            var filePath = @"C:\Users\Andrej\Desktop\Skola\IAU\2021-2022\cvicenia\tyzden-03\data\data.csv";
            File.WriteAllText(filePath, $"Author,");
            foreach(var enumEntry in Enum.GetValues(typeof(UnicodeCategory)))
            {
                File.AppendAllText(filePath, $"{enumEntry},");
                File.AppendAllText(filePath, $"{enumEntry}_pertokens,");
            }
            File.AppendAllText(filePath, $"Toperror,");
            File.AppendAllText(filePath, $"Righterror,");
            File.AppendAllText(filePath, $"Downerror,");
            File.AppendAllText(filePath, $"Lefterror,");
            File.AppendAllText(filePath, $"Misspellratio,");
            File.AppendAllText(filePath, $"Tokens{Environment.NewLine}");

            foreach (var err in errorPairs)
            {
                File.AppendAllText(filePath, $"{err.Entry.AuthorId.ToInitials()}, ");
                //Alternative Author name instead of ID of the author
                //File.AppendAllText(filePath, $"{err.Entry.Author.ToInitials()}, ");
                foreach(var categoryKey in err.Entry.UnicodeCategoryCounts.Keys)
                {
                    File.AppendAllText(filePath, $"{err.Entry.UnicodeCategoryCounts[categoryKey]},");
                    File.AppendAllText(filePath, $"{err.Entry.UnicodeCategoryCounts[categoryKey] / (double)err.Entry.TokenCount:0.###},");
                }
                File.AppendAllText(filePath, $"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 0)},");
                File.AppendAllText(filePath, $"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 1)},");
                File.AppendAllText(filePath, $"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 2)},");
                File.AppendAllText(filePath, $"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 3)},");
                File.AppendAllText(filePath, $"{err.Entry.MisspellRatio.ToString("0.###")},");
                File.AppendAllText(filePath, $"{err.Entry.TokenCount.ToString("0.###")}{Environment.NewLine}");
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
