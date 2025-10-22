using Cognex.VisionPro;
using Cognex.VisionPro.PMAlign;
using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Controls;
using DOOR_TRIM_INSPECTION.Controls.FormControls;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DOOR_TRIM_INSPECTION.Form
{
    /// <summary>
    /// Interaction logic for PopUpROISetupTryWin.xaml
    /// </summary>
    /// 
    public partial class PopUpROISetupTryWindow : System.Windows.Window
    {
        private string CropImagePath
        {
            get
            {
                string path = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                         "images\\TryCrop");
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);
                return path;
            }
        }

        private BackgroundWorker bgWorker;
        private DetectionROIDetailsUI ROI;
        private new CtrlRecipeWizardROISetup Parent;

        private static BitmapImage[] ImageList = new BitmapImage[5];
        private static Mat[] MatImageList = new Mat[5];
        private bool[] HasImage = new bool[5];

        public event Action<string> ParamUpdated;

        string timestamp;

        public PopUpROISetupTryWindow()
        {
            InitializeComponent();
            bgWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;


        }

        private void ResetTryResult()
        {
            SolidColorBrush brushTrying = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 128, 128, 128));
            lblMasterOkNg.Background = brushTrying;
            lblMasterOkNg.Content = "-";
            lblTryOkNg1.Background = brushTrying;
            lblTryOkNg1.Content = "-";
            lblTryOkNg2.Background = brushTrying;
            lblTryOkNg2.Content = "-";
            lblTryOkNg3.Background = brushTrying;
            lblTryOkNg3.Content = "-";
            lblTryOkNg4.Background = brushTrying;
            lblTryOkNg4.Content = "-";
        }

        private bool LoadAlternateImages = false;

        public void SetParam(DetectionROIDetailsUI roi, CtrlRecipeWizardROISetup parent)
        {
            ROI = roi;
           
            
            Parent = parent;
            LoadParameterView(ROI.Parameters);
            if (ROI.Parameters.Contains("alternateroi"))
            {
                OpenCvSharp.Rect AlternateRoi = TrialInspection.AlternateRoi;
                LoadAlternateImages = AlternateRoi.Height > 0 && AlternateRoi.Width > 0;
            }
            timestamp = DateTime.Now.ToString("yyMMdd_HHmmss");
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i <= 4; i++)
                {
                    BitmapImage image = null;

                    if (ImageList[i] != null)
                    {
                        string filePath = System.IO.Path.Combine(CropImagePath, $"crp_img_{i}_{timestamp}.bmp");
                        image = new BitmapImage(new Uri(filePath));
                    }

                    if (i == 0) { imgTry1.Source = image; }
                    if (i == 1) { imgTry2.Source = image; }
                    if (i == 2) { imgTry3.Source = image; }
                    if (i == 3) { imgTry4.Source = image; }
                    if (i == 4) { imgMaster.Source = image; }
                }
            });
            progressBarMaster.Visibility = Visibility.Collapsed;
            progressBarImgTry1.Visibility = Visibility.Collapsed;
            progressBarImgTry2.Visibility = Visibility.Collapsed;
            progressBarImgTry3.Visibility = Visibility.Collapsed;
            progressBarImgTry4.Visibility = Visibility.Collapsed;


        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i <= 4; i++)
            {
                BitmapImage img = ImageLoad(i);
                ImageList[i] = img;
                bgWorker.ReportProgress(i, ImageList);
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ResetTryResult();
            if (!bgWorker.IsBusy)
            {
                progressBarMaster.Visibility = Visibility.Visible;
                progressBarImgTry1.Visibility = Visibility.Visible;
                progressBarImgTry2.Visibility = Visibility.Visible;
                progressBarImgTry3.Visibility = Visibility.Visible;
                progressBarImgTry4.Visibility = Visibility.Visible;
                bgWorker.RunWorkerAsync();
            }
        }

        private void LoadParameterView(string parameters)
        {
            stkParameters.Children.Clear();
            List<AlgorithmParamOption> algoParams = GetParameters(parameters);
            foreach (AlgorithmParamOption algoParam in algoParams)
            {
                PopUpInputRow popUpInput = new PopUpInputRow();
                popUpInput.setInfo(algoParam.OptionName, algoParam.Value);

                popUpInput.setInfoMasterCropImage(algoParam.OptionName, algoParam.Value,MatImageList[4]);
                stkParameters.Children.Add(popUpInput);
            }
            List<string> paramOptions = new List<string>();
            foreach (PopUpInputRow pInput in stkParameters.Children.OfType<PopUpInputRow>().ToList())
            {
                paramOptions.Add(pInput.getInfo());
            }
            TrialInspection.SetParameters(paramOptions);
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
                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                if (bitmapImage.PixelWidth == 0 || bitmapImage.PixelHeight == 0)
                {
                    Console.WriteLine("bitmapImage is empty!");
                    Machine.logger.Write(eLogType.INFORMATION, "bitmapImage is empty!");
                }
                else
                {
                    //Console.WriteLine($"{bitmapImage.PixelWidth}, {bitmapImage.PixelHeight}");
                }
                return bitmapImage;
            }
        }

        private string[] GetImagesByPattern(string folderPath, string pattern)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);
            return System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly)
                            .Where(file => regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(file)))
                            .ToArray();
        }


        public BitmapImage ImageLoad(int id)
        {
            BitmapImage img = null;
            Mat matImg = null;
            
            if (id == 4)
            {
                bool front_door = ROI.front_door == 1 ? true : false;
                //img = ((App)System.Windows.Application.Current).GetImage(ROI.recipe_ID, front_door) != null ?
                //((App)System.Windows.Application.Current).GetImage(ROI.recipe_ID, front_door) : null;
               
                if (LoadAlternateImages)
                {
                    if (TrialInspection.AlternateImagePath != "")
                    {
                        Mat temp = (ROI.detection_class_ID == 14) ? Cv2.ImRead(TrialInspection.AlternateImagePath) : LevelOps.EqualizeHistColor(Cv2.ImRead(TrialInspection.AlternateImagePath)); // MEER 2025.07.24
                        matImg = temp;
                    }
                    else
                    {
                        string SearchPath = System.IO.Path.GetDirectoryName(((App)System.Windows.Application.Current).GetImagePath(ROI.recipe_ID, front_door));
                        if (!string.IsNullOrEmpty(SearchPath))
                        {
                            List<string> imageFiles = (ROI.detection_class_ID == 14) ? GetImagesByPattern(SearchPath, @"rear_\d{6}.bmp_\d{6}_sub_3$").ToList() : GetImagesByPattern(SearchPath, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();

                            if (imageFiles.Count > 0)
                            {
                                Mat temp = (ROI.detection_class_ID == 14) ? Cv2.ImRead(imageFiles[0]) : LevelOps.EqualizeHistColor(Cv2.ImRead(imageFiles[0])); // MEER 2025.02.11
                                matImg = new Mat(temp, TrialInspection.AlternateRoi);
                            }
                        }
                    }
                }
                else
                {
                    matImg = Cv2.ImRead(((App)System.Windows.Application.Current).GetImagePath(ROI.recipe_ID, front_door)); // MEER 2025.02.11
                }
            }
            else
            {
                // grab images
                //img = ((App)System.Windows.Application.Current).GetTrialImage(id) != null ?
                //    ((App)System.Windows.Application.Current).GetTrialImage(id) : null;
                if (LoadAlternateImages)
                {
                    string SearchPath = ((App)System.Windows.Application.Current).GetTrialImageFolder(id);
                    if (!string.IsNullOrEmpty(SearchPath))
                    {
                        List<string> imageFiles = (ROI.detection_class_ID == 14) ? GetImagesByPattern(SearchPath, @"rear_\d{6}.bmp_\d{6}_sub_3$").ToList() : GetImagesByPattern(SearchPath, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();

                        if (imageFiles.Count > 0)
                        {
                            Mat temp = (ROI.detection_class_ID == 14) ? Cv2.ImRead(imageFiles[0]) : LevelOps.EqualizeHistColor(Cv2.ImRead(imageFiles[0])); // MEER 2025.02.11
                            matImg = new Mat(temp,TrialInspection.AlternateRoi); 
                        }
                    }
                }
                else
                {
                    matImg = ((App)System.Windows.Application.Current).GetTrialImageMat(id); // MEER 2025.02.11
                }
            }

            if (matImg != null)
            {
                // convert to mat
                //Mat imgMat = ConvertBitmapImageToMat(img); // MEER 2025.02.11
                Mat imgMat = matImg;
                //Mat imgMat = Cv2.ImRead(img.UriSource.ToString());

                //crop the mat
                int x = (int)ROI.start_x;
                int y = (int)ROI.start_y;
                int width = (int)ROI.end_x - (int)ROI.start_x;
                int height = (int)ROI.end_y - (int)ROI.start_y;

                if (ROI.detection_class_ID == 9)
                {
                    int newWidth = (int)(width * 1.3);
                    int newHeight = (int)(height * 1.3);
                    x = (int)(x - (newWidth - width) / 2);
                    y = (int)(y - (newHeight - height) / 2);
                    width = newWidth;
                    height = newHeight;
                }
                if (ROI.detection_class_ID == 7)
                {
                    int newWidth = (int)(width * 1.5);
                    int newHeight = (int)(height * 1.5);
                    x = (int)(x - (newWidth - width) / 2);
                    y = (int)(y - (newHeight - height) / 2);
                    width = newWidth;
                    height = newHeight;
                }

                OpenCvSharp.Rect roiSize = new OpenCvSharp.Rect(x, y, width, height);
                if (LoadAlternateImages)
                {
                    if (matImg.Width> 0 &&
                    matImg.Height > 0)
                    {
                        string filePath = System.IO.Path.Combine(CropImagePath, $"crp_img_{id}_{timestamp}.bmp");
                        Cv2.ImWrite(filePath, matImg);
                        img = ConvertMatToBitmapImage(matImg);
                        MatImageList[id] = matImg.Clone(); // MEER 2025.02.11
                    }
                    else
                    {
                        img = null;
                        MatImageList[id] = null; // MEER 2025.02.11
                    }
                }
                else
                {
                    if (roiSize.X >= 0 &&
                    roiSize.Y >= 0 &&
                    roiSize.X + roiSize.Width <= imgMat.Width &&
                    roiSize.Y + roiSize.Height <= imgMat.Height)
                    {
                        Mat cropImg = new Mat(imgMat, roiSize);
                        string filePath = System.IO.Path.Combine(CropImagePath, $"crp_img_{id}_{timestamp}.bmp");
                        Cv2.ImWrite(filePath, cropImg);

                        img = ConvertMatToBitmapImage(cropImg);
                        MatImageList[id] = cropImg.Clone(); // MEER 2025.02.11
                    }
                    else
                    {
                        img = null;
                        MatImageList[id] = null; // MEER 2025.02.11
                    }
                }
            }
            return img;
        }

        #endregion



        private (BitmapImage, INSPECTION_RESULT, String) InspectionTry(BitmapImage img)
        {
            Mat imgMat = ConvertBitmapImageToMat(img);
            //Mat imgMat = Cv2.ImRead(img.UriSource.ToString().Substring(8).Replace("/", "\\"));

            List<string> paramOptions = new List<string>();
            foreach (PopUpInputRow pInput in stkParameters.Children.OfType<PopUpInputRow>().ToList())
            {
                paramOptions.Add(pInput.getInfo());
            }
            var RuleID = ROI.detection_class_ID;

            TrialInspection.SetParameters(paramOptions);

            INSPECTION_RESULT inspectionResult = INSPECTION_RESULT.NOT_FOUND;
            string conditions = null;
            switch (RuleID)
            {
                case 1:
                    //Console.WriteLine("COLOR"); // not required
                    break;
                case 2:
                    //Console.WriteLine("PLUG");


                    TrialPlugInspectionItem trialPlugInspectionItem = new TrialPlugInspectionItem(imgMat, TrialInspection.MaskMinThreshold, TrialInspection.MaskMaxThreshold, TrialInspection.MaskThresholdType, TrialInspection.MorphMinThreshold, TrialInspection.MorphMaxThreshold, TrialInspection.MorphThresholdType, TrialInspection.MinError, TrialInspection.MinError, TrialInspection.MinContourArea);
                    TrialPlugInspectionItem result2 = TrialInspection.ExecutePlugInspectionItem(trialPlugInspectionItem);
                    Mat resultImg2 = result2.ResultImageRegion;
                    img = ConvertMatToBitmapImage(resultImg2.Clone());
                    inspectionResult = result2.InspectionResult;
                    conditions = result2.Conditions;

                    break;
                case 3:
                    //Console.WriteLine("SCREW");

                    TrialScrewInspectionItem trialScrewInspectionItem = new TrialScrewInspectionItem(imgMat, TrialInspection.MinThreshold, TrialInspection.MaxThreshold, TrialInspection.MorphThresholdType, TrialInspection.MinContourArea);
                    TrialScrewInspectionItem result3 = TrialInspection.ExecuteTrialScrewInspection(trialScrewInspectionItem);
                    Mat resultImg3 = result3.ResultImageRegion;
                    img = ConvertMatToBitmapImage(resultImg3.Clone());
                    inspectionResult = result3.InspectionResult;
                    conditions = result3.Conditions;

                    break;
                case 4:
                    //Console.WriteLine("BOLT");

                    TrialBoltInspectionItem trialBoltInspectionItem = new TrialBoltInspectionItem(imgMat, TrialInspection.Color, TrialInspection.Bound, TrialInspection.MinTotalArea);
                    TrialBoltInspectionItem result4 = TrialInspection.ExecuteTrialBoltInpection(trialBoltInspectionItem);
                    Mat resultImg4 = result4.ResultImageRegion;
                    img = ConvertMatToBitmapImage(resultImg4.Clone());
                    inspectionResult = result4.InspectionResult;
                    conditions = result4.Conditions;

                    break;
                case 5:
                    //Console.WriteLine("PAD");
                    //Mat temp = Cv2.ImRead(@"D:\images\02\10\31RVNH22502100023_162711\rear_162711.bmp");
                    ////2162, 825, 2514, 973
                    //imgMat = new Mat(temp, new OpenCvSharp.Rect(2162, 825, 2514 - 2162, 973 - 825));
                    ////imgMat = 
                    TrialPadInspectionItem trialPadInspectionItem = new TrialPadInspectionItem(imgMat, TrialInspection.MinThreshold, TrialInspection.MaxThreshold, TrialInspection.Variance);
                    TrialPadInspectionItem result5 = TrialInspection.ExecuteTrialPadInpection(trialPadInspectionItem);
                    Mat resultImg5 = result5.ResultImageRegion;
                    img = ConvertMatToBitmapImage(resultImg5.Clone());
                    inspectionResult = result5.InspectionResult;
                    conditions = result5.Conditions;
                    break;
                case 6:
                    //Console.WriteLine("DEEP"); // not required
                    break;
                case 7:
                    //Console.WriteLine("SPEAKER");

                    TrialSpeakerInspectionItem trialSpeakerInspectionItem = new TrialSpeakerInspectionItem(imgMat, TrialInspection.Accuracy, TrialInspection.TemplatePath);
                    TrialSpeakerInspectionItem result7 = TrialInspection.ExecuteSpeakerInspectionItem(trialSpeakerInspectionItem);
                    Mat resultImg7 = result7.ResultImageRegion;
                    img = ConvertMatToBitmapImage(resultImg7.Clone());
                    inspectionResult = result7.InspectionResult;
                    conditions = result7.Conditions;

                    break;
                case 8:
                    //Console.WriteLine("SMALLPAD");

                    TrialSmallPadInspectionItem trialSmallPadInspectionItem = new TrialSmallPadInspectionItem(imgMat, TrialInspection.MinThreshold, TrialInspection.MaxThreshold, TrialInspection.Variance, TrialInspection.WhitePixelCount);
                    TrialSmallPadInspectionItem result8 = TrialInspection.ExecuteSmallPadInspectionItem(trialSmallPadInspectionItem);
                    Mat resultImg8 = result8.ResultImageRegion;
                    img = ConvertMatToBitmapImage(resultImg8.Clone());
                    inspectionResult = result8.InspectionResult;
                    conditions = result8.Conditions;
                    break;
                case 9:
                    //Console.WriteLine("SCREWMATCH");
                    TrialScrewMacthInspectionItem trialScrewMacthInspectionItem = new TrialScrewMacthInspectionItem(imgMat, TrialInspection.Accuracy, TrialInspection.TemplatePath);
                    TrialScrewMacthInspectionItem result9 = TrialInspection.ExecuteScrewMetchInspectionItem(trialScrewMacthInspectionItem);
                    Mat resultImg9 = result9.ResultImageRegion;
                    img = ConvertMatToBitmapImage(resultImg9.Clone());
                    inspectionResult = result9.InspectionResult;
                    conditions = result9.Conditions;

                    break;
                case 10:
                    //Console.WriteLine("PLUGMATCH");

                    TrialPlugMatchInspectionItem trialPlugMatchInspectionItem = new TrialPlugMatchInspectionItem(imgMat, TrialInspection.TemplatePath1, TrialInspection.TemplatePath2, TrialInspection.PlugDistanceX, TrialInspection.PlugDistanceY, TrialInspection.Accuracy, TrialInspection.MaxLengthX, TrialInspection.MaxLengthY, TrialInspection.Direction);
                    TrialPlugMatchInspectionItem result10 = TrialInspection.ExecuteTrialPlugMatchInpection(trialPlugMatchInspectionItem);
                    Mat resultImg10 = result10.ResultImageRegion;
                    img = ConvertMatToBitmapImage(resultImg10.Clone());
                    inspectionResult = result10.InspectionResult;
                    conditions = result10.Conditions;

                    break;

                default:
                    Console.WriteLine("Invalid RuleID");
                    Machine.logger.Write(eLogType.INFORMATION, $"Invalid RuleID {RuleID}");
                    break;
            }
            return (img, inspectionResult, conditions);
        }

        private (BitmapImage, INSPECTION_RESULT, String) InspectionTryMat(Mat imgMat) // MEER 2025.02.11
        {
            BitmapImage img = null;
            //Mat imgMat = Cv2.ImRead(img.UriSource.ToString().Substring(8).Replace("/", "\\"));
            Mat tempMat = imgMat.Clone();
            List<string> paramOptions = new List<string>();
            foreach (PopUpInputRow pInput in stkParameters.Children.OfType<PopUpInputRow>().ToList())
            {
                paramOptions.Add(pInput.getInfo());
            }
            var RuleID = ROI.detection_class_ID;

            TrialInspection.SetParameters(paramOptions);

            INSPECTION_RESULT inspectionResult = INSPECTION_RESULT.NOT_FOUND;
            string conditions = null;
            switch (RuleID)
            {
                case 1:
                    //Console.WriteLine("COLOR"); // not required
                    break;
                case 2:
                    //Console.WriteLine("PLUG");
                    TrialPlugInspectionItem trialPlugInspectionItem = new TrialPlugInspectionItem(tempMat, TrialInspection.MaskMinThreshold, TrialInspection.MaskMaxThreshold, TrialInspection.MaskThresholdType, TrialInspection.MorphMinThreshold, TrialInspection.MorphMaxThreshold, TrialInspection.MorphThresholdType, TrialInspection.MinError, TrialInspection.MinError, TrialInspection.MinContourArea);
                    TrialPlugInspectionItem result2 = TrialInspection.ExecutePlugInspectionItem(trialPlugInspectionItem);
                    Mat resultImg2 = result2.ResultImageRegion;
                    img = resultImg2 == null ? null : ConvertMatToBitmapImage(resultImg2.Clone());
                    inspectionResult = result2.InspectionResult;
                    conditions = result2.Conditions;
                    break;
                case 3:
                    //Console.WriteLine("SCREW");
                    TrialScrewInspectionItem trialScrewInspectionItem = new TrialScrewInspectionItem(tempMat, TrialInspection.MinThreshold, TrialInspection.MaxThreshold, TrialInspection.MorphThresholdType, TrialInspection.MinContourArea);
                    TrialScrewInspectionItem result3 = TrialInspection.ExecuteTrialScrewInspection(trialScrewInspectionItem);
                    Mat resultImg3 = result3.ResultImageRegion;
                    img = resultImg3 == null ? null : ConvertMatToBitmapImage(resultImg3.Clone());
                    inspectionResult = result3.InspectionResult;
                    conditions = result3.Conditions;
                    break;
                case 4:
                    //Console.WriteLine("BOLT");
                    TrialBoltInspectionItem trialBoltInspectionItem = new TrialBoltInspectionItem(tempMat, TrialInspection.Color, TrialInspection.Bound, TrialInspection.MinTotalArea);
                    TrialBoltInspectionItem result4 = TrialInspection.ExecuteTrialBoltInpection(trialBoltInspectionItem);
                    Mat resultImg4 = result4.ResultImageRegion;
                    img = resultImg4 == null ? null : ConvertMatToBitmapImage(resultImg4.Clone());
                    inspectionResult = result4.InspectionResult;
                    conditions = result4.Conditions;
                    break;
                case 5:
                    //Console.WriteLine("PAD");
                    TrialPadInspectionItem trialPadInspectionItem = new TrialPadInspectionItem(tempMat, TrialInspection.MinThreshold, TrialInspection.MaxThreshold, TrialInspection.Variance);
                    TrialPadInspectionItem result5 = TrialInspection.ExecuteTrialPadInpection(trialPadInspectionItem);
                    Mat resultImg5 = result5.ResultImageRegion;
                    img = resultImg5 == null ? null : ConvertMatToBitmapImage(resultImg5.Clone());
                    inspectionResult = result5.InspectionResult;
                    conditions = result5.Conditions;
                    break;
                case 6:
                    //Console.WriteLine("DEEP"); // not required
                    break;
                case 7:
                    //Console.WriteLine("SPEAKER");
                    TrialSpeakerInspectionItem trialSpeakerInspectionItem = new TrialSpeakerInspectionItem(tempMat, TrialInspection.Accuracy, TrialInspection.TemplatePath);
                    TrialSpeakerInspectionItem result7 = TrialInspection.ExecuteSpeakerInspectionItem(trialSpeakerInspectionItem);
                    Mat resultImg7 = result7.ResultImageRegion;
                    img = resultImg7 == null ? null : ConvertMatToBitmapImage(resultImg7.Clone());
                    inspectionResult = result7.InspectionResult;
                    conditions = result7.Conditions;
                    break;
                case 8:
                    //Console.WriteLine("SMALLPAD");
                    TrialSmallPadInspectionItem trialSmallPadInspectionItem = new TrialSmallPadInspectionItem(tempMat, TrialInspection.MinThreshold, TrialInspection.MaxThreshold, TrialInspection.Variance, TrialInspection.WhitePixelCount);
                    TrialSmallPadInspectionItem result8 = TrialInspection.ExecuteSmallPadInspectionItem(trialSmallPadInspectionItem);
                    Mat resultImg8 = result8.ResultImageRegion;
                    img = resultImg8 == null ? null : ConvertMatToBitmapImage(resultImg8.Clone());
                    inspectionResult = result8.InspectionResult;
                    conditions = result8.Conditions;
                    break;
                case 9:
                    //Console.WriteLine("SCREWMATCH");
                    TrialScrewMacthInspectionItem trialScrewMacthInspectionItem = new TrialScrewMacthInspectionItem(tempMat, TrialInspection.Accuracy, TrialInspection.TemplatePath);
                    TrialScrewMacthInspectionItem result9 = TrialInspection.ExecuteScrewMetchInspectionItem(trialScrewMacthInspectionItem);
                    Mat resultImg9 = result9.ResultImageRegion;
                    img = resultImg9 == null ? null : ConvertMatToBitmapImage(resultImg9.Clone());
                    inspectionResult = result9.InspectionResult;
                    conditions = result9.Conditions;
                    break;
                case 10:
                    //Console.WriteLine("PLUGMATCH");
                    TrialPlugMatchInspectionItem trialPlugMatchInspectionItem = new TrialPlugMatchInspectionItem(tempMat, TrialInspection.TemplatePath1, TrialInspection.TemplatePath2, TrialInspection.PlugDistanceX, TrialInspection.PlugDistanceY, TrialInspection.Accuracy, TrialInspection.MaxLengthX, TrialInspection.MaxLengthY, TrialInspection.Direction);
                    TrialPlugMatchInspectionItem result10 = TrialInspection.ExecuteTrialPlugMatchInpection(trialPlugMatchInspectionItem);
                    Mat resultImg10 = result10.ResultImageRegion;
                    img = resultImg10 == null ? null : ConvertMatToBitmapImage(resultImg10.Clone());
                    inspectionResult = result10.InspectionResult;
                    conditions = result10.Conditions;
                    break;
                case 11:
                    //Console.WriteLine("COLOR"); // not required
                    TrialColorMatchInspectionItem colorMatchInspectionItem = new TrialColorMatchInspectionItem(tempMat, TrialInspection.AvgColor, TrialInspection.Bound);
                    TrialColorMatchInspectionItem result = TrialInspection.ExecuteCompareColor(colorMatchInspectionItem);
                    Mat resultImg = result.ResultImageRegion;
                    img = resultImg == null ? null : ConvertMatToBitmapImage(resultImg.Clone());
                    inspectionResult = result.InspectionResult;
                    conditions = result.Conditions;
                    break;
                case 13:
                    //Console.WriteLine("White Pad");
                    TrialWhitePadInspectionItem whitePadInspectionItem = new TrialWhitePadInspectionItem(tempMat, TrialInspection.MinThreshold, TrialInspection.MaxThreshold, TrialInspection.MinTotalArea, TrialInspection.MaxTotalArea);
                    TrialWhitePadInspectionItem result13 = TrialInspection.ExecuteTrialWhitePadInpection(whitePadInspectionItem);
                    Mat resultImg13 = result13.ResultImageRegion;
                    img = resultImg13 == null ? null : ConvertMatToBitmapImage(resultImg13.Clone());
                    inspectionResult = result13.InspectionResult;
                    conditions = result13.Conditions;
                    break;
#if USE_COGNEX
                case 14:
                    //Console.WriteLine("COGNEXPLUGMATCH");
                    TrialCognexPlugMatchInspectionItem trialCognexMatchInspectionItem = new TrialCognexPlugMatchInspectionItem(imgMat, TrialInspection.PlugVppPath1, TrialInspection.PlugVppPath2, TrialInspection.PlugCogDistanceX, TrialInspection.PlugCogDistanceY, TrialInspection.OuterConfidence, TrialInspection.InnerConfidence, TrialInspection.MaxLengthX, TrialInspection.MaxLengthY, TrialInspection.Direction);
                    TrialCognexPlugMatchInspectionItem result14 = TrialInspection.ExecuteTrialCognexPlugMatchInpection(trialCognexMatchInspectionItem);
                    Mat resultImg14 = result14.ResultImageRegion;
                    img = ConvertMatToBitmapImage(resultImg14.Clone());
                    inspectionResult = result14.InspectionResult;
                    conditions = result14.Conditions;

                    break;
                case 15:
                    TrialCognexPlugMatchInspectionItem trialCognexOtherMatchInspectionItem = new TrialCognexPlugMatchInspectionItem(imgMat, TrialInspection.PlugVppPath1, TrialInspection.PlugVppPath2, TrialInspection.PlugCogDistanceX, TrialInspection.PlugCogDistanceY, TrialInspection.OuterConfidence, TrialInspection.InnerConfidence, TrialInspection.MaxLengthX, TrialInspection.MaxLengthY, TrialInspection.Direction);
                    TrialCognexPlugMatchInspectionItem result15 = TrialInspection.ExecuteTrialCognexPlugMatchInpection(trialCognexOtherMatchInspectionItem);
                    Mat resultImg15 = result15.ResultImageRegion;
                    img = ConvertMatToBitmapImage(resultImg15.Clone());
                    inspectionResult = result15.InspectionResult;
                    conditions = result15.Conditions;

                    break;
#endif
                default:
                    Console.WriteLine("Invalid RuleID");
                    Machine.logger.Write(eLogType.INFORMATION, $"Invalid RuleID {RuleID}");
                    break;
            }
            return (img, inspectionResult, conditions);
        }


        public static Mat BitmapImageToMat(BitmapImage bitmapImage)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(memory);
                memory.Position = 0;

                using (var bitmap = new System.Drawing.Bitmap(memory))
                {
                    return OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
                }
            }
        }


        private (BitmapImage, INSPECTION_RESULT, String) TryCogVproInsp()
        {
            bool IsFront = false;
            //BitmapImage bitmap = ((App)System.Windows.Application.Current).GetImage(ROI.recipe_ID, IsFront);

            var uri = new Uri(@"D:\images\07\16\23UNNH42502060010_225815\rear_225815.bmp_225815_sub_3.bmp", UriKind.Absolute);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.EndInit();
            bitmap.Freeze();

            Mat imgMat = BitmapImageToMat(bitmap);
            CogImage8Grey curImg = new Cognex.VisionPro.CogImage8Grey(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(imgMat));

            CogRectangle cogRectangle = new CogRectangle();

            List<string> paramOptions = new List<string>();
            foreach (PopUpInputRow pInput in stkParameters.Children.OfType<PopUpInputRow>().ToList())
            {
                paramOptions.Add(pInput.getInfo());
            }
            var aulternateroi = GetParameters(paramOptions[0])[0].Value;
            var vpp1 = GetParameters(paramOptions[1])[0].Value;
            var vpp2 = GetParameters(paramOptions[2])[0].Value;
            int[] values = aulternateroi
                .Split(',')                          
                .Select(s => int.Parse(s.Trim()))
                .ToArray();
            var x = 3700;//values[0];
            var y = 2300; //values[1];
            var w = 400;// values[2];
            var h = 300;// values[3];


            cogRectangle.X = x;
            cogRectangle.Y = y;
            cogRectangle.Width = w;
            cogRectangle.Height = h;
            cogRectangle.TipText = ROI.roi_name;

            Int32Rect cropArea = new Int32Rect(x, y, w, h);

            //plug holder inspection
            //string vppUri_plugHolder = @"D:\Images\04 Leadwire\White\plug_holder.vpp";
            CogPMAlignMultiTool cogPMAligns_plugHolder = ((CogPMAlignMultiTool)CogSerializer.LoadObjectFromFile(vpp1));
            cogPMAligns_plugHolder.InputImage = curImg.Copy(CogImageCopyModeConstants.CopyPixels);
            cogPMAligns_plugHolder.SearchRegion = cogRectangle;
            cogPMAligns_plugHolder.Run();
            double judgeThreshold_plugHolder = cogPMAligns_plugHolder.RunParams.PMAlignRunParams.AcceptThreshold;
            CogPMAlignResults cogResult_plugHolder = cogPMAligns_plugHolder.Results.PMAlignResults;
            INSPECTION_RESULT inspResult_plugHolder = cogResult_plugHolder[0].Score < judgeThreshold_plugHolder ? INSPECTION_RESULT.NG : INSPECTION_RESULT.OK;

            //plug inspection
            //string vppUri_plug = @"D:\Images\04 Leadwire\White\plug.vpp";
            CogPMAlignMultiTool cogPMAligns_plug = ((CogPMAlignMultiTool)CogSerializer.LoadObjectFromFile(vpp2));
            cogPMAligns_plug.InputImage = curImg.Copy(CogImageCopyModeConstants.CopyPixels);
            cogPMAligns_plug.SearchRegion = cogRectangle;
            cogPMAligns_plug.Run();
            double judgeThreshold_plug = cogPMAligns_plug.RunParams.PMAlignRunParams.AcceptThreshold;
            CogPMAlignResults cogResult_plug = cogPMAligns_plug.Results.PMAlignResults;
            var score = cogResult_plug.Count > 0 ? cogResult_plug[0].Score : 0;
            INSPECTION_RESULT inspResult_plug = score < judgeThreshold_plug ? INSPECTION_RESULT.NG : INSPECTION_RESULT.OK;

            string conditions = score.ToString() + " < " + judgeThreshold_plug.ToString();
            // Crop the image
            CroppedBitmap croppedImage = new CroppedBitmap(bitmap, cropArea);
            croppedImage.Freeze();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedImage));

            using (var memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                memoryStream.Position = 0;
                BitmapImage cropedBitmapImage = new BitmapImage();
                cropedBitmapImage.BeginInit();
                cropedBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                cropedBitmapImage.StreamSource = memoryStream;
                cropedBitmapImage.EndInit();
                cropedBitmapImage.Freeze();

                return (cropedBitmapImage, inspResult_plug, conditions);
            }

        }


        #region form button action
        private void BtnTry_Click(object sender, RoutedEventArgs e)
        {
            ResetTryResult();
            SolidColorBrush brushOK = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 46, 139, 87));
            SolidColorBrush brushNG = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 255, 0, 0));

            int detection_class_ID = ROI.detection_class_ID;

            try
            {                
                if (imgMaster.Source is BitmapImage bitmapImage)
                {
                    
	                //var result = InspectionTry(bitmapImage);
	                var result = InspectionTryMat(MatImageList[4]); // MEER 2025.02.11
	                if(result.Item1 != null)
	                    imgMasterOutput.Source = result.Item1.Clone();
	                lblMasterOkNg.Content = result.Item2.ToString();
	                lblMasterOkNg.Background = (result.Item2 == INSPECTION_RESULT.OK) ? brushOK : brushNG;
	                txtMasterOutput.Text = result.Item3;
                	
                }
                if (imgTry1.Source is BitmapImage b1)
                {
                    //var result = InspectionTry(b1);
                    var result = InspectionTryMat(MatImageList[0]); // MEER 2025.02.11
                    if (result.Item1 != null)
                        imgTryOutput1.Source = result.Item1.Clone();
                    lblTryOkNg1.Content = result.Item2.ToString();
                    lblTryOkNg1.Background = (result.Item2 == INSPECTION_RESULT.OK) ? brushOK : brushNG;
                    txtTryOutput1.Text = result.Item3;
                }
                if (imgTry2.Source is BitmapImage b2)
                {
                    //var result = InspectionTry(b2); 
                    var result = InspectionTryMat(MatImageList[1]); // MEER 2025.02.11
                    if (result.Item1 != null)
                        imgTryOutput2.Source = result.Item1.Clone();
                    lblTryOkNg2.Content = result.Item2.ToString();
                    lblTryOkNg2.Background = (result.Item2 == INSPECTION_RESULT.OK) ? brushOK : brushNG;
                    txtTryOutput2.Text = result.Item3;
                }
                if (imgTry3.Source is BitmapImage b3)
                {
                    //var result = InspectionTry(b3);
                    var result = InspectionTryMat(MatImageList[2]); // MEER 2025.02.11
                    if (result.Item1 != null)
                        imgTryOutput3.Source = result.Item1.Clone();
                    lblTryOkNg3.Content = result.Item2.ToString();
                    lblTryOkNg3.Background = (result.Item2 == INSPECTION_RESULT.OK) ? brushOK : brushNG;
                    txtTryOutput3.Text = result.Item3;
                }
                if (imgTry4.Source is BitmapImage b4)
                {
                    //var result = InspectionTry(b4); 
                    var result = InspectionTryMat(MatImageList[3]); // MEER 2025.02.11
                    imgTryOutput4.Source = result.Item1.Clone();
                    lblTryOkNg4.Content = result.Item2.ToString();
                    lblTryOkNg4.Background = (result.Item2 == INSPECTION_RESULT.OK) ? brushOK : brushNG;
                    txtTryOutput4.Text = result.Item3;
                }
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, "BtnTry_Click : "+ex.ToString());
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            TryFormClean();

            TrialInspection.ClearParameters();

        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            List<string> paramOptions = new List<string>();
            foreach (PopUpInputRow pInput in stkParameters.Children.OfType<PopUpInputRow>().ToList())
            {
                paramOptions.Add(pInput.getInfo());
            }
            string paramStr = string.Join("|", paramOptions);

            ParamUpdated.Invoke(paramStr);
        }

        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //TryFormClean();


            //for (int i = 0; i < 5; i++)
            //{

            //    string fileName = $"D:\\cropped_images\\crp_img*.bmp";
            //    try
            //    {
            //        if (File.Exists(fileName))
            //            File.Delete(fileName);
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}

        }

        private void TryFormClean()
        {
            imgTry1.Source = null;
            imgTry2.Source = null;
            imgTry3.Source = null;
            imgTry4.Source = null;
            imgMaster.Source = null;

            string filepath = CropImagePath;
            string filename = $"crp_img*.bmp";
            string[] filelist = System.IO.Directory.GetFiles(filepath, filename);

            try
            {
                foreach (string file in filelist)
                {
                    System.IO.File.Delete(file);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(BackgroundGrid);
            System.Drawing.Color color;
            if (position.X >= 0 && position.X < BackgroundGrid.ActualWidth &&
                position.Y >= 0 && position.Y < BackgroundGrid.ActualHeight)
            {
                    System.Windows.Point screenPosition = BackgroundGrid.PointToScreen(new System.Windows.Point(position.X, position.Y));
                    using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        using (Bitmap bmp = new Bitmap(1, 1, g))
                        {
                            using (Graphics bmpGraphics = Graphics.FromImage(bmp))
                            {
                                bmpGraphics.CopyFromScreen((int)screenPosition.X, (int)screenPosition.Y, 0, 0, new System.Drawing.Size(1, 1));
                            }
                        color= bmp.GetPixel(0, 0);
                        }
                    }
                    if(color!=null)
                        DrawingCanvasStatus.Content = "R : " + color.R + ", G : " + color.G + ", B : " + color.B;
                
            }
        }
    }
}
