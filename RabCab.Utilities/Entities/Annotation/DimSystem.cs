using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using RabCab.Calculators;
using RabCab.Engine.Enumerators;

namespace RabCab.Entities.Annotation
{
    internal class DimSystem
    {
        public List<RotatedDimension> SysList;

        public bool Highlighted { get; set; }
        public int SystemCount { get; set; }

        public DimSystem()
        {
            SystemCount = 0;
            SysList = new List<RotatedDimension>();
            Highlighted = false;
        }

        public void DeletePointByIndex(int index)
        {
            var database = Application.DocumentManager.MdiActiveDocument.Database;
            var dimPoints = GetDimPoints(CalcTol.ReturnCurrentTolerance());
            if (dimPoints.Count < index) return;
            var item = dimPoints[index];
            var topTransaction = database.TransactionManager.TopTransaction;
            using (topTransaction)
            {
                if (!item.IsLast)
                {
                    var dim1 = item.Dim1;
                    var dim2 = item.Dim2;
                    if (!dim1.IsWriteEnabled) dim1.UpgradeOpen();
                    if (!dim2.IsWriteEnabled) dim2.UpgradeOpen();
                    var dim1PointIndex = item.Dim1PointIndex;
                    var dim2PointIndex = item.Dim2PointIndex;
                    if (dim1PointIndex == 1)
                    {
                        if (dim2PointIndex != 1)
                            dim1.XLine1Point = dim2.XLine1Point;
                        else
                            dim1.XLine1Point = dim2.XLine2Point;
                    }
                    else if (dim2PointIndex != 1)
                    {
                        dim1.XLine2Point = dim2.XLine1Point;
                    }
                    else
                    {
                        dim1.XLine2Point = dim2.XLine2Point;
                    }
 dim1.UsingDefaultTextPosition = true;
                    dim2.Unhighlight();
                    dim2.Erase();
                    SysList.Remove(dim2);
                    SystemCount = SystemCount - 1;
                }
                else
                {
                    var rotatedDimension = item.Dim1;
                    if (!rotatedDimension.IsWriteEnabled) rotatedDimension.UpgradeOpen();
                    rotatedDimension.Unhighlight();
                    rotatedDimension.Erase();
                    SysList.Remove(rotatedDimension);
                    SystemCount = SystemCount - 1;
                }

                topTransaction.TransactionManager.QueueForGraphicsFlush();
            }
        }

        public void DeletePointByLine(Point3d fencePoint1, Point3d fencePoint2)
        {
            var equalPointDistance = CalcTol.ReturnCurrentTolerance();
            var dimPoints = GetDimPoints(equalPointDistance);
            if (dimPoints.Count == 0) return;
            var dimLinePoint = dimPoints[0].DimLinePoint;
            var point3d = dimPoints[1].DimLinePoint;
            var plane = Matrix3d.WorldToPlane(dimPoints[0].Dim1.Normal);
            var point3d1 = dimLinePoint.TransformBy(plane);
            var point2d = new Point2d(point3d1.X, point3d1.Y);
            var point3d2 = point3d.TransformBy(plane);
            var point2d1 = new Point2d(point3d2.X, point3d2.Y);
            var point3d3 = fencePoint1.TransformBy(plane);
            var point2d2 = new Point2d(point3d3.X, point3d3.Y);
            var point3d4 = fencePoint2.TransformBy(plane);
            var point2d3 = new Point2d(point3d4.X, point3d4.Y);
            var line2d = new Line2d(point2d, point2d1);
            var point = line2d.GetClosestPointTo(point2d2).Point;
            var point1 = line2d.GetClosestPointTo(point2d3).Point;
            var distanceTo = point.GetDistanceTo(point1);
            var flag = false;
            do
            {
                if (SysList.Count == 0) return;
                if (flag) dimPoints = GetDimPoints(equalPointDistance);
                var num = 0;
                foreach (var dimPoint in dimPoints)
                {
                    var point3d5 = dimPoint.DimLinePoint.TransformBy(plane);
                    var point2d4 = new Point2d(point3d5.X, point3d5.Y);
                    var distanceTo1 = point2d4.GetDistanceTo(point);
                    var num1 = point2d4.GetDistanceTo(point1);
                    if (distanceTo1 > distanceTo || num1 > distanceTo)
                    {
                        flag = false;
                        num++;
                    }
                    else
                    {
                        DeletePointByIndex(num);
                        flag = true;
                        break;
                    }
                }
            } while (flag);
        }

