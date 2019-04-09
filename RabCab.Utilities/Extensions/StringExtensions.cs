// -----------------------------------------------------------------------------------
//     <copyright file="StringExtensions.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Globalization;

namespace RabCab.Utilities.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        ///     Method to convert upper and lowercase input to Title Case
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string strInput)
        {
            //Convert input text To Upper to ensure all case starts as the same input
            var upperCase = strInput.ToUpper();

            //Create the text info converter
            var textInfo = new CultureInfo("en-US", false).TextInfo;

            //Return the title case of the input text
            return textInfo.ToTitleCase(upperCase.ToLower());
        }
    }
}