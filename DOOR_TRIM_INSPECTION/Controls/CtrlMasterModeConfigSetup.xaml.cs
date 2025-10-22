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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CtrlMasterModeConfigSetup.xaml
    /// </summary>
    public partial class CtrlMasterModeConfigSetup : UserControl
    {
        public CtrlMasterModeConfigSetup()
        {
            InitializeComponent();
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

        //private void btnMasterImg_Click(object sender, RoutedEventArgs e)
        //{
        //    string DirectoryPath = "";
        //    string newFrontFile = "";
        //    string newRearFile = "";
        //    string newCenterFile = "";
        //    string newLeftFile = "";
        //    string newRightFile = "";
        //    string DoorTrimID = "";
        //    string ScanTime = "";
        //    using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
        //    {
        //        dialog.ValidateNames = false; // 파일 이름 검증 비활성화
        //        dialog.CheckFileExists = false; // 실제 파일 존재 여부 비활성화
        //        dialog.CheckPathExists = true; // 경로 검증 활성화
        //        dialog.FileName = "폴더를 선택하세요"; // 표시될 파일 이름
        //        // 파일 필터를 설정하여 특정 파일을 숨김
        //        dialog.Filter = "폴더만 보기|*.no_extension";

        //        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //        {
        //            string folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
        //            Console.WriteLine($"선택된 폴더: {folderPath}");
        //            //    }
        //            //}

        //            //using (System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog())
        //            //{
        //            //    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //            //    {
        //            //DirectoryPath = folderDialog.SelectedPath;
        //            DirectoryPath = folderPath;
        //            string DirectoryName = DirectoryPath.Substring(DirectoryPath.LastIndexOf('\\') + 1);

        //            string[] parts = DirectoryName.Split('_');
        //            if (parts.Length == 2)
        //            {
        //                DoorTrimID = parts[0];
        //                ScanTime = parts[1];
        //                // Expected file names
        //                string frontFile = System.IO.Path.Combine(DirectoryPath, $"front_{ScanTime}.bmp");
        //                string rearFile = System.IO.Path.Combine(DirectoryPath, $"rear_{ScanTime}.bmp");
        //                string centerFile = System.IO.Path.Combine(DirectoryPath, $"rear_{ScanTime}.bmp_{ScanTime}_sub_1.bmp");
        //                string leftFile = System.IO.Path.Combine(DirectoryPath, $"rear_{ScanTime}.bmp_{ScanTime}_sub_0.bmp");
        //                string rightFile = System.IO.Path.Combine(DirectoryPath, $"rear_{ScanTime}.bmp_{ScanTime}_sub_2.bmp");

        //                // New file names
        //                newFrontFile = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_front.bmp");
        //                newRearFile = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_rear.bmp");
        //                newCenterFile = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_center.bmp");
        //                newLeftFile = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_left.bmp");
        //                newRightFile = System.IO.Path.Combine(MasterImagePath, $"{DoorTrimID}_right.bmp");

        //                try
        //                {
        //                    if (!System.IO.File.Exists(frontFile) ||
        //                        !System.IO.File.Exists(rearFile) ||
        //                        !System.IO.File.Exists(centerFile) ||
        //                        !System.IO.File.Exists(leftFile) ||
        //                        !System.IO.File.Exists(rightFile))
        //                    {
        //                        MessageBox.Show("All Required Files Could Not Be Found, Please select another door trim image");
        //                        return;
        //                    }


        //                    System.IO.File.Copy(frontFile, newFrontFile, true);
        //                    System.IO.File.Copy(rearFile, newRearFile, true);
        //                    System.IO.File.Copy(centerFile, newCenterFile, true);
        //                    System.IO.File.Copy(leftFile, newLeftFile, true);
        //                    System.IO.File.Copy(rightFile, newRightFile, true);

        //                    Console.WriteLine("Files copied successfully.");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"Error renaming files: {ex.Message}");
        //                }
        //            }
        //        }
        //    }



        // CHECK FILE AVAILABILITY AND IMPORT THEM TO THE WORKPLACE

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

        //}

        public void SetMasterTestSequence()
        {
            Setup setup = Machine.config.setup;

            string MASTER_TEST_SEQUENCE = setup.MASTER_TEST_SEQUENCE;

            chkFL_OK.IsChecked = MASTER_TEST_SEQUENCE[0] == '1';
            chkFL_NG.IsChecked = MASTER_TEST_SEQUENCE[1] == '1';

            chkFR_OK.IsChecked = MASTER_TEST_SEQUENCE[2] == '1';
            chkFR_NG.IsChecked = MASTER_TEST_SEQUENCE[3] == '1';

            chkRL_OK.IsChecked = MASTER_TEST_SEQUENCE[4] == '1';
            chkRL_NG.IsChecked = MASTER_TEST_SEQUENCE[5] == '1';

            chkRR_OK.IsChecked = MASTER_TEST_SEQUENCE[6] == '1';
            chkRR_NG.IsChecked = MASTER_TEST_SEQUENCE[7] == '1';


            txtFL_OK_DOORTRIM.Text = setup.FL_OK_DOOR_TRIM_ID;
            txtFL_OK_F.Text = setup.FL_OK_F_IMG;
            txtFL_OK_R.Text = setup.FL_OK_R_IMG;

            txtFL_NG_DOORTRIM.Text = setup.FL_NG_DOOR_TRIM_ID;
            txtFL_NG_F.Text = setup.FL_NG_F_IMG;
            txtFL_NG_R.Text = setup.FL_NG_R_IMG;


            txtFR_OK_DOORTRIM.Text = setup.FR_OK_DOOR_TRIM_ID;
            txtFR_OK_F.Text = setup.FR_OK_F_IMG;
            txtFR_OK_R.Text = setup.FR_OK_R_IMG;

            txtFR_NG_DOORTRIM.Text = setup.FR_NG_DOOR_TRIM_ID;
            txtFR_NG_F.Text = setup.FR_NG_F_IMG;
            txtFR_NG_R.Text = setup.FR_NG_R_IMG;

            txtRL_OK_DOORTRIM.Text = setup.RL_OK_DOOR_TRIM_ID;
            txtRL_OK_F.Text = setup.RL_OK_F_IMG;
            txtRL_OK_R.Text = setup.RL_OK_R_IMG;

            txtRL_NG_DOORTRIM.Text = setup.RL_NG_DOOR_TRIM_ID;
            txtRL_NG_F.Text = setup.RL_NG_F_IMG;
            txtRL_NG_R.Text = setup.RL_NG_R_IMG;

            txtRR_OK_DOORTRIM.Text = setup.RR_OK_DOOR_TRIM_ID;
            txtRR_OK_F.Text = setup.RR_OK_F_IMG;
            txtRR_OK_R.Text = setup.RR_OK_R_IMG;

            txtRR_NG_DOORTRIM.Text = setup.RR_NG_DOOR_TRIM_ID;
            txtRR_NG_F.Text = setup.RR_NG_F_IMG;
            txtRR_NG_R.Text = setup.RR_NG_R_IMG;

            ActivateDoorTrimTextBoxes();
        }


        public Setup GetMasterTestSequence(Setup setup)
        {
            char[] MASTER_TEST_SEQUENCE = new char[8];
            MASTER_TEST_SEQUENCE[0] = chkFL_OK.IsChecked == true ? '1' : '0';
            MASTER_TEST_SEQUENCE[1] = chkFL_NG.IsChecked == true ? '1' : '0';
            MASTER_TEST_SEQUENCE[2] = chkFR_OK.IsChecked == true ? '1' : '0';
            MASTER_TEST_SEQUENCE[3] = chkFR_NG.IsChecked == true ? '1' : '0';
            MASTER_TEST_SEQUENCE[4] = chkRL_OK.IsChecked == true ? '1' : '0';
            MASTER_TEST_SEQUENCE[5] = chkRL_NG.IsChecked == true ? '1' : '0';
            MASTER_TEST_SEQUENCE[6] = chkRR_OK.IsChecked == true ? '1' : '0';
            MASTER_TEST_SEQUENCE[7] = chkRR_NG.IsChecked == true ? '1' : '0';
            setup.MASTER_TEST_SEQUENCE = new string(MASTER_TEST_SEQUENCE);
            setup.FL_OK_DOOR_TRIM_ID = txtFL_OK_DOORTRIM.Text;
            setup.FL_OK_F_IMG = txtFL_OK_F.Text;
            setup.FL_OK_R_IMG = txtFL_OK_R.Text;

            setup.FL_NG_DOOR_TRIM_ID = txtFL_NG_DOORTRIM.Text;
            setup.FL_NG_F_IMG = txtFL_NG_F.Text;
            setup.FL_NG_R_IMG = txtFL_NG_R.Text;

            setup.FR_OK_DOOR_TRIM_ID = txtFR_OK_DOORTRIM.Text;
            setup.FR_OK_F_IMG = txtFR_OK_F.Text;
            setup.FR_OK_R_IMG = txtFR_OK_R.Text;

            setup.FR_NG_DOOR_TRIM_ID = txtFR_NG_DOORTRIM.Text;
            setup.FR_NG_F_IMG = txtFR_NG_F.Text;
            setup.FR_NG_R_IMG = txtFR_NG_R.Text;

            setup.RL_OK_DOOR_TRIM_ID = txtRL_OK_DOORTRIM.Text;
            setup.RL_OK_F_IMG = txtRL_OK_F.Text;
            setup.RL_OK_R_IMG = txtRL_OK_R.Text;

            setup.RL_NG_DOOR_TRIM_ID = txtRL_NG_DOORTRIM.Text;
            setup.RL_NG_F_IMG = txtRL_NG_F.Text;
            setup.RL_NG_R_IMG = txtRL_NG_R.Text;

            setup.RR_OK_DOOR_TRIM_ID = txtRR_OK_DOORTRIM.Text;
            setup.RR_OK_F_IMG = txtRR_OK_F.Text;
            setup.RR_OK_R_IMG = txtRR_OK_R.Text;

            setup.RR_OK_DOOR_TRIM_ID = txtRR_OK_DOORTRIM.Text;
            setup.RR_NG_F_IMG = txtRR_NG_F.Text;
            setup.RR_NG_R_IMG = txtRR_NG_R.Text;

            return setup;
        }

        private string[] GetImagesByPattern(string folderPath, string pattern)
        {
            Regex regex = new Regex(pattern);
            return System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly)
                            .Where(file => regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(file)))
                            .ToArray();
        }

        private void btnRearMasterImg_Click(object sender, RoutedEventArgs e)
        {
            // COPY ONLY FRONT FILE
            Button source = (Button)sender;

            string filePrefix = "";
            string rearFileSuffix = "_Rear.bmp";
            string leftFileSuffix = "_Left.bmp";
            string centerFileSuffix = "_Center.bmp";
            string rightFileSuffix = "_Right.bmp";
            
            // SELECT FRONT FILES
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp";

                System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // COPY FILE TO THE MASTER FOLDER // MEER 2025.01.15
                    string oriPath = openFileDialog.FileName;

                    // Expected file names
                    //string DirectoryPath = System.IO.Path.GetDirectoryName(oriPath);

                    //string DirectoryName = DirectoryPath.Substring(DirectoryPath.LastIndexOf('\\') + 1);

                    //string[] parts = DirectoryName.Split('_');
                    //if (parts.Length == 2)
                    //{
                    //    string DoorTrimID = parts[0];
                    //    if (DoorTrimID.Length != 17)
                    //    {
                    //        MessageBox.Show($"Path does not contain DoorTrimID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //        return;
                    //    }
                    //    filePrefix = DoorTrimID;
                    //}
                    //else
                    //{
                    //    MessageBox.Show($"Path does not contain DoorTrimID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //    return;
                    //}


                    string SearchPath = System.IO.Path.GetDirectoryName(oriPath);
                    List<string> rearSubImageFiles = new List<string>();
                    if (System.IO.Directory.Exists(SearchPath))
                    {

                        rearSubImageFiles = GetImagesByPattern(SearchPath, @"rear_\d{6}.bmp_\d{6}_sub_\d{1}$").ToList();
                        if (rearSubImageFiles.Count < 3 )
                        {
                            Console.WriteLine($"Rear 이미지가 없습니다: {SearchPath}");
                            Machine.logger.Write(eLogType.ERROR, $"Rear 이미지가 없습니다: {SearchPath}");
                            MessageBox.Show("Can't find image.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    try
                    {
                        filePrefix = GetDoorTrimIDFromTextbox(source);
                        string newRearFilePath = System.IO.Path.Combine(MasterImagePath, filePrefix + rearFileSuffix);
                        System.IO.File.Copy(oriPath, newRearFilePath, true);
                        string newCenterFilePath = "", newLeftFilePath = "", newRightFilePath = "";
                        
                        foreach (string subFilePath in rearSubImageFiles)
                        {
                            if (subFilePath.Contains("sub_0"))
                            {
                                newLeftFilePath = System.IO.Path.Combine(MasterImagePath, filePrefix + leftFileSuffix);
                                System.IO.File.Copy(subFilePath, newLeftFilePath, true);
                            }
                            else if (subFilePath.Contains("sub_1"))
                            {
                                newCenterFilePath = System.IO.Path.Combine(MasterImagePath, filePrefix + centerFileSuffix);
                                System.IO.File.Copy(subFilePath, newCenterFilePath, true);
                            }
                            else if (subFilePath.Contains("sub_2"))
                            {
                                newRightFilePath = System.IO.Path.Combine(MasterImagePath, filePrefix + rightFileSuffix);
                                System.IO.File.Copy(subFilePath, newRightFilePath, true);
                            }
                        }
                        UpdateTextBox(source, newRearFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error renaming files: {ex.Message}");
                        Machine.logger.Write(eLogType.ERROR, $"Error renaming files: {ex.Message}");
                    }
                }
                else
                {
                    UpdateTextBox(source, "");
                }
            }
        }

        private void UpdateTextBox(Button btn, string text)
        {
            if (btnFL_OK_F == btn)
            {
                txtFL_OK_F.Text = text;
            }
            else if (btnFL_NG_F == btn)
            {
                txtFL_NG_F.Text = text;
            }
            else if (btnFR_OK_F == btn)
            {
                txtFR_OK_F.Text = text;
            }
            else if (btnFR_NG_F == btn)
            {
                txtFR_NG_F.Text = text;
            }
            else if (btnRL_OK_F == btn)
            {
                txtRL_OK_F.Text = text;
            }
            else if (btnRL_NG_F == btn)
            {
                txtRL_NG_F.Text = text;
            }
            else if (btnRR_OK_F == btn)
            {
                txtRR_OK_F.Text = text;
            }
            else if (btnRR_NG_F == btn)
            {
                txtRR_NG_F.Text = text;
            }
            else if (btnFL_OK_R == btn)
            {
                txtFL_OK_R.Text = text;
            }
            else if (btnFL_NG_R == btn)
            {
                txtFL_NG_R.Text = text;
            }
            else if (btnFR_OK_R == btn)
            {
                txtFR_OK_R.Text = text;
            }
            else if (btnFR_NG_R == btn)
            {
                txtFR_NG_R.Text = text;
            }
            else if (btnRL_OK_R == btn)
            {
                txtRL_OK_R.Text = text;
            }
            else if (btnRL_NG_R == btn)
            {
                txtRL_NG_R.Text = text;
            }
            else if (btnRR_OK_R == btn)
            {
                txtRR_OK_R.Text = text;
            }
            else if (btnRR_NG_R == btn)
            {
                txtRR_NG_R.Text = text;
            }
        }

        private string GetDoorTrimIDFromTextbox(Button btn)
        {
            if (btnFL_OK_F == btn)
            {
                return txtFL_OK_DOORTRIM.Text;
            }
            else if (btnFL_NG_F == btn)
            {
                return txtFL_NG_DOORTRIM.Text;
            }
            else if (btnFR_OK_F == btn)
            {
                return txtFR_OK_DOORTRIM.Text;
            }
            else if (btnFR_NG_F == btn)
            {
                return txtFR_NG_DOORTRIM.Text;
            }
            else if (btnRL_OK_F == btn)
            {
                return txtRL_OK_DOORTRIM.Text;
            }
            else if (btnRL_NG_F == btn)
            {
                return txtRL_NG_DOORTRIM.Text;
            }
            else if (btnRR_OK_F == btn)
            {
                return txtRR_OK_DOORTRIM.Text;
            }
            else if (btnRR_NG_F == btn)
            {
                return txtRR_NG_DOORTRIM.Text;
            }
            else if (btnFL_OK_R == btn)
            {
                return txtFL_OK_DOORTRIM.Text;
            }
            else if (btnFL_NG_R == btn)
            {
                return txtFL_NG_DOORTRIM.Text;
            }
            else if (btnFR_OK_R == btn)
            {
                return txtFR_OK_DOORTRIM.Text;
            }
            else if (btnFR_NG_R == btn)
            {
                return txtFR_NG_DOORTRIM.Text;
            }
            else if (btnRL_OK_R == btn)
            {
                return txtRL_OK_DOORTRIM.Text;
            }
            else if (btnRL_NG_R == btn)
            {
                return txtRL_NG_DOORTRIM.Text;
            }
            else if (btnRR_OK_R == btn)
            {
                return txtRR_OK_DOORTRIM.Text;
            }
            else if (btnRR_NG_R == btn)
            {
                return txtRR_NG_DOORTRIM.Text;
            }
            return "";
        }

        private void btnFrontMasterImg_Click(object sender, RoutedEventArgs e)
        { 
            
            // COPY ONLY FRONT FILE
            Button source = (Button)sender;

            string filePrefix = "";
            string fileSuffix = "_Front.bmp";
            
            // SELECT FRONT FILES
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp";

                System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // COPY FILE TO THE MASTER FOLDER // MEER 2025.01.15
                    string oriPath = openFileDialog.FileName;

                    //string DirectoryPath = System.IO.Path.GetDirectoryName(oriPath);

                    //string DirectoryName = DirectoryPath.Substring(DirectoryPath.LastIndexOf('\\') + 1);

                    //string[] parts = DirectoryName.Split('_');
                    //if (parts.Length == 2)
                    //{
                    //    string DoorTrimID = parts[0];
                    //    if (DoorTrimID.Length != 17)
                    //    {
                    //        MessageBox.Show($"Path does not contain DoorTrimID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //        return;
                    //    }
                    //    filePrefix = DoorTrimID;
                    //}
                    //else
                    //{
                    //    MessageBox.Show($"Path does not contain DoorTrimID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //    return;
                    //}


                    filePrefix = GetDoorTrimIDFromTextbox(source);
                    // Expected file names
                    string frontFile = System.IO.Path.Combine(MasterImagePath, filePrefix + fileSuffix);

                    try
                    {
                        System.IO.File.Copy(oriPath, frontFile, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error renaming files: {ex.Message}");
                        Machine.logger.Write(eLogType.ERROR, $"Error renaming files: {ex.Message}");
                    }

                    UpdateTextBox(source, frontFile);
                }
                else
                {
                    UpdateTextBox(source, "");
                }
            }
        }



        public bool ValidateForm()
        {
            string strErrorMessage = (string)FindResource("TXT_PLEASE_PROVIDE_IMAGE");
            if (chkFL_OK.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtFL_OK_F.Text) || string.IsNullOrEmpty(txtFL_OK_R.Text))
                {
                    strErrorMessage = "\"" + (string)FindResource("TXT_FRONT_LEFT_DOOR") + " [OK] \"" + strErrorMessage;
                    MessageBox.Show(strErrorMessage);
                    return false;
                }
            }
            if (chkFL_NG.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtFL_NG_F.Text) || string.IsNullOrEmpty(txtFL_NG_R.Text))
                {
                    strErrorMessage = "\"" + (string)FindResource("TXT_FRONT_LEFT_DOOR") + " [NG] \"" + strErrorMessage;
                    MessageBox.Show(strErrorMessage);
                    return false;
                }
            }

            if (chkFR_OK.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtFR_OK_F.Text) || string.IsNullOrEmpty(txtFR_OK_R.Text))
                {
                    strErrorMessage = "\"" + (string)FindResource("TXT_FRONT_RIGHT_DOOR") + " [OK] \"" + strErrorMessage;
                    MessageBox.Show(strErrorMessage);
                    return false;
                }
            }
            if (chkFR_NG.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtFR_NG_F.Text) || string.IsNullOrEmpty(txtFR_NG_R.Text))
                {
                    strErrorMessage = "\"" + (string)FindResource("TXT_FRONT_RIGHT_DOOR") + " [NG] \"" + strErrorMessage;
                    MessageBox.Show(strErrorMessage);
                    return false;
                }
            }

            if (chkRL_OK.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtRL_OK_F.Text) || string.IsNullOrEmpty(txtRL_OK_R.Text))
                {
                    strErrorMessage = "\"" + (string)FindResource("TXT_REAR_LEFT_DOOR") + " [OK] \"" + strErrorMessage;
                    MessageBox.Show(strErrorMessage);
                    return false;
                }
            }
            if (chkRL_NG.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtRL_NG_F.Text) || string.IsNullOrEmpty(txtRL_NG_R.Text))
                {
                    strErrorMessage = "\"" + (string)FindResource("TXT_REAR_LEFT_DOOR") + " [NG] \"" + strErrorMessage;
                    MessageBox.Show(strErrorMessage);
                    return false;
                }
            }

            if (chkRR_OK.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtRR_OK_F.Text) || string.IsNullOrEmpty(txtRR_OK_R.Text))
                {
                    strErrorMessage = "\"" + (string)FindResource("TXT_REAR_RIGHT_DOOR") + " [OK] \"" + strErrorMessage;
                    MessageBox.Show(strErrorMessage);
                    return false;
                }
            }
            if (chkRR_NG.IsChecked == true)
            {
                if (string.IsNullOrEmpty(txtRR_NG_F.Text) || string.IsNullOrEmpty(txtRR_NG_R.Text))
                {
                    strErrorMessage = "\"" + (string)FindResource("TXT_REAR_RIGHT_DOOR") + " [NG] \"" + strErrorMessage;
                    MessageBox.Show(strErrorMessage);
                    return false;
                }
            }
            return true;

        }

        private void ActivateImageSelectionButtons()
        {
            btnFL_OK_F.IsEnabled = txtFL_OK_DOORTRIM.Text.Length == 17;
            btnFL_OK_R.IsEnabled = txtFL_OK_DOORTRIM.Text.Length == 17;

            btnFL_NG_F.IsEnabled = txtFL_NG_DOORTRIM.Text.Length == 17;
            btnFL_NG_R.IsEnabled = txtFL_NG_DOORTRIM.Text.Length == 17;

            btnFR_OK_F.IsEnabled = txtFR_OK_DOORTRIM.Text.Length == 17;
            btnFR_OK_R.IsEnabled = txtFR_OK_DOORTRIM.Text.Length == 17;

            btnFR_NG_F.IsEnabled = txtFR_NG_DOORTRIM.Text.Length == 17;
            btnFR_NG_R.IsEnabled = txtFR_NG_DOORTRIM.Text.Length == 17;

            btnRL_OK_F.IsEnabled = txtRL_OK_DOORTRIM.Text.Length == 17;
            btnRL_OK_R.IsEnabled = txtRL_OK_DOORTRIM.Text.Length == 17;

            btnRL_NG_F.IsEnabled = txtRL_NG_DOORTRIM.Text.Length == 17;
            btnRL_NG_R.IsEnabled = txtRL_NG_DOORTRIM.Text.Length == 17;

            btnRR_OK_F.IsEnabled = txtRR_OK_DOORTRIM.Text.Length == 17;
            btnRR_OK_R.IsEnabled = txtRR_OK_DOORTRIM.Text.Length == 17;

            btnRR_NG_F.IsEnabled = txtRR_NG_DOORTRIM.Text.Length == 17;
            btnRR_NG_R.IsEnabled = txtRR_NG_DOORTRIM.Text.Length == 17;
        }
        private void Txt_DOOR_TRIM_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActivateImageSelectionButtons();
        }


        private void ActivateDoorTrimTextBoxes()
        {
            txtFL_OK_DOORTRIM.IsEnabled = chkFL_OK.IsChecked == true;
            txtFL_NG_DOORTRIM.IsEnabled = chkFL_NG.IsChecked == true;
            txtFR_OK_DOORTRIM.IsEnabled = chkFR_OK.IsChecked == true;
            txtFR_NG_DOORTRIM.IsEnabled = chkFR_NG.IsChecked == true;
            txtRL_OK_DOORTRIM.IsEnabled = chkRL_OK.IsChecked == true;
            txtRL_NG_DOORTRIM.IsEnabled = chkRL_NG.IsChecked == true;
            txtRR_OK_DOORTRIM.IsEnabled = chkRR_OK.IsChecked == true;
            txtRR_NG_DOORTRIM.IsEnabled = chkRR_NG.IsChecked == true;

            ActivateImageSelectionButtons();
        }


        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            ActivateDoorTrimTextBoxes();
        }

        private void Chk_Unchecked(object sender, RoutedEventArgs e)
        {
            ActivateDoorTrimTextBoxes();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetMasterTestSequence();
        }
    }
}