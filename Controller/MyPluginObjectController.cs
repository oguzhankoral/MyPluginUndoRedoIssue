using MyPluginUndoRedoIssue.Objects;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.PlugIns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyPluginUndoRedoIssue.Events.MyPluginEvents;

namespace MyPluginUndoRedoIssue.Controller
{
    public class MyPluginObjectController
    {
        /// <summary>
        /// Keeps document related properties in active document
        /// </summary>
        private RhinoDoc Doc => RhinoDoc.ActiveDoc;

        public event MyPluginObjectSelectionEvents MyPluginObjectSelected;
        public event MyPluginObjectSelectionEvents MyPluginObjectDeselected;


        private GroupHelper? replaceGroupHelper;
        private GroupHelper? copyGroupHelper;
        private GroupHelper ReplaceGroupHelper => replaceGroupHelper ??= new GroupHelper();
        private GroupHelper CopyGroupHelper => copyGroupHelper ??= new GroupHelper();

        /// <summary>
        /// Keeps all objects
        /// </summary>
        public Dictionary<Guid, MyPluginObject> AllObjects { get; private set; } = new Dictionary<Guid, MyPluginObject>();
        public IEnumerable<MyPluginObject> ExistingObjects => AllObjects.Values.Where(obj => !obj.Geometry.IsDeleted).AsEnumerable();
        public IEnumerable<MyPluginObject> RemovedObjects => AllObjects.Values.Where(obj => obj.Geometry.IsDeleted).AsEnumerable();
        public List<MyPluginObject> SelectedObjects { get; private set; } = new List<MyPluginObject>();

        /// <summary>
        /// Contains selected and sieved curves in the Rhino Document
        /// </summary>
        private Dictionary<Guid, Curve> SelectedCurves { get; set; } = new Dictionary<Guid, Curve>();

        /// <summary>
        /// Default parameters for creating new buildings.
        /// </summary>
        private MyPluginObjectParameters DefaultParameters { get; set; }
        public bool CopyFlag { get; private set; }

        public MyPluginObjectController(MyPluginObjectParameters defaultParameters)
        {
            DefaultParameters = defaultParameters;
            HookRhinoEvents();
            HookPluginEvents();
            EnableIdleEventHandler();
        }

        /// <summary>
        /// Hook Rhino.Idle event
        /// </summary>
        public void EnableIdleEventHandler()
        {
            if (null == Idle)
                RhinoApp.Idle += Idle = OnIdle;
        }

        private void OnIdle(object sender, EventArgs e)
        {
            // Unhook the idle handler
            DisableIdleEventHandler();

            #region Group Helper

            // Check there is a replaced object or not
            if (ReplaceGroupHelper.ObjectsAsArray.Length > 0)
            {
                var intersectedGroupIndices = ReplaceGroupHelper.GroupIndices.Intersect(AllObjects.Values.Select(b => b.Geometry.GroupIndex));
                List<MyPluginObject> replacedObjects = AllObjects.Values.Where(b => intersectedGroupIndices.Contains(b.Geometry.GroupIndex)).ToList();

                if (replacedObjects.Count != 0)
                {
                    UpdateMyPluginObjectAfterReplacing(replacedObjects);
                }
            }
            // Check there is a copied object or not
            if (CopyGroupHelper.ObjectsAsList.Count > 0)
            {
                var intersectedGroupIndices = CopyGroupHelper.GroupIndices.Intersect(AllObjects.Values.Select(b => b.Geometry.GroupIndex));
                List<MyPluginObject> copiedObjects = AllObjects.Values.Where(b => intersectedGroupIndices.Contains(b.Geometry.GroupIndex)).ToList(); ;

                CopyFlag = false;
                if (copiedObjects.Count != 0)
                {
                    UpdateMyPluginObjectAfterReplacing(copiedObjects);
                }
            }

            // Reset the helpers
            ReplaceGroupHelper.Reset();
            CopyGroupHelper.Reset();
            #endregion

            #region Curve Selection
            List<Curve> curves = new List<Curve>();
            Dictionary<Guid, Curve> selectedCurves = new Dictionary<Guid, Curve>();

            List<RhinoObject> selectedRhinoObjectCurves = RhinoDoc.ActiveDoc.Objects.GetObjectsByType<RhinoObject>(new ObjectEnumeratorSettings()
            {
                ObjectTypeFilter = ObjectType.Curve,
                SelectedObjectsFilter = true
            }).ToList();

            foreach (RhinoObject rhinoObject in selectedRhinoObjectCurves)
            {
                Guid guidCurve = rhinoObject.Id;
                Curve curve = (Curve)rhinoObject.Geometry;
                selectedCurves.Add(guidCurve, curve);
            }
            SelectedCurves = selectedCurves;
            #endregion Curve Selectiom

            #region Brep Selection 

            List<RhinoObject> selectedRhinoObjectBreps = RhinoDoc.ActiveDoc.Objects.GetObjectsByType<RhinoObject>(new ObjectEnumeratorSettings()
            {
                ObjectTypeFilter = ObjectType.Brep,
                SelectedObjectsFilter = true
            }).ToList();

            List<Guid> selectedGuids = new List<Guid>();

            foreach (RhinoObject rhinoObject in selectedRhinoObjectBreps)
            {
                if (rhinoObject.GetGroupList() != null)
                {
                    int[] index = rhinoObject.GetGroupList();
                    Guid groupGuid = Doc.Groups.FindIndex(index[0]).Id;
                    selectedGuids.Add(groupGuid);
                }
            }

            // find selected buildings from existing buildings
            var existingObjectGuids = ExistingObjects.Select(b => b.GroupGuid).ToList();
            var intersectedGuids = selectedGuids.Intersect(existingObjectGuids).ToList();
            // This collections should turn list, otherwise BuildingList will give error like
            // 'Collection was modified; enumeration operation may not execute.'
            var selectedObjects = ExistingObjects.Where(b => selectedGuids.Contains(b.GroupGuid)).ToList();

            SelectedObjects = selectedObjects;
            #endregion Brep Selection

            #region Plugin Object Selection Control
            if (SelectedObjects.Count != 0)
            {
                MyPluginObjectSelected?.Invoke();
            }
            else
            {
                MyPluginObjectDeselected?.Invoke();
            }
            #endregion

            //Refresh view
            RhinoDoc.ActiveDoc.Views.Redraw();
            // Hook the idle handler
            EnableIdleEventHandler();
        }

