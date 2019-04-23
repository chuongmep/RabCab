﻿using Autodesk.AutoCAD.Runtime;
using RabCab.Settings;

namespace RabCab.Commands.PaletteKit
{
    internal class RcPaletteLayer
    {
        internal class RcDwgBrowser
        {
            /// <summary>
            /// </summary>
            [CommandMethod(SettingsInternal.CommandGroup, "_CMDDEFAULT",
                CommandFlags.Modal
                //| CommandFlags.Transparent
                //| CommandFlags.UsePickSet
                //| CommandFlags.Redraw
                //| CommandFlags.NoPerspective
                //| CommandFlags.NoMultiple
                //| CommandFlags.NoTileMode
                //| CommandFlags.NoPaperSpace
                //| CommandFlags.NoOem
                //| CommandFlags.Undefined
                //| CommandFlags.InProgress
                //| CommandFlags.Defun
                //| CommandFlags.NoNewStack
                //| CommandFlags.NoInternalLock
                //| CommandFlags.DocReadLock
                //| CommandFlags.DocExclusiveLock
                //| CommandFlags.Session
                //| CommandFlags.Interruptible
                //| CommandFlags.NoHistory
                //| CommandFlags.NoUndoMarker
                //| CommandFlags.NoBlockEditor
                //| CommandFlags.NoActionRecording
                //| CommandFlags.ActionMacro
                //| CommandFlags.NoInferConstraint 
            )]
            public void Cmd_Default()
            {
            }
        }
    }
}