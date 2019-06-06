// -----------------------------------------------------------------------------------
//     <copyright file="DatabaseExtensions.cs" company="CraterSpace">
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
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Extensions
{
    public static class DatabaseExtentions
    {
        /// <summary>
        ///     Method to append an Entity to the Database and Return its new Object ID
        /// </summary>
        /// <param name="acEnt">The Entity To Append the the Database</param>
        /// <param name="acCurDb">The Current Working Database</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        /// <returns></returns>
        public static ObjectId AppendEntity(this Database acCurDb, Entity acEnt, Transaction acTrans)
        {
            // Open the Block currently active space for write
            var acBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

            acBlkTblRec?.AppendEntity(acEnt);
            acTrans.AddNewlyCreatedDBObject(acEnt, true);

            //Return the objectID
            return acEnt.ObjectId;
        }

        /// <summary>
        ///     Method to append an Entity to the Database and Return its new Object ID
        /// </summary>
        /// <param name="acEnt">The Entity To Append the the Database</param>
        /// <param name="acCurDb">The Current Working Database</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        /// <returns></returns>
        public static ObjectId AppendEntity(this Database acCurDb, Entity acEnt)
        {
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block currently active space for write
                var acBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                acBlkTblRec?.AppendEntity(acEnt);
                acTrans.AddNewlyCreatedDBObject(acEnt, true);

                acTrans.Commit();
            }

            //Return the objectID
            return acEnt.ObjectId;
        }


        /// <summary>
        ///     Method to parse all blocks in a database and update a specified attribute value
        /// </summary>
        /// <param name="db"></param>
        /// <param name="blockName"></param>
        /// <param name="attbName"></param>
        /// <param name="attbValue"></param>
        public static void UpdateAttributesInDatabase(this Database db, Document acCurDoc, string blockName,
            string attbName,
            string attbValue)
        {
            var ed = acCurDoc.Editor;
            // Get the IDs of the spaces we want to process
            // and simply call a function to process each
            ObjectId msId, psId;

            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable) acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
                msId = bt[BlockTableRecord.ModelSpace];
                psId = bt[BlockTableRecord.PaperSpace];

                // Not needed, but quicker than aborting
                acTrans.Commit();
            }

            var msCount = BlockExtensions.UpdateAttributesForAll(acCurDoc, msId, blockName, attbName, attbValue);
            var psCount = BlockExtensions.UpdateAttributesForAll(acCurDoc, psId, blockName, attbName, attbValue);
            ed.Regen();

            // Display the results
            ed.WriteMessage("\nProcessing file: " + db.Filename);
            ed.WriteMessage(
                "\nUpdated {0} instance{1} of " +
                "attribute {2} in the modelspace.",
                msCount,
                msCount == 1 ? "" : "s",
                attbName);

            ed.WriteMessage(
                "\nUpdated {0} instance{1} of " +
                "attribute {2} in the default paperspace.",
                psCount,
                psCount == 1 ? "" : "s",
                attbName);
        }

        #region Utility Methods

        /// <summary>
        ///     Utility Method To Check A Block ID And Copy It From One Database To another
        /// </summary>
        /// <param name="blkId">The Block ID to Copy</param>
        /// <param name="acExtDb">The Database To Clone From</param>
        /// <param name="acCurDb">The Database To Clone To</param>
        /// <param name="acTrans">The Transaction To Clone From</param>
        /// <param name="acCurEd">The Current Working Editor</param>
        public static void CheckAndCopyBlock(this Database acCurDb, ObjectId blkId, Database acExtDb,
            Transaction acTrans,
            Editor acCurEd)
        {
            // Create an ID collection to Save Matching Blocks to
            var extBlockIds = new ObjectIdCollection();

            // Get The External Block
            var extBlkRec = acTrans.GetObject(blkId, OpenMode.ForRead, false) as BlockTableRecord;

            // Only add named & non-layout blocks to the copy list
            if (extBlkRec != null && !extBlkRec.IsAnonymous && !extBlkRec.IsLayout) extBlockIds.Add(extBlkRec.Id);

            //If Found, Add the Block
            if (extBlockIds.Count != 0)
            {
                var iMap = new IdMapping();
                acExtDb.WblockCloneObjects(extBlockIds, acCurDb.BlockTableId, iMap, DuplicateRecordCloning.Replace,
                    false);
                acCurEd.WriteMessage("\nBlock Added: {0}", extBlkRec?.Name);
            }

            //Dispose of the Block Table Record
            extBlkRec?.Dispose();
        }

        /// <summary>
        ///     Method to zoom a extens on a newly created Database
        /// </summary>
        /// <param name="myDb"></param>
        public static void ZoomExtents(this Database myDb)
        {
            myDb.Orthomode = false;
            myDb.Isolines = 4;
            myDb.Ltscale = 1;
            myDb.Pdmode = 3;
            myDb.Visretain = true;


            using (var tm = myDb.TransactionManager.StartTransaction())
            {
                try
                {
                    if (myDb.TileMode)
                    {
                        var vpt = tm.GetObject(myDb.ViewportTableId, OpenMode.ForRead) as ViewportTable;
                        if (vpt != null)
                        {
                            var vptr = tm.GetObject(vpt["*Active"], OpenMode.ForWrite) as ViewportTableRecord;
                            myDb.UpdateExt(true);
                            var pt3Max = myDb.Extmax;
                            var pt3Min = myDb.Extmin;
                            var pt2Max = new Point2d(pt3Max.X, pt3Max.Y);
                            var pt2Min = new Point2d(pt3Min.X, pt3Min.Y);
                            if (vptr != null)
                            {
                                vptr.CenterPoint = pt2Min + (pt2Max - pt2Min) / 2.0;
                                vptr.Height = (pt2Max.Y - pt2Min.Y) * 1.2;
                                vptr.Width = (pt2Max.X - pt2Min.X) * 1.2;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //Ignored
                }
                finally
                {
                    tm.Commit();
                    tm.Dispose();
                }
            }
        }

        #endregion

        #region Methods to Get Data Tables From A Specified Database

        /// <summary>
        ///     Returns the Block Table of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Block Table: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static BlockTable GetBlockTable(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create the Block table Variable
            BlockTable acBlkTbl;

            if (accessMode == 0) //Open Mode For Read
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
            else // Open Mode For Write
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForWrite) as BlockTable;

            // Return the Block Table
            return acBlkTbl;
        }

        /// <summary>
        ///     Returns the DimStyle Table of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the DimStyle Table: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static DimStyleTable GetDimStyleTable(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create the DimStyle table Variable
            DimStyleTable acDimTbl;

            if (accessMode == 0) //Open Mode For Read
                acDimTbl = acTrans.GetObject(acCurDb.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
            else // Open Mode For Write
                acDimTbl = acTrans.GetObject(acCurDb.DimStyleTableId, OpenMode.ForWrite) as DimStyleTable;

            // Return the DimStyle Table
            return acDimTbl;
        }

        /// <summary>
        ///     Returns the Layer Table of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Layer Table: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static LayerTable GetLayerTable(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create the Layer table Variable
            LayerTable acLyrTbl;

            if (accessMode == 0) //Open Mode For Read
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;
            else // Open Mode For Write
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForWrite) as LayerTable;

            // Return the Layer Table
            return acLyrTbl;
        }

        /// <summary>
        ///     Returns the Line-type Table of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Line-type Table: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static LinetypeTable GetLinetypeTable(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create the Line-type table Variable
            LinetypeTable acLtTbl;

            if (accessMode == 0) //Open Mode For Read
                acLtTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
            else // Open Mode For Write
                acLtTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForWrite) as LinetypeTable;

            // Return the Line-type Table
            return acLtTbl;
        }

        /// <summary>
        ///     Returns the RegApp Table of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the RegApp Table: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static RegAppTable GetRegAppTable(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create the RegApp table Variable
            RegAppTable acRegTbl;

            if (accessMode == 0) //Open Mode For Read
                acRegTbl = acTrans.GetObject(acCurDb.RegAppTableId, OpenMode.ForRead) as RegAppTable;
            else // Open Mode For Write
                acRegTbl = acTrans.GetObject(acCurDb.RegAppTableId, OpenMode.ForWrite) as RegAppTable;

            // Return the RegApp Table
            return acRegTbl;
        }

        /// <summary>
        ///     Returns the TextStyle Table of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the TextStyle Table: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static TextStyleTable GetTextStyleTable(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create the TextStyle table Variable
            TextStyleTable acTxtTbl;

            if (accessMode == 0) //Open Mode For Read
                acTxtTbl = acTrans.GetObject(acCurDb.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
            else // Open Mode For Write
                acTxtTbl = acTrans.GetObject(acCurDb.TextStyleTableId, OpenMode.ForWrite) as TextStyleTable;

            // Return the TextStyle Table
            return acTxtTbl;
        }

        /// <summary>
        ///     Returns the Ucs Table of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Ucs Table: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static UcsTable GetUcsTable(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create the Ucs table Variable
            UcsTable acUcsTbl;

            if (accessMode == 0) //Open Mode For Read
                acUcsTbl = acTrans.GetObject(acCurDb.UcsTableId, OpenMode.ForRead) as UcsTable;
            else // Open Mode For Write
                acUcsTbl = acTrans.GetObject(acCurDb.UcsTableId, OpenMode.ForWrite) as UcsTable;

            // Return the Ucs Table
            return acUcsTbl;
        }

        /// <summary>
        ///     Returns the View Table of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the View Table: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static ViewTable GetViewTable(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create the View table Variable
            ViewTable acVwTbl;

            if (accessMode == 0) //Open Mode For Read
                acVwTbl = acTrans.GetObject(acCurDb.ViewTableId, OpenMode.ForRead) as ViewTable;
            else // Open Mode For Write
                acVwTbl = acTrans.GetObject(acCurDb.ViewTableId, OpenMode.ForWrite) as ViewTable;

            // Return the View Table
            return acVwTbl;
        }

        /// <summary>
        ///     Returns the Viewport Table of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Viewport Table: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static ViewportTable GetViewportTable(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create the Viewport table Variable
            ViewportTable acVpTbl;

            if (accessMode == 0) //Open Mode For Read
                acVpTbl = acTrans.GetObject(acCurDb.ViewportTableId, OpenMode.ForRead) as ViewportTable;
            else // Open Mode For Write
                acVpTbl = acTrans.GetObject(acCurDb.ViewportTableId, OpenMode.ForWrite) as ViewportTable;

            // Return the Viewport Table
            return acVpTbl;
        }

        #endregion

        #region Methods To Get Table Records From A Specified Database

        /// <summary>
        ///     Returns the BlockTableRecord of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Block Table Record: Read = 0 , Write = 1+</param>
        /// <param name="spaceType">Drawing Space to get the Block Table Record From: ModelSpace = 0, PaperSpace = 1+</param>
        /// <returns></returns>
        public static BlockTableRecord GetBlockTableRecord(this Database acCurDb, Transaction acTrans, int accessMode,
            int spaceType)
        {
            // Open the current Block table for read
            var acBlkTbl = GetBlockTable(acCurDb, acTrans, 0);

            // Creat block table record variable
            BlockTableRecord acBlkTblRec;

            if (spaceType == 0) //Open Model Space
            {
                if (accessMode == 0) // Open Mode For Read
                    acBlkTblRec =
                        acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                else // Open Mode For Write
                    acBlkTblRec =
                        acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            }
            else //Open Paper Space
            {
                if (accessMode == 0) // Open Mode For Read
                    acBlkTblRec =
                        acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForRead) as BlockTableRecord;
                else // Open Mode For Write
                    acBlkTblRec =
                        acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;
            }

            // Return the block table record
            return acBlkTblRec;
        }

        /// <summary>
        ///     Returns the Full DimStyle Table Record of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the DimStyle Table Record: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static List<DimStyleTableRecord> GetDimStyleTableRecord(this Database acCurDb, Transaction acTrans,
            int accessMode)
        {
            // Open the current DimStyle table for read
            var acDimTbl = GetDimStyleTable(acCurDb, acTrans, 0);

            // Create a list for storing the DimStyles
            var dimStyleList = new List<DimStyleTableRecord>();

            // Iterate through all DimStyles of the drawing and add them to the list
            foreach (var dimStyleId in acDimTbl)
            {
                // Create a DimStyle table record
                DimStyleTableRecord acDimTblRec;

                if (accessMode == 0) // Open For Read
                    acDimTblRec = acTrans.GetObject(dimStyleId, OpenMode.ForRead) as DimStyleTableRecord;
                else // Open For Write
                    acDimTblRec = acTrans.GetObject(dimStyleId, OpenMode.ForWrite) as DimStyleTableRecord;

                // Add the current iteration DimStyle to the list
                dimStyleList.Add(acDimTblRec);
            }

            //Return the list of the drawing's DimStyles
            return dimStyleList;
        }

        /// <summary>
        ///     Returns the Full Layer Table Record of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Block Table Record: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static List<LayerTableRecord> GetLayerTableRecord(this Database acCurDb, Transaction acTrans,
            int accessMode)
        {
            // Open the current layer table for read
            var acLyrTbl = GetLayerTable(acCurDb, acTrans, 0);

            // Create a list for storing the layers
            var layerList = new List<LayerTableRecord>();

            // Iterate through all layers of the drawing and add them to the list
            foreach (var layerId in acLyrTbl)
            {
                // Create a layer table record
                LayerTableRecord acLyrTblRec;

                if (accessMode == 0) // Open For Read
                    acLyrTblRec = acTrans.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;
                else // Open For Write
                    acLyrTblRec = acTrans.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;

                // Add the current iteration layer to the list
                layerList.Add(acLyrTblRec);
            }

            //Return the list of the drawing's layers
            return layerList;
        }

        /// <summary>
        ///     Returns the Full Linetype Table Record of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the LineType Table Record: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static List<LinetypeTableRecord> GetLinetypeTableRecord(this Database acCurDb, Transaction acTrans,
            int accessMode)
        {
            // Open the current line-type table for read
            var acLtTbl = GetLinetypeTable(acCurDb, acTrans, 0);

            // Create a list for storing the line-types
            var ltList = new List<LinetypeTableRecord>();

            // Iterate through all line-types of the drawing and add them to the list
            foreach (var ltId in acLtTbl)
            {
                // Create a linetype table record
                LinetypeTableRecord acLtTblRec;

                if (accessMode == 0) // Open For Read
                    acLtTblRec = acTrans.GetObject(ltId, OpenMode.ForRead) as LinetypeTableRecord;
                else // Open For Write
                    acLtTblRec = acTrans.GetObject(ltId, OpenMode.ForWrite) as LinetypeTableRecord;

                // Add the current iteration line-type to the list
                ltList.Add(acLtTblRec);
            }

            //Return the list of the drawing's line-types
            return ltList;
        }

        /// <summary>
        ///     Returns the Full RegApp TableRecord of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the RegApp Table Record: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static List<RegAppTableRecord> GetRegAppTableRecord(this Database acCurDb, Transaction acTrans,
            int accessMode)
        {
            // Open the current RegApp table for read
            var acRegTbl = GetRegAppTable(acCurDb, acTrans, 0);

            // Create a list for storing the RegApps
            var regAppList = new List<RegAppTableRecord>();

            // Iterate through all RegApps of the drawing and add them to the list
            foreach (var regAppId in acRegTbl)
            {
                // Create a RegApp table record
                RegAppTableRecord acRegTblRec;

                if (accessMode == 0) // Open For Read
                    acRegTblRec = acTrans.GetObject(regAppId, OpenMode.ForRead) as RegAppTableRecord;
                else // Open For Write
                    acRegTblRec = acTrans.GetObject(regAppId, OpenMode.ForWrite) as RegAppTableRecord;

                // Add the current iteration RegApp to the list
                regAppList.Add(acRegTblRec);
            }

            //Return the list of the drawing's RegApps
            return regAppList;
        }

        /// <summary>
        ///     Returns the Full TextStyle Table Record of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the TextStyle Table Record: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static List<TextStyleTableRecord> GetTextStyleTableRecord(this Database acCurDb, Transaction acTrans,
            int accessMode)
        {
            // Open the current TextStyle table for read
            var acTxtTbl = GetTextStyleTable(acCurDb, acTrans, 0);

            // Create a list for storing the TextStyles
            var textStyleList = new List<TextStyleTableRecord>();

            // Iterate through all TextStyles of the drawing and add them to the list
            foreach (var textStyleId in acTxtTbl)
            {
                // Create a TextStyle table record
                TextStyleTableRecord acTxtTblRec;

                if (accessMode == 0) // Open For Read
                    acTxtTblRec = acTrans.GetObject(textStyleId, OpenMode.ForRead) as TextStyleTableRecord;
                else // Open For Write
                    acTxtTblRec = acTrans.GetObject(textStyleId, OpenMode.ForWrite) as TextStyleTableRecord;

                // Add the current iteration TextStyle to the list
                textStyleList.Add(acTxtTblRec);
            }

            //Return the list of the drawing's TextStyles
            return textStyleList;
        }

        /// <summary>
        ///     Returns the Full Ucs TableRecord of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Ucs Table Record: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static List<UcsTableRecord> GetUcsTableRecord(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Open the current Ucs table for read
            var acUcsTbl = GetUcsTable(acCurDb, acTrans, 0);

            // Create a list for storing the Ucs
            var ucsList = new List<UcsTableRecord>();

            // Iterate through all Ucss of the drawing and add them to the list
            foreach (var ucsId in acUcsTbl)
            {
                // Create a Ucs table record
                UcsTableRecord acUcsTblRec;

                if (accessMode == 0) // Open For Read
                    acUcsTblRec = acTrans.GetObject(ucsId, OpenMode.ForRead) as UcsTableRecord;
                else // Open For Write
                    acUcsTblRec = acTrans.GetObject(ucsId, OpenMode.ForWrite) as UcsTableRecord;

                // Add the current iteration Ucs to the list
                ucsList.Add(acUcsTblRec);
            }

            //Return the list of the drawing's Ucs
            return ucsList;
        }

        /// <summary>
        ///     Returns the Full View TableRecord of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the View Table Record: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static List<ViewTableRecord> GetViewTableRecord(this Database acCurDb, Transaction acTrans,
            int accessMode)
        {
            // Open the current View table for read
            var acVwTbl = GetViewTable(acCurDb, acTrans, 0);

            // Create a list for storing the Views
            var viewList = new List<ViewTableRecord>();

            // Iterate through all Views of the drawing and add them to the list
            foreach (var viewId in acVwTbl)
            {
                // Create a View table record
                ViewTableRecord acVwTblRec;

                if (accessMode == 0) // Open For Read
                    acVwTblRec = acTrans.GetObject(viewId, OpenMode.ForRead) as ViewTableRecord;
                else // Open For Write
                    acVwTblRec = acTrans.GetObject(viewId, OpenMode.ForWrite) as ViewTableRecord;

                // Add the current iteration View to the list
                viewList.Add(acVwTblRec);
            }

            //Return the list of the drawing's Views
            return viewList;
        }

        /// <summary>
        ///     Returns the Full Viewport TableRecord of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Viewport Table Record: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static List<ViewportTableRecord> GetViewportTableRecord(this Database acCurDb, Transaction acTrans,
            int accessMode)
        {
            // Open the current Viewport table for read
            var acVpTbl = GetViewportTable(acCurDb, acTrans, 0);

            // Create a list for storing the Viewports
            var viewportList = new List<ViewportTableRecord>();

            // Iterate through all Viewports of the drawing and add them to the list
            foreach (var viewportId in acVpTbl)
            {
                // Create a Viewport table record
                ViewportTableRecord acVpTblRec;

                if (accessMode == 0) // Open For Read
                    acVpTblRec = acTrans.GetObject(viewportId, OpenMode.ForRead) as ViewportTableRecord;
                else // Open For Write
                    acVpTblRec = acTrans.GetObject(viewportId, OpenMode.ForWrite) as ViewportTableRecord;

                // Add the current iteration Viewport to the list
                viewportList.Add(acVpTblRec);
            }

            //Return the list of the drawing's Viewports
            return viewportList;
        }

        #endregion

        #region Methods To Get Object Dictionaries From A Specified Database

        /// <summary>
        ///     Returns the Layout Dictionary of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Dictionary: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static DBDictionary GetLayoutDictionary(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create The DBDictionary Object
            DBDictionary layoutDict;

            if (accessMode == 0) //Open Mode For Read
                layoutDict = acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
            else //Open Mode For Write
                layoutDict = acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

            // Return the layout dictionary
            return layoutDict;
        }

        /// <summary>
        ///     Returns the MLeader Style Dictionary of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Dictionary: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static DBDictionary GetMLeaderDictionary(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create The DBDictionary Object
            DBDictionary mleaderDict;

            if (accessMode == 0) //Open Mode For Read
                mleaderDict = acTrans.GetObject(acCurDb.MLeaderStyleDictionaryId, OpenMode.ForRead) as DBDictionary;
            else //Open Mode For Write
                mleaderDict = acTrans.GetObject(acCurDb.MLeaderStyleDictionaryId, OpenMode.ForRead) as DBDictionary;

            // Return the MLeader Stlye Dictionary
            return mleaderDict;
        }

        /// <summary>
        ///     Returns the Section View Style Dictionary of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Dictionary: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static DBDictionary GetViewSectionDictionary(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create The DBDictionary Object
            DBDictionary viewSectDict;

            if (accessMode == 0) //Open Mode For Read
                viewSectDict =
                    acTrans.GetObject(acCurDb.SectionViewStyleDictionaryId, OpenMode.ForRead) as DBDictionary;
            else //Open Mode For Write
                viewSectDict =
                    acTrans.GetObject(acCurDb.SectionViewStyleDictionaryId, OpenMode.ForRead) as DBDictionary;

            // Return the Section View Style Dictionary
            return viewSectDict;
        }

        /// <summary>
        ///     Returns the Detail View Style Dictionary of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Dictionary: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static DBDictionary GetViewDetailDictionary(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create The DBDictionary Object
            DBDictionary viewDetDict;

            if (accessMode == 0) //Open Mode For Read
                viewDetDict = acTrans.GetObject(acCurDb.DetailViewStyleDictionaryId, OpenMode.ForRead) as DBDictionary;
            else //Open Mode For Write
                viewDetDict = acTrans.GetObject(acCurDb.DetailViewStyleDictionaryId, OpenMode.ForRead) as DBDictionary;

            // Return the Detail View Style Dictionary
            return viewDetDict;
        }

        /// <summary>
        ///     Returns the Table Style  Dictionary of the specified database.
        /// </summary>
        /// <param name="acCurDb">The Specified Working Database</param>
        /// <param name="acTrans">The Specified Working Transaction</param>
        /// <param name="accessMode">Access Mode of the Dictionary: Read = 0 , Write = 1+</param>
        /// <returns></returns>
        public static DBDictionary GetTableStyleDictionary(this Database acCurDb, Transaction acTrans, int accessMode)
        {
            // Create The DBDictionary Object
            DBDictionary tblDict;

            if (accessMode == 0) //Open Mode For Read
                tblDict = acTrans.GetObject(acCurDb.TableStyleDictionaryId, OpenMode.ForRead) as DBDictionary;
            else //Open Mode For Write
                tblDict = acTrans.GetObject(acCurDb.TableStyleDictionaryId, OpenMode.ForRead) as DBDictionary;

            // Return the Table Style Dictionary
            return tblDict;
        }

        #endregion

        #region Methods To Create New Objects In The Current Database

        /// <summary>
        ///     Method To Add A New Layer To The Current Drawing
        /// </summary>
        /// <param name="name">The Name of the New Layer</param>
        /// <param name="isOff">Is The New Layer Turned Off?</param>
        /// <param name="isFrozen">Is The New Layer Frozen?</param>
        /// <param name="isLocked">Is The New Layer Locked?</param>
        /// <param name="isPlottable">Is The New Layer Plottable?</param>
        /// <param name="color">The Color of the New Layer (In Autocad Color Method)</param>
        /// <param name="lType">The LineType of the New Layer</param>
        /// <param name="lWeight">The Lineweight of the New Layer</param>
        /// <param name="isVpFreeze">Is the New Layer Frozen In New Viewports?</param>
        /// <param name="desc">The Layer Description</param>
        /// <param name="trans">The Layer Transparency (In Bytes)</param>
        /// <param name="acLyrTbl">The Current Working Layer Table</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        public static void AddLayer(string name, bool isOff, bool isFrozen, bool isLocked, bool isPlottable,
            Color color, LinetypeTableRecord lType, LineWeight lWeight, bool isVpFreeze, string desc,
            byte trans, LayerTable acLyrTbl, Transaction acTrans)
        {
            // Check If Layer Exists In Layer Table
            if (acLyrTbl.Has(name)) return;

            // Create The New Layer
            var acLyrTblRec = new LayerTableRecord
            {
                Name = name,
                Color = color,
                LineWeight = lWeight,
                LinetypeObjectId = lType.ObjectId,
                IsOff = isOff,
                IsFrozen = isFrozen,
                IsLocked = isLocked,
                IsPlottable = isPlottable,
                ViewportVisibilityDefault = isVpFreeze,
                Description = desc,
                Transparency = new Transparency(trans)
            };

            // Upgrade the Layer Table For Write
            acLyrTbl.UpgradeOpen();

            // Append the New Layer to the Layer Table & Transaction
            acLyrTbl.Add(acLyrTblRec);
            acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
        }

        /// <summary>
        ///     Quick utility to add a layer to AutoCAD
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <param name="lType"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acTrans"></param>
        public static void AddLayer(this Database acCurDb, string name, Color color, string lType, Transaction acTrans)
        {
            var acLyrTbl = GetLayerTable(acCurDb, acTrans, 0);

            // Check If Layer Exists In Layer Table
            if (acLyrTbl.Has(name)) return;

            var acLtTable = GetLinetypeTable(acCurDb, acTrans, 0);

            acCurDb.AddLinetype(lType, acLtTable, acTrans);

            // Create The New Layer
            var acLyrTblRec = new LayerTableRecord
            {
                Name = name,
                Color = color
            };

            if (acLtTable.Has(lType))
            {
                // Upgrade the Layer Table Record for write
                acLtTable.UpgradeOpen();

                // Set the linetype for the layer
                acLyrTblRec.LinetypeObjectId = acLtTable[lType];
            }

            // Upgrade the Layer Table For Write
            acLyrTbl.UpgradeOpen();

            // Append the New Layer to the Layer Table & Transaction
            acLyrTbl.Add(acLyrTblRec);
            acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
        }

        /// <summary>
        ///     Method To Add A Linetype From the Acad.Lin File To The Current Drawing
        /// </summary>
        /// <param name="name">The Name of the Linetype</param>
        /// <param name="acLtTbl">The Current Working Linetype Table</param>
        /// <param name="acCurDb">The Current Working Database</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        public static void AddLinetype(this Database acCurDb, string name, LinetypeTable acLtTbl, Transaction acTrans)
        {
            // Check If Linetype Exists In Linetype Table
            if (acLtTbl.Has(name)) return;

            //Load the Linetype from the main Autocad Linetype File
            acCurDb.LoadLineTypeFile(name, "acad.lin");
        }

        #endregion

        #region Methods To Copy Object Styles From One DWG to Another

        /// <summary>
        ///     Method to Copy Dimension Styles from an External Database to the Current Database
        /// </summary>
        /// <param name="acCurEd">The Current Working Editor</param>
        /// <param name="acExtDb">The Database To Copy From</param>
        /// <param name="acCurDb">The Database to Copy To</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        public static void CopyAllDimStyles(this Database acCurDb, Editor acCurEd, Database acExtDb,
            Transaction acTrans)
        {
            // Get Current Working DimStyle Table
            var curDimStyleTbl = GetDimStyleTable(acCurDb, acTrans, 1);

            using (var acExtTrans = acExtDb.TransactionManager.StartTransaction())
            {
                // Iterate through Dimstyles in External Database and add them to the current Database if they do not exist
                foreach (var dimStyle in GetDimStyleTableRecord(acExtDb, acExtTrans, 0))
                {
                    // If current dim style table contains the style, continue
                    if (curDimStyleTbl.Has(dimStyle.Name)) continue;

                    // If name didnt exist in current database, copy its contents to a style that can be added to the current database
                    var transitionStyle = new DimStyleTableRecord();
                    transitionStyle.CopyFrom(dimStyle);

                    //Add the dimstyles to the current database
                    curDimStyleTbl.Add(transitionStyle);
                    acTrans.AddNewlyCreatedDBObject(transitionStyle, true);
                    acCurEd.WriteMessage("\nDimstyle Added: {0}", transitionStyle.Name);
                }

                // Commit the external transaction
                acExtTrans.Commit();
            }
        }

        /// <summary>
        ///     Method to Copy Layers from an External Database to the Current Database
        /// </summary>
        /// <param name="acCurEd">The Current Working Edito</param>
        /// <param name="acExtDb">The Database To Copy From</param>
        /// <param name="acCurDb">The Database to Copy To</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        public static void CopyAllLayers(this Database acCurDb, Editor acCurEd, Database acExtDb, Transaction acTrans)
        {
            // Get Current Working Layer Table
            var curLayerTbl = GetLayerTable(acCurDb, acTrans, 1);

            using (var acExtTrans = acExtDb.TransactionManager.StartTransaction())
            {
                // Iterate through Layers in External Database and add them to the current Database if they do not exist
                foreach (var layer in GetLayerTableRecord(acExtDb, acExtTrans, 0))
                {
                    // If current layer table table contains the style, continue
                    if (curLayerTbl.Has(layer.Name)) continue;

                    // If name didnt exist in current database, copy its contents to a layer that can be added to the current database
                    var transitionlayer = (LayerTableRecord) layer.Clone();

                    //Add the dimstyles to the current database
                    curLayerTbl.Add(transitionlayer);
                    acTrans.AddNewlyCreatedDBObject(transitionlayer, true);
                    acCurEd.WriteMessage("\nLayer Added: {0}", transitionlayer.Name);
                }

                acCurDb.Audit(true, false);

                // Commit the external transaction
                acExtTrans.Commit();
            }
        }

        /// <summary>
        ///     Method to Copy MLeader Styles from an External Database to the Current Database
        /// </summary>
        /// <param name="acCurEd">The Current Working Edito</param>
        /// <param name="acExtDb">The Database To Copy From</param>
        /// <param name="acCurDb">The Database to Copy To</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        public static void CopyAllMlStyles(this Database acCurDb, Editor acCurEd, Database acExtDb, Transaction acTrans)
        {
            // Get Current Working Layer Table
            var curMlDict = GetMLeaderDictionary(acCurDb, acTrans, 0);

            using (var acExtTrans = acExtDb.TransactionManager.StartTransaction())
            {
                var extMlStyles = GetMLeaderDictionary(acExtDb, acExtTrans, 0);

                // Iterate through Layers in External Database and add them to the current Database if they do not exist
                foreach (var mLeaderStyle in extMlStyles)
                {
                    // If current layer table table contains the style, continue
                    if (curMlDict.Contains(mLeaderStyle.Key)) continue;

                    // Get Object Id of Current MLeaderStyle
                    var id = extMlStyles.GetAt(mLeaderStyle.Key);

                    // Get The Current MLeaderStyle
                    var currentStyle = acExtTrans.GetObject(id, OpenMode.ForWrite) as MLeaderStyle;

                    // If name didnt exist in current database, copy its contents to a new MLeaderStyle that can be added to the current database
                    var newMlStyle = (MLeaderStyle) currentStyle?.Clone();

                    //Get Current Working Block Table
                    var curBlkTbl = GetBlockTable(acCurDb, acTrans, 0);

                    // Check If the Style Uses Blocks - Import them if so
                    if (currentStyle != null && currentStyle.ArrowSymbolId != ObjectId.Null)
                    {
                        // Get The External Block
                        var extBlkRec =
                            acExtTrans.GetObject(currentStyle.ArrowSymbolId, OpenMode.ForRead, false) as
                                BlockTableRecord;
                        var extBlkName = extBlkRec?.Name;

                        if (!curBlkTbl.Has(extBlkName))
                            acCurDb.CheckAndCopyBlock(currentStyle.ArrowSymbolId, acExtDb, acExtTrans, acCurEd);

                        // Set The Arrow Symbol
                        newMlStyle.ArrowSymbolId = curBlkTbl[extBlkName];
                    }

                    if (currentStyle != null && currentStyle.ContentType == ContentType.BlockContent)
                    {
                        // Get The External Block
                        var extBlkRec =
                            acExtTrans.GetObject(currentStyle.BlockId, OpenMode.ForRead, false) as BlockTableRecord;
                        var extBlkName = extBlkRec?.Name;

                        if (!curBlkTbl.Has(extBlkName))
                            acCurDb.CheckAndCopyBlock(currentStyle.BlockId, acExtDb, acExtTrans, acCurEd);

                        // Set The Block Symbol
                        newMlStyle.BlockId = curBlkTbl[extBlkName];
                    }

                    //Add the MLeaderStyle to the current database
                    newMlStyle?.PostMLeaderStyleToDb(acCurDb, currentStyle.Name);
                    acTrans.AddNewlyCreatedDBObject(newMlStyle, true);
                    acCurEd.WriteMessage("\nMLeaderStyle Added: {0}\n", currentStyle?.Name);
                }

                // Commit the external transaction
                acExtTrans.Commit();
            }
        }

        #endregion

        #region Purge

        public static bool PurgeSymbolTables(Database db, ObjectIdCollection tableIds, bool silent)
        {
            var itemsPurged = false;
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;

            var purgeableIds = new ObjectIdCollection();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId tableId in tableIds)
                {
                    var table = (SymbolTable) tr.GetObject(tableId, OpenMode.ForRead, false);
                    foreach (var recordId in table)
                        purgeableIds.Add(recordId);
                }

                db.Purge(purgeableIds);

                if (purgeableIds.Count == 0) return false;
                itemsPurged = true;

                foreach (ObjectId id in purgeableIds)
                    try
                    {
                        var record = (SymbolTableRecord) tr.GetObject(id, OpenMode.ForWrite);
                        var recordName = record.Name;
                        record.Erase();
                        if (!silent)
                            if (!recordName.Contains("|"))
                                ed.WriteMessage("\nPurging " + record.GetType().Name + " " + recordName);
                    }
                    catch (Exception e)
                    {
                        if (e.ErrorStatus == ErrorStatus.CannotBeErasedByCaller ||
                            e.ErrorStatus == (ErrorStatus) 20072)
                            itemsPurged = false;
                        else
                            throw e;
                    }

                tr.Commit();
            }

            return itemsPurged;
        }

        public static bool PurgeDictionaries(Database db, ObjectIdCollection dictIds, bool silent)
        {
            var itemsPurged = false;
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;

            var purgeableIds = new ObjectIdCollection();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId dictId in dictIds)
                {
                    var dict = (DBDictionary) tr.GetObject(dictId, OpenMode.ForRead, false);
                    foreach (var entry in dict) purgeableIds.Add(entry.m_value);
                }

                db.Purge(purgeableIds);

                if (purgeableIds.Count == 0) return false;
                itemsPurged = true;

                var nod = (DBDictionary) tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
                foreach (ObjectId id in purgeableIds)
                    try
                    {
                        var obj = tr.GetObject(id, OpenMode.ForWrite);
                        obj.Erase();
                        if (!silent)
                            foreach (ObjectId dictId in dictIds)
                            {
                                var dict = (DBDictionary) tr.GetObject(dictId, OpenMode.ForRead, false);
                                var dictName = nod.NameAt(dictId);
                                if (dict.Contains(id))
                                {
                                    ed.WriteMessage("\nPurging " + dict.NameAt(id) + " from " + dictName);
                                    break;
                                }
                            }
                    }
                    catch (Exception e)
                    {
                        if (e.ErrorStatus == ErrorStatus.CannotBeErasedByCaller ||
                            e.ErrorStatus == (ErrorStatus) 20072)
                            itemsPurged = false;
                        else
                            throw e;
                    }

                tr.Commit();
            }

            return itemsPurged;
        }

        public static void PurgeAll(this Database db, bool silent)
        {
            var tableIds = new ObjectIdCollection();
            tableIds.Add(db.BlockTableId);
            tableIds.Add(db.DimStyleTableId);
            tableIds.Add(db.LayerTableId);
            tableIds.Add(db.LinetypeTableId);
            tableIds.Add(db.RegAppTableId);
            tableIds.Add(db.TextStyleTableId);
            tableIds.Add(db.UcsTableId);
            tableIds.Add(db.ViewportTableId);
            tableIds.Add(db.ViewTableId);
            var dictIds = new ObjectIdCollection();
            dictIds.Add(db.MaterialDictionaryId);
            dictIds.Add(db.MLStyleDictionaryId);
            dictIds.Add(db.MLeaderStyleDictionaryId);
            dictIds.Add(db.PlotStyleNameDictionaryId);
            dictIds.Add(db.TableStyleDictionaryId);
            dictIds.Add(db.VisualStyleDictionaryId);
            while (PurgeSymbolTables(db, tableIds, silent) || PurgeDictionaries(db, dictIds, silent))
                continue;
        }
        #endregion
    }
}