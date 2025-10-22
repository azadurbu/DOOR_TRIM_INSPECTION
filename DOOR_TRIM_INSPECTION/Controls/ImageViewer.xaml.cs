using DOOR_TRIM_INSPECTION.Class;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DOOR_TRIM_INSPECTION.Controls
{

    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        public string RearImagePath { get; set; }
        public string FrontImagePath { get; set; }
        public System.Drawing.Size RearImageSize { get; set; }     // Sacle 적용된 이미지 크기
        public System.Drawing.Size FrontImageSize { get; set; }    // Sacle 적용된 이미지 크기

        public int RecipeID { get; set; }
        private Canvas canvas = null;

        private double ScreenRatio = .2;
        private System.Windows.Point startPoint;
        private System.Windows.Point origin;
        private double scaleFactor;
        public ImageViewer()
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
        }

        private void imgFront_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Check mouse double-click
            if (e.ClickCount == 2)
            {
                MainUIHelper.FrontImageHandle(borderFront, DrawingCanvasFront, imgFront);
            }
        }

        bool rearLoaded = false, frontLoaded = false;

        private void SaveHistoryImage(string DoorTrimID, DateTime BarcodeReadTime)
        {
            string imageSaveFolder = Machine.config.setup.ImagePath;
            string frontFileName = "result_front.png";
            string rearFileName = "result_rear.png";
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string Month = BarcodeReadTime.ToString("MM");
            string Date = BarcodeReadTime.ToString("dd");

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Month);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Date);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, DoorTrimID + "_" + BarcodeReadTime.ToString("HHmmss"));
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string rearFilePath = System.IO.Path.Combine(imageSaveFolder, rearFileName);
            string frontFilePath = System.IO.Path.Combine(imageSaveFolder, frontFileName);
            SaveCanvasImageForHistory(rearFilePath, frontFilePath);
        }

        private void SaveCanvasImageForHistory(string rearFilePath, string frontFilePath)
        {
            RenderTargetBitmap frontImage = new RenderTargetBitmap(
                (int)(imgFront.ActualWidth), (int)(imgFront.ActualHeight),
                96d, 96d, PixelFormats.Pbgra32);

            ((System.Windows.Media.ScaleTransform)DrawingCanvasFront.RenderTransform).ScaleX = 1;
            ((System.Windows.Media.ScaleTransform)DrawingCanvasFront.RenderTransform).ScaleY = 1;
            // Canvas 내용을 렌더링
            frontImage.Render(DrawingCanvasFront);

            // 1/4로 축소된 비트맵 생성
            double scaleFactor = 0.25; // 축소 비율
            TransformedBitmap scaledImage = new TransformedBitmap(frontImage, new ScaleTransform(scaleFactor, scaleFactor));

            using (FileStream fileStreamF = new FileStream(frontFilePath, FileMode.Create))
            {
                // PngBitmapEncoder를 사용하여 이미지 저장
                PngBitmapEncoder encoder = new PngBitmapEncoder();

                // scaledImage를 BitmapFrame으로 추가
                encoder.Frames.Add(BitmapFrame.Create(scaledImage));

                // 파일 스트림을 통해 저장
                encoder.Save(fileStreamF);
            }

            //using (FileStream fileStreamF = new FileStream(frontFilePath, FileMode.Create))
            //{
            //    //BmpBitmapEncoder encoder = new BmpBitmapEncoder(); // Use BmpBitmapEncoder instead of PngBitmapEncoder
            //    //encoder.Frames.Add(BitmapFrame.Create(frontImage));
            //    //encoder.Save(fileStreamF);

            //    PngBitmapEncoder encoder = new PngBitmapEncoder();
            //    encoder.Frames.Add(BitmapFrame.Create(frontImage));
            //    encoder.Save(fileStreamF);
            //}


            // Canvas 크기만큼 Bitmap 생성
            RenderTargetBitmap rearImage = new RenderTargetBitmap(
                (int)zoomBorder.ActualWidth, (int)zoomBorder.ActualHeight,
                96d, 96d, PixelFormats.Pbgra32);

            // Canvas 내용을 렌더링
            rearImage.Render(zoomBorder);

            //Rear 
            // Bitmap을 PNG 파일로 저장
            using (FileStream fileStreamR = new FileStream(rearFilePath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rearImage));
                encoder.Save(fileStreamR);
                //BmpBitmapEncoder encoder = new BmpBitmapEncoder(); // Use BmpBitmapEncoder instead of PngBitmapEncoder
                //encoder.Frames.Add(BitmapFrame.Create(rearImage));
                //encoder.Save(fileStreamR);
            }

            MainUIHelper.FrontImageHandle(borderFront, DrawingCanvasFront, imgFront);
            MainUIHelper.FrontImageHandle(borderFront, DrawingCanvasFront, imgFront);
            borderFront.Margin = new Thickness(20);
        }



        private void testTargetSize(int targetWidth, int targetHeight)
        {


            // Create a RenderTargetBitmap with the desired size
            RenderTargetBitmap testImage = new RenderTargetBitmap(
                targetWidth, targetHeight,
                96d, 96d, PixelFormats.Pbgra32);

            // Create a ScaleTransform to scale the content to the target size
            double scaleX = (double)targetWidth / zoomBorder.ActualWidth;
            double scaleY = (double)targetHeight / zoomBorder.ActualHeight;

            // Apply the scale transformation to the Canvas or the content inside the ZoomBorder
            ScaleTransform scaleTransform = new ScaleTransform(scaleX, scaleY);
            ScaleTransform scaleTransformBack = zoomBorder.GetScaleTransform(DrawingCanvasRear);
            zoomBorder.RenderTransform = scaleTransform;

            // Render the DrawingVisual to the RenderTargetBitmap
            testImage.Render(zoomBorder);

            try
            {
                // Save the rendered Bitmap to a file
                using (FileStream fileStream = new FileStream(@"D:\\test.png", FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(testImage));
                    encoder.Save(fileStream);
                }
                DrawingCanvasRear.RenderTransform = scaleTransformBack;
            }
            catch (System.Exception ex)
            {
                // Handle any errors that may occur during the file saving
                MessageBox.Show($"Error saving file: {ex.Message}");
            }
        }

        public void LoadImage(Mat RearImagePath)
        {
            this.RecipeID = RecipeID;

            Mat MatRear = new Mat();
            Mat temp = RearImagePath.Clone();
            Cv2.Rotate(temp, MatRear, OpenCvSharp.RotateFlags.Rotate180);
            Bitmap bitmapRear = BitmapConverter.ToBitmap(MatRear);

            var imgPathRear = MainUIHelper.LoadImageWpf(bitmapRear);

            imgRear.Source = imgPathRear;
            imgFront.Source = null;
            reSizeFrontImg(1);
            zoomBorder.Reset();

            // 강제로 UI 업데이트
            DoEvents();
        }

        public void LoadImages(string RearImagePath, string FrontImagePath, Inspection inspection, string DoorTrimID, DateTime BarcodeReadTime)
        {
            this.RecipeID = RecipeID;
            this.FrontImagePath = FrontImagePath;
            this.RearImagePath = RearImagePath;

            //Recipe CurrentRecipe = new Recipe(RecipeID);
            //Inspection inspection = new Inspection(CurrentRecipe);
            //inspection.SetRearInspectionImage(RearImagePath);
            //inspection.ExecuteRearInspection();
            Mat MatRear = MainUIHelper.DrawRearInspectionResult(inspection.RearInspectionResult, new Bitmap(RearImagePath), DrawingCanvasRear, RecipeID, true);
#if PROFILE_OUTPUT
            string imageSaveFolder = "D:\\PLUG_PROFILER_OUTPUT";

            string Month = BarcodeReadTime.ToString("MM");
            string Date = BarcodeReadTime.ToString("dd");

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Month);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Date);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, "OLD");
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            foreach (var InspectionResult in inspection.RearInspectionResult.PlugMatchInspectionResult)
            {
                List<int> OldLeadwireIDs = new List<int>() { 1584, 1903, 1910, 1919, 1911, 1915 };//1581, 1857, 1864, 1886, 3124, 3106, 3133, 3105
                if (OldLeadwireIDs.Contains(InspectionResult.RegionID))
                {
                    // SAVE IMAGE
                    string filePath = System.IO.Path.Combine(imageSaveFolder, DoorTrimID + "_" + BarcodeReadTime.ToString("HHmmss") + "_" + InspectionResult.RegionID + ".bmp");

                    Cv2.ImWrite(filePath, InspectionResult.ProcessedImageRegion);
                }
            }
