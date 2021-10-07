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
        public static Hunspell hunspell;
        static Spellcheck()
        {
            hunspell = new Hunspell(affPath, dictPath);
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
        public static void Init()
        {
            
        }
    }
}
