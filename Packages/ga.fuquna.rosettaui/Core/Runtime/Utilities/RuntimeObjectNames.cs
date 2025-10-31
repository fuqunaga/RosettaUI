using System.Text;

namespace RosettaUI
{
    /// <summary>
    /// ObjectNames clone running at runtime
    /// /// </summary>
    public static class RuntimeObjectNames
    {
        // refs: https://gist.github.com/ErnSur/842d72606159fdd865979c4d9db21a18
        public static string NicifyVariableName(string input)
        {
            var result = new StringBuilder(input.Length * 2);

            var prevIsLetter = false;
            var prevIsLetterUpper = false;
            var prevIsDigit = false;
            var prevIsStartOfWord = false;
            var prevIsNumberWord = false;

            var firstCharIndex = 0;
            if (input.StartsWith('_'))
                firstCharIndex = 1;
            else if (input.StartsWith("m_"))
                firstCharIndex = 2;

            for (var i = input.Length - 1; i >= firstCharIndex; i--)
            {
                var currentChar = input[i];
                var currIsLetter = char.IsLetter(currentChar);
                if (i == firstCharIndex && currIsLetter)
                    currentChar = char.ToUpper(currentChar);
                var currIsLetterUpper = char.IsUpper(currentChar);
                var currIsDigit = char.IsDigit(currentChar);
                var currIsSpacer = currentChar == ' ' || currentChar == '_';

                var addSpace = (currIsLetter && !currIsLetterUpper && prevIsLetterUpper) ||
                               (currIsLetter && prevIsLetterUpper && prevIsStartOfWord) ||
                               (currIsDigit && prevIsStartOfWord) ||
                               (!currIsDigit && prevIsNumberWord) ||
                               (currIsLetter && !currIsLetterUpper && prevIsDigit);

                if (!currIsSpacer && addSpace)
                {
                    result.Insert(0, ' ');
                }

                result.Insert(0, currentChar);
                prevIsStartOfWord = currIsLetter && currIsLetterUpper && prevIsLetter && !prevIsLetterUpper;
                prevIsNumberWord = currIsDigit && prevIsLetter && !prevIsLetterUpper;
                prevIsLetterUpper = currIsLetter && currIsLetterUpper;
                prevIsLetter = currIsLetter;
                prevIsDigit = currIsDigit;
            }

            return result.ToString();
        }
    }
}