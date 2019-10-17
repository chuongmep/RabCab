using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CsvHelper;
using RabCab.Analysis;
using RabCab.Calculators;
using RabCab.Engine.Enumerators;
using RabCab.Entities.Controls;
using RabCab.Extensions;
using RabCab.Settings;
using static RabCab.Engine.Enumerators.Enums.SortBy;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

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
                .ThenByDescendingIf(sCrit.HasFlag(Thickness), e => e.Thickness)
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
                if (ent.RcName != string.Empty)
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
                    //Progress progress bar or exit if ESC has been pressed
                    if (!pWorker.Progress()) return mList;

                    var acSol = acTrans.GetObject(objId, OpenMode.ForRead) as Solid3d;

                    if (acSol == null) continue;

                    mList.Add(new EntInfo(acSol, acCurDb, acTrans));
                }
            }

            return mList;
        }

        public static ObjectId[] SelectSimilar(this Entity acEnt, List<string> propFilter, Editor acCurEd,
            Database acCurDb, Transaction acTrans, bool select)
        {
            var entProps = GetProperties(acEnt, propFilter);
            var matchList = new List<ObjectId>();

            //Get the Current Space
            var acCurSpaceBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

            if (acCurSpaceBlkTblRec == null) return new[] {ObjectId.Null};

            var objCount = 0;

            using (var pWorker = new ProgressAgent("Parsing Solids: ", 1))
            {
                foreach (var objId in acCurSpaceBlkTblRec)
                {
                    //Progress progress bar or exit if ESC has been pressed
                    if (!pWorker.Progress())
                    {
                        acTrans.Abort();
                        return new[] {ObjectId.Null};
                    }

                    var compEnt = acTrans.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (compEnt == null) continue;

                    //Check if entity can be parsed by traverse
                    var travSol = compEnt as Solid3d;

                    if (travSol != null)
                    {
                        var eTrav = new EntInfo(travSol, acCurDb, acTrans);
                        travSol.AddXData(eTrav, acCurDb, acTrans);
                    }

                    var compProps = GetProperties(compEnt, propFilter);

                    if (entProps.SequenceEqual(compProps)) matchList.Add(objId);

                    objCount++;
                }
            }

            if (!select) return matchList.ToArray();

            if (matchList.Count > 0)
            {
                acCurEd.SetImpliedSelection(matchList.ToArray());
                acCurEd.WriteMessage($"\n{objCount} Objects parsed - {matchList.Count - 1} duplicates found.");
            }
            else
            {
                acCurEd.WriteMessage("\nNo duplicates found.");
            }

            return matchList.ToArray();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acEnt"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetProperties(Entity acEnt, List<string> filter)
        {
            var propDict = new Dictionary<string, string>();

            //Get Com Properties
            var acadObj = acEnt.AcadObject;
            var props = TypeDescriptor.GetProperties(acadObj);

            //NAME
            if (filter.Contains("PartName"))
                propDict.Add("Part Name", acEnt.GetPartName());
            //LENGTH
            if (filter.Contains("Length"))
                propDict.Add("Length", acEnt.GetPartLength().ToString());
            //WIDTH
            if (filter.Contains("Width"))
                propDict.Add("Width", acEnt.GetPartWidth().ToString());
            //THICKNESS
            if (filter.Contains("Thickness"))
                propDict.Add("Thickness", acEnt.GetPartThickness().ToString());
            //VOLUME
            if (filter.Contains("Volume"))
                propDict.Add("Volume", acEnt.GetPartVolume().ToString());
            //ISSWEEP
            if (filter.Contains("IsSweep"))
                propDict.Add("Is Sweep", acEnt.GetIsSweep().ToString());
            //ISMIRROR
            if (filter.Contains("IsMirror"))
                propDict.Add("Is Mirror", acEnt.GetIsMirror().ToString());
            //HASHOLES
            if (filter.Contains("HasHoles"))
                propDict.Add("Has Holes", acEnt.GetHasHoles().ToString());
            //TXDIRECTION
            if (filter.Contains("Texture"))
                propDict.Add("Texture Direction", acEnt.GetTextureDirection().ToString());
            //PRODTYPE
            if (filter.Contains("Production"))
                propDict.Add("Production", acEnt.GetProductionType().ToString());
            //BASEHANDLE
            if (filter.Contains("ParentHandle"))
                propDict.Add("Parent Handle", acEnt.GetParent().ToString());

            //Iterate through properties
            foreach (PropertyDescriptor prop in props)
            {
                if (!filter.Contains(prop.DisplayName)) continue;

                var value = prop.GetValue(acadObj);
                if (value == null) continue;

                var isNumeric = double.TryParse(value.ToString(), out var checkVal);

                if (isNumeric)
                {
                    checkVal = checkVal.RoundToTolerance();

                    try
                    {
                        propDict.Add(prop.DisplayName, checkVal.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        MailAgent.Report(e.Message);
                    }
                }
                else
                {
                    try
                    {
                        propDict.Add(prop.DisplayName, value.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        MailAgent.Report(e.Message);
                    }
                }
            }

            return propDict;
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

        private static Enums.TableHeader GetTCrit(out int count)
        {
            count = 10;

            var tCrit = Enums.TableHeader.Layer |
                        Enums.TableHeader.Color |
                        Enums.TableHeader.Name |
                        Enums.TableHeader.Width |
                        Enums.TableHeader.Length |
                        Enums.TableHeader.Thickness |
                        Enums.TableHeader.Volume |
                        Enums.TableHeader.Texture |
                        Enums.TableHeader.Production |
                        Enums.TableHeader.Qty;

            if (!SettingsUser.BomLayer)
            {
                tCrit -= Enums.TableHeader.Layer;
                count--;
            }

            if (!SettingsUser.BomColor)
            {
                tCrit -= Enums.TableHeader.Color;
                count--;
            }

            if (!SettingsUser.BomName)
            {
                tCrit -= Enums.TableHeader.Name;
                count--;
            }

            if (!SettingsUser.BomWidth)
            {
                tCrit -= Enums.TableHeader.Width;
                count--;
            }

            if (!SettingsUser.BomLength)
            {
                tCrit -= Enums.TableHeader.Length;
                count--;
            }

            if (!SettingsUser.BomThickness)
            {
                tCrit -= Enums.TableHeader.Thickness;
                count--;
            }

            if (!SettingsUser.BomVolume)
            {
                tCrit -= Enums.TableHeader.Volume;
                count--;
            }

            if (!SettingsUser.BomTextureDirection)
            {
                tCrit -= Enums.TableHeader.Texture;
                count--;
            }

            if (!SettingsUser.BomProductionType)
            {
                tCrit -= Enums.TableHeader.Production;
                count--;
            }

            if (!SettingsUser.BomQty)
            {
                tCrit -= Enums.TableHeader.Qty;
                count--;
            }

            return tCrit;
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
                        //Progress progress bar or exit if ESC has been pressed
                        if (!pWorker.Progress())
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
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {
                    Console.WriteLine(e);
                    MailAgent.Report(e.Message);
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
            var gList = enumerable.OrderBy(e => e.Key.Name)
                .ThenByIf(sCrit.HasFlag(MixS4S), e => e.Key.Box)
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
                        //Progress progress bar or exit if ESC has been pressed
                        if (!pWorker.Progress())
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
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {
                    throw;
                    Console.WriteLine(e);
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
        public static void SortAndExport(this List<EntInfo> eList, string mainPath, ProgressAgent pWorker,
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
            var gList = enumerable.OrderBy(e => e.Key.Name)
                .ThenByIf(sCrit.HasFlag(MixS4S), e => e.Key.Box)
                .ThenByIf(sCrit.HasFlag(Layer), e => e.Key.Layer)
                .ThenByIf(sCrit.HasFlag(Color), e => e.Key.Color)
                .ThenByDescendingIf(sCrit.HasFlag(Thickness), e => e.Key.Thickness)
                .ThenByDescending(e => e.Key.Length)
                .ThenByDescending(e => e.Key.Width)
                .ThenByDescending(e => e.Key.Volume).ToList();


            pWorker.Reset("Exporting Parts: ");
            pWorker.SetTotalOperations(gList.Count());

            var exportList = new List<EntInfo>();
            if (!File.Exists(SettingsUser.ExportTemplatePath))
                MessageBox.Show(new AcadMainWindow(),
                    @"Maching Template File Not Found - Please check directory: " +
                    SettingsUser.ExportTemplatePath);

            if (gList.Count > 0)
                try
                {
                    foreach (var group in gList)
                    {
                        //Progress progress bar or exit if ESC has been pressed
                        if (!pWorker.Progress())
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
                        nonMirrors.ExportParts(mainPath, acCurDb, ref exportList, multAmount);
                        mirrors.UpdatePartData(false, acCurEd, acCurDb, acTrans);
                        mirrors.ExportParts(mainPath, acCurDb, ref exportList, multAmount);
                    }

                    var csvName = mainPath + "\\" + SettingsUser.NamedPartsFileName + ".csv";

                    if (File.Exists(csvName)) File.Delete(csvName);

                    //Write a CSV file containing information from the named entities
                    using (var writer = new StreamWriter(csvName))
                    {
                        using (var csv = new CsvWriter(writer))
                        {
                            csv.Configuration.RegisterClassMap<EntMap>();
                            csv.WriteRecords(exportList);
                        }
                    }

                    acCurEd.WriteMessage("\nParts saved out - opening directory.");
                    Process.Start(mainPath);
                }
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {
                    Console.WriteLine(e);
                    MailAgent.Report(e.Message);
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
        public static void SortToTable(this List<EntInfo> eList, ProgressAgent pWorker,
            Database acCurDb, Editor acCurEd, Transaction acTrans, Table acTable, int multAmount = 1)
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
            var gList = enumerable.OrderBy(e => e.Key.Name)
                .ThenByIf(sCrit.HasFlag(MixS4S), e => e.Key.Box)
                .ThenByIf(sCrit.HasFlag(Layer), e => e.Key.Layer)
                .ThenByIf(sCrit.HasFlag(Color), e => e.Key.Color)
                .ThenByDescendingIf(sCrit.HasFlag(Thickness), e => e.Key.Thickness)
                .ThenByDescending(e => e.Key.Length)
                .ThenByDescending(e => e.Key.Width)
                .ThenByDescending(e => e.Key.Volume).ToList();


            pWorker.Reset("Sorting Solids: ");
            pWorker.SetTotalOperations(gList.Count());

            var partList = new List<EntInfo>();

            if (gList.Count <= 0) return;
            {
                try
                {
                    foreach (var group in gList)
                    {
                        //Progress progress bar or exit if ESC has been pressed
                        if (!pWorker.Progress())
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

                        foreach (var e in nonMirrors)
                        {
                            if (e.IsChild) continue;
                            partList.Add(e);
                        }

                        mirrors.UpdatePartData(false, acCurEd, acCurDb, acTrans);

                        foreach (var e in mirrors)
                        {
                            if (e.IsChild) continue;
                            partList.Add(e);
                        }

                        ;
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {
                    Console.WriteLine(e);
                    MailAgent.Report(e.Message);
                    throw;
                }
            }

            if (partList.Count <= 0) return;

            var tCrit = GetTCrit(out var count);

            var colCount = count;
            var rowCount = partList.Count + 2;

            acTable.SetSize(rowCount, colCount);

            var rowHeight = SettingsUser.TableRowHeight;
            _ = SettingsUser.TableColumnWidth;
            var textHeight = SettingsUser.TableTextHeight;

            acTable.SetRowHeight(rowHeight);

            var header = acTable.Cells[0, 0];
            header.Value = SettingsUser.BomTitle;
            header.Alignment = CellAlignment.MiddleCenter;
            header.TextHeight = textHeight;

            var counter = 1;

            var headers = new List<string>();

            //Create Headers

            foreach (Enum value in Enum.GetValues(tCrit.GetType()))
                if (tCrit.HasFlag(value))
                    headers.Add(value.ToString());

            for (var i = 0; i < headers.Count; i++) acTable.Cells[counter, i].TextString = headers[i];

            counter++;

            foreach (var p in partList)
            {
                for (var i = 0; i < headers.Count; i++)
                {
                    var hText = headers[i];
                    var tString = string.Empty;

                    switch (hText)
                    {
                        case "Layer":
                            tString = p.EntLayer;
                            break;

                        case "Color":
                            tString = p.EntColor.ToString();
                            break;
                        case "Name":
                            tString = p.RcName;
                            break;

                        case "Width":
                            tString = acCurDb.ConvertToDwgUnits(p.Width);
                            break;
                        case "Length":
                            tString = acCurDb.ConvertToDwgUnits(p.Length);
                            break;

                        case "Thickness":
                            tString = acCurDb.ConvertToDwgUnits(p.Thickness);
                            break;

                        case "Volume":
                            tString = acCurDb.ConvertToDwgUnits(p.Volume);
                            break;

                        case "Texture":
                            tString = EnumAgent.GetNameOf(p.TxDirection);
                            break;

                        case "Production":
                            tString = EnumAgent.GetNameOf(p.ProdType);
                            break;
                        case "Qty":
                            tString = (p.RcQtyTotal * multAmount).ToString();
                            break;
                    }

                    if (string.IsNullOrEmpty(tString))
                        tString = string.Empty;

                    acTable.Cells[counter, i].TextString = tString;
                }

                counter++;
            }
        }

        private static void ExportParts(this List<EntInfo> eList, string filePath,
            Database acCurDb, ref List<EntInfo> xPortList, int multAmount)
        {
            foreach (var e in eList)
            {
                if (e.IsChild) continue;

                var saveDirectory = filePath + "\\" + e.EntLayer;


                if (Directory.Exists(saveDirectory) == false) Directory.CreateDirectory(saveDirectory);

                var ePath = saveDirectory + "\\" + e.RcName + ".dwg";

                if (e.RcName == string.Empty || string.IsNullOrEmpty(e.RcName)) ePath = ePath.Replace(".dwg", "#.dwg");

                var exists = true;
                var count = 0;

                while (exists)
                {
                    if (File.Exists(ePath))
                    {
                        var oldCount = count;
                        count++;

                        if (ePath.Contains(oldCount + ".dwg"))
                            ePath = ePath.Replace(oldCount + ".dwg", count + ".dwg");
                        else
                            ePath = ePath.Replace(".dwg", "_" + count + ".dwg");

                        continue;
                    }

                    exists = false;
                }

                Solid3d entClone = null;
                var objectIdCol = new ObjectIdCollection();

                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    var acEnt = acTrans.GetObject(e.ObjId, OpenMode.ForRead) as Solid3d;
                    if (acEnt != null)
                    {
                        entClone = acEnt.Clone() as Solid3d;

                        if (entClone != null)
                        {
                            entClone.TransformBy(e.LayMatrix);
                            acCurDb.AppendEntity(entClone, acTrans);
                            entClone.TopLeftToOrigin();
                            objectIdCol.Add(entClone.ObjectId);
                        }
                    }

                    acTrans.Commit();
                }

                if (entClone == null) continue;

                using (var newDb = new Database(true, false))
                {
                    try
                    {
                        newDb.ReadDwgFile(
                            SettingsUser.ExportTemplatePath,
                            FileOpenMode.OpenForReadAndAllShare, false, null);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    var objIdCol = new ObjectIdCollection {entClone.ObjectId};
                    acCurDb.Wblock(newDb, objIdCol, Point3d.Origin, DuplicateRecordCloning.Replace);

                    using (var tr = newDb.TransactionManager.StartTransaction())
                    {
                        tr.TransactionManager.QueueForGraphicsFlush();

                        var btr =
                            (BlockTableRecord)
                            tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(newDb), OpenMode.ForRead);
                        foreach (var curId in btr)
                        {
                            var ent = (Solid3d) tr.GetObject(curId, OpenMode.ForWrite);

                            if (ent != null)
                            {
                                ent.CleanBody();
                                ent.TransformBy(
                                    Matrix3d.Displacement(
                                        ent.GeometricExtents.MinPoint.GetVectorTo(Point3d.Origin)));
                            }
                        }

                        tr.Commit();
                    }

                    newDb.ZoomExtents();

                    //save as 2004 version dwg
                    try
                    {
                        newDb.SaveAs(ePath, SettingsUser.SaveVersion);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    newDb.CloseInput(true);
                    e.FilePath = ePath;
                }

                using (var acTrans = acCurDb.TransactionManager.StartOpenCloseTransaction())
                {
                    entClone = acTrans.GetObject(entClone.ObjectId, OpenMode.ForWrite) as Solid3d;

                    if (entClone != null)
                    {
                        entClone.Erase();
                        entClone.Dispose();
                    }

                    acTrans.Commit();
                }

                e.RcQtyTotal *= multAmount;
                xPortList.Add(e);
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

                //Manipulate based on texture direction - width & length
                //TODO

                if (e.TxDirection == Enums.TextureDirection.Across)
                {
                    var cInfo = new EntInfo(cloneSol, acCurDb, acTrans);
                    if (cloneSol != null) cloneSol.TransformBy(cInfo.Z90);

                    yStep = cloneSol.TopLeftTo(layPoint.Convert3D());

                    using (var acText = new MText())
                    {
                        acText.TextHeight = SettingsUser.LayTextHeight;
                        acText.Contents = "<<TEXTURE>>";

                        //ParseAndFill the insertion point and text alignment
                        double zPt = 0;

                        var yPt = layPoint.Y - acText.TextHeight - yStep / 2;
                        zPt = e.Thickness + .01;

                        var xPt = layPoint.X + cInfo.Length / 2;
                        acText.Attachment = AttachmentPoint.MiddleCenter;

                        acText.Location = new Point3d(xPt, yPt, zPt);
                        acText.Width = cInfo.Length;

                        //Append the text
                        acCurDb.AppendEntity(acText, acTrans);
                    }
                }
                else if (e.TxDirection == Enums.TextureDirection.Along)
                {
                    using (var acText = new MText())
                    {
                        acText.TextHeight = SettingsUser.LayTextHeight;
                        acText.Contents = "<<TEXTURE>>";

                        //ParseAndFill the insertion point and text alignment
                        double zPt = 0;

                        var yPt = layPoint.Y - acText.TextHeight - yStep / 2;
                        zPt = e.Thickness + .01;

                        //Default Lay Left
                        var xPt = layPoint.X + e.Length / 2;
                        acText.Attachment = AttachmentPoint.MiddleCenter;

                        acText.Location = new Point3d(xPt, yPt, zPt);
                        acText.Width = e.Length;

                        //Append the text
                        acCurDb.AppendEntity(acText, acTrans);
                    }
                }

                var cloneHandle = cloneSol.Handle;
                e.ChildHandles.Add(cloneHandle);

                acSol.UpdateXData(e.ChildHandles, Enums.XDataCode.ChildObjects, acCurDb, acTrans);
                cloneSol.UpdateXData(acSol.Handle, Enums.XDataCode.ParentObject, acCurDb, acTrans);
                cloneSol.UpdateXData(string.Empty, Enums.XDataCode.ChildObjects, acCurDb, acTrans);

                if (cloneSol.CheckRotation())
                    cloneSol.TopLeftTo(layPoint.Convert3D());

                var longStep = yStep;

                if (SettingsUser.LayFlatShot)
                {
                    var acCurEd = Application.DocumentManager.CurrentDocument.Editor;
                    var userCoordSystem = acCurEd.CurrentUserCoordinateSystem;


                    if (SettingsUser.LayAllSidesFlatShot)
                    {
                        longStep = cloneSol.FlattenAllSides(acCurDb, acCurEd, acTrans, false, false);
                    }
                    else
                    {
                        cloneSol.Flatten(acTrans, acCurDb, acCurEd, true, false, true, userCoordSystem);

                        if (SettingsUser.RetainHiddenLines)
                            cloneSol.Flatten(acTrans, acCurDb, acCurEd, false, true, true, userCoordSystem);
                    }
                }

                using (var acText = new MText())
                {
                    acText.TextHeight = SettingsUser.LayTextHeight;
                    acText.Contents = e.RcName +
                                      " | MAT: " + e.EntLayer +
                                      " | TECH: " + EnumAgent.GetNameOf(e.ProdType) +
                                      " | QTY PER ASM: " + e.RcQtyTotal;
                    acText.Layer = "Defpoints";
                    //acText.ColorIndex = ;                           

                    //ParseAndFill the insertion point and text alignment
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

                if (SettingsUser.LayFlatShot)
                {
                    var curChildren = acSol.GetChildren();
                    var removableChild = cloneSol.Handle;
                    curChildren.Remove(removableChild);

                    acSol.UpdateXData(curChildren, Enums.XDataCode.ChildObjects, acCurDb, acTrans);

                    cloneSol.Erase();
                    cloneSol.Dispose();

                    if (SettingsUser.LayAllSidesFlatShot) yStep = longStep;
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
                    eInfo.ParentHandle = baseInfo.Hndl;
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
            Func<T, TKey> sel)
        {
            return predicate ? list.OrderBy(f => sel(f)) : list.OrderBy(a => 1);
        }

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
            Func<T, TKey> sel)
        {
            return predicate ? list.OrderByDescending(f => sel(f)) : list.OrderBy(a => 1);
        }

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
            Func<T, TKey> sel)
        {
            return predicate ? list.ThenBy(f => sel(f)) : list;
        }

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
            Func<T, TKey> sel)
        {
            return predicate ? list.ThenByDescending(f => sel(f)) : list;
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