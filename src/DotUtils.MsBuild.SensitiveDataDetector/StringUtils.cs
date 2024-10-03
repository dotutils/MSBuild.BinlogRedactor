// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.Build.SensitiveDataDetector
{
    internal static class StringUtils
    {
#if !NET
        public static string Replace(
            this string input,
            string oldValue, string newValue,
            StringComparison comparisonType)
        {

            // Check inputs.
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (oldValue == null)
            {
                throw new ArgumentNullException(nameof(oldValue));
            }

            if (oldValue.Length == 0)
            {
                throw new ArgumentException("String cannot be of zero length.");
            }

            if (input.Length == 0)
            {
                return input;
            }

            StringBuilder result = new StringBuilder(input.Length);

            int nextStartIndex;
            int startSearchFromIndex = 0;
            while ((nextStartIndex = input.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != -1)
            {

                // Append all characters prefixing the next find.
                int prefixLength = nextStartIndex - startSearchFromIndex;
                if (prefixLength != 0)
                {
                    result.Append(input, startSearchFromIndex, prefixLength);
                }

                // Append the replacement.
                result.Append(newValue);

                // Skip to the end of the replaced substring.
                startSearchFromIndex = nextStartIndex + oldValue.Length;
            }


            // Append the last part to the result.
            result.Append(input, startSearchFromIndex, input.Length - startSearchFromIndex);

            return result.ToString();
        }
#endif

        public static (int lineNumber, int columnNumber) GetLineAndColumn(string input, int index)
        {
            int lineNumber = 1;
            int columnNumber = 1;

            for (int i = 0; i < index; i++)
            {
                if (input[i] == '\n')
                {
                    lineNumber++;
                    columnNumber = 1;
                }
                else
                {
                    columnNumber++;
                }
            }

            return (lineNumber, columnNumber);
        }

    }
}