        private void UpdateMyPluginObjectAfterReplacing(List<MyPluginObject> replacedObjects)
        {
            // NOTES FOR DALE
            // TODO: Scale, Move, Rotate etc. reactions jumps here but there is two different undo reaction in Undo Multiple stack and
            // its not manageable for other plugin parts like list to calculate other parameters that is not in this plugin but
            // I have in my original plugin

            // TODO: I simply want to wrap this reactions (Scale, Rotate.. wherever my GroupHelper catch some objects) 
            // with my plugin object reactions in one Undo and Redo
            uint sn = Doc.BeginUndoRecord("Update object after reaction");
            foreach (var replacedObject in replacedObjects)
            {
                var newObject = MyPluginObjectReader.ReadFromGeometry(replacedObject.Geometry.SubObjGuids.ToList(), DefaultParameters, Doc);

                bool verticalScaleControl = Math.Abs(replacedObject.Height - newObject.Height) > 0.0001;
                bool horizontalScaleControl = Math.Abs(replacedObject.Distance - newObject.Distance) > 0.0001;

                // Understand scaling is "Scale1D or Scale2D or Scale"
                bool scale1DControl = verticalScaleControl && !horizontalScaleControl;
                bool scale3DControl = verticalScaleControl && horizontalScaleControl;

                // Update according to scaling type
                if (scale1DControl)
                {
                    ScaleVertical(replacedObject, newObject);
                }
                else if (scale3DControl)
                {
                    ScaleVerticalAndHorizontal(replacedObject, newObject);
                }
                else
                {
                    ScaleHorizontal(replacedObject, newObject);
                }
            }
            Doc.EndUndoRecord(sn);
        }

        private void ScaleHorizontal(MyPluginObject replacedObject, MyPluginObject newObject)
        {
            double oldDistance = replacedObject.Distance;
            double newTotalDistance = newObject.Geometry.TotalDistance;

            int newNumberOfObject = (int)(newTotalDistance / oldDistance);
            
            MyPluginObject duplicated = replacedObject.Duplicate();
            duplicated.NumberOfObjects = newNumberOfObject;
            duplicated.Distance = oldDistance;
            //duplicated.NumberOfObjects = replacedObject.Parameters.NumberOfObjects
            duplicated.RecreateOnChange(replacedObject);
        }

        private void ScaleVerticalAndHorizontal(MyPluginObject replacedObject, MyPluginObject newObject)
        {
            double newDistance = newObject.Distance;
            double newHeight = newObject.Height;
            MyPluginObject duplicated = replacedObject.Duplicate();
            duplicated.Distance = newDistance;
            duplicated.Height = newDistance;
            duplicated.RecreateOnChange(replacedObject);
        }

        private void ScaleVertical(MyPluginObject replacedObject, MyPluginObject newObject)
        {
            double newDistance = newObject.Height;
            MyPluginObject duplicated = replacedObject.Duplicate();
            duplicated.Height = newDistance;
            duplicated.RecreateOnChange(replacedObject);
        }

        /// <summary>
        /// Unhook Rhino.Idle event
        /// </summary>
        public void DisableIdleEventHandler()
        {
            if (null != Idle)
            {
                RhinoApp.Idle -= Idle;
                Idle = null;
            }
        }

