using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RabCab.Calculators;

namespace RabCab.Entities.Annotation
{
    internal class DimSystem
    {
        #region Constructor

        public List<RotatedDimension> SysList;
        private bool Highlighted { get; set; }
        public int Count { get; private set; }

        public DimSystem()
        {
            Count = 0;
            SysList = new List<RotatedDimension>();
            Highlighted = false;
        }

        #endregion

        private void Delete(int index)
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;

            var sysPts = GetSystemPoints(CalcTol.ReturnCurrentTolerance());
            if (sysPts.Count < index) return;

            var pt = sysPts[index];

            using (var acTrans = acCurDb.TransactionManager.TopTransaction)
            {
                if (!pt.IsLast)
                {
                    var dim1 = pt.Dim1;
                    var dim2 = pt.Dim2;
                    if (!dim1.IsWriteEnabled) dim1.UpgradeOpen();
                    if (!dim2.IsWriteEnabled) dim2.UpgradeOpen();

                    var dim1PointIndex = pt.Dim1PointIndex;
                    var dim2PointIndex = pt.Dim2PointIndex;

                    if (dim1PointIndex == 1)
                    {
                        dim1.XLine1Point = dim2PointIndex != 1 ? dim2.XLine1Point : dim2.XLine2Point;
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
                    Count -= 1;
                }
                else
                {
                    var rotatedDimension = pt.Dim1;
                    if (!rotatedDimension.IsWriteEnabled) rotatedDimension.UpgradeOpen();

                    rotatedDimension.Unhighlight();
                    rotatedDimension.Erase();

                    SysList.Remove(rotatedDimension);
                    Count -= 1;
                }

                acTrans.TransactionManager.QueueForGraphicsFlush();
            }
        }

        public void Delete(Point3d fencePoint1, Point3d fencePoint2)
        {
            var eqPoint = CalcTol.ReturnCurrentTolerance();
            var sysPoints = GetSystemPoints(eqPoint);
            if (sysPoints.Count == 0) return;

            var dlPoint = sysPoints[0].DimLinePoint;
            var pt = sysPoints[1].DimLinePoint;
            var wPlane = Matrix3d.WorldToPlane(sysPoints[0].Dim1.Normal);
            var point3d1 = dlPoint.TransformBy(wPlane);
            var point2d = new Point2d(point3d1.X, point3d1.Y);
            var point3d2 = pt.TransformBy(wPlane);
            var point2d1 = new Point2d(point3d2.X, point3d2.Y);
            var point3d3 = fencePoint1.TransformBy(wPlane);
            var point2d2 = new Point2d(point3d3.X, point3d3.Y);
            var point3d4 = fencePoint2.TransformBy(wPlane);
            var point2d3 = new Point2d(point3d4.X, point3d4.Y);
            var line2d = new Line2d(point2d, point2d1);
            var point = line2d.GetClosestPointTo(point2d2).Point;
            var point1 = line2d.GetClosestPointTo(point2d3).Point;
            var distanceTo = point.GetDistanceTo(point1);
            var flag = false;
            do
            {
                if (SysList.Count == 0) return;
                if (flag) sysPoints = GetSystemPoints(eqPoint);
                var num = 0;

                foreach (var sysPoint in sysPoints)
                {
                    var point3d5 = sysPoint.DimLinePoint.TransformBy(wPlane);
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
                        Delete(num);
                        flag = true;
                        break;
                    }
                }

            } while (flag);
        }

        public void Delete(Point3d deletePoint)
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            var dPoints = GetSystemPoints(CalcTol.ReturnCurrentTolerance());
            var nums = new List<double>();
            if (dPoints.Count == 0) return;
            foreach (var sysPoint in dPoints) nums.Add(deletePoint.DistanceTo(sysPoint.DimLinePoint));
            var num = nums.IndexOf(nums.Min());
            var pt = dPoints[num];
           
            using (var topTransaction = acCurDb.TransactionManager.TopTransaction)
            {
                if (!pt.IsLast)
                {
                    var dim1 = pt.Dim1;
                    var dim2 = pt.Dim2;
                    if (!dim1.IsWriteEnabled) dim1.UpgradeOpen();
                    if (!dim2.IsWriteEnabled) dim2.UpgradeOpen();
                    var dim1PointIndex = pt.Dim1PointIndex;
                    var dim2PointIndex = pt.Dim2PointIndex;
                    if (dim1PointIndex == 1)
                    {
                        dim1.XLine1Point = dim2PointIndex != 1 ? dim2.XLine1Point : dim2.XLine2Point;
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
                    Count -= 1;
                    if (Highlighted) dim1.Highlight();
                }
                else
                {
                    var rotatedDimension = pt.Dim1;
                    if (!rotatedDimension.IsWriteEnabled) rotatedDimension.UpgradeOpen();
                    rotatedDimension.Unhighlight();
                    rotatedDimension.Erase();
                    SysList.Remove(rotatedDimension);
                    Count -= 1;
                }

                topTransaction.TransactionManager.QueueForGraphicsFlush();
            }
        }

