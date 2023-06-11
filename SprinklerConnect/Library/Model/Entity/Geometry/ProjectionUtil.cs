﻿#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;
#endregion

namespace Utility
{
    public static class ProjectionUtil
    {
        /// <summary>
        /// Lấy điểm chiếu của một điểm lên đường thẳng
        /// </summary>
        /// <param name="line">Đường thẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static XYZ GetProjectPoint(this Line line, XYZ point)
        {
            if (line.IsBound)
            {
                if (line.IsPointInLineOrExtend(point)) return point;
            }
            XYZ vecL = line.Direction;
            var origin = line.Origin;
            XYZ vecP = point - origin;
            Plane p = Plane.CreateByOriginAndBasis
                (origin, vecL.Normalize(), vecL.CrossProduct(vecP).Normalize());
            return GetProjectPoint(p, point);
        }

        /// <summary>
        /// Lấy điểm chiếu của một điểm lên đường thẳng
        /// </summary>
        /// <param name="line">Đường thẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static XYZ GetProjectPoint(this Curve line, XYZ point)
        {
            if (line.Convert2Line().IsPointInLineOrExtend(point)) return point;
            XYZ vecL = line.GetEndPoint(1) - line.GetEndPoint(0);
            XYZ vecP = point - line.GetEndPoint(0);
            Plane p = Plane.CreateByOriginAndBasis
                (line.GetEndPoint(0), vecL.Normalize(), vecL.CrossProduct(vecP).Normalize());
            return GetProjectPoint(p, point);
        }

        /// <summary>
        /// Lấy điểm chiếu của một điểm lên mặt phẳng
        /// </summary>
        /// <param name="plane">Mặt phẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static XYZ GetProjectPoint(this Plane plane, XYZ point)
        {
            double d = plane.GetSignedDistance(point);
            XYZ q = point + plane.Normal * d;
            return plane.IsPointInPlane(q) ? q : point + plane.Normal * -d;
        }

        /// <summary>
        /// Lấy điểm chiếu của một điểm lên mặt phẳng
        /// </summary>
        /// <param name="f">Mặt đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static XYZ GetProjectPoint(this PlanarFace f, XYZ point)
        {
            Plane p = f.GetPlane();
            return GetProjectPoint(p, point);
        }

        public static double GetDistance(this Line line, XYZ pnt)
        {
            return (line.GetProjectPoint(pnt) - pnt).GetLength();
        }

        public static double GetDistance(this Plane plane, XYZ pnt)
        {
            var vecFromPnt2Plane = plane.GetProjectPoint(pnt) - pnt;
            var normal = plane.Normal;

            var dirFactor = 1;
            if (normal.IsSameDirection(vecFromPnt2Plane))
            {
                dirFactor = -1;
            }

            return vecFromPnt2Plane.GetLength() * dirFactor;
        }

        public static double GetAbsDistance(this Plane plane, XYZ pnt)
        {
            return Math.Abs(plane.GetDistance(pnt));
        }

        public static double GetDistance(this PlanarFace planarFace, XYZ pnt)
        {
            return planarFace.GetPlane().GetDistance(pnt);
        }

        public static double GetAbsDistance(this PlanarFace planarFace, XYZ pnt)
        {
            return Math.Abs(planarFace.GetDistance(pnt));
        }

        /// <summary>
        /// Lấy đường thẳng chiếu của một đường thẳng lên mặt phẳng
        /// </summary>
        /// <param name="plane">Mặt phẳng đang xét</param>
        /// <param name="c">Đường thẳng đang xét</param>
        /// <returns></returns>
        public static Curve GetProjectLine(this Plane plane, Curve c)
        {
            return Line.CreateBound(GetProjectPoint(plane, c.GetEndPoint(0)), GetProjectPoint(plane, c.GetEndPoint(1)));
        }

        /// <summary>
        /// "Purge" một List<Curve> được lấy từ các cạnh định nghĩa một mặt trong Revit nhưng gặp một số lỗi phần mềm
        /// </summary>
        /// <param name="cs">List<Curve> đang xét</param>
        /// <returns></returns>
        public static List<Curve> Purge(this List<Curve> cs)
        {
            bool check = true;
            List<Curve> cs1 = new List<Curve>();
            for (int i = 0; i < cs.Count; i++)
            {
                double l = (cs[i].GetEndPoint(0) - cs[i].GetEndPoint(1)).GetLength();
                if (!l.IsBigger(10.0.milimeter2Feet()))
                {
                    check = false;
                    if (i == 0)
                    {
                        cs1.Add(Line.CreateBound(cs[i].GetEndPoint(0), cs[i + 1].GetEndPoint(1)));
                        i = i + 1;
                    }
                    else
                    {
                        cs1[cs1.Count - 1] =
                            Line.CreateBound(cs[i - 1].GetEndPoint(0), cs[i].GetEndPoint(1));
                    }
                }
                else
                {
                    cs1.Add(cs[i]);
                }
            }
            if (check)
            {
                return cs1;
            }
            else
            {
                return Purge(cs1);
            }
        }

        /// <summary>
        /// "Purge" một List<Curve> được lấy từ các cạnh định nghĩa một mặt trong Revit nhưng gặp một số lỗi phần mềm
        /// </summary>
        /// <param name="cl">List<Curve> đang xét</param>
        /// <returns></returns>
        public static List<Curve> Purge(this CurveLoop cl)
        {
            return cl.ConvertCurveLoopToCurveList().Purge();
        }
    }
}
