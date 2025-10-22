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
using System.Windows.Shapes;

namespace DOOR_TRIM_INSPECTION
{
    /// <summary>
    /// Interaction logic for WinInWin.xaml
    /// </summary>
    public partial class WinInWin : Window
    {
        public WinInWin()
        {
            InitializeComponent();
        }

        // Open the floating panel when the button is clicked
        private void OpenFloatingPanel(object sender, RoutedEventArgs e)
        {
            FloatingPanel.Show();
        }

        // Close the floating panel when the user clicks the close button (example in your UI)
        private void CloseFloatingPanel(object sender, RoutedEventArgs e)
        {
            FloatingPanel.Hide();
        }
    }
}