        public void Extend(int pntIndex, int extendTo, Point3d newPoint, double equalDistTolerance)
        {
            var sysPoints = GetSystemPoints(equalDistTolerance);
            if (sysPoints.Count == 0) return;
            if (sysPoints.Count < pntIndex) return;

            var pt = sysPoints[pntIndex];
            double num = 0;
            double num1 = 0;

            if (!pt.IsLast)
            {
                var dimLinePoint = pt.DimLinePoint;
                var point3d = pt.Dim1PointIndex != 1 ? pt.Dim1.XLine2Point : pt.Dim1.XLine1Point;
                var point3d1 = pt.Dim2PointIndex != 1 ? pt.Dim2.XLine2Point : pt.Dim2.XLine1Point;
                num = point3d.DistanceTo(dimLinePoint);
                num1 = point3d1.DistanceTo(dimLinePoint);
            }

            switch (extendTo)
            {
                case 1 when !pt.IsLast:
                {
                    if (num < num1)
                    {
                        if (!pt.Dim2.IsWriteEnabled) pt.Dim2.UpgradeOpen();
                        if (pt.Dim2PointIndex == 1)
                        {
                            if (pt.Dim1PointIndex == 1)
                            {
                                pt.Dim2.XLine1Point = pt.Dim1.XLine1Point;
                                return;
                            }

                            pt.Dim2.XLine1Point = pt.Dim1.XLine2Point;
                            return;
                        }

                        if (pt.Dim1PointIndex == 1)
                        {
                            pt.Dim2.XLine2Point = pt.Dim1.XLine1Point;
                            return;
                        }

                        pt.Dim2.XLine2Point = pt.Dim1.XLine2Point;
                        return;
                    }

                    if (!pt.Dim1.IsWriteEnabled) pt.Dim1.UpgradeOpen();
                    if (pt.Dim1PointIndex == 1)
                    {
                        if (pt.Dim2PointIndex == 1)
                        {
                            pt.Dim1.XLine1Point = pt.Dim2.XLine1Point;
                            return;
                        }

                        pt.Dim1.XLine1Point = pt.Dim2.XLine2Point;
                        return;
                    }

                    if (pt.Dim2PointIndex == 1)
                    {
                        pt.Dim1.XLine2Point = pt.Dim2.XLine1Point;
                        return;
                    }

                    pt.Dim1.XLine2Point = pt.Dim2.XLine2Point;
                    return;
                }

                case 2 when !pt.IsLast:
                {
                    if (num > num1)
                    {
                        if (!pt.Dim2.IsWriteEnabled) pt.Dim2.UpgradeOpen();
                        if (pt.Dim2PointIndex == 1)
                        {
                            if (pt.Dim1PointIndex == 1)
                            {
                                pt.Dim2.XLine1Point = pt.Dim1.XLine1Point;
                                return;
                            }

                            pt.Dim2.XLine1Point = pt.Dim1.XLine2Point;
                            return;
                        }

                        if (pt.Dim1PointIndex == 1)
                        {
                            pt.Dim2.XLine2Point = pt.Dim1.XLine1Point;
                            return;
                        }

                        pt.Dim2.XLine2Point = pt.Dim1.XLine2Point;
                        return;
                    }

                    if (!pt.Dim1.IsWriteEnabled) pt.Dim1.UpgradeOpen();
                    if (pt.Dim1PointIndex == 1)
                    {
                        if (pt.Dim2PointIndex == 1)
                        {
                            pt.Dim1.XLine1Point = pt.Dim2.XLine1Point;
                            return;
                        }

                        pt.Dim1.XLine1Point = pt.Dim2.XLine2Point;
                        return;
                    }

                    if (pt.Dim2PointIndex == 1)
                    {
                        pt.Dim1.XLine2Point = pt.Dim2.XLine1Point;
                        return;
                    }

                    pt.Dim1.XLine2Point = pt.Dim2.XLine2Point;
                    return;
                }

                case 0:
                {
                    var plane = Matrix3d.WorldToPlane(sysPoints[0].Dim1.Normal);
                    var dimLinePoint1 = pt.DimLinePoint;
                    Point3d point3d2;
                    point3d2 = pt.Dim1PointIndex != 1 ? pt.Dim1.XLine2Point : pt.Dim1.XLine1Point;
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
                    if (!pt.Dim1.IsWriteEnabled) pt.Dim1.UpgradeOpen();
                    if (!pt.IsLast && !pt.Dim2.IsWriteEnabled) pt.Dim2.UpgradeOpen();
                    if (pt.Dim1PointIndex != 1)
                        pt.Dim1.XLine2Point = point3d7;
                    else
                        pt.Dim1.XLine1Point = point3d7;
                    if (!pt.IsLast)
                    {
                        if (pt.Dim2PointIndex == 1)
                        {
                            pt.Dim2.XLine1Point = point3d7;
                            return;
                        }

                        pt.Dim2.XLine2Point = point3d7;
                    }

                    break;
                }
            }
        }

