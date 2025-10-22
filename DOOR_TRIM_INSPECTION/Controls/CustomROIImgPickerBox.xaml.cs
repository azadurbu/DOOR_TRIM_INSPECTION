using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Controls.FormControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CustomFileSelectionBox.xaml
    /// </summary>
    public partial class CustomROIImgPickerBox : Window
    {
        private string ImageFolder = Machine.config.setup.ImagePath; // GET IT FROM MACHINE
        public string SelectedFilePath;
        public string SelectedROI { get; set; }

        // drawing on image
        private WriteableBitmap writeableBitmap;
        private int width = 500;
        private int height = 500;
        private bool isDrawing = true;
        private bool isErasing = false;
        private bool isClicked = false;
        private bool isCircle = false;
        private Point previousPoint;
        private int brushSize = 5;
        private Stack<WriteableBitmap> undoStack = new Stack<WriteableBitmap>(); // Undo stack
        private Color brushColor = Colors.Black;
        private Color LastbrushColor = Colors.Black;

        public String TemplatePath = "";

        public CustomROIImgPickerBox()
        {
            InitializeComponent();

            // Initialize WriteableBitmap and canvas
            writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            ImageBrush brush = new ImageBrush(writeableBitmap);
            canvas.Background = brush;

            // Set the Canvas size to match the WriteableBitmap size
            canvas.Width = width;
            canvas.Height = height;
            canvasImage.Width = width;
            canvasImage.Height = height;

            // Add mouse event for drawing or erasing
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseUp += Canvas_MouseUp;
        }
        #region Crop Image
        public void SetFilePath(string filePath)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
            if (fi.Exists)
            {
                string oriPath = filePath;

                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(filePath);
                roiImageViewer.SetImage(filePath);
            }
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

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp";
                openFileDialog.InitialDirectory = ImageFolder;
                System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // COPY FILE TO THE APPLICATION FOLDER // MEER 2025.01.15
                    string oriPath = openFileDialog.FileName;

                    SelectedFilePath = oriPath;
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(SelectedFilePath);
                    roiImageViewer.SetImage(SelectedFilePath);
                }
            }
        }

        private void BtnPickROI_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Shapes.Rectangle roi = roiImageViewer.ROI;
            SelectedROI = roiImageViewer.SelectedROI;
            if (SelectedROI == "0,0,0,0")
                MessageBox.Show("ROI를 선택해 주세요");
            else
                DialogResult = true;
            //DialogResult = SelectedROI != "0,0,0,0";
        }

        private void btnPickROICropImg_Click(object sender, RoutedEventArgs e)
        {
            Canvas canvas = roiImageViewer.zbImgCanvas;
            Rectangle cropArea = canvas.Children.OfType<Rectangle>().FirstOrDefault();

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();

            //recipe image if not set any image
            SelectedFilePath = SelectedFilePath == null ? PopUpInputRow.SelectedFilePath : SelectedFilePath;

            bitmap.UriSource = new Uri(SelectedFilePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            // Create the Image control
            Image imageControl = new Image
            {
                Source = bitmap,
                Stretch = Stretch.Uniform // Or Stretch.None if you want 1:1 pixel mapping
            };
            BitmapSource croppedImage = CropImageFromCanvas(imageControl, cropArea);

            if(croppedImage != null)
            {
                SaveCroppedImgAsBmp(croppedImage);
                CropImage.Visibility = Visibility.Collapsed;
                DrawOnImage.Visibility = Visibility.Visible;
                LoadCroppedImg(croppedImage);
            }
            
        }

        public void SaveCroppedImgAsBmp(BitmapSource image)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"crop_img_{timestamp}.bmp";
            string fullPath = Environment.CurrentDirectory + "\\images\\TryCrop\\" + fileName;

            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }


        #region helper function
        public BitmapSource CropImageFromCanvas(Image imageControl, Rectangle selectionRect)
        {
            if(selectionRect == null)
            {

                return null;
            }


            // Get the rectangle's position and size
            double x = Canvas.GetLeft(selectionRect);
            double y = Canvas.GetTop(selectionRect);
            double width = selectionRect.Width;
            double height = selectionRect.Height;

            // Ensure positions are not NaN
            if (double.IsNaN(x)) x = 0;
            if (double.IsNaN(y)) y = 0;

            var bitmap = imageControl.Source as BitmapSource;
            if (bitmap == null) return null;

            // Get the position of the image in the canvas
            double imageX = Canvas.GetLeft(imageControl);
            double imageY = Canvas.GetTop(imageControl);

            if (double.IsNaN(imageX)) imageX = 0;
            if (double.IsNaN(imageY)) imageY = 0;

            // Translate rectangle coordinates relative to the image
            int cropX = (int)(x - imageX);
            int cropY = (int)(y - imageY);
            int cropWidth = (int)width;
            int cropHeight = (int)height;

            // Ensure the crop rectangle is within bounds
            cropX = Math.Max(0, Math.Min(cropX, bitmap.PixelWidth - 1));
            cropY = Math.Max(0, Math.Min(cropY, bitmap.PixelHeight - 1));
            cropWidth = Math.Min(cropWidth, bitmap.PixelWidth - cropX);
            cropHeight = Math.Min(cropHeight, bitmap.PixelHeight - cropY);

            // Create and return the cropped bitmap
            var cropped = new CroppedBitmap(bitmap, new Int32Rect(cropX, cropY, cropWidth, cropHeight));
            return cropped;
        }

        #endregion
        #endregion


        #region Draw on Image
        #region canvas actiuon
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var startPoint = e.GetPosition(canvas);
            // Map the canvas coordinates to the WriteableBitmap coordinates
            double scaleX = writeableBitmap.PixelWidth / canvas.ActualWidth;
            double scaleY = writeableBitmap.PixelHeight / canvas.ActualHeight;

            int bitmapX = (int)(startPoint.X * scaleX);
            int bitmapY = (int)(startPoint.Y * scaleY);

            if (isErasing)
            {
                isClicked = true;
                ErasePixel(bitmapX, bitmapY);
                SaveUndoState();
            }
            else if (isDrawing)
            {
                isClicked = true;
                previousPoint = new Point(bitmapX, bitmapY);
                DrawLineOnBitmap(previousPoint, new Point(bitmapX, bitmapY));
                SaveUndoState();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var startPoint = e.GetPosition(canvas);
            double scaleX = writeableBitmap.PixelWidth / canvas.ActualWidth;
            double scaleY = writeableBitmap.PixelHeight / canvas.ActualHeight;

            int bitmapX = (int)(startPoint.X * scaleX);
            int bitmapY = (int)(startPoint.Y * scaleY);

            if (isDrawing && isClicked)
            {
                DrawLineOnBitmap(previousPoint, new Point(bitmapX, bitmapY));
                previousPoint = new Point(bitmapX, bitmapY);
            }
            else if (isErasing && isClicked)
            {
                ErasePixel(bitmapX, bitmapY);
            }
        }

        // Mouse up event to stop drawing
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isClicked = false;
        }
        #endregion


        #region form button action
        // Load Image Button Click
        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif";

            if (openFileDialog.ShowDialog() == true)
            {

                var bitmap = new System.Windows.Media.Imaging.BitmapImage(new Uri(openFileDialog.FileName));

                if (bitmap.Height > bitmap.Width)
                {
                    double fixedHeight = 500;
                    double originalWidth = bitmap.PixelWidth;
                    double originalHeight = bitmap.PixelHeight;
                    double newWidth = (fixedHeight / originalHeight) * originalWidth;

                    // Set the image source and size
                    canvasImage.Source = bitmap;
                    canvasImage.Width = newWidth;
                    canvasImage.Height = fixedHeight;
                }
                else
                {
                    // Desired fixed width
                    double fixedWidth = 500;

                    // Calculate the proportional height
                    double originalWidth = bitmap.PixelWidth;
                    double originalHeight = bitmap.PixelHeight;
                    double newHeight = (fixedWidth / originalWidth) * originalHeight;

                    // Set the image source and size
                    canvasImage.Source = bitmap;
                    canvasImage.Width = fixedWidth;
                    canvasImage.Height = newHeight;

                }
                canvas.Height = canvasImage.Height;
                canvas.Width = canvasImage.Width;
            }
        }


        BitmapImage SourceCropedImg;

        //load image 
        private void LoadCroppedImg(BitmapSource FileName)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Encode the BitmapSource into the memory stream
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(FileName));
                encoder.Save(memoryStream);

                // Create a new BitmapImage from the stream
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();
                bitmap.Freeze(); // Optional: makes it cross-thread accessible                
                SourceCropedImg = bitmap;
                //BitmapImage bitmap = new System.Windows.Media.Imaging.BitmapImage(new Uri(FileName));

                if (bitmap.Height > bitmap.Width)
                {
                    double fixedHeight = 600;
                    double originalWidth = bitmap.PixelWidth;
                    double originalHeight = bitmap.PixelHeight;
                    double newWidth = (fixedHeight / originalHeight) * originalWidth;

                    // Set the image source and size
                    canvasImage.Source = bitmap;
                    canvasImage.Width = newWidth;
                    canvasImage.Height = fixedHeight;
                }
                else
                {
                    // Desired fixed width
                    double fixedWidth = 600;

                    // Calculate the proportional height
                    double originalWidth = bitmap.PixelWidth;
                    double originalHeight = bitmap.PixelHeight;
                    double newHeight = (fixedWidth / originalWidth) * originalHeight;

                    // Set the image source and size
                    canvasImage.Source = bitmap;
                    canvasImage.Width = fixedWidth;
                    canvasImage.Height = newHeight;

                }
                canvas.Height = canvasImage.Height;
                canvas.Width = canvasImage.Width;
            }
        }
        // Save Button Click
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (canvasImage.Source == null || canvasImage.ActualWidth == 0 || canvasImage.ActualHeight == 0)
                return;

            RenderTargetBitmap renderBitmap1 = new RenderTargetBitmap((int)canvasImage.ActualWidth, (int)canvasImage.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderBitmap1.Render(canvasImage);

            Canvas canvas2 = canvas;
            RenderTargetBitmap renderBitmap2 = new RenderTargetBitmap((int)canvas2.ActualWidth,
                (int)canvas2.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);

            canvas2.Measure(new Size(canvas2.ActualWidth, canvas2.ActualHeight));
            canvas2.Arrange(new Rect(new Size(canvas2.ActualWidth, canvas2.ActualHeight)));
            renderBitmap2.Render(canvas2);

            var dg = new DrawingGroup();

            var id1 = new ImageDrawing(renderBitmap1, new Rect(0, 0, renderBitmap1.Width, renderBitmap1.Height));
            var id2 = new ImageDrawing(renderBitmap2, new Rect(0, 0, renderBitmap2.Width, renderBitmap2.Height));

            dg.Children.Add(id1);
            dg.Children.Add(id2);

            RenderTargetBitmap combinedImg = new RenderTargetBitmap((int)Math.Max(renderBitmap1.Width, renderBitmap2.Width),
                (int)Math.Max(renderBitmap1.Height, renderBitmap2.Height), 96, 96, PixelFormats.Pbgra32);


            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawDrawing(dg);
            }
            combinedImg.Render(dv);

            //resize same as croped image

            double targetWidth = SourceCropedImg.PixelWidth;
            double targetHeight = SourceCropedImg.PixelHeight;

            RenderTargetBitmap resizedCombinedImg = new RenderTargetBitmap(
                (int)targetWidth,
                (int)targetHeight,
                96, 96,
                PixelFormats.Pbgra32);

            // Scale the combined image using a DrawingVisual
            var dvResized = new DrawingVisual();
            using (var dc = dvResized.RenderOpen())
            {
                dc.DrawImage(combinedImg, new Rect(0, 0, targetWidth, targetHeight));
            }
            resizedCombinedImg.Render(dvResized);

            string filePath = PopUpInputRow.existingTemplate;
            bool ConvertTo24Bit = true;
            if (ConvertTo24Bit)
            {
                // Convert to 24-bit BMP (Bgr24 removes alpha)
                FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap();
                convertedBitmap.BeginInit();
                convertedBitmap.Source = resizedCombinedImg;
                convertedBitmap.DestinationFormat = PixelFormats.Bgr24;
                convertedBitmap.EndInit();

                // Save path handling
                string directory = System.IO.Path.GetDirectoryName(filePath);
                string filenameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(filePath);
                string newFileName = $"{filenameWithoutExt}_{DateTime.Now:yyyyMMdd_HHmmss}.bmp";
                string copyFilePath = System.IO.Path.Combine(directory, newFileName);

                // Save 24-bit BMP copy
                BmpBitmapEncoder bmpEncoder = new BmpBitmapEncoder();
                bmpEncoder.Frames.Add(BitmapFrame.Create(convertedBitmap));
                using (FileStream fs = new FileStream(copyFilePath, FileMode.Create))
                {
                    bmpEncoder.Save(fs);
                }
                bmpEncoder = new BmpBitmapEncoder();
                bmpEncoder.Frames.Add(BitmapFrame.Create(convertedBitmap));
                // Save to original path
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    bmpEncoder.Save(fs);
                }


            }
            else
            {

                //SaveImage(combinedImg);
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(resizedCombinedImg));

                // save a copy of the file
                string directory = System.IO.Path.GetDirectoryName(filePath);
                string filenameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(filePath);
                string extension = System.IO.Path.GetExtension(filePath);
                string newFileName = $"{filenameWithoutExt}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}{extension}";

                string copyFilePath = System.IO.Path.Combine(directory, newFileName);
                //copy file save
                using (FileStream fs = new FileStream(copyFilePath, FileMode.Create)) { encoder.Save(fs); }
                //file save
                BitmapEncoder encoder1 = new PngBitmapEncoder();
                encoder1.Frames.Add(BitmapFrame.Create(resizedCombinedImg));
                using (FileStream fs = new FileStream(filePath, FileMode.Create)) { encoder1.Save(fs); }
            }

            TemplatePath = filePath;
            DialogResult = true;
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (undoStack.Count > 0)
            {
                writeableBitmap = undoStack.Pop();
                canvas.Background = new ImageBrush(writeableBitmap);
            }
        }

        // Toggle Erase Mode (Erase button click event)
        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isErasing)
            {
                isErasing = false;
                isDrawing = true;
                EraseButton.Content = "Erase";
                brushColor = LastbrushColor;
            }
            else if (isDrawing)
            {
                isErasing = true;
                isDrawing = false;
                EraseButton.Content = "Draw";
                brushColor = Colors.Transparent;
            }
        }

        // Toggle Circle Mode (Circle button click event)
        private void CircleButton_Click(object sender, RoutedEventArgs e)
        {
            if (isCircle)
            {
                isCircle = false;
                CircleButton.Content = "Circle";
            }
            else
            {
                isCircle = true;
                CircleButton.Content = "Rectangle";
            }
        }

        private void btnBlackBrush_Click(object sender, RoutedEventArgs e)
        {
            brushColor = Colors.Black;
            LastbrushColor = brushColor;
            isDrawing = true;
            isErasing = false;
            EraseButton.Content = "Erase";
        }

        private void btnWhiteBrush_Click(object sender, RoutedEventArgs e)
        {
            brushColor = Colors.White;
            LastbrushColor = brushColor;
            isDrawing = true;
            isErasing = false;
            EraseButton.Content = "Erase";
        }

        private void brushSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brushSize = (int)e.NewValue;
        }
        #endregion


        #region helper functions
        // Save the current canvas state to the undo stack
        private void SaveUndoState()
        {
            var copyBitmap = new WriteableBitmap(writeableBitmap);
            undoStack.Push(copyBitmap);
        }

        private void SaveImage(RenderTargetBitmap renderTargetBitmap)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG Image(*.jpg)|*.jpg|BMP Image (*.bmp*)|*.bmp|PNG Image(*.png)|*.png|GIF Image(*.gif)|*.gif";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
        }

        private void DrawLineOnBitmap(Point start, Point end)
        {
            int x1 = (int)start.X;
            int y1 = (int)start.Y;
            int x2 = (int)end.X;
            int y2 = (int)end.Y;

            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = (x1 < x2) ? 1 : -1;
            int sy = (y1 < y2) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (isCircle)
                    DrawCircle(x1, y1);
                else
                    DrawRectangle(x1, y1);

                if (x1 == x2 && y1 == y2) break;
                int e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        private void ErasePixel(int x, int y)
        {
            if (isCircle)
                DrawCircle(x, y);
            else
                DrawRectangle(x, y);
        }

        private void DrawRectangle(int x1, int y1)
        {
            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    int dxAdjusted = x1 + i;
                    int dyAdjusted = y1 + j;

                    if (dxAdjusted >= 0 && dxAdjusted < writeableBitmap.PixelWidth &&
                        dyAdjusted >= 0 && dyAdjusted < writeableBitmap.PixelHeight)
                    {
                        SetPixel(dxAdjusted, dyAdjusted, brushColor);
                    }
                }
            }
        }

        private void DrawCircle(int centerX, int centerY)
        {
            int radius = brushSize;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        int x = centerX + dx;
                        int y = centerY + dy;

                        if (x >= 0 && x < writeableBitmap.PixelWidth && y >= 0 && y < writeableBitmap.PixelHeight)
                        {
                            SetPixel(x, y, brushColor);
                        }
                    }
                }
            }
        }

        private void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= writeableBitmap.PixelWidth || y < 0 || y >= writeableBitmap.PixelHeight)
                return;

            int stride = writeableBitmap.PixelWidth * 4; // 4 bytes per pixel (BGRA)
            int index = (y * stride) + (x * 4);

            byte[] pixels = new byte[4];
            pixels[0] = color.B;
            pixels[1] = color.G;
            pixels[2] = color.R;
            pixels[3] = color.A;

            writeableBitmap.WritePixels(new Int32Rect(x, y, 1, 1), pixels, stride, 0);
        }
        #endregion

        #endregion

    }
}