        public void DeletePointByPoint(Point3d deletePoint)
        {
            var database = Application.DocumentManager.MdiActiveDocument.Database;
            var dimPoints = GetDimPoints(CalcTol.ReturnCurrentTolerance());
            var nums = new List<double>();
            if (dimPoints.Count == 0) return;
            foreach (var dimPoint in dimPoints) nums.Add(deletePoint.DistanceTo(dimPoint.DimLinePoint));
            var num = nums.IndexOf(nums.Min());
            var item = dimPoints[num];
            var topTransaction = database.TransactionManager.TopTransaction;
            using (topTransaction)
            {
                if (!item.IsLast)
                {
                    var dim1 = item.Dim1;
                    var dim2 = item.Dim2;
                    if (!dim1.IsWriteEnabled) dim1.UpgradeOpen();
                    if (!dim2.IsWriteEnabled) dim2.UpgradeOpen();
                    var dim1PointIndex = item.Dim1PointIndex;
                    var dim2PointIndex = item.Dim2PointIndex;
                    if (dim1PointIndex == 1)
                    {
                        if (dim2PointIndex != 1)
                            dim1.XLine1Point = dim2.XLine1Point;
                        else
                            dim1.XLine1Point = dim2.XLine2Point;
                    }
                    else if (dim2PointIndex != 1)
                    {
                        dim1.XLine2Point = dim2.XLine1Point;
                    }
                    else
                    {
                        dim1.XLine2Point = dim2.XLine2Point;
                    }
                    dim1.UsingDefaultTextPosition = true;
                    dim2.Unhighlight();
                    dim2.Erase();
                    SysList.Remove(dim2);
                    SystemCount = SystemCount - 1;
                    if (Highlighted) dim1.Highlight();
                }
                else
                {
                    var rotatedDimension = item.Dim1;
                    if (!rotatedDimension.IsWriteEnabled) rotatedDimension.UpgradeOpen();
                    rotatedDimension.Unhighlight();
                    rotatedDimension.Erase();
                    SysList.Remove(rotatedDimension);
                    SystemCount = SystemCount - 1;
                }

                topTransaction.TransactionManager.QueueForGraphicsFlush();
            }
        }

        public void ExtendDimSystemBasePoint(int pntIndex, int extendTo, Point3d newPoint, double equalDistTolerance)
        {
            var dimPoints = GetDimPoints(equalDistTolerance);
            if (dimPoints.Count == 0) return;
            if (dimPoints.Count < pntIndex) return;
            var item = dimPoints[pntIndex];
            double num = 0;
            double num1 = 0;
            if (!item.IsLast)
            {
                var dimLinePoint = item.DimLinePoint;
                Point3d point3d;
                point3d = item.Dim1PointIndex != 1 ? item.Dim1.XLine2Point : item.Dim1.XLine1Point;
                Point3d point3d1;
                point3d1 = item.Dim2PointIndex != 1 ? item.Dim2.XLine2Point : item.Dim2.XLine1Point;
                num = point3d.DistanceTo(dimLinePoint);
                num1 = point3d1.DistanceTo(dimLinePoint);
            }

            if (extendTo == 1 && !item.IsLast)
            {
                if (num < num1)
                {
                    if (!item.Dim2.IsWriteEnabled) item.Dim2.UpgradeOpen();
                    if (item.Dim2PointIndex == 1)
                    {
                        if (item.Dim1PointIndex == 1)
                        {
                            item.Dim2.XLine1Point = item.Dim1.XLine1Point;
                            return;
                        }

                        item.Dim2.XLine1Point = item.Dim1.XLine2Point;
                        return;
                    }

                    if (item.Dim1PointIndex == 1)
                    {
                        item.Dim2.XLine2Point = item.Dim1.XLine1Point;
                        return;
                    }

                    item.Dim2.XLine2Point = item.Dim1.XLine2Point;
                    return;
                }

                if (!item.Dim1.IsWriteEnabled) item.Dim1.UpgradeOpen();
                if (item.Dim1PointIndex == 1)
                {
                    if (item.Dim2PointIndex == 1)
                    {
                        item.Dim1.XLine1Point = item.Dim2.XLine1Point;
                        return;
                    }

                    item.Dim1.XLine1Point = item.Dim2.XLine2Point;
                    return;
                }

                if (item.Dim2PointIndex == 1)
                {
                    item.Dim1.XLine2Point = item.Dim2.XLine1Point;
                    return;
                }

                item.Dim1.XLine2Point = item.Dim2.XLine2Point;
                return;
            }

            if (extendTo == 2 && !item.IsLast)
            {
                if (num > num1)
                {
                    if (!item.Dim2.IsWriteEnabled) item.Dim2.UpgradeOpen();
                    if (item.Dim2PointIndex == 1)
                    {
                        if (item.Dim1PointIndex == 1)
                        {
                            item.Dim2.XLine1Point = item.Dim1.XLine1Point;
                            return;
                        }

                        item.Dim2.XLine1Point = item.Dim1.XLine2Point;
                        return;
                    }

                    if (item.Dim1PointIndex == 1)
                    {
                        item.Dim2.XLine2Point = item.Dim1.XLine1Point;
                        return;
                    }

                    item.Dim2.XLine2Point = item.Dim1.XLine2Point;
                    return;
                }

                if (!item.Dim1.IsWriteEnabled) item.Dim1.UpgradeOpen();
                if (item.Dim1PointIndex == 1)
                {
                    if (item.Dim2PointIndex == 1)
                    {
                        item.Dim1.XLine1Point = item.Dim2.XLine1Point;
                        return;
                    }

                    item.Dim1.XLine1Point = item.Dim2.XLine2Point;
                    return;
                }

                if (item.Dim2PointIndex == 1)
                {
                    item.Dim1.XLine2Point = item.Dim2.XLine1Point;
                    return;
                }

                item.Dim1.XLine2Point = item.Dim2.XLine2Point;
                return;
            }

            if (extendTo == 0)
            {
                var plane = Matrix3d.WorldToPlane(dimPoints[0].Dim1.Normal);
                var dimLinePoint1 = item.DimLinePoint;
                Point3d point3d2;
                point3d2 = item.Dim1PointIndex != 1 ? item.Dim1.XLine2Point : item.Dim1.XLine1Point;
                var point3d3 = dimLinePoint1.TransformBy(plane);
                var point2d = new Point2d(point3d3.X, point3d3.Y);
                var point3d4 = point3d2.TransformBy(plane);
                var point2d1 = new Point2d(point3d4.X, point3d4.Y);
                var point3d5 = newPoint.TransformBy(plane);
                var point2d2 = new Point2d(point3d5.X, point3d5.Y);
                var line2d = new Line2d(point2d, point2d1);
                var point = line2d.GetClosestPointTo(point2d2).Point;
                var point3d6 = new Point3d(point.X, point.Y, 0);
                var point3d7 = point3d6.TransformBy(plane.Inverse());
                if (!item.Dim1.IsWriteEnabled) item.Dim1.UpgradeOpen();
                if (!item.IsLast && !item.Dim2.IsWriteEnabled) item.Dim2.UpgradeOpen();
                if (item.Dim1PointIndex != 1)
                    item.Dim1.XLine2Point = point3d7;
                else
                    item.Dim1.XLine1Point = point3d7;
                if (!item.IsLast)
                {
                    if (item.Dim2PointIndex == 1)
                    {
                        item.Dim2.XLine1Point = point3d7;
                        return;
                    }

                    item.Dim2.XLine2Point = point3d7;
                }
            }
        }

