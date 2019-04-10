using System;

namespace RabCab.Utilities.Initialization
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
