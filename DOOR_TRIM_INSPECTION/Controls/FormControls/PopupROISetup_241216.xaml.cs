using DOOR_TRIM_INSPECTION.Class;
using Doortrim_Inspection.Class;
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

namespace DOOR_TRIM_INSPECTION.Controls.FormControls
{
    /// <summary>
    /// Interaction logic for PopupROISetup.xaml
    /// </summary>
    public partial class PopupROISetup : UserControl
    {
        private DetectionROIDetailsUI _RoiInfo;
        private List<DetectionROIDetailsUI> _rectanglesList;
        private DataGrid _rectanglesDataGrid;
        private Canvas _currentCanvas = null;
        private StackPanel stkPanelAddGroup;
        private List<DetectionROIDetailsUI> selectedROIsForChk;
        
        public PopupROISetup(DetectionROIDetailsUI ROIInfo, List<DetectionROIDetailsUI> ROIList, DataGrid rectanglesDataGrid, Canvas currentCanvas, StackPanel _stkPanelAddGroup, List<DetectionROIDetailsUI> _selectedROIsForChk)
        {
            InitializeComponent();
            LoadDetectionClasses();
            _RoiInfo = ROIInfo;
            _rectanglesList = ROIList;
            _currentCanvas = currentCanvas;
            _rectanglesDataGrid = rectanglesDataGrid;
            stkPanelAddGroup = _stkPanelAddGroup;
            selectedROIsForChk = _selectedROIsForChk;

            txtROIName.Text = _RoiInfo.roi_name;
            //cboDetectionClass.SelectedItem = string.IsNullOrEmpty(_RoiInfo.DetectionClassName)
            //? cboDetectionClass.Items.Cast<DetectionClass>().FirstOrDefault() // Select the first item if empty
            //: cboDetectionClass.Items.Cast<DetectionClass>()
            //                          .FirstOrDefault(d => d.DetectionClassName == _RoiInfo.DetectionClassName);

            if(_RoiInfo.detection_class_ID != 0)
                cboDetectionClass.SelectedItem = cboDetectionClass.Items.Cast<DetectionClass>()
                                      .FirstOrDefault(d => d.RuleID == _RoiInfo.detection_class_ID);
        }

        private void LoadDetectionClasses()
        {
            List<DetectionClass> detectionClasses = Machine.hmcDBHelper.GetDetectionClasses();
            cboDetectionClass.ItemsSource = detectionClasses;
            cboDetectionClass.DisplayMemberPath = "DetectionClassName";
            cboDetectionClass.SelectedValuePath = "DetectionClassID";
            
            if (detectionClasses.Count > 0)
            {
                cboDetectionClass.SelectedIndex = 0;
                LoadParameterView((DetectionClass)cboDetectionClass.SelectedItem);
            }
        }

        private List<KeyValuePair<string, string>> ParseParam(string param)
        {
            List<KeyValuePair<string, string>> dlClasses = new List<KeyValuePair<string, string>>(); // LOAD FROM XML (Name, ID)

            string[] datas = param.Split('|');
            if (datas == null)
                return null;
            if (datas[0].Length == 0 || datas[0].IndexOf(":") == -1)
                return null;
            foreach (string s in datas)
            {
                string[] keyvalue = s.Split(':');
                dlClasses.Add(new KeyValuePair<string, string>(keyvalue[0].Trim(), keyvalue[1].Trim()));
            }
            return dlClasses;
        }

        private List<AlgorithmParamOption> GetParameters(string paramString)
        {

            List<KeyValuePair<string, string>> keyValuePairs = ParseParam(paramString);
            if (keyValuePairs == null)
                return null;

            List<AlgorithmParamOption> parameters = new List<AlgorithmParamOption>(); // LOAD FROM XML (Name, ID, Default Value)

            foreach (KeyValuePair<string, string> pair in keyValuePairs)
                parameters.Add(new AlgorithmParamOption(pair.Key.Trim(), pair.Value.Trim()));

            return parameters;

        }

        private void LoadParameterView(DetectionClass selectedItem)
        {
            stkParameters.Children.Clear();
            List<AlgorithmParamOption> algoParams = GetParameters(selectedItem.Parameters);
            foreach (AlgorithmParamOption algoParam in algoParams)
            {
                // ADD VIEW
                PopUpInputRow popInput = new PopUpInputRow();
                popInput.setInfo(algoParam.OptionName, algoParam.Value);
                stkParameters.Children.Add(popInput);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> paramOptions = new List<string>();
                foreach (PopUpInputRow pInput in stkParameters.Children.OfType<PopUpInputRow>().ToList())
                {
                    paramOptions.Add(pInput.getInfo());
                }

                string rectParams = string.Join("|", paramOptions);
                var selectedDetectionClass = cboDetectionClass.SelectedItem as DetectionClass;

                _RoiInfo.roi_name = txtROIName.Text;
                _RoiInfo.roi_name = txtROIName.Text;
                ImageEditorHelper.UpdateRoiInformation(_rectanglesList,
                    _RoiInfo.roi_ui_counter,
                    _RoiInfo.start_x,
                    _RoiInfo.start_y,
                    _RoiInfo.end_x, 
                    _RoiInfo.end_y,
                    _RoiInfo.roi_name,
                    selectedDetectionClass.RuleID,
                    selectedDetectionClass.RuleName,
                    rectParams);
                ImageEditorHelper.LoadROIsList(_rectanglesDataGrid, _rectanglesList);
                ImageEditorHelper.ClearGroup(stkPanelAddGroup, selectedROIsForChk);
                this.Visibility = Visibility.Collapsed;
                ImageEditorHelper.RemoveResizeHandles(_currentCanvas);
                CtrlRecipeWizardROISetup.addbtnsteps = 0;
                CtrlRecipeWizardROISetup.editBtnsteps = 0;
                _rectanglesDataGrid.IsEnabled = true;

            }
            catch (Exception ex)
            {
                Machine.logger.Write(eLogType.ERROR, ex.Message);
            }
        }

        private void CboDetectionClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadParameterView((DetectionClass)cboDetectionClass.SelectedItem);
        }
    }
}