        public int GetClosestDimPoint(Point3d point, double equalDistTolerance)
        {
            var dimPoints = GetDimPoints(equalDistTolerance);
            var nums = new List<double>();
            if (dimPoints.Count == 0) return -1;
            foreach (var dimPoint in dimPoints) nums.Add(point.DistanceTo(dimPoint.DimLinePoint));
            return nums.IndexOf(nums.Min());
        }

        public static DimSystem GetDimSystem(RotatedDimension masterDim, double angleTolerance,
            double equalDistTolerance)
        {
            var database = Application.DocumentManager.MdiActiveDocument.Database;
            var dimSystem = new DimSystem();
            using (database.TransactionManager.TopTransaction)
            {
                var objectIdCollections = ZGetDimsWithSameAngle(masterDim.Rotation, masterDim.BlockId, masterDim.Normal,
                    masterDim.Id, masterDim.LayerId, angleTolerance);
                var point3dCollections = new Point3dCollection();
                masterDim.GetStretchPoints(point3dCollections);
                var item = point3dCollections[2];
                var point3d = point3dCollections[3];
                var objectIdCollections1 = ZGetDimsOnSameLine(objectIdCollections, item, point3d, equalDistTolerance);
                var eachOther = ZGetDimsNextToEachOther(objectIdCollections1, item, point3d, equalDistTolerance);
                eachOther.Add(masterDim);
                dimSystem.SysList = eachOther;
                dimSystem.SystemCount = eachOther.Count;
            }

            return dimSystem;
        }

