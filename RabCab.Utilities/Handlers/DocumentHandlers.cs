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
using RabCab.Commands.PaletteKit;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Handlers
{
    /// <summary>
    /// TODO
    /// </summary>
    internal static class DocumentHandlers
    {
        /// <summary>
        /// TODO
        /// </summary>
        internal static void AddDocEvents()
        {
            // Get the current document
            var acDocMan = Application.DocumentManager;
            var acDoc = acDocMan.MdiActiveDocument;

            //Doc Manager Handlers
            acDocMan.DocumentToBeDeactivated +=  BeginDocClose;
            acDocMan.DocumentActivated += DocActivated;

            //Doc Handlers
            acDoc.ImpliedSelectionChanged += Doc_ImpliedSelectionChanged;

        }

        /// <summary>
        /// TODO
        /// </summary>
        internal static void RemoveDocEvents()
        {
            // Get the current document
            var acDocMan = Application.DocumentManager;
            var acDoc = acDocMan.MdiActiveDocument;

            //Doc Manager Handlers
            acDocMan.DocumentToBeDeactivated -= BeginDocClose;
            acDocMan.DocumentActivated -= DocActivated;

            //Doc Handlers
            acDoc.ImpliedSelectionChanged -= Doc_ImpliedSelectionChanged;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="senderObj"></param>
        /// <param name="docBegClsEvtArgs"></param>
        private static void BeginDocClose(object senderObj,
            DocumentCollectionEventArgs docBegClsEvtArgs)
        {
           //TODO
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="senderObj"></param>
        /// <param name="docActEvent"></param>
        private static void DocActivated(object senderObj,
            DocumentCollectionEventArgs docActEvent)
        {
            //Notebook Handlers
            if (SettingsInternal.EnNotePal)
            {
                RcPaletteNotebook.UpdNotePal();
            }
        }

        /// <summary>
        ///     Handler used to populate RC Pallette when selections are changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Doc_ImpliedSelectionChanged(object sender, EventArgs e)
        {
            if (SettingsInternal.EnMetPal == false) return;

            if (RcPaletteMetric._rcPal == null) return;

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
    }
}