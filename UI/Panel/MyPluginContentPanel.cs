using Eto.Forms;
using MyPluginUndoRedoIssue.Controller;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPluginUndoRedoIssue.UI
{
    public class MyPluginContentPanel : Panel
    {
        /// <summary>
        /// Rhino Document
        /// </summary>
        private RhinoDoc Doc { get; set; }

        /// <summary>
        /// Controller manages plugin objects
        /// </summary>
        private MyPluginObjectController Controller { get; set; }

        public MyPluginContentPanel(RhinoDoc doc, MyPluginObjectController controller)
        {
            Doc = doc;
            Controller = controller;
            Content = new MyPluginTabControl(Controller);
        }
    }
}
