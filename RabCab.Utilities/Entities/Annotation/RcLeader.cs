using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Entities.Annotation
{
    public static class RcLeader
    {
        //Create variable to hold Current Leader Id
        private static ObjectId _curLeaderId = ObjectId.Null;

        //Create bools to check if editing or creating leader
        private static bool _editingL;
        private static bool _creatingL;

        /// <summary>
        ///     Method to parse if mleaders have been modified
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void rcLeader_ObjectModified(object sender, ObjectEventArgs e)
        {
            if (!SettingsUser.PartLeaderEnabled) return;

            var curDb = (Database) sender;
            if (curDb == null || curDb.IsDisposed)
                return;

            var dbObj = e.DBObject;

            if (dbObj == null || dbObj.IsDisposed || dbObj.IsErased || _editingL || !_creatingL ||
                !(e.DBObject is MLeader))
                return;

            _creatingL = false;
            _curLeaderId = dbObj.ObjectId;
        }

        public static void rcLeader_CommandWillStart(object sender, CommandEventArgs e)
        {
            _editingL = false;
            _creatingL = true;
        }

        /// <summary>
        ///     Method to run when commands are ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void rcLeader_CommandEnded(object sender, CommandEventArgs e)
        {
            if (!SettingsUser.PartLeaderEnabled) return;

            var acCurDoc = (Document) sender;

            if (acCurDoc == null || acCurDoc.IsDisposed || !acCurDoc.IsActive) return;

            if (e.GlobalCommandName == "MLEADER" ||
                e.GlobalCommandName == "GRIP_STRETCH" ||
                e.GlobalCommandName == "MOVE" ||
                e.GlobalCommandName == "AIMLEADEREDITREMOVE" ||
                e.GlobalCommandName == "AIMLEADEREDITADD")
            {
                _editingL = true;

                UpdateLeader(acCurDoc);

                _curLeaderId = ObjectId.Null;
                _editingL = false;
                _creatingL = true;
            }
        }

        /// <summary>
        ///     Method to update mleaders with substitution text
        /// </summary>
        /// <param name="acCurDoc"></param>
        private static void UpdateLeader(Document acCurDoc)
        {
            var acCUrDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            using (var acTrans = acCUrDb.TransactionManager.StartTransaction())
            {
                try
                {
                    var mLeader = acTrans.GetObject(_curLeaderId, OpenMode.ForWrite) as MLeader;

                    if (mLeader == null)
                    {
                        acTrans.Abort();
                        return;
                    }

                    var obj = mLeader.GetObjectUnderArrow();

                    if (obj != ObjectId.Null)
                    {
                        var ent = acTrans.GetObject(obj, OpenMode.ForWrite) as Entity;

                        if (mLeader.ContentType == ContentType.MTextContent)
                        {
                            var mt = new MText();
                            mt.SetDatabaseDefaults();

                            //TODO let user set the type of contents
                            mt.Contents = ent.GetPartName();

                            mLeader.MText = mt;

                            mt.Dispose();
                        }
                        else if (mLeader.ContentType == ContentType.BlockContent)
                        {
                            var blkTblRef =
                                acTrans.GetObject(mLeader.BlockContentId, OpenMode.ForWrite) as BlockTableRecord;

                            blkTblRef?.UpdateMleaderBlockSubst(mLeader, ent, acCurDoc, acCurEd);
                        }
                    }
                    else
                    {
                        acTrans.Abort();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    acCurEd.WriteMessage("\n" + ex.Message);
                    acTrans.Abort();
                    return;
                }

                acTrans.Commit();
            }
        }

        /// <summary>
        ///     Method to find the object under the mleader arrow
        /// </summary>
        /// <param name="mLeader"></param>
        /// <returns></returns>
        private static ObjectId GetObjectUnderArrow(this MLeader mLeader)
        {
            var leaderInd = mLeader.GetLeaderIndexes();
            var objId = ObjectId.Null;

            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            try
            {
                if (leaderInd.Count != 0)
                {
                    var leaderLineInd = mLeader.GetLeaderLineIndexes((int) leaderInd[0]);
                    var arrowPt = mLeader.GetFirstVertex((int) leaderLineInd[0]);

                    SelectionSet ss = null;

                    if (LayoutManager.Current.CurrentLayout == "Model")
                    {
                        arrowPt = arrowPt.Trans(acCurEd, CoordSystem.Wcs, CoordSystem.Ucs);

                        var p = arrowPt;
                        var tol = 0.01;

                        var p1 = new Point3d(p.X - tol, p.Y - tol, p.Z - tol);
                        var p2 = new Point3d(p.X + tol, p.Y + tol, p.Z + tol);

                        var res = acCurEd.SelectCrossingWindow(p1, p2);

                        ss = res.Value;
                    }
                    else
                    {
                        var layoutName = LayoutManager.Current.CurrentLayout;

                        using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                        {
                            var dbDict = (DBDictionary) acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead);

                            if (dbDict.Contains(layoutName))
                            {
                                var id = dbDict.GetAt(layoutName);

                                var curLayout = acTrans.GetObject(id, OpenMode.ForRead) as Layout;

                                if (curLayout != null)
                                {
                                    var vPorts = new List<Viewport>();

                                    //Get viewports from chosen layout
                                    using (
                                        var blkTblRec =
                                            acTrans.GetObject(curLayout.BlockTableRecordId, OpenMode.ForRead) as
                                                BlockTableRecord)
                                    {
                                        if (blkTblRec != null)
                                            foreach (var lObjId in blkTblRec)
                                            {
                                                if (lObjId == acCurDb.PaperSpaceVportId) continue;

                                                var acVp = acTrans.GetObject(lObjId, OpenMode.ForWrite) as Viewport;
                                                if (acVp != null && !acVp.IsErased) vPorts.Add(acVp);
                                            }
                                    }

                                    if (vPorts.Count > 0)
                                    {
                                        var insideVp = false;
                                        var vpLocation = 0;

                                        for (var index = 0; index < vPorts.Count; index++)
                                        {
                                            var acVp = vPorts[index];
                                            if (arrowPt.IsInside(acVp.GeometricExtents))
                                            {
                                                insideVp = true;
                                                vpLocation = index;
                                                break;
                                            }
                                        }

                                        if (insideVp)
                                        {
                                            var psVpPnts = new Point3dCollection();

                                            // now extract the viewport geometry
                                            vPorts[vpLocation].GetGripPoints(psVpPnts, new IntegerCollection(),
                                                new IntegerCollection());

                                            // let's assume a rectangular vport for now, make the cross-direction grips square
                                            var tmp = psVpPnts[2];
                                            psVpPnts[2] = psVpPnts[1];
                                            psVpPnts[1] = tmp;

                                            var xform = vPorts[vpLocation].Dcs2Wcs() * vPorts[vpLocation].Psdcs2Dcs();
                                            arrowPt = arrowPt.TransformBy(xform);

                                            // now switch to MS
                                            acCurEd.SwitchToModelSpace();
                                            // set the CVPort
                                            Application.SetSystemVariable("CVPORT", vPorts[vpLocation].Number);

                                            var p = arrowPt;
                                            var tol = 0.01;
                                            var p1 = new Point3d(p.X - tol, p.Y - tol, p.Z - tol);
                                            var p2 = new Point3d(p.X + tol, p.Y + tol, p.Z + tol);

                                            var res = acCurEd.SelectCrossingWindow(p1, p2);

                                            if (res.Status != PromptStatus.OK)
                                                acCurEd.WriteMessage(res.Status.ToString());

                                            ss = res.Value;

                                            // now switch to MS
                                            acCurEd.SwitchToPaperSpace();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                acTrans.Abort();
                            }

                            acTrans.Commit();
                        }
                    }

                    if (ss == null) return objId;

                    var objIds = new ObjectIdCollection();

                    foreach (SelectedObject obj in ss)
                        if (obj.ObjectId != _curLeaderId)
                            objIds.Add(obj.ObjectId);

                    if (objIds.Count > 0) objId = objIds[objIds.Count - 1];
                }
            }
            catch (Exception)
            {
                //Ignored
            }

            return objId;
        }

        /// <summary>
        ///     Method to update the mleaders
        /// </summary>
        public static void UpdateMleaders()
        {
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var layoutName = LayoutManager.Current.CurrentLayout;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var counter = 0;

                var dbDict = (DBDictionary) acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead);

                if (dbDict.Contains(layoutName))
                {
                    var id = dbDict.GetAt(layoutName);

                    var curLayout = acTrans.GetObject(id, OpenMode.ForRead) as Layout;

                    if (curLayout != null)
                        //Get viewports from chosen layout
                        using (
                            var blkTblRec =
                                acTrans.GetObject(curLayout.BlockTableRecordId, OpenMode.ForRead) as
                                    BlockTableRecord)
                        {
                            if (blkTblRec != null)
                                foreach (var mlObj in blkTblRec)
                                {
                                    var ml = acTrans.GetObject(mlObj, OpenMode.ForRead) as MLeader;

                                    if (ml != null)
                                    {
                                        _curLeaderId = mlObj;
                                        UpdateLeader(acCurDoc);
                                        counter++;
                                    }
                                }
                        }
                }
                else
                {
                    acTrans.Abort();
                }

                acTrans.Commit();
                acCurEd.WriteMessage($"\n{counter} leaders updated.");
            }
        }


        public static void UpdateMleaderBlockSubst(this BlockTableRecord acBlkTblLRec, MLeader mLeader, Entity ent,
            Document acCurDoc, Editor acCurEd)
        {
            acBlkTblLRec?.UpdateMleaderAttributeBySubstitution(mLeader, EnumAgent.GetNameOf(Enums.Substitution.Part),
                ent.GetPartName(), acCurDoc, acCurEd);

            //TODO finish adding substitutions
        }
    }
}