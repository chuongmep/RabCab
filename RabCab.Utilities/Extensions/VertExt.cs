using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Geometry;
using RabCab.Calculators;
using RabCab.Settings;
using Exception = System.Exception;

namespace RabCab.Extensions
{
    internal class VertExt : IComparable<VertExt>
    {
        private readonly bool _rightAngle;
        private readonly bool _rightCs;
        private readonly double _vertAngle;
        public Point3d VertPoint;
        public EdgeExt XEdge;
        public EdgeExt YEdge;
        public EdgeExt ZEdge;
        public Vector3d Normal;

        //TODO
        public VertExt(Vertex vtx, BoundaryLoop owner)
        {
            VertPoint = vtx.Point;
            XEdge = new EdgeExt();
            YEdge = new EdgeExt();
            ZEdge = new EdgeExt();
            Normal = new Vector3d();
            _vertAngle = 0.0;
            _rightAngle = false;
            _rightCs = false;

            foreach (var eInfo in GetEdges(vtx, owner))
            {
                var sTang = eInfo.Tangent;

                if (sTang.Length < SettingsUser.TolPoint) continue;

                if (eInfo.OnLoop)
                {
                    if (XEdge.IsNull)
                    {
                        XEdge = eInfo;
                    }
                    else if (XEdge.Length >= eInfo.Length)
                    {
                        YEdge = eInfo;
                    }
                    else
                    {
                        YEdge = XEdge;
                        XEdge = eInfo;
                    }

                    if (!Normal.IsLessThanTol()) continue;

                    Normal = eInfo.Normal;
                }
                else
                {
                    ZEdge = eInfo;
                }
            }

            if (XEdge.Length < SettingsUser.TolPoint) return;

            if (YEdge.Length > SettingsUser.TolPoint)
            {
                _vertAngle = XEdge.Tangent.GetAngleTo(YEdge.Tangent);
                if (_vertAngle <= SettingsInternal.TolVector * 10 ||
                    _vertAngle >= 3.14159265358979 - SettingsInternal.TolVector * 10)
                {
                    YEdge = new EdgeExt();
                }
                else
                {
                    if (SettingsUser.PrioritizeRightAngles)
                        _rightAngle = Math.Abs(_vertAngle - 1.5707963267949) < SettingsInternal.TolVector;

                    if (Normal.IsLessThanTol()) Normal = XEdge.Tangent.CrossProduct(YEdge.Tangent);
                }
            }

            if (ZEdge.Length < SettingsUser.TolPoint) return;

            if (!Normal.IsLessThanTol())
            {
                if (Normal.GetAngleTo(ZEdge.Tangent) > 1.5707963267949) Normal = Normal.Negate();

                if (YEdge.Length > SettingsUser.TolPoint)
                    _rightCs = YEdge.Tangent.GetAngleTo(Normal.CrossProduct(XEdge.Tangent)) <
                               1.5707963267949;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        int IComparable<VertExt>.CompareTo(VertExt other)
        {
            return CompareTo(other);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="vtx"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        private List<EdgeExt> GetEdges(Vertex vtx, BoundaryLoop owner)
        {
            var eList = new List<EdgeExt>();

            try
            {
                foreach (var edge in vtx.Edges)
                {
                    var eInfo = new EdgeExt(edge, vtx, owner);

                    if (eInfo.IsNull) continue;

                    eList.Add(eInfo);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return eList;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(VertExt other)
        {
            var num = _rightAngle.CompareTo(other._rightAngle);

            if (num != 0) return num;

            if (!XEdge.Length.IsEqualSize(other.XEdge.Length)) return XEdge.Length.CompareTo(other.XEdge.Length);

            num = _rightCs.CompareTo(other._rightCs);

            if (num == 0)

                if (Math.Abs(_vertAngle - other._vertAngle) <= SettingsInternal.TolVector)
                    if (Math.Abs(YEdge.Length - other.YEdge.Length) <= SettingsUser.TolPoint)
                        if (Math.Abs(ZEdge.Length - other.ZEdge.Length) <= SettingsUser.TolPoint)
                            return 0;
                        else
                            return ZEdge.Length.CompareTo(other.ZEdge.Length);
                    else
                        return YEdge.Length.CompareTo(other.YEdge.Length);
                else
                    return _vertAngle.CompareTo(other._vertAngle);
            return num;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <returns></returns>
        public Matrix3d LayMatrix()
        {
            Vector3d normal;
            Vector3d vector3D;
            Vector3d normal1;
            Matrix3d matrix3D;
            Vector3d vector;

            if (XEdge.Length < SettingsUser.TolPoint)
            {
                matrix3D = new Matrix3d();
                return matrix3D;
            }

            if (!_rightAngle)
            {
                vector = XEdge.Eaxis.ToVector();
                normal = vector.GetNormal();
            }
            else
            {
                normal = XEdge.Tangent.GetNormal();
            }

            if (!Normal.IsLessThanTol())
            {
                normal1 = Normal.GetNormal();
                vector = normal1.CrossProduct(normal);
                vector3D = vector.GetNormal();
                vector = normal.CrossProduct(vector3D);
                normal1 = vector.GetNormal();
            }
            else
            {
                if (YEdge.Length < SettingsUser.TolPoint)
                {
                    matrix3D = new Matrix3d();
                    return matrix3D;
                }

                vector3D = YEdge.Tangent.GetNormal();
                vector = normal.CrossProduct(YEdge.Tangent);
                normal1 = vector.GetNormal();
                vector = normal1.CrossProduct(normal);
                vector3D = vector.GetNormal();
            }

            return Matrix3d.AlignCoordinateSystem(VertPoint, normal, vector3D, normal1, Point3d.Origin, Vector3d.XAxis,
                Vector3d.YAxis, Vector3d.ZAxis);
        }
    }
}