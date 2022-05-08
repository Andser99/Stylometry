# Authorship Attribution
## Requirements

##### Feature extraction
- Visual Studio 2019 (or higher)
- .NET 5.0
- PoS Tagger english model (included, Stylometry/Dictionary/EnglishPOS.nbin)
- word frequency dictionary for Symspell (included, Stylometry/Dictionary/frequency\_dictionary\_en\_82\_765.txt)

_Optional_
If you want to use Hunspell instead of Symspell, beware that hunspell may not work correctly when the word contains emojis or UTF surrogates.
- .aff, .dic files for Hunspell (included, Stylometry/Dictionary/en\_US.dic, en\_US.aff)
##### Model testing
- Python 3.8.3
dateutil == 2.8.2
matplotlib == 3.4.2
numpy == 1.19.5
pandas == 1.3.3
scipy == 1.7.1
seaborn == 0.11.2
session_info == 1.0.0
sklearn == 0.24.2
statsmodels == 0.12.2
- Jupyter Notebook

## Setup
##### C#
Spellcheck.cs
direct the variables to the dictionary paths
``` 
affPath = @"...\Stylometry\Dictionary\en_GB.aff";
dictPath = @"...\Stylometry\Dictionary\en_GB.dic";
frequencyDict = @"...\Stylometry\Dictionary\frequency_dictionary_en_82_765.txt";
frequencyBigramDict = @"...\Stylometry\Dictionary\frequency_bigramdictionary_en_243_342.txt";
```
Algos.cs
path to the PoS .nbin model
```
POS_MODELPATH = @"...\Stylometry\Dictionary\EnglishPOS.nbin";
```

DataLoader.cs
line 24, path to the csv file containing source texts with authors
```
using var streamReader = new StreamReader(@"...\articles.csv");
```
##### Python
Open jupyter notebook at \Python
Models.ipynb contains everything necessary, some extracted examples are already included in the same directory.
Running everything up to Evaluation will give the accuracy for the selected features located in the Dataframes part. Optionally, sampling data to different sizes can be skipped completely, denoted by OPTIONAL.

## Data Loader
Loads a csv file and maps it onto a list of ArticleEntry class objects, currently supported headers are: Author, Claps, ReadingTime, Link, Text

## Extended Article Entry
### Misspell ratio
Calculates the rate of grammatical errors over the word count - word count is calculated from the number of tokens present in the text, based on the OpenNLP english rule based tokenizer.
Errors are checked using SymSpell (https://github.com/wolfgarbe/symspell) which provides a better performance than NHunspell and also returns the edit distance (which is its delete distance because of the implementation)
The misspell ratio calculation also creates a list containing pairs of (Original, Fixed) words from the text source.
### Unicode Category counts
Counts characters of each 29 unicode categories in the text.
e.g. LowerCaseLetter, Control, OtherLetter, MathSymbol
https://docs.microsoft.com/en-us/dotnet/api/system.globalization.unicodecategory?view=net-5.0
### Runes
Not every character is necessarily readable and supported by older libraries or functions because of Surrogates (https://en.wikipedia.org/wiki/Plane_(Unicode)#Basic_Multilingual_Plane)
During Unicode Category counts creation each character is also converted to a Rune class type and stored in a list.

### Idiosyncracies
#### Positional Character Errors
Misspelled tokens are checked for the direction towards which the author misspells individual characters in them, e.g. the word "autjor" (from the original "author") equals to a right positional errors
since the character 'j' is to the right of 'h' on an English QWERTY keyboard. The position matrix is a 3 by 11 array containing "qwertyuiop[", "asdfghjkl;'" and "zxcvbnm,./". 
When searching for which position corresponds to each misspell, the furthest directional path is chosen - in the case of 'h' and 'c' it would be 'left' since 'c's relative position 
to 'h' is X=-3 Y=-1, the X position is larger and thus more relevant.
These errors are biased towards the dimension in which more characters are present, even though misspells usually occur in neighbouring characters (e.g. 'r' instead of 't' is more common than 'd' instead of 't')
during tests, there were more left/right errors than up/down.

#### Swaps
While extracting positional errors, swapped key pairs are also generated. E.g. 'Hellp' instead of 'Hello' counts as one <p,o> pair for that given article.