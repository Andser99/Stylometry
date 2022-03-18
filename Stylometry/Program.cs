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
            var SEED = 2;
            var MOD = 1;
            var COUNT = 50;
            var rnd = new Random(SEED);
            //
            // Gets the <COUNT> longest article from each author that has <COUNT>+ articles
            var authorsWithNArticles = new List<ExtendedArticleEntry>();
            Console.WriteLine($"Filtering authors MOD={MOD}, COUNT={COUNT}");
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
                err.Entry.PosTagFrequency = new double[Algos.TagDict.Count];
                err.Entry.PosTagFrequency = Algos.POSTag(err.Entry);
            }
            // Sort tags from most frequent to least frequent
            Algos.SortTags();

            //int startIndex = 0;
            //int endIndex = Algos.sortedTags.Count / 100;
            int startIndex = Algos.firstZeroTag - (Algos.sortedTags.Count / 100) >= 0 ? Algos.firstZeroTag - (Algos.sortedTags.Count / 100) : 0;
            int endIndex = Algos.firstZeroTag;

            //Save data
            var filePath = @"C:\Users\Andrej\source\repos\Stylometry\Python\kratke_data.csv";
            //Header
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
            for (int i = startIndex; i < endIndex; i++)
            {
                File.AppendAllText(filePath, $"pos_{i}_{Algos.TagDict.FirstOrDefault(_ => _.Value == Algos.sortedTags[i].Index).Key.Replace(",", "CO")},");
            }
            for (int i = 0; i < Algos.KeyboardDict.Count; i++)
            {
                File.AppendAllText(filePath, $"swap_{Algos.KeyboardDict.FirstOrDefault(_ => _.Value == i).Key.Replace(",", "CO")},");
            }
            File.AppendAllText(filePath, $"Tokens{Environment.NewLine}");

            // Data
            int writingCounter = 0;
            DateTime lastPrint = DateTime.Now;
            Console.WriteLine($"Writing authors to file - {writingCounter}/{errorPairs.Count}");
            foreach (var err in errorPairs)
            {
                var line = new StringBuilder();
                writingCounter++;
                if (DateTime.Now - lastPrint > TimeSpan.FromMilliseconds(2000))
                {
                    lastPrint = DateTime.Now;
                    Console.WriteLine($"Writing authors to file - {writingCounter}/{errorPairs.Count}");
                }

                line.Append($"{err.Entry.AuthorId}, ");
                //File.AppendAllText(filePath, $"{err.Entry.AuthorId.ToInitials()}, ");
                //Alternative Author name instead of ID of the author
                //File.AppendAllText(filePath, $"{err.Entry.Author.ToInitials()}, ");
                foreach(var categoryKey in err.Entry.UnicodeCategoryCounts.Keys)
                {
                    line.Append($"{err.Entry.UnicodeCategoryCounts[categoryKey]},");
                    line.Append($"{err.Entry.UnicodeCategoryCounts[categoryKey] / (double)err.Entry.TokenCount:0.###},");
                    //File.AppendAllText(filePath, $"{err.Entry.UnicodeCategoryCounts[categoryKey]},");
                    //File.AppendAllText(filePath, $"{err.Entry.UnicodeCategoryCounts[categoryKey] / (double)err.Entry.TokenCount:0.###},");
                }
                line.Append($"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 0)},");
                line.Append($"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 1)},");
                line.Append($"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 2)},");
                line.Append($"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 3)},");
                //File.AppendAllText(filePath, $"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 0)},");
                //File.AppendAllText(filePath, $"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 1)},");
                //File.AppendAllText(filePath, $"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 2)},");
                //File.AppendAllText(filePath, $"{err.Entry.MisspelledDirections.Count(_ => _.Direction == 3)},");

                line.Append($"{err.Entry.MisspellRatio:0.###},");
                //File.AppendAllText(filePath, $"{err.Entry.MisspellRatio.ToString("0.###")},");
                for (int i = startIndex; i < endIndex; i++)
                {
                    line.Append($"{err.Entry.PosTagFrequency[Algos.sortedTags[i].Index]:0.0##},");
                    //File.AppendAllText(filePath, $"{err.Entry.PosTagFrequency[i]:0.0##},");
                }
                for (int i = 0; i < Algos.KeyboardDict.Count; i++)
                {
                    line.Append($"{err.Entry.SwappedKeysOccurence[i] / err.Entry.TokenCount:0.###},");
                }
                line.Append($"{err.Entry.TokenCount:0.###}{Environment.NewLine}");
                //File.AppendAllText(filePath, $"{err.Entry.TokenCount.ToString("0.###")}{Environment.NewLine}");

                File.AppendAllText(filePath, line.ToString());
            }

            Console.WriteLine($"Writing authors to file Done.");
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
