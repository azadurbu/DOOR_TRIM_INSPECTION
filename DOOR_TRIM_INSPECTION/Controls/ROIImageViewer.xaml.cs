using OpenCvSharp;
using OpenCvSharp.Extensions;
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
    /// Interaction logic for ROIImageViewer.xaml
    /// </summary>
    public partial class ROIImageViewer : UserControl
    {
        private bool _isAdding;
        private bool _isDrag;
        private bool _isDragging;

        private Canvas canvas = null;
        private double ScreenRatio = .2;
        private System.Windows.Point startPoint;

        private System.Windows.Point origin;
        private SolidColorBrush ROIBorderColor = Brushes.White;

        private SolidColorBrush ROIFill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 255, 255, 40));
        private SolidColorBrush ROIFillSelected = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 255, 255, 0));

        private SolidColorBrush SelectedBtnColor = new SolidColorBrush(Color.FromArgb(255, 241, 240, 232));
        private SolidColorBrush BtnColor = new SolidColorBrush(Color.FromArgb(255, 179, 200, 207));

        private bool _isEditing;


        private Rectangle ROI = null;

        public string SelectedROI
        {
            get { return ROI == null ? "0,0,0,0" :
                    $"{(int)Canvas.GetLeft(ROI)},{(int)Canvas.GetTop(ROI)},{(int)ROI.ActualWidth},{(int)ROI.ActualHeight}"; }
        }
        public ROIImageViewer()
        {
            InitializeComponent();
          

            canvas = zbImgCanvas;

            //canvas zoom pan action set
            canvas.MouseWheel += ZoomPan_MouseWheel;
            canvas.MouseLeftButtonDown += ZoomPan_MouseLeftButtonDown;
            canvas.MouseLeftButtonUp += ZoomPan_MouseLeftButtonUp;
            canvas.MouseMove += ZoomPan_MouseMove;
            canvas.PreviewMouseRightButtonDown += ZoomPan_PreviewMouseRightButtonDown;
            canvas.Loaded += ZoomPan_Loaded;

            //rectangle drawing action set
            canvas.MouseLeftButtonDown += Canvas_MouseDown;
            canvas.MouseLeftButtonUp += Canvas_MouseUp;
            canvas.MouseMove += Canvas_MouseMove;

            // resize and center the image at the beginning
            if (canvas is FrameworkElement fe) fe.Loaded += (s, e) => zoomBorder.ImageLoad(canvas);

        }

        public void SetROI(string strROI)
        {
            string[] literals = strROI.Split(',');
            if (literals.Length == 4)
            {
                if (ROI != null)
                {
                    canvas.Children.Remove(ROI);
                    ROI = null;
                }

                ROI = new Rectangle()
                {
                    Fill = ROIFill,
                    Stroke = ROIBorderColor,
                    StrokeThickness = 2,
                };

                Canvas.SetLeft(ROI,double.Parse(literals[0]));
                Canvas.SetTop(ROI, double.Parse(literals[1]));
                ROI.Width = double.Parse(literals[2]);
                ROI.Height = double.Parse(literals[3]);
                canvas.Children.Add(ROI);
                //AddROIEvent(ROI);
            }
        }
        public void SetImage(string ImagePath)
        {
            Mat temp = Cv2.ImRead(ImagePath);
            if(ImagePath.Contains("sub_1"))
                temp = LevelOps.EqualizeHistColor(temp);
            img.Source = LoadImageWpf(BitmapConverter.ToBitmap(temp));
        }

         public static BitmapImage LoadImageWpf(System.Drawing.Bitmap temp)
        {
            BitmapImage bitmapImage;
            using (var memoryStream = new System.IO.MemoryStream())
            {
                temp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;

                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }

        #region canvas zoom pan events
        private void ZoomPan_Loaded(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomOutViewCentered();
        }

        private void ZoomPan_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            zoomBorder.Reset();
        }

        private void ZoomPan_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isAdding || _isDrag) return;
            if (canvas != null)
            {
                Canvas childCanvas = (Canvas)canvas;
                if (canvas.IsMouseCaptured)
                {
                    var tt = zoomBorder.GetTranslateTransform(canvas);
                    System.Windows.Vector v = startPoint - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                    zoomBorder.ConstrainTranslationToBounds();
                }
            }
        }

        private void ZoomPan_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isAdding || _isDrag) return;
            if (canvas != null)
            {
                canvas.ReleaseMouseCapture();
                zoomBorder.Cursor = Cursors.Arrow;
            }
        }

        private void ZoomPan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDrag) return;

            if (canvas != null)
            {
                var tt = zoomBorder.GetTranslateTransform(canvas);
                startPoint = e.GetPosition(this);
                origin = new System.Windows.Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                canvas.CaptureMouse();
            }
        }

        private void ZoomPan_MouseWheel(object sender, MouseWheelEventArgs e)
        {
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

                System.Windows.Point relative = e.GetPosition(canvas);

                double absoluteX = relative.X * st.ScaleX + tt.X;
                double absoluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = absoluteX - relative.X * st.ScaleX;
                tt.Y = absoluteY - relative.Y * st.ScaleY;
            }
        }
        #endregion

        private void BtnDraw_Click(object sender, RoutedEventArgs e)
        {
            DrawROICanvas();

        }

        private void DrawROICanvas()
        {
            if (_isEditing && _isAdding) return;
            _isAdding = true;
        }

        #region rectangle drawing event
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isAdding == false || _isDragging) return;

            if (_isAdding == true)
            {
                startPoint = e.GetPosition(canvas);
                if (ROI != null)
                {
                    canvas.Children.Remove(ROI);
                    ROI = null;
                }

                ROI = new Rectangle()
                {
                    Fill = ROIFill,
                    Stroke = ROIBorderColor,
                    StrokeThickness = 2,
                };

                Canvas.SetLeft(ROI, startPoint.X);
                Canvas.SetTop(ROI, startPoint.Y);
                canvas.Children.Add(ROI);
                AddROIEvent(ROI);
                canvas.CaptureMouse();

            }
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || ROI == null
                || _isAdding == false || _isDragging == true) return;

            var currentPoint = e.GetPosition(canvas);

            double imageWidth = img.ActualWidth;
            double imageHeight = img.ActualHeight;

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
            if (_isAdding == false || _isDragging) return;

            AddROIEvent(ROI);
            _isAdding = false;
            _isEditing = true;
            canvas.ReleaseMouseCapture();
        }

        //double click detection var
        private void AddROIEvent(Rectangle _ROI)
        {
            _ROI.MouseLeftButtonDown += (s, e) =>
            {
                if (_isAdding || !_isDrag) return;
                if (e.ClickCount == 1)
                {
                    _ROI.CaptureMouse();
                    startPoint = e.GetPosition(canvas);
                    _isDragging = true;
                }
            };

            _ROI.MouseMove += (s, e) =>
            {
                if (_isDragging)
                {
                    System.Windows.Point mousePosition = e.GetPosition(canvas);

                    double newX = mousePosition.X - startPoint.X;
                    double newY = mousePosition.Y - startPoint.Y;

                    var newLeft = Canvas.GetLeft(_ROI) + newX;
                    var newTop = Canvas.GetTop(_ROI) + newY;

                    double imageWidth = img.ActualWidth;
                    double imageHeight = img.ActualHeight;

                    double roiWidth = _ROI.Width;
                    double roiHeight = _ROI.Height;

                    newLeft = Math.Max(0, Math.Min(newLeft, imageWidth - roiWidth));
                    newTop = Math.Max(0, Math.Min(newTop, imageHeight - roiHeight));

                    Canvas.SetLeft(_ROI, newLeft);
                    Canvas.SetTop(_ROI, newTop);

                    startPoint = mousePosition;
                }
            };

            _ROI.MouseLeftButtonUp += (s, e) =>
            {
                _ROI.ReleaseMouseCapture();
                _isDragging = false;
            };

            _ROI.MouseEnter += (s, e) =>
            {
                if (_isAdding == true) return;
                _ROI.Fill = ROIFillSelected;
                _isDrag = true;
            };

            _ROI.MouseLeave += (s, e) =>
            {
                _isDrag = false;
                _ROI.Fill = ROIFill;
            };
        }
        #endregion

    }
}
