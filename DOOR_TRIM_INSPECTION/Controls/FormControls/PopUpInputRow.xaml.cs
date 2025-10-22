using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.IO;
using Microsoft.Win32;

namespace DOOR_TRIM_INSPECTION.Controls.FormControls
{
    /// <summary>
    /// Interaction logic for PopUpInputRow.xaml
    /// </summary>
    public partial class PopUpInputRow : UserControl
    {
        private enum RowMode
        {
            TEXTBOX,
            COLOR_SLIDER,
            NUMERIC_UPDOWN,
            COMBO_BOX,
            COLOR_PICKER,
            IMAGE_UPLOADER,
            IMAGE_COLOR_PICKER,
            ACCU_SLIDER,
            PLUG_DISTANSE,
            PLUG_ALTERNATE_ROI,
#if USE_COGNEX
            PLUG_VPP_SELECT,
            PLUG_DISTANSE_COG
#endif
        }

        private string TemplatePath {
            get {
                return
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                         "images\\Recipe");
            }
        }

        private RowMode rowMode { get; set; }
        public static string SelectedFilePath { get; private set; }

        private Mat CropMasterImage = null;
        public PopUpInputRow()
        {
            InitializeComponent();
            CropMasterImage = null;
        }

        private void SetValue(RowMode rowMode, String Key, String Value)
        {
            this.rowMode = rowMode;
            SetUI(this.rowMode);

            switch (this.rowMode)
            {
                case RowMode.TEXTBOX:
                    SetTextGridValue(Key, Value);
                    break;
                case RowMode.COLOR_SLIDER:
                    SetColorSliderGridValue(Key, Value);
                    break;
                case RowMode.NUMERIC_UPDOWN:
                    SetNumericUpDownGridValue(Key, Value);
                    break;
                case RowMode.COMBO_BOX:
                    SetComboGridValue(Key, Value);
                    break;
                case RowMode.COLOR_PICKER:
                    SetColorPickerGridValue(Key, Value);
                    break;
                case RowMode.IMAGE_UPLOADER:
                    SetImageUploaderGridValue(Key, Value);
                    break;
                case RowMode.IMAGE_COLOR_PICKER:
                    SetImageColorPickerGridValue(Key, Value);
                    break;
                case RowMode.ACCU_SLIDER:
                    SetAccuSliderGridValue(Key, Value);
                    break;
                case RowMode.PLUG_DISTANSE:
                    SetPlugDistanceGridValue(Key, Value);
                    break;
                case RowMode.PLUG_ALTERNATE_ROI:
                    SetPlugAlternateROIGridValue(Key, Value);
                    break;
#if USE_COGNEX
                case RowMode.PLUG_VPP_SELECT:
                    SetPlugVppGridValue(Key, Value);
                    break;
                case RowMode.PLUG_DISTANSE_COG:
                    SetPlugDistanceCogGridValue(Key, Value);
                    break;
#endif
            }
        }


        private void SetPlugDistanceCogGridValue(string key, string value)
        {
            lblCogPlugDistance.Content = key;
            txtCogPlugDistance.Text = value;
        }
        private void SetPlugVppGridValue(string key, string value)
        {
            lblVppSelection.Content = key;
            txtVppSelection.Text = value;
        }

