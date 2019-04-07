using System;
using System.Globalization;
using System.Linq;

namespace RabCab.Utilities.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Method to convert upper and lowercase input to Title Case
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string strInput)
        {
            //Convert input text To Upper to ensure all case starts as the same input
            var upperCase = strInput.ToUpper();

            //Create the text info converter
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            //Return the title case of the input text
            return textInfo.ToTitleCase(upperCase.ToLower());
        }
    }
}
