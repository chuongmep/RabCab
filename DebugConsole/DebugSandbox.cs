using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabCab;
using RabCab.Initialization;

namespace DebugConsole
{
    static class DebugSandbox
    {
        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("-------- RabCab Debugger --------\n");
            Console.WriteLine("Please enter commands to test...");
            ListenForInput();
        }

        /// <summary>
        /// Method for recursively listening for input from debugger
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
                    case "XMLREAD":
                        RabCab.Initialization.Debugging.Cmd_TestXml();
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
