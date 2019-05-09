// -----------------------------------------------------------------------------------
//     <copyright file="XDataAgent.cs" company="CraterSpace">
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
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RabCab.Analysis;
using RabCab.Extensions;
using RabCab.Settings;
using static RabCab.Engine.Enumerators.Enums;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Agents
{
    public static class XDataAgent
    {
        /// <summary>
        /// Add or edit a Xrecord data in a named dictionary (the dictionary and xrecord are created if not already exist)
        /// </summary>
        /// <param name="dictName">The dictionary name</param>
        /// <param name="key">the xrecord key</param>
        /// <param name="resbuf">the xrecord data</param>
        public static void SetXrecord(string dictName, string key, ResultBuffer resbuf)
        {
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var NOD =
                    (DBDictionary) acTrans.GetObject(acCurDb.NamedObjectsDictionaryId, OpenMode.ForRead);
                DBDictionary dict;
                if (NOD.Contains(dictName))
                {
                    dict = (DBDictionary) acTrans.GetObject(NOD.GetAt(dictName), OpenMode.ForWrite);
                }
                else
                {
                    dict = new DBDictionary();
                    NOD.UpgradeOpen();
                    NOD.SetAt(dictName, dict);
                    acTrans.AddNewlyCreatedDBObject(dict, true);
                }

                var xRec = new Xrecord();
                xRec.Data = resbuf;
                dict.SetAt(key, xRec);
                acTrans.AddNewlyCreatedDBObject(xRec, true);
                acTrans.Commit();
            }
        }

        /// <summary>
        ///     Gets an xrecord data in a named dictionary
        /// </summary>
        /// <param name="dictName">The dictionary name</param>
        /// <param name="key">The xrecord key</param>
        /// <returns>The xrecord data or null if the dictionary or the xrecord do not exist</returns>
        public static ResultBuffer GetXrecord(string dictName, string key)
        {
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var NOD =
                    (DBDictionary) acTrans.GetObject(acCurDb.NamedObjectsDictionaryId, OpenMode.ForRead);
                if (!NOD.Contains(dictName))
                    return null;
                var dict = acTrans.GetObject(NOD.GetAt(dictName), OpenMode.ForRead) as DBDictionary;
                if (dict == null || !dict.Contains(key))
                    return null;
                var xRec = acTrans.GetObject(dict.GetAt(key), OpenMode.ForRead) as Xrecord;
                if (xRec == null)
                    return null;
                return xRec.Data;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acEnt"></param>
        /// <param name="eInfo"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acTrans"></param>
        public static void AddXData(this Entity acEnt, EntInfo eInfo, Database acCurDb, Transaction acTrans)
        {
            //If solid is not open for write, open it for write
            acEnt.Upgrade();

            // Open the Registered Applications table for read
            var acRegAppTbl = acTrans.GetObject(acCurDb.RegAppTableId, OpenMode.ForRead) as RegAppTable;

            // Check to see if the Registered Applications table record for the custom app exists
            if (acRegAppTbl != null && acRegAppTbl.Has(SettingsInternal.CommandGroup) == false)
                using (var acRegAppTblRec = new RegAppTableRecord())
                {
                    acRegAppTblRec.Name = SettingsInternal.CommandGroup;

                    acRegAppTbl.UpgradeOpen();
                    acRegAppTbl.Add(acRegAppTblRec);
                    acTrans.AddNewlyCreatedDBObject(acRegAppTblRec, true);
                }

            // Define the Xdata to add to each selected object
            using (var rBuffer = new ResultBuffer())
            {
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataRegAppName, SettingsInternal.CommandGroup));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataAsciiString, eInfo.RcName));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataAsciiString, eInfo.RcInfo));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, eInfo.Length));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, eInfo.Width));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, eInfo.Thickness));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, eInfo.Volume));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, eInfo.MaxArea));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, eInfo.MaxPerimeter));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, eInfo.Asymmetry));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataAsciiString, eInfo.AsymString));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, eInfo.RcQtyOf));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, eInfo.RcQtyTotal));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, eInfo.NumberOfChanges));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, Convert.ToInt32(eInfo.IsSweep)));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, Convert.ToInt32(eInfo.IsMirror)));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, Convert.ToInt32(eInfo.HasHoles)));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, eInfo.TxDirection));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, eInfo.ProdType));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataHandle, eInfo.BaseHandle));
                rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataAsciiString,
                    string.Join(",", eInfo.ChildHandles)));
                // Append the extended data to the object
                acEnt.XData = rBuffer;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="acEnt"></param>
        /// <param name="value"></param>
        /// <param name="xCode"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acTrans"></param>
        public static void UpdateXData<T>(this Entity acEnt, T value, XDataCode xCode, Database acCurDb,
            Transaction acTrans)
        {
            //If solid is not open for write, open it for write
            acEnt.Upgrade();

            // Open the Registered Applications table for read
            var acRegAppTbl = acTrans.GetObject(acCurDb.RegAppTableId, OpenMode.ForRead) as RegAppTable;

            // Check to see if the Registered Applications table record for the custom app exists
            if (acRegAppTbl != null && acRegAppTbl.Has(SettingsInternal.CommandGroup) == false)
            {
                using (var acRegAppTblRec = new RegAppTableRecord())
                {
                    acRegAppTblRec.Name = SettingsInternal.CommandGroup;

                    acRegAppTbl.UpgradeOpen();
                    acRegAppTbl.Add(acRegAppTblRec);
                    acTrans.AddNewlyCreatedDBObject(acRegAppTblRec, true);
                }

                // Define the Xdata to add to each selected object
                using (var rBuffer = new ResultBuffer())
                {
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataRegAppName, SettingsInternal.CommandGroup));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataAsciiString, ""));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataAsciiString, ""));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataReal, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataAsciiString, ""));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, -1));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataInteger32, -1));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataHandle, 0));
                    rBuffer.Add(new TypedValue((int) DxfCode.ExtendedDataAsciiString, ""));
                    // Append the extended data to the object
                    acEnt.XData = rBuffer;
                }
            }

            try
            {
                //Update the specified Value
                var rBuffer = acEnt.GetXDataForApplication(SettingsInternal.CommandGroup);

                var rcData = rBuffer.AsArray();
                var xDataIndex = (int) xCode;

                if (value.GetType() == typeof(List<Handle>))
                {
                    var childList = value as List<Handle>;
                    var childString = "";

                    if (childList != null && childList.Count > 0) childString = string.Join(",", childList);

                    rcData[xDataIndex] = new TypedValue((int) (DxfCode) rcData[xDataIndex].TypeCode, childString);
                }
                else
                {
                    rcData[xDataIndex] = new TypedValue((int) (DxfCode) rcData[xDataIndex].TypeCode, value);
                }

                //Reassign the XData
                rBuffer = new ResultBuffer(rcData);
                acEnt.XData = rBuffer;
            }
            catch
            {
                // ignored
            }
        }

        public static T GetXData<T>(this Entity acEnt, XDataCode xCode)
        {
            //Get the XData from RC_DATA
            var rb = acEnt.GetXDataForApplication(SettingsInternal.CommandGroup);
            if (rb == null) return default;

            var rvArr = rb.AsArray();

            return (T) Convert.ChangeType(rvArr[(int) xCode].Value, typeof(T));
        }

        public static string GetAppName(this Entity acEnt)
        {
            return GetXData<string>(acEnt, XDataCode.App);
        }

        public static string GetPartName(this Entity acEnt)
        {
            return GetXData<string>(acEnt, XDataCode.Name);
        }

        public static string GetPartInfo(this Entity acEnt)
        {
            return GetXData<string>(acEnt, XDataCode.Info);
        }

        public static double GetPartLength(this Entity acEnt)
        {
            return GetXData<double>(acEnt, XDataCode.Length);
        }

        public static double GetPartWidth(this Entity acEnt)
        {
            return GetXData<double>(acEnt, XDataCode.Width);
        }

        public static double GetPartThickness(this Entity acEnt)
        {
            return GetXData<double>(acEnt, XDataCode.Thickness);
        }

        public static double GetPartVolume(this Entity acEnt)
        {
            return GetXData<double>(acEnt, XDataCode.Volume);
        }

        public static double GetPartArea(this Entity acEnt)
        {
            return GetXData<double>(acEnt, XDataCode.MaxArea);
        }

        public static double GetPartPerimeter(this Entity acEnt)
        {
            return GetXData<double>(acEnt, XDataCode.MaxPerimeter);
        }

        public static double GetPartAsymmetry(this Entity acEnt)
        {
            return GetXData<double>(acEnt, XDataCode.Asymmetry);
        }

        public static string GetAsymVector(this Entity acEnt)
        {
            return GetXData<string>(acEnt, XDataCode.AsymmetryVector);
        }

        public static int GetQtyOf(this Entity acEnt)
        {
            return GetXData<int>(acEnt, XDataCode.PartOf);
        }

        public static int GetQtyTotal(this Entity acEnt)
        {
            return GetXData<int>(acEnt, XDataCode.PartTotal);
        }

        public static int GetNumChanges(this Entity acEnt)
        {
            return GetXData<int>(acEnt, XDataCode.NumChanges);
        }

        public static bool GetIsSweep(this Entity acEnt)
        {
            return GetXData<bool>(acEnt, XDataCode.IsSweep);
        }

        public static bool GetIsMirror(this Entity acEnt)
        {
            return GetXData<bool>(acEnt, XDataCode.IsMirror);
        }

        public static bool GetHasHoles(this Entity acEnt)
        {
            return GetXData<bool>(acEnt, XDataCode.HasHoles);
        }

        public static TextureDirection GetTextureDirection(this Entity acEnt)
        {
            return (TextureDirection) GetXData<int>(acEnt, XDataCode.TextureDirection);
        }

        public static ProductionType GetProductionType(this Entity acEnt)
        {
            return (ProductionType) GetXData<int>(acEnt, XDataCode.ProductionType);
        }

        public static Handle GetParent(this Entity acEnt)
        {
            return new Handle(Convert.ToInt64(GetXData<string>(acEnt, XDataCode.ParentObject), 16));
        }

        public static List<Handle> GetChildren(this Entity acEnt)
        {
            var sepStr = GetXData<string>(acEnt, XDataCode.ChildObjects);
            var cHandles = new List<Handle>();
            if (sepStr == null) return cHandles;

            if (sepStr.Contains(','))
            {
                var chds = sepStr.Split(',');

                foreach (var chd in chds) cHandles.Add(new Handle(Convert.ToInt64(chd, 16)));
            }
            else
            {
                try
                {
                    cHandles.Add(new Handle(Convert.ToInt64(sepStr, 16)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return cHandles;
        }
    }
}