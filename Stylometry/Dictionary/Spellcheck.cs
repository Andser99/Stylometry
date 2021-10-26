using NHunspell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylometry
{
    class Spellcheck
    {
        public static string affPath = @"C:\Users\Andrej\source\repos\Stylometry\Stylometry\Dictionary\en_GB.aff";
        public static string dictPath = @"C:\Users\Andrej\source\repos\Stylometry\Stylometry\Dictionary\en_GB.dic";
        public static string frequencyDict = @"C:\Users\Andrej\source\repos\Stylometry\Stylometry\Dictionary\frequency_dictionary_en_82_765.txt";
        public static string frequencyBigramDict = @"C:\Users\Andrej\source\repos\Stylometry\Stylometry\Dictionary\frequency_bigramdictionary_en_243_342.txt";
        public static Hunspell hunspell;
        public static SymSpell symspell;
        static Spellcheck()
        {
            hunspell = new Hunspell(affPath, dictPath);



            // Example from https://github.com/wolfgarbe/symspell
            int initialCapacity = 82765;
            int maxEditDistanceDictionary = 6; // Precalc edit distance
            symspell = new SymSpell(initialCapacity, maxEditDistanceDictionary);

            //load dictionary
            string dictionaryPath = frequencyDict;
            int termIndex = 0; //column of the term in the dictionary text file
            int countIndex = 1; //column of the term frequency in the dictionary text file
            if (!symspell.LoadDictionary(dictionaryPath, termIndex, countIndex))
            {
                Console.WriteLine("SymSpell dictionary not found!");
                //press any key to exit program
                Console.ReadKey();
                return;
            }



            //Engine = new SpellEngine();
            //LanguageConfig enConfig = new LanguageConfig();
            //enConfig.LanguageCode = "en";
            //enConfig.HunspellAffFile = affPath;
            //enConfig.HunspellDictFile = dictPath;
            //enConfig.HunspellKey = "";
            //enConfig.HyphenDictFile = @"C:\Users\Andrej\source\repos\Stylometry\Stylometry\Dictionary\hyph_en_GB.dic";
            //enConfig.MyThesDatFile = @"C:\Users\Andrej\source\repos\Stylometry\Stylometry\Dictionary\th_en_US_v2.dat";
            //Engine.AddLanguage(enConfig);
        }
        public static List<SymSpell.SuggestItem> GetSymSpell(string str)
        {
            return symspell.Lookup(str, SymSpell.Verbosity.Closest, 2);
        }
        public static void Init()
        {
            
        }
    }
}
