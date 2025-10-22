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
    public partial class CustomImagePickerBox : Window
    {
        private string ImageFolder = Machine.config.setup.ImagePath; // GET IT FROM MACHINE

      
        public string SelectedFilePath;
       
        public CustomImagePickerBox()
        {
            InitializeComponent();
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
                bool isTemp = false;
                string temp = "";
                if (System.IO.Directory.Exists(SearchPath))
                {
                    if (rdoRear.IsChecked == true)
                    {
                        temp = System.IO.Path.Combine(SearchPath, "result_rear.png");
                        if (System.IO.Directory.Exists(SearchPath))
                            isTemp = true;
                            imageFiles = GetImagesByPattern(SearchPath, @"^rear_\d{6}$").ToList();
                    }
                    else if (rdoFront.IsChecked == true)
                    {
                        temp = System.IO.Path.Combine(SearchPath, "result_front.png");
                        if (System.IO.Directory.Exists(SearchPath))
                            isTemp = true;
                            imageFiles = GetImagesByPattern(SearchPath, @"^front_\d{6}$").ToList();
                    }
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

                    SelectedFilePath = imageFiles[0];
                    if (isTemp)
                    {
                        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(temp);
                        imgPreview.Source = LoadImageWpf(bitmap);
                    }
                    else
                    {
                        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(SelectedFilePath);
                        imgPreview.Source = LoadImageWpf(bitmap);
                    }
                }
            }
        }

       
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

                    SelectedFilePath = oriPath;
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(SelectedFilePath);
                    imgPreview.Source = LoadImageWpf(bitmap);
                }
            }
        }

        private void BtnPickImage_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