        public int GetNearest(Point3d point, double equalDistTolerance)
        {
            var sysPoints = GetSystemPoints(equalDistTolerance);
            var nums = new List<double>();
            if (sysPoints.Count == 0) return -1;
            foreach (var sysPoint in sysPoints) nums.Add(point.DistanceTo(sysPoint.DimLinePoint));
            return nums.IndexOf(nums.Min());
        }

        public static DimSystem GetSystem(RotatedDimension masterDim, double angleTolerance,
            double equalDistTolerance)
        {
            var database = Application.DocumentManager.MdiActiveDocument.Database;
            var dimSystem = new DimSystem();
            using (database.TransactionManager.TopTransaction)
            {
                var objectIdCollections = GetSameAngleSystem(masterDim.Rotation, masterDim.BlockId, masterDim.Normal,
                    masterDim.Id, masterDim.LayerId, angleTolerance);
                var point3dCollections = new Point3dCollection();
                masterDim.GetStretchPoints(point3dCollections);
                var item = point3dCollections[2];
                var point3d = point3dCollections[3];
                var objectIdCollections1 = GetSameLineSystem(objectIdCollections, item, point3d, equalDistTolerance);
                var eachOther = GetAdjacent(objectIdCollections1, item, point3d, equalDistTolerance);
                eachOther.Add(masterDim);
                dimSystem.SysList = eachOther;
                dimSystem.Count = eachOther.Count;
            }

            return dimSystem;
        }

        public List<SysPoint> GetSystemPoints(double equalDistTolerance)
        {
            var dimSystem = this;
            var sysPoints = new List<SysPoint>();
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
                                    foreach (var sysPoint in sysPoints)
                                        if (sysPoint.Dim1 != listOfDim)
                                        {
                                            if (!(sysPoint.Dim2 == listOfDim) || sysPoint.Dim2PointIndex != 1) continue;
                                            flag2 = false;
                                            flag = true;
                                        }
                                        else
                                        {
                                            if (sysPoint.Dim1PointIndex != 1) continue;
                                            flag2 = false;
                                            flag = true;
                                        }

                                    if (flag2)
                                    {
                                        var sysPoint1 = new SysPoint()
                                        {
                                            Dim1 = listOfDim,
                                            Dim2 = current,
                                            Dim1PointIndex = 1,
                                            Dim2PointIndex = 1,
                                            IsLast = false,
                                            DimLinePoint = item
                                        };
                                        flag = true;
                                        sysPoints.Add(sysPoint1);
                                    }
                                }
                                else if (num1 < equalDistTolerance)
                                {
                                    var flag3 = true;
                                    foreach (var sysPoint2 in sysPoints)
                                        if (sysPoint2.Dim1 != listOfDim)
                                        {
                                            if (!(sysPoint2.Dim2 == listOfDim) || sysPoint2.Dim2PointIndex != 1)
                                                continue;
                                            flag3 = false;
                                            flag = true;
                                        }
                                        else
                                        {
                                            if (sysPoint2.Dim1PointIndex != 1) continue;
                                            flag3 = false;
                                            flag = true;
                                        }

                                    if (flag3)
                                    {
                                        var sysPoint3 = new SysPoint
                                        {
                                            Dim1 = listOfDim,
                                            Dim2 = current,
                                            Dim1PointIndex = 1,
                                            Dim2PointIndex = 2,
                                            IsLast = false,
                                            DimLinePoint = item
                                        };
                                        flag = true;
                                        sysPoints.Add(sysPoint3);
                                    }
                                }

