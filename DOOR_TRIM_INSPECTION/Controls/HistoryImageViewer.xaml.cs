using DOOR_TRIM_INSPECTION.Class;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ViDi2;

namespace DOOR_TRIM_INSPECTION.Controls
{

    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class HistoryImageViewer : UserControl
    {
        public string RearImagePath { get; set; }
        public string FrontImagePath { get; set; }
        public int RecipeID { get; set; }
        private Canvas canvas = null;

        private double ScreenRatio = .2;
        private System.Windows.Point startPoint;
        private System.Windows.Point origin;
        private double scaleFactor;
        public HistoryImageViewer()
        {
            InitializeComponent();

            canvas = DrawingCanvasRear;
            //canvas zoom pan action set
            canvas.MouseWheel += ZoomPan_MouseWheel;
            canvas.MouseLeftButtonDown += ZoomPan_MouseLeftButtonDown;
            canvas.MouseLeftButtonUp += ZoomPan_MouseLeftButtonUp;
            canvas.MouseMove += ZoomPan_MouseMove;
            canvas.PreviewMouseRightButtonDown += ZoomPan_PreviewMouseRightButtonDown;
            canvas.Loaded += ZoomPan_Loaded;

            // resize and center the image at the beginning
            if (canvas is FrameworkElement fe) fe.Loaded += (s, e) => zoomBorder.ImageLoad(canvas);

            reSizeFrontImg(0.14);
        }

        public void Clear()
        {
            imgRear.Source = null;
            imgFront.Source = null;
        }

        public void ZoomClear()
        {
            zoomBorder.Reset();
        }

        public void LoadHistoryImages(int RecipeID, string RearImagePath, string FrontImagePath)
        {
            this.RecipeID = RecipeID;
            this.FrontImagePath = FrontImagePath;
            this.RearImagePath = RearImagePath;

            Bitmap bitmapRear = new Bitmap(RearImagePath);
            Bitmap bitmapFront = new Bitmap(FrontImagePath);

            var imgPathRear = MainUIHelper.LoadImageWpf(bitmapRear);
            var imgPathFront = MainUIHelper.LoadImageWpf(bitmapFront);


            imgRear.Source = imgPathRear;
            imgFront.Source = imgPathFront;
            CalculateScaleFactor(imgPathFront);
            //reSizeFrontImg(.035);
            //zoomBorder.Reset();
            DoEvents();
        }
        public static void DoEvents()
        {
            var frame = new System.Windows.Threading.DispatcherFrame();
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => frame.Continue = false));
            System.Windows.Threading.Dispatcher.PushFrame(frame);
        }
        private void reSizeFrontImg(double sf)
        {
            

            ScaleTransform scaleTransform = new ScaleTransform();
            scaleFactor = sf; //.035; // increase size to render and save big image
            scaleTransform.ScaleX = scaleFactor;
            scaleTransform.ScaleY = scaleFactor;

            DrawingCanvasFront.RenderTransform = scaleTransform;
        }
        private void CalculateScaleFactor(BitmapImage bitmapImage)
        {
            // BitmapImage의 원본 크기
            double imageWidth = bitmapImage.PixelWidth;
            double imageHeight = bitmapImage.PixelHeight;

            // DrawingCanvasFront의 실제 크기
            double canvasWidth = DrawingCanvasFront.ActualWidth;
            double canvasHeight = DrawingCanvasFront.ActualHeight;

            // X축과 Y축 배율 계산
            double scaleX = canvasWidth / imageWidth;
            double scaleY = canvasHeight / imageHeight;

            // 두 축 중 작은 값을 선택하여 scaleFactor로 사용
            double scaleFactor = Math.Min(scaleX, scaleY);

            reSizeFrontImg(scaleFactor);
        }


        private void ZoomPan_Loaded(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomOutViewCentered();
        }

        private void ZoomPan_MouseMove(object sender, MouseEventArgs e)
        {
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

        private void ZoomPan_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            zoomBorder.Reset();
        }

        private void ZoomPan_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (canvas != null)
            {
                canvas.ReleaseMouseCapture();
                zoomBorder.Cursor = Cursors.Arrow;
            }
        }

        private void ZoomPan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
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

        private void imgFront_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Check mouse double-click
            if (e.ClickCount == 2)
            {
                //MainUIHelper.FrontImageHandle(borderFront, DrawingCanvasFront, imgFront);
                MainUIHelper.FrontImageHandle(borderFront, DrawingCanvasFront, imgFront, scaleFactor); 
            }
        }


        public void LoadImages(string RearImagePath, string FrontImagePath, Inspection inspection, string DoorTrimID)
        {
            if (RearImagePath == null || FrontImagePath == null)
                return;
            if (RearImagePath.Length == 0 || FrontImagePath.Length == 0)
                return;
                this.RecipeID = RecipeID;
            this.FrontImagePath = FrontImagePath;
            this.RearImagePath = RearImagePath;

            //Recipe CurrentRecipe = new Recipe(RecipeID);
            //Inspection inspection = new Inspection(CurrentRecipe);
            //inspection.SetRearInspectionImage(RearImagePath);
            //inspection.ExecuteRearInspection();
            Mat MatRear = MainUIHelper.DrawRearInspectionResult(inspection.RearInspectionResult, new Bitmap(RearImagePath), DrawingCanvasRear, RecipeID, true);
            Bitmap bitmapRear = BitmapConverter.ToBitmap(MatRear);

            //inspection.SetFrontInspectionImage(FrontImagePath);
            //inspection.ExecuteFrontInspection();
            Mat MatFront = MainUIHelper.DrawFrontInspectionResult(inspection.FrontInspectionResult, new Bitmap(FrontImagePath), DrawingCanvasFront, RecipeID);

            Bitmap bitmapFront = BitmapConverter.ToBitmap(MatFront);

            // SAVE THESE IMAGES FOR HISTORY MEER 2025.01.24
            //if(BarcodeReadTime != DateTime.MinValue)
            //    SaveHistoryImage(DoorTrimID, BarcodeReadTime, MatRear, MatFront);


            var imgPathRear = MainUIHelper.LoadImageWpf(bitmapRear);
            var imgPathFront = MainUIHelper.LoadImageWpf(bitmapFront);

            if (inspection.FrontInspectionResultCode != INSPECTION_RESULT.OK)
                borderFront.BorderBrush = System.Windows.Media.Brushes.Red;
            else
                borderFront.BorderBrush = System.Windows.Media.Brushes.GreenYellow;

            imgRear.Source = imgPathRear;
            imgFront.Source = imgPathFront;

            //CalculateScaleFactor(imgPathFront);
            //reSizeFrontImg(.035);
            //zoomBorder.Reset();
            //DoEvents();

            reSizeFrontImg(0.035);
            zoomBorder.Reset();
            MainUIHelper.FrontImageHandle(borderFront, DrawingCanvasFront, imgFront);
            // both of this line is needed to set it small and put it on a corner
            MainUIHelper.FrontImageHandle(borderFront, DrawingCanvasFront, imgFront);
        }

    }
}
