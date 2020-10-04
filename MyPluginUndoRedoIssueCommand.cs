using System;
using System.Collections.Generic;
using MyPluginUndoRedoIssue.UI;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace MyPluginUndoRedoIssue
{
    public class MyPluginUndoRedoIssueCommand : Command
    {
        public MyPluginUndoRedoIssueCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;

            Panels.RegisterPanel(PlugIn, typeof(MyPluginMainPanel), LOC.STR("oguzhankoral"), null);
        }

        ///<summary>The only instance of this command.</summary>
        public static MyPluginUndoRedoIssueCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "MyPluginUndoRedoIssueCommand"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine("MyPlugin is starting...");
            Panels.OpenPanel(typeof(MyPluginMainPanel).GUID);
            return Result.Success;
        }
    }
}