        public List<DimPoint> GetDimPoints(double equalDistTolerance)
        {
            var dimSystem = this;
            var dimPoints = new List<DimPoint>();
            foreach (var listOfDim in dimSystem.SysList)
            {
                var point3dCollections = new Point3dCollection();
                listOfDim.GetStretchPoints(point3dCollections);
                var item = point3dCollections[2];
                var point3d = point3dCollections[3];
                var flag = false;
                var flag1 = false;
                var enumerator = dimSystem.SysList.GetEnumerator();
                try
                {
                    do
                    {
                    Label0:
                        if (!enumerator.MoveNext()) break;
                        var current = enumerator.Current;
                        if (current != listOfDim)
                        {
                            var point3dCollections1 = new Point3dCollection();
                            if (current != null)
                            {
                                current.GetStretchPoints(point3dCollections1);
                                var item1 = point3dCollections1[2];
                                var point3d1 = point3dCollections1[3];
                                var num = item.DistanceTo(item1);
                                var num1 = item.DistanceTo(point3d1);
                                var num2 = point3d.DistanceTo(item1);
                                var num3 = point3d.DistanceTo(point3d1);
                                if (num < equalDistTolerance)
                                {
                                    var flag2 = true;
                                    foreach (var dimPoint in dimPoints)
                                        if (dimPoint.Dim1 != listOfDim)
                                        {
                                            if (!(dimPoint.Dim2 == listOfDim) || dimPoint.Dim2PointIndex != 1) continue;
                                            flag2 = false;
                                            flag = true;
                                        }
                                        else
                                        {
                                            if (dimPoint.Dim1PointIndex != 1) continue;
                                            flag2 = false;
                                            flag = true;
                                        }

                                    if (flag2)
                                    {
                                        var dimPoint1 = new DimPoint
                                        {
                                            Dim1 = listOfDim,
                                            Dim2 = current,
                                            Dim1PointIndex = 1,
                                            Dim2PointIndex = 1,
                                            IsLast = false,
                                            DimLinePoint = item
                                        };
                                        flag = true;
                                        dimPoints.Add(dimPoint1);
                                    }
                                }
                                else if (num1 < equalDistTolerance)
                                {
                                    var flag3 = true;
                                    foreach (var dimPoint2 in dimPoints)
                                        if (dimPoint2.Dim1 != listOfDim)
                                        {
                                            if (!(dimPoint2.Dim2 == listOfDim) || dimPoint2.Dim2PointIndex != 1)
                                                continue;
                                            flag3 = false;
                                            flag = true;
                                        }
                                        else
                                        {
                                            if (dimPoint2.Dim1PointIndex != 1) continue;
                                            flag3 = false;
                                            flag = true;
                                        }

                                    if (flag3)
                                    {
                                        var dimPoint3 = new DimPoint
                                        {
                                            Dim1 = listOfDim,
                                            Dim2 = current,
                                            Dim1PointIndex = 1,
                                            Dim2PointIndex = 2,
                                            IsLast = false,
                                            DimLinePoint = item
                                        };
                                        flag = true;
                                        dimPoints.Add(dimPoint3);
                                    }
                                }

                                if (num2 < equalDistTolerance)
                                {
                                    var flag4 = true;
                                    foreach (var dimPoint4 in dimPoints)
                                        if (dimPoint4.Dim1 != listOfDim)
                                        {
                                            if (!(dimPoint4.Dim2 == listOfDim) || dimPoint4.Dim2PointIndex != 2)
                                                continue;
                                            flag4 = false;
                                            flag1 = true;
                                        }
                                        else
                                        {
                                            if (dimPoint4.Dim1PointIndex != 2) continue;
                                            flag4 = false;
                                            flag1 = true;
                                        }

                                    if (flag4)
                                    {
                                        var dimPoint5 = new DimPoint
                                        {
                                            Dim1 = listOfDim,
                                            Dim2 = current,
                                            Dim1PointIndex = 2,
                                            Dim2PointIndex = 1,
                                            IsLast = false,
                                            DimLinePoint = point3d
                                        };
                                        flag1 = true;
                                        dimPoints.Add(dimPoint5);
                                    }
                                }

                                if (num3 >= equalDistTolerance) continue;
                                var flag5 = true;
                                foreach (var dimPoint6 in dimPoints)
                                    if (dimPoint6.Dim1 != listOfDim)
                                    {
                                        if (!(dimPoint6.Dim2 == listOfDim) || dimPoint6.Dim2PointIndex != 2) continue;
                                        flag5 = false;
                                        flag1 = true;
                                    }
                                    else
                                    {
                                        if (dimPoint6.Dim1PointIndex != 2) continue;
                                        flag5 = false;
                                        flag1 = true;
                                    }

                                if (!flag5) continue;
                                var dimPoint7 = new DimPoint
                                {
                                    Dim1 = listOfDim,
                                    Dim2 = current,
                                    Dim1PointIndex = 2,
                                    Dim2PointIndex = 2,
                                    IsLast = false,
                                    DimLinePoint = point3d
                                };
                                flag1 = true;
                                dimPoints.Add(dimPoint7);
                            }
                        }
                        else
                        {
                            goto Label0;
                        }
                    } while (!flag || !flag1);
                }
                finally
                {
                    ((IDisposable)enumerator).Dispose();
                }

                if (!flag)
                {
                    var dimPoint8 = new DimPoint
                    {
                        Dim1 = listOfDim,
                        Dim2 = null,
                        Dim1PointIndex = 1,
                        Dim2PointIndex = -1,
                        IsLast = true,
                        DimLinePoint = item
                    };
                    dimPoints.Add(dimPoint8);
                }

                if (flag1) continue;
                var dimPoint9 = new DimPoint
                {
                    Dim1 = listOfDim,
                    Dim2 = null,
                    Dim1PointIndex = 2,
                    Dim2PointIndex = -1,
                    IsLast = true,
                    DimLinePoint = point3d
                };
                dimPoints.Add(dimPoint9);
            }

            return dimPoints;
        }

