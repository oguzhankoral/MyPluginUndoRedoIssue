using Rhino;
using Rhino.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using Eto.Forms;
using MyPluginUndoRedoIssue.Controller;
using MyPluginUndoRedoIssue.Objects;

namespace MyPluginUndoRedoIssue.UI
{
    [Guid("00D2DD72-DDE0-46CB-9714-A84F3A01BC70")]
    public class MyPluginMainPanel : Panel, IPanel
    {

        /// <summary>
        /// Returns the ID of this panel.
        /// </summary>
        public static System.Guid PanelId => typeof(MyPluginMainPanel).GUID;

        /// <summary>
        /// Rhino Document number
        /// </summary>
        private uint DocUint { get; set; }

        /// <summary>
        /// Rhino Document
        /// </summary>
        private RhinoDoc Doc => RhinoDoc.FromRuntimeSerialNumber(DocUint);

        public MyPluginObjectParameters Parameters { get; set; }

        public MyPluginMainPanel(uint documentRuntimeSerialNumber)
        {
            DocUint = documentRuntimeSerialNumber;
            // Padding around the main container
            Padding = 6;
            CreateContent();
        }

        private void CreateContent()
        {
            Parameters = DefaultObjects.CreateDefault();
            MyPluginObjectController controller = new MyPluginObjectController(Parameters);
            Content = new MyPluginContentPanel(Doc, controller);
        }

        public void PanelClosing(uint documentSerialNumber, bool onCloseDocument)
        {
            // Called when the document or panel container is closed/destroyed
            // Rhino.RhinoApp.WriteLine($"Panel closing for document {documentSerialNumber}, this serial number {DocUint} should be the same");
        }

        public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason)
        {
            // Called when the panel tab is hidden, in Mac Rhino this will happen
            // for a document panel when a new document becomes active, the previous
            // documents panel will get hidden and the new current panel will get shown.
            // Rhino.RhinoApp.WriteLine($"Panel hidden for document {documentSerialNumber}, this serial number {DocUint} should be the same");
        }

        public void PanelShown(uint documentSerialNumber, ShowPanelReason reason)
        {
            // Called when the panel tab is made visible, in Mac Rhino this will happen
            // for a document panel when a new document becomes active, the previous
            // documents panel will get hidden and the new current panel will get shown.
            // Rhino.RhinoApp.WriteLine($"Panel shown for document {documentSerialNumber}, this serial number {DocUint} should be the same");
        }
    }
}
