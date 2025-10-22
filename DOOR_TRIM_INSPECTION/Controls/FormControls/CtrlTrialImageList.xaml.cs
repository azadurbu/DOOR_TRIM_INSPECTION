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
    /// Interaction logic for CtrlTrialImageList.xaml
    /// </summary>
    public partial class CtrlTrialImageList : UserControl
    {
        public CtrlTrialImageList()
        {
            InitializeComponent();
        }

        public void ClearImages()
        {
            imgSelector1.ClearImage(0);
            imgSelector2.ClearImage(1);
            imgSelector3.ClearImage(2);
            imgSelector4.ClearImage(3);
        }
    }
}
