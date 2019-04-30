using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Interop.Common;
using Autodesk.AutoCAD.Runtime;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Commands.ReferenceSuite.BlockKit
{
    class RcAutoBlock
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_AUTOBLOCK",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            //| CommandFlags.UsePickSet
            //| CommandFlags.Redraw
            //| CommandFlags.NoPerspective
            //| CommandFlags.NoMultiple
            //| CommandFlags.NoTileMode
            | CommandFlags.NoPaperSpace
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
            | CommandFlags.NoBlockEditor
            | CommandFlags.NoActionRecording
            | CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_AutoBlock()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false);
            if (objIds.Length <= 0) return;

            var bNameTaken = true;
            var bName = "";

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForWrite) as BlockTable;

                if (acBlkTbl == null)
                {
                    acTrans.Abort();
                    return;
                }

                while (bNameTaken)
                {
                    var checkName = acCurEd.GetSimpleString("\nEnter desired block name: ");

                    if (string.IsNullOrEmpty(checkName))
                    {
                        acTrans.Abort();
                        return;
                    }

                    if (acBlkTbl.Has(checkName))
                    {
                        acCurEd.WriteMessage("\nBlock name already exists.");
                        continue;
                    }

                    bName = checkName;
                    bNameTaken = false;
                }

                var objCol = new ObjectIdCollection();

                foreach (var obj in objIds)
                    objCol.Add(obj);

                var extents = acTrans.GetExtents(objIds, acCurDb);

                var acBtr = new BlockTableRecord();

                acBtr.Name = bName;
                acBtr.Origin = extents.MinPoint;
                acBtr.BlockScaling = BlockScaling.Uniform;
                acBtr.Explodable = true;
                acBtr.Units = UnitsValue.Inches;

                #region TODO replace with xml reading

                //Add attribute definitions
                //TODO read XML to get attributes to add
                //For now we'll use a test value

                var attTag = new AttributeDefinition
                {
                    Justify = AttachmentPoint.MiddleCenter,
                    AlignmentPoint = extents.MinPoint,
                    Prompt = "Tag:",
                    Tag = "Tag",
                    TextString = bName,
                    Height = 1,
                    Invisible = true,
                    LockPositionInBlock = true
                };

                var attCrate = new AttributeDefinition
                {
                    Justify = AttachmentPoint.MiddleCenter,
                    AlignmentPoint = extents.MinPoint,
                    Prompt = "Crate:",
                    Tag = "Crate",
                    TextString = "",
                    Height = 1,
                    Invisible = true,
                    LockPositionInBlock = true
                };

                #endregion

                var blockId = acBlkTbl.Add(acBtr);

                acBtr.AppendEntity(attTag);
                acBtr.AppendEntity(attCrate);
                acTrans.AddNewlyCreatedDBObject(acBtr, true);

                var map = new IdMapping();
                acCurDb.DeepCloneObjects(objCol, acBtr.ObjectId, map, false);
                var objCol2 = new ObjectIdCollection();

                foreach (IdPair pair in map)
                {
                    if (!pair.IsPrimary) continue;
                    var ent = acTrans.GetObject(pair.Value, OpenMode.ForWrite) as Entity;

                    if (ent == null) continue;
                    ent.Layer = "0";
                    ent.ColorIndex = 0;
                    objCol2.Add(ent.ObjectId);
                }

                acBtr.AssumeOwnershipOf(objCol2);

                BlockReference acBr = new BlockReference(extents.MinPoint, blockId);
                acCurDb.AppendEntity(acBr);
                acBr.AppendAttributes(acBtr, acTrans);

                foreach (var obj in objIds)
                {
                    var acEnt = acTrans.GetObject(obj, OpenMode.ForWrite) as Entity;
                    if (acEnt == null) continue;
                    acEnt.Erase();
                }

                acTrans.Commit();
            }

            acCurDb.Audit(true, false);
        }

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_AUTOATT",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            //| CommandFlags.UsePickSet
            //| CommandFlags.Redraw
            //| CommandFlags.NoPerspective
            //| CommandFlags.NoMultiple
            //| CommandFlags.NoTileMode
            | CommandFlags.NoPaperSpace
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
            | CommandFlags.NoBlockEditor
            | CommandFlags.NoActionRecording
            | CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_AutoAtt()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Insert, false);
            if (objIds.Length <= 0) return;
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for write
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (acBlkTbl == null)
                {
                    acTrans.Abort();
                    return;
                }

                foreach (var obj in objIds)
                {
                    var acBref = acTrans.GetObject(obj, OpenMode.ForRead) as BlockReference;

                    if (acBref == null) continue;

                    BlockTableRecord btr = null;

                    if (acBref.IsDynamicBlock)
                    {
                        btr = acTrans.GetObject(acBref.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                    }
                    else
                    {
                        btr = acTrans.GetObject(acBref.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                    }

                    if (btr == null) continue;

                    #region TODO replace with xml reading
                    var bName = acBref.Name;

                    //Add attribute definitions
                    //TODO read XML to get attributes to add
                    //For now we'll use a test value

                    var attTag = new AttributeDefinition
                    {
                        Justify = AttachmentPoint.MiddleCenter,
                        AlignmentPoint = btr.Origin,
                        Prompt = "Tag:",
                        Tag = "Tag",
                        TextString = bName,
                        Height = 1,
                        Invisible = true,
                        LockPositionInBlock = true
                    };

                    var attCrate = new AttributeDefinition
                    {
                        Justify = AttachmentPoint.MiddleCenter,
                        AlignmentPoint = btr.Origin,
                        Prompt = "Crate:",
                        Tag = "Crate",
                        TextString = "",
                        Height = 1,
                        Invisible = true,
                        LockPositionInBlock = true
                    };

                    btr.UpgradeOpen();
                    btr.AppendEntity(attTag);
                    btr.AppendEntity(attCrate);


                    #endregion
                    acBref.UpgradeOpen();
                    acBref.AppendAttributes(btr, acTrans);
                    acBref.DowngradeOpen();
                    btr.DowngradeOpen();
                }

                acTrans.Commit();
            }

            acCurDb.Audit(true, false);
        }
    }
}