        private void HookPluginEvents()
        {
            MyPluginObject.RecreateEvent += MyPluginObject_RecreateEvent;
        }

        private void MyPluginObject_RecreateEvent(object sender, MyPluginObjectArgs e)
        {
            AddObjectsToList(e.NewObject);
        }

        private EventHandler<RhinoReplaceObjectEventArgs>? ReplaceObject;
        private EventHandler? Idle;

        private void HookRhinoEvents()
        {
            if (null == ReplaceObject)
                RhinoDoc.ReplaceRhinoObject += ReplaceObject = OnReplaceRhinoObject;
            RhinoDoc.AddRhinoObject += RhinoDoc_AddRhinoObject;
            RhinoDoc.BeforeTransformObjects += RhinoDoc_BeforeTransformObjects;
        }

        private bool IsMyPluginObject(RhinoObject rhinoObject)
        {
            ObjectAttributes attributes = rhinoObject.Attributes;
            return attributes.UserDictionary.ContainsKey("MyPluginObject");
        }

        private void RhinoDoc_BeforeTransformObjects(object sender, RhinoTransformObjectsEventArgs e)
        {
            if (e.ObjectsWillBeCopied && IsMyPluginObject(e.Objects[0]))
            {
                CopyFlag = true;
            }
        }

        private void RhinoDoc_AddRhinoObject(object sender, RhinoObjectEventArgs e)
        {
            if (e?.TheObject != null && IsMyPluginObject(e.TheObject) && CopyFlag)
            {
                // Add the new object to the helper
                CopyGroupHelper.Add(e.TheObject);
                // Hook up the idle handler
                EnableIdleEventHandler();
            }
        }

        private void UnhookRhinoEvents()
        {
            if (null != ReplaceObject)
            {
                RhinoDoc.ReplaceRhinoObject -= ReplaceObject;
                ReplaceObject = null;
            }
            RhinoDoc.AddRhinoObject -= RhinoDoc_AddRhinoObject;
            RhinoDoc.BeforeTransformObjects -= RhinoDoc_BeforeTransformObjects;
        }

        /// <summary>
        /// RhinoDoc.ReplaceRhinoObject event handler
        /// </summary>
        private void OnReplaceRhinoObject(object sender, RhinoReplaceObjectEventArgs e)
        {
            if (e?.NewRhinoObject != null)
            {
                // Add the new object to the helper
                ReplaceGroupHelper.Add(e.NewRhinoObject);
                // Hook up the idle handler
                EnableIdleEventHandler();
            }
        }

        internal MyPluginObject LastSelectedMyPluginObject()
        {
            return SelectedObjects.Last();
        }

        internal void CreateMyPluginObject()
        {
            uint sn = Doc.BeginUndoRecord("Create My Plugin Object");
            if (SelectedCurves.Count > 0)
            {
                List<MyPluginObject> defaultObjects = MyPluginObjectCreator.CreateDefault(SelectedCurves.Values.ToList(), DefaultParameters);
                foreach (MyPluginObject obj in defaultObjects)
                {
                    obj.Geometry.AddToDocAndSelect();
                    AddObjectsToList(obj);
                }
                SelectedCurves.Clear();
            }
            else
            {
                MyPluginObject defaultObject = MyPluginObjectCreator.CreateDefault(DefaultParameters);
                defaultObject.Geometry.AddToDocAndSelect();
                AddObjectsToList(defaultObject);
            }
            Doc.EndUndoRecord(sn);
        }

        private void AddObjectsToList(MyPluginObject obj)
        {
            AllObjects.Add(obj.GroupGuid, obj);
        }

        internal void UpdateSelectedObjectsNumberOfObjects(int newValue)
        {
            uint sn = Doc.BeginUndoRecord("Update Number Of Objects");
            foreach (var obj in SelectedObjects)
            {
                MyPluginObject newObj = obj.Duplicate();
                newObj.NumberOfObjects = newValue;
                newObj.RecreateOnChange(obj);
            }
            Doc.EndUndoRecord(sn);
        }

        internal void UpdateSelectedObjectsHeight(double newValue)
        {
            uint sn = Doc.BeginUndoRecord("Update Height");
            foreach (var obj in SelectedObjects)
            {
                MyPluginObject newObj = obj.Duplicate();
                newObj.Height = newValue;
                newObj.RecreateOnChange(obj);
            }
            Doc.EndUndoRecord(sn);
        }

        internal void UpdateSelectedObjectsDistance(double newValue)
        {
            uint sn = Doc.BeginUndoRecord("Update Distance");
            foreach (var obj in SelectedObjects)
            {
                MyPluginObject newObj = obj.Duplicate();
                newObj.Distance = newValue;
                newObj.RecreateOnChange(obj);
            }
            Doc.EndUndoRecord(sn);
        }
    }
}
