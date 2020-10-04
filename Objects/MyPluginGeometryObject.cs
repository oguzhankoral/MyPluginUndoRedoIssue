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
    public class MyPluginGeometryObject
    {
        public Curve BorderCurve { get; set; }
        public List<Curve> Curves { get; set; }
        public MyPluginObjectParameters Parameters { get; set; }

        /// <summary>
        /// ObjectAttributes keep the data that related to building and document
        /// </summary>
        private ObjectAttributes Attributes { get; set; }
        private RhinoDoc Doc => RhinoDoc.ActiveDoc;

        public bool IsDeleted { get; internal set; }
        public Guid GroupGuid { get; private set; }
        public int GroupIndex { get; private set; }

        private List<MyPluginGeometrySubObject> SubObjects { get; set; }
        public List<Guid> SubObjGuids { get; private set; }

        public int NumberOfObjects
        {
            get => SubObjects.Count;
            set
            {
                Parameters.NumberOfObjects = value;
                CreateSubObjects();
            }
        }
        public double Height
        {
            get => SubObjects[0].Height;
            set
            {
                Parameters.Height = value;
                CreateSubObjects();
            }
        }

        public double Distance
        {
            get => new Line(SubObjects[0].Center, SubObjects[1].Center).Length;
            set
            {
                Parameters.Distance = value;
                CreateSubObjects();
            }
        }

        public double Volume => SubObjects.Sum(s => s.Volume);
        public double Area => SubObjects.Sum(s => s.Area);
        public double TotalDistance => Distance * NumberOfObjects;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="parameters"></param>
        public MyPluginGeometryObject(Curve curve, MyPluginObjectParameters parameters)
        {
            BorderCurve = curve;
            Parameters = parameters;
            CreateSubObjects();
        }

        private void CreateSubObjects()
        {
            var subObjects = new List<MyPluginGeometrySubObject>();
            for (int i = 0; i < Parameters.NumberOfObjects; i++)
            {
                var move = new Vector3d(0, Parameters.Distance * i, 0);
                Rhino.Geometry.Transform translation = Rhino.Geometry.Transform.Translation(move);
                Curve borderCurve = BorderCurve.DuplicateCurve();
                borderCurve.Transform(translation);
                var subObj = new MyPluginGeometrySubObject(borderCurve, Parameters.Height);
                subObjects.Add(subObj);
            }
            SubObjects = subObjects;
        }

        internal MyPluginGeometryObject Duplicate()
        {
            Curve duplicatedCurve = BorderCurve.DuplicateCurve();
            return new MyPluginGeometryObject(duplicatedCurve, Parameters);
        }

        public void AddToDocAndSelect()
        {
            AddToDoc();
            Select();
        }

        public void AddToDoc()
        {
            Attributes = Doc.CreateDefaultAttributes();
            // Hide isocurves
            Attributes.WireDensity = -1;
            // Name of the object in rhino document
            Attributes.Name = "MyPluginObject";
            // Set to UserDictionary
            Attributes.UserDictionary.Set("MyPluginObject", true);
            // Add group with "MyPluginObject" name
            var group_index = Doc.Groups.Add("MyPluginObject");
            Attributes.AddToGroup(group_index);
            GroupGuid = RhinoDoc.ActiveDoc.Groups.FindIndex(group_index).Id;
            GroupIndex = group_index;
            Attributes.UserDictionary.Set("GroupIndex", GroupIndex);

            List<Guid> guidsToDelete = new List<Guid>();
            foreach (MyPluginGeometrySubObject subObj in SubObjects)
            {
                subObj.AddToDoc(Doc, Attributes);
                guidsToDelete.Add(subObj.Guid);
            }
            SubObjGuids = guidsToDelete;
        }

        private void Select()
        {
            Doc.Objects.Select(SubObjGuids);
        }

        /// <summary>
        /// Remove subObjects from Rhino Document
        /// </summary>
        /// <param name="doc"></param>
        public void RemoveFromDoc()
        {
            foreach (MyPluginGeometrySubObject subObj in SubObjects)
            {
                Doc.Objects.Delete(subObj.RhinoObject);
            }
        }


    }
}
