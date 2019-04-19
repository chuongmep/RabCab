using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using RabCab.Analysis;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using static RabCab.Engine.Enumerators.Enums.SortBy;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Agents
{
    public static class SortingAgent
    {
        public static int CurrentPartNumber = 1;

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="eInfoList"></param>
        public static void SortSolids(this List<EntInfo> eInfoList)
        {
            #region Sorting Criteria

            var sCrit = Layer
                        | Color
                        | Thickness
                        | MixS4S;

            if (!SettingsUser.SortByLayer) sCrit -= Layer;
            if (!SettingsUser.SortByColor) sCrit -= Color;
            if (!SettingsUser.SortByThickness) sCrit -= Thickness;
            if (!SettingsUser.MixS4S) sCrit -= MixS4S;

            #endregion

            var sortedList = eInfoList.OrderByDescendingIf(sCrit.HasFlag(MixS4S), e => e.IsBox)
                .ThenByIf(sCrit.HasFlag(Layer), e => e.EntLayer)
                .ThenByIf(sCrit.HasFlag(Color), e => e.EntColor)
                .ThenByIf(sCrit.HasFlag(Thickness), e => e.Thickness)
                .ThenByDescending(e => e.Length).ThenByDescending(e => e.Volume); //.ThenByDescending(e => e.Width);

            eInfoList = sortedList.ToList();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="eInfoList"></param>
        public static void SortByName(this List<EntInfo> eInfoList)
        {
            var namedEnts = new List<EntInfo>();
            var unNamedEnts = new List<EntInfo>();

            foreach (var ent in eInfoList)
            {
                if (ent.RcName != "")
                {
                    namedEnts.Add(ent);
                    continue;
                }

                unNamedEnts.Add(ent);
            }

            var sortedNamed = namedEnts.OrderBy(e => e.RcName);
            unNamedEnts.SortSolids();

            var combinedList = new List<EntInfo>();
            combinedList.AddRange(sortedNamed);
            combinedList.AddRange(unNamedEnts);

            eInfoList = combinedList;
        }


        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="objIds"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acTrans"></param>
        /// <returns></returns>
        public static List<EntInfo> SortSolids(this ObjectId[] objIds, Database acCurDb, Transaction acTrans)
        {
            var eList = MeasureSolids(objIds, acCurDb, acTrans);
            eList.SortSolids();
            return eList;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="objIds"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acTrans"></param>
        /// <returns></returns>
        private static List<EntInfo> MeasureSolids(this ObjectId[] objIds, Database acCurDb, Transaction acTrans)
        {
            var mList = new List<EntInfo>();

            using (var pWorker = new ProgressAgent("Parsing Solids: ", objIds.Count()))
            {
                foreach (var objId in objIds)
                {
                    //Tick progress bar or exit if ESC has been pressed
                    if (!pWorker.Tick()) return mList;

                    var acSol = acTrans.GetObject(objId, OpenMode.ForRead) as Solid3d;

                    if (acSol == null) continue;

                    mList.Add(new EntInfo(acSol, acCurDb, acTrans));
                }
            }

            return mList;
        }

        private static Enums.SortBy GetSCrit()
        {
            var sCrit = Name
                        | Layer
                        | Color
                        | Thickness
                        | MixS4S;

            if (!SettingsUser.SortByName) sCrit -= Name;
            if (!SettingsUser.SortByLayer) sCrit -= Layer;
            if (!SettingsUser.SortByColor) sCrit -= Color;
            if (!SettingsUser.SortByThickness) sCrit -= Thickness;
            if (!SettingsUser.MixS4S) sCrit -= MixS4S;

            return sCrit;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="eList"></param>
        /// <param name="nameParts"></param>
        /// <param name="pWorker"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acCurEd"></param>
        /// <param name="acTrans"></param>
        public static void SortAndName(this List<EntInfo> eList, ProgressAgent pWorker,
            Database acCurDb, Editor acCurEd, Transaction acTrans)
        {
            var sCrit = GetSCrit();

            var groups = eList.GroupBy(x => new
            {
                Box = sCrit.HasFlag(MixS4S) && x.IsBox,
                Layer = sCrit.HasFlag(Layer) ? x.EntLayer : null,
                Color = sCrit.HasFlag(Color) ? x.EntColor : null,
                Thickness = sCrit.HasFlag(Thickness) ? x.Thickness : double.NaN,
                x.Length,
                x.Width,
                x.Volume,
                x.Asymmetry,
                x.TxDirection
            });

            pWorker.Reset("Naming Solids: ");

            var gList = groups.OrderByDescendingIf(sCrit.HasFlag(MixS4S), e => e.Key.Box)
                .ThenByIf(sCrit.HasFlag(Layer), e => e.Key.Layer)
                .ThenByIf(sCrit.HasFlag(Color), e => e.Key.Color)
                .ThenByDescendingIf(sCrit.HasFlag(Thickness), e => e.Key.Thickness)
                .ThenByDescending(e => e.Key.Length)
                .ThenByDescending(e => e.Key.Width)
                .ThenByDescending(e => e.Key.Volume)
                .ToList();

            pWorker.SetTotalOperations(gList.Count());

            if (gList.Count > 0)
                try
                {
                    foreach (var group in gList)
                    {
                        //Tick progress bar or exit if ESC has been pressed
                        if (!pWorker.Tick())
                        {
                            acTrans.Abort();
                            return;
                        }

                        var baseInfo = group.First();

                        var nonMirrors = new List<EntInfo>();
                        var mirrors = new List<EntInfo>();

                        var firstParse = true;

                        //Find Mirrors
                        foreach (var eInfo in group)
                        {
                            if (firstParse)
                            {
                                nonMirrors.Add(eInfo);
                                firstParse = false;
                                continue;
                            }

                            if (eInfo.IsMirrorOf(baseInfo))
                                mirrors.Add(eInfo);
                            else
                                nonMirrors.Add(eInfo);
                        }

                        nonMirrors.UpdatePartData(true, acCurEd, acCurDb, acTrans);
                        mirrors.UpdatePartData(true, acCurEd, acCurDb, acTrans);
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
        /// <param name="eList"></param>
        /// <param name="pWorker"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acCurEd"></param>
        /// <param name="acTrans"></param>
        public static void SortAndLay(this List<EntInfo> eList, Point2d layPoint, ProgressAgent pWorker,
            Database acCurDb, Editor acCurEd, Transaction acTrans, int multAmount = 1)
        {
            var sCrit = GetSCrit();

            var groups = eList.GroupBy(x => new
            {
                Box = sCrit.HasFlag(MixS4S) && x.IsBox,
                Name = sCrit.HasFlag(Name) ? x.RcName : null,
                Layer = sCrit.HasFlag(Layer) ? x.EntLayer : null,
                Color = sCrit.HasFlag(Color) ? x.EntColor : null,
                Thickness = sCrit.HasFlag(Thickness) ? x.Thickness : double.NaN,
                x.Length,
                x.Width,
                x.Volume,
                x.Asymmetry,
                x.TxDirection
            });


            var enumerable = groups.ToList();
            var gList = enumerable.OrderBy(e => e.Key.Name).ThenByIf(sCrit.HasFlag(MixS4S), e => e.Key.Box)
                .ThenByIf(sCrit.HasFlag(Layer), e => e.Key.Layer)
                .ThenByIf(sCrit.HasFlag(Color), e => e.Key.Color)
                .ThenByDescendingIf(sCrit.HasFlag(Thickness), e => e.Key.Thickness)
                .ThenByDescending(e => e.Key.Length)
                .ThenByDescending(e => e.Key.Width)
                .ThenByDescending(e => e.Key.Volume).ToList();
            ;

            pWorker.Reset("Laying Solids: ");
            pWorker.SetTotalOperations(gList.Count());

            if (gList.Count > 0)
                try
                {
                    foreach (var group in gList)
                    {
                        //Tick progress bar or exit if ESC has been pressed
                        if (!pWorker.Tick())
                        {
                            acTrans.Abort();
                            return;
                        }

                        var baseInfo = group.First();

                        var nonMirrors = new List<EntInfo>();
                        var mirrors = new List<EntInfo>();

                        var firstParse = true;

                        //Find Mirrors
                        foreach (var eInfo in group)
                        {
                            if (firstParse)
                            {
                                nonMirrors.Add(eInfo);
                                firstParse = false;
                                continue;
                            }

                            if (eInfo.IsMirrorOf(baseInfo))
                                mirrors.Add(eInfo);
                            else
                                nonMirrors.Add(eInfo);
                        }

                        nonMirrors.UpdatePartData(false, acCurEd, acCurDb, acTrans);
                        nonMirrors.LayParts(ref layPoint, acCurDb, acTrans, multAmount);
                        mirrors.UpdatePartData(false, acCurEd, acCurDb, acTrans);
                        mirrors.LayParts(ref layPoint, acCurDb, acTrans, multAmount);
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
        /// <param name="eList"></param>
        /// <param name="layPoint"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acTrans"></param>
        /// <param name="multAmount"></param>
        private static void LayParts(this List<EntInfo> eList, ref Point2d layPoint, Database acCurDb,
            Transaction acTrans, int multAmount = 1)
        {
            foreach (var e in eList)
            {
                if (e.IsChild) continue;

                var acSol = acTrans.GetObject(e.ObjId, OpenMode.ForRead) as Solid3d;
                if (acSol == null) continue;

                var cloneSol = acSol.Clone() as Solid3d;

                cloneSol?.TransformBy(e.LayMatrix);
                acCurDb.AppendEntity(cloneSol, acTrans);
                var yStep = cloneSol.TopLeftTo(layPoint.Convert3D());

                if (cloneSol.CheckRotation())
                    cloneSol.TopLeftTo(layPoint.Convert3D());

                using (var acText = new MText())
                {
                    acText.TextHeight = SettingsUser.LayTextHeight;
                    acText.Contents = e.RcName + " - " + e.RcQtyTotal * multAmount + " Pieces";
                    //acText.Layer = ;
                    //acText.ColorIndex = ;                           

                    //Parse the insertion point and text alignment
                    double zPt = 0;

                    //Default Lay Above
                    var yPt = layPoint.Y + 1;

                    if (SettingsUser.LayTextInside)
                    {
                        yPt = layPoint.Y - yStep / 2;
                        zPt = e.Thickness + .01;
                    }

                    //Default Lay Left
                    var xPt = layPoint.X;
                    acText.Attachment = AttachmentPoint.BottomLeft;

                    if (SettingsUser.LayTextCenter)
                    {
                        xPt = layPoint.X + e.Length / 2;
                        acText.Attachment = AttachmentPoint.MiddleCenter;
                    }

                    acText.Location = new Point3d(xPt, yPt, zPt);

                    //Appent the text
                    acCurDb.AppendEntity(acText, acTrans);
                }

                layPoint = new Point2d(layPoint.X, layPoint.Y - yStep - SettingsUser.LayStep);
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="eList"></param>
        /// <param name="acCurEd"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acTrans"></param>
        public static void UpdatePartData(this List<EntInfo> eList, bool nameParts, Editor acCurEd, Database acCurDb,
            Transaction acTrans)
        {
            if (eList.Count <= 0) return;

            var baseInfo = eList.First();
            var baseSolid = acTrans.GetObject(baseInfo.ObjId, OpenMode.ForRead) as Solid3d;

            var nameString = SettingsUser.NamingConvention;

            if (CurrentPartNumber < 10)
                nameString += "0";
            nameString += CurrentPartNumber;

            if (baseSolid == null) return;

            var partCount = 1;

            var groupTotal = eList.Count;

            foreach (var eInfo in eList)
            {
                var acSol = acTrans.GetObject(eInfo.ObjId, OpenMode.ForRead) as Solid3d;

                if (acSol == null) continue;

                var handle = acSol.Handle;

                if (nameParts)
                    eInfo.RcName = nameString;

                eInfo.RcQtyOf = partCount;

                eInfo.RcQtyTotal = groupTotal;

                if (baseInfo.Hndl.ToString() != handle.ToString())
                {
                    eInfo.IsChild = true;
                    eInfo.BaseHandle = baseInfo.Hndl;
                    baseInfo.ChildHandles.Add(handle);
                    baseSolid.UpdateXData(baseInfo.ChildHandles, Enums.XDataCode.ChildObjects, acCurDb, acTrans);
                }

                acSol.AddXData(eInfo, acCurDb, acTrans);

                string printStr;

                if (eInfo.IsChild)
                    printStr = "\n\t\u2022 [C] |" + eInfo.PrintInfo(true);
                else // Is Parent
                    printStr = "\n[P] | " + eInfo.PrintInfo(false);

                acCurEd.WriteMessage(printStr + " | Part " + partCount + " Of " + groupTotal);

                partCount++;
            }

            CurrentPartNumber++;
        }
    }

    public static class ListExt
    {
        /// <summary>
        ///     TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <param name="sel"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> OrderByIf<T, TKey>(this IEnumerable<T> list, bool predicate,
            Func<T, TKey> sel) =>
            predicate ? list.OrderBy(f => sel(f)) : list.OrderBy(a => 1);

        /// <summary>
        ///     TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <param name="sel"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> OrderByDescendingIf<T, TKey>(this IEnumerable<T> list, bool predicate,
            Func<T, TKey> sel) =>
            predicate ? list.OrderByDescending(f => sel(f)) : list.OrderBy(a => 1);

        /// <summary>
        ///     TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <param name="sel"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> ThenByIf<T, TKey>(this IOrderedEnumerable<T> list, bool predicate,
            Func<T, TKey> sel) =>
            predicate ? list.ThenBy(f => sel(f)) : list;

        /// <summary>
        ///     TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <param name="sel"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> ThenByDescendingIf<T, TKey>(this IOrderedEnumerable<T> list, bool predicate,
            Func<T, TKey> sel) =>
            predicate ? list.ThenByDescending(f => sel(f)) : list;

        public static bool Compare(this EntInfo x, EntInfo y, bool compareNames)
        {
            if (SettingsUser.SortByLayer)
                if (x.EntLayer != y.EntLayer)
                    return false;

            if (SettingsUser.SortByColor)
                if (x.EntColor != y.EntColor)
                    return false;

            if (SettingsUser.SortByThickness)
                if (x.Thickness != y.Thickness)
                    return false;

            if (SettingsUser.SortByName)
                if (x.RcName != y.RcName)
                    return false;

            return true;
        }
    }
}