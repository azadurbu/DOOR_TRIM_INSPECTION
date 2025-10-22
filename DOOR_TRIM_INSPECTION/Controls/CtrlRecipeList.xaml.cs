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
    /// Interaction logic for CtrlRecipeList.xaml
    /// </summary>
    public partial class CtrlRecipeList : UserControl
    {
        private CtrlRecipeSetup parentCtl { get; set; }

        public CtrlRecipeList()
        {
            InitializeComponent();
            
           
        }

        public void SetParent(CtrlRecipeSetup recipeSetup)
        {
            parentCtl = recipeSetup;
        }

        public void LoadRecipesFromDB()
        {
            dgRecipes.ItemsSource = Machine.hmcDBHelper.GetRecipes();
            //dgRecipes.SelectedIndex = dgRecipes.Items.Count - 1;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            parentCtl.ShowPage(RECIPE_SETUP_TABS.RECIPE_WIZARD, 0);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            bool result = CustomMessageBox.Show("", Window.GetWindow(this));
            if (result)
            {
                RecipeDB selectedRecipe = dgRecipes.SelectedItem as RecipeDB;

                if (selectedRecipe != null)
                    if (Machine.hmcDBHelper.DeleteRecipe(selectedRecipe.RecipeID))
                        LoadRecipesFromDB();
            }
        }

       

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            RecipeDB selectedRecipe = dgRecipes.SelectedItem as RecipeDB;

            if (selectedRecipe != null)
            {
                ((App)Application.Current).RecipeID = selectedRecipe.RecipeID;
                //EnableGrid(false);
                parentCtl.ShowPage(RECIPE_SETUP_TABS.RECIPE_WIZARD, selectedRecipe.RecipeID);
            }
        }


       

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not Implemented");
        }

        public void ShowImageSelectionPage(FORM_MODE formMode, bool isFront)
        {
            //Recipe selectedRecipe = dgRecipes.Items[dgRecipes.Items.Count - 1] as Recipe;
            //if (isFront)
            //{
            //    parentCtl.ShowPage(RECIPE_SETUP_TABS.IMAGE_SELECTION, formMode, isFront, selectedRecipe.RecipeID);
            //}
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecipesFromDB();
        }
    }
}
