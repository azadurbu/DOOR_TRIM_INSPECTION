using DOOR_TRIM_INSPECTION.Class;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using System.Windows.Shapes;

namespace DOOR_TRIM_INSPECTION.Form
{
    /// <summary>
    /// Interaction logic for TrialInspectionWindow.xaml
    /// </summary>
    public partial class TrialInspectionWindow : Window
    {
        private string rearFilePath;
        private string rearSub1FilePath;
//#if USE_EXTRA_CAM
        private string rearSub2FilePath;
//#endif
        private string frontFilePath;
        private int recipeID = 0;
        private string doorTrimID;
        private BackgroundWorker bgWorker;
        private Inspection inspection;


        private string TrialInspectionImagePath
        {
            get
            {
                string path = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                         "images\\TrialInspection");
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);
                return path;
            }
        }

        public TrialInspectionWindow()
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
//#if USE_EXTRA_CAM
        public void SetParameters(string RearFilePath, string RearSub1FilePath, string FrontFilePath, int RecipeID, string DoorTrimID, string RearSub2FilePath  = "")
//#else
//        public void SetParameters(string RearFilePath, string RearSub1FilePath, string FrontFilePath, int RecipeID, string DoorTrimID)
//#endif
        {
            rearFilePath = RearFilePath;
            rearSub1FilePath = RearSub1FilePath;
//#if USE_EXTRA_CAM
            rearSub2FilePath = RearSub2FilePath;
//#endif
            frontFilePath = FrontFilePath;
            recipeID = RecipeID;
            doorTrimID = DoorTrimID;
            ////rearFilePath = @"D:\images\2025.01.14_01\01\A008NH12412130008_000000\rear_000000.bmp";
            //frontFilePath = @"D:\images\2025.01.14_01\01\A008NH12412130008_000000\front_000000.bmp";
            //recipeID = 15;
            //doorTrimID = "A008NH12412130008";
   
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // 검사 결과 처리
                ctrlImageVIew.ZoomClear();

                //bgWorker.RunWorkerAsync();
                ctrlImageVIew.RecipeID = recipeID;
                ctrlImageVIew.LoadImages(rearFilePath, frontFilePath, inspection, doorTrimID);
                progressBarMaster.Visibility = Visibility.Collapsed;
                ctrlImageVIew.Visibility = Visibility.Visible;


                if (inspection.FrontInspectionResultCode == INSPECTION_RESULT.NG)
                {
                    ctrlImageVIew.borderFront.BorderBrush = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    ctrlImageVIew.borderFront.BorderBrush = System.Windows.Media.Brushes.GreenYellow;
                }
            });
        }
        private Recipe CurrentRecipe;
        private void SetCurrentRecipe()
        {
            switch (Machine.BarcodeData.DOOR_TYPE % 5)
            {
                case 1:
                    CurrentRecipe = new Recipe(Machine.config.setup.RecipeNumFrontLeft);
                    break;
                case 2:
                    CurrentRecipe = new Recipe(Machine.config.setup.RecipeNumFrontRight);
                    break;
                case 3:
                    CurrentRecipe = new Recipe(Machine.config.setup.RecipeNumRearLeft);
                    break;
                case 4:
                    CurrentRecipe = new Recipe(Machine.config.setup.RecipeNumRearRight);
                    break;
                default:
                    CurrentRecipe = new Recipe(Machine.config.setup.RecipeNum);
                    break;
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Machine.InspectionMode = INSPECTION_MODE.HISTORY;

            // 바코드 및 레시피 설정
            Machine.BarcodeData = new BarCodeHelper(doorTrimID);
            Machine.ALCData = Machine.dyDBHelper.GetALCMIS3PF(new BarCodeHelper(doorTrimID));
            SetCurrentRecipe();

            // 검사 수행
            inspection = new Inspection(CurrentRecipe);
            //inspection.IsTrialInspection = false;

            // Rear 이미지 검사
            inspection.SetRearInspectionImage(rearFilePath, false); // 첫 번째 Rear 이미지 사용
            inspection.SetRearSub1InspectionImage(rearSub1FilePath); // 첫 번째 Rear sub1 이미지 사용
//#if USE_EXTRA_CAM
            inspection.SetRearSub2InspectionImage(rearSub2FilePath); // 첫 번째 Rear sub1 이미지 사용
//#endif
            inspection.ExecuteRearInspection();

            // Front 이미지 검사
            inspection.SetFrontInspectionImage(frontFilePath, false); // 첫 번째 Front 이미지 사용
            inspection.ExecuteFrontInspection();
        }

        private string SaveMergedImage(string doorTrimID, OpenCvSharp.Mat mergedImage, bool IsFront)
        {
            string imageSaveFolder = Machine.config.setup.ImagePath;

            
            DateTime _barcodeReadTime = DateTime.Now;
           
            string filePath = System.IO.Path.Combine(TrialInspectionImagePath, IsFront ? "front" : "rear");
            filePath += "_" + _barcodeReadTime.ToString("HHmmss") + ".bmp";
            OpenCvSharp.Cv2.ImWrite(filePath, mergedImage);
            //mergedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return filePath;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bgWorker.RunWorkerAsync();
        }
    }
}
