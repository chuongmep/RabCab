using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using RabCab.Analysis;
using RabCab.Settings;
using static RabCab.Engine.Enumerators.Enums;
using static RabCab.Engine.Enumerators.Enums.SortBy;

namespace RabCab.Agents
{
    public static class SortingAgent
    {
        public static int CurrentCount = 1;

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

            eInfoList.Sort(new CompareAgent(sCrit));
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

    /// <summary>
    /// TODO
    /// </summary>
    internal class CompareAgent : IComparer<EntInfo>
    {
        private readonly SortBy _criteria;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="criteria"></param>
        public CompareAgent(SortBy criteria)
        {
            _criteria = criteria;
        } 

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(EntInfo x, EntInfo y)
        {
            if (_criteria.HasFlag(Layer))
            {
                int comp = String.Compare(x.EntLayer, y.EntLayer, StringComparison.Ordinal);

                if (comp != 0)
                    return comp;
            }
            if (_criteria.HasFlag(Color))
            {
                int comp = 0;
                if (x.EntColor != null)
                {
                    comp = (y.EntColor != null ? x.EntColor.CompareTo(y.EntColor) : 1);
                }
                else
                {
                    comp = (y.EntColor != null ? -1 : 0);
                }
                if (comp != 0)
                {
                    return comp;
                }
                comp = String.Compare(x.EntMaterial, y.EntMaterial, StringComparison.Ordinal);
                if (comp != 0)
                {
                    return comp;
                };
            }

            if (_criteria.HasFlag(Thickness))
            {
                int comp = -x.Thickness.CompareTo(y.Thickness);
                if (comp != 0)
                {
                    return comp;
                }
            }

            if (_criteria.HasFlag(Name))
            {
                int comp = String.Compare(x.RcName, y.RcName, StringComparison.OrdinalIgnoreCase);
                if (comp != 0)
                {
                    return comp;
                }
            }

            int compM = -x.Width.CompareTo(y.Width);
            if (compM != 0)
            {
                return compM;
            }
            compM = -x.Length.CompareTo(y.Length);
            if (compM != 0)
            {
                return compM;
            }

            if (!_criteria.HasFlag(Thickness))
            {
                int comp = -x.Thickness.CompareTo(y.Thickness);
                if (comp != 0)
                {
                    return comp;
                }
            }

            compM = x.Volume.CompareTo(y.Volume);
            if (compM != 0)
            {
                return compM;
            }
            compM = x.Asymmetry.CompareTo(y.Asymmetry);
            if (compM != 0)
            {
                return compM;
            }

            compM = x.TxDirection.CompareTo(y.TxDirection);

            return compM;
        }
    }
}
