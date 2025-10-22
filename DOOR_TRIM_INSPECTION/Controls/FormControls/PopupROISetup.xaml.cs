using DOOR_TRIM_INSPECTION.Form;
using DOOR_TRIM_INSPECTION.Class;
using Doortrim_Inspection.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DOOR_TRIM_INSPECTION.Controls.FormControls
{
    /// <summary>
    /// Interaction logic for PopupROISetup.xaml
    /// </summary>
    public partial class PopupROISetup : UserControl
    {
        

        public DetectionROIDetailsUI ROI { get; private set; }
        public new CtrlRecipeWizardROISetup Parent { get; set; }
        private PopUpROISetupTryWindow ROITryWindow = null;
        public Rectangle background = null;
        private DetectionClass SelectedItem = null;
        private Rectangle Corner = null;
        private Rectangle Middle = null;
        private string parameters = null;
        private PopUpROISetupTryWindow ROITryPopup
        {
            get
            {
                if (ROITryWindow == null)
                    ROITryWindow = new PopUpROISetupTryWindow();
                return ROITryWindow;
            }
        }

        public PopupROISetup()
        {
            InitializeComponent();
            //LoadDetectionClasses();
        }

        public void SetPopup(DetectionROIDetailsUI roi, CtrlRecipeWizardROISetup parent)
        {
            ROI = roi;
            Parent = parent;
            LoadData(ROI);
            SizeWindow();
            parameters = ROI.Parameters;
            //buttonTry.Visibility = parent.IsFront 
            //    || ROI.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[0].RuleID
            //    || ROI.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[5].RuleID
            //    || ROI.detection_class_ID == Machine.hmcDBHelper.dETECT_LIST.Items[11].RuleID
            //    || ROI.detection_roi_ID == 0
            //    ? Visibility.Hidden : Visibility.Visible;
            
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> paramOptions = new List<string>();
                foreach (PopUpInputRow pInput in stkParameters.Children.OfType<PopUpInputRow>().ToList())
                {
                    paramOptions.Add(pInput.getInfo());
                }

                string rectParams = string.Join("|", paramOptions);
                var selectedDetectionClass = cboDetectionClass.SelectedItem as DetectionClass;

                var roiToUpdate = Parent.ROIs.FirstOrDefault(roi => roi.roi_ui_counter == ROI.roi_ui_counter);

                if (roiToUpdate != null)
                {
                    roiToUpdate.start_x = ROI.start_x;
                    roiToUpdate.start_y = ROI.start_y;
                    roiToUpdate.end_x = ROI.end_x;
                    roiToUpdate.end_y = ROI.end_y;
                    roiToUpdate.roi_name = txtROIName.Text;
                    roiToUpdate.DetectionClassName = selectedDetectionClass.DetectionClassName.Trim();
                    roiToUpdate.detection_class_ID = selectedDetectionClass.RuleID;
                    roiToUpdate.Parameters = rectParams;
                    roiToUpdate.ALC_CODE = ROI.ALC_CODE != null ? ROI.ALC_CODE.Trim() : null;
                    roiToUpdate.ALC_NAME = ROI.ALC_NAME != null ? ROI.ALC_NAME.Trim() : null;
                    roiToUpdate.group_name = ROI.group_name != null ? ROI.group_name.Trim() : null;
                    roiToUpdate.Use = ROI.Use;
                }
                int DetectionROIID = Machine.hmcDBHelper.SaveDetectionROI(ROI);
                int roiID = Parent.ROIs.FindIndex(roi => roi.roi_ui_counter == ROI.roi_ui_counter);
                if (DetectionROIID != -1)
                {
                    Parent.ROIs[roiID].detection_roi_ID = DetectionROIID;
                }
                Parent.RemovePopup();
                ImageEditorHelper.RemoveResizeHandles(Parent.DrawingCanvas);
                Parent.StopAddEdit();
                Parent.LoadROIsList();
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, ex.Message);
            }
        }

        private void CboDetectionClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboDetectionClass.Items.Count == 0) return;
           SelectedItem = cboDetectionClass.SelectedItem == null ? (DetectionClass)cboDetectionClass.Items.GetItemAt(0) : (DetectionClass)cboDetectionClass.SelectedItem;
            var detectionClass = SelectedItem;
            LoadParameterView(detectionClass);
            SizeWindow();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if(Parent._isEditing == true && Parent._isAddEdit == false)
            {
                ROI.Parameters = parameters;
                Parent.CancelROI();
            }
            else if(Parent._isEditing == true && Parent._isAddEdit == true)
            {
                Parent.RemoveROI();
            }
                

            Parent.LoadROIsList();
            ROI.Stroke = Brushes.Yellow;
        }        

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("정말로 삭제하시겠습니까?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Parent.RemoveROI();
            }
        }

        private void TryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ROITryWindow == null)
                {
                    ROITryWindow = new PopUpROISetupTryWindow();
                    ROITryWindow.Closed += (s, args) => ROITryWindow = null;
                    ROITryWindow.Show();
                }
                else
                {
                    if (ROITryWindow.WindowState == WindowState.Minimized)
                    {
                        ROITryWindow.WindowState = WindowState.Normal;
                    }
                    ROITryWindow.Activate();
                }
                ROITryWindow.SetParam(ROI, Parent);
                ROITryWindow.ParamUpdated += ROITryWindow_ParamUpdated;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
            }
        }

        private void ROITryWindow_ParamUpdated(string paramStr)
        {
            if (ROI != null)
            {
                ROI.Parameters = paramStr;
                LoadData(ROI);
                if(ROITryWindow!=null)
                ROITryWindow.Close();
            }
        }


        #region helper
        private void SizeWindow()
        {
            var detectionClass = SelectedItem;
            if (detectionClass != null)
            {
                List<AlgorithmParamOption> algoParams = GetParameters(detectionClass.Parameters);
                var paramCount = algoParams.Count;
                this.Width = 330;
                this.Height = 175 + 40 * paramCount+10;
                if (detectionClass.Parameters.Contains("alternate")) this.Height += 40;

            }
            else
            {
                this.Width = 330;
                this.Height = 175 + 40 * 2;
            }
            SizeUpdate();
        }

        private void SizeUpdate()
        {
            if (Parent != null)
            {
                var st = Parent.zoomBorder.Scale(Parent.DrawingCanvas);
                var scalex = 1 / st.ScaleX; /*< 1 ? 1 : 1 / st.ScaleX;*/
                var scaley = 1 / st.ScaleY; /*< 1 ? 1 : 1 / st.ScaleY;*/

                double xPosition = Canvas.GetLeft(ROI) + ROI.Width + 10;
                double yPosition = Canvas.GetTop(ROI);

                if (xPosition + this.Width * scalex > Parent.imgDoorTrim.ActualWidth)
                {
                    xPosition = xPosition - ROI.Width - this.Width * scalex - 10;
                }

                if (yPosition + this.Height * scaley > Parent.imgDoorTrim.ActualHeight)
                {
                    yPosition = yPosition - this.Height * scaley - 10;
                }

                Canvas.SetLeft(this, xPosition);
                Canvas.SetTop(this, yPosition);
                Canvas.SetZIndex(this, 9999);

                //Console.WriteLine($"IMG AW {imgDoorTrim.ActualWidth}, P AW {PopUp.ActualWidth}, P X {xPosition}, IMG AH {imgDoorTrim.ActualHeight}, P AH {PopUp.ActualHeight}, P Y {yPosition}");

                ScaleTransform scaleTransform = new ScaleTransform(scalex, scaley);

                if (this is FrameworkElement popupContent)
                {
                    this.LayoutTransform = scaleTransform;
                }
                try
                {
                    Parent.DrawingCanvas.Children.Remove(this);
                    Parent.DrawingCanvas.Children.Add(this);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.ToString()}");
                }
            } 
        }

        private void LoadData(DetectionROIDetailsUI ROI)
        {
            txtROIName.Text = ROI.roi_name;
            if (ROI.detection_class_ID != 0)
            {
                SelectedItem = cboDetectionClass.Items.Cast<DetectionClass>()
                .FirstOrDefault(d => d.RuleID == ROI.detection_class_ID);

                cboDetectionClass.SelectedItem = SelectedItem;

                if (SelectedItem != null)
                {
                    if (ROI.Parameters != null)
                    {
                        SelectedItem.Parameters = ROI.Parameters;
                    }
                    LoadParameterView(SelectedItem);
                }
                int position = ROI.roi_name_location;
                ROINameLocationRectDisplay(position);
            }
            else
            {
                cboDetectionClass.SelectedIndex = 0;
                SelectedItem = (DetectionClass)cboDetectionClass.SelectedItem;
                LoadParameterView(SelectedItem);
                TL.Fill = Brushes.Black;
                T.Fill = Brushes.Black;
                Corner = TL;
                Middle = T;
            }
        }

        public void LoadDetectionClasses()
        {
            List<DetectionClass> detectionClasses = Machine.hmcDBHelper.GetDetectionClasses();
            cboDetectionClass.ItemsSource = null;
            cboDetectionClass.ItemsSource = detectionClasses;
            cboDetectionClass.DisplayMemberPath = "DetectionClassName";
            cboDetectionClass.SelectedValuePath = "DetectionClassID";
                
            if (detectionClasses.Count > 0)
            {
                if (ROI == null)
                    cboDetectionClass.SelectedIndex = 0;
                else
                    cboDetectionClass.SelectedIndex = ROI.detection_class_ID;

                SelectedItem = (DetectionClass)cboDetectionClass.SelectedItem;
                LoadParameterView(SelectedItem);
            }
        }

        private List<AlgorithmParamOption> GetParameters(string paramString)
        {

            List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(paramString);
            if (keyValuePairs == null)
                return null;

            List<AlgorithmParamOption> parameters = new List<AlgorithmParamOption>(); // LOAD FROM XML (Name, ID, Default Value)

            foreach (KeyValuePair<string, string> pair in keyValuePairs)
                parameters.Add(new AlgorithmParamOption(pair.Key.Trim(), pair.Value.Trim()));

            return parameters;

        }

        private void LoadParameterView(DetectionClass selectedItem)
        {
            stkParameters.Children.Clear();
            List<AlgorithmParamOption> algoParams = GetParameters(selectedItem.Parameters);
            foreach (AlgorithmParamOption algoParam in algoParams)
            {
                PopUpInputRow popUpInput = new PopUpInputRow();
                popUpInput.setInfo(algoParam.OptionName, algoParam.Value);
                if (ROI != null)
                    ((App)Application.Current).SelectedROI = new OpenCvSharp.Rect((int)ROI.start_x, (int)ROI.start_y, (int)(ROI.end_x - ROI.start_x), (int)(ROI.end_y - ROI.start_y));
                stkParameters.Children.Add(popUpInput);
            }
        }


        public void TryPopUp()
        {
            Parent.DrawingCanvas.Children.Remove(background);
            //Parent.DrawingCanvas.Children.Remove(ROITryPopup);
            Parent._isZoomPanDisabled = false;
        }



        
        private void TextPositionCorner(object sender, MouseEventArgs e)
        {
            Rectangle r = sender as Rectangle;
            if (r != null)
            {
                TextPositionClearCorner();
                r.Fill = Brushes.Black;
                Corner = r;
                TextPositionSet();
            }
        }

        private void TextPositionClearCorner()
        {
            foreach (var child in gridTextPosition.Children)
            {
                if (child is Rectangle r)
                {
                    if (r.Name == "TL" || r.Name == "TR" || r.Name == "BL" || r.Name == "BR")
                        r.Fill = Brushes.Transparent;
                }
            }
        }


        private void TextPositionMiddle(object sender, MouseEventArgs e)
        {
            Rectangle r = sender as Rectangle;
            if (r != null)
            {
                TextPositionClearMiddle();
                r.Fill = Brushes.Black;
                Middle = r;
                TextPositionSet();
            }
        }

        private void TextPositionClearMiddle()
        {
            foreach (var child in gridTextPosition.Children)
            {
                if (child is Rectangle r)
                {
                    if (r.Name == "T" || r.Name == "B" || r.Name == "L" || r.Name == "R")
                        r.Fill = Brushes.Transparent;
                }
            }
        } 

        private void TextPositionSet()
        {
            if(Corner == TL && Middle == T ) // position 1
            {
                Parent.TextPosition(ROI, 1);
            }
            else if (Corner == TR && Middle == T ) // position 2
            {
                Parent.TextPosition(ROI, 2);
            }
            else if (Corner == TR && Middle == R ) // position 3
            {
                Parent.TextPosition(ROI, 3);
            }
            else if (Corner == BR && Middle == R ) // position 4
            {
                Parent.TextPosition(ROI, 4);
            }
            else if (Corner == BR && Middle == B ) // position 5
            {
                Parent.TextPosition(ROI, 5);
            }
            else if (Corner == BL && Middle == B ) // position 6
            {
                Parent.TextPosition(ROI, 6);
            }
            else if (Corner == BL && Middle == L ) // position 7
            {
                Parent.TextPosition(ROI, 7);
            }
            else if (Corner == TL && Middle == L ) // position 8
            {
                Parent.TextPosition(ROI, 8);
            }
        }

        private void ROINameLocationRectDisplay(int position)
        {
            TextPositionClearMiddle();
            TextPositionClearCorner();

            if (position == 1) // TL, T
            {
                TL.Fill = Brushes.Black;
                T.Fill = Brushes.Black;
                Corner = TL;
                Middle = T;
            }
            else if (position == 2) // TR, T
            {
                TR.Fill = Brushes.Black;
                T.Fill = Brushes.Black;
                Corner = TR;
                Middle = T;
            }
            else if (position == 3) // TR, R
            {
                TR.Fill = Brushes.Black;
                R.Fill = Brushes.Black;
                Corner = TR;
                Middle = R;
            }
            else if (position == 4) // BR, R
            {
                BR.Fill = Brushes.Black;
                R.Fill = Brushes.Black;
                Corner = BR;
                Middle = R;
            }
            else if (position == 5) // BR, B
            {
                BR.Fill = Brushes.Black;
                B.Fill = Brushes.Black;
                Corner = BR;
                Middle = B;
            }
            else if (position == 6) // BL, B
            {
                BL.Fill = Brushes.Black;
                B.Fill = Brushes.Black;
                Corner = BL;
                Middle = B;
            }
            else if (position == 7) // BL, L
            {
                BL.Fill = Brushes.Black;
                T.Fill = Brushes.Black;
                Corner = BL;
                Middle = L;
            }
            else if (position == 8)  // TL, L
            {
                TL.Fill = Brushes.Black;    
                L.Fill = Brushes.Black;
                Corner = TL;
                Middle = L;
            }
        }
        #endregion

        private void TxtROIName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ROI.roi_name = txtROIName.Text;
            Parent.RoiName(ROI);
        }
    }
}
