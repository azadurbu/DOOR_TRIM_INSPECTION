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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CtrlRecipeWizard.xaml
    /// </summary>
    public partial class CtrlRecipeWizard : UserControl
    {
        private Wizard wizard = null;

        private BitmapImage imgPastState = null;
        private BitmapImage imgCurrentState = null;

        public event Action<bool, RECIPE_SETUP_STEPS> ToggleTrialPaneVisibility;

        private BitmapImage imgFutureState = null;

        private CtrlRecipeSetup parentCtl { get; set; }
        public void SetParent(CtrlRecipeSetup recipeSetup)
        {
            parentCtl = recipeSetup;
        }

        public CtrlRecipeWizard()
        {
            InitializeComponent();
        }

        public void SetUIMode(int RecipeID = 0)
        {
            if (RecipeID != 0)
                basicInfoSetup.SetCurrentRecipe(Machine.hmcDBHelper.GetRecipe(RecipeID));
            else
                basicInfoSetup.Reset();
        }
        private void InitWizard()
        {
            wizard = new Wizard();
            wizard.AddStep(new WizardStep(RECIPE_SETUP_STEPS.BASIC_INFO, basicInfoSetup));
            wizard.AddStep(new WizardStep(RECIPE_SETUP_STEPS.FRONT_ROI, frontROISetup));
            wizard.AddStep(new WizardStep(RECIPE_SETUP_STEPS.REAR_ROI, rearROISetup));

            string appPath = AppDomain.CurrentDomain.BaseDirectory;

            imgPastState = new BitmapImage(new Uri(System.IO.Path.Combine(appPath, "images/finished.png")));
            imgCurrentState = new BitmapImage(new Uri(System.IO.Path.Combine(appPath, "images/current.png")));
            imgFutureState = new BitmapImage(new Uri(System.IO.Path.Combine(appPath, "images/pending.png")));
            UpdateContent();
        }

        private void UpdateContent()
        {
            var currentStep = wizard.GetCurrentStep();
            if (currentStep != null)
            {
                basicInfoSetup.Visibility = currentStep.Step == RECIPE_SETUP_STEPS.BASIC_INFO ? Visibility.Visible : Visibility.Hidden;
                frontROISetup.Visibility = currentStep.Step == RECIPE_SETUP_STEPS.FRONT_ROI ? Visibility.Visible : Visibility.Hidden;
                rearROISetup.Visibility = currentStep.Step == RECIPE_SETUP_STEPS.REAR_ROI ? Visibility.Visible : Visibility.Hidden;

                switch (currentStep.Step)
                {
                    case RECIPE_SETUP_STEPS.BASIC_INFO:
                        gridStep1.Style = (Style)FindResource("CurrentStateButton");
                        imgStep1.Source = imgCurrentState;
                        gridStep2.Style = (Style)FindResource("FutureStateButton");
                        imgStep2.Source = imgFutureState;
                        gridStep3.Style = (Style)FindResource("FutureStateButton");
                        imgStep3.Source = imgFutureState;
                        ToggleTrialPaneVisibility?.Invoke(false, RECIPE_SETUP_STEPS.BASIC_INFO);
                        break;
                    case RECIPE_SETUP_STEPS.FRONT_ROI:
                        gridStep1.Style = (Style)FindResource("PastStateButton");
                        imgStep1.Source = imgPastState;
                        gridStep2.Style = (Style)FindResource("CurrentStateButton");
                        imgStep2.Source = imgCurrentState;
                        gridStep3.Style = (Style)FindResource("FutureStateButton");
                        imgStep3.Source = imgFutureState;
                        ToggleTrialPaneVisibility?.Invoke(true, RECIPE_SETUP_STEPS.FRONT_ROI);
                        break;
                    case RECIPE_SETUP_STEPS.REAR_ROI:
                        gridStep1.Style = (Style)FindResource("PastStateButton");
                        imgStep1.Source = imgPastState;
                        gridStep2.Style = (Style)FindResource("PastStateButton");
                        imgStep2.Source = imgPastState;
                        gridStep3.Style = (Style)FindResource("CurrentStateButton");
                        imgStep3.Source = imgCurrentState;
                        ToggleTrialPaneVisibility?.Invoke(true, RECIPE_SETUP_STEPS.FRONT_ROI);
                        break;
                }

            }

            

            btnPrev.IsEnabled = wizard.CanMovePrevious();
            btnNext.IsEnabled = wizard.CanMoveNext();
            btnNext.Visibility = wizard.CanMoveNext() ? Visibility.Visible : Visibility.Hidden;
            btnSave.Visibility = wizard.CanMoveNext() ? Visibility.Hidden : Visibility.Visible;
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            wizard.MovePrevious();
            UpdateContent();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            var currentStep = wizard.GetCurrentStep();
            bool MoveNext = false;
            if (currentStep != null)
            {
                switch (currentStep.Step)
                {
                    case RECIPE_SETUP_STEPS.BASIC_INFO:
                        RecipeDB recipe = basicInfoSetup.GetCurrentRecipe();
                        if (string.IsNullOrEmpty(recipe.FrontImagePath) && string.IsNullOrEmpty(recipe.RearImagePath))
                        {
                            MessageBox.Show("Please Select Both Front and Rear Image");
                            MoveNext = false;
                        }
                        else if (Machine.hmcDBHelper.SaveRecipe(recipe))
                        {
                            if (recipe.RecipeID == 0) // WE NEED TO CHANGE THE DB CODES TO RETURN THE ID INSTEAD OF BOOL LATER
                            {
                                recipe.RecipeID = Machine.hmcDBHelper.GetLatestRecipeID();
                            }
                            if (((App)System.Windows.Application.Current).usePreloadedImages)
                            {
                                ((App)System.Windows.Application.Current).UpdateImage(recipe.RecipeID, recipe.FrontImagePath, recipe.RearImagePath);
                                ((App)System.Windows.Application.Current).usePreloadedImages = false;
                            }
                            frontROISetup.SetCurrentRecipeInfo(recipe, true);
                            rearROISetup.SetCurrentRecipeInfo(recipe, false);
                           
                            MoveNext = true;
                        }
                        frontROISetup._isFromPrevious = true;
                        break;
                    case RECIPE_SETUP_STEPS.FRONT_ROI:
                        List<DetectionROIDetailsUI> frontROIs = frontROISetup.GetDetectionROIs();
                        if (frontROIs.Count > 0)
                        {
                            foreach (DetectionROIDetailsUI roiDet in frontROIs)
                            {
                                Machine.hmcDBHelper.SaveDetectionROI(roiDet);
                            }
                        }
                        MoveNext = true;
                        rearROISetup._isFromPrevious = true;
                        
                        break;
                    case RECIPE_SETUP_STEPS.REAR_ROI:
                        break;
                }

            }
            if (MoveNext)
            {
                wizard.MoveNext();
                UpdateContent();
            }
            else
            {
                // LOG ERROR
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // SAVE DATA
            var currentStep = wizard.GetCurrentStep();
            if (currentStep.Step == RECIPE_SETUP_STEPS.REAR_ROI)
            {
                List<DetectionROIDetailsUI> rearROIs =    rearROISetup.GetDetectionROIs();
                if (rearROIs.Count > 0)
                {
                    foreach (DetectionROIDetailsUI roiDet in rearROIs)
                    {
                        Machine.hmcDBHelper.SaveDetectionROI(roiDet);
                    }
                }
                ToggleTrialPaneVisibility?.Invoke(false, RECIPE_SETUP_STEPS.BASIC_INFO);
                parentCtl.ShowPage(RECIPE_SETUP_TABS.RECIPE_LIST, 0);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitWizard();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            ToggleTrialPaneVisibility?.Invoke(false, RECIPE_SETUP_STEPS.BASIC_INFO);
            parentCtl.ShowPage(RECIPE_SETUP_TABS.RECIPE_LIST, 0);
        }

        private void BasicInfoSetup_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
