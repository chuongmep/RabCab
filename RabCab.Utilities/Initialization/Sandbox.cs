// -----------------------------------------------------------------------------------
//     <copyright file="Sandbox.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/10/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;

namespace RabCab.Initialization
{
    /// <summary>
    ///     Utility method for writing to console during debug operations
    /// </summary>
    /// <param name="input"></param>
    public static class Sandbox
    {
        public static void Write(string input)
        {
#if DEBUG
            Console.Write(input);
#endif
        }

        /// <summary>
        ///     Utility method for writing to console during debug operations
        /// </summary>
        /// <param name="input"></param>
        public static void WriteLine(string input)
        {
#if DEBUG
            Console.WriteLine(input);
#endif
        }
    }
}