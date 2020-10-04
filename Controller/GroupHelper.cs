using Rhino;
using Rhino.DocObjects;
using System.Collections.Generic;
using System.Linq;

namespace MyPluginUndoRedoIssue.Controller
{
    internal class GroupHelper
    {
        private uint DocRuntimeSerialNumber;
        public List<int> GroupIndices { get; private set; }

        public Dictionary<RhinoObject, RhinoObject> NewOldObjects { get; private set; }

        public GroupHelper()
        {
            DocRuntimeSerialNumber = 0;
            GroupIndices = new List<int>();
        }

        /// <summary>
        /// Adds a Rhino object's group indices to the list.
        /// </summary>
        public void Add(RhinoObject rhinoObject)
        {
            if (rhinoObject != null)
            {
                DocRuntimeSerialNumber = rhinoObject.Document.RuntimeSerialNumber;
                AddRange(rhinoObject.Attributes.GetGroupList());
            }
        }

        /// <summary>
        /// Adds rhino objects to dictionary with old and new geometries
        /// </summary>
        public void Add(RhinoObject oldRhinoObject, RhinoObject newRhinoObject)
        {
            if (oldRhinoObject != null && newRhinoObject != null)
            {
                DocRuntimeSerialNumber = oldRhinoObject.Document.RuntimeSerialNumber;
                NewOldObjects.Add(newRhinoObject, oldRhinoObject);
            }
        }

        /// <summary>
        /// Adds a group index to the list.
        /// </summary>
        private void Add(int index)
        {
            if (!GroupIndices.Contains(index))
                GroupIndices.Add(index);
        }

        /// <summary>
        /// Adds an enumeration of group indices to the list.
        /// </summary>
        private void AddRange(IEnumerable<int> indices)
        {
            foreach (var index in indices)
                Add(index);
        }

        /// <summary>
        /// Returns the number of groups.
        /// </summary>
        public int Count => GroupIndices.Count;

        /// <summary>
        /// Returns the Rhino objects that are group members.
        /// </summary>
        public List<List<RhinoObject>> ObjectsAsList
        {
            get
            {
                var doc = RhinoDoc.FromRuntimeSerialNumber(DocRuntimeSerialNumber);
                if (doc != null && GroupIndices.Count > 0)
                {
                    var rhinoObjects = new List<List<RhinoObject>>();
                    foreach (var index in GroupIndices)
                    {
                        var innerRhinoObjects = new List<RhinoObject>();
                        var rhinoObjectsFromGroup = doc.Groups.GroupMembers(index);
                        if (rhinoObjectsFromGroup != null)
                        {
                            innerRhinoObjects.AddRange(rhinoObjectsFromGroup);
                        }
                        rhinoObjects.Add(innerRhinoObjects);
                    }
                    return rhinoObjects;
                }
                return new List<List<RhinoObject>>();
            }
        }

        /// <summary>
        /// Returns the Rhino objects as single list
        /// </summary>
        public RhinoObject[] ObjectsAsArray
        {
            get
            {
                var doc = RhinoDoc.FromRuntimeSerialNumber(DocRuntimeSerialNumber);
                if (doc != null && GroupIndices.Count > 0)
                {
                    var rhinoObjects = new List<RhinoObject>();
                    foreach (var index in GroupIndices)
                    {
                        var rhinoObjectsFromGroup = doc.Groups.GroupMembers(index);
                        if (rhinoObjectsFromGroup != null)
                        {
                            rhinoObjects.AddRange(rhinoObjectsFromGroup);
                        }
                    }
                    return rhinoObjects.Distinct().ToArray();
                }
                return new RhinoObject[0];
            }
        }

        /// <summary>
        /// Resets the helper.
        /// </summary>
        public void Reset()
        {
            GroupIndices.Clear();
            DocRuntimeSerialNumber = 0;
        }
    }
}