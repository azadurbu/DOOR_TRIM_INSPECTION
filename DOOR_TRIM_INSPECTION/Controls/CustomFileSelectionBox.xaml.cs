using DOOR_TRIM_INSPECTION.Class;
using System;
using System.Collections.Generic;
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

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CustomFileSelectionBox.xaml
    /// </summary>
    public partial class CustomFileSelectionBox : Window
    {
        private string ImageFolder = Machine.config.setup.ImagePath; // GET IT FROM MACHINE

        private int start_x;
        private int start_y;
        private int end_x;
        private int end_y;

        public Color DetectedColor;
       
        public CustomFileSelectionBox()
        {
            InitializeComponent();
            SetROI();
        }

        public void SetROI()
        {
            OpenCvSharp.Rect SelectedRect = ((App)Application.Current).SelectedROI;
            this.start_x = SelectedRect.X;
            this.start_y = SelectedRect.Y;
            this.end_x = SelectedRect.X + SelectedRect.Width;
            this.end_y = SelectedRect.Y + SelectedRect.Height;
        }

        private void TxtDoorTrimID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtDoorTrimID.Text.Length > 5)
            {
                List<InspectionResult> results = Machine.hmcDBHelper.GetInspectionResultsByDoorTrim(txtDoorTrimID.Text);
                if (results.Count == 0)
                {
                    results.Add(new InspectionResult() { DoorTrimID = "No Results Found!" });
                }
                lstDoorTrims.ItemsSource = results;
                lstDoorTrims.DisplayMemberPath = "DoorTrimID";
                lstDoorTrims.SelectedValuePath = "DoorTrimID";
            }
        }

        private string[] GetImagesByPattern(string folderPath, string pattern)
        {
            Regex regex = new Regex(pattern);
            return System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly)
                            .Where(file => regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(file)))
                            .ToArray();
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

        private void LstDoorTrims_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InspectionResult inspItem = lstDoorTrims.SelectedItem as InspectionResult;
            if (inspItem != null)
            {
                string day = inspItem.InspectionTime.ToString("dd");
                string month = inspItem.InspectionTime.ToString("MM");
                //string year = inspItem.InspectionTime.ToString("yyyy");
                string barcodeScanTime = inspItem.InspectionTime.ToString("HHmmss");
                string SearchPath = System.IO.Path.Combine(ImageFolder, /*year,*/ month, day, inspItem.DoorTrimID + "_" + barcodeScanTime);

                List<string> imageFiles = new List<string>();

                if (System.IO.Directory.Exists(SearchPath))
                {
                    
                    if (rdoRear.IsChecked == true)
                        imageFiles = GetImagesByPattern(SearchPath, @"^rear_\d{6}$").ToList();
                    else if (rdoFront.IsChecked == true)
                        imageFiles = GetImagesByPattern(SearchPath, @"^front_\d{6}$").ToList();

                }

                if (imageFiles.Count == 0)
                {
                    Console.WriteLine($"Rear 또는 Front 이미지가 없습니다: {SearchPath}");
                    Machine.logger.Write(eLogType.INFORMATION, $"Rear 또는 Front 이미지가 없습니다: {SearchPath}");
                    MessageBox.Show("Can't find image.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    OpenCvSharp.Mat matImage = OpenCvSharp.Cv2.ImRead(imageFiles[0], OpenCvSharp.ImreadModes.Color);
                    RunColorInspection(matImage);
                    OpenCvSharp.Cv2.Rectangle(matImage, new OpenCvSharp.Rect(this.start_x, this.start_y, this.end_x- this.start_x, this.end_y- this.start_y), OpenCvSharp.Scalar.FromRgb(0, 255, 0), 8);
                    System.Drawing.Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(matImage);
                    imgPreview.Source = LoadImageWpf(bitmap);
                }
            }
        }

        private void RunColorInspection(OpenCvSharp.Mat matImage)
        {
            if (this.start_x == 0 && this.start_y == 0 && this.end_x == 0 && this.end_y == 0)
            {
                DetectedColor = Color.FromRgb(0,0,0);
                rectImageColorPicker.Fill = new SolidColorBrush(DetectedColor);
                txtImageColorPicker.Text = $"{DetectedColor.R},{DetectedColor.G},{DetectedColor.B}";
            }

            DetectionROIDetails roiDetails = new DetectionROIDetails() { start_x = this.start_x, start_y = this.start_y, end_x = this.end_x, end_y = this.end_y, ALC_CODE="", ALC_NAME="" };
            List<DetectionROIDetails> roiList = new List<DetectionROIDetails>() { roiDetails };
            ColorInspection cInsp = new ColorInspection(matImage, roiList); // MEER 2025.05.27 REMOVING TRIAL INSPECTION FLAG
            cInsp.Execute();
            DetectedColor = cInsp.ColorInspectionItems[0].Color;
            rectImageColorPicker.Fill = new SolidColorBrush(DetectedColor);
            txtImageColorPicker.Text = $"{DetectedColor.R},{DetectedColor.G},{DetectedColor.B}";
        }

        private void BtnColorPicker_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        //public static bool Show(int startX, int startY, int endX, int endY, Window that)
        //{
        //    var dialog = new CustomFileSelectionBox(startX, startY, endX, endY);
        //    dialog.Owner = that;
        //    return dialog.ShowDialog() == true;
        //}

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp";

                System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // COPY FILE TO THE APPLICATION FOLDER // MEER 2025.01.15
                    string oriPath = openFileDialog.FileName;
                    OpenCvSharp.Mat matImage = OpenCvSharp.Cv2.ImRead(oriPath, OpenCvSharp.ImreadModes.Color);
                    RunColorInspection(matImage);
                    OpenCvSharp.Cv2.Rectangle(matImage, new OpenCvSharp.Rect(this.start_x, this.start_y, this.end_x - this.start_x, this.end_y - this.start_y), OpenCvSharp.Scalar.FromRgb(0, 255, 0), 8);
                    System.Drawing.Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(matImage);
                    imgPreview.Source = LoadImageWpf(bitmap);
                }
            }
        }
    }
}