        public List<int> GetDimPointsByLine(Point3d fencePoint1, Point3d fencePoint2, double equalDistTolerance)
        {
            var nums = new List<int>();
            var dimPoints = GetDimPoints(equalDistTolerance);
            if (dimPoints.Count == 0) return nums;
            var dimLinePoint = dimPoints[0].DimLinePoint;
            var point3d = dimPoints[1].DimLinePoint;
            var plane = Matrix3d.WorldToPlane(dimPoints[0].Dim1.Normal);
            var point3d1 = dimLinePoint.TransformBy(plane);
            var point2d = new Point2d(point3d1.X, point3d1.Y);
            var point3d2 = point3d.TransformBy(plane);
            var point2d1 = new Point2d(point3d2.X, point3d2.Y);
            var point3d3 = fencePoint1.TransformBy(plane);
            var point2d2 = new Point2d(point3d3.X, point3d3.Y);
            var point3d4 = fencePoint2.TransformBy(plane);
            var point2d3 = new Point2d(point3d4.X, point3d4.Y);
            var line2d = new Line2d(point2d, point2d1);
            var point = line2d.GetClosestPointTo(point2d2).Point;
            var point1 = line2d.GetClosestPointTo(point2d3).Point;
            var distanceTo = point.GetDistanceTo(point1);
            var num = 0;
            foreach (var dimPoint in dimPoints)
            {
                var point3d5 = dimPoint.DimLinePoint.TransformBy(plane);
                var point2d4 = new Point2d(point3d5.X, point3d5.Y);
                var distanceTo1 = point2d4.GetDistanceTo(point);
                var num1 = point2d4.GetDistanceTo(point1);
                if (distanceTo1 <= distanceTo && num1 <= distanceTo) nums.Add(num);
                num++;
            }

            return nums;
        }

        public void Highlight()
        {
            foreach (var listOfDim in SysList)
            {
                listOfDim.Highlight();
                Highlighted = true;
            }
        }

        public void InsertPoint(Point3d newPoint)
        {
            var database = Application.DocumentManager.MdiActiveDocument.Database;
            var flag = false;
            var nums = new List<double>();
            RotatedDimension item = null;
            RotatedDimension rotatedDimension;
            foreach (var listOfDim in SysList)
            {
                var point3dCollections = new Point3dCollection();
                listOfDim.GetStretchPoints(point3dCollections);
                var point3d = point3dCollections[2];
                var item1 = point3dCollections[3];
                var plane = Matrix3d.WorldToPlane(listOfDim.Normal);
                var point3d1 = point3d.TransformBy(plane);
                var point2d = new Point2d(point3d1.X, point3d1.Y);
                var point3d2 = item1.TransformBy(plane);
                var point2d1 = new Point2d(point3d2.X, point3d2.Y);
                var point3d3 = newPoint.TransformBy(plane);
                var point2d2 = new Point2d(point3d3.X, point3d3.Y);
                var line2d = new Line2d(point2d, point2d1);
                var point = line2d.GetClosestPointTo(point2d2).Point;
                var distanceTo = point2d.GetDistanceTo(point2d1);
                if (point2d.GetDistanceTo(point) > distanceTo || point2d1.GetDistanceTo(point) > distanceTo)
                {
                    var num = point.GetDistanceTo(point2d);
                    var distanceTo1 = point.GetDistanceTo(point2d1);
                    nums.Add(Math.Min(num, distanceTo1));
                }
                else
                {
                    flag = true;
                    item = listOfDim;
                }
            }

            var topTransaction = database.TransactionManager.TopTransaction;
            using (topTransaction)
            {
                var obj = topTransaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                if (!flag)
                {
                    var num1 = nums.IndexOf(nums.Min());
                    item = SysList[num1];
                    rotatedDimension = (RotatedDimension)item.Clone();
                    var point3dCollections1 = new Point3dCollection();
                    item.GetStretchPoints(point3dCollections1);
                    var item2 = point3dCollections1[2];
                    var item3 = point3dCollections1[3];
                    if (newPoint.DistanceTo(item2) >= newPoint.DistanceTo(item3))
                    {
                        rotatedDimension.XLine1Point = item.XLine2Point;
                        rotatedDimension.XLine2Point = newPoint;
                    }
                    else
                    {
                        rotatedDimension.XLine1Point = newPoint;
                        rotatedDimension.XLine2Point = item.XLine1Point;
                    }

                    rotatedDimension.UsingDefaultTextPosition = true;
                }
                else
                {
                    rotatedDimension = (RotatedDimension)item.Clone();
                    if (!item.IsWriteEnabled) item.UpgradeOpen();
                    item.XLine2Point = newPoint;
                    rotatedDimension.XLine1Point = newPoint;
                     item.UsingDefaultTextPosition = true;
                     rotatedDimension.UsingDefaultTextPosition = true;
                }

                if (obj != null) obj.AppendEntity(rotatedDimension);
                topTransaction.AddNewlyCreatedDBObject(rotatedDimension, true);
            }

            SysList.Add(rotatedDimension);
            SystemCount = SystemCount + 1;
            if (Highlighted)
            {
                topTransaction.TransactionManager.QueueForGraphicsFlush();
                rotatedDimension.Highlight();
                item.Highlight();
            }
        }

