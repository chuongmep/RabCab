﻿// -----------------------------------------------------------------------------------
//     <copyright file="BlockExtensions.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace RabCab.Extensions
{
    public static class BlockExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="acBlkTableRec"></param>
        /// <param name="blockName"></param>
        /// <param name="insertPt"></param>
        /// <param name="acCurDb"></param>
        /// <returns></returns>
        public static BlockReference InsertBlock(this BlockTableRecord acBlkTableRec, string blockName,
            Point3d insertPt,
            Database acCurDb)
        {
            BlockReference acBlkRef = null;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (acBlkTbl != null && acBlkTbl.Has(blockName))
                {
                    var blkRecId = acBlkTbl[blockName];

                    // Insert the block into the current space
                    if (blkRecId != ObjectId.Null)
                    {
                        acBlkRef = new BlockReference(insertPt, blkRecId);
                        acBlkTableRec?.AppendEntity(acBlkRef);
                        acTrans.AddNewlyCreatedDBObject(acBlkRef, true);
                        acBlkRef.AppendAttributes(blkRecId, acTrans);
                    }

                    acTrans.Commit();
                }
            }

            return acBlkRef;
        }

        /// <summary>
        ///     Get all references to the given BlockTableRecord, including
        ///     references to anonymous dynamic BlockTableRecords.
        /// </summary>
        public static IEnumerable<BlockReference> GetBlockReferences(
            this BlockTableRecord btr,
            OpenMode mode = OpenMode.ForRead,
            bool directOnly = true)
        {
            if (btr == null)
                throw new ArgumentNullException("btr");
            var tr = btr.Database.TransactionManager.TopTransaction;
            if (tr == null)
                throw new InvalidOperationException("No transaction");
            var ids = btr.GetBlockReferenceIds(directOnly, true);
            var cnt = ids.Count;
            for (var i = 0; i < cnt; i++)
                yield return (BlockReference)
                    tr.GetObject(ids[i], mode, false, false);
            if (btr.IsDynamicBlock)
            {
                BlockTableRecord btr2 = null;
                var blockIds = btr.GetAnonymousBlockIds();
                cnt = blockIds.Count;
                for (var i = 0; i < cnt; i++)
                {
                    btr2 = (BlockTableRecord) tr.GetObject(blockIds[i],
                        OpenMode.ForRead, false, false);
                    ids = btr2.GetBlockReferenceIds(directOnly, true);
                    var cnt2 = ids.Count;
                    for (var j = 0; j < cnt2; j++)
                        yield return (BlockReference)
                            tr.GetObject(ids[j], mode, false, false);
                }
            }
        }

        /// <summary>
        ///     Utility method to set block attributes
        /// </summary>
        /// <param name="acBlkRef"></param>
        /// <param name="blkRecId"></param>
        /// <param name="acTrans"></param>
        public static void AppendAttributes(this BlockReference acBlkRef, BlockTableRecord acBlkTblRec,
            Transaction acTrans)
        {
            // Verify block table record has attribute definitions associated with it
            if (acBlkTblRec != null && acBlkTblRec.HasAttributeDefinitions)
                foreach (var objId in acBlkTblRec)
                {
                    var dbObj = acTrans.GetObject(objId, OpenMode.ForRead);

                    if (!(dbObj is AttributeDefinition acAtt)) continue;

                    if (acAtt.Constant) continue;

                    using (var acAttRef = new AttributeReference())
                    {
                        acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);

                        if (!acBlkRef.ContainsAttributeDef(acAtt.Tag, acTrans))
                        {
                            acAttRef.TextString = acAtt.TextString;
                            acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                            acTrans.AddNewlyCreatedDBObject(acAttRef, true);
                        }
                    }
                }
        }

        /// <summary>
        ///     Utility method to set block attributes
        /// </summary>
        /// <param name="acBlkRef"></param>
        /// <param name="acBlkTblRec"></param>
        /// <param name="acTrans"></param>
        public static void AppendAttributes(this BlockReference acBlkRef, ObjectId blkRecId, Transaction acTrans)
        {
            BlockTableRecord acBlkTblRec;
            acBlkTblRec = acTrans.GetObject(blkRecId, OpenMode.ForRead) as BlockTableRecord;

            // Verify block table record has attribute definitions associated with it
            if (acBlkTblRec.HasAttributeDefinitions)
            {
                // Add attributes from the block table record
                foreach (var objID in acBlkTblRec)
                {
                    var dbObj = acTrans.GetObject(objID, OpenMode.ForRead);

                    if (dbObj is AttributeDefinition)
                    {
                        var acAtt = dbObj as AttributeDefinition;

                        if (!acAtt.Constant)
                        {
                            using (var acAttRef = new AttributeReference())
                            {
                                acAttRef.SetAttributeFromBlock(acAtt, acBlkRef.BlockTransform);
                                acAttRef.TextString = acAtt.TextString;
                                acBlkRef.AttributeCollection.AppendAttribute(acAttRef);
                                acTrans.AddNewlyCreatedDBObject(acAttRef, true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Method to replace an Attribute by its name
        /// </summary>
        /// <param name="br"></param>
        /// <param name="attbName"></param>
        /// <param name="attbValue"></param>
        /// <param name="acCurDoc"></param>
        /// <param name="acCurEd"></param>
        public static void UpdateAttributeByName(this BlockReference br, string attbName, string attbValue,
            Document acCurDoc, Editor acCurEd)
        {
            using (var acTrans = acCurDoc.TransactionManager.StartTransaction())
            {
                // Check each of the attributes...
                foreach (ObjectId arId in br.AttributeCollection)
                {
                    var obj = acTrans.GetObject(arId, OpenMode.ForRead);

                    var ar = obj as AttributeReference;

                    if (ar != null)
                        if (ar.Tag.ToUpper() == attbName)
                        {
                            // If so, update the value
                            // and increment the counter
                            ar.UpgradeOpen();
                            ar.TextString = attbValue;
                            ar.DowngradeOpen();
                        }
                }

                acCurEd.Regen();

                acTrans.Commit();
            }
        }

        /// <summary>
        ///     Method to replace an Attribute by its name
        /// </summary>
        /// <param name="br"></param>
        /// <param name="attbName"></param>
        /// <param name="attbValue"></param>
        /// <param name="acCurDoc"></param>
        /// <param name="acCurEd"></param>
        public static void UpdateAttributeByTag(this BlockReference br, string attbName, string attbValue,
            Document acCurDoc, Editor acCurEd, Transaction acTrans)
        {
            // Check each of the attributes...
            foreach (ObjectId arId in br.AttributeCollection)
            {
                var obj = acTrans.GetObject(arId, OpenMode.ForRead);

                var ar = obj as AttributeReference;

                if (ar == null) continue;
                if (ar.Tag.ToUpper() == attbName)
                {
                    // If so, update the value
                    // and increment the counter
                    ar.UpgradeOpen();
                    ar.TextString = attbValue;
                    ar.DowngradeOpen();
                }
            }

            //acCurEd.Regen();
        }

        /// <summary>
        ///     Method to replace an attribute text by substitution
        /// </summary>
        /// <param name="br"></param>
        /// <param name="subName"></param>
        /// <param name="attbValue"></param>
        /// <param name="acCurDoc"></param>
        /// <param name="acCurEd"></param>
        public static void UpdateAttributeBySubstitution(this BlockReference br, string subName, string attbValue,
            Document acCurDoc, Editor acCurEd)
        {
            using (var acTrans = acCurDoc.TransactionManager.StartTransaction())
            {
                // Check each of the attributes...
                foreach (ObjectId arId in br.AttributeCollection)
                {
                    var obj = acTrans.GetObject(arId, OpenMode.ForRead);

                    var ar = obj as AttributeReference;

                    if (ar != null)
                    {
                        ar.UpgradeOpen();

                        var subBrackets = "[" + subName + "]";

                        if (ar.TextString.Contains(subBrackets))
                        {
                            var newText = ar.TextString;
                            newText = newText.Replace(subBrackets, attbValue);
                            ar.TextString = newText;
                        }

                        ar.DowngradeOpen();
                    }
                }

                acCurEd.Regen();
                acTrans.Commit();
            }
        }

        /// <summary>
        ///     Method to replace an attribute text by substitution
        /// </summary>
        /// <param name="mLeader"></param>
        /// <param name="subName"></param>
        /// <param name="attbValue"></param>
        /// <param name="btr"></param>
        /// <param name="acCurDoc"></param>
        /// <param name="acCurEd"></param>
        public static void UpdateMleaderAttributeBySubstitution(this BlockTableRecord btr, MLeader mLeader,
            string subName, string attbValue, Document acCurDoc, Editor acCurEd)
        {
            using (var acTrans = acCurDoc.TransactionManager.StartTransaction())
            {
                // Check each of the attributes...
                foreach (var arId in btr)
                {
                    var aDef = acTrans.GetObject(arId, OpenMode.ForWrite) as AttributeDefinition;
                    var ml = acTrans.GetObject(mLeader.ObjectId, OpenMode.ForWrite) as MLeader;

                    if (aDef != null)
                        if (ml != null)
                        {
                            var ar = ml.GetBlockAttribute(aDef.Id);

                            if (ar != null)
                            {
                                //ar.UpgradeOpen();

                                var subBrackets = "[" + subName + "]";

                                if (ar.TextString.Contains(subBrackets))
                                {
                                    var newText = ar.TextString;
                                    newText = newText.Replace(subBrackets, attbValue);
                                    ar.TextString = newText;
                                }

                                //ar.DowngradeOpen();
                            }

                            ml.SetBlockAttribute(aDef.Id, ar);
                        }
                }

                acCurEd.Regen();
                acTrans.Commit();
            }
        }

        /// <summary>
        ///     Method to replace an attribute text by substitution
        /// </summary>
        /// <param name="br"></param>
        /// <param name="acCurDoc"></param>
        /// <param name="acCurEd"></param>
        public static void ClearAttributes(this BlockReference br, Document acCurDoc, Editor acCurEd)
        {
            using (var acTrans = acCurDoc.TransactionManager.StartTransaction())
            {
                // Check each of the attributes...
                foreach (ObjectId arId in br.AttributeCollection)
                {
                    var obj = acTrans.GetObject(arId, OpenMode.ForRead);

                    var ar = obj as AttributeReference;

                    if (ar != null)
                    {
                        ar.UpgradeOpen();
                        ar.TextString = string.Empty;
                        ar.DowngradeOpen();
                    }
                }

                acCurEd.Regen();
                acTrans.Commit();
            }
        }

        /// <summary>
        ///     Method to recursively change an attribute in all blocks of a database
        /// </summary>
        /// <param name="acCurDoc" />
        /// <param name="objId"></param>
        /// <param name="blockName"></param>
        /// <param name="attbName"></param>
        /// <param name="attbValue"></param>
        /// <returns></returns>
        public static int UpdateAttributesForAll(Document acCurDoc, ObjectId objId, string blockName, string attbName,
            string attbValue)
        {
            // Will return the number of attributes modified
            var changedCount = 0;

            using (var acTrans = acCurDoc.TransactionManager.StartTransaction())
            {
                var btr = (BlockTableRecord) acTrans.GetObject(objId, OpenMode.ForRead);

                // Test each entity in the container...
                foreach (var entId in btr)
                {
                    var ent = acTrans.GetObject(entId, OpenMode.ForRead) as Entity;

                    if (ent != null)
                    {
                        var br = ent as BlockReference;

                        if (br != null)
                        {
                            var bd = (BlockTableRecord) acTrans.GetObject(br.BlockTableRecord, OpenMode.ForRead);

                            // ... to see whether it's a block with
                            // the name we're after
                            if (bd.Name.ToUpper() == blockName)
                                foreach (ObjectId arId in br.AttributeCollection)
                                {
                                    var obj = acTrans.GetObject(arId, OpenMode.ForRead);

                                    var ar = obj as AttributeReference;

                                    if (ar != null)
                                        if (ar.Tag.ToUpper() == attbName)
                                        {
                                            // If so, update the value
                                            // and increment the counter
                                            ar.UpgradeOpen();

                                            ar.TextString = attbValue;
                                            ar.DowngradeOpen();
                                            changedCount++;
                                        }
                                }

                            // Recurse for nested blocks
                            changedCount += UpdateAttributesForAll(acCurDoc, br.BlockTableRecord, blockName, attbName,
                                attbValue);
                        }
                    }

                    acTrans.Commit();
                }
            }

            return changedCount;
        }

        public static bool ContainsAttributeDef(this BlockTableRecord acBlkRc, string Tag, Transaction acTrans)
        {
            foreach (var objId in acBlkRc)
            {
                var obj = acTrans.GetObject(objId, OpenMode.ForRead);

                var aDef = obj as AttributeDefinition;

                if (aDef == null) continue;
                if (aDef.Tag == Tag) return true;
            }

            return false;
        }

        public static bool ContainsAttributeDef(this BlockReference acBlkRef, string Tag, Transaction acTrans)
        {
            foreach (ObjectId objId in acBlkRef.AttributeCollection)
            {
                var obj = acTrans.GetObject(objId, OpenMode.ForRead);

                var aRef = obj as AttributeReference;

                if (aRef == null) continue;
                if (aRef.Tag == Tag) return true;
            }

            return false;
        }

        public static ObjectIdCollection ExplodeBlock(this BlockReference br, Transaction tr, Database db,
            bool erase = true)
        {
            // We'll collect the BlockReferences created in a collection
            var toExplode = new ObjectIdCollection();
            var objIds = new ObjectIdCollection();

            // Define our handler to capture the nested block references
            void Handler(object s, ObjectEventArgs e)
            {
                switch (e.DBObject)
                {
                    case BlockReference _:
                        toExplode.Add(e.DBObject.ObjectId);
                        break;
                    default:
                        objIds.Add(e.DBObject.ObjectId);
                        break;
                }
            }

            // Add our handler around the explode call, removing it
            // directly afterwards

            db.ObjectAppended += Handler;
            br.ExplodeToOwnerSpace();
            db.ObjectAppended -= Handler;

            // Go through the results and recurse, exploding the
            // contents

            foreach (ObjectId bid in toExplode)
            {
                var nBr = (BlockReference) tr.GetObject(bid, OpenMode.ForRead);

                var exIds = nBr.ExplodeBlock(tr, db);

                foreach (ObjectId id in exIds) objIds.Add(id);
            }


            // We might also just let it drop out of scope
            toExplode.Clear();

            // To replicate the explode command, we're delete the
            // original entity

            if (erase)
            {
                br.UpgradeOpen();
                br.Erase();
                br.DowngradeOpen();
            }

            return objIds;
        }
    }
}