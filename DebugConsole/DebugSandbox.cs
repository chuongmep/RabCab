// -----------------------------------------------------------------------------------
//     <copyright file="DebugSandbox.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/10/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using RabCab.Initialization;

namespace DebugConsole
{
    internal static class DebugSandbox
    {
        /// <summary>
        ///     Main method
        /// </summary>
        /// <param name="args"></param>
        private static void Main()
        {
            Console.WriteLine("-------- RabCab Debugger --------\n");
            Console.WriteLine("Please enter commands to test...");
            ListenForInput();
        }

        /// <summary>
        ///     Method for recursively listening for input from debugger
        /// </summary>
        private static void ListenForInput()
        {
            //Set listening variable
            var listening = true;

            //While user does not choose to exit, keep asking for input
            while (listening)
            {
                //Write an empty line to space out commands
                Console.WriteLine("");

                //Read input from the user
                var input = Console.ReadLine();

                switch (input?.ToUpper())
                {
                    case "REPOFASTENERS":
                        Debugging.Cmd_TestXml();
                        break;
                    case "REPOMATERIALS":
                        Debugging.Cmd_TestMats();
                        break;
                    case "EXIT":
                        listening = false;
                        break;
                    default:
                        Console.Write("No command exists with that name.");
                        break;
                }
            }
        }
    }
}