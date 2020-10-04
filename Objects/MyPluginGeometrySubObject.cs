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
    public class MyPluginGeometrySubObject
    {
        #region Rhino Geometry Related Fields
        public RhinoObject RhinoObject { get; private set; }

        public Point3d Center => ClosedBrep.GetBoundingBox(true).Center;

        /// <summary>
        /// BorderCurve uses to extrude brep to create subObj
        /// </summary>
        private Curve BorderCurve;

        public List<Brep> FaceBreps { get; private set; }

        /// <summary>
        /// Brep creates after initializing object by using BorderCurve and Height that coming from Parameters
        /// Uses for transform and take Guid from Rhino Document by adding to Document
        /// </summary>
        private Brep ClosedBrep;
        #endregion Rhino Related Fields

        #region Rhino Document related properties
        /// <summary>
        /// Holds the active document
        /// </summary>
        private RhinoDoc Doc => RhinoDoc.ActiveDoc;

        public bool IsDeleted // { get; private set; } = true;
        {
            get
            {
                if (RhinoObject == null)
                    return true;
                else
                    return RhinoObject.IsDeleted;
            }
        }

        /// <summary>
        /// Holds the Guid that belongs to Brep in the Rhino Document
        /// </summary>
        public Guid Guid { get; set; }

        #endregion Rhino Document related properties

        public double Height => FaceBreps[1].GetArea() / BorderCurve.GetLength();
        private double HeightForCreate { get; set; }

        public double Volume => ClosedBrep.GetVolume();

        public double Area => FaceBreps[0].GetArea();

        public MyPluginGeometrySubObject(Curve borderCurve, double height)
        {
            BorderCurve = borderCurve;
            HeightForCreate = height;
            CreateBrep();
        }

        private void CreateBrep()
        {
            // Create transform 
            Vector3d subObjVector = new Vector3d(0, 0, HeightForCreate);
            Transform moveFirst = Rhino.Geometry.Transform.Translation(subObjVector);

            // Create upper curve
            Curve ceilingCurve = BorderCurve.DuplicateCurve();
            ceilingCurve.Transform(moveFirst);

            List<Brep> breps = new List<Brep>();
            Brep floorBrep = Brep.CreatePlanarBreps(BorderCurve, 0.0001).First();
            breps.Add(floorBrep);

            Surface wallsSurface = Surface.CreateExtrusion(BorderCurve, subObjVector);
            Brep wallBrep = Brep.CreateFromSurface(wallsSurface);
            breps.Add(wallBrep);

            // Create ceiling surface of subObj
            Brep ceilingBrep = Brep.CreatePlanarBreps(ceilingCurve, 0.0001).First();
            breps.Add(ceilingBrep);

            // Join and close breps
            Brep subObj = Brep.JoinBreps(breps, 0.0001).First();
            FaceBreps = breps;
            ClosedBrep = subObj;
        }

        /// <summary>
        /// Add subObj to Rhino Document as Brep
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="attributes"></param>
        public void AddToDoc(RhinoDoc doc, ObjectAttributes attributes)
        {
            Guid = doc.Objects.AddBrep(ClosedBrep, attributes);
            RhinoObject = doc.Objects.FindId(Guid);
        }

        /// <summary>
        /// Remove subObj from Rhino Document
        /// </summary>
        /// <param name="doc"></param>
        public void RemoveFromDoc(RhinoDoc doc)
        {
            doc.Objects.Delete(RhinoObject);
        }
    }
}