        private void SetImageColorPickerGridValue(string key, string value)
        {
            lblImageColorPicker.Content = key;
            txtImageColorPicker.Text = value;
            string[] rgb = value.Split(',');
            rectImageColorPicker.Fill = new SolidColorBrush(Color.FromRgb(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2])));
        }

        private void SetImageUploaderGridValue(string key, string value)
        {
            lblImageSelection.Content = key;
            txtImageSelection.Text = value;
        }

        private void SetPlugDistanceGridValue(string key, string value)
        {
            lblPlugDistance.Content = key;
            txtPlugDistance.Text = value;
        }

        private void SetPlugAlternateROIGridValue(string key, string value)
        {
            lblPlugROI.Content = key;
            string[] splits = value.Split(',');
            if (splits.Count() > 4)
            {
                txtPlugAlternateROI.Text = $"{splits[0]},{splits[1]},{splits[2]},{splits[3]}";
                txtAlternateROIImage.Text = splits[4];
            }
            else
            {
                txtPlugAlternateROI.Text = value;
                txtAlternateROIImage.Text = "";
            }
        }

        private void SetColorPickerGridValue(string key, string value)
        {
            lblColorPicker.Content = key;
            txtColorPicker.Text = value;
            string[] rgbParts = value.Split(',');
            if (rgbParts.Length == 3)
            {
                byte r = byte.Parse(rgbParts[0]);
                byte g = byte.Parse(rgbParts[1]);
                byte b = byte.Parse(rgbParts[2]);

                var selectedColor = System.Windows.Media.Color.FromRgb(r, g, b);
                rectColorPicker.Fill = new SolidColorBrush(selectedColor);
            }
        }

        private void SetComboGridValue(string key, string value)
        {
            lblCombo.Content = key;
            cmbCombo.Items.Clear();

            List<KeyValuePair<string, string>> cmbValues = new List<KeyValuePair<string, string>>();
            cmbValues.Add(new KeyValuePair<string, string>((string)FindResource("TXT_POPUP_UP_DIRECTION"), "Up"));
            cmbValues.Add(new KeyValuePair<string, string>((string)FindResource("TXT_POPUP_DOWN_DIRECTION"), "Down"));
            cmbValues.Add(new KeyValuePair<string, string>((string)FindResource("TXT_POPUP_LEFT_DIRECTION"), "Left"));
            cmbValues.Add(new KeyValuePair<string, string>((string)FindResource("TXT_POPUP_RIGHT_DIRECTION"), "Right"));


            cmbCombo.ItemsSource = cmbValues;
            cmbCombo.DisplayMemberPath = "Key";
            cmbCombo.SelectedValuePath = "Value";

            cmbCombo.SelectedValue = value;
        }

        private void SetNumericUpDownGridValue(string key, string value)
        {
            lblNumUpDown.Content = key;
            txtNumUpDown.Text = value;
        }

        private void SetColorSliderGridValue(string key, string value)
        {
            lblColorSlider.Content = key;
            sliderColor.Value = int.Parse(value);
            txtColorSlider.Text = value;
        }

        private void SetAccuSliderGridValue(string key, string value)
        {
            lblAccuSlider.Content = key;
            sliderAccu.Value = int.Parse(value);
            txtAccuSlider.Text = value;
        }

        private void SetTextGridValue(string key, string value)
        {
            lblText.Content = key;
            txtText.Text = value;
        }

        private void SetUI(RowMode rowMode)
        {

            gridTxt.Visibility = Visibility.Collapsed;
            gridColorSlider.Visibility = Visibility.Collapsed;
            gridAccuSlider.Visibility = Visibility.Collapsed;
            gridNumUpDown.Visibility = Visibility.Collapsed;
            gridCombo.Visibility = Visibility.Collapsed;
            gridColorPicker.Visibility = Visibility.Collapsed;
            gridImageSelection.Visibility = Visibility.Collapsed;
            gridImageColorPicker.Visibility = Visibility.Collapsed;
            gridPlugDistance.Visibility = Visibility.Collapsed;
            gridPlugAlternateROI.Visibility = Visibility.Collapsed;
#if USE_COGNEX
            gridVppSelection.Visibility = Visibility.Collapsed;
            gridCogPlugDistance.Visibility = Visibility.Collapsed;
#endif
            switch (this.rowMode)
            {
                case RowMode.TEXTBOX:
                    gridTxt.Visibility = Visibility.Visible;
                    break;
                case RowMode.COLOR_SLIDER:
                    gridColorSlider.Visibility = Visibility.Visible;
                    break;
                case RowMode.NUMERIC_UPDOWN:
                    gridNumUpDown.Visibility = Visibility.Visible;
                    break;
                case RowMode.COMBO_BOX:
                    gridCombo.Visibility = Visibility.Visible;
                    break;
                case RowMode.COLOR_PICKER:
                    gridColorPicker.Visibility = Visibility.Visible;
                    break;
                case RowMode.IMAGE_UPLOADER:
                    gridImageSelection.Visibility = Visibility.Visible;
                    break;
                case RowMode.IMAGE_COLOR_PICKER:
                    gridImageColorPicker.Visibility = Visibility.Visible;
                    break;
                case RowMode.ACCU_SLIDER:
                    gridAccuSlider.Visibility = Visibility.Visible;
                    break;
                case RowMode.PLUG_DISTANSE:
                    gridPlugDistance.Visibility = Visibility.Visible;
                    break;
                case RowMode.PLUG_ALTERNATE_ROI:
                    gridPlugAlternateROI.Visibility = Visibility.Visible;
                    break;
#if USE_COGNEX
                case RowMode.PLUG_VPP_SELECT:
                    gridVppSelection.Visibility = Visibility.Visible;
                    break;
                case RowMode.PLUG_DISTANSE_COG:
                    gridCogPlugDistance.Visibility = Visibility.Visible;
                    break;
#endif
            }

        }

        public void setInfo(string name, string value)
        {
            string comString = name.ToLower();
            RowMode rowMode = RowMode.TEXTBOX;
            if (comString.Contains("template"))
            {
                // SHOW FILE UPLOAD BUTTON
                rowMode = RowMode.IMAGE_UPLOADER;
            }
            else if (comString.Contains("thresh"))
            {
                // SHOW SLIDER
                rowMode = RowMode.COLOR_SLIDER;
            }
            else if (comString.Contains("accuracy") || comString.Contains("outerconfidence")
                || comString.Contains("innerconfidence"))
            {
                // SHOW SLIDER
                rowMode = RowMode.ACCU_SLIDER;
            }
            else if (comString.Contains("area") || comString.Contains("error")
                || comString.Contains("length") || comString.Contains("bound")|| comString.Contains("variance"))
            {
                rowMode = RowMode.NUMERIC_UPDOWN;
            }
            else if (comString.Equals("color"))
            {
                rowMode = RowMode.COLOR_PICKER;
            }
            else if (comString.Equals("avgcolor"))
            {
                rowMode = RowMode.IMAGE_COLOR_PICKER;
            }
            else if (comString.Contains("direction"))
            {
                rowMode = RowMode.COMBO_BOX;
            }
            else if (comString.Contains("plugdistance"))
            {
                rowMode = RowMode.PLUG_DISTANSE;
            }
            else if (comString.Contains("alternateroi"))
            {
                rowMode = RowMode.PLUG_ALTERNATE_ROI;

            }
#if USE_COGNEX
            else if (comString.Contains("vpppath"))
            {
                rowMode = RowMode.PLUG_VPP_SELECT;
            }
            else if (comString.Contains("plugcogdistance"))
            {
                rowMode = RowMode.PLUG_DISTANSE_COG;
            }
#endif
            SetValue(rowMode, name, value);
        }

        public void setInfoMasterCropImage(string name, string value, Mat CropMasterImage)
        {
            if (CropMasterImage == null)
                return;
            string comString = name.ToLower();
            if (comString.Contains("plugdistance"))
            {
                this.CropMasterImage = CropMasterImage.Clone();
            }
        }

        public string getInfo()
        {
            string retVal = "";
            switch (this.rowMode)
            {
                case RowMode.TEXTBOX:
                    retVal = $"{lblText.Content};{txtText.Text}";
                    break;
                case RowMode.COLOR_SLIDER:
                    retVal = $"{lblColorSlider.Content};{txtColorSlider.Text}";
                    break;
                case RowMode.NUMERIC_UPDOWN:
                    retVal = $"{lblNumUpDown.Content};{txtNumUpDown.Text}";
                    break;
                case RowMode.COMBO_BOX:
                    retVal = $"{lblCombo.Content};{cmbCombo.SelectedValue}";
                    break;
                case RowMode.COLOR_PICKER:
                    retVal = $"{lblColorPicker.Content};{txtColorPicker.Text}";
                    break;
                case RowMode.IMAGE_UPLOADER:
                    retVal = $"{lblImageSelection.Content};{txtImageSelection.Text}";
                    break;
                case RowMode.IMAGE_COLOR_PICKER:
                    retVal = $"{lblImageColorPicker.Content};{txtImageColorPicker.Text}";
                    break;
                case RowMode.ACCU_SLIDER:
                    retVal = $"{lblAccuSlider.Content};{txtAccuSlider.Text}";
                    break;
                case RowMode.PLUG_DISTANSE:
                    retVal = $"{lblPlugDistance.Content};{txtPlugDistance.Text}";
                    break;
                case RowMode.PLUG_ALTERNATE_ROI:
                    retVal = $"{lblPlugROI.Content};{txtPlugAlternateROI.Text},{txtAlternateROIImage.Text}";
                    break;
#if USE_COGNEX
                case RowMode.PLUG_VPP_SELECT:
                    retVal = $"{lblVppSelection.Content};{txtVppSelection.Text}";
                    break;
                case RowMode.PLUG_DISTANSE_COG:
                    retVal = $"{lblCogPlugDistance.Content};{txtCogPlugDistance.Text}";
                    break;
#endif
            }
            return retVal;
        }
        public static string existingTemplate;
        private void BtnImageSelection_Click(object sender, RoutedEventArgs e)
        {
            Controls.CustomROIImgPickerBox roiBox = new Controls.CustomROIImgPickerBox();
            roiBox.Owner = System.Windows.Window.GetWindow(this);
            string path = ((App)Application.Current).GetCurrentRecipeImagePath(false);
            FileInfo di = new FileInfo(path);
            List<string> imageFiles = GetImagesByPattern(di.DirectoryName, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();

            SelectedFilePath = imageFiles[0];

            if (imageFiles.Count > 0)
            {
                roiBox.SetFilePath(imageFiles[0]);
            }
            existingTemplate = txtImageSelection.Text;

            var result = roiBox.ShowDialog() == true;
            string TemplatePath = result ? roiBox.TemplatePath : "";
            if (TemplatePath != "")
            {
                txtImageSelection.Text = TemplatePath;
                existingTemplate = "";
            }
                
        }

        //private void BtnImageSelection_Click(object sender, RoutedEventArgs e)
        //{
        //    using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
        //    {
        //        openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp";
        //        openFileDialog.InitialDirectory = System.Windows.Forms.Application.StartupPath + @"\Images\Recipe\";
        //        System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

        //        if (result == System.Windows.Forms.DialogResult.OK)
        //        {
        //            // COPY FILE TO THE APPLICATION FOLDER // MEER 2025.01.15
        //            string oriPath = openFileDialog.FileName;
        //            string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
        //            string fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
        //            string fileExt = System.IO.Path.GetExtension(fileName);
        //            string newFileName = fileNameWithoutExt + DateTime.Now.ToString("_HH_mm_ss") + fileExt;
        //            string newPath = System.IO.Path.Combine(TemplatePath, "backup");
        //            string newFilePath = System.IO.Path.Combine(newPath, newFileName);

        //            if (!Directory.Exists(newPath))
        //            {
        //                Directory.CreateDirectory(newPath);
        //            }
        //            System.IO.File.Copy(oriPath, newFilePath, true);
        //            // COPY FILE TO THE APPLICATION FOLDER // MEER 2025.01.15
        //            txtImageSelection.Text = newFilePath;

        //            //if (lblImageSelection.Content.ToString()== "TemplatePath1")
        //            //    templatePath1 = newPath;

        //            //if (lblImageSelection.Content.ToString() == "TemplatePath2")
        //            //    templatePath2 = newPath;
        //        }
        //    }
        //}

        private void SliderColor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtColorSlider.Text = sliderColor.Value.ToString();
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            int numVal = int.Parse(txtNumUpDown.Text);
            if (numVal > 10)
                txtNumUpDown.Text = (numVal - 10).ToString();
            else
                txtNumUpDown.Text = (0).ToString();
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            int numVal = int.Parse(txtNumUpDown.Text);
            txtNumUpDown.Text = (numVal + 10).ToString();

        }

        private void BtnColorPicker_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog())
            {
                string[] values = txtColorPicker.Text.Split(',');
                System.Drawing.Color temp = System.Drawing.Color.FromArgb(255, byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]));
                colorDialog.Color = temp;

                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var selectedColor = System.Windows.Media.Color.FromArgb(
                        colorDialog.Color.A,
                        colorDialog.Color.R,
                        colorDialog.Color.G,
                        colorDialog.Color.B);

                    rectColorPicker.Fill = new SolidColorBrush(selectedColor);
                    txtColorPicker.Text = $"{selectedColor.R},{selectedColor.G},{selectedColor.B}";
                }
            }
        }

        private void BtnImageColorPicker_Click(object sender, RoutedEventArgs e)
        {
            string[] values = txtImageColorPicker.Text.Split(',');
            Controls.CustomFileSelectionBox cfBox = new Controls.CustomFileSelectionBox();
            cfBox.Owner = System.Windows.Window.GetWindow(this);
            var result = cfBox.ShowDialog() == true; 
            Color detColor = result ? cfBox.DetectedColor : Color.FromRgb(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]));
            rectImageColorPicker.Fill = new SolidColorBrush(detColor);
            txtImageColorPicker.Text = $"{detColor.R},{detColor.G},{detColor.B}"; ;
        }

        private void NumericTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]$"))
            {
                e.Handled = true;  // Cancel the input
            }
        }

        private void TxtColorSlider_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtColorSlider.Text))
            {
                sliderColor.Value = 0;
                return;

            }
            if (int.Parse(txtColorSlider.Text) > 255)
                txtColorSlider.Text = "255";
            sliderColor.Value = int.Parse(txtColorSlider.Text);
        }


        private void TxtAccuSlider_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtAccuSlider.Text))
            {
                sliderColor.Value = 0;
                return;

            }
            if (int.Parse(txtAccuSlider.Text) > 100)
                txtAccuSlider.Text = "100";
            sliderAccu.Value = int.Parse(txtAccuSlider.Text);
        }

        private void SliderAccu_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtAccuSlider.Text = sliderAccu.Value.ToString();
        }

        private void TxtColorPicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            //string input = txtColorPicker.Text.Trim();

            //var regex = new System.Text.RegularExpressions.Regex(@"^(\d{1,3}),(\d{1,3}),(\d{1,3})$");

            //if (regex.IsMatch(input))
            //{
            //    var rgbValues = input.Split(',');

            //    int r = int.Parse(rgbValues[0]);
            //    int g = int.Parse(rgbValues[1]);
            //    int b = int.Parse(rgbValues[2]);

            //    if (r > 255)
            //    {
            //        r = 255;
            //    }
            //    if ( g > 255)
            //    {
            //        g = 255;
            //    }
            //    if ( b > 255)
            //    {
            //        b = 255;
            //    }
            //    txtColorPicker.Text = $"{r},{g},{b}";
            //    Color color = Color.FromRgb((byte)r, (byte)g, (byte)b);
            //    rectColorPicker.Fill = new SolidColorBrush(color);


            //}

            string[] parts =  ((TextBox)sender).Text.Split(',');

            // Check if there are exactly 3 parts (R, G, B)
            if (parts.Length == 3)
            {
                // Validate each part is a number between 0 and 255
                for (int i = 0; i < 3; i++)
                {
                    if (!int.TryParse(parts[i], out int value) || value < 0 || value > 255)
                    {
                        parts[i] = "255";
                    }
                }

                int r = int.Parse(parts[0]);
                int g = int.Parse(parts[1]);
                int b = int.Parse(parts[2]);
                if(((TextBox)sender).Name== "txtImageColorPicker")
                {
                    txtImageColorPicker.Text = $"{r},{g},{b}";
                    Color color = Color.FromRgb((byte)r, (byte)g, (byte)b);
                    rectImageColorPicker.Fill = new SolidColorBrush(color);

                }
                else if (((TextBox)sender).Name == "txtColorPicker")
                {
                    txtColorPicker.Text = $"{r},{g},{b}";
                    Color color = Color.FromRgb((byte)r, (byte)g, (byte)b);
                    rectColorPicker.Fill = new SolidColorBrush(color);

                }
            }
            else
            {
                // Invalid format: Show a warning if there aren't exactly 3 parts
            }

        }


        private void TxtColorPicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9,]$"))  // Allow digits and comma
            {
                e.Handled = true;
            }
        }

        private void btnGetDistance_Click(object sender, RoutedEventArgs e)
        {
          //  return; // MEER STOPPING THE CODE TEMPORARILY

            bool tParent = this.Parent is StackPanel;

            //Bitmap bitmap = ((App)Application.Current).GetImage(15, false);
            //rect 
            Mat img = null;
            bool useAlternateROI = false;

            string imagePath = ((App)Application.Current).GetCurrentRecipeRearSub1ImagePath();

                imagePath = ((App)Application.Current).GetCurrentRecipeImagePath(false);
                img = Cv2.ImRead(imagePath);

            OpenCvSharp.Rect AlternateROI = new OpenCvSharp.Rect(0, 0, 0, 0);
            string Direction = "";
            string templateBgr1Path = "";
            string templateBgr2Path = "";

            OpenCvSharp.Rect temp = ((App)Application.Current).SelectedROI;
            // FIND PATH FROM PARENT
            if (this.Parent is StackPanel)
            {
                StackPanel stkPanel = this.Parent as StackPanel;
                foreach (PopUpInputRow row in stkPanel.Children.OfType<PopUpInputRow>().ToList())
                {
                    string rowData = row.getInfo();

                    if (rowData.ToLower().Contains("templatepath1"))
                    {
                        string[] splits = rowData.Split(';');
                        templateBgr1Path = splits[1];
                    }
                    if (rowData.ToLower().Contains("templatepath2"))
                    {
                        string[] splits = rowData.Split(';');
                        templateBgr2Path = splits[1];
                    }
                    if (rowData.ToLower().Contains("direction"))
                    {
                        string[] splits = rowData.Split(';');
                        Direction = splits[1];
                    }
                    if (rowData.ToLower().Contains("alternateroi"))
                    {
                        if (rowData == null)
                            continue;
                        if (rowData.Length == 0)
                            continue;
                        string[] splits = rowData.Split(';');
                        string[] valus = splits[1].Split(',');
                        AlternateROI = new OpenCvSharp.Rect(int.Parse(valus[0]), int.Parse(valus[1]), int.Parse(valus[2]), int.Parse(valus[3]));
                        
                        if (AlternateROI.Width > 0 && AlternateROI.Height > 0)
                        {
                            imagePath = ((App)Application.Current).GetCurrentRecipeRearSub1ImagePath();
                            img = Cv2.ImRead(imagePath);
                            img =LevelOps.EqualizeHistColor(img);
                            temp = AlternateROI;
                        }
                    }
                }
            }
            // FIND PATH FROM PARENT

            //int newWidth = (int)(temp.Width * 1.3);
            //int newHeight = (int)(temp.Height * 1.3);
            //temp = new OpenCvSharp.Rect(temp.X - ((newWidth - temp.Width) / 2), temp.Y - ((newHeight - temp.Height) / 2), newWidth, newHeight);
            Mat RoiImg = null;
                RoiImg = new Mat(img, temp);

            Mat templateBgr1 = Cv2.ImRead(templateBgr1Path);
            Mat templateBgr2 = Cv2.ImRead(templateBgr2Path);

            RoiImg = PreProcessImagePlug(RoiImg);
            templateBgr1 = PreProcessImagePlug(templateBgr1);
            templateBgr2 = PreProcessImagePlug(templateBgr2);

            Cv2.ImWrite("Try_main.bmp", RoiImg);
            // 템플릿 매칭 
            Mat result1 = new Mat();
            Mat result2 = new Mat();

            Cv2.MatchTemplate(RoiImg, templateBgr1, result1, TemplateMatchModes.CCoeffNormed);
            Cv2.MatchTemplate(RoiImg, templateBgr2, result2, TemplateMatchModes.CCoeffNormed);

            // 가장 높은 매칭 점수를 가진 좌표 찾기 
            double minVal, maxVal;
            OpenCvSharp.Point minLoc, maxLoc;
            Cv2.MinMaxLoc(result1, out minVal, out maxVal, out minLoc, out maxLoc);
            double maxValTempl1 = maxVal;
            // 사각형의 크기와 위치 설정 
            OpenCvSharp.Point topLeft1 = maxLoc;
            Cv2.MinMaxLoc(result2, out minVal, out maxVal, out minLoc, out maxLoc);
            double maxValTempl2 = maxVal;
            // 사각형의 크기와 위치 설정 
            OpenCvSharp.Point topLeft2 = maxLoc;
            int Distance = int.MinValue;
            if (lblPlugDistance.Content.ToString() == "PlugDistanceY")
            {
                Distance = topLeft2.Y - topLeft1.Y;
            }
            else
            {
                Distance = topLeft2.X - topLeft1.X;
            }
            //if (Direction.ToLower() == "left")
            //    Distance = topLeft2.X - topLeft1.X;
            //else if (Direction.ToLower() == "right")
            //    Distance = topLeft1.X - topLeft2.X;
            //else if (Direction.ToLower() == "up")
            //    Distance = topLeft2.Y - topLeft1.Y;
            //else
            //    Distance = topLeft1.Y - topLeft2.Y;
            txtPlugDistance.Text = Distance.ToString();
        }

        private static Mat PreProcessImagePlug(Mat image)
        {
            try
            {
                Mat grayImage = new Mat();
                Mat blurredImage = new Mat();
                // 1. 그레이스케일 변환
                if (image.Channels() != 1)
                    Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);

                // 3. 가우시안 블러
                Cv2.GaussianBlur(grayImage, blurredImage, new OpenCvSharp.Size(5, 5), 0);

                return blurredImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine("전처리 중 오류 발생: " + ex.ToString());
                Machine.logger.Write(eLogType.ERROR, ex.ToString());
                return image; // 오류 발생 시 원본 이미지를 반환
            }
        }
        private void TxtImageColorPicker_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TxtImageColorPicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

       

        private void BtnROISelection_Click(object sender, RoutedEventArgs e)
        {
            Controls.CustomROIPickerBox roiBox = new Controls.CustomROIPickerBox();
            roiBox.Owner = System.Windows.Window.GetWindow(this);
            string path = ((App)Application.Current).GetCurrentRecipeImagePath(false);
            FileInfo di = new FileInfo(path);
            List<string> imageFiles = GetImagesByPattern(di.DirectoryName, @"rear_\d{6}.bmp_\d{6}_sub_1$").ToList();

            if (imageFiles.Count > 0)
            {
                roiBox.SetFilePath(imageFiles[0]);
                roiBox.SetROI(txtPlugAlternateROI.Text);
            }
            
            var result = roiBox.ShowDialog() == true;
            string SelectedROI = result ? roiBox.SelectedROI : "";
            if (SelectedROI.Length != 0)
            {
                txtPlugAlternateROI.Text = SelectedROI;
                // CROP IMAGE AND SAVE PATH
                if (SelectedROI != "0,0,0,0")
                {
                    string[] lits = SelectedROI.Split(',');
                    int x = int.Parse(lits[0]);
                    int y = int.Parse(lits[1]);
                    int width = int.Parse(lits[2]);
                    int height = int.Parse(lits[3]);

                    Mat img = new Mat(roiBox.SelectedFilePath);
                    if (img != null)
                    {
                        Mat cropImg = new Mat(img, new OpenCvSharp.Rect(x, y, width, height));
                        string recipeImagePath = ((App)Application.Current).GetCurrentRecipeImagePath(false);
                        recipeImagePath = Path.GetDirectoryName(recipeImagePath);
                        string saveFilePath = Path.Combine(recipeImagePath, $"vpp_master_{DateTime.Now:yyyyMMdd_HHmmss}.bmp");
                        cropImg.SaveImage(saveFilePath);
                        txtAlternateROIImage.Text = saveFilePath;
                    }
                }
            }
        }

        private string[] GetImagesByPattern(string folderPath, string pattern)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);
            return System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly)
                            .Where(file => regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(file)))
                            .ToArray();
        }

        private void txtNumUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            // Only allow numbers
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)))
            {
                e.Handled = true; // Cancel the input
            }
        }

        private void BtnVppSelection_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "VPP files (*.vpp)|*.vpp";
            openFileDialog.Title = "Select a VPP File";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                txtVppSelection.Text = $"{selectedFilePath}";
            }
        }

        private void btnGetCogDistance_Click(object sender, RoutedEventArgs e)
        {
#if USE_COGNEX
            bool tParent = this.Parent is StackPanel;

            string templateVPP1Path = "";
            string templateVPP2Path = "";
            string AlternateROIPath = "";
            if (tParent)
            {
                StackPanel stkPanel = this.Parent as StackPanel;
                foreach (PopUpInputRow row in stkPanel.Children.OfType<PopUpInputRow>().ToList())
                {
                    string rowData = row.getInfo();

                    if (rowData.ToLower().Contains("plugvpppath1"))
                    {
                        string[] splits = rowData.Split(';');
                        templateVPP1Path = splits[1];
                    }
                    if (rowData.ToLower().Contains("plugvpppath2"))
                    {
                        string[] splits = rowData.Split(';');
                        templateVPP2Path = splits[1];
                    }
                    if (rowData.ToLower().Contains("alternateroi"))
                    {
                        if (rowData == null)
                            continue;
                        if (rowData.Length == 0)
                            continue;
                        string[] splits = rowData.Split(';');
                        string[] valus = splits[1].Split(',');
                        AlternateROIPath = valus[4];
                    }
                }

                Mat imgMat = null;
                if (AlternateROIPath != "")
                {
                    imgMat = Cv2.ImRead(AlternateROIPath);
                }
                else
                {
                    //load current recipe image
                    imgMat = Cv2.ImRead(((App)Application.Current).GetCurrentRecipeImagePath(false));
                }
                Tuple<OpenCvSharp.Rect, double> tmpl1Result = Machine.cognexVisionDetection.FindTemplate(templateVPP1Path, imgMat, new OpenCvSharp.Rect());
                OpenCvSharp.Rect rectTmpl1 = tmpl1Result.Item1;
                double scoreTmpl1 = tmpl1Result.Item2;
                OpenCvSharp.Point topLeft1 = rectTmpl1.TopLeft;

                Tuple<OpenCvSharp.Rect, double> tmpl2Result = Machine.cognexVisionDetection.FindTemplate(templateVPP2Path, imgMat, new OpenCvSharp.Rect());
                OpenCvSharp.Rect rectTmpl2 = tmpl2Result.Item1;
                double scoreTmpl2 = tmpl2Result.Item2;
                OpenCvSharp.Point topLeft2 = rectTmpl2.TopLeft;

                int Distance = int.MinValue;

                if(lblCogPlugDistance.Content.ToString() == "PlugCogDistanceX")
                {
                    Distance = topLeft2.X - topLeft1.X;
                }
                else
                {
                    Distance = topLeft2.Y - topLeft1.Y;
                }

                txtCogPlugDistance.Text = Distance.ToString();
                
            }
#endif
        }

    }
}
