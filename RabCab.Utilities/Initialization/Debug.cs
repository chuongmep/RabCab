using Autodesk.AutoCAD.Runtime;
using RabCab.Utilities.Initialization;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof (Debug))]

namespace RabCab.Utilities.Initialization
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    internal class Debug
    {
    }
}