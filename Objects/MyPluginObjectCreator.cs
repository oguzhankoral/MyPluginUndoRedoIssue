using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPluginUndoRedoIssue.Objects
{
    public static class MyPluginObjectCreator
    {
        /// <summary>
        /// Creates from selected curves
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static List<MyPluginObject> CreateDefault(List<Curve> curves, MyPluginObjectParameters parameters)
        {
            List<MyPluginObject> objects = new List<MyPluginObject>();
            foreach (var curve in curves)
            {
                objects.Add(CreateFromCurve(curve, parameters));
            }
            return objects;
        }

        /// <summary>
        /// Creates as default
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static MyPluginObject CreateDefault(MyPluginObjectParameters parameters)
        {
            Point3d pt0 = new Point3d(5, 5, 0);
            Point3d pt1 = new Point3d(-5, -5, 0);

            Rectangle3d rectangle = new Rectangle3d(new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1)), pt0, pt1);
            NurbsCurve nurbsCurve = rectangle.ToNurbsCurve();
            Curve curve = nurbsCurve.DuplicateCurve();
            return CreateFromCurve(curve, parameters);
        }

        private static MyPluginObject CreateFromCurve(Curve curve, MyPluginObjectParameters defaultParameters)
        {
            MyPluginGeometryObject geoObj = new MyPluginGeometryObject(curve, defaultParameters);
            return new MyPluginObject(geoObj, defaultParameters);
        }
    }
}
