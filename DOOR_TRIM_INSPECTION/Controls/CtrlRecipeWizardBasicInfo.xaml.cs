using DOOR_TRIM_INSPECTION.Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for RecipeWizardBasicInfo.xaml
    /// </summary>
    public partial class CtrlRecipeWizardBasicInfo : System.Windows.Controls.UserControl
    {
        private RecipeDB CurrentRecipe = null;

        private string ImageFolder = Machine.config.setup.ImagePath; // GET IT FROM MACHINE
        private string RecipeImageFolder = "RecipeImages";
        private string FrontImageName = "Front.bmp";
        private string RearImageName = "Rear.bmp";


        // PRIVATE PROPERTIES TO HOLD FORM DATA
        private string FrontImagePath { get; set; }
        private string RearImagePath { get; set; }
        private string RearSub1ImagePath { get; set; }
        // PRIVATE PROPERTIES TO HOLD FORM DATA


        public CtrlRecipeWizardBasicInfo()
        {
            InitializeComponent();
            LoadControlData();
        }

        public void Reset()
        {
            CurrentRecipe = null;
            RearImagePath = "";
            RearSub1ImagePath = "";
            FrontImagePath = "";

        }

        public void SetCurrentRecipe(RecipeDB recipe)
        {
            CurrentRecipe = recipe;

            txtRecipeName.Text = CurrentRecipe.RecipeName;
            cboYear.SelectedValue = CurrentRecipe.Year;

            switch (CurrentRecipe.DoorType)
            {
                case "Front Left":
                    cboDoorType.SelectedValue = "FL";
                    break;
                case "Front Right":
                    cboDoorType.SelectedValue = "FR";
                    break;
                case "Rear Left":
                    cboDoorType.SelectedValue = "RL";
                    break;
                case "Rear Right":
                    cboDoorType.SelectedValue = "RR";
                    break;
            }
            txtModel.Text = CurrentRecipe.Model;

            // IMAGES
           

            if (((App)System.Windows.Application.Current).usePreloadedImages)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                BitmapImage imgFront = ((App)System.Windows.Application.Current).GetImage(CurrentRecipe.RecipeID, true);

                if (imgFront != null)
                {
                    FrontImagePath = CurrentRecipe.FrontImagePath;
                    imgFrontImage.Source = imgFront;

                }
                else
                {
                    imgFrontImage.Source = null;
                }
                sw.Stop();
                Console.WriteLine("Preloaded Front Image Loading Time: " + sw.ElapsedMilliseconds);
                Machine.logger.Write(eLogType.INFORMATION, "Preloaded Front Image Loading Time: " + sw.ElapsedMilliseconds);
                ((App)System.Windows.Application.Current).usePreloadedImages = false;
            }
            else
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                if (!string.IsNullOrEmpty(CurrentRecipe.FrontImagePath) && File.Exists(CurrentRecipe.FrontImagePath))
                {
                    //FrontImagePath = CurrentRecipe.FrontImagePath;
                    //imgFrontImage.Source = new BitmapImage(new Uri(CurrentRecipe.FrontImagePath));
                    FrontImagePath = CurrentRecipe.FrontImagePath;
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(CurrentRecipe.FrontImagePath);
                    bitmap.EndInit();
                    imgFrontImage.Source = bitmap;

                }
                else
                {
                    imgFrontImage.Source = null;
                }
                sw.Stop();
                Console.WriteLine("Front Image Loading Time: " + sw.ElapsedMilliseconds);
                Machine.logger.Write(eLogType.INFORMATION, "Front Image Loading Time: " + sw.ElapsedMilliseconds);
            }
            if (((App)System.Windows.Application.Current).usePreloadedImages)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                BitmapImage imgRear = ((App)System.Windows.Application.Current).GetImage(CurrentRecipe.RecipeID, false);

                if (imgRear != null)
                {
                    RearImagePath = CurrentRecipe.RearImagePath;
                    imgRearImage.Source = imgRear;
                    FileInfo fi = new FileInfo(CurrentRecipe.RearImagePath);
                    if (fi.Exists)
                    {
                        List<string> imageFiles = GetImagesByPattern(fi.DirectoryName, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();
                        RearSub1ImagePath = imageFiles[0];
                    }
                }
                else
                {
                    imgRearImage.Source = null;
                }
                sw.Stop();
                Console.WriteLine("Preloaded Rear Image Loading Time: " + sw.ElapsedMilliseconds);
                Machine.logger.Write(eLogType.INFORMATION, "Preloaded Rear Image Loading Time: " + sw.ElapsedMilliseconds);

                ((App)System.Windows.Application.Current).usePreloadedImages = false;
            }
            else
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                if (!string.IsNullOrEmpty(CurrentRecipe.RearImagePath) && File.Exists(CurrentRecipe.RearImagePath))
                {
                    //RearImagePath = CurrentRecipe.RearImagePath;
                    //imgRearImage.Source = new BitmapImage(new Uri(CurrentRecipe.RearImagePath));
                    RearImagePath = CurrentRecipe.RearImagePath;

                    FileInfo fi = new FileInfo(CurrentRecipe.RearImagePath);
                    if (fi.Exists)
                    {
                        List<string> imageFiles = GetImagesByPattern(fi.DirectoryName, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();
                        RearSub1ImagePath = imageFiles[0];
                    }

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(CurrentRecipe.RearImagePath);
                    bitmap.EndInit();
                    imgRearImage.Source = bitmap;
                }
                else
                {
                    imgRearImage.Source = null;
                }
                sw.Stop();
                Console.WriteLine("Rear Image Loading Time: " + sw.ElapsedMilliseconds);
                Machine.logger.Write(eLogType.INFORMATION, "Rear Image Loading Time: " + sw.ElapsedMilliseconds);
            }
        }

        private string[] GetImagesByPattern(string folderPath, string pattern)
        {
            Regex regex = new Regex(pattern);
            return System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly)
                            .Where(file => regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(file)))
                            .ToArray();
        }

        public RecipeDB GetCurrentRecipe()
        {
            if (CurrentRecipe == null || CurrentRecipe.RecipeID == 0)
            {
                CurrentRecipe = new RecipeDB();
                CurrentRecipe.RecipeName = txtRecipeName.Text.Trim();
            }

            CurrentRecipe.FrontImagePath = FrontImagePath;
            CurrentRecipe.RearImagePath = RearImagePath;

            CurrentRecipe.Model = txtModel.Text;
            CurrentRecipe.Year = (string)cboYear.SelectedValue;

            switch (cboDoorType.SelectedValue)
            {
                case "FL":
                    CurrentRecipe.DoorType = "Front Left";
                    break;
                case "FR":
                    CurrentRecipe.DoorType = "Front Right";
                    break;
                case "RL":
                    CurrentRecipe.DoorType = "Rear Left";
                    break;
                case "RR":
                    CurrentRecipe.DoorType = "Rear Right";
                    break;
            }

            return CurrentRecipe;
        }

        private void LoadControlData()
        {
            List<string> years = new List<string>();

            for (int y = 2024; y < 2031; y++)
            {
                years.Add(y.ToString());
            }
            cboYear.ItemsSource = years;



            List<KeyValuePair<string, string>> doorTypes = new List<KeyValuePair<string, string>>();
            doorTypes.Add(new KeyValuePair<string, string>("FL", "Front Left"));
            doorTypes.Add(new KeyValuePair<string, string>("FR", "Front Right"));
            doorTypes.Add(new KeyValuePair<string, string>("RL", "Rear Left"));
            doorTypes.Add(new KeyValuePair<string, string>("RR", "Rear Right"));

            cboDoorType.ItemsSource = doorTypes;
            cboDoorType.DisplayMemberPath = "Value";
            cboDoorType.SelectedValuePath = "Key";

            rdoFromMasterImg.IsChecked = true;


            List<InspectionResult> InspectionResults = Machine.hmcDBHelper.GetInspectionResults();
            cboDoorTrimCodes.Items.Clear();
            cboDoorTrimCodes.ItemsSource = InspectionResults;
            cboDoorTrimCodes.DisplayMemberPath = "DoorTrimID";
            cboDoorTrimCodes.SelectedValuePath = "InspectionResultID";
        }

        private void TxtModel_TextChanged(object sender, TextChangedEventArgs e)
        {
            GenerateRecipeName();
        }



        private void CboYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateRecipeName();
        }

        private void CboDoorType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateRecipeName();
        }

        private void GenerateRecipeName()
        {
            if (CurrentRecipe == null || string.IsNullOrEmpty(CurrentRecipe.RecipeName))
            {
                List<string> Literals = new List<string>();
                Literals.Add(DateTime.Now.ToString("yyMMdd"));
                if (!string.IsNullOrEmpty(txtModel.Text))
                    Literals.Add(txtModel.Text);
                if (cboYear.SelectedValue != null)
                    Literals.Add(cboYear.SelectedValue.ToString());
                if (cboDoorType.SelectedValue != null)
                    Literals.Add(cboDoorType.SelectedValue.ToString());

                txtRecipeName.Text = Literals.Count > 1 ? string.Join("_", Literals) : "";
            }
            else
            {
                txtRecipeName.Text = CurrentRecipe.RecipeName;
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrEmpty(txtModel.Text))
            {
                System.Windows.MessageBox.Show("Please provide Model.");
                return false;
            }
            if (cboYear.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please Select Year.");
                return false;
            }
            if (cboDoorType.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please provide Door Type");
                return false;
            }
            return true;
        }

        private string CreateDestinationDirectory(string DoorTrimCode)
        {
            string RootDestPath = System.IO.Path.Combine(ImageFolder, RecipeImageFolder);
            if (!Directory.Exists(RootDestPath))
            {
                Directory.CreateDirectory(RootDestPath);
            }
            GenerateRecipeName();
            RootDestPath = System.IO.Path.Combine(RootDestPath, DoorTrimCode);
            if (!Directory.Exists(RootDestPath))
            {
                Directory.CreateDirectory(RootDestPath);
            }
            return RootDestPath;
        }
        private void BtnSelectFrontImage_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp";
                openFileDialog.InitialDirectory = Machine.config.setup.ImagePath;
                DialogResult result = openFileDialog.ShowDialog();
                
                if (result == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    // COPY TO APPLICATION FOLDER
                    string RootDestPath = CreateDestinationDirectory(txtRecipeName.Text);
                    if (!Directory.Exists(RootDestPath))
                    {
                        Directory.CreateDirectory(RootDestPath);
                    }
                    string destFilePath = System.IO.Path.Combine(RootDestPath, FrontImageName);

                    //FrontImagePath = destFilePath; // KEEP IN PRIVATE PROPERTY
                    FrontImagePath = filePath; // 2025.08.28 AZAD, RECIPE IMAGE CHANGE FIX
                    File.Copy(filePath, destFilePath, true);


                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath); // USING OLD SOURCE BECAUSE FILE COPY TAKES TIME
                    bitmap.EndInit();
                    imgFrontImage.Source = bitmap;
                    ((App)System.Windows.Application.Current).usePreloadedImages = true;
                }
            }

        }

        private void BtnSelectRearImage_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp";
                openFileDialog.InitialDirectory = Machine.config.setup.ImagePath;
                DialogResult result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    // COPY TO APPLICATION FOLDER
                    string RootDestPath = CreateDestinationDirectory(txtRecipeName.Text);
                    if (!Directory.Exists(RootDestPath))
                    {
                        Directory.CreateDirectory(RootDestPath);
                    }
                    string destFilePath = System.IO.Path.Combine(RootDestPath, RearImageName);

                    //RearImagePath = destFilePath; // KEEP IN PRIVATE PROPERTY
                    RearImagePath = filePath; // 2025.08.28 AZAD, RECIPE IMAGE CHANGE FIX
                    File.Copy(filePath, destFilePath, true);
                    
                    FileInfo fi = new FileInfo(destFilePath);
                    if (fi.Exists)
                    {
                        List<string> imageFiles = GetImagesByPattern(fi.DirectoryName, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();
                        RearSub1ImagePath = imageFiles[0];
                    }

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath); // USING OLD SOURCE BECAUSE FILE COPY TAKES TIME
                    bitmap.EndInit();
                    imgRearImage.Source = bitmap;
                    ((App)System.Windows.Application.Current).usePreloadedImages = true;
                }
            }

        }

        private void BtnSelectPreviousResultImages_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            if (cboDoorTrimCodes.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please select a Door Timr Code.");
                return;
            }

            InspectionResult inspItem = cboDoorTrimCodes.SelectedItem as InspectionResult;
            if (inspItem != null)
            {
                string day = inspItem.InspectionTime.ToString("dd");
                string month = inspItem.InspectionTime.ToString("MM");
                string year = inspItem.InspectionTime.ToString("yyyy");

                string SearchPath = System.IO.Path.Combine(ImageFolder, year, month, day, inspItem.DoorTrimID);

                string DestPath = CreateDestinationDirectory(txtRecipeName.Text);

                if (Directory.Exists(SearchPath))
                {
                    RearImagePath = System.IO.Path.Combine(SearchPath, RearImageName); // KEEP IN PRIVATE PROPERTY
                    string destFilePath = System.IO.Path.Combine(DestPath, RearImageName);
                    File.Copy(RearImagePath, destFilePath, true);

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(RearImagePath); // USING OLD SOURCE BECAUSE FILE COPY TAKES TIME
                    bitmap.EndInit();
                    imgRearImage.Source = bitmap;

                    FrontImagePath = System.IO.Path.Combine(SearchPath, FrontImageName); // KEEP IN PRIVATE PROPERTY
                    destFilePath = System.IO.Path.Combine(DestPath, FrontImageName);
                    File.Copy(FrontImagePath, destFilePath, true);

                    FileInfo fi = new FileInfo(destFilePath);
                    if (fi.Exists)
                    {
                        List<string> imageFiles = GetImagesByPattern(fi.DirectoryName, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();
                        RearSub1ImagePath = imageFiles[0];
                    }

                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(System.IO.Path.Combine(SearchPath, FrontImageName)); // USING OLD SOURCE BECAUSE FILE COPY TAKES TIME
                    bitmap.EndInit();
                    imgFrontImage.Source = bitmap;
                }
            }
        }

        private void RdoFromMasterImg_Checked(object sender, RoutedEventArgs e)
        {
            AdjustImageSourceButtons();
        }

        private void RdoFromFolder_Checked(object sender, RoutedEventArgs e)
        {
            AdjustImageSourceButtons();
        }

        private void RdoFromPrevResult_Checked(object sender, RoutedEventArgs e)
        {
            AdjustImageSourceButtons();
        }

        private void AdjustImageSourceButtons()
        {
            if (btnSelectFrontImage != null && rdoFromMasterImg != null) btnSelectFrontImage.IsEnabled = (bool)rdoFromMasterImg.IsChecked;
            if (btnSelectRearImage != null && rdoFromMasterImg != null) btnSelectRearImage.IsEnabled = (bool)rdoFromMasterImg.IsChecked;
            if (btnSelectPreviousResultImages != null && rdoFromPrevResult != null) btnSelectPreviousResultImages.IsEnabled = (bool)rdoFromPrevResult.IsChecked;
            if (btnMergeFromFolder != null && rdoFromFolder != null) btnMergeFromFolder.IsEnabled = (bool)rdoFromFolder.IsChecked;
        }

        private void BtnMergeFromFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string InputFolder = folderBrowserDialog.SelectedPath;


                    string[] imagePaths_r = Directory.GetFiles(InputFolder, "back*.bmp");
                    string[] imagePaths_f = Directory.GetFiles(InputFolder, "front*.bmp");


                    ////////
                    ///
                    /// IMAGE MERGE
                    /// 
                    /// 
                    ////////
                    Bitmap MergedImageFront = null;
                    Bitmap MergedImageRear = null;
                    ////////
                    ///
                    /// IMAGE MERGE
                    /// 
                    /// 
                    ////////

                    // COPY TO APPLICATION FOLDER
                    string DestPath = CreateDestinationDirectory(txtRecipeName.Text);

                    string destFrontFilePath = System.IO.Path.Combine(DestPath, FrontImageName);
                    string destRearFilePath = System.IO.Path.Combine(DestPath, RearImageName);


                    FrontImagePath = destFrontFilePath;// KEEP IN PRIVATE PROPERTY
                    RearImagePath = destRearFilePath;// KEEP IN PRIVATE PROPERTY


                    MergedImageFront.Save(destFrontFilePath);
                    MergedImageRear.Save(destRearFilePath);

                    BitmapImage bitmapImage = new BitmapImage();
                    using (MemoryStream memory = new MemoryStream())
                    {
                        MergedImageFront.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                        memory.Position = 0;
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                    }

                    imgFrontImage.Source = bitmapImage;

                    bitmapImage = new BitmapImage();
                    using (MemoryStream memory = new MemoryStream())
                    {
                        MergedImageRear.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                        memory.Position = 0;
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                    }

                    imgRearImage.Source = bitmapImage;
                }
            }

        }
    }
}
