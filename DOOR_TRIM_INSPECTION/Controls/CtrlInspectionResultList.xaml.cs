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
    /// Interaction logic for CtrlInspectionResultList.xaml
    /// </summary>
    public partial class CtrlInspectionResultList : UserControl
    {
        public CtrlInspectionResultList()
        {
            InitializeComponent();

            SetFrontInspectionItems();
            SetPlugInspectionItems();
            SetPadInspectionItems();
            SetScrewInspectionItems();
            SetStopperInspectionItems();
            SetBoltInspectionItems();
        }


        public void SetFrontInspectionItems()
        {
            CtrlInspectionResultItem item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpFrontInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpFrontInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpFrontInspectionResult.Children.Add(item);

        }

        public void SetPlugInspectionItems()
        {
            CtrlInspectionResultItem item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpPlugInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpPlugInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpPlugInspectionResult.Children.Add(item);
        }

        public void SetBoltInspectionItems()
        {
            CtrlInspectionResultItem item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpBoltInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpBoltInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpBoltInspectionResult.Children.Add(item);
        }

        public void SetPadInspectionItems()
        {
            CtrlInspectionResultItem item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpPadInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpPadInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpPadInspectionResult.Children.Add(item);
        }

        public void SetScrewInspectionItems()
        {
            CtrlInspectionResultItem item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpScrewInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpScrewInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpScrewInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpScrewInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpScrewInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpScrewInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpScrewInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpScrewInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpScrewInspectionResult.Children.Add(item);

        }

        public void SetStopperInspectionItems()
        {
            CtrlInspectionResultItem item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);
            item = new CtrlInspectionResultItem("Upper Trim", "Color", "OK");
            stpStopperInspectionResult.Children.Add(item);


        }
    }
}