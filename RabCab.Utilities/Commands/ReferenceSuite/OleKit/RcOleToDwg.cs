﻿// -----------------------------------------------------------------------------------
//     <copyright file="RcOleToDwg.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/10/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.Runtime;
using RabCab.Utilities.Settings;

namespace RabCab.Utilities.Commands.ReferenceSuite.OleKit
{
    internal class RcOleToDwg
    {
        internal class RcMainPalette
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