using Eto.Drawing;
using Eto.Forms;
using MyPluginUndoRedoIssue.Controller;
using MyPluginUndoRedoIssue.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyPluginUndoRedoIssue.UI
{
    public class MyPluginTabControl : Panel
    {

        private Button createButton;
        private Label heightLabel;
        private TextBox heightTextBox;
        private Label distanceLabel;
        private TextBox distanceTextBox;
        private Label numberOfObjectLabel;
        private TextBox numberOfObjectTextBox;

        private Label volumeLabel;
        private Label volumeValueLabel;
        private Label areaLabel;
        private Label areaValueLabel;
        private Label totalDistanceLabel;
        private Label totalDistanceValueLabel;

        /// <summary>
        /// Controller manages plugin objects
        /// </summary>
        private MyPluginObjectController Controller { get; set; }

        public MyPluginTabControl(MyPluginObjectController controller)
        {
            Controller = controller;

            Create = new RelayCommand<object>(obj => { CreateCommand(); });

            // initalization
            InitializeComponent();
            InitializeLayout();

            // event registrations
            RegisterSelectionEvents();
            RegisterControlEvents();
        }

        private void InitializeComponent()
        {
            createButton = new Button()
            {
                Text = "Create",
                Command = Create,
            };

            numberOfObjectLabel = new Label()
            {
                Text = "Number Of Object:",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right,
                Size = new Size(100, 20),
            };

            numberOfObjectTextBox = new TextBox()
            {
                Text = "0",
                Enabled = false,
                Size = new Size(100, 20),
            };

            heightLabel = new Label()
            {
                Text = "Height :",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right,
                Size = new Size(100, 20),
            };

            heightTextBox = new TextBox()
            {
                Text = "0",
                Enabled = false,
                Size = new Size(100, 20),
            };

            distanceLabel = new Label()
            {
                Text = "Distance :",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right,
                Size = new Size(100, 20),
            };

            distanceTextBox = new TextBox()
            {
                Text = "0",
                Enabled = false,
                Size = new Size(100,20),
            };

            volumeLabel = new Label()
            {
                Text = "Volume :",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right,
                Size = new Size(100, 20),
            };

            volumeValueLabel = new Label()
            {
                Text = "0",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Left,
                Size = new Size(100, 20),
            };

            areaLabel = new Label()
            {
                Text = "Area :",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right,
                Size = new Size(100, 20),
            };

            areaValueLabel = new Label()
            {
                Text = "0",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Left,
                Size = new Size(100, 20),
            };

            totalDistanceLabel = new Label()
            {
                Text = "Total Distance :",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right,
                Size = new Size(100, 20),
            };

            totalDistanceValueLabel = new Label()
            {
                Text = "0",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Left,
                Size = new Size(100, 20),
            };
        }

        private void InitializeLayout()
        {
            TableLayout layout = new TableLayout()
            {
                // Padding around the table
                Padding = 5,
                // Spacing between table cells
                Spacing = new Eto.Drawing.Size(15, 15),
                Rows =
                {
                    new TableRow(createButton),
                    new TableRow(numberOfObjectLabel, numberOfObjectTextBox),
                    new TableRow(heightLabel, heightTextBox),
                    new TableRow(distanceLabel, distanceTextBox),
                    new TableRow(volumeLabel, volumeValueLabel),
                    new TableRow(areaLabel, areaValueLabel),
                    new TableRow(totalDistanceLabel, totalDistanceValueLabel),
                }
            };
            Content = layout;
        }

        private void RegisterSelectionEvents()
        {
            Controller.MyPluginObjectSelected += Controller_MyPluginObjectSelected;
            Controller.MyPluginObjectDeselected += Controller_MyPluginObjectDeselected;
        }
        private void UnregisterSelectionEvents()
        {
            Controller.MyPluginObjectSelected -= Controller_MyPluginObjectSelected;
            Controller.MyPluginObjectDeselected -= Controller_MyPluginObjectDeselected;
        }

        private void Controller_MyPluginObjectSelected()
        {
            DisableRhinoIdle();
            UnregisterControlEvents();
            UnregisterSelectionEvents();

            FillTextBoxesFromMyPluginObject();

            RegisterControlEvents();
            RegisterSelectionEvents();
            EnableRhinoIdle();
        }

        private void FillTextBoxesFromMyPluginObject()
        {
            MyPluginObject lastSelected = Controller.LastSelectedMyPluginObject();

            numberOfObjectTextBox.Text = lastSelected.NumberOfObjects.ToString();
            heightTextBox.Text = Math.Round(lastSelected.Height, 2, MidpointRounding.ToEven).ToString();
            distanceTextBox.Text = Math.Round(lastSelected.Distance, 2, MidpointRounding.ToEven).ToString();
            volumeValueLabel.Text = Math.Round(lastSelected.Geometry.Volume, 2, MidpointRounding.ToEven).ToString();
            areaValueLabel.Text = Math.Round(lastSelected.Geometry.Area, 2, MidpointRounding.ToEven).ToString();
            totalDistanceValueLabel.Text = Math.Round(lastSelected.Geometry.TotalDistance, 2, MidpointRounding.ToEven).ToString();

            numberOfObjectTextBox.Enabled = true;
            heightTextBox.Enabled = true;
            distanceTextBox.Enabled = true;
        }

        private void Controller_MyPluginObjectDeselected()
        {
            UnregisterControlEvents();
            UnregisterSelectionEvents();

            MakeZeroTextBoxes();

            RegisterControlEvents();
            RegisterSelectionEvents();
        }

        private void MakeZeroTextBoxes()
        {
            numberOfObjectTextBox.Text = "0";
            heightTextBox.Text = "0";
            distanceTextBox.Text = "0";

            volumeValueLabel.Text = "0";
            areaValueLabel.Text = "0";
            totalDistanceValueLabel.Text = "0";

            numberOfObjectTextBox.Enabled = false;
            heightTextBox.Enabled = false;
            distanceTextBox.Enabled = false;
        }

        private void DisableRhinoIdle()
        {
            Controller.DisableIdleEventHandler();
        }
        private void EnableRhinoIdle()
        {
            Controller.EnableIdleEventHandler();
        }

        private void PreUpdateTextBoxActions()
        {
            DisableRhinoIdle();
            UnregisterControlEvents();
            UnregisterSelectionEvents();
        }
        private void PostUpdateTextBoxActions()
        {
            FillTextBoxesFromMyPluginObject();
            RegisterControlEvents();
            RegisterSelectionEvents();
            EnableRhinoIdle();
        }

        private void UpdateNumberOfObjectsTextBox()
        {
            PreUpdateTextBoxActions();
            int newValue = int.Parse(numberOfObjectTextBox.Text);
            Controller.UpdateSelectedObjectsNumberOfObjects(newValue);
            PostUpdateTextBoxActions();
        }

        private void UpdateHeightTextBox()
        {
            PreUpdateTextBoxActions();
            double newValue = double.Parse(heightTextBox.Text);
            Controller.UpdateSelectedObjectsHeight(newValue);
            PostUpdateTextBoxActions();
        }
        
        private void UpdateDistanceTextBox()
        {
            PreUpdateTextBoxActions();
            double newValue = double.Parse(distanceTextBox.Text);
            Controller.UpdateSelectedObjectsDistance(newValue);
            PostUpdateTextBoxActions();
        }


        private void NumberOfObjectTextBox_LostFocus(object sender, EventArgs e)
        {
            if (!FlagForGotFocusAndLostFocusEvent)
            {
                UpdateNumberOfObjectsTextBox();
                FlagForGotFocusAndLostFocusEvent = false;
            }
            EnableRhinoIdle();
        }
        private void NumberOfObjectTextBox_GotFocus(object sender, EventArgs e)
        {
            DisableRhinoIdle();
            FlagForGotFocusAndLostFocusEvent = true;
        }
        private void NumberOfObjectTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Enter || e.Key == Keys.Tab)
            {
                UpdateNumberOfObjectsTextBox();
                FlagForGotFocusAndLostFocusEvent = false;
            }
        }


        private void DistanceTextBox_LostFocus(object sender, EventArgs e)
        {
            if (!FlagForGotFocusAndLostFocusEvent)
            {
                UpdateDistanceTextBox();
                FlagForGotFocusAndLostFocusEvent = false;
            }
            EnableRhinoIdle();
        }
        private void DistanceTextBox_GotFocus(object sender, EventArgs e)
        {
            DisableRhinoIdle();
            FlagForGotFocusAndLostFocusEvent = true;
        }
        private void DistanceTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Enter || e.Key == Keys.Tab)
            {
                UpdateDistanceTextBox();
                FlagForGotFocusAndLostFocusEvent = false;
            }
        }


        private void HeightTextBox_LostFocus(object sender, EventArgs e)
        {
            if (!FlagForGotFocusAndLostFocusEvent)
            {
                UpdateHeightTextBox();
                FlagForGotFocusAndLostFocusEvent = false;
            }
            EnableRhinoIdle();
        }
        private void HeightTextBox_GotFocus(object sender, EventArgs e)
        {
            DisableRhinoIdle();
            FlagForGotFocusAndLostFocusEvent = true;
        }     
        private void HeightTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Enter || e.Key == Keys.Tab)
            {
                UpdateHeightTextBox();
                FlagForGotFocusAndLostFocusEvent = false;
            }
        }

        private void RegisterControlEvents()
        {
            numberOfObjectTextBox.KeyDown += NumberOfObjectTextBox_KeyDown;
            heightTextBox.KeyDown += HeightTextBox_KeyDown;
            distanceTextBox.KeyDown += DistanceTextBox_KeyDown;

            numberOfObjectTextBox.GotFocus += NumberOfObjectTextBox_GotFocus;
            numberOfObjectTextBox.LostFocus += NumberOfObjectTextBox_LostFocus;

            heightTextBox.GotFocus += HeightTextBox_GotFocus;
            heightTextBox.LostFocus += HeightTextBox_LostFocus;

            distanceTextBox.GotFocus += DistanceTextBox_GotFocus;
            distanceTextBox.LostFocus += DistanceTextBox_LostFocus;
        }

        private void UnregisterControlEvents()
        {
            numberOfObjectTextBox.KeyDown -= NumberOfObjectTextBox_KeyDown;
            heightTextBox.KeyDown -= HeightTextBox_KeyDown;
            distanceTextBox.KeyDown -= DistanceTextBox_KeyDown;

            numberOfObjectTextBox.GotFocus -= NumberOfObjectTextBox_GotFocus;
            numberOfObjectTextBox.LostFocus -= NumberOfObjectTextBox_LostFocus;

            heightTextBox.GotFocus -= HeightTextBox_GotFocus;
            heightTextBox.LostFocus -= HeightTextBox_LostFocus;

            distanceTextBox.GotFocus -= DistanceTextBox_GotFocus;
            distanceTextBox.LostFocus -= DistanceTextBox_LostFocus;
        }


        private void CreateCommand()
        {
            Controller.CreateMyPluginObject();
        }

        public ICommand Create { get; }
        public bool FlagForGotFocusAndLostFocusEvent { get; private set; }
    }
}
