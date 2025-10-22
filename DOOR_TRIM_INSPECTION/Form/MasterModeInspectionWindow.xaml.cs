using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Controls;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class InspectionSequence
    {
        public string RearFilePath { get; set; }
        public string FrontFilePath { get; set; }
        public string CenterFilePath { get; set; }
        public string DoorTrimID { get; set; }
        public string BarCodeReadTime { get; set; }
        public int RecipeID { get; set; }
        public Label Label { get; set; }
        public INSPECTION_RESULT DesiredInspectionResult { get; set; }
        public INSPECTION_RESULT InspectionResult { get; set; }
        public Inspection Inspection { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsInspected { get; set; }
        public ImageViewer ImageViewer { get; set; }
    }

    /// <summary>
    /// Interaction logic for MasterModeInspectionWindow.xaml
    /// </summary>
    public partial class MasterModeInspectionWindow : System.Windows.Window
    {
        private BackgroundWorker bgWorker;
        private BackgroundWorker bgWorkerAuto;

        private Label source;
        private List<InspectionSequence> InspectionSequences = new List<InspectionSequence>();

        public MasterModeInspectionWindow(bool continueTest)
        {
            InitializeComponent();

            if (!continueTest)
                SaveTestResult(); // THIS WILL RESET THE TEST 

            Machine.InspectionMode = INSPECTION_MODE.MASTER;
            bgWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            //bgWorker.ProgressChanged += BgWorker_ProgressChanged;
        }

        private void SetUI()
        {
            lblFLOK.Content = (string)FindResource("TXT_FRONT_LEFT_DOOR") + " OK";
            lblFLNG.Content = (string)FindResource("TXT_FRONT_LEFT_DOOR") + " NG";
            lblFROK.Content = (string)FindResource("TXT_FRONT_RIGHT_DOOR") + " OK";
            lblFRNG.Content = (string)FindResource("TXT_FRONT_RIGHT_DOOR") + " NG";
            lblRLOK.Content = (string)FindResource("TXT_REAR_LEFT_DOOR") + " OK";
            lblRLNG.Content = (string)FindResource("TXT_REAR_LEFT_DOOR") + " NG";
            lblRROK.Content = (string)FindResource("TXT_REAR_RIGHT_DOOR") + " OK";
            lblRRNG.Content = (string)FindResource("TXT_REAR_RIGHT_DOOR") + " NG";
            if (!System.IO.File.Exists(MasterResultFile))
                InitTestResult();
            using (System.IO.StreamReader reader = new System.IO.StreamReader(MasterResultFile))
            {
                string line = reader.ReadLine();
                string[] literals = line.Split(',');

                if (literals.Length == 9)
                {
                    lblFLOK.Background = literals[1] == "1" ? brushOK : (literals[1] == "0" ? brushNG : brushPending);
                    lblFLNG.Background = literals[2] == "1" ? brushOK : (literals[2] == "0" ? brushNG : brushPending);
                    lblFROK.Background = literals[3] == "1" ? brushOK : (literals[3] == "0" ? brushNG : brushPending);
                    lblFRNG.Background = literals[4] == "1" ? brushOK : (literals[4] == "0" ? brushNG : brushPending);
                    lblRLOK.Background = literals[5] == "1" ? brushOK : (literals[5] == "0" ? brushNG : brushPending);
                    lblRLNG.Background = literals[6] == "1" ? brushOK : (literals[6] == "0" ? brushNG : brushPending);
                    lblRROK.Background = literals[7] == "1" ? brushOK : (literals[7] == "0" ? brushNG : brushPending);
                    lblRRNG.Background = literals[8] == "1" ? brushOK : (literals[8] == "0" ? brushNG : brushPending);
                }
            }

        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                InspectionSequence seq = (InspectionSequence)e.Result;

                if (seq.Label == lblFLOK)
                {
                    Console.WriteLine("Finished Front Left OK Image");
                    lblStatus.Content = "Finished Front Left OK Image";
                    seq.IsCorrect = !seq.Inspection.AnyNG;
                }
                if (seq.Label == lblFLNG)
                {
                    Console.WriteLine("Finished Front Left NG Image");
                    lblStatus.Content = "Finished Front Left NG Image";
                    seq.IsCorrect = !seq.Inspection.AnyOK;
                }
                if (seq.Label == lblFROK)
                {
                    Console.WriteLine("Finished Front Right OK Image");
                    lblStatus.Content = "Finished Front Right OK Image";
                    seq.IsCorrect = !seq.Inspection.AnyNG;
                }
                if (seq.Label == lblFRNG)
                {
                    Console.WriteLine("Finished Front Right NG Image");
                    lblStatus.Content = "Finished Front Right NG Image";
                    seq.IsCorrect = !seq.Inspection.AnyOK;
                }
                if (seq.Label == lblRLOK)
                {
                    Console.WriteLine("Finished Rear Left OK Image");
                    lblStatus.Content = "Finished Rear Left OK Image";
                    seq.IsCorrect = !seq.Inspection.AnyNG;
                }
                if (seq.Label == lblRLNG)
                {
                    Console.WriteLine("Finished Rear Left NG Image");
                    lblStatus.Content = "Finished Rear Left NG Image";
                    seq.IsCorrect = !seq.Inspection.AnyOK;
                }
                if (seq.Label == lblRROK)
                {
                    Console.WriteLine("Finished Rear Right OK Image");
                    lblStatus.Content = "Finished Rear Right OK Image";
                    seq.IsCorrect = !seq.Inspection.AnyNG;
                }
                if (seq.Label == lblRRNG)
                {
                    Console.WriteLine("Finished Rear Right NG Image");
                    lblStatus.Content = "Finished Rear Right NG Image";
                    seq.IsCorrect = !seq.Inspection.AnyOK;
                }
                SetResult(seq.Label, !seq.IsCorrect ? "FAIL" : "PASS");
                ctrlImageViewer.Clear();
                ctrlImageViewer.ZoomClear();
                ctrlImageViewer.LoadImages(seq.RearFilePath, seq.FrontFilePath, seq.Inspection, seq.DoorTrimID, DateTime.Now); // MEER 2025.01.24 FOR HISTORY IMAGE
                if (INSPECTION_RESULT.NG == seq.Inspection.FrontInspectionResultCode)
                    ctrlImageViewer.borderFront.BorderBrush = Brushes.Red;
                else
                    ctrlImageViewer.borderFront.BorderBrush = Brushes.GreenYellow;
                ctrlImageViewer.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
            }
        }

        public void ClearCanvas()
        {
            ctrlImageViewer.Clear();
        }

        public void UpdateUI(Inspection inspection, string DoorTrimID, DateTime BarcodeReadTime, string rearImagePath, string frontImagePath)
        {
            try
            {
                ctrlImageViewer.Clear();
                ctrlImageViewer.ZoomClear();
                ctrlImageViewer.Visibility = Visibility.Visible;
                ctrlImageViewer.LoadImages(rearImagePath, frontImagePath, inspection, DoorTrimID, BarcodeReadTime); // MEER 2025.01.24 FOR HISTORY IMAGE
                if (INSPECTION_RESULT.NG == inspection.FrontInspectionResultCode)
                    ctrlImageViewer.borderFront.BorderBrush = Brushes.Red;
                else
                    ctrlImageViewer.borderFront.BorderBrush = Brushes.GreenYellow;
                //ctrlImageViewer.Visibility = Visibility.Visible;

                if (lblFLOK == source || lblFROK == source || lblRLOK == source || lblRROK == source)
                {
                    SetResult(source, inspection.AnyNG ? "FAIL" : "PASS");
                }
                else
                {
                    SetResult(source, inspection.AnyOK ? "FAIL" : "PASS");
                }
                Machine.sequence.StopSequencs();
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                InspectionSequence seq = (InspectionSequence)e.Argument;
                // 바코드 및 레시피 설정
                Machine.BarcodeData = new BarCodeHelper(seq.DoorTrimID);
                Machine.ALCData = Machine.dyDBHelper.GetALCMIS3PF(new BarCodeHelper(seq.DoorTrimID));
                Recipe CurrentRecipe = new Recipe(seq.RecipeID);

                // 검사 수행
                Inspection inspection = new Inspection(CurrentRecipe);
                // Rear 이미지 검사
                inspection.SetRearInspectionImage(seq.RearFilePath, false); // 첫 번째 Rear 이미지 사용
                inspection.SetRearSub1InspectionImage(seq.CenterFilePath); // 첫 번째 Rear sub1 이미지 사용
                inspection.ExecuteRearInspection();

                // Front 이미지 검사
                inspection.SetFrontInspectionImage(seq.FrontFilePath, false); // 첫 번째 Front 이미지 사용
                inspection.ExecuteFrontInspection();

                seq.InspectionResult = inspection.RearInspectionResultCode;
                seq.Inspection = inspection;
                e.Result = seq;
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            InitializeForm();
        }

        private SolidColorBrush brushPending = new SolidColorBrush(Colors.DarkGray);
        private SolidColorBrush brushTrying = new SolidColorBrush(Colors.WhiteSmoke);
        private SolidColorBrush brushOK = new SolidColorBrush(Colors.SeaGreen);
        private SolidColorBrush brushNG = new SolidColorBrush(Colors.Crimson);
        private void InitializeForm()
        {
            char[] MASTER_TEST_SEQUENCE = Machine.config.setup.MASTER_TEST_SEQUENCE.ToCharArray();

            lblFLOK.Visibility = MASTER_TEST_SEQUENCE[0] == '0' ? Visibility.Collapsed : Visibility.Visible;
            lblFLNG.Visibility = MASTER_TEST_SEQUENCE[1] == '0' ? Visibility.Collapsed : Visibility.Visible;
            lblFROK.Visibility = MASTER_TEST_SEQUENCE[2] == '0' ? Visibility.Collapsed : Visibility.Visible;
            lblFRNG.Visibility = MASTER_TEST_SEQUENCE[3] == '0' ? Visibility.Collapsed : Visibility.Visible;
            lblRLOK.Visibility = MASTER_TEST_SEQUENCE[4] == '0' ? Visibility.Collapsed : Visibility.Visible;
            lblRLNG.Visibility = MASTER_TEST_SEQUENCE[5] == '0' ? Visibility.Collapsed : Visibility.Visible;
            lblRROK.Visibility = MASTER_TEST_SEQUENCE[6] == '0' ? Visibility.Collapsed : Visibility.Visible;
            lblRRNG.Visibility = MASTER_TEST_SEQUENCE[7] == '0' ? Visibility.Collapsed : Visibility.Visible;

            SetUI();
        }


        private void SetStatus(string status)
        {
            lblStatus.Content = status;
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

        public string CALLED_FROM { get; internal set; }
        public bool USE_MASTER_IMAGES { get { return Machine.config.setup.USE_MASTER_IMAGES; } }
        public void SetResult(Label lbl, string result)
        {
            if (result == "*")
                lbl.Background = brushTrying;
            else if (result == "PASS")
                lbl.Background = brushOK;
            else
                lbl.Background = brushNG;
        }

        private void LblResult_MouseDown(object sender, MouseButtonEventArgs e)
        {
            source = sender as Label;
            ctrlImageViewer.Visibility = Visibility.Collapsed;

            if (USE_MASTER_IMAGES)
            {
                InspectionSequence seq = new InspectionSequence();
                string FrontFileSuffix = "";
                int RecipeID = 0;
                INSPECTION_RESULT DesiredInspectionResult = INSPECTION_RESULT.NOT_FOUND;

                if (lblFLOK == source)
                {
                    FrontFileSuffix = Machine.config.setup.FL_OK_F_IMG;
                    RecipeID = Machine.config.setup.RecipeNumFrontLeft;
                    DesiredInspectionResult = INSPECTION_RESULT.OK;
                }
                else if (lblFLNG == source)
                {
                    FrontFileSuffix = Machine.config.setup.FL_NG_F_IMG;
                    RecipeID = Machine.config.setup.RecipeNumFrontLeft;
                    DesiredInspectionResult = INSPECTION_RESULT.NG;
                }
                else if (lblFROK == source)
                {
                    FrontFileSuffix = Machine.config.setup.FR_OK_F_IMG;
                    RecipeID = Machine.config.setup.RecipeNumFrontRight;
                    DesiredInspectionResult = INSPECTION_RESULT.OK;
                }
                else if (lblFRNG == source)
                {
                    FrontFileSuffix = Machine.config.setup.FR_NG_F_IMG;
                    RecipeID = Machine.config.setup.RecipeNumFrontRight;
                    DesiredInspectionResult = INSPECTION_RESULT.NG;
                }
                else if (lblRLOK == source)
                {
                    FrontFileSuffix = Machine.config.setup.RL_OK_F_IMG;
                    RecipeID = Machine.config.setup.RecipeNumRearLeft;
                    DesiredInspectionResult = INSPECTION_RESULT.OK;
                }
                else if (lblRLNG == source)
                {
                    FrontFileSuffix = Machine.config.setup.RL_NG_F_IMG;
                    RecipeID = Machine.config.setup.RecipeNumRearLeft;
                    DesiredInspectionResult = INSPECTION_RESULT.NG;
                }
                else if (lblRROK == source)
                {
                    FrontFileSuffix = Machine.config.setup.RR_OK_F_IMG;
                    RecipeID = Machine.config.setup.RecipeNumRearRight;
                    DesiredInspectionResult = INSPECTION_RESULT.OK;
                }
                else if (lblRRNG == source)
                {
                    FrontFileSuffix = Machine.config.setup.RR_NG_F_IMG;
                    RecipeID = Machine.config.setup.RecipeNumRearRight;
                    DesiredInspectionResult = INSPECTION_RESULT.NG;
                }

                seq = new InspectionSequence()
                {
                    RearFilePath = System.IO.Path.Combine(MasterImagePath, $"{FrontFileSuffix}_rear.bmp"),
                    FrontFilePath = System.IO.Path.Combine(MasterImagePath, $"{FrontFileSuffix}_front.bmp"),
                    CenterFilePath = System.IO.Path.Combine(MasterImagePath, $"{FrontFileSuffix}_center.bmp"),
                    DoorTrimID = FrontFileSuffix,
                    RecipeID = RecipeID,
                    Label = source,
                    DesiredInspectionResult = DesiredInspectionResult
                };
                SaveImageForInference(FrontFileSuffix);

                if (!bgWorker.IsBusy)
                    bgWorker.RunWorkerAsync(argument: seq);
            }
            else
            {
                if(Machine.sequence!=null)
                Machine.sequence.StopSequencs();
                Machine.sequence = new Sequence(this);
                Machine.sequence.StartSequence();
                ClearCanvas();
            }
            source.Background = brushTrying;
        }

        private void SaveImageForInference(string DoorTrimID)
        {
            //int yoffset = 98;
            //int xoffset01 = 548;
            //int xoffset02 = 234;
            int yoffset = 127;
            int xoffset01 = 469;
            int xoffset02 = 467;

            Mat left = Cv2.ImRead(System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_left.bmp"));
            Mat right = Cv2.ImRead(System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_right.bmp"));
            Mat leftCrop = new Mat(left, new OpenCvSharp.Rect(0, 0, left.Width - xoffset02, left.Height - yoffset));
            Mat rightCrop = new Mat(right, new OpenCvSharp.Rect(xoffset01, yoffset, left.Width - xoffset01, left.Height - yoffset));

            Mat result = new Mat();


            string tempFileName = "infer.bmp";
            Mat resultOri = new Mat();
            Cv2.HConcat(new Mat[] { leftCrop, rightCrop }, resultOri);
            Cv2.ImWrite(tempFileName, resultOri);
        }

        private void BtnRetry_Click(object sender, RoutedEventArgs e)
        {
            //ExecuteTest();
        }

        private void BtnOverride_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool CanClose()
        {
            bool canClose = true;
            if (lblFLOK.Visibility == Visibility.Visible)
                canClose &= lblFLOK.Background == brushOK;
            if (lblFROK.Visibility == Visibility.Visible)
                canClose &= lblFLNG.Background == brushOK;
            if (lblFROK.Visibility == Visibility.Visible)
                canClose &= lblFROK.Background == brushOK;
            if (lblFRNG.Visibility == Visibility.Visible)
                canClose &= lblFRNG.Background == brushOK;
            if (lblRLOK.Visibility == Visibility.Visible)
                canClose &= lblRLOK.Background == brushOK;
            if (lblRLNG.Visibility == Visibility.Visible)
                canClose &= lblRLNG.Background == brushOK;
            if (lblRROK.Visibility == Visibility.Visible)
                canClose &= lblRROK.Background == brushOK;
            if (lblRRNG.Visibility == Visibility.Visible)
                canClose &= lblRRNG.Background == brushOK;
            return canClose;
        }

        private string MasterResultFile
        {
            get
            {
                return System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                         "master_test_result.txt");
            }
        }

        private void InitTestResult()
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(MasterResultFile, false)) // 'false' overwrites the file
            {
                string line = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},-1,-1,-1,-1,-1,-1,-1,-1";

                writer.WriteLine(line);
            }
        }

        private void SaveTestResult(bool allPass = false)
        {
            string line = "";
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(MasterResultFile, false)) // 'false' overwrites the file
            {
                if (allPass)
                    line = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},1,1,1,1,1,1,1,1";
                else
                    line = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},{GetStatus(lblFLOK)},{GetStatus(lblFLNG)}," +
                       $"{GetStatus(lblFROK)},{GetStatus(lblFRNG)},{GetStatus(lblRLOK)},{GetStatus(lblRLNG)},{GetStatus(lblRROK)},{GetStatus(lblRRNG)}";

                writer.WriteLine(line);
            }
        }

        private string GetStatus(Label label)
        {
            return (label.Background == brushOK) ? "1" : ((label.Background == brushNG) ? "0" : "-1");
        }

        private void BtnPass_Click(object sender, RoutedEventArgs e)
        {

            // Show password prompt dialog
            PasswordPromptWindow passwordPrompt = new PasswordPromptWindow();
            var result = passwordPrompt.ShowDialog();

            if (result == true ) // Replace with your actual password validation logic
            {

                //if (CALLED_FROM == "SETTINGS")
                //    Machine.InspectionMode = INSPECTION_MODE.DEFAULT;
                if(passwordPrompt.Password != "1234")
                {
                    MessageBox.Show("비밀번호가 틀렸습니다.");
                    return;
                }
                // save pass result
                SaveTestResult(true);
                this.Close();
                Machine.InspectionMode = INSPECTION_MODE.DEFAULT;

                if (CALLED_FROM == "SETTINGS")
                    return;
                //MainWindow mainWindow = new MainWindow();
                //mainWindow.Show();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // check result visible item all pass
            if (CanClose())
            {
                SaveTestResult(true);
                Machine.InspectionMode = INSPECTION_MODE.DEFAULT;
                this.Close();
            }
            else
            {
                MessageBox.Show("검사를 모두 진행하세요.");
            }
        }

        private void btnAuto_Click(object sender, RoutedEventArgs e)
        {
            ctrlImageViewer.Visibility = Visibility.Collapsed;
            bgWorkerAuto = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            bgWorkerAuto.DoWork += BgWorker_DoWork_Auto;
            bgWorkerAuto.RunWorkerCompleted += BgWorker_RunWorkerCompleted_Auto;
            bgWorkerAuto.ProgressChanged += BgWorker_ProgressChanged_Auto;
            ExecuteTest();
        }

        private void BgWorker_ProgressChanged_Auto(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                InspectionSequence seq = (InspectionSequence)e.UserState;

                if (seq.Label == lblFLOK)
                {
                    Console.WriteLine("Finished Front Left OK Image");
                    lblStatus.Content = "Finished Front Left OK Image";
                    seq.IsCorrect = !seq.Inspection.AnyNG;
                }
                if (seq.Label == lblFLNG)
                {
                    Console.WriteLine("Finished Front Left NG Image");
                    lblStatus.Content = "Finished Front Left NG Image";
                    seq.IsCorrect = !seq.Inspection.AnyOK;
                }
                if (seq.Label == lblFROK)
                {
                    Console.WriteLine("Finished Front Right OK Image");
                    lblStatus.Content = "Finished Front Right OK Image";
                    seq.IsCorrect = !seq.Inspection.AnyNG;
                }
                if (seq.Label == lblFRNG)
                {
                    Console.WriteLine("Finished Front Right NG Image");
                    lblStatus.Content = "Finished Front Right NG Image";
                    seq.IsCorrect = !seq.Inspection.AnyOK;
                }
                if (seq.Label == lblRLOK)
                {
                    Console.WriteLine("Finished Rear Left OK Image");
                    lblStatus.Content = "Finished Rear Left OK Image";
                    seq.IsCorrect = !seq.Inspection.AnyNG;
                }
                if (seq.Label == lblRLNG)
                {
                    Console.WriteLine("Finished Rear Left NG Image");
                    lblStatus.Content = "Finished Rear Left NG Image";
                    seq.IsCorrect = !seq.Inspection.AnyOK;
                }
                if (seq.Label == lblRROK)
                {
                    Console.WriteLine("Finished Rear Right OK Image");
                    lblStatus.Content = "Finished Rear Right OK Image";
                    seq.IsCorrect = !seq.Inspection.AnyNG;
                }
                if (seq.Label == lblRRNG)
                {
                    Console.WriteLine("Finished Rear Right NG Image");
                    lblStatus.Content = "Finished Rear Right NG Image";
                    seq.IsCorrect = !seq.Inspection.AnyOK;
                }

                // FIND RESULT 
                SetResult(seq.Label, !seq.IsCorrect ? "FAIL" : "PASS");
                seq.ImageViewer.ZoomClear();
                seq.ImageViewer.RecipeID = seq.RecipeID;
                seq.ImageViewer.Visibility = Visibility.Visible;
                seq.ImageViewer.LoadImages(seq.RearFilePath, seq.FrontFilePath, seq.Inspection, seq.DoorTrimID, DateTime.Now);
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
            }
        }

        private void BgWorker_RunWorkerCompleted_Auto(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                HistoryImageViewer imgViewer = (HistoryImageViewer)e.Result;
                imgViewer.Visibility = Visibility.Visible;

                lblStatus.Content = "Test Coompleted!";
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
            }
        }

        private void BgWorker_DoWork_Auto(object sender, DoWorkEventArgs e)
        {
            try
            {
                //List<InspectionSequence> inspSeqs = (List<InspectionSequence>)e.Argument;
                foreach (InspectionSequence seq in InspectionSequences)
                {
                    // 바코드 및 레시피 설정
                    Machine.BarcodeData = new BarCodeHelper(seq.DoorTrimID);
                    Machine.ALCData = Machine.dyDBHelper.GetALCMIS3PF(new BarCodeHelper(seq.DoorTrimID));
                    Recipe CurrentRecipe = new Recipe(seq.RecipeID);

                    // 검사 수행
                    Inspection inspection = new Inspection(CurrentRecipe);
                    //inspection.IsTrialInspection = false;

                    // Rear 이미지 검사
                    inspection.SetRearInspectionImage(seq.RearFilePath, false); // 첫 번째 Rear 이미지 사용
                    inspection.SetRearSub1InspectionImage(seq.CenterFilePath); // 첫 번째 Rear sub1 이미지 사용
                    inspection.ExecuteRearInspection();

                    // Front 이미지 검사
                    inspection.SetFrontInspectionImage(seq.FrontFilePath, false); // 첫 번째 Front 이미지 사용
                    inspection.ExecuteFrontInspection();

                    seq.InspectionResult = inspection.RearInspectionResultCode;
                    seq.Inspection = inspection;
                    bgWorkerAuto.ReportProgress(20, seq);
                }
                if (InspectionSequences.Count > 0)
                    e.Result = InspectionSequences[InspectionSequences.Count - 1].ImageViewer;
            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
            }
        }


        private void AddInspectionSequence(string DoorTrimID, int recipeID, INSPECTION_RESULT desiredResult, Label label, ImageViewer imgViewer)
        {
            string frontFilePath = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_front.bmp");
            string rearFilePath = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_rear.bmp");
            string centerFilePath = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_center.bmp");

            if (!System.IO.File.Exists(frontFilePath))
            {
                MessageBox.Show($"Can't find image for {DoorTrimID}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.IO.File.Exists(rearFilePath) || !System.IO.File.Exists(centerFilePath))
            {
                MessageBox.Show($"Can't find image for {DoorTrimID}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            InspectionSequences.Add(new InspectionSequence()
            {
                RearFilePath = rearFilePath,
                FrontFilePath = frontFilePath,
                CenterFilePath = centerFilePath,
                DoorTrimID = DoorTrimID,
                RecipeID = recipeID,
                Label = label,
                ImageViewer = imgViewer,
                DesiredInspectionResult = desiredResult
            });
        }

        private void ExecuteTest()
        {
            string DoorTrimID = "";
            InspectionSequences.Clear();
            int RecipeID = 0;
            if (lblFLOK.Visibility == Visibility.Visible)
            {
                DoorTrimID = Machine.config.setup.FL_OK_DOOR_TRIM_ID; 
                
                RecipeID = Machine.config.setup.RecipeNumFrontLeft;
                SetResult(lblFLOK, "*");
                AddInspectionSequence(DoorTrimID,  RecipeID, INSPECTION_RESULT.OK, lblFLOK, ctrlImageVIewFLOK);
            }

            if (lblFLNG.Visibility == Visibility.Visible)
            {
                DoorTrimID = Machine.config.setup.FL_NG_DOOR_TRIM_ID; 
               
                RecipeID = Machine.config.setup.RecipeNumFrontLeft;
                SetResult(lblFLNG, "*");
                AddInspectionSequence(DoorTrimID,  RecipeID, INSPECTION_RESULT.NG, lblFLNG, ctrlImageVIewFLNG);
            }

            if (lblFROK.Visibility == Visibility.Visible )
            {
                DoorTrimID = Machine.config.setup.FR_OK_DOOR_TRIM_ID;
                
                RecipeID = Machine.config.setup.RecipeNumFrontRight;
                SetResult(lblFROK, "*");
                AddInspectionSequence(DoorTrimID,  RecipeID, INSPECTION_RESULT.OK, lblFROK, ctrlImageVIewFROK);
            }

            if (lblFRNG.Visibility == Visibility.Visible )
            {
                DoorTrimID = Machine.config.setup.FR_NG_DOOR_TRIM_ID;
               
                RecipeID = Machine.config.setup.RecipeNumFrontRight;
                SetResult(lblFRNG, "*");
                AddInspectionSequence(DoorTrimID,  RecipeID, INSPECTION_RESULT.NG, lblFRNG, ctrlImageVIewFRNG);
            }

            if (lblRLOK.Visibility == Visibility.Visible )
            {
                DoorTrimID = Machine.config.setup.RL_OK_DOOR_TRIM_ID;
                
                RecipeID = Machine.config.setup.RecipeNumRearLeft;
                SetResult(lblRLOK, "*");
                AddInspectionSequence(DoorTrimID,  RecipeID, INSPECTION_RESULT.OK, lblRLOK, ctrlImageVIewRLOK);
            }

            if (lblRLNG.Visibility == Visibility.Visible)
            {
                DoorTrimID = Machine.config.setup.RL_NG_DOOR_TRIM_ID;
               
                RecipeID = Machine.config.setup.RecipeNumRearLeft;
                SetResult(lblRLNG, "*");
                AddInspectionSequence(DoorTrimID,  RecipeID, INSPECTION_RESULT.NG, lblRLNG, ctrlImageVIewRLNG);
            }

            if (lblRROK.Visibility == Visibility.Visible )
            {
                DoorTrimID = Machine.config.setup.RR_OK_DOOR_TRIM_ID;
                
                RecipeID = Machine.config.setup.RecipeNumRearRight;
                SetResult(lblRROK, "*");
                AddInspectionSequence(DoorTrimID,  RecipeID, INSPECTION_RESULT.OK, lblRROK, ctrlImageVIewRROK);
            }

            if (lblRRNG.Visibility == Visibility.Visible )
            {
                DoorTrimID = Machine.config.setup.RR_NG_DOOR_TRIM_ID;
                
                RecipeID = Machine.config.setup.RecipeNumRearRight;
                SetResult(lblRRNG, "*");
                AddInspectionSequence(DoorTrimID,  RecipeID, INSPECTION_RESULT.NG, lblRRNG, ctrlImageVIewRRNG);
            }

            if (!bgWorkerAuto.IsBusy)
                bgWorkerAuto.RunWorkerAsync(argument: InspectionSequences);
        }
    }
}
