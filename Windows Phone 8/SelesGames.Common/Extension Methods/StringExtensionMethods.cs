using System.Text;

namespace System
{
    public static class StringExtensionMethods
    {
        public static string ToTitleCase(this string thePhrase)
        {
            StringBuilder newString = new StringBuilder();
            StringBuilder nextString = new StringBuilder();
            string[] phraseArray;
            string theWord;
            string returnValue;
            phraseArray = thePhrase.Split(null);
            for (int i = 0; i < phraseArray.Length; i++)
            {
                theWord = phraseArray[i].ToLower();
                if (theWord.Length > 1)
                {
                    if (theWord.Substring(1, 1) == "'")
                    {
                        //Process word with apostrophe at position 1 in 0 based string.
                        if (nextString.Length > 0)
                            nextString.Replace(nextString.ToString(), null);
                        nextString.Append(theWord.Substring(0, 1).ToUpper());
                        nextString.Append("'");
                        nextString.Append(theWord.Substring(2, 1).ToUpper());
                        nextString.Append(theWord.Substring(3).ToLower());
                        nextString.Append(" ");
                    }
                    else
                    {
                        if (theWord.Length > 1 && theWord.Substring(0, 2) == "mc")
                        {
                            //Process McName.
                            if (nextString.Length > 0)
                                nextString.Replace(nextString.ToString(), null);
                            nextString.Append("Mc");
                            nextString.Append(theWord.Substring(2, 1).ToUpper());
                            nextString.Append(theWord.Substring(3).ToLower());
                            nextString.Append(" ");
                        }
                        else
                        {
                            if (theWord.Length > 2 && theWord.Substring(0, 3) == "mac")
                            {
                                //Process MacName.
                                if (nextString.Length > 0)
                                    nextString.Replace(nextString.ToString(), null);
                                nextString.Append("Mac");
                                nextString.Append(theWord.Substring(3, 1).ToUpper());
                                nextString.Append(theWord.Substring(4).ToLower());
                                nextString.Append(" ");
                            }
                            else
                            {
                                //Process normal word (possible apostrophe near end of word.
                                if (nextString.Length > 0)
                                    nextString.Replace(nextString.ToString(), null);
                                nextString.Append(theWord.Substring(0, 1).ToUpper());
                                nextString.Append(theWord.Substring(1).ToLower());
                                nextString.Append(" ");
                            }
                        }
                    }
                }
                else
                {
                    //Process normal single character length word.
                    if (nextString.Length > 0)
                        nextString.Replace(nextString.ToString(), null);
                    nextString.Append(theWord.ToUpper());
                    nextString.Append(" ");
                }
                newString.Append(nextString);
            }
            returnValue = newString.ToString();
            return returnValue.Trim();
        }
    }
}