        public void MoveDimSystem(Point3d newPoint, double equalDistTolerance)
        {
            var dimPoints = GetDimPoints(equalDistTolerance);
            if (dimPoints.Count == 0) return;
            var dimLinePoint = dimPoints[0].DimLinePoint;
            var point3d = dimPoints[1].DimLinePoint;
            var plane = Matrix3d.WorldToPlane(dimPoints[0].Dim1.Normal);
            var point3d1 = dimLinePoint.TransformBy(plane);
            var point2d = new Point2d(point3d1.X, point3d1.Y);
            var point3d2 = point3d.TransformBy(plane);
            var point2d1 = new Point2d(point3d2.X, point3d2.Y);
            var point3d3 = newPoint.TransformBy(plane);
            var point2d2 = new Point2d(point3d3.X, point3d3.Y);
            var line2d = new Line2d(point2d2, point2d.GetVectorTo(point2d1));
            foreach (var listOfDim in SysList)
            {
                var point3d4 = listOfDim.DimLinePoint.TransformBy(plane);
                var point2d3 = new Point2d(point3d4.X, point3d4.Y);
                var point = line2d.GetClosestPointTo(point2d3).Point;
                var point3d5 = new Point3d(point.X, point.Y, 0);
                var point3d6 = point3d5.TransformBy(plane.Inverse());
                if (!listOfDim.IsWriteEnabled) listOfDim.UpgradeOpen();
                if (!listOfDim.UsingDefaultTextPosition)
                {
                    var dimLinePoint1 = listOfDim.DimLinePoint;
                    var textPosition = listOfDim.TextPosition;
                    listOfDim.DimLinePoint = point3d6;
                    var vectorTo = dimLinePoint1.GetVectorTo(listOfDim.DimLinePoint);
                    listOfDim.TextPosition = textPosition.Add(vectorTo);
                }
                else
                {
                    listOfDim.DimLinePoint = point3d6;
                }
            }
        }

        public void PointProperties(int pntIndex, bool modifyArrowhead, string arrowheadName, bool modifyExtensionLine,
            bool suppressExtLine, double equalDistTolerance)
        {
            int num;
            int num1;
            int num2;
            var dimPoints = GetDimPoints(equalDistTolerance);
            if (dimPoints.Count == 0) return;
            if (dimPoints.Count < pntIndex) return;
            var item = dimPoints[pntIndex];
            var objectId = new ObjectId();
            if (modifyArrowhead && !(arrowheadName == ".")) objectId = ZGetArrowObjectId(arrowheadName);
            if (item.IsLast)
            {
                num2 = item.Dim1PointIndex != 1 ? 2 : 1;
                if (!item.Dim1.IsWriteEnabled) item.Dim1.UpgradeOpen();
                if (modifyExtensionLine)
                {
                    if (num2 != 1)
                        item.Dim1.Dimse2 = suppressExtLine;
                    else
                        item.Dim1.Dimse1 = suppressExtLine;
                }

                if (modifyArrowhead)
                {
                    var dimblk = new ObjectId();
                    var flag = false;
                    if (objectId.IsNull && !item.Dim1.Dimblk.IsNull)
                    {
                        dimblk = item.Dim1.Dimblk;
                        item.Dim1.Dimblk = objectId;
                        flag = true;
                    }

                    item.Dim1.Dimsah = true;
                    if (num2 != 1)
                    {
                        item.Dim1.Dimblk2 = objectId;
                        if (flag) item.Dim1.Dimblk1 = dimblk;
                    }
                    else
                    {
                        item.Dim1.Dimblk1 = objectId;
                        if (flag) item.Dim1.Dimblk2 = dimblk;
                    }
                }

                item.Dim1.RecomputeDimensionBlock(true);
                return;
            }

            num = item.Dim1PointIndex != 1 ? 2 : 1;
            num1 = item.Dim2PointIndex != 1 ? 2 : 1;
            if (!item.Dim1.IsWriteEnabled) item.Dim1.UpgradeOpen();
            if (!item.Dim2.IsWriteEnabled) item.Dim2.UpgradeOpen();
            if (modifyExtensionLine)
            {
                if (num != 1)
                    item.Dim1.Dimse2 = suppressExtLine;
                else
                    item.Dim1.Dimse1 = suppressExtLine;
                if (num1 != 1)
                    item.Dim2.Dimse2 = suppressExtLine;
                else
                    item.Dim2.Dimse1 = suppressExtLine;
            }

            if (modifyArrowhead)
            {
                var dimblk1 = new ObjectId();
                var objectId1 = new ObjectId();
                var flag1 = false;
                var flag2 = false;
                if (objectId.IsNull)
                {
                    if (!item.Dim1.Dimblk.IsNull)
                    {
                        dimblk1 = item.Dim1.Dimblk;
                        item.Dim1.Dimblk = objectId;
                        flag1 = true;
                    }

                    if (!item.Dim2.Dimblk.IsNull)
                    {
                        objectId1 = item.Dim2.Dimblk;
                        item.Dim2.Dimblk = objectId;
                        flag2 = true;
                    }
                }

                item.Dim1.Dimsah = true;
                if (num != 1)
                {
                    item.Dim1.Dimblk2 = objectId;
                    if (flag1) item.Dim1.Dimblk1 = dimblk1;
                }
                else
                {
                    item.Dim1.Dimblk1 = objectId;
                    if (flag1) item.Dim1.Dimblk2 = dimblk1;
                }

                item.Dim2.Dimsah = true;
                if (num1 != 1)
                {
                    item.Dim2.Dimblk2 = objectId;
                    if (flag2) item.Dim2.Dimblk1 = objectId1;
                }
                else
                {
                    item.Dim2.Dimblk1 = objectId;
                    if (flag2) item.Dim2.Dimblk2 = objectId1;
                }
            }

            item.Dim1.RecomputeDimensionBlock(true);
            item.Dim2.RecomputeDimensionBlock(true);
        }

