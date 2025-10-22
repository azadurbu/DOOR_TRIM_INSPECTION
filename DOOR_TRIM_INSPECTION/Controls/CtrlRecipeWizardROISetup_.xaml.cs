using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Controls.FormControls;
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

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CtrlRecipeWizardROISetup.xaml
    /// </summary>
    public partial class CtrlRecipeWizardROISetup : UserControl
    {
        private RecipeDB CurrentRecipe = null;
        private bool IsFront = true;
        private List<DetectionROIDetailsUI> ROIs = new List<DetectionROIDetailsUI>();

        private string roiDefaultName = "ROI";
        private int roiIDSelected = -1;
        private static bool isEditingRoi = false;
        private System.Windows.Point clickPosition = new System.Windows.Point(0, 0);
        private bool isAddingRoi = false;
        private System.Windows.Point _startPoint = new System.Windows.Point(0, 0);
        private System.Windows.Shapes.Rectangle _currentRoi = null;
        private List<System.Windows.Shapes.Rectangle> canvasROIs = new List<System.Windows.Shapes.Rectangle>();
        private int roiCounter = 0;
        private Canvas _currentCanvas = null;
        private bool isDragging = false;
        private System.Windows.Shapes.Rectangle _draggedRoi = null;
        private List<PopupROISetup> _activePopups = new List<PopupROISetup>();
        private List<DetectionROIDetailsUI> selectedROIsForChk = new List<DetectionROIDetailsUI>();


        public CtrlRecipeWizardROISetup()
        {
            InitializeComponent();
            _currentCanvas = DrawingCanvas;
            _currentCanvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            _currentCanvas.PreviewMouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            ImageEditorHelper.LoadROIsList(dgROIs, ROIs);
            ImageEditorHelper.ClearGroup(stkPanelAddGroup, selectedROIsForChk);
        }

        public void SetCurrentRecipeInfo(RecipeDB recipe, bool isFront)
        {
            CurrentRecipe = recipe;
            IsFront = isFront;

            LoadFormData();
        }

        public List<DetectionROIDetailsUI> GetDetectionROIs()
        {
            return ROIs;
        }

        private void LoadFormData()
        {
            LoadImage();
            
            // LOAD DETECTION ROIs
            LoadROIs();
            // LOAD RULES FOR THE POPUP
            LoadALCCodes();
            //Canvas.SetLeft(imgDoorTrim, -(zoomBorder.Width /2));
        }

        private void LoadALCCodes()
        {
            List<KeyValuePair<string, string>> ALCCodes = new List<KeyValuePair<string, string>>();
            ALCCodes.Add(new KeyValuePair<string, string>("SPA01", "Upper Trim"));
            ALCCodes.Add(new KeyValuePair<string, string>("SPA02", "Arm"));
            ALCCodes.Add(new KeyValuePair<string, string>("SPA03", "Lower Trim"));
            ALCCodes.Add(new KeyValuePair<string, string>("SPA04", "Grill"));
            ALCCodes.Add(new KeyValuePair<string, string>("SPA05", "Full Handle"));
            ALCCodes.Add(new KeyValuePair<string, string>("SPA06", "흡음제"));
            ALCCodes.Add(new KeyValuePair<string, string>("SPA07", "Handle"));
            ALCCodes.Add(new KeyValuePair<string, string>("SPA08", "Switch"));
            ALCCodes.Add(new KeyValuePair<string, string>("SPA09", "IMS"));
            ALCCodes.Add(new KeyValuePair<string, string>("SPA10", "Speaker"));
            ALCCodes.Add(new KeyValuePair<string, string>("SPA11", "Wire"));
            cboALCCode.ItemsSource = ALCCodes;
            cboALCCode.DisplayMemberPath = "Key";
            cboALCCode.SelectedValuePath = "Value";
        }

        private void LoadROIs()
        {
            List<DetectionROIDetails> dbROIs = Machine.hmcDBHelper.GetDetectionROIs(CurrentRecipe.RecipeID, IsFront);
            ROIs = new List<DetectionROIDetailsUI>();

            for (int i = _currentCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (_currentCanvas.Children[i] is Rectangle)
                {
                    _currentCanvas.Children.RemoveAt(i);
                }
            }

            foreach (DetectionROIDetails dbROI in dbROIs)
            {
                ROIs.Add(dbROI.toDetectionROIDetailsUI());
                ROIs.ElementAt(ROIs.Count - 1).roi_ui_counter = ROIs.Count;
                var roi = ImageEditorHelper.CreateRectangle(dbROI.start_x, dbROI.start_y, dbROI.end_x, dbROI.end_y, _currentCanvas);
                canvasROIs.Add(roi);
                AttachRectangleEvents(roi);
            }
            roiCounter = ROIs.Count();
        
            //dgROIs.Items.Clear();
            dgROIs.ItemsSource = ROIs;
        }

        private int AddToROIList(DetectionROIDetailsUI newROI)
        {
            ROIs.Add(newROI);
            ROIs.ElementAt(ROIs.Count - 1).roi_ui_counter = ROIs.Count;
            return ROIs.Count();
        }

        private void LoadImage()
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = IsFront ? new Uri(CurrentRecipe.FrontImagePath) : new Uri(CurrentRecipe.RearImagePath); // USING OLD SOURCE BECAUSE FILE COPY TAKES TIME
            bitmap.EndInit();
            imgDoorTrim.Source = bitmap;
            
            //imgDoorTrim.Margin = new Thickness(-3300, 0, 0, 0);
            //_currentCanvas.InvalidateArrange();
            //_currentCanvas.InvalidateMeasure();
            //_currentCanvas.InvalidateVisual();
        }
        public static int addbtnsteps = 0;
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (addbtnsteps != 0 || editBtnsteps != 0) return;
            if (isEditingRoi == true)
                isEditingRoi = false;
            if ( addbtnsteps == 0 ) isAddingRoi = true;
            if (isAddingRoi == true)
            {
                ImageEditorHelper.RemoveResizeHandles(_currentCanvas);
                RemoveAllPopups();
                dgROIs.IsEnabled = false;
            }
        }
        public static int editBtnsteps = 0;
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (addbtnsteps != 0 || editBtnsteps != 0) return;
            DetectionROIDetailsUI selectedROI = dgROIs.SelectedItem as DetectionROIDetailsUI;

            if (selectedROI != null)
            {
                isAddingRoi = false;
                isEditingRoi = true;
                editBtnsteps = 1;
                _currentRoi = null;
                var roi = ImageEditorHelper.FindRoiById(selectedROI.roi_ui_counter, ROIs, canvasROIs);
                ImageEditorHelper.RemoveResizeHandles(_currentCanvas);
                ImageEditorHelper.AddResizeHandles(ROIs, selectedROI.roi_ui_counter, roi, _currentCanvas, zoomBorder);
                ShowPopUp(roi, _currentCanvas);
                dgROIs.IsEnabled = false;
            }
            
        }

        private void ShowPopUp(System.Windows.Shapes.Rectangle _currentROI, Canvas _currentCanvas)
        {
            if (_currentROI != null)
            {
                roiIDSelected = ImageEditorHelper.RoiIDFromList(_currentROI, ROIs);
                var rectangleInfo = ROIs.FirstOrDefault(r => r.roi_ui_counter == roiIDSelected);

                var st = zoomBorder.Scale(_currentCanvas);
                var scalex = 1 / st.ScaleX < 1 ? 1 : 1 / st.ScaleX;
                var scaley = 1 / st.ScaleY < 1 ? 1 : 1 / st.ScaleY;
                var popUpWindow = new PopupROISetup(rectangleInfo, ROIs, dgROIs, _currentCanvas, stkPanelAddGroup, selectedROIsForChk);

                //Calculate position based on the selected rectangle
                double xPosition = Canvas.GetLeft(_currentROI) + _currentROI.Width + 10;
                double yPosition = Canvas.GetTop(_currentROI);

                if (xPosition + popUpWindow.Width > _currentCanvas.RenderSize.Width)
                {
                    xPosition = Canvas.GetLeft(_currentROI) - popUpWindow.Width - 10;
                }

                if (yPosition + popUpWindow.Height > _currentCanvas.ActualHeight)
                {
                    yPosition = Canvas.GetTop(_currentROI) - popUpWindow.Height - 10;
                }

                Canvas.SetLeft(popUpWindow, xPosition);
                Canvas.SetTop(popUpWindow, yPosition);

                ScaleTransform scaleTransform = new ScaleTransform(scalex, scaley);

                if (popUpWindow is FrameworkElement popupContent)
                {
                    popUpWindow.LayoutTransform = scaleTransform;
                }
                _currentCanvas.Children.Add(popUpWindow);
                RemoveAllPopups();
                _activePopups.Add(popUpWindow);
                //_currentCanvas.MouseDown += PopupCanvas_MouseDown;
            }
        }
        
        internal void SetImageMargin()
        {
            imgDoorTrim.Margin = new Thickness(0, 0, 0, 0);
            zoomBorder.IsDrawing(false);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (addbtnsteps != 0 || editBtnsteps != 0) return;
            var rectInfo = dgROIs.SelectedItem as DetectionROIDetailsUI;
            if (rectInfo != null)
            {
                var rectangle = ImageEditorHelper.FindRoiById(rectInfo.roi_ui_counter, ROIs, canvasROIs);
                if (rectangle != null)
                {
                    var itemToRemove = ROIs.FirstOrDefault(item => item.roi_ui_counter == rectInfo.roi_ui_counter);
                    if (itemToRemove != null)
                    {
                        //ROIs.Remove(itemToRemove);
                        ImageEditorHelper.RemoveROI(rectInfo.roi_ui_counter, ROIs, dgROIs, canvasROIs, _currentCanvas, _currentRoi);
                        _currentRoi = null;
                        ImageEditorHelper.ClearGroup(stkPanelAddGroup, selectedROIsForChk);
                    }
                }
            }
        }

        private void RoiDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgROIs.SelectedItem is DetectionROIDetailsUI rectInfo)
            {
                var rectangle = ImageEditorHelper.FindRoiById(rectInfo.roi_ui_counter, ROIs, canvasROIs);

                if (rectangle != null)
                {
                    ImageEditorHelper.ResetAllROIsToTransparent(canvasROIs);
                    rectangle.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 255, 255, 0));
                }
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            imgDoorTrim.Margin = new Thickness(0, 0, 0, 0);

            if (isAddingRoi)
            {
                addbtnsteps = 1;
                editBtnsteps = 0;
                roiCounter++;
                _startPoint = e.GetPosition(_currentCanvas);
                _currentRoi = new System.Windows.Shapes.Rectangle
                {
                    Stroke = System.Windows.Media.Brushes.Yellow,
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 0, 0))
                };
                _currentCanvas.Children.Add(_currentRoi);
                _currentCanvas.MouseMove += Canvas_MouseMove;
                canvasROIs.Add(_currentRoi);
                zoomBorder.IsDrawing(true);
            }
            ImageEditorHelper.ResetAllROIsToTransparent(canvasROIs);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_currentRoi != null)
            {
                var endPoint = e.GetPosition(_currentCanvas);
                //var endPoint = new Point(_startPoint.X + 100, _startPoint.Y + 100);
                ImageEditorHelper.UpdateROI(_currentRoi, _startPoint, endPoint);
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentRoi != null && isAddingRoi == true && addbtnsteps == 1)
            {
                isAddingRoi = false;
                zoomBorder.IsDrawing(false);
                var endPoint = Mouse.GetPosition(_currentCanvas);
                //var endPoint = new Point(_startPoint.X + 100, _startPoint.Y + 100);
                // Store the start point for resizing
                _startPoint = new System.Windows.Point(Canvas.GetLeft(_currentRoi), Canvas.GetTop(_currentRoi));
                string rectangleName = roiDefaultName + " " + roiCounter;
                var currentRecipe = CurrentRecipe.RecipeID;
                bool isFront = IsFront;
                var rectangleInfo = new DetectionROIDetailsUI
                {
                    start_x = _startPoint.X,
                    start_y = _startPoint.Y,
                    end_x = endPoint.X,
                    end_y = endPoint.Y,
                    roi_name = rectangleName, 
                    recipe_ID = currentRecipe,
                    front_door = isFront == true ? 1 : 0
                };
                AddToROIList(rectangleInfo);
                AttachRectangleEvents(_currentRoi);
                ShowPopUp(_currentRoi, _currentCanvas);
            }
            _currentCanvas.MouseMove -= Canvas_MouseMove;
            zoomBorder.IsDrawing(false);
        }

        private void AttachRectangleEvents(System.Windows.Shapes.Rectangle rectangle)
        {
            rectangle.MouseLeftButtonDown += (s, e) =>
            {
                if (isAddingRoi == true) return;
                _currentRoi = null;
                e.Handled = true;
                double topLeftX = Canvas.GetLeft(rectangle);
                double topLeftY = Canvas.GetTop(rectangle);
                double bottomRightX = topLeftX + rectangle.Width;
                double bottomRightY = topLeftY + rectangle.Height;
                zoomBorder.IsDrawing(true);
                //if (isEditingRoi)
                //{
                //    editBtnsteps = 0;
                //    isEditingRoi = false;
                //    ImageEditorHelper.RemoveResizeHandles(_currentCanvas);
                //    RemoveAllPopups();
                //}
                //else 
                if (isEditingRoi == false && isDragging == false)
                {
                    // Start dragging
                    isDragging = true;
                    clickPosition = e.GetPosition(_currentCanvas);
                    rectangle.CaptureMouse();
                    _draggedRoi = rectangle;
                    topLeftX = Canvas.GetLeft(rectangle);
                    topLeftY = Canvas.GetTop(rectangle);
                    bottomRightX = topLeftX + rectangle.Width;
                    bottomRightY = topLeftY + rectangle.Height;
                    //rectangleIDSelected = ImageEditorHelper.RectangleID(topLeftX, topLeftY, bottomRightX, bottomRightY, rectanglesList);
                    roiIDSelected = ImageEditorHelper.RoiIDFromList(rectangle, ROIs);
                    ImageEditorHelper.SelectRowInDataGrid(roiIDSelected, dgROIs);
                }
            };

            rectangle.MouseMove += (s, e) =>
            {
                if (isAddingRoi == true) return;
                if (isDragging && _draggedRoi == rectangle)
                {
                    _currentRoi = rectangle;
                    var r2x1 = Canvas.GetLeft(_currentRoi);
                    var r2y1 = Canvas.GetTop(_currentRoi);
                    var r2x2 = _currentRoi.Width;
                    var r2y2 = _currentRoi.Height;
                    var rectangleInfo = ROIs.FirstOrDefault(r => ImageEditorHelper.CalculateIntersectionPercentage(
                                                            r.start_x, r.start_y, r.end_x - r.start_x, r.end_y - r.start_y,
                                                            r2x1, r2y1, r2x2, r2y2) > 99);


                    var currentPosition = e.GetPosition(_currentCanvas);
                    var offsetX = currentPosition.X - clickPosition.X;
                    var offsetY = currentPosition.Y - clickPosition.Y;

                    var newLeft = Canvas.GetLeft(rectangle) + offsetX;
                    var newTop = Canvas.GetTop(rectangle) + offsetY;

                    Canvas.SetLeft(rectangle, newLeft);
                    Canvas.SetTop(rectangle, newTop);

                    clickPosition = currentPosition;

                    // Update all handles' positions to match the new size/position of the rectangle
                    foreach (var handle in _currentCanvas.Children.OfType<System.Windows.Shapes.Rectangle>())
                    {
                        if (handle.Tag is ImageEditorHelper.HandlePosition positionTag)
                        {
                            ImageEditorHelper.UpdateHandlePosition(ROIs, rectangle, handle); // Update each handle's position
                        }
                    }
                    if (rectangleInfo != null)
                    ImageEditorHelper.UpdateRoiInformation(ROIs, roiIDSelected, newLeft, newTop, newLeft + rectangle.Width, newTop + rectangle.Height, rectangleInfo.roi_name,rectangleInfo.detection_class_ID, rectangleInfo.DetectionClassName,rectangleInfo.Parameters);
                }
            };

            rectangle.MouseLeftButtonUp += (s, e) =>
            {
                if (isAddingRoi == true) return;
                isDragging = false;
                 rectangle.ReleaseMouseCapture();
                _draggedRoi = null; // Clear the reference when done dragging
                //dgROIs.ItemsSource = ROIs;
                ImageEditorHelper.SelectRowInDataGrid(ImageEditorHelper.RoiIDFromList(rectangle, ROIs), dgROIs);
                zoomBorder.IsDrawing(false);
                
                roiIDSelected = ImageEditorHelper.RoiIDFromList(rectangle, ROIs);
                var rectangleInfo = ROIs.FirstOrDefault(r => r.roi_ui_counter == roiIDSelected);
                if (roiIDSelected != -1 && addbtnsteps == 1)
                {
                    ImageEditorHelper.UpdateRoiInformation(ROIs, roiIDSelected, rectangleInfo.start_x, rectangleInfo.start_y, rectangleInfo.end_x, rectangleInfo.end_y, rectangleInfo.roi_name, rectangleInfo.detection_class_ID, rectangleInfo.DetectionClassName, rectangleInfo.Parameters);
                    if(isAddingRoi)
                    ShowPopUp(rectangle, _currentCanvas);
                }
                _currentRoi = rectangle;
            };
            // Visual feedback on hover
            rectangle.MouseEnter += (s, e) =>
            {
                if (isAddingRoi == true) return;
                rectangle.Stroke = System.Windows.Media.Brushes.Blue;
            };
            rectangle.MouseLeave += (s, e) =>
            {
                if (isAddingRoi == true) return;
                rectangle.Stroke = System.Windows.Media.Brushes.Yellow;
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            //copy rectangle
            if (e.Key == Key.C)
            {
                CopyRectangle();
            }
        }

        private void CopyRectangle()
        {
            if (addbtnsteps != 0 || editBtnsteps != 0) return;

            if (_currentRoi != null)
            {
                roiCounter++;
                System.Windows.Shapes.Rectangle copiedRectangle = new System.Windows.Shapes.Rectangle
                {
                    Width = _currentRoi.Width,
                    Height = _currentRoi.Height,
                    Fill = _currentRoi.Fill,
                    Stroke = _currentRoi.Stroke,
                    StrokeThickness = _currentRoi.StrokeThickness,
                };

                double currentLeft = Canvas.GetLeft(_currentRoi);
                double currentTop = Canvas.GetTop(_currentRoi);

                Canvas.SetLeft(copiedRectangle, currentLeft + 10);
                Canvas.SetTop(copiedRectangle, currentTop + 10);

                _currentCanvas.Children.Add(copiedRectangle);
                _currentRoi = copiedRectangle;
                AttachRectangleEvents(_currentRoi);
                // Store rectangle information
                string rectangleName = roiDefaultName + " " + roiCounter;

                var rectangleInfo = new DetectionROIDetailsUI
                {
                    start_x = currentLeft + 10,
                    start_y = currentTop + 10,
                    end_x = currentLeft + 10 + copiedRectangle.Width,
                    end_y = currentTop + 10 + copiedRectangle.Height,
                    roi_name = rectangleName
                };
                AddToROIList(rectangleInfo);
                canvasROIs.Add(copiedRectangle);
                ShowPopUp(_currentRoi, _currentCanvas);
            }
            else
            {
                MessageBox.Show("No rectangle selected to copy!");
            }
        }

        private void RemoveAllPopups()
        {
            foreach (var popup in _activePopups)
            {
                if (_currentCanvas.Children.Contains(popup))
                {
                    _currentCanvas.Children.Remove(popup);
                }
            }
            _activePopups.Clear();
            //_currentCanvas.MouseDown -= PopupCanvas_MouseDown;
        }

        private void PopupCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isAddingRoi || isEditingRoi)
            {
                RemoveAllPopups();
                _currentCanvas.MouseDown -= PopupCanvas_MouseDown;

            }
        }

        //private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    RemoveAllPopups();
        //}

        
        private void SaveGroupName_Click(object sender, RoutedEventArgs e)
        {
            string groupName = txtGroupName.Text;

            if (!string.IsNullOrEmpty(groupName))
            {
                foreach (var item in selectedROIsForChk)
                {
                    item.group_name = groupName;
                }
                //GroupNamePopup.IsOpen = false;

                selectedROIsForChk.Clear();
            }
        }

        private void CancelGroupName_Click(object sender, RoutedEventArgs e)
        {
            //GroupNamePopup.IsOpen = false;
        }

        private void CboALCCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyValuePair<string, string> keyValuePair = (KeyValuePair<string, string>)cboALCCode.SelectedItem;
            txtALCName.Text = keyValuePair.Value;
        }

        
        private void BtnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<string, string> selectedItem = (KeyValuePair<string, string>)cboALCCode.SelectedItem;
            string selectedALCCode = selectedItem.Key;
            foreach (var item in selectedROIsForChk)
            {
                item.ALC_CODE = selectedALCCode;
                item.ALC_NAME = txtALCName.Text;
                item.group_name = txtGroupName.Text;
            }

            foreach (var selectedROI in selectedROIsForChk)
            {
                var existingROI = ROIs.FirstOrDefault(roi => roi.roi_ui_counter == selectedROI.roi_ui_counter);

                if (existingROI != null)
                {
                    existingROI.ALC_CODE = selectedROI.ALC_CODE;
                    existingROI.ALC_NAME = selectedROI.ALC_NAME;
                    existingROI.group_name = selectedROI.group_name;
                }
            }
            ClearCheckedCheckboxes();
            ClearGroupForm();
            ImageEditorHelper.LoadROIsList(dgROIs, ROIs);
            ImageEditorHelper.ClearGroup(stkPanelAddGroup, selectedROIsForChk);
        }
        

        private void ClearCheckedCheckboxes()
        {
            foreach (var row in dgROIs.Items)
            {
                DataGridRow dataGridRow = (DataGridRow)dgROIs.ItemContainerGenerator.ContainerFromItem(row);

                if (dataGridRow != null)
                {
                    CheckBox checkBox = GetChildOfType<CheckBox>(dataGridRow);

                    if (checkBox != null)
                    {
                        checkBox.IsChecked = false;
                    }
                }
            }
        }
        private T GetChildOfType<T>(DependencyObject parent) where T : DependencyObject
        {
            // This method recursively searches the visual tree for the first child of the specified type
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    return (T)child;
                }

                T result = GetChildOfType<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void ClearGroupForm()
        {
            btnCreateGroup.IsEnabled = false;
            txtALCName.Text = "";
            txtGroupName.Text = "";
            cboALCCode.SelectedIndex = 0;
        }

        private void BtnCreateGroup_Click(object sender, RoutedEventArgs e)
        {
            stkPanelAddGroup.Visibility = selectedROIsForChk.Count > 1? Visibility.Visible: Visibility.Hidden;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (addbtnsteps == 1 || editBtnsteps == 1)
            {
                (sender as CheckBox).IsChecked = false; return;
            }
            var selectedItem = (sender as CheckBox)?.DataContext as DetectionROIDetailsUI;
            if (selectedItem != null && !selectedROIsForChk.Contains(selectedItem))
            {
                selectedROIsForChk.Add(selectedItem);
            }
            btnCreateGroup.IsEnabled = selectedROIsForChk.Count > 1;
            btnRemoveGroup.IsEnabled = selectedROIsForChk.Count > 1;
            editBtnsteps = 2;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (sender as CheckBox)?.DataContext as DetectionROIDetailsUI;
            if (selectedItem != null)
            {
                selectedROIsForChk.Remove(selectedItem);
            }
            btnCreateGroup.IsEnabled = selectedROIsForChk.Count > 1;
            btnRemoveGroup.IsEnabled = selectedROIsForChk.Count > 1;
            editBtnsteps = selectedROIsForChk.Count >= 1 ? 2 : 0;
        }

        private void BtnRemoveGroup_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<string, string> selectedItem = (KeyValuePair<string, string>)cboALCCode.SelectedItem;
            string selectedALCCode = selectedItem.Key;
            foreach (var item in selectedROIsForChk)
            {
                item.ALC_CODE = "";
                item.ALC_NAME = "";
                item.group_name = "";
            }
            ClearCheckedCheckboxes();
            ClearGroupForm();
            ImageEditorHelper.LoadROIsList(dgROIs, ROIs);
            ImageEditorHelper.ClearGroup(stkPanelAddGroup, selectedROIsForChk);
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            zoomBorder.Reset();
        }

        public void ResetCanvas()
        {
            zoomBorder.Reset();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            zoomBorder.Reset();
        }
    }
}
