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
    /// Interaction logic for CtrlInspectionResultItem.xaml
    /// </summary>
    public partial class CtrlInspectionResultItem : UserControl
    {
        public CtrlInspectionResultItem(string Name, string Type, string InspectionResult)
        {
            InitializeComponent();
            txtName.Text = Name;
            txtType.Text = Type;
            txtInspResult.Text = InspectionResult;

            if (InspectionResult == "OK")
            {
                txtInspResult.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
                txtInspResult.Background = new SolidColorBrush(Colors.MediumSeaGreen);
            }
            else if (InspectionResult == "NG")
            {
                txtInspResult.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
                txtInspResult.Background = new SolidColorBrush(Colors.Crimson);
            }
            else // COLOR ITEM
            {
                txtInspResult.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
                txtInspResult.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(txtInspResult.Text));
            }
        }

    }
}
