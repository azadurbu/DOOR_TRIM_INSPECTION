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
    /// Interaction logic for CtrlRecipeWizardROISetupTry.xaml
    /// </summary>
    public partial class CtrlRecipeWizardROISetupTry : UserControl
    {
        public DetectionROIDetailsUI Parameter { get; private set; }
        public new CtrlRecipeWizardROISetup Parent { get; set; }

        public CtrlRecipeWizardROISetupTry()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Parent.try_view.Visibility = Visibility.Hidden;
            Parent.rear_view.Visibility = Visibility.Visible;
        }

        internal void SetPopup(DetectionROIDetailsUI parameter, CtrlRecipeWizardROISetup parent)
        {
            Parameter = parameter;
            Parent = parent;
        }
    }
}
