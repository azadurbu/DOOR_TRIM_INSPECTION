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
    /// Interaction logic for CtrlHarwareConfig.xaml
    /// </summary>
    public partial class CtrlHarwareConfigSetup : UserControl
    {
        private Setup setup;
        public CtrlHarwareConfigSetup()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                setup = Machine.config.setup;
                if (setup.cameraProp.Count == setup.maxCamCount)
                {
                    ctlCam1Prop.LoadData(setup.cameraProp.ElementAt(0));
                    ctlCam2Prop.LoadData(setup.cameraProp.ElementAt(1));
                    ctlCam3Prop.LoadData(setup.cameraProp.ElementAt(2));
//#if USE_EXTRA_CAM
//                    ctlCam4Prop.LoadData(setup.cameraProp.ElementAt(3));
//#endif
                }
              

                Device.Light.LightProperty lightProp = setup.lightProp;
                txtLightChannel.Text = lightProp.Channel.ToString();
                txtLightLevel.Text = lightProp.Level.ToString();
                txtLightPort.Text = lightProp.Comport;

                txtBarcodePort.Text = setup.BarcodeComport;
            }
            catch (Exception ex) { Machine.logger.WriteException(eLogType.ERROR, ex); }
        }
        private void BtnSaveHWConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveHWConfig();
        }

        private void SaveHWConfig()
        {
            try
            {
                setup = Machine.config.setup;


                if (setup.cameraProp.Count == setup.maxCamCount)
                {
                    setup.cameraProp.ElementAt(0).SetProperty(ctlCam1Prop.GetData());
                    setup.cameraProp.ElementAt(1).SetProperty(ctlCam2Prop.GetData());
                    setup.cameraProp.ElementAt(2).SetProperty(ctlCam3Prop.GetData());
//#if USE_EXTRA_CAM
//                    setup.cameraProp.ElementAt(3).SetProperty(ctlCam4Prop.GetData());
//#endif
                }

                Device.Light.LightProperty lightProp = new Device.Light.LightProperty();
                lightProp.Channel = int.Parse(txtLightChannel.Text);
                lightProp.Level = int.Parse(txtLightLevel.Text);
                lightProp.Comport = txtLightPort.Text;

                setup.lightProp.SetProperty(lightProp);

                setup.BarcodeComport = txtBarcodePort.Text;

                Machine.config.SaveConfig();
                Machine.config.LoadConfig();
            }
            catch (Exception err)
            {
                Machine.logger.WriteException(eLogType.ERROR, err);
            }
        }

        private void btnLightOn_Click(object sender, RoutedEventArgs e)
        {
            Machine.Light_Comm.LightOnOffEN(true, 1, 1600);
        }

        private void btnLightOff_Click(object sender, RoutedEventArgs e)
        {
            Machine.Light_Comm.LightOnOffEN(false, 1, 0);
        }

        private void btnLightResetOn_Click(object sender, RoutedEventArgs e)
        {

            Machine.DIO_Comm.SendSetLightRest(true); 
        }

        private void btnLightResetOff_Click(object sender, RoutedEventArgs e)
        {

            Machine.DIO_Comm.SendSetLightRest(false); 
        }
    }
}