                                if (num2 < equalDistTolerance)
                                {
                                    var flag4 = true;
                                    foreach (var sysPoint4 in sysPoints)
                                        if (sysPoint4.Dim1 != listOfDim)
                                        {
                                            if (!(sysPoint4.Dim2 == listOfDim) || sysPoint4.Dim2PointIndex != 2)
                                                continue;
                                            flag4 = false;
                                            flag1 = true;
                                        }
                                        else
                                        {
                                            if (sysPoint4.Dim1PointIndex != 2) continue;
                                            flag4 = false;
                                            flag1 = true;
                                        }

                                    if (flag4)
                                    {
                                        var sysPoint5 = new SysPoint()
                                        {
                                            Dim1 = listOfDim,
                                            Dim2 = current,
                                            Dim1PointIndex = 2,
                                            Dim2PointIndex = 1,
                                            IsLast = false,
                                            DimLinePoint = point3d
                                        };
                                        flag1 = true;
                                        sysPoints.Add(sysPoint5);
                                    }
                                }

                                if (num3 >= equalDistTolerance) continue;
                                var flag5 = true;
                                foreach (var sysPoint6 in sysPoints)
                                    if (sysPoint6.Dim1 != listOfDim)
                                    {
                                        if (!(sysPoint6.Dim2 == listOfDim) || sysPoint6.Dim2PointIndex != 2) continue;
                                        flag5 = false;
                                        flag1 = true;
                                    }
                                    else
                                    {
                                        if (sysPoint6.Dim1PointIndex != 2) continue;
                                        flag5 = false;
                                        flag1 = true;
                                    }

