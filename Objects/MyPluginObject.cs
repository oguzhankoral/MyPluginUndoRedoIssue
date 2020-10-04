using Modelur.DoubleExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPluginUndoRedoIssue.Objects
{
    public class MyPluginObject
    {
        public static event EventHandler<MyPluginObjectArgs> RecreateEvent;
        public MyPluginGeometryObject Geometry { get; set; }
        public MyPluginObjectParameters Parameters { get; set; }

        public int NumberOfObjects
        {
            get
            {
                return Geometry.NumberOfObjects;
            }
            set
            {
                Geometry.NumberOfObjects = value;
                UpdateParametersToGeometry();
            }
        }

        public double Height
        {
            get
            {
                return Geometry.Height;
            }
            set
            {
                Geometry.Height = value;
                UpdateParametersToGeometry();
            }
        }

        public double Distance
        {
            get
            {
                return Geometry.Distance;
            }
            set
            {
                Geometry.Distance = value;
                UpdateParametersToGeometry();
            }
        }

        private void UpdateParametersToGeometry()
        {
            // Height
            if (!Parameters.Height.AlmostEqual(Geometry.Height))
            {
                Parameters.Height = Geometry.Height;
            }

            // Distance
            if (!Parameters.Distance.AlmostEqual(Geometry.Distance))
            {
                Parameters.Distance = Geometry.Distance;
            }

            // Distance
            if (!(Parameters.NumberOfObjects == Geometry.NumberOfObjects))
            {
                Parameters.NumberOfObjects = Geometry.NumberOfObjects;
            }
        }

        public Guid GroupGuid => Geometry.GroupGuid;

        public MyPluginObject(MyPluginGeometryObject geometry, MyPluginObjectParameters parameters)
        {
            Geometry = geometry;
            Parameters = parameters;

        }

        public void RecreateOnChange(MyPluginObject previousObj)
        {
            MyPluginObjectArgs args = new MyPluginObjectArgs();
            args.OldGuid = GroupGuid;
            args.OldObject = previousObj;
            previousObj.Geometry.RemoveFromDoc();
            Geometry.AddToDocAndSelect();
            args.NewGuid = GroupGuid;
            args.NewObject = this;
            RecreateEvent?.Invoke(this, args);
        }

        internal MyPluginObject Duplicate()
        {
            MyPluginGeometryObject geoObj = Geometry.Duplicate();
            return new MyPluginObject(geoObj, Parameters);
        }
    }

    public class MyPluginObjectArgs
    {
        public Guid OldGuid { get; set; }
        public Guid NewGuid { get; set; }
        public MyPluginObject OldObject { get; set; }
        public MyPluginObject NewObject { get; set; }
    }
}
