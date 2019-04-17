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
        private readonly Point3d _vertPoint;
        private readonly EdgeExt _xEdge;
        private readonly EdgeExt _yEdge;
        private readonly EdgeExt _zEdge;
        private Vector3d _normal;

        //TODO
        public VertExt(Vertex vtx, BoundaryLoop owner)
        {
            _vertPoint = vtx.Point;
            _xEdge = new EdgeExt();
            _yEdge = new EdgeExt();
            _zEdge = new EdgeExt();
            _normal = new Vector3d();
            _vertAngle = 0.0;
            _rightAngle = false;
            _rightCs = false;

            foreach (var eInfo in GetEdges(vtx, owner))
            {
                var sTang = eInfo.Tangent;

                if (sTang.Length < SettingsUser.TolPoint) continue;

                if (eInfo.OnLoop)
                {
                    if (_xEdge.IsNull)
                    {
                        _xEdge = eInfo;
                    }
                    else if (_xEdge.Length >= eInfo.Length)
                    {
                        _yEdge = eInfo;
                    }
                    else
                    {
                        _yEdge = _xEdge;
                        _xEdge = eInfo;
                    }

                    if (!_normal.IsLessThanTol()) continue;

                    _normal = eInfo.Normal;
                }
                else
                {
                    _zEdge = eInfo;
                }
            }

            if (_xEdge.Length < SettingsUser.TolPoint) return;

            if (_yEdge.Length > SettingsUser.TolPoint)
            {
                _vertAngle = _xEdge.Tangent.GetAngleTo(_yEdge.Tangent);
                if (_vertAngle <= SettingsInternal.TolVector * 10 ||
                    _vertAngle >= 3.14159265358979 - SettingsInternal.TolVector * 10)
                {
                    _yEdge = new EdgeExt();
                }
                else
                {
                    if (SettingsUser.PrioritizeRightAngles)
                        _rightAngle = Math.Abs(_vertAngle - 1.5707963267949) < SettingsInternal.TolVector;

                    if (_normal.IsLessThanTol()) _normal = _xEdge.Tangent.CrossProduct(_yEdge.Tangent);
                }
            }

            if (_zEdge.Length < SettingsUser.TolPoint) return;

            if (!_normal.IsLessThanTol())
            {
                if (_normal.GetAngleTo(_zEdge.Tangent) > 1.5707963267949) _normal = _normal.Negate();

                if (_yEdge.Length > SettingsUser.TolPoint)
                    _rightCs = _yEdge.Tangent.GetAngleTo(_normal.CrossProduct(_xEdge.Tangent)) <
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

            if (!_xEdge.Length.IsEqualSize(other._xEdge.Length)) return _xEdge.Length.CompareTo(other._xEdge.Length);

            num = _rightCs.CompareTo(other._rightCs);

            if (num == 0)

                if (Math.Abs(_vertAngle - other._vertAngle) <= SettingsInternal.TolVector)
                    if (Math.Abs(_yEdge.Length - other._yEdge.Length) <= SettingsUser.TolPoint)
                        if (Math.Abs(_zEdge.Length - other._zEdge.Length) <= SettingsUser.TolPoint)
                            return 0;
                        else
                            return _zEdge.Length.CompareTo(other._zEdge.Length);
                    else
                        return _yEdge.Length.CompareTo(other._yEdge.Length);
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

            if (_xEdge.Length < SettingsUser.TolPoint)
            {
                matrix3D = new Matrix3d();
                return matrix3D;
            }

            if (!_rightAngle)
            {
                vector = _xEdge.Eaxis.ToVector();
                normal = vector.GetNormal();
            }
            else
            {
                normal = _xEdge.Tangent.GetNormal();
            }

            if (!this._normal.IsLessThanTol())
            {
                normal1 = this._normal.GetNormal();
                vector = normal1.CrossProduct(normal);
                vector3D = vector.GetNormal();
                vector = normal.CrossProduct(vector3D);
                normal1 = vector.GetNormal();
            }
            else
            {
                if (_yEdge.Length < SettingsUser.TolPoint)
                {
                    matrix3D = new Matrix3d();
                    return matrix3D;
                }

                vector3D = _yEdge.Tangent.GetNormal();
                vector = normal.CrossProduct(_yEdge.Tangent);
                normal1 = vector.GetNormal();
                vector = normal1.CrossProduct(normal);
                vector3D = vector.GetNormal();
            }

            return Matrix3d.AlignCoordinateSystem(_vertPoint, normal, vector3D, normal1, Point3d.Origin, Vector3d.XAxis,
                Vector3d.YAxis, Vector3d.ZAxis);
        }
    }
}