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
using DOOR_TRIM_INSPECTION.Device.Camera;
using OpenCvSharp;

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CtrlCameraProperty.xaml
    /// </summary>
    public partial class CtrlCameraProperty : UserControl
    {
        public CtrlCameraProperty()
        {
            InitializeComponent();
        }

        public void LoadData(CameraProperty camProp)
        {
            txtCameraName.Text = camProp.CamName;
            txtCamIPAddress.Text = camProp.CamAddress;
            txtCamSerialNum.Text = camProp.SerialNumber;
            txtCamExposure.Text = camProp.Exposure.ToString();
            txtOffsetX.Text = camProp.OffsetX.ToString();
            txtOffsetY.Text = camProp.OffsetY.ToString();
            txtWidth.Text = camProp.Width.ToString();
            txtHeight.Text = camProp.Height.ToString();
            txtFrameRate.Text = camProp.frameRate.ToString();
            txtGain.Text = camProp.gain.ToString();
        }

        public CameraProperty GetData()
        {
            CameraProperty camProp = new CameraProperty();

            camProp.CamName = txtCameraName.Text;
            camProp.CamAddress = txtCamIPAddress.Text;
            camProp.SerialNumber = txtCamSerialNum.Text;
            camProp.Exposure = int.Parse(txtCamExposure.Text);
            camProp.OffsetX = int.Parse(txtOffsetX.Text);
            camProp.OffsetY = int.Parse(txtOffsetY.Text);
            camProp.Width = int.Parse(txtWidth.Text);
            camProp.Height = int.Parse(txtHeight.Text);
            camProp.frameRate = int.Parse(txtFrameRate.Text);
            camProp.gain = int.Parse(txtGain.Text);
            return camProp;
        }
    }
}
