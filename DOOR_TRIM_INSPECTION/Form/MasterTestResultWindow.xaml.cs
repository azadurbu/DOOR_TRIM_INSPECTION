using DOOR_TRIM_INSPECTION.Class;
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
using System.Windows.Shapes;

namespace DOOR_TRIM_INSPECTION.Form
{
    /// <summary>
    /// Interaction logic for MasterTestResultWindow.xaml
    /// </summary>
    public partial class MasterTestResultWindow : Window
    {
        private string MasterResultFile
        {
            get
            {
                return System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                         "master_test_result.txt");
            }
        }

        public MasterTestResultWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetUI();
        }

        private SolidColorBrush brushPending = new SolidColorBrush(Colors.DarkGray);
        private SolidColorBrush brushOK = new SolidColorBrush(Colors.SeaGreen);
        private SolidColorBrush brushNG = new SolidColorBrush(Colors.Crimson);

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

            using (System.IO.StreamReader reader = new System.IO.StreamReader(MasterResultFile))
            {
                string line = reader.ReadLine();
                string[] literals = line.Split(',');

                if (literals.Length == 9)
                {
                    lblTestTime.Content = literals[0];
                    lblFLOK.Background = literals[1] == "1" ? brushOK : (literals[1] == "0" ? brushNG : brushPending);
                    lblFLNG.Background = literals[2] == "1" ? brushOK : (literals[2] == "0" ? brushNG : brushPending);
                    lblFROK.Background = literals[3] == "1" ? brushOK : (literals[3] == "0" ? brushNG : brushPending);
                    lblFRNG.Background = literals[4] == "1" ? brushOK : (literals[4] == "0" ? brushNG : brushPending);
                    lblRLOK.Background = literals[5] == "1" ? brushOK : (literals[5] == "0" ? brushNG : brushPending);
                    lblRLNG.Background = literals[6] == "1" ? brushOK : (literals[6] == "0" ? brushNG : brushPending);
                    lblRROK.Background = literals[7] == "1" ? brushOK : (literals[7] == "0" ? brushNG : brushPending);
                    lblRRNG.Background = literals[8] == "1" ? brushOK : (literals[8] == "0" ? brushNG : brushPending);

                    lblFLOK.Visibility = literals[1] == "-1" ? Visibility.Collapsed : Visibility.Visible;
                    lblFLNG.Visibility = literals[2] == "-1" ? Visibility.Collapsed : Visibility.Visible;
                    lblFROK.Visibility = literals[3] == "-1" ? Visibility.Collapsed : Visibility.Visible;
                    lblFRNG.Visibility = literals[4] == "-1" ? Visibility.Collapsed : Visibility.Visible;
                    lblRLOK.Visibility = literals[5] == "-1" ? Visibility.Collapsed : Visibility.Visible;
                    lblRLNG.Visibility = literals[6] == "-1" ? Visibility.Collapsed : Visibility.Visible;
                    lblRROK.Visibility = literals[7] == "-1" ? Visibility.Collapsed : Visibility.Visible;
                    lblRRNG.Visibility = literals[8] == "-1" ? Visibility.Collapsed : Visibility.Visible;
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



        private void LblInspectionResult_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ctrlImageViewer.ZoomClear();
            ctrlImageViewer.Clear();

            string imageSaveFolder = System.IO.Path.Combine(MasterImagePath, "Results");
            string frontFileName = ""; // $"result_{DoorTrimID}_front.png";
            string rearFileName = ""; // $"result_{DoorTrimID}_rear.png";
            string rearFilePath = "";
            string frontFilePath = "";
            Label source = sender as Label;
            if (lblFLOK == source)
            {
                frontFileName = $"result_{Machine.config.setup.FL_OK_DOOR_TRIM_ID}_front.png";
                rearFileName = $"result_{Machine.config.setup.FL_OK_DOOR_TRIM_ID}_rear.png";

            }
            else if (lblFLNG == source)
            {
                frontFileName = $"result_{Machine.config.setup.FL_NG_DOOR_TRIM_ID}_front.png";
                rearFileName = $"result_{Machine.config.setup.FL_NG_DOOR_TRIM_ID}_rear.png";
            }
            else if (lblFROK == source)
            {
                frontFileName = $"result_{Machine.config.setup.FR_OK_DOOR_TRIM_ID}_front.png";
                rearFileName = $"result_{Machine.config.setup.FR_OK_DOOR_TRIM_ID}_rear.png";
            }
            else if (lblFRNG == source)
            {
                frontFileName = $"result_{Machine.config.setup.FR_NG_DOOR_TRIM_ID}_front.png";
                rearFileName = $"result_{Machine.config.setup.FR_NG_DOOR_TRIM_ID}_rear.png";
            }
            else if (lblRLOK == source)
            {
                frontFileName = $"result_{Machine.config.setup.RL_OK_DOOR_TRIM_ID}_front.png";
                rearFileName = $"result_{Machine.config.setup.RL_OK_DOOR_TRIM_ID}_rear.png";
            }
            else if (lblRLNG == source)
            {
                frontFileName = $"result_{Machine.config.setup.RL_NG_DOOR_TRIM_ID}_front.png";
                rearFileName = $"result_{Machine.config.setup.RL_NG_DOOR_TRIM_ID}_rear.png";
            }
            else if (lblRROK == source)
            {
                frontFileName = $"result_{Machine.config.setup.RR_OK_DOOR_TRIM_ID}_front.png";
                rearFileName = $"result_{Machine.config.setup.RR_OK_DOOR_TRIM_ID}_rear.png";
            }
            else if (lblRRNG == source)
            {
                frontFileName = $"result_{Machine.config.setup.RR_NG_DOOR_TRIM_ID}_front.png";
                rearFileName = $"result_{Machine.config.setup.RR_NG_DOOR_TRIM_ID}_rear.png";
            }
            rearFilePath = System.IO.Path.Combine(imageSaveFolder, rearFileName);
            frontFilePath = System.IO.Path.Combine(imageSaveFolder, frontFileName);

            if (!System.IO.File.Exists(rearFilePath) || !System.IO.File.Exists(frontFilePath))
            {
                MessageBox.Show("결과 파일을 찾을 수 없습니다!");
                rearFilePath = "D:\\Images\\RecipeImages\\result_rear.bmp";
                frontFilePath = "D:\\Images\\RecipeImages\\result_front.bmp";
               
            }
            ctrlImageViewer.Visibility = Visibility.Visible;
            ctrlImageViewer.LoadHistoryImages(0, rearFilePath, frontFilePath);
        }
    }
}
