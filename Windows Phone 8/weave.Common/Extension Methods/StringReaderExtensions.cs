using System.Collections.Generic;

namespace System.IO
{
    public static class StringReaderExtensions
    {
        public static IEnumerable<string> ReadLines(this StringReader sr)
        {
            bool linesRemain = true;
            while (linesRemain)
            {
                var line = sr.ReadLine();
                if (line != null)
                {
                    yield return line;
                }
                else
                    linesRemain = false;
            }
        }
    }
}
