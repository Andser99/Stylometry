# Authorship Attribution

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