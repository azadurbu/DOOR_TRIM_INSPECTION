using DOOR_TRIM_INSPECTION.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CtrlAddEditAlgorithm.xaml
    /// </summary>
    public partial class CtrlAddEditAlgorithm : UserControl
    {
        private CtrlAlgorithmSetup parentCtl = null;
        private DetectionClass curData = null;
        private int paramsSelectedIndex = -1;
        private bool isEdit = false;
        public CtrlAddEditAlgorithm()
        {
            InitializeComponent();


           
        }

        public void SetParent(CtrlAlgorithmSetup algoSetup)
        {
            parentCtl = algoSetup;
        }

        public void SetFormMode(FORM_MODE mode)
        {
            cboRuleName.IsEnabled = mode != FORM_MODE.DISABLED;
            //lstParameters.IsEnabled = mode != FORM_MODE.DISABLED;
            txtAlgorithmName.IsEnabled = mode != FORM_MODE.DISABLED;
            btnSave.IsEnabled = mode != FORM_MODE.DISABLED;
            btnCancel.IsEnabled = mode != FORM_MODE.DISABLED;
            //if (mode == FORM_MODE.DISABLED)
            //    gridParams.Visibility = Visibility.Hidden;
            //else gridParams.Visibility = Visibility.Visible;

        }

        /// <summary>
        /// Load Form Combo and Listbox from XML
        /// </summary>
        private void LoadControlData()
        {
            List<DETECT_NAME> dETECT_LISTs = Machine.hmcDBHelper.GetDetectList();

            cboRuleName.ItemsSource = dETECT_LISTs;
            cboRuleName.DisplayMemberPath = "NameKr";
            cboRuleName.SelectedValuePath = "RuleID";
            cboRuleName.SelectedIndex = 0;
        }
        /// <summary>
        /// Init Form :: Deselect All
        /// </summary>
        private void InitForm()
        {
            txtAlgorithmName.Text = string.Empty;
            cboRuleName.SelectedValue = 0;
            //lstParameters.UnselectAll();
            curData = null;
        }

        public void SetFormData(DetectionClass detClass)
        {
            isEdit = true;
            curData = detClass;

            txtAlgorithmName.Text = curData.DetectionClassName;
            //cboDLClass.SelectedValue = curData.DLClassID;
            //for(int i=0;i<cboRuleName.Items.Count;i++)
            //{
            //    if (((DETECT_LIST)cboRuleName.Items[i]).NameKr.Trim().ToUpper() == curData.DetectionClassName.Trim().ToUpper())
            //        cboRuleName.SelectedIndex = i;
            //}
            cboRuleName.SelectedValue= curData.RuleID ;
            //cboRuleName.SelectedItem = curData.DetectionClassName;

            //List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(curData.Parameters);
            //if (keyValuePairs == null)
            //    return;

            //List<AlgorithmParamOption> parameters = new List<AlgorithmParamOption>(); // LOAD FROM XML (Name, ID, Default Value)

            //foreach (KeyValuePair<string, string> pair in keyValuePairs)
            //    parameters.Add(new AlgorithmParamOption(pair.Key.Trim(), pair.Value.Trim()));

            //lstParameters.SelectionMode = SelectionMode.Extended;
            //lstParameters.ItemsSource = parameters;
            //lstParameters.DisplayMemberPath = "OptionName";
            //lstParameters.SelectedValuePath = "Item1";

            LoadParameterView(detClass.Parameters);


        }

        private void LoadParameterView(string parameters)
        {
            stkParameters.Children.Clear();
            List<AlgorithmParamOption> algoParams = GetParameters(parameters);
            foreach (AlgorithmParamOption algoParam in algoParams)
            {
                FormControls.PopUpInputRow popUpInput = new FormControls.PopUpInputRow();
                popUpInput.setInfo(algoParam.OptionName, algoParam.Value);
                ((App)Application.Current).SelectedROI = new OpenCvSharp.Rect(0,0,0,0);
                stkParameters.Children.Add(popUpInput);
            }
        }

        private List<AlgorithmParamOption> GetParameters(string paramString)
        {

            List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(paramString);
            if (keyValuePairs == null)
                return null;

            List<AlgorithmParamOption> parameters = new List<AlgorithmParamOption>(); // LOAD FROM XML (Name, ID, Default Value)

            foreach (KeyValuePair<string, string> pair in keyValuePairs)
                parameters.Add(new AlgorithmParamOption(pair.Key.Trim(), pair.Value.Trim()));

            return parameters;

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            InitForm();
            //cbxUseParam_ChecekdChange(false);
            SetFormMode(FORM_MODE.DISABLED);
            isEdit = false;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            DetectionClass detClass = new DetectionClass();

            DetectionClass detectionClass;
            if (curData != null)
            {
                detClass.DetectionClassID = curData.DetectionClassID;
                //detClass.Parameters = curData.Parameters;
            }
            
            if (isEdit)
                detectionClass = Machine.hmcDBHelper.GetDetectionClass(curData.DetectionClassID);
            else
                detectionClass = Machine.hmcDBHelper.GetDetectionDefaultClasses(((DETECT_NAME)cboRuleName.SelectedItem).NameEn.Trim().ToUpper());
            
            detClass.DetectionClassName = txtAlgorithmName.Text;
            detClass.DetectionClassID = (int)detectionClass.DetectionClassID;
            detClass.RuleID = (int)detectionClass.RuleID;
            //if(detClass.Parameters==null)
            //    detClass.Parameters = detectionClass.Parameters;

            // MEER 2025.01.20
            detClass.Parameters = GetParameters();


            detClass.RuleName = txtAlgorithmName.Text.Trim();

            if (Machine.hmcDBHelper.SaveDetectionClass(detClass))
            {
                parentCtl.LoadAlgorithmsFromDB();
                InitForm();
                SetFormMode(FORM_MODE.DISABLED);
            }
            isEdit = false;
        }

        private string GetParameters()
        {
            string rectParams = "";
            List<string> paramOptions = new List<string>();
            foreach (FormControls.PopUpInputRow pInput in stkParameters.Children.OfType<FormControls.PopUpInputRow>().ToList())
            {
                paramOptions.Add(pInput.getInfo());
            }

            rectParams = string.Join("|", paramOptions);

            return rectParams;
        }

        private void ShowParamForm(bool Show)
        {
            //lbParamName.Visibility = Show ? Visibility.Visible : Visibility.Collapsed;
            //txtParamValue.Visibility = Show ? Visibility.Visible : Visibility.Collapsed;
            //btnParamSave.Visibility = Show ? Visibility.Visible : Visibility.Collapsed;
        
        }

        private void lstParameters_Selected(object sender, SelectionChangedEventArgs e)
        {
            //if (lstParameters.SelectedItem == null)
            //{
            //    lbParamName.Content = "";
            //    txtParamValue.Text = "";
            //    ShowParamForm(false);
            //    return;
            //}

            //ShowParamForm(true);
            //string strParamName = ((AlgorithmParamOption)lstParameters.SelectedItem).OptionName.ToString();
            //char[] charArray = new char[strParamName.Length + 5]; // 5 Space for the Spaces :-)
            //string explodedParamName = "";
            //foreach (char c in strParamName)
            //    explodedParamName += ((c >= 65 && c <= 90) ? " " + c.ToString() : c.ToString());

            //lbParamName.Content = explodedParamName.Trim(); // ((AlgorithmParamOption)lstParameters.SelectedItem).OptionName.ToString();
            //txtParamValue.Text = ((AlgorithmParamOption)lstParameters.SelectedItem).Value.ToString();


            //cbxUseParam_ChecekdChange(true);
        }

        //private void cbxUseParam_ChecekdChange(bool enable)
        //{
        //        lbParamName.IsEnabled = enable;
        //        txtParamValue.IsEnabled = enable;
        //}


        private void cboRuleName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int idx = cboRuleName.SelectedIndex;
            if (idx == -1)
            {
                stkParameters.Children.Clear();
                return;
            }

            string value = ((DETECT_NAME)cboRuleName.Items[idx]).NameEn.Trim();

            DetectionClass detectionClass = Machine.hmcDBHelper.GetDetectionDefaultClasses(value.Trim().ToUpper());


            LoadParameterView(detectionClass.Parameters);
        }

        private void BtnParamSave_Click(object sender, RoutedEventArgs e)
        {
            //DetectionClass detClass = new DetectionClass();

            //if (curData != null)
            //    detClass.DetectionClassID = curData.DetectionClassID;
            //DetectionClass detectionClass;
            //if (isEdit)
            //    detectionClass = Machine.hmcDBHelper.GetDetectionClass(curData.DetectionClassID);
            //else
            //    detectionClass = Machine.hmcDBHelper.GetDetectionDefaultClasses(((DETECT_NAME)cboRuleName.SelectedItem).NameEn.Trim().ToUpper());

            //curData = detectionClass;
            //List<KeyValuePair<string, string>> keyValuePairs = HanMechDBHelper.ParseParam(detectionClass.Parameters);

            //string ParamName = ((AlgorithmParamOption)lstParameters.SelectedItem).OptionName.ToString();
            //string ParamValue = txtParamValue.Text;

            //for (int i=0;i< keyValuePairs.Count;i++)
            //{
            //    if (keyValuePairs[i].Key == ParamName)
            //    {
            //        keyValuePairs[i] = new KeyValuePair<string, string>(ParamName, ParamValue);
            //        break;
            //    }
            //}
            //string ParamData = string.Join("|", keyValuePairs.Select(kvp => $"{kvp.Key};{kvp.Value}"));
            //curData.Parameters = ParamData;
            //((AlgorithmParamOption)lstParameters.SelectedItem).Value = ParamValue;
        }

        private void CtrlAddEditAlgorithm_Loaded(object sender, RoutedEventArgs e)
        {
            try { LoadControlData(); } catch { }
            InitForm(); // ADD NEW VIEW
            SetFormMode(FORM_MODE.DISABLED);
        }
    }
}