#if USE_COGNEX
            imageSaveFolder = "D:\\PLUG_PROFILER_OUTPUT";
            // SAVE IMAGE
            Month = BarcodeReadTime.ToString("MM");
            Date = BarcodeReadTime.ToString("dd");

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Month);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Date);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, "NEW");
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            foreach (var InspectionResult in inspection.RearInspectionResult.PlugCognexInspectionResult)
            {
                List<int> NewLeadwireIDs = new List<int>() { 4160, 4161, 4162, 4164, 4163, 4165 };//1581, 1857, 1864, 1886, 3124, 3106, 3133, 3105
                if (NewLeadwireIDs.Contains(InspectionResult.RegionID))
                {
                    string filePath = System.IO.Path.Combine(imageSaveFolder, DoorTrimID + "_" + BarcodeReadTime.ToString("HHmmss") + "_" + InspectionResult.RegionID + ".bmp");

                    Cv2.ImWrite(filePath, InspectionResult.ProcessedImageRegion);
                }
            }
#endif
#endif

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


            imgRear.Source = imgPathRear;
            imgFront.Source = imgPathFront;
            reSizeFrontImg(0.035);
            zoomBorder.Reset();

            // 강제로 UI 업데이트
            DoEvents();

            if (Machine.InspectionMode == INSPECTION_MODE.DEFAULT)
            {
                string MargeFRImgPath = SaveCanvasBitmap();
                //string MargeFRImgPath = SaveCanvasBitmap_(MatFront,MatRear);
                SaveHistoryImage(DoorTrimID, BarcodeReadTime);

                //SaveMargeFrontRearImage(BitmapConverter.ToMat(bitmapFront), BitmapConverter.ToMat(bitmapRear));
                Machine.dyDBHelper.SaveVisionResult(Machine.BarcodeData, MargeFRImgPath, inspection.RearInspectionResultCode == INSPECTION_RESULT.OK && inspection.FrontInspectionResultCode == INSPECTION_RESULT.OK, BarcodeReadTime);
                Machine.dyDBHelper.SaveCheckHistory(Machine.BarcodeData, inspection.RearInspectionResultCode == INSPECTION_RESULT.OK && inspection.FrontInspectionResultCode == INSPECTION_RESULT.OK, BarcodeReadTime);
            }
            else if (Machine.InspectionMode == INSPECTION_MODE.MASTER)
            {
                try
                {
                    string MargeFRImgPath = SaveCanvasBitmapMasterInspMode();
                    SaveMasterInspectionImage(DoorTrimID);
                    string OriginalBarcode = Machine.BarcodeData.Barcode;
                    string BarCodeWithDateTime = $"{OriginalBarcode}_{BarcodeReadTime.ToString("yyMMddHHmmss")}";
                    Machine.BarcodeData.Barcode = BarCodeWithDateTime;
                    Machine.dyDBHelper.SaveVisionResult(Machine.BarcodeData, MargeFRImgPath, inspection.RearInspectionResultCode == INSPECTION_RESULT.OK && inspection.FrontInspectionResultCode == INSPECTION_RESULT.OK, BarcodeReadTime);
                    Machine.BarcodeData.Barcode = OriginalBarcode;
                    Machine.dyDBHelper.SaveCheckHistory(Machine.BarcodeData, inspection.RearInspectionResultCode == INSPECTION_RESULT.OK && inspection.FrontInspectionResultCode == INSPECTION_RESULT.OK, BarcodeReadTime); // TEMPORARY OFF, DUE TO FIELD SIZE
                }
                catch (Exception ex)
                {
                   
                }
            }
        }

        private string MasterImagePath
        {
            get
            {
                string path = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                         "images\\MasterModeImages");
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);
                return path;
            }
        }

        private void SaveMasterInspectionImage(string DoorTrimID)
        {
            string imageSaveFolder = Path.Combine(MasterImagePath, "Results");
            string frontFileName = $"result_{DoorTrimID}_front.png";
            string rearFileName = $"result_{DoorTrimID}_rear.png";
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string rearFilePath = System.IO.Path.Combine(imageSaveFolder, rearFileName);
            string frontFilePath = System.IO.Path.Combine(imageSaveFolder, frontFileName);
            SaveCanvasImageForHistory(rearFilePath, frontFilePath);
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
            reSizeFrontImg(.035);
            zoomBorder.Reset();
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

        public string SaveMargeFrontRearImage(Mat FrontImg, Mat RearImg)
        {
            int frontW = (int)(FrontImg.Width / (RearImg.Width / (double)380));
            int frontH = (int)(FrontImg.Height / (RearImg.Width / (double)380));
            int rearW = (int)(RearImg.Width / (RearImg.Width / (double)1920));
            int rearH = (int)(RearImg.Height / (RearImg.Width / (double)1920));

            int x = rearW - frontW;
            int y = 0;

            Mat FrontTempImg = new Mat();
            Cv2.Resize(FrontImg, FrontTempImg, new OpenCvSharp.Size(frontW, frontH));
            Mat RearTempImg = new Mat();
            Cv2.Resize(RearImg, RearTempImg, new OpenCvSharp.Size(rearW, rearH));

            FrontTempImg.CopyTo(new Mat(RearTempImg, new OpenCvSharp.Rect(x, y, frontW, frontH)));
            Cv2.Rectangle(RearTempImg, new OpenCvSharp.Rect(x, y, frontW, frontH), Scalar.White, 4);

            string imageSaveFolder = Machine.config.setup.ImagePath;

            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            DateTime _barcodeReadTime = Machine.sequence.BarcodeReadTime;
            string Month = _barcodeReadTime.ToString("MM");
            string Date = _barcodeReadTime.ToString("dd");

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Month);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Date);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Machine.BarcodeData.Barcode + "_" + _barcodeReadTime.ToString("HHmmss"));
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string filePath = System.IO.Path.Combine(imageSaveFolder, "toDB_");
            filePath += "_" + _barcodeReadTime.ToString("HHmmss") + ".bmp";

            Cv2.ImWrite(filePath, RearTempImg);
            return filePath;
        }


        public void LoadImages(Mat RearImageMat, Mat FrontImageMat, int RecipeID)
        {
            this.RecipeID = RecipeID;
            Bitmap FrontImageBmp = BitmapConverter.ToBitmap(FrontImageMat);
            Bitmap RearImageBmp = BitmapConverter.ToBitmap(RearImageMat);

            Recipe CurrentRecipe = new Recipe(RecipeID);
            Inspection inspection = new Inspection(CurrentRecipe);
            inspection.SetRearInspectionImage(RearImageMat);
            inspection.ExecuteRearInspection();
            //Mat MatRear = MainUIHelper.DrawRearInspectionResult(inspection.RearInspectionResult,RearImageBmp, DrawingCanvasRear);
            //Bitmap bitmapRear = BitmapConverter.ToBitmap(MatRear);

            inspection.SetFrontInspectionImage(FrontImageMat);
            inspection.ExecuteFrontInspection();
            //Mat MatFront = MainUIHelper.DrawFrontInspectionResult(inspection.FrontInspectionResult, FrontImageBmp, DrawingCanvasFront);
            //Bitmap bitmapFront = BitmapConverter.ToBitmap(MatFront);

            var imgPathRear = MainUIHelper.LoadImageWpf(RearImageBmp);
            var imgPathFront = MainUIHelper.LoadImageWpf(FrontImageBmp);
            imgRear.Source = imgPathRear;
            imgFront.Source = imgPathFront;
            zoomBorder.Reset();
            reSizeFrontImg(1);
        }

        public void ZoomClear()
        {
            zoomBorder.Reset();
        }

        public void Clear()
        {
            for (int i = DrawingCanvasRear.Children.Count - 1; i >= 0; i--)
            {
                if (DrawingCanvasRear.Children[i] is TextBlock)
                {
                    DrawingCanvasRear.Children.RemoveAt(i);
                }
                else if (DrawingCanvasRear.Children[i] is System.Windows.Shapes.Rectangle)
                {
                    DrawingCanvasRear.Children.RemoveAt(i);
                }
            }

            for (int i = DrawingCanvasFront.Children.Count - 1; i >= 0; i--)
            {
                if (DrawingCanvasFront.Children[i] is TextBlock)
                {
                    DrawingCanvasFront.Children.RemoveAt(i);
                }
                else if (DrawingCanvasFront.Children[i] is System.Windows.Shapes.Rectangle)
                {
                    DrawingCanvasFront.Children.RemoveAt(i);
                }
            }

            imgRear.Source = null;
            imgFront.Source = null;
        }


        public string SaveCanvasBitmap()
        {
            // Canvas 크기만큼 Bitmap 생성
            RenderTargetBitmap baseImage = new RenderTargetBitmap(
                (int)zoomBorder.ActualWidth, (int)zoomBorder.ActualHeight,
                96d, 96d, PixelFormats.Pbgra32);

            // Canvas 내용을 렌더링
            baseImage.Render(zoomBorder);

            RenderTargetBitmap overlayImage = new RenderTargetBitmap(
                (int)DrawingCanvasFront.ActualWidth, (int)DrawingCanvasFront.ActualHeight,
                96d, 96d, PixelFormats.Pbgra32);

            // Canvas 내용을 렌더링
            overlayImage.Render(DrawingCanvasFront);
            double scaleFactor = 0.035; //.035; // increase size to render and save big image
            if (DrawingCanvasFront.ActualWidth < 300)
                scaleFactor = 1;
            TransformedBitmap scaledImage = new TransformedBitmap(overlayImage, new ScaleTransform(scaleFactor, scaleFactor));

            // DrawingVisual 및 DrawingContext 생성
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                // 기본 이미지 그리기
                context.DrawImage(baseImage, new System.Windows.Rect(0, 0, baseImage.PixelWidth, baseImage.PixelHeight));
                //SolidBrush brush = new SolidBrush(System.Drawing.Color.GreenYellow);
                System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                context.DrawRectangle(brush, new System.Windows.Media.Pen(brush, 0), new System.Windows.Rect(baseImage.PixelWidth - scaledImage.PixelWidth - 10, 5, scaledImage.PixelWidth + 5, scaledImage.PixelHeight + 5));
                // 겹칠 이미지 그리기 (위치 및 크기 조정 가능)
                context.DrawImage(scaledImage, new System.Windows.Rect(baseImage.PixelWidth - scaledImage.PixelWidth - 10, 5, scaledImage.PixelWidth, scaledImage.PixelHeight));
            }

            // RenderTargetBitmap에 렌더링
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                baseImage.PixelWidth, baseImage.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(visual);


            string imageSaveFolder = Machine.config.setup.ImagePath;

            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            DateTime _barcodeReadTime = Machine.sequence.BarcodeReadTime;
            string Month = _barcodeReadTime.ToString("MM");
            string Date = _barcodeReadTime.ToString("dd");

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Month);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Date);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Machine.BarcodeData.Barcode + "_" + _barcodeReadTime.ToString("HHmmss"));
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string filePath = System.IO.Path.Combine(imageSaveFolder, "toDB_");
            //filePath += "_" + _barcodeReadTime.ToString("HHmmss") + ".png";
            filePath += "_" + _barcodeReadTime.ToString("HHmmss") + ".jpg";

            // Bitmap을 PNG 파일로 저장
            //using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            //{
            //    PngBitmapEncoder encoder = new PngBitmapEncoder();
            //    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            //    encoder.Save(fileStream);
            //}
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.QualityLevel = 100; // 
                jpegEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                jpegEncoder.Save(fileStream);
            }
            return filePath;

            //MessageBox.Show("Canvas 이미지가 저장되었습니다: CanvasImage.png");
        }

        public string SaveCanvasBitmapMasterInspMode()
        {
            // Canvas 크기만큼 Bitmap 생성
            RenderTargetBitmap baseImage = new RenderTargetBitmap(
                (int)zoomBorder.ActualWidth, (int)zoomBorder.ActualHeight,
                96d, 96d, PixelFormats.Pbgra32);

            // Canvas 내용을 렌더링
            baseImage.Render(zoomBorder);

            RenderTargetBitmap overlayImage = new RenderTargetBitmap(
                (int)DrawingCanvasFront.ActualWidth, (int)DrawingCanvasFront.ActualHeight,
                96d, 96d, PixelFormats.Pbgra32);

            // Canvas 내용을 렌더링
            overlayImage.Render(DrawingCanvasFront);
            double scaleFactor = 0.035; //.035; // increase size to render and save big image
            if (DrawingCanvasFront.ActualWidth < 300)
                scaleFactor = 1;
            TransformedBitmap scaledImage = new TransformedBitmap(overlayImage, new ScaleTransform(scaleFactor, scaleFactor));

            // DrawingVisual 및 DrawingContext 생성
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                // 기본 이미지 그리기
                context.DrawImage(baseImage, new System.Windows.Rect(0, 0, baseImage.PixelWidth, baseImage.PixelHeight));
                //SolidBrush brush = new SolidBrush(System.Drawing.Color.GreenYellow);
                System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                context.DrawRectangle(brush, new System.Windows.Media.Pen(brush, 0), new System.Windows.Rect(baseImage.PixelWidth - scaledImage.PixelWidth - 10, 5, scaledImage.PixelWidth + 5, scaledImage.PixelHeight + 5));
                // 겹칠 이미지 그리기 (위치 및 크기 조정 가능)
                context.DrawImage(scaledImage, new System.Windows.Rect(baseImage.PixelWidth - scaledImage.PixelWidth - 10, 5, scaledImage.PixelWidth, scaledImage.PixelHeight));
            }

            // RenderTargetBitmap에 렌더링
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                baseImage.PixelWidth, baseImage.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(visual);


            string imageSaveFolder = Machine.config.setup.ImagePath;

            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string filePath = System.IO.Path.Combine(imageSaveFolder, "temp_master_image.jpg");
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.QualityLevel = 100; // 
                jpegEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                jpegEncoder.Save(fileStream);
            }
            return filePath;

            //MessageBox.Show("Canvas 이미지가 저장되었습니다: CanvasImage.png");
        }

        public string SaveCanvasBitmap_(Mat FrontImg, Mat RearImg)
        {
            OpenCvSharp.Size ScaleSize = new OpenCvSharp.Size(1520, 895);

            int FrontSacleWidth = 250;
            int ErarSacleHeight = 895;

            // Create a gray background image
            Mat grayBackground = new Mat(ScaleSize, MatType.CV_8UC3, new Scalar(128, 128, 128));

            // Resize FrontImg to the specified width
            double frontAspectRatio = (double)FrontImg.Height / FrontImg.Width;
            int frontHeight = (int)(FrontSacleWidth * frontAspectRatio);
            Mat resizedFrontImg = new Mat();
            Cv2.Resize(FrontImg, resizedFrontImg, new OpenCvSharp.Size(FrontSacleWidth, frontHeight));

            // Resize RearImg to the specified height
            double rearAspectRatio = (double)RearImg.Width / RearImg.Height;
            int rearWidth = (int)(ErarSacleHeight * rearAspectRatio);
            Mat resizedRearImg = new Mat();
            Cv2.Resize(RearImg, resizedRearImg, new OpenCvSharp.Size(rearWidth, ErarSacleHeight));

            // Draw resizedRearImg at (0, 0) on grayBackground
            resizedRearImg.CopyTo(new Mat(grayBackground, new OpenCvSharp.Rect(0, 0, resizedRearImg.Width, resizedRearImg.Height)));

            // Draw resizedFrontImg at (grayBackground.Width - resizedFrontImg.Width - 10, 0) on grayBackground
            resizedFrontImg.CopyTo(new Mat(grayBackground, new OpenCvSharp.Rect(grayBackground.Width - resizedFrontImg.Width - 10, 0, resizedFrontImg.Width, resizedFrontImg.Height)));

            string imageSaveFolder = Machine.config.setup.ImagePath;

            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            DateTime _barcodeReadTime = Machine.sequence.BarcodeReadTime;
            string Month = _barcodeReadTime.ToString("MM");
            string Date = _barcodeReadTime.ToString("dd");

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Month);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Date);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Machine.BarcodeData.Barcode + "_" + _barcodeReadTime.ToString("HHmmss"));
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string filePath = System.IO.Path.Combine(imageSaveFolder, "toDB_");
            filePath += "_" + _barcodeReadTime.ToString("HHmmss") + ".png";

            Cv2.ImWrite(filePath, grayBackground);
            return filePath;

            //MessageBox.Show("Canvas 이미지가 저장되었습니다: CanvasImage.png");
        }

        private void reSizeFrontImg(double sf) // AD 2025.01.31
        {
            if (sf == 1)
                borderFront.Margin = new Thickness(0, 0, -1000, 0);
            else
                borderFront.Margin = new Thickness(20);

            ScaleTransform scaleTransform = new ScaleTransform();
            scaleFactor = sf; //.035; // increase size to render and save big image
            scaleTransform.ScaleX = scaleFactor;
            scaleTransform.ScaleY = scaleFactor;

            DrawingCanvasFront.RenderTransform = scaleTransform;
        }

        #region canvas zoom pan events
        private void ZoomPan_Loaded(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomOutViewCentered();
        }

        private void ZoomPan_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //zoomBorder.Reset();
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
        #endregion
    }
}
