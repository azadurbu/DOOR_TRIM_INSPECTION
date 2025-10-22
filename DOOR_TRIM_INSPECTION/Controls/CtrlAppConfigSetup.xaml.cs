using DOOR_TRIM_INSPECTION.Class;
using DOOR_TRIM_INSPECTION.Form;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CtrlAppConfigSetup.xaml
    /// </summary>
    public partial class CtrlAppConfigSetup : UserControl
    {
        private Setup setup;
        private AIConfig aiConfig;

        public CtrlAppConfigSetup()
        {
            InitializeComponent();
        }

        private void LoadFormData()
        {
            List<RecipeDB> recipes = Machine.hmcDBHelper.GetRecipes();
            cboFrontLeftDoor.ItemsSource = recipes.Where(r => r.DoorType == "Front Left").ToList();
            cboFrontLeftDoor.DisplayMemberPath = "RecipeName";
            cboFrontLeftDoor.SelectedValuePath = "RecipeID";

            cboFrontRightDoor.ItemsSource = recipes.Where(r => r.DoorType == "Front Right").ToList();
            cboFrontRightDoor.DisplayMemberPath = "RecipeName";
            cboFrontRightDoor.SelectedValuePath = "RecipeID";

            cboRearLeftDoor.ItemsSource = recipes.Where(r => r.DoorType == "Rear Left").ToList();
            cboRearLeftDoor.DisplayMemberPath = "RecipeName";
            cboRearLeftDoor.SelectedValuePath = "RecipeID";

            cboRearRightDoor.ItemsSource = recipes.Where(r => r.DoorType == "Rear Right").ToList();
            cboRearRightDoor.DisplayMemberPath = "RecipeName";
            cboRearRightDoor.SelectedValuePath = "RecipeID";

            List<KeyValuePair<int, string>> days = new List<KeyValuePair<int, string>>()
                                                    {
                                                        new KeyValuePair<int, string>(7,"7"),
                                                        new KeyValuePair<int, string>(15,"15"),
                                                        new KeyValuePair<int, string>(30,"30"),
                                                        new KeyValuePair<int, string>(60,"60"),
                                                        new KeyValuePair<int, string>(180,"180"),
                                                        new KeyValuePair<int, string>(365,"365")
                                                    };
            cboImageStoragePeriod.ItemsSource = days;
            cboImageStoragePeriod.DisplayMemberPath = "Value";
            cboImageStoragePeriod.SelectedValuePath = "Key";


            List<KeyValuePair<int, string>> methods = new List<KeyValuePair<int, string>>()
                                                    {
                                                        new KeyValuePair<int, string>(0,"RULE"),
                                                        new KeyValuePair<int, string>(1,"DEEP"),
                                                        new KeyValuePair<int, string>(2,"HYBRID")
                                                    };
            cboAnalysisMethod.ItemsSource = methods;
            cboAnalysisMethod.DisplayMemberPath = "Value";
            cboAnalysisMethod.SelectedValuePath = "Key";

            chkUseInCalculation.IsChecked = Machine.config.aiConfig.UseInCalculation;
            gvAIClasses.ItemsSource = Machine.config.aiConfig.AIClasses;

            //20250910-AD
            for (int i = 0; i < 24; i++)
            {
                ctrlMasterModeConfig.cbxMasterModeHour.Items.Add(i.ToString("D2"));
            }
            for (int i = 0; i < 60; i += 5)
            {
                ctrlMasterModeConfig.cbxMasterModeMin.Items.Add(i.ToString("D2"));
            }

            var timeParts = Machine.config.setup.MasterModeTestTime.Split(':');
            ctrlMasterModeConfig.cbxMasterModeHour.SelectedItem = timeParts[0].PadLeft(2, '0');
            ctrlMasterModeConfig.cbxMasterModeMin.SelectedItem = timeParts[1].PadLeft(2, '0');
        }


        private void SaveConfig()
        {
            try
            {
                setup = Machine.config.setup;

                setup.RecipeNumFrontLeft = Convert.ToInt32(cboFrontLeftDoor.SelectedValue);
                setup.RecipeNumFrontRight = Convert.ToInt32(cboFrontRightDoor.SelectedValue);
                setup.RecipeNumRearLeft = Convert.ToInt32(cboRearLeftDoor.SelectedValue);
                setup.RecipeNumRearRight = Convert.ToInt32(cboRearRightDoor.SelectedValue);

                setup.TimeoutDataClear = Convert.ToInt32(txtTimeOutDataClr.Text);
                setup.TimeoutGrab = Convert.ToInt32(txtTimeOutGrab.Text);
                setup.TimeoutInsp = Convert.ToInt32(txtTimeOutInsp.Text);

                setup.AnalysisMethod = Convert.ToInt32(cboAnalysisMethod.SelectedValue);
                setup.WorkspacePath = txtAIWorkspace.Text;

                setup.ImageStoragePeriod = Convert.ToInt32(cboImageStoragePeriod.SelectedValue);
                setup.ImagePath = txtImgFolder.Text;

                setup.GONGCD = txtProdCONGCD.Text;
                setup.LINECD = txtProdLINECD.Text;

                StitcherParam stitcherParam = new StitcherParam();
                stitcherParam.CompositingResol = Convert.ToDouble(txtStitchComp.Text);
                stitcherParam.RegistrationResol = Convert.ToDouble(txtStitchingRegRes.Text);
                stitcherParam.SeamEstimationResol = Convert.ToDouble(txtStitchSeam.Text);
                stitcherParam.PanoConfidenceThresh = Convert.ToDouble(txtStitchPano.Text);
                setup.stitcherParam.SetParam(stitcherParam);
                                
                setup.MasterModeTestTime = $"{ctrlMasterModeConfig.cbxMasterModeHour.SelectedValue}:{ ctrlMasterModeConfig.cbxMasterModeMin.SelectedValue}";//20250910-AD


                //char[] MASTER_TEST_SEQUENCE = new char[8];
                //MASTER_TEST_SEQUENCE[0] = chkFL_OK.IsChecked == true ? '1' : '0';
                //MASTER_TEST_SEQUENCE[1] = chkFL_NG.IsChecked == true  ? '1' : '0'; ;
                //MASTER_TEST_SEQUENCE[2] = chkFR_OK.IsChecked == true  ? '1' : '0'; ;
                //MASTER_TEST_SEQUENCE[3] = chkFR_NG.IsChecked == true  ? '1' : '0'; ;
                //MASTER_TEST_SEQUENCE[4] = chkRL_OK.IsChecked == true  ? '1' : '0'; ;
                //MASTER_TEST_SEQUENCE[5] = chkRL_NG.IsChecked == true  ? '1' : '0'; ;
                //MASTER_TEST_SEQUENCE[6] = chkRR_OK.IsChecked == true  ? '1' : '0'; ;
                //MASTER_TEST_SEQUENCE[7] = chkRR_NG.IsChecked == true  ? '1' : '0'; ;


                //setup.USE_MASTER_IMAGES = (bool)chkUseMasterImages.IsChecked;
                //setup.MASTER_TEST_SEQUENCE = new string(MASTER_TEST_SEQUENCE);

                //setup.FL_OK_DOOR_TRIM_ID = txtFL_OK.Text;
                //setup.FL_NG_DOOR_TRIM_ID = txtFL_NG.Text;

                //setup.FR_OK_DOOR_TRIM_ID = txtFR_OK.Text;
                //setup.FR_NG_DOOR_TRIM_ID = txtFR_NG.Text;

                //setup.RL_OK_DOOR_TRIM_ID = txtRL_OK.Text;
                //setup.RL_NG_DOOR_TRIM_ID = txtRL_NG.Text;

                //setup.RR_OK_DOOR_TRIM_ID = txtRR_OK.Text;
                //setup.RR_NG_DOOR_TRIM_ID = txtRR_NG.Text;

                setup.VPP_PATH = txtVppFolder.Text;
                setup.USE_COGNEX_RESULT = (bool)chkUseCognex.IsChecked;
                if ((bool)rdoShowAlgoName.IsChecked)
                    setup.SHOW_ALGO_NAME = true;
                else
                    setup.SHOW_ALGO_NAME = false;
                if (!ctrlMasterModeConfig.ValidateForm())
                    return;
                setup = ctrlMasterModeConfig.GetMasterTestSequence(setup);

                Machine.config.SaveConfig();
                Machine.config.LoadConfig();

                MessageBox.Show("Save Successful!");
            }
            catch (Exception err)
            {
                Machine.logger.WriteException(eLogType.ERROR, err);
            }
        }
        public void LoadConfig()
        {
            try
            {
                setup = Machine.config.setup;

                cboFrontLeftDoor.SelectedValue = setup.RecipeNumFrontLeft;
                cboFrontRightDoor.SelectedValue = setup.RecipeNumFrontRight;
                cboRearLeftDoor.SelectedValue = setup.RecipeNumRearLeft;
                cboRearRightDoor.SelectedValue = setup.RecipeNumRearRight;

                txtTimeOutDataClr.Text = setup.TimeoutDataClear.ToString();
                txtTimeOutGrab.Text = setup.TimeoutGrab.ToString();
                txtTimeOutInsp.Text = setup.TimeoutInsp.ToString();

                cboAnalysisMethod.SelectedValue = setup.AnalysisMethod;
                txtAIWorkspace.Text = setup.WorkspacePath;

                cboImageStoragePeriod.SelectedValue = setup.ImageStoragePeriod.ToString();
                txtImgFolder.Text = setup.ImagePath;

                txtProdCONGCD.Text = setup.GONGCD;
                txtProdLINECD.Text = setup.LINECD;

                txtStitchComp.Text = setup.stitcherParam.CompositingResol.ToString();
                txtStitchingRegRes.Text = setup.stitcherParam.RegistrationResol.ToString();
                txtStitchSeam.Text = setup.stitcherParam.SeamEstimationResol.ToString();
                txtStitchPano.Text = setup.stitcherParam.PanoConfidenceThresh.ToString();


                ctrlMasterModeConfig.SetMasterTestSequence();

                //chkUseMasterImages.IsChecked = setup.USE_MASTER_IMAGES;
                //string MASTER_TEST_SEQUENCE = setup.MASTER_TEST_SEQUENCE;

                //chkFL_OK.IsChecked = MASTER_TEST_SEQUENCE[0] == '1';
                //chkFL_NG.IsChecked = MASTER_TEST_SEQUENCE[1] == '1';

                //chkFR_OK.IsChecked = MASTER_TEST_SEQUENCE[2] == '1';
                //chkFR_NG.IsChecked = MASTER_TEST_SEQUENCE[3] == '1';

                //chkRL_OK.IsChecked = MASTER_TEST_SEQUENCE[4] == '1';
                //chkRL_NG.IsChecked = MASTER_TEST_SEQUENCE[5] == '1';

                //chkRR_OK.IsChecked = MASTER_TEST_SEQUENCE[6] == '1';
                //chkRR_NG.IsChecked = MASTER_TEST_SEQUENCE[7] == '1';

                //txtFL_OK.Text = setup.FL_OK_DOOR_TRIM_ID;
                //txtFL_NG.Text = setup.FL_NG_DOOR_TRIM_ID;

                //txtFR_OK.Text = setup.FR_OK_DOOR_TRIM_ID;
                //txtFR_NG.Text = setup.FR_NG_DOOR_TRIM_ID;

                //txtRL_OK.Text = setup.RL_OK_DOOR_TRIM_ID;
                //txtRL_NG.Text = setup.RL_NG_DOOR_TRIM_ID;

                //txtRR_OK.Text = setup.RR_OK_DOOR_TRIM_ID;
                //txtRR_NG.Text = setup.RR_NG_DOOR_TRIM_ID;
                txtVppFolder.Text = setup.VPP_PATH;
                chkUseCognex.IsChecked = setup.USE_COGNEX_RESULT;
                if (setup.SHOW_ALGO_NAME)
                    rdoShowAlgoName.IsChecked = true;
                else
                    rdoShowROIName.IsChecked = true;
                var timeParts = Machine.config.setup.MasterModeTestTime.Split(':');
                ctrlMasterModeConfig.cbxMasterModeHour.SelectedItem = timeParts[0].PadLeft(2, '0');
                ctrlMasterModeConfig.cbxMasterModeMin.SelectedItem = timeParts[1].PadLeft(2, '0');
            }
            catch (Exception ex) { Machine.logger.WriteException(eLogType.ERROR, ex); }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFormData();
            LoadConfig();
            LoadAIClasses();
        }

        private void LoadAIClasses()
        {
            // LOAD AI CLASSES
            aiConfig = Machine.config.aiConfig;
            chkUseInCalculation.IsChecked = aiConfig.UseInCalculation;
            gvAIClasses.ItemsSource = aiConfig.AIClasses;
        }

        private void BtnSaveAppSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveAIConfig();
            SaveConfig();
        }

        private void SaveAIConfig()
        {
            try
            {
                aiConfig = Machine.config.aiConfig;

                // READ AI CONFIG FROM THE GRID
                List<AIClass> classList = new List<AIClass>();
                foreach (var item in gvAIClasses.Items)
                {
                    if (item is AIClass aiClass)
                    {
                        classList.Add(aiClass);
                    }
                }
                aiConfig.UseInCalculation = (bool)chkUseInCalculation.IsChecked;

                Machine.config.SaveAIConfig();
                Machine.config.LoadAIConfig();
            }
            catch (Exception err)
            {
                Machine.logger.WriteException(eLogType.ERROR, err);
            }

        }

        private void BtnSelectImageFolder_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string InputFolder = folderBrowserDialog.SelectedPath;
                    txtImgFolder.Text = InputFolder;
                }
            }
        }

        private void BtnSelectAIWorkSpace_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Workpsace files (*.vrws)|*.vrws";

                System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    // COPY TO APPLICATION FOLDER
                    txtAIWorkspace.Text = filePath;
                }
            }
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
        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            //CheckBox source = sender as CheckBox;

            //if (chkFL_F_OK == source)
            //    chkFL_R_OK.IsChecked = true;
            //if (chkFL_R_OK == source)
            //    chkFL_F_OK.IsChecked = true;
            //if (chkFL_F_NG == source)
            //    chkFL_R_NG.IsChecked = true;
            //if (chkFL_R_NG == source)
            //    chkFL_F_NG.IsChecked = true;

            //if (chkFR_F_OK == source)
            //    chkFR_R_OK.IsChecked = true;
            //if (chkFR_R_OK == source)
            //    chkFR_F_OK.IsChecked = true;
            //if (chkFR_F_NG == source)
            //    chkFR_R_NG.IsChecked = true;
            //if (chkFR_R_NG == source)
            //    chkFR_F_NG.IsChecked = true;

            //if (chkRL_F_OK == source)
            //    chkRL_R_OK.IsChecked = true;
            //if (chkRL_R_OK == source)
            //    chkRL_F_OK.IsChecked = true;
            //if (chkRL_F_NG == source)
            //    chkRL_R_NG.IsChecked = true;
            //if (chkRL_R_NG == source)
            //    chkRL_F_NG.IsChecked = true;

            //if (chkRR_F_OK == source)
            //    chkRR_R_OK.IsChecked = true;
            //if (chkRR_R_OK == source)
            //    chkRR_F_OK.IsChecked = true;
            //if (chkRR_F_NG == source)
            //    chkRR_R_NG.IsChecked = true;
            //if (chkRR_R_NG == source)
            //    chkRR_F_NG.IsChecked = true;
        }

        private void btnMasterImg_Click(object sender, RoutedEventArgs e)
        {
            //string DirectoryPath = "";
            //string newFrontFile = "";
            //string newRearFile = "";
            //string newCenterFile = "";
            //string newLeftFile = "";
            //string newRightFile = "";
            //string DoorTrimID = "";
            //string ScanTime = "";
            //using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
            //{
            //    dialog.ValidateNames = false; // 파일 이름 검증 비활성화
            //    dialog.CheckFileExists = false; // 실제 파일 존재 여부 비활성화
            //    dialog.CheckPathExists = true; // 경로 검증 활성화
            //    dialog.FileName = "폴더를 선택하세요"; // 표시될 파일 이름
            //    // 파일 필터를 설정하여 특정 파일을 숨김
            //    dialog.Filter = "폴더만 보기|*.no_extension";

            //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    {
            //        string folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
            //        Console.WriteLine($"선택된 폴더: {folderPath}");
            //        //    }
            //        //}

            //        //using (System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            //        //{
            //        //    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //        //    {
            //        //DirectoryPath = folderDialog.SelectedPath;
            //        DirectoryPath = folderPath;
            //        string DirectoryName = DirectoryPath.Substring(DirectoryPath.LastIndexOf('\\') + 1);

            //        string[] parts = DirectoryName.Split('_');
            //        if (parts.Length == 2)
            //        {
            //            DoorTrimID = parts[0];
            //            ScanTime = parts[1];
            //            // Expected file names
            //            string frontFile = System.IO.Path.Combine(DirectoryPath, $"front_{ScanTime}.bmp");
            //            string rearFile = System.IO.Path.Combine(DirectoryPath, $"rear_{ScanTime}.bmp");
            //            string centerFile = System.IO.Path.Combine(DirectoryPath, $"rear_{ScanTime}.bmp_{ScanTime}_sub_1.bmp");
            //            string leftFile = System.IO.Path.Combine(DirectoryPath, $"rear_{ScanTime}.bmp_{ScanTime}_sub_0.bmp");
            //            string rightFile = System.IO.Path.Combine(DirectoryPath, $"rear_{ScanTime}.bmp_{ScanTime}_sub_2.bmp");

            //            // New file names
            //            newFrontFile = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_front.bmp");
            //            newRearFile = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_rear.bmp");
            //            newCenterFile = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_center.bmp");
            //            newLeftFile = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_left.bmp");
            //            newRightFile = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_right.bmp");

            //            try
            //            {
            //                if (!System.IO.File.Exists(frontFile) ||
            //                    !System.IO.File.Exists(rearFile) ||
            //                    !System.IO.File.Exists(centerFile) ||
            //                    !System.IO.File.Exists(leftFile) ||
            //                    !System.IO.File.Exists(rightFile))
            //                {
            //                    MessageBox.Show("All Required Files Could Not Be Found, Please select another door trim image");
            //                    return;
            //                }


            //                System.IO.File.Copy(frontFile, newFrontFile, true);
            //                System.IO.File.Copy(rearFile, newRearFile, true);
            //                System.IO.File.Copy(centerFile, newCenterFile, true);
            //                System.IO.File.Copy(leftFile, newLeftFile, true);
            //                System.IO.File.Copy(rightFile, newRightFile, true);

            //                Console.WriteLine("Files copied successfully.");
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine($"Error renaming files: {ex.Message}");
            //            }
            //        }
            //    }
            //}



            //// CHECK FILE AVAILABILITY AND IMPORT THEM TO THE WORKPLACE

            //Button source = (Button)sender;
            //if (btnFL_OK == source)
            //{
            //    txtFL_OK.Text = DoorTrimID;
            //}
            //else if (btnFL_NG == source)
            //{
            //    txtFL_NG.Text = DoorTrimID;
            //}
            //else if (btnFR_OK == source)
            //{
            //    txtFR_OK.Text = DoorTrimID;
            //}
            //else if (btnFR_NG == source)
            //{
            //    txtFR_NG.Text = DoorTrimID;
            //}
            //else if (btnRL_OK == source)
            //{
            //    txtRL_OK.Text = DoorTrimID;
            //}
            //else if (btnRL_NG == source)
            //{
            //    txtRL_NG.Text = DoorTrimID;
            //}
            //else if (btnRR_OK == source)
            //{
            //    txtRR_OK.Text = DoorTrimID;
            //}
            //else if (btnRR_NG == source)
            //{
            //    txtRR_NG.Text = DoorTrimID;
            //}

        }

        private void Chk_Unchecked(object sender, RoutedEventArgs e)
        {
            //CheckBox source = sender as CheckBox;

            //if (chkFL_OK == source)
            //    chkFL_OK.IsChecked = false;
            //if (chkFL_R_OK == source)
            //    chkFL_F_OK.IsChecked = false;
            //if (chkFL_F_NG == source)
            //    chkFL_R_NG.IsChecked = false;
            //if (chkFL_R_NG == source)
            //    chkFL_F_NG.IsChecked = false;

            //if (chkFR_F_OK == source)
            //    chkFR_R_OK.IsChecked = false;
            //if (chkFR_R_OK == source)
            //    chkFR_F_OK.IsChecked = false;
            //if (chkFR_F_NG == source)
            //    chkFR_R_NG.IsChecked = false;
            //if (chkFR_R_NG == source)
            //    chkFR_F_NG.IsChecked = false;

            //if (chkRL_F_OK == source)
            //    chkRL_R_OK.IsChecked = false;
            //if (chkRL_R_OK == source)
            //    chkRL_F_OK.IsChecked = false;
            //if (chkRL_F_NG == source)
            //    chkRL_R_NG.IsChecked = false;
            //if (chkRL_R_NG == source)
            //    chkRL_F_NG.IsChecked = false;

            //if (chkRR_F_OK == source)
            //    chkRR_R_OK.IsChecked = false;
            //if (chkRR_R_OK == source)
            //    chkRR_F_OK.IsChecked = false;
            //if (chkRR_F_NG == source)
            //    chkRR_R_NG.IsChecked = false;
            //if (chkRR_R_NG == source)
            //    chkRR_F_NG.IsChecked = false;
        }

        private void BtnMasterModeTest_Click(object sender, RoutedEventArgs e)
        {
            MasterModeInspectionWindow masterTestWindow = new MasterModeInspectionWindow(true);
            masterTestWindow.CALLED_FROM = "SETTINGS";
            masterTestWindow.Show();
        }

        private void BtnMasterTestResult_Click(object sender, RoutedEventArgs e)
        {
            MasterTestResultWindow resultWindow = new MasterTestResultWindow();

            Window parent = Window.GetWindow(this);

            resultWindow.Owner = parent;

            resultWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            resultWindow.Left = parent.Left + (parent.Width - resultWindow.Width) / 2;
            resultWindow.Top = parent.Top + (parent.Height - resultWindow.Height) / 2;

            resultWindow.ShowDialog();
        }

        private void BtnSelectVppFolder_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string InputFolder = folderBrowserDialog.SelectedPath;
                    txtVppFolder.Text = InputFolder;
                }
            }
        }
    }
}
