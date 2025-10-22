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
    /// Interaction logic for CtrlImageSelector.xaml
    /// </summary>
    public partial class CtrlImageSelector : UserControl
    {
        private bool _isImageLoaded = false;
        public CtrlImageSelector()
        {
            InitializeComponent();
        }

        private void ToggleIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isImageLoaded)
            {
                // Remove the image
                int index = int.Parse(this.Name.Replace("imgSelector", "")) - 1;
                ClearImage(index);
                ImagePreview.Source = null;
                ImagePreview.Visibility = Visibility.Collapsed;

                // Restore the plus icon
                VerticalRect.Width = 5;
                VerticalRect.Height = 50;
                VerticalRect.RenderTransform = null;
                HorizontalRect.Width = 50;
                HorizontalRect.Height = 5;
                HorizontalRect.RenderTransform = null;

                _isImageLoaded = false;
            }
            else
            {
                Controls.CustomImagePickerBox cfBox = new Controls.CustomImagePickerBox();
                cfBox.Owner = Window.GetWindow(this);
                var result = cfBox.ShowDialog() == true;
                string filePath = result ? cfBox.SelectedFilePath : "";

                if (!string.IsNullOrEmpty(filePath))
                {
                    // Load the image
                    BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                    ImagePreview.Source = bitmap;
                    ImagePreview.Visibility = Visibility.Visible;

                    int index = int.Parse(this.Name.Replace("imgSelector", "")) - 1;
                    ((App)Application.Current).SetTrialImageFolder(index, System.IO.Path.GetDirectoryName(filePath));
                    ((App)Application.Current).SetTrialImage(index, bitmap);

                    // MEER 2025.02.11
                    OpenCvSharp.Mat matImg = OpenCvSharp.Cv2.ImRead(filePath);
                    ((App)Application.Current).SetTrialImageMat(index, matImg);

                    // Change the icon to cross
                    VerticalRect.Width = 50;
                    VerticalRect.Height = 5;
                    VerticalRect.RenderTransform = new RotateTransform(45, HorizontalRect.Width / 2, HorizontalRect.Height / 2);

                    HorizontalRect.Width = 5;
                    HorizontalRect.Height = 50;
                    HorizontalRect.RenderTransform = new RotateTransform(45, HorizontalRect.Width / 2, HorizontalRect.Height / 2);

                    _isImageLoaded = true;
                }


                // Open file dialog to select an image
                //System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
                //{
                //    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
                //};

                //if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                //{
                //    // Load the image
                //    BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                //    ImagePreview.Source = bitmap;
                //    ImagePreview.Visibility = Visibility.Visible;

                //    int index = int.Parse(this.Name.Replace("imgSelector", "")) - 1;
                //    ((App)Application.Current).SetTrialImage(index, bitmap);
                //    // Change the icon to cross
                //    VerticalRect.Width = 50;
                //    VerticalRect.Height = 5;
                //    VerticalRect.RenderTransform = new RotateTransform(45, HorizontalRect.Width / 2, HorizontalRect.Height / 2);

                //    HorizontalRect.Width = 5;
                //    HorizontalRect.Height = 50;
                //    HorizontalRect.RenderTransform = new RotateTransform(45, HorizontalRect.Width / 2, HorizontalRect.Height / 2);

                //    _isImageLoaded = true;
                //}
            }
        }

        public void ClearImage(int index)
        {
            ((App)Application.Current).ClearTrialImage(index);
            ((App)Application.Current).ClearTrialImageMat(index); // MEER 2025.02.11
            ((App)Application.Current).ClearTrialImageFolder(index);
            ImagePreview.Source = null;
            ImagePreview.Visibility = Visibility.Collapsed;

            // Restore the plus icon
            VerticalRect.Width = 5;
            VerticalRect.Height = 50;
            VerticalRect.RenderTransform = null;
            HorizontalRect.Width = 50;
            HorizontalRect.Height = 5;
            HorizontalRect.RenderTransform = null;

            _isImageLoaded = false;
        }
    }
}
