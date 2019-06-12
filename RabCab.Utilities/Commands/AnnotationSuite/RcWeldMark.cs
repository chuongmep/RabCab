using System;
using System.Security.Cryptography;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Calculators;
using RabCab.Engine.GUI;
using RabCab.Entities.Annotation;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcWeldMark
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_WELDMARK",
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
        public void Cmd_WeldMark()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            try
            {

                Entity jigEnt = WeldJig.Jig(out var arrowStart, out var symStart);
                ObjectId jigId = ObjectId.Null;

                if (jigEnt == null)
                {
                    jigEnt.Dispose();
                    return;
                }

                jigId = acCurDb.AppendEntity(jigEnt);
                Line line = null;
                ObjectId lineId = ObjectId.Null;
                
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    var arrowPt = new Point2d(arrowStart.X, arrowStart.Y);
                    var symPt = new Point2d(symStart.X, symStart.Y);

                        var angle = arrowPt.AngleBetween(symPt);

                        var length = SettingsUser.WeldSymbolLength;

                        if (angle > 90 && angle < 270)
                        {
                            length = -length;
                        }
                        
                        line = new Line(symStart, new Point3d(symStart.X + length, symStart.Y, 0));

                        lineId = acCurDb.AppendEntity(line, acTrans);
                    
                        
                    acTrans.Commit();
                }

                var weldGui = new WeldGui(line.StartPoint, line.EndPoint);
                var result = weldGui.ShowDialog();
                
                TransientAgent.Clear();
                
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    if (result == DialogResult.OK)
                    {
                        foreach (var ent in weldGui.drawnEnts)
                        {
                            acCurDb.AppendEntity(ent, acTrans);
                        }
                    }
                    else
                    {
                        var jigDel = acTrans.GetObject(jigId, OpenMode.ForWrite) as Entity;
                        if (jigDel != null)
                        {
                            jigDel.Erase();
                            jigDel.Dispose();
                        }

                        var lineDel = acTrans.GetObject(lineId, OpenMode.ForWrite) as Entity;
                        if (lineDel != null)
                        {
                            lineDel.Erase();
                            lineDel.Dispose();
                        }

                        line.Dispose();

                    }

                    acTrans.Commit();
                }

               weldGui.Dispose();

                Utils.SetFocusToDwgView();
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
            }
        }
    }
}