using DOOR_TRIM_INSPECTION.Class;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;

namespace DOOR_TRIM_INSPECTION
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public HistoryWindow historyWindow = null;
        public SettingsWindow settingsWindow = null;

        private List<DoorTrimInsp> inspResultForGrid = new List<DoorTrimInsp>();
        private string DoorTrimID = string.Empty;
        private Recipe CurrentRecipe;

        private DispatcherTimer _timer;

        private Timer scheduleTimer;
        private TimeSpan triggerTime;
        private bool forceClose = false;
        private DateTime lastRunDate;
        public MainWindow()
        {
            InitializeComponent();

            // Get the usable screen area excluding the taskbar
            var workingArea = SystemParameters.WorkArea;

            // Set the MaxHeight of the window to the height of the usable area
            //this.MaxHeight = workingArea.Height;

            // Optionally, set the height of the window to fit within the available space
            //this.Height = workingArea.Height; // or any custom height, e.g., 80% of the screen height

            InitLanguage("ko-KR");

            // DispatcherTimer 생성 및 설정
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100); // 100ms 간격
            _timer.Tick += Timer_Tick; // Tick 이벤트 핸들러 연결
            _timer.Start(); // 타이머 시작

			Machine.InspectionMode = INSPECTION_MODE.DEFAULT;

            //LoadALCResult();
            //LoadNGItems();
            //SetHWStatus();

            string configuredTime = Machine.config.setup.MasterModeTestTime; // or read from file
            triggerTime = TimeSpan.Parse(configuredTime);
            ScheduleNextTrigger();

        }

        private void ScheduleNextTrigger()
        {
            DateTime now = DateTime.Now;
            DateTime next = now.Date + triggerTime;
            if (next <= now)
                next = next.AddDays(1);

            TimeSpan wait = next - now;

            // If a timer already exists, dispose it
            scheduleTimer?.Dispose();

            // Create a new timer that fires once
            scheduleTimer = new Timer(OnTimerElapsed, null, wait, Timeout.InfiniteTimeSpan);
        }

        private void OnTimerElapsed(object state)
        {
            while (Machine.sequence.SeqStep != eSeqStep.SEQ_READ_BARCODE)
                Thread.Sleep(100);
            // We’re on a ThreadPool thread — must marshal to UI thread
            Dispatcher.BeginInvoke((Action)(() =>
            {
                //while (Machine.sequence.SeqStep != eSeqStep.SEQ_START)
                //    Thread.Sleep(100);
                
                // Close the main window
                ClearCanvas();
                SolidColorBrush brushTrying = new SolidColorBrush(Colors.SeaGreen);
                btnStartStop.Background = brushTrying;
                Machine.ProgramMode = eProgramMode.Stop;
                Machine.sequence.StopSequencs();
                forceClose = true;
                this.Close(); // from inside MainWindow
                if (IsMasterTestPending())
                    StartMasterInspection(true);
                else
                {
                    StartMasterInspection(false);
                }
            }));
        }

        private bool IsMasterTestPending()
        {
            bool isPending = false;
            if (System.IO.File.Exists(MasterResultFile))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(MasterResultFile))
                {
                    string line = reader.ReadLine();
                    string[] literals = line.Split(',');
                    foreach (string lit in literals)
                    {
                        if (lit == "-1" || lit == "0")
                        {
                            isPending = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(MasterResultFile, false)) // 'false' overwrites the file
                {
                    string line = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},-1,-1,-1,-1,-1,-1,-1,-1";

                    writer.WriteLine(line);
                }
                return true;
            }
            return isPending;
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
        private void StartMasterInspection(bool continueTest)
        {
            Form.MasterModeInspectionWindow masterInspWindow = new Form.MasterModeInspectionWindow(continueTest);
            masterInspWindow.Closing += MasterInspWindow_Closing;
            masterInspWindow.Show();
        }

        private void MasterInspWindow_Closing(object sender, CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        // Tick 이벤트 핸들러
        private void Timer_Tick(object sender, EventArgs e)
        {
            SetHWStatus(); // 100ms마다 실행
        }

        private void SetHWStatus()
        {
            // Get Camera status
            bool[] isCamOpen = new bool[Machine.config.setup.maxCamCount];
            for (int i = 0; i < isCamOpen.Length; i++)
                isCamOpen[i] = Machine.camManager.IsOpen(i);
            ctrlCommStatus.SetCam1Comm(isCamOpen);
            // Set Camera status view
            // Get PLC status
            ctrlCommStatus.SetPLCComm(true);
            // Set PLC status view
            // Get Light status
            ctrlCommStatus.SetLightComm(true);
            // Set Light status view
            ctrlCommStatus.UpdateStatus();
        }

        private void LoadNGItems()
        {
            List<DoorTrimInsp> NGItems = new List<DoorTrimInsp>();
            for (int i = 0; i < 30; i++)
            {
                DoorTrimInsp NGItem = new DoorTrimInsp($"Screw{i}", "Screw", "NG");
                NGItems.Add(NGItem);
            }
            gvNgInspResult.ItemsSource = NGItems;
        }

        private void LoadALCResult()
        {
            List<DoorTrimInsp> ALCitems = new List<DoorTrimInsp>();
            for (int i = 0; i < 30; i++)
            {
                DoorTrimInsp ALCItem = new DoorTrimInsp($"ALC{i}", $"CODE{i}", "NG");
                ALCitems.Add(ALCItem);
            }
            gvAlcInspResult.ItemsSource = ALCitems;
        }

        private void InitLanguage(string Locale)
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Locale)
            {
                case "en-US":
                    dict.Source = new Uri("..\\Resources\\Strings.xaml", UriKind.Relative);
                    break;
                case "ko-KR":
                    dict.Source = new Uri("..\\Resources\\Strings.ko-KR.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }

        private void BtnExitApplication_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            //HistoryWindow hWin = new HistoryWindow();
            //hWin.ShowDialog();
            if (historyWindow == null)
            {
                historyWindow = new HistoryWindow();
                historyWindow.Closed += (s, args) => historyWindow = null;
                historyWindow.Show();
            }
            else
            {
                if (historyWindow.WindowState == WindowState.Minimized)
                {
                    historyWindow.WindowState = WindowState.Normal;
                }
                historyWindow.Activate();
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            //SettingsWindow stWin = new SettingsWindow();
            //stWin.ShowDialog();
            if (settingsWindow == null)
            {
                settingsWindow = new SettingsWindow();
                settingsWindow.Closed += (s, args) => settingsWindow = null;
                settingsWindow.Show();
            }
            else
            {
                if (settingsWindow.WindowState == WindowState.Minimized)
                {
                    settingsWindow.WindowState = WindowState.Normal;
                }
                settingsWindow.Activate();
            }
        }

        public void UpdateUI(Inspection inspection, string DoorTrimID, DateTime BarcodeReadTime, string rearImagePath, string frontImagePath)
        {
            ctrlImageVIew.ZoomClear();
            ctrlImageVIew.LoadImages(rearImagePath, frontImagePath, inspection, DoorTrimID, BarcodeReadTime); // MEER 2025.01.24 FOR HISTORY IMAGE
            SetResult(inspection.FrontInspectionResultCode, inspection.RearInspectionResultCode);
            SetNGData(inspection);
            SetFrontInspectionBorder(inspection.FrontInspectionResultCode);
            inspResultForGrid = inspection.SaveInspectionResult(DoorTrimID, BarcodeReadTime, inspResultForGrid);
            LoadInspectionResultIntoGrid();
            LoadInspectionSummary(inspection.GetInspectionSummary());
        }

        public void UpdateUITestGrab(Mat rearImage)
        {
            ctrlImageVIew.ZoomClear();
            ctrlImageVIew.LoadImage(rearImage);
        }

        private void LoadInspectionSummary(InspectionSummary inspectionSummary)
        {
            ctrlInspectionSummary.SetInspectionSummaryResult(inspectionSummary);
        }

        public void ClearCanvasSetBarcode(string _barcode)
        {
            pnlInspectionResult.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 128, 128, 128));
            ctrlInspectionSummary.ClearInspectionSummaryResult();
            lblInspectionResult.Content = "-";
            lblBarCode.Content = _barcode;
            DoorTrimID = _barcode;
            ctrlImageVIew.Clear();
            gvAlcInspResult.ItemsSource = null;
            gvAlcInspResult.Items.Clear();
            gvNgInspResult.ItemsSource = null;
            gvNgInspResult.Items.Clear();
            SetALCView();
            LoadInspectionResultIntoGrid();
        }

        private void LoadInspectionResultIntoGrid()
        {
            gvAlcInspResult.ItemsSource = null;
            gvAlcInspResult.ItemsSource = inspResultForGrid;
        }

        private void SetALCView()
        {
            int idx = 0;
            inspResultForGrid = new List<DoorTrimInsp>();
            try
            {
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA01), ""));
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA02), ""));
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA03), ""));
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA04), ""));
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA05), ""));
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA06), ""));
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA07), ""));
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA08), ""));
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA09), ""));
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA10), ""));
                inspResultForGrid.Add(new DoorTrimInsp(Machine.ALC_KOREAN[idx++], Machine.ALCData.ChageXtoBar(Machine.ALCData.SPA11), ""));
            }
            catch (Exception e) { }
        }

        public void ClearCanvas()
        {
            pnlInspectionResult.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 128, 128, 128));
            lblInspectionResult.Content = "-";
            lblBarCode.Content = "-";
            ctrlImageVIew.Clear();
            gvAlcInspResult.ItemsSource = null;
            gvAlcInspResult.Items.Clear();
            gvNgInspResult.ItemsSource = null;
            gvNgInspResult.Items.Clear();
            ctrlInspectionSummary.ClearInspectionSummaryResult();
        }
        public void SetResult(INSPECTION_RESULT Frontresult, INSPECTION_RESULT Rearresult)
        {
            if (Frontresult == INSPECTION_RESULT.OK && Rearresult == INSPECTION_RESULT.OK)
            {
                pnlInspectionResult.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 46, 139, 87));
                lblInspectionResult.Content = "OK";

            }
            else
            {
                pnlInspectionResult.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 255, 0, 0));
                lblInspectionResult.Content = "NG";
            }
        }

        /// <summary>
        /// 검사 로직
        /// </summary>
        private void PerformInspection()
        {
            foreach (var folder in barcodeFolders)
            {
                // 검사 로직 (동기적 수행)
                ProcessBarcodeFolder(folder);
            }
        }

        private List<DirectoryInfo> barcodeFolders = new List<DirectoryInfo>(); // 모든 바코드 폴더 리스트
        private int currentBarcodeFolderIndex = 0; // 현재 바코드 폴더 인덱스

        private void lblInspectionResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ClearCanvas();
            string tempImagesFolderPath = @"D:\JW\01. 진행\27. 덕양\program\31RNNH22502060003_080933";

            // 모든 바코드 폴더를 로드 (최초 실행 시)
            //if (!barcodeFolders.Any())
            //{
            //    DirectoryInfo rootFolder = new DirectoryInfo(tempImagesFolderPath);
            //    DirectoryInfo[] dateFolders = rootFolder.GetDirectories("*", SearchOption.TopDirectoryOnly);

            //    foreach (var dateFolder in dateFolders)
            //    {
            //        // 날짜 폴더 안의 바코드 폴더 추가
            //        barcodeFolders.AddRange(dateFolder.GetDirectories("*", SearchOption.TopDirectoryOnly));
            //    }

            //    if (!barcodeFolders.Any())
            //    {
            //        MessageBox.Show("처리할 바코드 폴더가 없습니다.");
            //        return;
            //    }
            //}

            // 현재 바코드 폴더 처리
            //if (currentBarcodeFolderIndex < barcodeFolders.Count)
            {
                //DirectoryInfo currentFolder = barcodeFolders[currentBarcodeFolderIndex];
                DirectoryInfo currentFolder = new DirectoryInfo(tempImagesFolderPath);
                ProcessBarcodeFolder(currentFolder); // 바코드 폴더 처리
                // UI 업데이트
                //lblStatus.Content = $"Processing barcode folder {currentBarcodeFolderIndex + 1} of {barcodeFolders.Count}";

                // 다음 폴더로 이동
                currentBarcodeFolderIndex++;
            }
            //else
            //{
            //    // 모든 폴더 처리 완료
            //    MessageBox.Show("모든 바코드 폴더를 처리했습니다.");
            //    currentBarcodeFolderIndex = 0;
            //    barcodeFolders.Clear();
            //}
        }

        /// <summary>
        /// 바코드 폴더 처리 로직
        /// </summary>
        private void ProcessBarcodeFolder(DirectoryInfo folder)
        {
            try
            {
                string barcode = folder.Name.Substring(0, 17); // 바코드 이름 (폴더 이름)
                ClearCanvasSetBarcode(barcode);
                Console.WriteLine($"Processing barcode folder: {barcode}");
                Machine.logger.Write(eLogType.INSPECTION, $"Processing barcode folder: {barcode}");

                // 이미지 파일 검색
                //string[] rearImages = GetImagesByPattern(folder.FullName, @"^rear_\d{6}$");
                //string[] frontImages = GetImagesByPattern(folder.FullName, @"^front_\d{6}$");

                string[] rearSub1Images = GetImagesByPattern(folder.FullName, @"rear_\d{6}.bmp_\d{6}_sub_1$");

                string[] rearImages = GetImagesByPattern(folder.FullName, @"^rear_\d{6}.bmp_\d{6}_sub_\d{1}$");
                string[] frontImages = GetImagesByPattern(folder.FullName, @"^front_\d{6}.bmp_\d{6}_sub_\d{1}$");

                if (!rearImages.Any() || !frontImages.Any() || !rearSub1Images.Any())
                {
                    Console.WriteLine($"Rear 또는 Front 이미지가 없습니다: {folder.FullName}");
                    Machine.logger.Write(eLogType.INSPECTION, $"Processing barcode folder: {barcode}");
                    return;
                }

                Machine.LoadImagesFront = new Mat[frontImages.Length];
                Machine.LoadImagesRear  = new Mat[rearImages.Length];
                for (int i = 0; i < frontImages.Length; i++)
                {
                    Machine.LoadImagesFront[i] = Cv2.ImRead(frontImages[i]);
                    Machine.LoadImagesRear[i] = Cv2.ImRead(rearImages[i]);
                }
                Machine.ProgramMode = eProgramMode.Test;

                // 바코드 및 레시피 설정
                Machine.BarcodeScan.Barcode = barcode;
                //Machine.BarcodeData = new BarCodeHelper(barcode);
                //Machine.ALCData = Machine.dyDBHelper.GetALCMIS3PF(new BarCodeHelper(barcode));
                //Machine.sequence.BarcodeReadTime = DateTime.Now;
                //SetCurrentRecipe();

                //// 검사 수행
                //Inspection inspection = new Inspection(CurrentRecipe);
                //inspection.IsTrialInspection = true;
                //bool isEqual = false;
                //if (folder.Parent.Name == "06" || folder.Parent.Name == "05" || folder.Parent.Name == "04" || folder.Parent.Name == "03")
                //    isEqual = true;
                //// Rear 이미지 검사
                //inspection.SetRearInspectionImage(rearImages[0], isEqual); // 첫 번째 Rear 이미지 사용
                //inspection.SetRearSub1InspectionImage(rearSub1Images[0]); // 첫 번째 Rear 이미지 사용
                //inspection.ExecuteRearInspection();

                //// Front 이미지 검사
                //inspection.SetFrontInspectionImage(frontImages[0], isEqual); // 첫 번째 Front 이미지 사용
                //inspection.ExecuteFrontInspection();


                //string preName = Machine.config.setup.ImagePath + "\\";
                //string fPath = SaveMergedImage(barcode, inspection.Getimage(true), true);
                //string rPath = SaveMergedImage(barcode, inspection.Getimage(false), false);

                //// 검사 결과 처리
                //ctrlImageVIew.ZoomClear();
                //ctrlImageVIew.LoadImages(rPath, fPath, inspection, barcode, Machine.sequence.BarcodeReadTime);

                //SetNGData(inspection);
                //SetResult(inspection.RearInspectionResultCode);
                //SetALCView();
                //LoadInspectionResultIntoGrid();
                //LoadInspectionSummary(inspection.GetInspectionSummary());

                //inspection.SaveInspectionResult(DoorTrimID, Machine.sequence.BarcodeReadTime, inspResultForGrid);

                //Console.WriteLine($"검사가 완료되었습니다: {folder.FullName}");
                //Machine.logger.Write(eLogType.INSPECTION, $"검사가 완료되었습니다: {folder.FullName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류 발생: {ex.ToString()}");
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
            }
        }

        public void ShowWrongBarcode()
        {
            lblBarCode.Content = (string)FindResource("TXT_WRONG_BARCODE");
            pnlInspectionResult.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 128, 128, 128));
            ctrlInspectionSummary.ClearInspectionSummaryResult();
            lblInspectionResult.Content = "-";
            ctrlImageVIew.Clear();
            gvAlcInspResult.ItemsSource = null;
            gvNgInspResult.ItemsSource = null;
        }

        private string SaveMergedImage(string doorTrimID, Mat mergedImage, Mat[] subimage, DateTime _barcodeReadTime, bool IsFront)
        {
            string imageSaveFolder = Machine.config.setup.ImagePath;

            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string Month = _barcodeReadTime.ToString("MM");
            string Date = _barcodeReadTime.ToString("dd");

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Month);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, Date);
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, doorTrimID + "_" + _barcodeReadTime.ToString("HHmmss"));
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string filePath = System.IO.Path.Combine(imageSaveFolder, IsFront ? "front" : "rear");
            filePath += "_" + _barcodeReadTime.ToString("HHmmss") + ".bmp";
            string subfilePath = filePath + "_" + _barcodeReadTime.ToString("HHmmss") + "_sub";
            Cv2.ImWrite(filePath, mergedImage);
            for (int i = 0; i < subimage.Length; i++)
            {
                Cv2.ImWrite(subfilePath + "_" + i + ".bmp", subimage[i]);
            }

            return filePath;
        }

        private string SaveMergedImage(string doorTrimID, Mat mergedImage, bool IsFront)
        {
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

            imageSaveFolder = System.IO.Path.Combine(imageSaveFolder, doorTrimID + "_" + _barcodeReadTime.ToString("HHmmss"));
            if (!Directory.Exists(imageSaveFolder))
                Directory.CreateDirectory(imageSaveFolder);

            string filePath = System.IO.Path.Combine(imageSaveFolder, IsFront ? "front" : "rear");
            filePath += "_" + _barcodeReadTime.ToString("HHmmss") + ".bmp";
            Cv2.ImWrite(filePath, mergedImage);
            //mergedImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return filePath;
        }

        /// <summary>
        /// 지정된 패턴으로 파일 검색
        /// </summary>
        private string[] GetImagesByPattern(string folderPath, string pattern)
        {
            Regex regex = new Regex(pattern);
            return Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly)
                            .Where(file => regex.IsMatch(Path.GetFileNameWithoutExtension(file)))
                            .ToArray();
        }

        /// <summary>
        /// 현재 레시피 설정
        /// </summary>
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

        private void SetNGData(Inspection inspection)
        {
            List<DoorTrimInsp> NGItems = new List<DoorTrimInsp>();
            foreach (PlugInspectionItem result in inspection.RearInspectionResult.PlugInspectionResult)
            {
                if (result.InspectionResult != INSPECTION_RESULT.OK)
                {
                    NGItems.Add(new DoorTrimInsp(result.DetectionClassName, result.RegionID.ToString(), "NG"));
                }
            }
            foreach (ScrewInspectionItem result in inspection.RearInspectionResult.ScrewInspectionResult)
            {
                if (result.InspectionResult != INSPECTION_RESULT.OK)
                {
                    NGItems.Add(new DoorTrimInsp(result.DetectionClassName, result.RegionID.ToString(), "NG"));
                }
            }
            foreach (BoltInspectionItem result in inspection.RearInspectionResult.BoltInspectionResult)
            {
                if (result.InspectionResult != INSPECTION_RESULT.OK)
                {
                    NGItems.Add(new DoorTrimInsp(result.DetectionClassName, result.RegionID.ToString(), "NG"));
                }
            }
            foreach (PadInspectionItem result in inspection.RearInspectionResult.PadInspectionResult)
            {
                if (result.InspectionResult != INSPECTION_RESULT.OK)
                {
                    NGItems.Add(new DoorTrimInsp(result.DetectionClassName, result.RegionID.ToString(), "NG"));
                }
            }
            foreach (SpeakerInspectionItem result in inspection.RearInspectionResult.SpeakerInspectionResult)
            {
                if (result.InspectionResult != INSPECTION_RESULT.OK)
                {
                    NGItems.Add(new DoorTrimInsp(result.DetectionClassName, result.RegionID.ToString(), "NG"));
                }
            }
            foreach (SmallPadInspectionItem result in inspection.RearInspectionResult.SmallPadInspectionResult)
            {
                if (result.InspectionResult != INSPECTION_RESULT.OK)
                {
                    NGItems.Add(new DoorTrimInsp(result.DetectionClassName, result.RegionID.ToString(), "NG"));
                }
            }
            foreach (ScrewMacthInspectionItem result in inspection.RearInspectionResult.ScrewMacthInspectionResult)
            {
                if (result.InspectionResult != INSPECTION_RESULT.OK)
                {
                    NGItems.Add(new DoorTrimInsp(result.DetectionClassName, result.RegionID.ToString(), "NG"));
                }
            }
            foreach (PlugMatchInspectionItem result in inspection.RearInspectionResult.PlugMatchInspectionResult)
            {
                if (result.InspectionResult != INSPECTION_RESULT.OK)
                {
                    NGItems.Add(new DoorTrimInsp(result.DetectionClassName, result.RegionID.ToString(), "NG"));
                }
            }
            gvNgInspResult.ItemsSource = NGItems;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Machine.sequence = new Sequence(this);

            string strStop = (string)FindResource("TXT_STOP");
            btnStartStop.Content = strStop;
            Machine.sequence.StartSequence();
            SolidColorBrush brushTrying = new SolidColorBrush(Colors.Crimson);
            btnStartStop.Background = brushTrying;
        }

        private void BtnStartStop_Click(object sender, RoutedEventArgs e)
        {
            string strStart = (string)FindResource("TXT_START");
            string strStop = (string)FindResource("TXT_STOP");

            if (btnStartStop.Content.ToString() != strStop)
            {
                btnGrab.Visibility = Visibility.Hidden;
                btnStartStop.Content = strStop;
                Machine.sequence.StartSequence();
                SolidColorBrush brushTrying = new SolidColorBrush(Colors.Crimson);
                btnStartStop.Background = brushTrying;
                //AddZoomSlider();
            }
            else
            {
                MessageBoxResult result = MessageBox.Show( "검사모드를 정지하시겠습니까?", "경고", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    btnStartStop.Content = strStart;
                    ClearCanvas();
                    SolidColorBrush brushTrying = new SolidColorBrush(Colors.SeaGreen);
                    btnStartStop.Background = brushTrying;
                    Machine.ProgramMode = eProgramMode.Stop;
                    Machine.sequence.StopSequencs();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(Machine.ProgramMode != eProgramMode.Stop)
            {
                MessageBox.Show("검사가 진행중입니다.\r\n프로그램을 종료하시려면 정지모드로 변경 해주세요.");
                e.Cancel = true; // 종료 취소
                return;
            }
            // MessageBox를 표시

            if (!forceClose)
            {
                var result = MessageBox.Show("정말로 종료하시겠습니까?", "종료 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

                // Yes를 선택하지 않은 경우 종료를 취소
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true; // 종료 취소
                }
                else
                {
                    Machine.ProgramMode = eProgramMode.Stop;
                    Machine.Light_Comm.Terminate();
                    Machine.camManager.Terminate();
                    Machine.dyDBHelper.Terminate();
                    Machine.hmcDBHelper.Terminate();
                    ((App)Application.Current).ClearRecipeImages();
                }
            }
        }

        private void lblBarCode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ctrlImageVIew.SaveCanvasBitmap();
        }

        public void SetFrontInspectionBorder(INSPECTION_RESULT inspResult)
        {
            if (INSPECTION_RESULT.NG == inspResult)
                ctrlImageVIew.borderFront.BorderBrush = Brushes.Red;
            else
                ctrlImageVIew.borderFront.BorderBrush = Brushes.GreenYellow;
        }

        private void TxtApplicationName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Controls.CustomFileSelectionBox cbox = new Controls.CustomFileSelectionBox();
            //cbox.Show();
        }

        private void ImgLogo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // 더블클릭 여부 확인
            {
                if (btnGrab.Visibility == Visibility.Visible)
                {
                    btnGrab.Visibility = Visibility.Hidden;
                    return;
                }
                string strStart = (string)FindResource("TXT_START");
                if (btnStartStop.Content.ToString() == strStart)
                    btnGrab.Visibility = Visibility.Visible;
            }
        }

        private void btnGrab_Click(object sender, RoutedEventArgs e)
        {
            ClearCanvas();
            try
            {
                Machine.Light_Comm.LightOnOffEN(true, 1, 1600);
                System.Threading.Thread.Sleep(200);
                Machine.camManager.StartGrab();
                DateTime start = DateTime.Now;
                DateTime end = DateTime.Now;

                TimeSpan timeout = TimeSpan.FromSeconds(5); // 최대 대기 시간 설정 5)
                while (true)
                {
                    end = DateTime.Now;

                    TimeSpan elapsedTime = end - start; // 경과 시간 계산

                    // 경과 시간이 타임아웃을 초과하면 종료
                    if (elapsedTime > timeout)
                    {
                        Machine.camManager.StopGrab();
                        Console.WriteLine("Timeout occurred!");
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
                    if (Machine.camManager.IsGrabbing() == false)
                        break;
                }

                //Merge Image
                Mat[] Images = new Mat[3];
                Images[0] = Machine.camManager.GetGrabImage(0);
                Images[1] = Machine.camManager.GetGrabImage(1);
                Images[2] = Machine.camManager.GetGrabImage(2);

                if (Images[0] == null || Images[1] == null || Images[2] == null)
                {
                    Machine.Light_Comm.LightOnOffEN(true, 1, 0);
                    return;
                }
                Cv2.Transpose(Images[0], Images[0]);
                Cv2.Flip(Images[0], Images[0], FlipMode.Y);
                //Cv2.Transpose(Images[1], Images[1]);
                //Cv2.Flip(Images[1], Images[1], FlipMode.Y);
                Cv2.Transpose(Images[2], Images[2]);
                Cv2.Flip(Images[2], Images[2], FlipMode.Y);

                //MergeImageFirst = MergeImagesStitcher(Images);
                Mat MergeImage = MergeImagesMat(Images, true);
                DateTime now = DateTime.Now;
                for (int i = 0; i < 3; i++)
                    Images[i].ImWrite("Manual_" + i + "_"+now.ToString("hhMMss")+".bmp");
                Task.Run(() =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.UpdateUITestGrab(MergeImage);
                    });
                });
                Machine.camManager.StopGrab();
                Machine.Light_Comm.LightOnOffEN(true, 1, 0);
            }
            catch
            {
                Machine.camManager.StopGrab();
                Machine.Light_Comm.LightOnOffEN(true, 1, 0);
            }
        }

        private Mat MergeImagesMat(Mat[] imgs, bool IsFirst = false)
        {
            int yoffset = 98;
            int xoffset01 = 548;
            int xoffset02 = 234;
            Mat left = imgs[0].Clone();
            Mat right = imgs[2].Clone();
            Mat leftCrop = new Mat(left, new OpenCvSharp.Rect(0, 0, left.Width - xoffset02, left.Height - yoffset));
            Mat rightCrop = new Mat(right, new OpenCvSharp.Rect(xoffset01, yoffset, left.Width - xoffset01, left.Height - yoffset));

            Mat result = new Mat();

            if (IsFirst) // MEER 2025.01.23 SAVE ORIGINAL IMAGES FOR DEEP LEARNING
            {
                string tempFileName = "infer.bmp";
                Mat resultOri = new Mat();
                Cv2.HConcat(new Mat[] { leftCrop, rightCrop }, resultOri);
                Cv2.ImWrite(tempFileName, resultOri);
            }

            leftCrop = LevelOps.EqualizeHistColor(leftCrop);
            rightCrop = LevelOps.EqualizeHistColor(rightCrop);

            Cv2.HConcat(new Mat[] { leftCrop, rightCrop }, result);

            return result;
        }

        private void TxtApplicationName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Machine.dyDBHelper.UpdateCycleTime(new BarCodeHelper("17RXNH22509110215"), "4.03");
            //Form.MasterModeInspectionWindow startupWindow = new Form.MasterModeInspectionWindow();
            //startupWindow.Show();
        }
    }
}
