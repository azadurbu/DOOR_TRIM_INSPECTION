using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Controls.FormControls;
using Doortrim_Inspection.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        public bool IsFront = true;

        public DetectionROIDetailsUI ROI;
        public List<DetectionROIDetailsUI> ROIs;
        private List<DetectionROIDetailsUI> selectedROIsForChk;
        private Dictionary<int, TextBlock> roiTextBlocks;


        private int roiCounter = 0;
        private Canvas canvas = null;
        private PopupROISetup popUpWindow = null;

        public PopupROISetup PopUp
        {
            get
            {
                if (popUpWindow == null)
                    popUpWindow = new PopupROISetup();
                popUpWindow.LoadDetectionClasses();
                return popUpWindow;
            }
        }

        private int ROIThickness = 4;
        public SolidColorBrush ROIFill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 255, 255, 255));
        public SolidColorBrush ROIFillSelected = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 255, 255, 0));
        private SolidColorBrush ROIBorder = Brushes.Yellow;
        private string roiDefaultName = "ROI";

        public bool _isAdding = false;
        public bool _isCopying = false;
        public bool _isDragging = false;
        public bool _isEditing = false;
        public bool _isAddEdit = false;
        public bool _isZoomPanDisabled = false;
        public bool _isFromPrevious = false;
        private double ScreenRatio = .2;
        private Point startPoint;
        private Point origin;
        private int fs = 48;

        //double click detection var
        private int clickCount = 0;
        private DateTime lastClickTime = DateTime.MinValue;
        private DateTime currentClickTime = DateTime.MinValue;
        private const double clickThreshold = 500; // in milliseconds

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);

        public Color GetColorAt(Point cursor)
        {

            int x = (int)cursor.X, y = (int)cursor.Y;
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int a = (int)GetPixel(dc, x, y);
            ReleaseDC(desk, dc);
            return Color.FromArgb(255, (byte)((a >> 0) & 0xff), (byte)((a >> 8) & 0xff), (byte)((a >> 16) & 0xff));
        }
        public CtrlRecipeWizardROISetup()
        {
            InitializeComponent();
            canvas = DrawingCanvas;

            //canvas zoom pan action set
            canvas.MouseWheel += ZoomPan_MouseWheel;
            canvas.MouseLeftButtonDown += ZoomPan_MouseLeftButtonDown;
            canvas.MouseLeftButtonUp += ZoomPan_MouseLeftButtonUp;
            canvas.MouseMove += ZoomPan_MouseMove;
            //canvas.PreviewMouseRightButtonDown += ZoomPan_PreviewMouseRightButtonDown;
            canvas.Loaded += ZoomPan_Loaded;

            //rectangle drawing action set
            canvas.MouseLeftButtonDown += Canvas_MouseDown;
            canvas.MouseLeftButtonUp += Canvas_MouseUp;
            canvas.MouseMove += Canvas_MouseMove;

            // resize and center the image at the beginning
            if (canvas is FrameworkElement fe) fe.Loaded += (s, e) => zoomBorder.ImageLoad(canvas);
            this.LayoutUpdated += CtrlRecipeWizardROISetup_LayoutUpdated;
        }

        private void CtrlRecipeWizardROISetup_LayoutUpdated(object s, EventArgs e)
        {
            if (_isFromPrevious)
            {
                ClearDisabled();
            }
        }

        private int RoiCounter()
        {
            return ++roiCounter;
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
            roiCounter = 0;
            List<DetectionROIDetails> dbROIs = Machine.hmcDBHelper.GetDetectionROIs(CurrentRecipe.RecipeID, IsFront);
            ROIs = new List<DetectionROIDetailsUI>();
            roiTextBlocks = new Dictionary<int, TextBlock>();
            selectedROIsForChk = null;

            //for (int i = canvas.Children.Count - 1; i >= 0; i--)
            //{
            //    if (canvas.Children[i] is DetectionROIDetailsUI)
            //    {
            //        canvas.Children.RemoveAt(i);
            //    }
            //}
            canvas.Children.RemoveRange(1, canvas.Children.Count - 1);

            foreach (DetectionROIDetails dbROI in dbROIs)
            {
                ROI = dbROI.toDetectionROIDetailsUI();

                DrawROI(ROI);
            }
            roiCounter = ROIs.Count();
            ROI = null;
            LoadROIsList();
        }

        public void DrawROI(DetectionROIDetailsUI roi, bool isCopy = false)
        {
            var roi_counter = RoiCounter();
            string roiNam;
            if (isCopy == true)
            {
                roiNam = roiDefaultName + " " + roi_counter;
            }
            else
            {
                roiNam = roi.roi_name;
            }

            DetectionROIDetailsUI newROI = new DetectionROIDetailsUI
            {
                Width = roi.end_x - roi.start_x,
                Height = roi.end_y - roi.start_y,
                roi_ui_counter = roi_counter,
                start_x = roi.start_x,
                start_y = roi.start_y,
                end_x = roi.end_x,
                end_y = roi.end_y,
                detection_roi_ID = roi.detection_roi_ID,
                roi_name = roiNam,
                detection_class_ID = roi.detection_class_ID,
                DetectionClassName = roi.DetectionClassName.Trim(),
                Parameters = roi.Parameters.Trim(),
                recipe_ID = CurrentRecipe.RecipeID,
                front_door = roi.front_door,
                ALC_CODE = roi.ALC_CODE.Trim(),
                ALC_NAME = roi.ALC_NAME.Trim(),
                group_name = roi.group_name.Trim(),
                Use = roi.Use,
                roi_name_location = roi.roi_name_location
            };
            newROI.Stroke = ROIBorder;
            newROI.StrokeThickness = ROIThickness;
            newROI.Fill = ROIFill;

            if (isCopy == true)
            {
                Canvas.SetLeft(newROI, roi.start_x + 100);
                Canvas.SetTop(newROI, roi.start_y + 100);
            }
            else
            {
                Canvas.SetLeft(newROI, roi.start_x);
                Canvas.SetTop(newROI, roi.start_y);
            }
            canvas.Children.Add(newROI);
            AddROIEvent(newROI);
            ROIs.Add(newROI);
            ROI = newROI;
            RoiName(ROI);
        }

        //private int AddToROIList(DetectionROIDetailsUI newROI)
        //{
        //    ROIs.Add(newROI);
        //    ROIs.ElementAt(ROIs.Count - 1).roi_ui_counter = ROIs.Count;
        //    return ROIs.Count();
        //}

        private void LoadImage()
        {
            if (((App)System.Windows.Application.Current).usePreloadedImages)
            {
                BitmapImage bitmap = ((App)System.Windows.Application.Current).GetImage(CurrentRecipe.RecipeID, IsFront);
                if (bitmap != null)
                {
                    imgDoorTrim.Source = bitmap;
                }
                ((App)System.Windows.Application.Current).usePreloadedImages = false;
            }
            else
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = IsFront ? new Uri(CurrentRecipe.FrontImagePath) : new Uri(CurrentRecipe.RearImagePath); // USING OLD SOURCE BECAUSE FILE COPY TAKES TIME
                bitmap.EndInit();
                imgDoorTrim.Source = bitmap;
            }
        }


        #region rectangle drawing event
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isAdding == false || _isDragging == true || ImageEditorHelper._isEditingROIHandle == true) return;

            if (_isAdding == true)
            {
                startPoint = e.GetPosition(canvas);
                var counter = RoiCounter();

                List<DetectionROIDetailsUI> lastROIList = ROIs.Where(x => x.roi_name.StartsWith("ROI")).OrderBy(o => o.roi_name).ToList();
                if (lastROIList.Count > 0)
                {
                    DetectionROIDetailsUI lastROI = lastROIList.ElementAt(0);
                    int roiNum = 0;
                    int.TryParse(lastROI.roi_name.Replace("ROI", "").Trim(), out roiNum);
                    counter = Math.Max(counter, roiNum + 1) ;
                }
                
                ROI = new DetectionROIDetailsUI
                {
                    roi_ui_counter = counter,
                    recipe_ID = CurrentRecipe.RecipeID,
                    roi_name = roiDefaultName + " " + counter,
                    front_door = IsFront == true ? 1 : 0,
                    Stroke = ROIBorder,
                    Fill = ROIFill,
                    StrokeThickness = ROIThickness,
                    roi_name_location = 1
                };
                Canvas.SetLeft(ROI, startPoint.X);
                Canvas.SetTop(ROI, startPoint.Y);
                canvas.Children.Add(ROI);
                AddROIEvent(ROI);
                ROIs.Add(ROI);
                canvas.CaptureMouse();
            }

        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Ensure the image has a valid source
                if (imgDoorTrim.Source is BitmapSource bitmapSource)
                {
                    // Get the position of the mouse relative to the image
                    Point mousePosition = e.GetPosition(imgDoorTrim);

                    // Scale the mouse position to the image's actual pixel dimensions
                    int xx = (int)(mousePosition.X * bitmapSource.PixelWidth / imgDoorTrim.ActualWidth);
                    int yy = (int)(mousePosition.Y * bitmapSource.PixelHeight / imgDoorTrim.ActualHeight);

                    // Ensure the coordinates are within bounds
                    if (xx >= 0 && xx < bitmapSource.PixelWidth && yy >= 0 && yy < bitmapSource.PixelHeight)
                    {
                        // Get pixel data
                        var pixelData = new byte[4]; // BGRA format
                        var stride = bitmapSource.PixelWidth * 4;
                        bitmapSource.CopyPixels(new Int32Rect(xx, yy, 1, 1), pixelData, stride, 0);

                        // Extract color components
                        byte blue = pixelData[0];
                        byte green = pixelData[1];
                        byte red = pixelData[2];
                        byte alpha = pixelData[3];

                        Color color = Color.FromArgb(alpha, red, green, blue);
                        //= GetColorAt(e.GetPosition(this));
                        DrawingCanvasStatus.Text = "R : " + color.R + ", G : " + color.G + ", B : " + color.B;
                    }
                }
            }
            catch { }
            if (e.LeftButton == MouseButtonState.Released || ROI == null
                || _isAdding == false || _isDragging == true
                || ImageEditorHelper._isEditingROIHandle == true) return;

            var currentPoint = e.GetPosition(canvas);

            double imageWidth = imgDoorTrim.ActualWidth;
            double imageHeight = imgDoorTrim.ActualHeight;

            currentPoint.X = Math.Max(0, Math.Min(currentPoint.X, imageWidth));
            currentPoint.Y = Math.Max(0, Math.Min(currentPoint.Y, imageHeight));

            var x = Math.Min(currentPoint.X, startPoint.X);
            var y = Math.Min(currentPoint.Y, startPoint.Y);


            var w = Math.Min(Math.Abs(currentPoint.X - startPoint.X), imageWidth - x);
            var h = Math.Min(Math.Abs(currentPoint.Y - startPoint.Y), imageHeight - y);
            //var w = Math.Max(currentPoint.X, startPoint.X) - x;
            //var h = Math.Max(currentPoint.Y, startPoint.Y) - y;

            ROI.Width = w;
            ROI.Height = h;

            Canvas.SetLeft(ROI, x);
            Canvas.SetTop(ROI, y);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isAdding == false || _isDragging == true || ImageEditorHelper._isEditingROIHandle == true) return;

            AddROIEvent(ROI);
            _isAdding = false;
            _isEditing = true;
            _isAddEdit = true;
            ImageEditorHelper.OnRoiUpdated += CtrlRecipeWizardROISetup_OnRoiUpdated;
            ImageEditorHelper.RemoveResizeHandles(canvas);
            ImageEditorHelper.AddResizeHandles(ROI, canvas);
            ShowPopUp(ROI);
            canvas.ReleaseMouseCapture();
            RoiName(ROI);
        }
        #endregion

        #region canvas zoom pan events
        private void ZoomPan_Loaded(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomOutViewCentered();
        }

        private void ZoomPan_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isZoomPanDisabled == true) return;
            zoomBorder.Reset();
            if (ROI != null)
            {
                ImageEditorHelper.RemoveResizeHandles(canvas);
                ImageEditorHelper.AddResizeHandles(ROI, canvas);
                RemovePopup();
                ShowPopUp(ROI);
            }
        }

        private void ZoomPan_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isZoomPanDisabled == true || _isAdding == true || _isDragging == true || ImageEditorHelper._isEditingROIHandle == true) return;
            if (canvas != null)
            {
                Canvas childCanvas = (Canvas)canvas;
                if (canvas.IsMouseCaptured)
                {
                    var tt = zoomBorder.GetTranslateTransform(canvas);
                    Vector v = startPoint - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                    zoomBorder.ConstrainTranslationToBounds();
                }
            }
        }

        private void ZoomPan_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isZoomPanDisabled == true || _isAdding == true || _isDragging == true || ImageEditorHelper._isEditingROIHandle == true) return;
            if (canvas != null)
            {
                canvas.ReleaseMouseCapture();
                zoomBorder.Cursor = Cursors.Arrow;
            }
        }

        private void ZoomPan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isZoomPanDisabled == true || _isAdding == true || _isDragging == true || ImageEditorHelper._isEditingROIHandle == true) return;
            if (canvas != null)
            {
                var tt = zoomBorder.GetTranslateTransform(canvas);
                startPoint = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                canvas.CaptureMouse();
            }
        }

        private void ZoomPan_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_isZoomPanDisabled == true || _isAdding == true || _isDragging == true || ImageEditorHelper._isEditingROIHandle == true) return;

            if (canvas != null)
            {
                var st = zoomBorder.GetScaleTransform(canvas);
                var tt = zoomBorder.GetTranslateTransform(canvas);

                double zoom = e.Delta > 0 ? ScreenRatio : -ScreenRatio;
                if (!(e.Delta > 0) && (st.ScaleX <= ScreenRatio + .1 || st.ScaleY <= ScreenRatio + .1))
                {
                    zoomBorder.Reset();
                    return;
                }

                Point relative = e.GetPosition(canvas);

                double absoluteX = relative.X * st.ScaleX + tt.X;
                double absoluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = absoluteX - relative.X * st.ScaleX;
                tt.Y = absoluteY - relative.Y * st.ScaleY;

                if (ROI != null)
                {
                    ImageEditorHelper.RemoveResizeHandles(canvas);
                    ImageEditorHelper.AddResizeHandles(ROI, canvas);
                    RemovePopup();
                    ShowPopUp(ROI);
                }
            }
        }
        #endregion


        #region Form Action
        private void RoiDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgROIs.SelectedItem is DetectionROIDetailsUI RoiInfo)
            {
                if (RoiInfo != null)
                {
                    FillClear();
                    RoiInfo.Fill = ROIFillSelected;
                }
            }
        }


        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            DrawROICanvas();
        }

        private void DrawROICanvas()
        {
            if (_isEditing == true && _isAddEdit == true && _isAdding == true) return;
            _isAdding = true;
            btnAdd.IsEnabled = false;
            dgROIs.IsEnabled = false;
        }

        private void ShowPopUp(DetectionROIDetailsUI ROIInfo)
        {
            if (ROIInfo != null)
            {
                try
                {
                    PopUp.SetPopup(ROIInfo, this);
                    RoiName(ROIInfo);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Machine.logger.Write(eLogType.ERROR, e.ToString());
                }
            }
        } 

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var selectedROI = (sender as CheckBox)?.DataContext as DetectionROIDetailsUI;
            var existingROI = ROIs.FirstOrDefault(roi => roi.roi_ui_counter == selectedROI.roi_ui_counter);
             
            if (selectedROI != null && existingROI != null && !selectedROIsForChk.Contains(selectedROI))
            {
                selectedROIsForChk.Add(selectedROI);
                existingROI.isChecked = true;
            }
            btnRemoveGroup.IsEnabled = false;
            btnCreateGroup.IsEnabled = false;
            btnModifyGroup.IsEnabled = false;

            foreach (var roi in selectedROIsForChk)
            {
                if (roi.group_name != "")
                {
                    btnRemoveGroup.IsEnabled = true;
                    btnCreateGroup.Visibility = Visibility.Collapsed;
                    btnModifyGroup.Visibility = Visibility.Visible;
                    btnModifyGroup.IsEnabled = true;
                    break;
                }
                else
                {
                    btnRemoveGroup.IsEnabled = false;
                    btnCreateGroup.Visibility = Visibility.Visible;
                    btnModifyGroup.Visibility = Visibility.Collapsed;
                    btnCreateGroup.IsEnabled = true;
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var selectedROI = (sender as CheckBox)?.DataContext as DetectionROIDetailsUI;
            var existingROI = ROIs.FirstOrDefault(roi => roi.roi_ui_counter == selectedROI.roi_ui_counter);

            if (selectedROI != null && existingROI != null)
            {
                selectedROIsForChk.Remove(selectedROI);
                existingROI.isChecked = false;

            }
            btnRemoveGroup.IsEnabled = false;
            btnCreateGroup.IsEnabled = false;
            btnModifyGroup.IsEnabled = false;
            foreach (var roi in selectedROIsForChk)
            {
                if (roi.group_name != "")
                {
                    btnRemoveGroup.IsEnabled = true;
                    btnCreateGroup.Visibility = Visibility.Collapsed;
                    btnModifyGroup.Visibility = Visibility.Visible;
                    btnModifyGroup.IsEnabled = true;
                    break;
                }
                else
                {
                    btnRemoveGroup.IsEnabled = false;
                    btnCreateGroup.Visibility = Visibility.Visible;
                    btnModifyGroup.Visibility = Visibility.Collapsed;
                    btnCreateGroup.IsEnabled = true;
                }
            }
        }

        private void BtnRemoveGroup_Click(object sender, RoutedEventArgs e)
        {
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
            btnRemoveGroup.IsEnabled = false;
        }

        private void BtnCreateGroup_Click(object sender, RoutedEventArgs e)
        {
            stkPanelAddGroup.Visibility = selectedROIsForChk.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            _isAddEdit = true;
            btnAdd.IsEnabled = false;
            dgROIs.IsEnabled = false;
            btnRemoveGroup.IsEnabled = false;
            cboALCCode.IsEnabled = true;
            txtALCName.IsEnabled = true;
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

            ClearDisabled();
        }


        private void BtnCancelGroup_Click(object sender, RoutedEventArgs e)
        {
            ClearDisabled();
        }

        private void DgROIs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgROIs.SelectedItem is DetectionROIDetailsUI _ROI)
            {
                if (_ROI != null)
                {
                    ROI = _ROI;
                    _isEditing = true;
                    ShowPopUp(_ROI);
                    ImageEditorHelper.RemoveResizeHandles(canvas);
                    ImageEditorHelper.OnRoiUpdated += CtrlRecipeWizardROISetup_OnRoiUpdated;
                    ImageEditorHelper.AddResizeHandles(_ROI, canvas);
                    btnAdd.IsEnabled = false;
                    dgROIs.IsEnabled = false;
                    FillClear();
                    _ROI.Fill = ROIFillSelected;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                CopyRectangle();
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D)
            {
                DrawROICanvas();
            }
            if (e.Key == Key.Escape)
            {
                ClearDisabled();
            }
            if (e.Key == Key.Delete)
            {
                RemoveROI();
            }
        }

        #endregion


        #region helpers
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
            foreach (var roi in ROIs)
            {
                roi.isChecked = false;
            }
            selectedROIsForChk = new List<DetectionROIDetailsUI>();
            LoadROIsList();
        }

        private void ClearGroupForm()
        {
            btnCreateGroup.IsEnabled = false;
            txtALCName.Text = "";
            txtGroupName.Text = "";
            cboALCCode.SelectedIndex = 0;
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

        public void LoadROIsList()
        {
            dgROIs.ItemsSource = ROIs;
            dgROIs.Items.Refresh();
        }

        private void AddROIEvent(DetectionROIDetailsUI _ROI)
        {
            _ROI.MouseLeftButtonDown += (s, e) =>
            {
                if (_isAdding == true || _isDragging == true || ImageEditorHelper._isEditingROIHandle == true) return;
                Point mousePosition = e.GetPosition(canvas);

                // Check the time between clicks
                currentClickTime = DateTime.Now;

                // If the time between clicks is less than the threshold, count it as a click
                if ((currentClickTime - lastClickTime).TotalMilliseconds <= clickThreshold)
                {
                    clickCount++;
                }
                else
                {
                    clickCount = 1; // Reset count if time gap is too large for a double-click
                }

                lastClickTime = currentClickTime; // Update the time of the last click

                if (clickCount == 1)
                {
                    // Check if the mouse is inside the rectangle
                    Rect rectBounds = new Rect(Canvas.GetLeft(_ROI), Canvas.GetTop(_ROI), _ROI.Width, _ROI.Height);

                    if (rectBounds.Contains(mousePosition))
                    {
                        if (_isEditing == true && ROI != _ROI) return;
                        _isDragging = true;
                        _ROI.CaptureMouse();
                        startPoint = mousePosition;
                    }
                }
                else if (clickCount >= 2)
                {
                    if (_isEditing == false)
                    {
                        ROI = _ROI;
                        _isEditing = true;
                        ShowPopUp(_ROI);
                        ImageEditorHelper.RemoveResizeHandles(canvas);
                        ImageEditorHelper.AddResizeHandles(_ROI, canvas);
                        btnAdd.IsEnabled = false;
                        dgROIs.IsEnabled = false;
                        FillClear();
                        _ROI.Fill = ROIFillSelected;
                        dgROIs.SelectedItem = _ROI;
                        dgROIs.ScrollIntoView(dgROIs.SelectedItem);

                    }
                }
            };

            _ROI.MouseMove += (s, e) =>
            {
                if (_isAdding == true || _isDragging == false || ImageEditorHelper._isEditingROIHandle == true) return;
                currentClickTime = DateTime.Now;
                if ((currentClickTime - lastClickTime).TotalMilliseconds <= clickThreshold/2) return;
                if (_isDragging)
                {
                    if (e.LeftButton != MouseButtonState.Pressed)
                        return;
                    Point currentMousePosition = e.GetPosition(canvas);

                    double newX = currentMousePosition.X - startPoint.X;
                    double newY = currentMousePosition.Y - startPoint.Y;

                    var newLeft = Canvas.GetLeft(_ROI) + newX;
                    var newTop = Canvas.GetTop(_ROI) + newY;

                    double imageWidth = imgDoorTrim.ActualWidth;
                    double imageHeight = imgDoorTrim.ActualHeight;

                    double roiWidth = _ROI.Width;
                    double roiHeight = _ROI.Height;

                    newLeft = Math.Max(0, Math.Min(newLeft, imageWidth - roiWidth));
                    newTop = Math.Max(0, Math.Min(newTop, imageHeight - roiHeight));

                    Canvas.SetLeft(_ROI, newLeft);
                    Canvas.SetTop(_ROI, newTop);

                    startPoint = currentMousePosition;
                    ImageEditorHelper.OnRoiUpdated += CtrlRecipeWizardROISetup_OnRoiUpdated;

                    if (_isEditing == true)
                    {
                        RemovePopup();
                        ShowPopUp(_ROI);
                        ImageEditorHelper.RemoveResizeHandles(canvas);
                        ImageEditorHelper.AddResizeHandles(_ROI, canvas);
                    }
                }
            };

            _ROI.MouseLeftButtonUp += (s, e) =>
            {
                if (_isAdding == true || _isDragging == false || ImageEditorHelper._isEditingROIHandle == true) return;
                _ROI.ReleaseMouseCapture();
                _isDragging = false;
            };

            _ROI.MouseEnter += (s, e) =>
            {
                if (_isAdding == true || _isEditing == true || ImageEditorHelper._isEditingROIHandle == true) return;
                _ROI.Stroke = System.Windows.Media.Brushes.Blue;
            };
            _ROI.MouseLeave += (s, e) =>
            {
                if (_isAdding == true || _isEditing == true || ImageEditorHelper._isEditingROIHandle == true) return;
                _ROI.Stroke = Brushes.Yellow;
            };
        }

        public void FillClear()
        {
            foreach (var roi in ROIs) roi.Fill = ROIFill;
        }

        private void CtrlRecipeWizardROISetup_OnRoiUpdated(DetectionROIDetailsUI roi)
        {
            var selectedROI = ROIs.FirstOrDefault(r => r.roi_ui_counter == roi.roi_ui_counter);

            if (selectedROI != null)
            {
                selectedROI.start_x = roi.start_x;
                selectedROI.start_y = roi.start_y;
                selectedROI.end_x = roi.end_x;
                selectedROI.end_y = roi.end_y;
            }
        }

        private void CopyRectangle()
        {
            if (ROI != null && _isAdding == false && _isAddEdit == false && _isCopying == false)
            {
                DrawROI(ROI, true);
                _isAdding = false;
                _isEditing = true;
                _isCopying = true;
                _isAddEdit = true;
                canvas.Children.Remove(PopUp);
                ShowPopUp(ROI);
                ImageEditorHelper.RemoveResizeHandles(canvas);
                ImageEditorHelper.OnRoiUpdated += CtrlRecipeWizardROISetup_OnRoiUpdated;
                ImageEditorHelper.AddResizeHandles(ROI, canvas);
                FillClear();
                ROI.Fill = ROIFillSelected;
            }
        }

        private void ClearDisabled()
        {
            if (_isAddEdit == true || _isCopying == true)
            {
                ROIs.Remove(ROI);
                canvas.Children.Remove(ROI);
                RemoveRoiName(ROI);
            }
            StopAddEdit();
            RemovePopup();
            ClearCheckedCheckboxes();
            ClearGroupForm();
            btnAdd.IsEnabled = true;
            dgROIs.IsEnabled = true;
            btnRemoveGroup.IsEnabled = false;
            stkPanelAddGroup.Visibility = Visibility.Collapsed;
            ImageEditorHelper.RemoveResizeHandles(canvas);
        }

        public void RemoveROI()
        {
            if (ROI != null)
            {
                var roiID = ROI.detection_roi_ID != 0 ? ROI.detection_roi_ID : 0;
                ROIs.Remove(ROI);
                RemovePopup();
                RemoveRoiName(ROI);
                canvas.Children.Remove(ROI);
                ImageEditorHelper.RemoveResizeHandles(canvas);
                StopAddEdit();
                LoadROIsList();
                DOOR_TRIM_INSPECTION.Machine.hmcDBHelper.DeleteDetectionROI(roiID);
            }
        }

        public void CancelROI()
        {
            if (ROI != null)
            {
                RemovePopup();
                ImageEditorHelper.RemoveResizeHandles(canvas);
                StopAddEdit();
            }
        }

        public void StopAddEdit()
        {
            _isAdding = false;
            _isDragging = false;
            _isEditing = false;
            _isAddEdit = false;
            _isCopying = false;
            _isFromPrevious = false;
            ImageEditorHelper._isEditingROIHandle = false;
            ROI = null;
            btnAdd.IsEnabled = true;
            dgROIs.IsEnabled = true;
            FillClear();
        }

        public void RemovePopup()
        {
            canvas.Children.Remove(PopUp);
        }

        public void RemoveRoiName(DetectionROIDetailsUI ROI)
        {
            if (ROI == null) return;
            if (roiTextBlocks.ContainsKey(ROI.roi_ui_counter))
            {
                TextBlock textBlockToRemove = roiTextBlocks[ROI.roi_ui_counter];

                canvas.Children.Remove(textBlockToRemove);

                //roiTextBlocks.Remove(ROI.roi_ui_counter);
            }
        }

        //draw roi name
        public void RoiName(DetectionROIDetailsUI ROI)
        {
            // Create a TextBlock to display the name and value near the rectangle
            TextBlock textBlock = new TextBlock
            {
                Text = $"{ROI.roi_name}",
                Foreground = System.Windows.Media.Brushes.White,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(120, 0, 0, 0)),
                FontSize = fs
            };

            if (!roiTextBlocks.ContainsKey(ROI.roi_ui_counter))
                roiTextBlocks.Add(ROI.roi_ui_counter, textBlock);
            else
            {
                RemoveRoiName(ROI);
                roiTextBlocks[ROI.roi_ui_counter] = textBlock;
            }
            TextPosition(ROI);
        }
        
        // Position the TextBlock near the rectangle
        public void TextPosition(DetectionROIDetailsUI ROI, int p = 0)
        {
            if(ROI != null)
            {
                if(roiTextBlocks.ContainsKey(ROI.roi_ui_counter))
                {
                    int position;
                    TextBlock textBlock = roiTextBlocks[ROI.roi_ui_counter];

                    if(p == 0)
                    {
                        position = ROI.roi_name_location;
                        RemoveRoiName(ROI);
                    }
                    else
                    {
                        position = p;
                        RemoveRoiName(ROI);
                    }

                    if (position == 1) // TL, T
                    {
                        Canvas.SetLeft(textBlock, ROI.start_x + 4);
                        Canvas.SetTop(textBlock, ROI.start_y - fs - (fs / 2));
                    }
                    else if (position == 2) // TR, T
                    {
                        Canvas.SetLeft(textBlock, ROI.end_x + 4- textBlock.ActualWidth);
                        Canvas.SetTop(textBlock, ROI.start_y - fs - (fs / 2));
                    }
                    else if (position == 3) // TR, R
                    {
                        Canvas.SetLeft(textBlock, ROI.end_x + 4);
                        Canvas.SetTop(textBlock, ROI.start_y);
                    }
                    else if (position == 4) // BR, R
                    {
                        Canvas.SetLeft(textBlock, ROI.end_x + 4);
                        Canvas.SetTop(textBlock, ROI.end_y - textBlock.ActualHeight);
                    }
                    else if (position == 5) // BR, B
                    {
                        Canvas.SetLeft(textBlock, ROI.end_x + 4 - textBlock.ActualWidth);
                        Canvas.SetTop(textBlock, ROI.end_y);
                    }
                    else if (position == 6) // BL, B
                    {
                        Canvas.SetLeft(textBlock, ROI.start_x + 4);
                        Canvas.SetTop(textBlock, ROI.end_y);
                    }
                    else if (position == 7) // BL, L
                    {
                        Canvas.SetLeft(textBlock, ROI.start_x - 4 - textBlock.ActualWidth);
                        Canvas.SetTop(textBlock, ROI.end_y - textBlock.ActualHeight);
                    }
                    else if (position == 8)  // TL, L
                    {
                        Canvas.SetLeft(textBlock, ROI.start_x - 4 - textBlock.ActualWidth);
                        Canvas.SetTop(textBlock, ROI.start_y);
                    }
                    canvas.Children.Add(textBlock);
                    if (p != 0)
                        ROIs.FirstOrDefault(r => r.roi_ui_counter == ROI.roi_ui_counter).roi_name_location = position;
                }
            }
        }
        #endregion

        private void BtnModifyGroup_Click(object sender, RoutedEventArgs e)
        {
            stkPanelAddGroup.Visibility = selectedROIsForChk.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            _isAddEdit = true;
            btnAdd.IsEnabled = false;
            dgROIs.IsEnabled = false;
            btnRemoveGroup.IsEnabled = false;
            foreach(var roi in selectedROIsForChk)
            {
                if(roi.ALC_NAME != "")
                {
                    cboALCCode.SelectedItem = cboALCCode.Items.Cast<KeyValuePair<string, string>>().FirstOrDefault(d=>d.Key == roi.ALC_CODE);
                    cboALCCode.IsEnabled = false;
                    txtALCName.Text = roi.ALC_NAME;
                    txtALCName.IsEnabled = false;
                    txtGroupName.Text = roi.group_name;
                    break;
                }
            }
        }
    }
}
