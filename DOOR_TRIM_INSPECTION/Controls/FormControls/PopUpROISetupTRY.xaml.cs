using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DOOR_TRIM_INSPECTION.Class;
using OpenCvSharp;

namespace DOOR_TRIM_INSPECTION.Controls.FormControls
{
    /// <summary>
    /// Interaction logic for PopUpROISetupTRY.xaml
    /// </summary>
    public partial class PopUpROISetupTRY : System.Windows.Controls.UserControl
    {
        private DetectionROIDetailsUI ROI;
        private new CtrlRecipeWizardROISetup Parent;

        public PopUpROISetupTRY()
        {
            InitializeComponent();
        }

        public void SetParam(DetectionROIDetailsUI roi, CtrlRecipeWizardROISetup parent)
        {
            ROI = roi;
            Parent = parent;
            LoadParameterView(ROI.Parameters);
            
            ImageLoad(0);
            ImageLoad(1);
            ImageLoad(2);
            ImageLoad(3);

        }




        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp";

                DialogResult result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    RecipeDB recipe = Machine.hmcDBHelper.GetRecipe(ROI.recipe_ID);
                    string RootDestPath = System.IO.Path.Combine(recipe.RecipeName);

                    if (!Directory.Exists(RootDestPath))
                    {
                        Directory.CreateDirectory(RootDestPath);
                    }
                    string copyFilePath = System.IO.Path.Combine(RootDestPath, $"TryImage {DateTime.Now:yy-MM-dd_HH-mm-ss}.bmp");

                    File.Copy(filePath, copyFilePath, true);

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.EndInit();
                    
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Parent.DrawingCanvas.Children.Remove(Parent.PopUp.background);
            Parent.DrawingCanvas.Children.Remove(this);
            Parent._isZoomPanDisabled = false;
        }

        private void LoadParameterView(string parameters)
        {
            stkParameters.Children.Clear();
            List<AlgorithmParamOption> algoParams = GetParameters(parameters);
            foreach (AlgorithmParamOption algoParam in algoParams)
            {
                PopUpInputRow popUpInput = new PopUpInputRow();
                popUpInput.setInfo(algoParam.OptionName, algoParam.Value);
                stkParameters.Children.Add(popUpInput);
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

        #region helper

        private Mat ConvertBitmapImageToMat(BitmapImage bitmapImage)
        {
            MemoryStream memoryStream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(memoryStream);

            byte[] byteArray = memoryStream.ToArray();
            Mat mat = Cv2.ImDecode(byteArray, ImreadModes.Color);
            if (mat.Empty())
            {
                Console.WriteLine("Decoded Mat is empty!");
                Machine.logger.Write(eLogType.INFORMATION, "Decoded Mat is empty!");
            }
            return mat;
        }

        private BitmapImage ConvertMatToBitmapImage(Mat mat)
        {

            if (mat.Empty())
            {
                Console.WriteLine("The Mat is empty before encoding!");
                Machine.logger.Write(eLogType.INFORMATION, "The Mat is empty before encoding!");
                return null;
            }

            byte[] imageBytes;

            bool isEncoded = Cv2.ImEncode(".png", mat, out imageBytes);
            if (!isEncoded)
            {
                throw new Exception("Failed to encode the Mat to image.");
            }

            using (MemoryStream memoryStream = new MemoryStream(imageBytes))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

                if (bitmapImage.PixelWidth == 0 || bitmapImage.PixelHeight == 0)
                {
                    Console.WriteLine("bitmapImage is empty!");
                    Machine.logger.Write(eLogType.INFORMATION, "bitmapImage is empty!");
                }
                else
                {
                    Console.WriteLine($"{bitmapImage.PixelWidth}, {bitmapImage.PixelHeight}");
                    Machine.logger.Write(eLogType.INFORMATION, $"{bitmapImage.PixelWidth}, {bitmapImage.PixelHeight}");
                }
                return bitmapImage;
            }
        }

        public void SetImageSize(double height, double width)
        {
            imgTry1.Height = height;
            imgTry1.Width = width;
            imgTry2.Height = height;
            imgTry2.Width = width;
            imgTry3.Height = height;
            imgTry3.Width = width;
            imgTry4.Height = height;
            imgTry4.Width = width;
        }

        public void ImageLoad(int id)
        {
            // grab images
            BitmapImage img = ((App)System.Windows.Application.Current).GetTrialImage(id) != null ?
                ((App)System.Windows.Application.Current).GetTrialImage(id) : null;

            if (img != null)
            {
                // convert to mat
                Mat imgMat = ConvertBitmapImageToMat(img);

                //crop the mat
                int x = (int)ROI.start_x;
                int y = (int)ROI.start_y;
                int width = (int)ROI.end_x - (int)ROI.start_x;
                int height = (int)ROI.end_y - (int)ROI.start_y;
                OpenCvSharp.Rect roiSize = new OpenCvSharp.Rect(x, y, width, height);

                Mat cropImg = new Mat(imgMat, roiSize);

                img = ConvertMatToBitmapImage(cropImg.Clone());
            }

            if (img == null)
            {
                Console.WriteLine("Failed to load or crop the image.");
                Machine.logger.Write(eLogType.ERROR, "Failed to load or crop the image.");
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (id == 0) imgTry1.Source = img;
                if (id == 1) imgTry2.Source = img;
                if (id == 2) imgTry3.Source = img;
                if (id == 3) imgTry4.Source = img;
            });
        }

        #endregion
    }
}