        public void Unhighlight()
        {
            foreach (var listOfDim in SysList)
            {
                listOfDim.Unhighlight();
                Highlighted = false;
            }
        }

        private static ObjectId ZGetArrowObjectId(string newArrName)
        {
            ObjectId @null;
            var database = Application.DocumentManager.MdiActiveDocument.Database;
            var systemVariable = Application.GetSystemVariable("DIMBLK") as string;
            Application.SetSystemVariable("DIMBLK", newArrName);
            if (systemVariable != null && systemVariable.Length == 0)
                Application.SetSystemVariable("DIMBLK", ".");
            else
                Application.SetSystemVariable("DIMBLK", systemVariable);
            var topTransaction = database.TransactionManager.TopTransaction;
            using (topTransaction)
            {
                var obj = (BlockTable)topTransaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                @null = obj[newArrName];
            }

            return @null;
        }

        private static List<RotatedDimension> ZGetDimsNextToEachOther(ObjectIdCollection originDimsColl,
            Point3d masterDimLineStartPnt, Point3d masterDimLineEndPnt, double equalDistTolerance)
        {
            bool flag;
            var database = Application.DocumentManager.MdiActiveDocument.Database;
            var rotatedDimensions = new List<RotatedDimension>();
            var point3d = masterDimLineEndPnt;
            var point3d1 = masterDimLineStartPnt;
            var topTransaction = database.TransactionManager.TopTransaction;
            using (topTransaction)
            {
                do
                {
                    flag = false;
                    foreach (ObjectId objectId in originDimsColl)
                    {
                        var obj = (Entity)topTransaction.GetObject(objectId, OpenMode.ForRead);
                        if (!(obj is RotatedDimension)) continue;
                        var rotatedDimension = (RotatedDimension)obj;
                        var point3dCollections = new Point3dCollection();
                        rotatedDimension.GetStretchPoints(point3dCollections);
                        var item = point3dCollections[2];
                        var item1 = point3dCollections[3];
                        if (point3d.DistanceTo(item) < equalDistTolerance)
                        {
                            point3d = item1;
                            flag = true;
                            rotatedDimensions.Add(rotatedDimension);
                            originDimsColl.Remove(rotatedDimension.Id);
                            break;
                        }

                        if (point3d.DistanceTo(item1) < equalDistTolerance)
                        {
                            point3d = item;
                            flag = true;
                            rotatedDimensions.Add(rotatedDimension);
                            originDimsColl.Remove(rotatedDimension.Id);
                            break;
                        }

                        if (point3d1.DistanceTo(item1) >= equalDistTolerance)
                        {
                            if (point3d1.DistanceTo(item) >= equalDistTolerance) continue;
                            point3d1 = item1;
                            flag = true;
                            rotatedDimensions.Add(rotatedDimension);
                            originDimsColl.Remove(rotatedDimension.Id);
                            break;
                        }

                        point3d1 = item;
                        flag = true;
                        rotatedDimensions.Add(rotatedDimension);
                        originDimsColl.Remove(rotatedDimension.Id);
                        break;
                    }
                } while (flag);
            }

            return rotatedDimensions;
        }

        private static ObjectIdCollection ZGetDimsOnSameLine(ObjectIdCollection dimsColl, Point3d dimLineStartPnt,
            Point3d dimLineEndPnt, double equalDistTolerance)
        {
            var database = Application.DocumentManager.MdiActiveDocument.Database;
            var line3d = new Line3d(dimLineStartPnt, dimLineEndPnt);
            var objectIdCollections = new ObjectIdCollection();
            var topTransaction = database.TransactionManager.TopTransaction;
            using (topTransaction)
            {
                foreach (ObjectId objectId in dimsColl)
                {
                    var obj = (Entity)topTransaction.GetObject(objectId, OpenMode.ForRead);
                    if (!(obj is RotatedDimension)) continue;
                    var rotatedDimension = (RotatedDimension)obj;
                    if (line3d.GetDistanceTo(rotatedDimension.DimLinePoint) >= equalDistTolerance) continue;
                    objectIdCollections.Add(rotatedDimension.Id);
                }
            }

            return objectIdCollections;
        }

