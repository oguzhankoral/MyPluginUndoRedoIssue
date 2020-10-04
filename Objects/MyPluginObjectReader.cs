using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPluginUndoRedoIssue.Objects
{
    public static class MyPluginObjectReader
    {
        internal static MyPluginObject ReadFromGeometry(List<Guid> guids, MyPluginObjectParameters parameters, RhinoDoc doc)
        {
            // Find RhinoObject with guid in Rhino Document
            RhinoObject obj = doc.Objects.FindId(guids[0]);
            GeometryBase geo = obj.Geometry;
            Brep brep = (Brep)geo;
            Point3d point1 = brep.GetBoundingBox(true).Center;

            RhinoObject obj2 = doc.Objects.FindId(guids[1]);
            Brep brep2 = (Brep)obj2.Geometry;
            Point3d point2 = brep2.GetBoundingBox(true).Center;

            double distance = new Line(point1, point2).Length;

            // Decompose geometry
            var subObjects = obj.GetSubObjects().AsEnumerable();
            IEnumerable<Brep> subBreps = subObjects.Select(o => (Brep)o.Geometry);

            // filter vertical and horizontal breps by using their sub edges to understand locations
            var verticalBreps = subBreps.Where(b => b.Edges.Any(e => e.PointAtStart.Z != e.PointAtEnd.Z));
            var horizontalBreps = subBreps.Except(verticalBreps);

            // middle height of brep according to Z value of bounding box
            double midLevel = obj.Geometry.GetBoundingBox(true).Center.Z;

            Brep floorBrep = horizontalBreps.Where(b => b.Edges.First().PointAtEnd.Z < midLevel).First();
            Brep ceilingBrep = horizontalBreps.Where(b => b.Edges.First().PointAtEnd.Z > midLevel).First();
            Brep wallBrep = Brep.JoinBreps(verticalBreps, 0.0001).First();
            
            // Extract every storeys's border curve
            Curve borderCurve = GetBorderCurveFromRhinoObject(floorBrep);

            double height = Math.Round(wallBrep.GetArea() / borderCurve.GetLength(), 2, MidpointRounding.ToEven);
            

            parameters.Distance = distance;
            parameters.Height = height;
            MyPluginGeometryObject geoObj = new MyPluginGeometryObject(borderCurve.DuplicateCurve(), parameters);

            return new MyPluginObject(geoObj, parameters);
        }

        private static Curve GetBorderCurveFromRhinoObject(Brep floor)
        {
            var curves = new List<Curve>();
            foreach (var edge in floor.Edges)
            {
                // Find only the naked edges 
                if (edge.Valence == EdgeAdjacency.Naked)
                {
                    var crv = edge.DuplicateCurve();
                    if (crv.IsLinear())
                        crv = new LineCurve(crv.PointAtStart, crv.PointAtEnd);
                    curves.Add(crv);
                }
            }

            var tol = 2.1 * RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            var borderCurve = Curve.JoinCurves(curves, tol);
            return borderCurve[0];
        }
    }
}
