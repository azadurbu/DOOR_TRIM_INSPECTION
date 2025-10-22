using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CtrlCommStatus.xaml
    /// </summary>
    public partial class CtrlCommStatus : UserControl
    {
        public CtrlCommStatus()
        {
            InitializeComponent();
        }
        bool isCam1Comm, isCam2Comm, isCam3Comm, isPLCComm, isLightComm;
        public void UpdateStatus(bool isCam1Comm, bool isCam2Comm, bool isCam3Comm, bool isPLCComm, bool isLightComm)
        {
            this.isCam1Comm = isCam1Comm;
            this.isCam2Comm = isCam2Comm;
            this.isCam3Comm = isCam3Comm;
            this.isPLCComm = isPLCComm;
            this.isLightComm = isLightComm;
        }

        public void SetCam1Comm(bool[] isCamComm)
        {
            try
            {
                this.isCam1Comm = isCamComm[0];
                this.isCam2Comm = isCamComm[1];
                this.isCam3Comm = isCamComm[2];
            }
            catch (Exception e) { }
        }
        public void SetPLCComm(bool isPLCComm)
        {
            this.isPLCComm = isPLCComm;
        }
        public void SetLightComm(bool isLightComm)
        {
            this.isLightComm = isLightComm;
        }

        public void UpdateStatus()
        {
            Animate(CircleCam1, isCam1Comm);
            Animate(CircleCam2, isCam2Comm);
            Animate(CircleCam3, isCam3Comm);
            Animate(CirclePLC, isPLCComm);
            Animate(CircleLight, isLightComm);
            lblDateTime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void Animate(Ellipse circle, bool isOk)
        {
            if (isOk)
                circle.Fill = new RadialGradientBrush(Colors.ForestGreen, Colors.SeaGreen);
            else
                circle.Fill = new RadialGradientBrush(Color.FromRgb(245, 114, 121), Colors.Crimson);
            //var storyboard = (Storyboard)this.Resources[$"ResizingStoryboard{circle.Name}"];
            //storyboard.Begin(this);
        }

        private void StopAnimation(Ellipse circle)
        {
            circle.Fill = new RadialGradientBrush(Color.FromRgb(245, 114, 121), Colors.Crimson);
            //var storyboard = (Storyboard)this.Resources[$"ResizingStoryboard{circle.Name}"];
            //storyboard?.Stop(this);

            //circle.Width = 15;
            //circle.Height = 15;
        }

    }
}
