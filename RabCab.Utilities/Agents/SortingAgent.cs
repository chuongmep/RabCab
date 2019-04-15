using Autodesk.AutoCAD.DatabaseServices;
using RabCab.Analysis;
using RabCab.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using static RabCab.Engine.Enumerators.Enums.SortBy;

namespace RabCab.Agents
{
    public static class SortingAgent
    {
        public static int CurrentPartNumber = 1;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="eInfoList"></param>
        public static void SortSolids(this List<EntInfo> eInfoList)
        {
            #region Sorting Criteria

            var sCrit = Layer
                        | Color
                        | Thickness
                        | Name
                        | GroupSame
                        | SplitByLayer
                        | MixS4S;

            if (!SettingsUser.SortByLayer) sCrit -= Layer;
            if (!SettingsUser.SortByColor) sCrit -= Color;
            if (!SettingsUser.SortByThickness) sCrit -= Thickness;
            if (!SettingsUser.SortByName) sCrit -= Name;
            if (!SettingsUser.GroupSame) sCrit -= GroupSame;
            if (!SettingsUser.SplitByLayer) sCrit -= SplitByLayer;
            if (!SettingsUser.MixS4S) sCrit -= MixS4S;

            #endregion

            var sortedList = eInfoList.OrderByIf(sCrit.HasFlag(MixS4S), e => e.IsBox)
                .ThenByIf(sCrit.HasFlag(Name), e => e.RcName)
                .ThenByIf(sCrit.HasFlag(Layer), e => e.EntLayer)
                .ThenByIf(sCrit.HasFlag(Color), e => e.EntColor)
                .ThenByIf(sCrit.HasFlag(Thickness), e => e.Thickness)
                .ThenBy(e => e.Length).ThenBy(e => e.Width);

            eInfoList = sortedList.ToList();
        }

        /// <summary>
        /// TODO
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
        /// TODO
        /// </summary>
        /// <param name="objIds"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acTrans"></param>
        /// <returns></returns>
        private static List<EntInfo> MeasureSolids(this ObjectId[] objIds, Database acCurDb, Transaction acTrans)
        {
            var mList = new List<EntInfo>();

            using (var pWorker = new ProgressAgent("Parsing Solids: ", Enumerable.Count(objIds)))
            {
                foreach (var objId in objIds)
                {
                    //Tick progress bar or exit if ESC has been pressed
                    if (!pWorker.Tick())
                    {
                        return mList;
                    }

                    var acSol = acTrans.GetObject(objId, OpenMode.ForRead) as Solid3d;

                    if (acSol == null) continue;

                    mList.Add(new EntInfo(acSol, acCurDb, acTrans));
                }
            }

            return mList;
        }
    }

    public static class ListExt
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <param name="sel"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> OrderByIf<T, TKey>(this IEnumerable<T> list, bool predicate, Func<T, TKey> sel)
        {
            if (predicate)
                return list.OrderBy(f => sel(f));
            else
            {
                return list.OrderBy(a => 1);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <param name="sel"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> ThenByIf<T, TKey>(this IOrderedEnumerable<T> list, bool predicate, Func<T, TKey> sel)
        {
            if (predicate)
                return list.ThenBy(f => sel(f));
            else
                return list;
        }

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