        private static ObjectIdCollection ZGetDimsWithSameAngle(double angle, ObjectId dimSpaceId, Vector3d dimNormal,
            ObjectId masterDimId, ObjectId masterDimLayerId, double angleTolerance)
        {
            var database = Application.DocumentManager.MdiActiveDocument.Database;
            var objectIdCollections = new ObjectIdCollection();
            var topTransaction = database.TransactionManager.TopTransaction;
            using (topTransaction)
            {
                var obj = topTransaction.GetObject(dimSpaceId, OpenMode.ForRead) as BlockTableRecord;
                var num = angle + 3.14159265358979;
                var num1 = angle - 3.14159265358979;
                if (obj != null)
                    foreach (var objectId in obj)
                    {
                        var entity = (Entity)topTransaction.GetObject(objectId, OpenMode.ForRead);
                        if (!(entity is RotatedDimension) || !(entity.LayerId == masterDimLayerId)) continue;
                        var rotatedDimension = (RotatedDimension)entity;
                        if (Math.Abs(rotatedDimension.Rotation - angle) >= angleTolerance &&
                            Math.Abs(rotatedDimension.Rotation - num) >= angleTolerance &&
                            Math.Abs(rotatedDimension.Rotation - num1) >= angleTolerance ||
                            !(rotatedDimension.Normal == dimNormal) || !(rotatedDimension.Id != masterDimId)) continue;
                        objectIdCollections.Add(rotatedDimension.Id);
                    }
            }

            return objectIdCollections;
        }

        public static int[] ViewportNumbers()
        {
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            Database workingDatabase = HostApplicationServices.WorkingDatabase;
            if (workingDatabase.TileMode)
            {
                return new int[0];
            }
            IList<int> nums = new List<int>();
            Transaction transaction = workingDatabase.TransactionManager.StartTransaction();
            using (transaction)
            {
                Viewport obj = transaction.GetObject(editor.ActiveViewportId, OpenMode.ForRead) as Viewport;
                if (!(obj != null) || obj.Number != 1)
                {
                    foreach (ObjectId viewport in workingDatabase.GetViewports(false))
                    {
                        obj = (Viewport)transaction.GetObject(viewport, OpenMode.ForRead);
                        nums.Add(obj.Number);
                    }
                }
                else
                {
                    nums.Add(1);
                }
                transaction.Commit();
            }
            int[] numArray = new int[nums.Count];
            nums.CopyTo(numArray, 0);
            return numArray;
        }

        public static Point3d zGetIntersection(DimSystem _dimSet, List<DimPoint> _dimSetPnts, int _index,
            Point3d fencePoint1, Point3d fencePoint2, double equalPoints)
        {
            var plane = Matrix3d.WorldToPlane(_dimSet.SysList[0].Normal);
            var point3d = fencePoint1.TransformBy(plane);
            var point2d = new Point2d(point3d.X, point3d.Y);
            var point3d1 = fencePoint2.TransformBy(plane);
            var point2d1 = new Point2d(point3d1.X, point3d1.Y);
            var line2d = new Line2d(point2d, point2d1);
            var item = _dimSetPnts[_index];
            var dimLinePoint = item.DimLinePoint;
            var point3d2 = new Point3d();
            point3d2 = item.Dim1PointIndex != 1 ? item.Dim1.XLine2Point : item.Dim1.XLine1Point;
            var point3d3 = dimLinePoint.TransformBy(plane);
            var point2d2 = new Point2d(point3d3.X, point3d3.Y);
            var point3d4 = point3d2.TransformBy(plane);
            var point2d3 = new Point2d(point3d4.X, point3d4.Y);
            if (Math.Abs(point2d2.GetDistanceTo(point2d3)) < equalPoints) return new Point3d(-99999, -99999, -99999);
            var line2d1 = new Line2d(point2d2, point2d3);
            var point2d4 = line2d.IntersectWith(line2d1)[0];
            var point3d5 = new Point3d(point2d4.X, point2d4.Y, 0);
            return point3d5.TransformBy(plane.Inverse());
        }

        public static Point3d[] zGetPointOnDimSet(Point3d pnt1, Point3d pnt2, Point3d _newPoint)
        {
            var line3d = new Line3d(pnt1, pnt2);
            var point = line3d.GetClosestPointTo(_newPoint).Point;
            var num = pnt1.DistanceTo(pnt2);
            Point3d point3d;
            if (pnt1.DistanceTo(_newPoint) > num || pnt2.DistanceTo(_newPoint) > num)
                point3d = pnt1.DistanceTo(_newPoint) >= pnt2.DistanceTo(_newPoint) ? pnt2 : pnt1;
            else
                point3d = point;
            return new[] { point3d, point };
        }
    }

}