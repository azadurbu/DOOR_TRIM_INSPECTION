using DOOR_TRIM_INSPECTION.Class;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DOOR_TRIM_INSPECTION
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public List<RecipeImage> RecipeImages { get; private set; }
        public System.Windows.Media.Imaging.BitmapImage[] TrialImages = new System.Windows.Media.Imaging.BitmapImage[4];
        public string[] TrialImageFolders = new string[4];
        public OpenCvSharp.Mat[] TrialImagesMat = new OpenCvSharp.Mat[4];
        public bool usePreloadedImages = true;

        public double lOffset = 0;

        public OpenCvSharp.Rect SelectedROI { get; set; }

        public int RecipeID { get; set; }
        
        Mutex mutex = null;
        public App()
        {


            // 어플리케이션 이름 확인
            string applicationName = Process.GetCurrentProcess().ProcessName;
            Duplicate_execution(applicationName);
        }

        /// <summary>
        /// 중복실행방지
        /// </summary>
        /// <param name="mutexName"></param>
        private void Duplicate_execution(string mutexName)
        {
            try
            {
                mutex = new Mutex(false, mutexName);
            }
            catch (Exception ex)
            {
                Application.Current.Shutdown();
            }
            if (mutex.WaitOne(0, false))
            {
                //TestBoltInspection();
                Machine.Initialize();
                InitializeComponent();
            }
            else
            {
                Application.Current.Shutdown();
                MessageBox.Show("프로그램이 이미 실행중입니다.");
            }
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

        private void MasterInspWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            /*
            Console.WriteLine("Startup Args: " + string.Join(" ", Environment.GetCommandLineArgs()));
            if (e.Args.Length > 0 && e.Args[0] == "startup")
            {
                Console.WriteLine("Running from Startup");
                StartMasterInspection(false);
            }
            else
            {
                Console.WriteLine("Not Running from Startup");
                // CHECK FOR PENDING MASTER INSPECTION
                if (IsMasterTestPending())
                    StartMasterInspection(true);
                else
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                }
            }
            */
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            RunPreloadImages();
        }

        public void RunPreloadImages()
        {
            if (usePreloadedImages)
            {
                PreloadImages();
                usePreloadedImages = false;
            }

        }
        private void PreloadImages()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            RecipeImages = new List<RecipeImage>();
            int i = 1;
            List<RecipeDB> recipes = Machine.hmcDBHelper.GetRecipes();
            foreach (RecipeDB recipe in recipes)
            {
                try
                {
                    Console.WriteLine($"Loading {i}/{recipes.Count} Preload Images ... ");
                    Machine.logger.Write(eLogType.INFORMATION, $"Loading {i++}/{recipes.Count} Preload Images ... ");
                    RecipeImages.Add(
                        new RecipeImage
                        {
                            RecipeID = recipe.RecipeID,
                            FrontImage = LoadImage(recipe.FrontImagePath),
                            RearImage = LoadImage(recipe.RearImagePath),
                            RearSub1Image = LoadImage(recipe.RearSub1ImagePath),
                            FrontImagePath = recipe.FrontImagePath,
                            RearImagePath = recipe.RearImagePath,
                            RearSub1ImagePath = recipe.RearSub1ImagePath,
                        }
                    );
                }
                catch (Exception ex)
                {
                    Machine.logger.Write(eLogType.ERROR, ex.Message);
                }
            }
            sw.Stop();
            Console.WriteLine("Time Taken to Load All Images: " + sw.ElapsedMilliseconds);
            Machine.logger.Write(eLogType.INFORMATION, "Time Taken to Load All Images: " + sw.ElapsedMilliseconds);
        }

        private System.Windows.Media.Imaging.BitmapImage LoadImage(string path)
        {
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            //bitmap.DecodePixelWidth = 1920;
            bitmap.EndInit();
            return bitmap;
        }

        public System.Windows.Media.Imaging.BitmapImage GetImage(int RecipeID, bool IsFront)
        {
            var recipe = RecipeImages.FirstOrDefault(r => r.RecipeID == RecipeID);
            if (recipe != null)
            {
                return IsFront ? recipe.FrontImage : recipe.RearImage;
            }
            return null;
        }

        public string GetImagePath(int RecipeID, bool IsFront)
        {
            var recipe = RecipeImages.FirstOrDefault(r => r.RecipeID == RecipeID);
            if (recipe != null)
            {
                return IsFront ? recipe.FrontImagePath : recipe.RearImagePath;
            }
            return "";
        }

        public string GetCurrentRecipeImagePath(bool IsFront)
        {
            var recipe = RecipeImages.FirstOrDefault(r => r.RecipeID == RecipeID);
            if (recipe != null)
            {
                return IsFront ? recipe.FrontImagePath : recipe.RearImagePath;
            }
            return "";
        }

        public string GetRearSub1ImagePath(int RecipeID)
        {
            var recipe = RecipeImages.FirstOrDefault(r => r.RecipeID == RecipeID);
            if (recipe != null)
            {
                return recipe.RearSub1ImagePath;
            }
            return "";
        }

        public string GetCurrentRecipeRearSub1ImagePath()
        {
            var recipe = RecipeImages.FirstOrDefault(r => r.RecipeID == RecipeID);
            if (recipe != null)
            {
                return  recipe.RearSub1ImagePath;
            }
            return "";
        }


        public void UpdateImage(int recipeId, string newFrontPath, string newBackPath)
        {
            var recipe = RecipeImages.FirstOrDefault(r => r.RecipeID == recipeId);
            if (recipe != null)
            {
                recipe.FrontImage = LoadImage(newFrontPath);
                recipe.RearImage = LoadImage(newBackPath);
            }
            else
            {
                RecipeImages.Add(
                        new RecipeImage
                        {
                            RecipeID = recipeId,
                            FrontImage = LoadImage(newFrontPath),
                            RearImage = LoadImage(newBackPath),
                            FrontImagePath = newFrontPath,
                            RearImagePath = newBackPath
                        }
                    );
            }
        }

        public void ClearRecipeImages()
        {
            if (RecipeImages != null)
            {
                foreach (var recipeImage in RecipeImages)
                {
                    DisposeImage(recipeImage.FrontImage);
                    DisposeImage(recipeImage.RearImage);
                }
                RecipeImages.Clear();
                RecipeImages = null;
            }
        }

        private void DisposeImage(System.Windows.Media.Imaging.BitmapImage image)
        {
            if (image != null)
            {
                image.StreamSource?.Dispose(); // Dispose of the stream if it exists
            }
        }

        public void SetTrialImage(int index, System.Windows.Media.Imaging.BitmapImage img)
        {
            TrialImages[index] = img; 
        }

        public System.Windows.Media.Imaging.BitmapImage GetTrialImage(int index)
        {
            if (TrialImages[index] != null)
                return TrialImages[index];
            return null;
        }

        public void SetTrialImageFolder(int index, string folderPath)
        {
            TrialImageFolders[index] = folderPath;
        }

        public string GetTrialImageFolder(int index)
        {
            if (TrialImageFolders[index] != null)
                return TrialImageFolders[index];
            return null;
        }

        public void ClearTrialImageFolder(int index)
        {
            TrialImageFolders[index] = "";
        }

        public void ClearTrialImage(int index)
        {
            DisposeImage(TrialImages[index]);
            TrialImages[index] = null;
        }

        public void SetTrialImageMat(int index, OpenCvSharp.Mat img)
        {
            TrialImagesMat[index] = img.Clone();
        }

        public OpenCvSharp.Mat GetTrialImageMat(int index)
        {
            if (TrialImagesMat[index] != null)
                return TrialImagesMat[index].Clone();
            return null;
        }

        public void ClearTrialImageMat(int index)
        {
            TrialImagesMat[index] = null;
        }

    }
}
