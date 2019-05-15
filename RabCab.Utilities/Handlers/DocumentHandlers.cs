// -----------------------------------------------------------------------------------
//     <copyright file="DocumentHandlers.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using RabCab.Agents;
using RabCab.Commands.PaletteKit;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Handlers
{
    /// <summary>
    ///     TODO
    /// </summary>
    internal static class DocumentHandlers
    {
        /// <summary>
        ///     TODO
        /// </summary>
        internal static void AddDocEvents()
        {
            try
            {
                // Get the current document
                var acDocMan = Application.DocumentManager;
                var acDoc = acDocMan.MdiActiveDocument;

                //Doc Manager Handlers
                acDocMan.DocumentToBeDeactivated += BeginDocClose;
                acDocMan.DocumentActivated += DocActivated;

                //Doc Handlers
                acDoc.ImpliedSelectionChanged += Doc_ImpliedSelectionChanged;
                acDoc.Database.ObjectModified += Database_ObjectModified;
                acDoc.Database.ObjectErased += Database_ObjectErased;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        internal static void RemoveDocEvents()
        {
            try
            {
                // Get the current document
                var acDocMan = Application.DocumentManager;
                var acDoc = acDocMan.MdiActiveDocument;

                //Doc Manager Handlers
                acDocMan.DocumentToBeDeactivated -= BeginDocClose;
                acDocMan.DocumentActivated -= DocActivated;

                //Doc Handlers
                acDoc.ImpliedSelectionChanged -= Doc_ImpliedSelectionChanged;
                acDoc.Database.ObjectModified -= Database_ObjectModified;
                acDoc.Database.ObjectErased -= Database_ObjectErased;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void DocActivated(object senderObj,
            DocumentCollectionEventArgs docActEvent)
        {
            try
            {
                //Notebook Handlers
                if (SettingsInternal.EnNotePal) RcPaletteNotebook.UpdNotePal();

                if (Application.DocumentManager.CurrentDocument != null)
                {
                    Application.DocumentManager.CurrentDocument.ImpliedSelectionChanged += Doc_ImpliedSelectionChanged;
                    Application.DocumentManager.CurrentDocument.Database.ObjectModified += Database_ObjectModified;
                    Application.DocumentManager.CurrentDocument.Database.ObjectErased += Database_ObjectErased;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="senderObj"></param>
        /// <param name="docBegClsEvtArgs"></param>
        private static void BeginDocClose(object senderObj,
            DocumentCollectionEventArgs docBegClsEvtArgs)
        {
            try
            {
                if (Application.DocumentManager.CurrentDocument != null)
                {
                    Application.DocumentManager.CurrentDocument.ImpliedSelectionChanged -= Doc_ImpliedSelectionChanged;
                    Application.DocumentManager.CurrentDocument.Database.ObjectModified -= Database_ObjectModified;
                    Application.DocumentManager.CurrentDocument.Database.ObjectErased -= Database_ObjectErased;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        private static void Doc_ImpliedSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (SettingsInternal.EnMetPal == false) return;

                if (RcPaletteMetric.RcPal == null) return;

                var acCurDoc = Application.DocumentManager.MdiActiveDocument;
                if (acCurDoc == null) return;

                var acCurDb = acCurDoc.Database;
                var acCurEd = acCurDoc.Editor;

                var selRes = acCurEd.SelectImplied();

                if (selRes.Status == PromptStatus.OK)
                {
                    var objIds = selRes.Value.GetObjectIds();
                    RcPaletteMetric.ParseAndFill(objIds, acCurDb);
                }
                else
                {
                    RcPaletteMetric.ParseAndFill(new ObjectId[0], acCurDb);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }


        private static void Database_ObjectModified(object sender, ObjectEventArgs e)
        {
            var acCurDb = (Database) sender;
            if (acCurDb == null || acCurDb.IsDisposed)
                return;

            var dbObj = e.DBObject;

            if (dbObj == null || dbObj.IsDisposed || dbObj.IsErased)
                return;

            if (dbObj is Solid3d acSol) acSol.Update(acCurDb);
        }

        private static void Database_ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            var acCurDb = (Database) sender;
            if (acCurDb == null || acCurDb.IsDisposed)
                return;

            var dbObj = e.DBObject;

            if (dbObj == null || dbObj.IsDisposed || dbObj.IsErased)
                return;

            if (dbObj is Solid3d acSol)
            {
                var acCurDoc = Application.DocumentManager.MdiActiveDocument;
                var acCurEd = acCurDoc.Editor;
                var handle = acSol.Handle;

                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    var objs = acCurEd.SelectAllOfType("3DSOLID", acTrans);

                    foreach (var obj in objs)
                    {
                        var cSol = acTrans.GetObject(obj, OpenMode.ForRead) as Solid3d;
                        if (cSol == null) continue;

                        if (!cSol.HasXData()) continue;
                        cSol.Upgrade();

                        var cHandles = cSol.GetChildren();
                        var pHandle = cSol.GetParent();

                        if (cHandles.Count > 0)
                            if (cHandles.Contains(handle))
                            {
                                cHandles.Remove(handle);

                                cSol.UpdateXData(cHandles, Enums.XDataCode.ChildObjects, acCurDb, acTrans);
                            }

                        if (pHandle == handle)
                            cSol.UpdateXData(default(Handle), Enums.XDataCode.ParentObject, acCurDb, acTrans);

                        cSol.Update(acCurDb);

                        cSol.Downgrade();
                    }

                    acTrans.Commit();
                }
            }
        }
    }
}