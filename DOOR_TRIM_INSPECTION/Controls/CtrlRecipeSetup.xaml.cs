using DOOR_TRIM_INSPECTION.Class;
using System;
using System.Windows.Controls;

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CtrlRecipeBuilder.xaml
    /// </summary>

    public partial class CtrlRecipeSetup : UserControl
    {
        public event Action<bool, RECIPE_SETUP_STEPS> ToggleTrialPaneVisibility;

        public CtrlRecipeSetup()
        {
            InitializeComponent();
            ctrlRecipeList.SetParent(this);
            ctrlRecipeWizard.SetParent(this);
            ctrlRecipeWizard.ToggleTrialPaneVisibility += CtrlRecipeWizard_ToggleTrialPaneVisibility;
        }

        private void CtrlRecipeWizard_ToggleTrialPaneVisibility(bool Show, RECIPE_SETUP_STEPS step)
        {
            ToggleTrialPaneVisibility?.Invoke(Show, step);
        }

        public void ShowPage(RECIPE_SETUP_TABS tab, int RecipeID)
        {
            switch (tab)
            {
                case RECIPE_SETUP_TABS.RECIPE_LIST:
                    tabControlRecipe.SelectedIndex = 0;
                    ctrlRecipeWizard.SetUIMode(0);
                    break;
                case RECIPE_SETUP_TABS.RECIPE_WIZARD:
                    ctrlRecipeWizard.SetUIMode(RecipeID);
                    tabControlRecipe.SelectedIndex = 1;
                    break;
            }
        }

        public void Reset()
        {
            ShowPage(RECIPE_SETUP_TABS.RECIPE_LIST, 0);
        }
    }
}
