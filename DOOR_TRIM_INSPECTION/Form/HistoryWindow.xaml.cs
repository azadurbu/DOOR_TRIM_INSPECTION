using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Form;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DOOR_TRIM_INSPECTION
{
    public class SearchParameters
    {
        public DateTime ToDate { get; set; }
        public int DateDiff { get; set; }
        public string ResultType { get; set; }
        public string ShiftType { get; set; }
        public string DoorType { get; set; }
        public string SearchType { get; set; }

        public SearchParameters() { }
    }

    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class HistoryWindow : System.Windows.Window
    {
        private string rearFilePath;
        private string frontFilePath;
        private int recipeID = 0;
        private string doorTrimID;
        private DateTime inspTime;
        private TrialInspectionWindow TrialInspectionWindow = null;

        private TrialInspectionWindow TrialInspection
        {
            get
            {
                if (TrialInspectionWindow == null)
                    TrialInspectionWindow = new TrialInspectionWindow();
                return TrialInspectionWindow;
            }
        }

        public HistoryWindow()
        {
            InitializeComponent();
        }

        private void BtnCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn3Days_Click(object sender, RoutedEventArgs e)
        {
            dtpTo.SelectedDate = DateTime.Today;
            dtpFrom.SelectedDate = DateTime.Today.AddDays(-3);
        }

        private void Btn7Days_Click(object sender, RoutedEventArgs e)
        {
            dtpTo.SelectedDate = DateTime.Today;
            dtpFrom.SelectedDate = DateTime.Today.AddDays(-7);
        }

        private void Btn15Days_Click(object sender, RoutedEventArgs e)
        {
            dtpTo.SelectedDate = DateTime.Today;
            dtpFrom.SelectedDate = DateTime.Today.AddDays(-15);
        }

        private void Btn30Days_Click(object sender, RoutedEventArgs e)
        {
            dtpTo.SelectedDate = DateTime.Today;
            dtpFrom.SelectedDate = DateTime.Today.AddDays(-30);
        }

        private void Btn90Days_Click(object sender, RoutedEventArgs e)
        {
            dtpTo.SelectedDate = DateTime.Today;
            dtpFrom.SelectedDate = DateTime.Today.AddDays(-90);
        }

        private void Btn180Days_Click(object sender, RoutedEventArgs e)
        {
            dtpTo.SelectedDate = DateTime.Today;
            dtpFrom.SelectedDate = DateTime.Today.AddDays(-180);
        }

        //private void ChkAllDoors_Checked(object sender, RoutedEventArgs e)
        //{
        //    chkFL.IsChecked = !chkAllDoors.IsChecked;
        //    chkFR.IsChecked = !chkAllDoors.IsChecked;
        //    chkRL.IsChecked = !chkAllDoors.IsChecked;
        //    chkRR.IsChecked = !chkAllDoors.IsChecked;
        //}

        //private void ChkFL_Checked(object sender, RoutedEventArgs e)
        //{
        //    chkAllDoors.IsChecked = !chkFL.IsChecked;
        //}

        //private void ChkFR_Checked(object sender, RoutedEventArgs e)
        //{
        //    chkAllDoors.IsChecked = !chkFR.IsChecked;
        //}

        //private void ChkRL_Checked(object sender, RoutedEventArgs e)
        //{
        //    chkAllDoors.IsChecked = !chkRL.IsChecked;
        //}

        //private void ChkRR_Checked(object sender, RoutedEventArgs e)
        //{
        //    chkAllDoors.IsChecked = !chkRR.IsChecked;
        //}

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearFormData();
            
            //SearchParameters srcParams = new SearchParameters();
            //srcParams.ToDate = dtpTo.DisplayDate;
            //srcParams.DateDiff = (dtpFrom.DisplayDate - dtpTo.DisplayDate).Days;

            if (!ValidateSearchForm()) return;

            DateTime dtFrom = dtpFrom.SelectedDate.Value;
            DateTime dtTO = dtpTo.SelectedDate.Value;
            string ResultType = rdoOKResult.IsChecked.Value ? "OK" : (rdoNGResult.IsChecked.Value ? "NG" : "");
            string DoorTrimID = txtDoorTrimID.Text.Trim();


            List<InspectionResult> inspectionResults = Machine.hmcDBHelper.GetInspectionResultsByQuery(dtFrom, dtTO, ResultType, DoorTrimID);
            // DoorTrimID 값이 "NH4"인 데이터만 필터링// DoorTrimID 값이 "NH4"인 데이터만 필터링

            if(rdoAllDoorTypeFL.IsChecked.Value)
                inspectionResults=inspectionResults.Where(x => x.DoorTrimID.Contains("NH1") || x.DoorTrimID.Contains("NH5")).ToList();
            else if (rdoAllDoorTypeFR.IsChecked.Value)
                inspectionResults=inspectionResults.Where(x => x.DoorTrimID.Contains("NH2") || x.DoorTrimID.Contains("NH6")).ToList();
            else if (rdoAllDoorTypeRL.IsChecked.Value)
                inspectionResults = inspectionResults.Where(x => x.DoorTrimID.Contains("NH3") || x.DoorTrimID.Contains("NH7")).ToList();
            else if (rdoAllDoorTypeRR.IsChecked.Value)
                inspectionResults = inspectionResults.Where(x => x.DoorTrimID.Contains("NH4") || x.DoorTrimID.Contains("NH8")).ToList();

            // DataGridView에 필터링된 결과를 바인딩
            dgvSearchResult.ItemsSource = inspectionResults;
            LblSearchResultCount.Content = inspectionResults.Count().ToString();
        }

        private void ClearFormData()
        {
            //ctrlImageView.Clear();
            dgvSearchResult.ItemsSource = null;
            gvAlcInspResult.ItemsSource = null;
            gvNgInspResult.ItemsSource = null;
            frontFilePath = "";
            rearFilePath = "";
            ctrlImageView.LoadHistoryImages(0, 
                System.IO.Path.Combine(Machine.config.setup.ImagePath, "RecipeImages\\result_rear.bmp"),
                System.IO.Path.Combine(Machine.config.setup.ImagePath, "RecipeImages\\result_front.bmp"));
        }

        private bool ValidateSearchForm()
        {
            if (dtpFrom.SelectedDate == null || dtpTo.SelectedDate == null)
            {
                dtpTo.SelectedDate = DateTime.Today;

                dtpFrom.SelectedDate = DateTime.Today.AddDays(-3);
            }
            return true;
        }

        private void LoadNGItems(int InspResultID)
        {
            //List<DoorTrimInsp> NGItems = new List<DoorTrimInsp>();
            gvNgInspResult.ItemsSource = Machine.hmcDBHelper.GetNGInspectionResults(InspResultID);
        }

        private void LoadALCResult(int InspResultID)
        {
            //List<DoorTrimInsp> ALCitems = new List<DoorTrimInsp>();
            gvAlcInspResult.ItemsSource = Machine.hmcDBHelper.GetALCInspectionResults(InspResultID);
        }

        private string[] GetImagesByPattern(string folderPath, string pattern)
        {
            Regex regex = new Regex(pattern);
            return System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly)
                            .Where(file => regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(file)))
                            .ToArray();
        }

        private void DgvSearchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InspectionResult selectedResult = dgvSearchResult.SelectedItem as InspectionResult;

            if (selectedResult == null)
                return;

            LoadNGItems(selectedResult.InspectionResultID);
            recipeID = selectedResult.RecipeID;
            doorTrimID = selectedResult.DoorTrimID;
            inspTime = selectedResult.InspectionTime;
            LoadHistoryImages(recipeID, selectedResult.DoorTrimID, selectedResult.InspectionTime);
            LoadALCResult(selectedResult.InspectionResultID);

            btnRunInspection.Visibility = selectedResult != null ? Visibility.Visible : Visibility.Collapsed;

            if(Machine.hmcDBHelper.HistoryInspectionResult(1, selectedResult.InspectionResultID))
            {
                ctrlImageView.borderFront.BorderBrush = Brushes.GreenYellow;
            }
            else
            {
                ctrlImageView.borderFront.BorderBrush = Brushes.Red;
            }
        }

        private void LoadHistoryImages(int RecipeID, string DoorTrimID, DateTime BarcodeReadTime)
        {
            try
            {
                //ctrlImageView.ZoomClear();

                string frontFileName = "result_front.png";
                string rearFileName = "result_rear.png";
                string imageSaveFolder = Machine.config.setup.ImagePath;


                string Month = BarcodeReadTime.ToString("MM");
                string Date = BarcodeReadTime.ToString("dd");

                string dirPath = System.IO.Path.Combine(imageSaveFolder, Month, Date, DoorTrimID + "_" + BarcodeReadTime.ToString("HHmmss"));

                rearFilePath = System.IO.Path.Combine(dirPath, rearFileName);
                frontFilePath = System.IO.Path.Combine(dirPath, frontFileName);
               
                //rearFilePath = "E:\\IMAGES\\11\\14\\rear.bmp";
                //frontFilePath = "E:\\IMAGES\\11\\14\\front.bmp";

                ctrlImageView.LoadHistoryImages(RecipeID, rearFilePath, frontFilePath);

                
            }
            catch (Exception ex)
            {

            }
        }

        private void BtnRunInspection_Click(object sender, RoutedEventArgs e)
        {
            // OPEN A NEW WINDOW
            if (TrialInspectionWindow == null)
            {
                TrialInspectionWindow = new TrialInspectionWindow();
                TrialInspectionWindow.Closed += (s, args) => TrialInspectionWindow = null;

                string imageSaveFolder = Machine.config.setup.ImagePath;
                string Month = inspTime.ToString("MM");
                string Date = inspTime.ToString("dd");

                string SearchPath = System.IO.Path.Combine(imageSaveFolder, Month, Date, doorTrimID + "_" + inspTime.ToString("HHmmss"));

                string rearFileLocation = "";
                string rearSub1FileLocation = "";
//#if USE_EXTRA_CAM
                string rearSub2FileLocation = "";
//#endif
                string frontFileLocation = "";

                
                if (System.IO.Directory.Exists(SearchPath))
                {
                    // REAR FILE PATH
                    List<string> rearImageFiles = GetImagesByPattern(SearchPath, @"^rear_\d{6}$").ToList();
                    if (rearImageFiles.Count == 0)
                    {
                        Console.WriteLine($"Rear 또는 Front 이미지가 없습니다: {SearchPath}");
                        Machine.logger.Write(eLogType.ERROR, $"Rear 또는 Front 이미지가 없습니다: {SearchPath}");
                        MessageBox.Show("Can't find image.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        rearFileLocation = rearImageFiles[0];
                    }
                    // REAR Sub1 FILE PATH
                    List<string> rearSub1ImageFiles = GetImagesByPattern(SearchPath, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();
                    if (rearSub1ImageFiles.Count == 0)
                    {
                        Console.WriteLine($"Rear 또는 Front 이미지가 없습니다: {SearchPath}");
                        Machine.logger.Write(eLogType.ERROR, $"Rear 또는 Front 이미지가 없습니다: {SearchPath}");
                        MessageBox.Show("Can't find image.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        rearSub1FileLocation = rearSub1ImageFiles[0];
                    }
//#if USE_EXTRA_CAM
                    // REAR Sub1 FILE PATH
                    List<string> rearSub2ImageFiles = GetImagesByPattern(SearchPath, @"rear_\d{6}.bmp_\d{6}_sub_2$").ToList();
                    if (rearSub2ImageFiles.Count == 0)
                    {
                        Console.WriteLine($"Rear 또는 Front 이미지가 없습니다: {SearchPath}");
                        Machine.logger.Write(eLogType.ERROR, $"Rear 또는 Front 이미지가 없습니다: {SearchPath}");
                        MessageBox.Show("Can't find image.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        rearSub2FileLocation = rearSub2ImageFiles[0];
                    }
//#endif
                    // FRONT FILE PATH
                    List<string> frontImageFiles = GetImagesByPattern(SearchPath, @"^front_\d{6}$").ToList();
                    if (frontImageFiles.Count == 0)
                    {
                        Console.WriteLine($"Front 또는 Front 이미지가 없습니다: {SearchPath}");
                        Machine.logger.Write(eLogType.ERROR, $"Front 또는 Front 이미지가 없습니다: {SearchPath}");
                        MessageBox.Show("Can't find image.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        frontFileLocation = frontImageFiles[0];
                    }

                    // SAVE BLACK MERGED IMAGE FOR DEEP LEARNING
                    SaveImageForInference(SearchPath);
                }


//#if USE_EXTRA_CAM
                TrialInspectionWindow.SetParameters(rearFileLocation, rearSub1FileLocation, frontFileLocation, recipeID, doorTrimID, rearSub2FileLocation);
//#else
//                TrialInspectionWindow.SetParameters(rearFileLocation, rearSub1FileLocation, frontFileLocation, recipeID, doorTrimID);
//#endif
                TrialInspectionWindow.Show();
            }
            else
            {
                if (TrialInspectionWindow.WindowState == WindowState.Minimized)
                {
                    TrialInspectionWindow.WindowState = WindowState.Normal;
                }
                TrialInspectionWindow.Activate();
            }

            // VARIABLES = front, rearFilePath, recipeID
            // SHOW LOADER
            // BACKGROUND WORKER
            // ON COMPLETE SHOW IMAGE
        }

        private void SaveImageForInference(string searchPath)
        {
            //int yoffset = 98;
            //int xoffset01 = 548;
            //int xoffset02 = 234;
            int yoffset = 127;
            int xoffset01 = 469;
            int xoffset02 = 467;

            // REAR Sub0 FILE PATH >> LEFT
            string LeftFilePath = "";
            List<string> rearSub0ImageFiles = GetImagesByPattern(searchPath, @"rear_\d{6}.bmp_\d{6}_sub_0$").ToList();
            if (rearSub0ImageFiles.Count == 0)
            {
                Console.WriteLine($"Rear 또는 Front 이미지가 없습니다: {searchPath}");
                Machine.logger.Write(eLogType.ERROR, $"Rear 또는 Front 이미지가 없습니다: {searchPath}");
                MessageBox.Show("Can't find image.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                LeftFilePath = rearSub0ImageFiles[0];
            }

            // REAR Sub2 FILE PATH >> RIGHT
            string RightFilePath = "";
            List<string> rearSub2ImageFiles = GetImagesByPattern(searchPath, @"rear_\d{6}.bmp_\d{6}_sub_2$").ToList();
            if (rearSub2ImageFiles.Count == 0)
            {
                Console.WriteLine($"Rear 또는 Front 이미지가 없습니다: {searchPath}");
                Machine.logger.Write(eLogType.ERROR, $"Rear 또는 Front 이미지가 없습니다: {searchPath}");
                MessageBox.Show("Can't find image.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                RightFilePath = rearSub2ImageFiles[0];
            }

            Mat left = Cv2.ImRead(LeftFilePath);
            Mat right = Cv2.ImRead(RightFilePath);
            Mat leftCrop = new Mat(left, new OpenCvSharp.Rect(0, 0, left.Width - xoffset02, left.Height - yoffset));
            Mat rightCrop = new Mat(right, new OpenCvSharp.Rect(xoffset01, yoffset, left.Width - xoffset01, left.Height - yoffset));

            Mat result = new Mat();

            leftCrop = LevelOps.EqualizeHistColor(leftCrop); // MEER 2025.04.28
            rightCrop = LevelOps.EqualizeHistColor(rightCrop); // MEER 2025.04.28

            string tempFileName = "infer.bmp";
            Mat resultOri = new Mat();
            Cv2.HConcat(new Mat[] { leftCrop, rightCrop }, resultOri);
            Cv2.ImWrite(tempFileName, resultOri);
        }

        private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {

            string imageSaveFolder = Machine.config.setup.ImagePath;
            string Month = inspTime.ToString("MM");
            string Date = inspTime.ToString("dd");

            string folderPath = System.IO.Path.Combine(imageSaveFolder, Month, Date, doorTrimID + "_" + inspTime.ToString("HHmmss"));
            if (folderPath != null && System.IO.Directory.Exists(folderPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                MessageBox.Show("폴더를 찾을 수 없습니다.");
            }
        }

        private void btnGetImageLevel_Click(object sender, RoutedEventArgs e)
        {

            string imageSaveFolder = Machine.config.setup.ImagePath;
            string Month = inspTime.ToString("MM");
            string Date = inspTime.ToString("dd");

            string SearchPath = System.IO.Path.Combine(imageSaveFolder, Month, Date, doorTrimID + "_" + inspTime.ToString("HHmmss"));

            string rearFileLocation = "";
            string rearSub1FileLocation = "";
            string frontFileLocation = "";


            if (System.IO.Directory.Exists(SearchPath))
            {
                // REAR Sub1 FILE PATH
                List<string> rearSub1ImageFiles = GetImagesByPattern(SearchPath, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();
                if (rearSub1ImageFiles.Count == 0)
                {
                    Console.WriteLine($"Rear 또는 Front 이미지가 없습니다: {SearchPath}");
                    Machine.logger.Write(eLogType.ERROR, $"Rear 또는 Front 이미지가 없습니다: {SearchPath}");
                    MessageBox.Show("Can't find image.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    rearSub1FileLocation = rearSub1ImageFiles[0];
                }
                // FRONT FILE PATH
                List<string> frontImageFiles = GetImagesByPattern(SearchPath, @"^front_\d{6}.bmp_\d{6}_sub_1$").ToList();
                if (frontImageFiles.Count == 0)
                {
                    Console.WriteLine($"Front 또는 Front 이미지가 없습니다: {SearchPath}");
                    Machine.logger.Write(eLogType.ERROR, $"Front 또는 Front 이미지가 없습니다: {SearchPath}");
                    MessageBox.Show("Can't find image.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    frontFileLocation = frontImageFiles[0];
                }

                Mat front = Cv2.ImRead(frontFileLocation,ImreadModes.Grayscale);
                Mat rear = Cv2.ImRead(rearSub1FileLocation, ImreadModes.Grayscale);
                Scalar frontMean = Cv2.Mean(front);
                Scalar rearMean = Cv2.Mean(rear);
                
                MessageBox.Show("전면 밝기 : " + frontMean.Val0.ToString("0.00") + "\n후면 밝기 : " + rearMean.Val0.ToString("0.00"), "Image Level", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