                                if (!flag5) continue;
                                var sysPoint7 = new SysPoint()
                                {
                                    Dim1 = listOfDim,
                                    Dim2 = current,
                                    Dim1PointIndex = 2,
                                    Dim2PointIndex = 2,
                                    IsLast = false,
                                    DimLinePoint = point3d
                                };
                                flag1 = true;
                                sysPoints.Add(sysPoint7);
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
                    ((IDisposable) enumerator).Dispose();
                }

                if (!flag)
                {
                    var sysPoint8 = new SysPoint()
                    {
                        Dim1 = listOfDim,
                        Dim2 = null,
                        Dim1PointIndex = 1,
                        Dim2PointIndex = -1,
                        IsLast = true,
                        DimLinePoint = item
                    };
                    sysPoints.Add(sysPoint8);
                }

                if (flag1) continue;
                var sysPoint9 = new SysPoint()
                {
                    Dim1 = listOfDim,
                    Dim2 = null,
                    Dim1PointIndex = 2,
                    Dim2PointIndex = -1,
                    IsLast = true,
                    DimLinePoint = point3d
                };
                sysPoints.Add(sysPoint9);
            }

            return sysPoints;
        }

        public List<int> GetSystemByLine(Point3d fencePoint1, Point3d fencePoint2, double equalDistTolerance)
        {
            var nums = new List<int>();
            var sysPoints = GetSystemPoints(equalDistTolerance);
            if (sysPoints.Count == 0) return nums;
            var dimLinePoint = sysPoints[0].DimLinePoint;
            var point3d = sysPoints[1].DimLinePoint;
            var plane = Matrix3d.WorldToPlane(sysPoints[0].Dim1.Normal);
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
            foreach (var sysPoint in sysPoints)
            {
                var point3d5 = sysPoint.DimLinePoint.TransformBy(plane);
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

        public void Insert(Point3d newPoint)
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
                    rotatedDimension = (RotatedDimension) item.Clone();
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
                    rotatedDimension = (RotatedDimension) item.Clone();
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
            Count = Count + 1;
            if (Highlighted)
            {
                topTransaction.TransactionManager.QueueForGraphicsFlush();
                rotatedDimension.Highlight();
                item.Highlight();
            }
        }

        public void MoveSystem(Point3d newPoint, double equalDistTolerance)
        {
            var sysPoints = GetSystemPoints(equalDistTolerance);
            if (sysPoints.Count == 0) return;
            var dimLinePoint = sysPoints[0].DimLinePoint;
            var point3d = sysPoints[1].DimLinePoint;
            var plane = Matrix3d.WorldToPlane(sysPoints[0].Dim1.Normal);
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

        public void GetProps(int pntIndex, bool modifyArrowhead, string arrowheadName, bool modifyExtensionLine,
            bool suppressExtLine, double equalDistTolerance)
        {
            int num;
            int num1;
            int num2;
            var sysPoints = GetSystemPoints(equalDistTolerance);
            if (sysPoints.Count == 0) return;
            if (sysPoints.Count < pntIndex) return;
            var item = sysPoints[pntIndex];
            var objectId = new ObjectId();
            if (modifyArrowhead && !(arrowheadName == ".")) objectId = GetArrowId(arrowheadName);
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

        private static ObjectId GetArrowId(string newArrName)
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
                var obj = (BlockTable) topTransaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                @null = obj[newArrName];
            }

            return @null;
        }

        private static List<RotatedDimension> GetAdjacent(ObjectIdCollection originDimsColl,
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
                        var obj = (Entity) topTransaction.GetObject(objectId, OpenMode.ForRead);
                        if (!(obj is RotatedDimension)) continue;
                        var rotatedDimension = (RotatedDimension) obj;
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

        private static ObjectIdCollection GetSameLineSystem(ObjectIdCollection dimsColl, Point3d dimLineStartPnt,
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
                    var obj = (Entity) topTransaction.GetObject(objectId, OpenMode.ForRead);
                    if (!(obj is RotatedDimension)) continue;
                    var rotatedDimension = (RotatedDimension) obj;
                    if (line3d.GetDistanceTo(rotatedDimension.DimLinePoint) >= equalDistTolerance) continue;
                    objectIdCollections.Add(rotatedDimension.Id);
                }
            }

            return objectIdCollections;
        }

        private static ObjectIdCollection GetSameAngleSystem(double angle, ObjectId dimSpaceId, Vector3d dimNormal,
            ObjectId mainDimId, ObjectId mainLayerId, double angleTolerance)
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            var objectIdCollections = new ObjectIdCollection();
            var topTransaction = acCurDb.TransactionManager.TopTransaction;
            using (topTransaction)
            {
                var obj = topTransaction.GetObject(dimSpaceId, OpenMode.ForRead) as BlockTableRecord;
                var num = angle + Math.PI;
                var num1 = angle - Math.PI;
                if (obj != null)
                    foreach (var objectId in obj)
                    {
                        var entity = (Entity) topTransaction.GetObject(objectId, OpenMode.ForRead);
                        if (!(entity is RotatedDimension) || !(entity.LayerId == mainLayerId)) continue;
                        var rotatedDimension = (RotatedDimension) entity;
                        if (Math.Abs(rotatedDimension.Rotation - angle) >= angleTolerance &&
                            Math.Abs(rotatedDimension.Rotation - num) >= angleTolerance &&
                            Math.Abs(rotatedDimension.Rotation - num1) >= angleTolerance ||
                            !(rotatedDimension.Normal == dimNormal) || !(rotatedDimension.Id != mainDimId)) continue;
                        objectIdCollections.Add(rotatedDimension.Id);
                    }
            }

            return objectIdCollections;
        }

        public static int[] ActiveViewports()
        {
            var acCurEd = Application.DocumentManager.MdiActiveDocument.Editor;
            var acCurDb = HostApplicationServices.WorkingDatabase;
            if (acCurDb.TileMode) return new int[0];
            IList<int> nums = new List<int>();
          
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var obj = acTrans.GetObject(acCurEd.ActiveViewportId, OpenMode.ForRead) as Viewport;
                if (!(obj != null) || obj.Number != 1)
                    foreach (ObjectId viewport in acCurDb.GetViewports(false))
                    {
                        obj = (Viewport) acTrans.GetObject(viewport, OpenMode.ForRead);
                        nums.Add(obj.Number);
                    }
                else
                    nums.Add(1);

                acTrans.Commit();
            }

            return nums.ToArray();
        }

        public static Point3d GetCrossing(DimSystem sys, List<SysPoint> sysPnts, int index,
            Point3d fencePoint1, Point3d fencePoint2, double equalPoints)
        {
            var plane = Matrix3d.WorldToPlane(sys.SysList[0].Normal);
            var point3d = fencePoint1.TransformBy(plane);
            var point2d = new Point2d(point3d.X, point3d.Y);
            var point3d1 = fencePoint2.TransformBy(plane);
            var point2d1 = new Point2d(point3d1.X, point3d1.Y);
            var line2d = new Line2d(point2d, point2d1);
            var item = sysPnts[index];
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

        public static Point3d[] GetSystemPoint(Point3d pnt1, Point3d pnt2, Point3d newPoint)
        {
            var line3d = new Line3d(pnt1, pnt2);
            var point = line3d.GetClosestPointTo(newPoint).Point;
            var num = pnt1.DistanceTo(pnt2);
            Point3d point3d;
            if (pnt1.DistanceTo(newPoint) > num || pnt2.DistanceTo(newPoint) > num)
                point3d = pnt1.DistanceTo(newPoint) >= pnt2.DistanceTo(newPoint) ? pnt2 : pnt1;
            else
                point3d = point;
            return new[] {point3d, point};
        }
    }
}