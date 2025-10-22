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
    public partial class CustomROIPickerBox : Window
    {
        private string ImageFolder = Machine.config.setup.ImagePath; // GET IT FROM MACHINE

        public string SelectedFilePath;

        

        public string SelectedROI { get; set; }

       
        public CustomROIPickerBox()
        {
            InitializeComponent();
        }

        public void SetROI(string strROI)
        {
            roiImageViewer.SetROI(strROI);
        }

        public void SetFilePath(string filePath)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
            if (fi.Exists)
            {
                SelectedFilePath = filePath;
                
                roiImageViewer.SetImage(SelectedFilePath);
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
    }
}
