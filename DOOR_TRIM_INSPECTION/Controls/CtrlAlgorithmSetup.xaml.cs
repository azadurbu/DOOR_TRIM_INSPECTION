using DOOR_TRIM_INSPECTION.Class;
using System.Windows;
using System.Windows.Controls;

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CtrlAlgorithmSetup.xaml
    /// </summary>
    public partial class CtrlAlgorithmSetup : UserControl
    {
        public CtrlAlgorithmSetup()
        {
            InitializeComponent();
        }

        public void LoadAlgorithmsFromDB()
        {
            try { dgAlgorithms.ItemsSource = Machine.hmcDBHelper.GetDetectionClasses(); } catch { }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // ADD NEW ALGORITHM/CLASS
            ctrlAddEditAlgorithm.SetFormMode(FORM_MODE.ADD);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // ADD FORM IN EDIT MODE

            Button button = sender as Button;
            DetectionClass detClass = button.CommandParameter as DetectionClass;

            if (detClass != null)
            {
                ctrlAddEditAlgorithm.SetFormData(detClass);
            }
            ctrlAddEditAlgorithm.SetFormMode(FORM_MODE.EDIT);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            bool result = CustomMessageBox.Show("", Window.GetWindow(this));
            if (result)
            {
                // DELETE ITEM
                Button button = sender as Button;
                DetectionClass detClass = button.CommandParameter as DetectionClass;

                if (Machine.hmcDBHelper.DetectionClassInUse(detClass.DetectionClassID))
                {
                    MessageBox.Show("레시피에서 사용 중인 알고리즘은 삭제할 수 없습니다.");
                    return;
                }

                if (Machine.hmcDBHelper.DeleteDetectionClass(detClass.DetectionClassID))
                    LoadAlgorithmsFromDB();
            }
        }

        private void CtrlAlgorithmSetup_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAlgorithmsFromDB();
            ctrlAddEditAlgorithm.SetParent(this);
        }
    }
}
