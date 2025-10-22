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

namespace DOOR_TRIM_INSPECTION
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void BtnCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnAlgo_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0;
            CtlRecipeSetup_ToggleTrialPaneVisibility(false, RECIPE_SETUP_STEPS.BASIC_INFO);
            ctlRecipeSetup.ToggleTrialPaneVisibility -= CtlRecipeSetup_ToggleTrialPaneVisibility;
        }

        private void BtnRecipe_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
            ctlRecipeSetup.ToggleTrialPaneVisibility += CtlRecipeSetup_ToggleTrialPaneVisibility;
        }

        private void CtlRecipeSetup_ToggleTrialPaneVisibility(bool Show, RECIPE_SETUP_STEPS step)
        {
            ctrlTrialImageList.Visibility = Show && step != RECIPE_SETUP_STEPS.BASIC_INFO? Visibility.Visible : Visibility.Collapsed;

            // CLEAR IMAGES WHEN PAGE CHANGES
            ctrlTrialImageList.ClearImages();

        }

        private void BtnHardware_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 2;
            CtlRecipeSetup_ToggleTrialPaneVisibility(false, RECIPE_SETUP_STEPS.BASIC_INFO);
            ctlRecipeSetup.ToggleTrialPaneVisibility -= CtlRecipeSetup_ToggleTrialPaneVisibility;
        }

        private void BtnApplication_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 3;
            CtlRecipeSetup_ToggleTrialPaneVisibility(false, RECIPE_SETUP_STEPS.BASIC_INFO);
            ctlRecipeSetup.ToggleTrialPaneVisibility -= CtlRecipeSetup_ToggleTrialPaneVisibility;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Task.Run(() => DeleteFilesInFolder(
                System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                         "images\\TryCrop")));
        }

        private void DeleteFilesInFolder(string folderPath)
        {
            try
            {
                // Check if folder exists
                if (System.IO.Directory.Exists(folderPath))
                {
                    // Get all files in the directory
                    var files = System.IO.Directory.GetFiles(folderPath);

                    foreach (var file in files)
                    {
                        try
                        {
                            // Delete each file
                            System.IO.File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            // Handle potential errors (file might be in use, etc.)
                            Machine.logger.Write(eLogType.ERROR, $"Error deleting file {file}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Machine.logger.Write(eLogType.ERROR, $"Folder {folderPath} does not exist.");
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occur while accessing the directory
                Machine.logger.Write(eLogType.ERROR, $"Error accessing folder {folderPath}: {ex.Message}");
            }
        }
    }
}
