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
    /// Interaction logic for InspectionSummary.xaml
    /// </summary>
    public partial class CtrlInspectionSummary : UserControl
    {
        public CtrlInspectionSummary()
        {
            InitializeComponent();
            ClearInspectionSummaryResult();
        }

        public void ClearInspectionSummaryResult()
        {
            lblScrewNum.Content = "-";
            lblPadNum.Content = "-";
            lblSpeakerNum.Content = "-";
            lblLeadWireNum.Content = "-";
            lblFastenerNum.Content = "-";
            lblFusionNum.Content = "-";

        }
        public void SetInspectionSummaryResult(InspectionSummary inspSummary)
        {
            lblScrewNum.Content = inspSummary.ScrewInfo;
            lblPadNum.Content = inspSummary.PadInfo;
            lblSpeakerNum.Content = inspSummary.SpeakerInfo;
            lblLeadWireNum.Content = inspSummary.LeadwireInfo;
            lblFastenerNum.Content = inspSummary.FastenerInfo;
            lblFusionNum.Content = inspSummary.FusionInfo;
            
        }

    }
}
