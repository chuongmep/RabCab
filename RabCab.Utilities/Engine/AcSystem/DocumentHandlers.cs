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
using RabCab.Commands.AutomationSuite;
using RabCab.Commands.PaletteKit;
using RabCab.Entities.Annotation;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Engine.AcSystem
{
    /// <summary>
    ///     TODO
    /// </summary>
    internal static class DocumentHandlers
    {
        #region Secondary Doc Handlers

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #endregion

        #region Handler Init

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

                //RCLEADER
                acDoc.CommandWillStart += RcLeader.rcLeader_CommandWillStart;
                acDoc.CommandEnded += RcLeader.rcLeader_CommandEnded;
                acDoc.CommandCancelled += RcLeader.rcLeader_CommandEnded;
                acDoc.CommandFailed += RcLeader.rcLeader_CommandEnded;
                acDoc.Database.ObjectModified += RcLeader.rcLeader_ObjectModified;

                //AUTOLAYER
                acDoc.CommandWillStart += RcAutoLayer.autoLayer_CommandWillStart;
                acDoc.CommandEnded += RcAutoLayer.autoLayer_CommandEnded;
                acDoc.CommandCancelled += RcAutoLayer.autoLayer_CommandEnded;
                acDoc.CommandFailed += RcAutoLayer.autoLayer_CommandEnded;
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

                //RCLEADER
                acDoc.CommandWillStart -= RcLeader.rcLeader_CommandWillStart;
                acDoc.CommandEnded -= RcLeader.rcLeader_CommandEnded;
                acDoc.CommandCancelled -= RcLeader.rcLeader_CommandEnded;
                acDoc.CommandFailed -= RcLeader.rcLeader_CommandEnded;
                acDoc.Database.ObjectModified -= RcLeader.rcLeader_ObjectModified;

                //AUTOLAYER
                acDoc.CommandWillStart -= RcAutoLayer.autoLayer_CommandWillStart;
                acDoc.CommandEnded -= RcAutoLayer.autoLayer_CommandEnded;
                acDoc.CommandCancelled -= RcAutoLayer.autoLayer_CommandEnded;
                acDoc.CommandFailed -= RcAutoLayer.autoLayer_CommandEnded;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion

        #region Main Doc Handlers

        private static void DocActivated(object senderObj,
            DocumentCollectionEventArgs docActEvent)
        {
            try
            {
                // Get the current document
                var acDocMan = Application.DocumentManager;
                var acDoc = acDocMan.MdiActiveDocument;

                if (acDoc == null) return;

                //Notebook Handlers
                if (SettingsInternal.EnNotePal) RcPaletteNotebook.UpdNotePal();

                acDoc.ImpliedSelectionChanged += Doc_ImpliedSelectionChanged;

                //RCLEADER
                acDoc.CommandWillStart += RcLeader.rcLeader_CommandWillStart;
                acDoc.CommandEnded += RcLeader.rcLeader_CommandEnded;
                acDoc.CommandCancelled += RcLeader.rcLeader_CommandEnded;
                acDoc.CommandFailed += RcLeader.rcLeader_CommandEnded;

                acDoc.Database.ObjectModified += RcLeader.rcLeader_ObjectModified;

                //AUTOLAYER
                acDoc.CommandWillStart += RcAutoLayer.autoLayer_CommandWillStart;
                acDoc.CommandEnded += RcAutoLayer.autoLayer_CommandEnded;
                acDoc.CommandCancelled += RcAutoLayer.autoLayer_CommandEnded;
                acDoc.CommandFailed += RcAutoLayer.autoLayer_CommandEnded;
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
                // Get the current document
                var acDocMan = Application.DocumentManager;
                var acDoc = acDocMan.MdiActiveDocument;

                if (acDoc == null) return;
                acDoc.ImpliedSelectionChanged -= Doc_ImpliedSelectionChanged;

                //RCLEADER
                acDoc.CommandWillStart -= RcLeader.rcLeader_CommandWillStart;
                acDoc.CommandEnded -= RcLeader.rcLeader_CommandEnded;
                acDoc.CommandCancelled -= RcLeader.rcLeader_CommandEnded;
                acDoc.CommandFailed -= RcLeader.rcLeader_CommandEnded;
                acDoc.Database.ObjectModified -= RcLeader.rcLeader_ObjectModified;

                //AUTOLAYER
                acDoc.CommandWillStart -= RcAutoLayer.autoLayer_CommandWillStart;
                acDoc.CommandEnded -= RcAutoLayer.autoLayer_CommandEnded;
                acDoc.CommandCancelled -= RcAutoLayer.autoLayer_CommandEnded;
                acDoc.CommandFailed -= RcAutoLayer.autoLayer_CommandEnded;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion
    }
}