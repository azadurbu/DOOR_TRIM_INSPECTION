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

namespace DOOR_TRIM_INSPECTION.Controls
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string message)
        {
            InitializeComponent();
            //MessageText.Text = message;
        }

        public static bool Show(string message, Window that)
        {
            var dialog = new CustomMessageBox(message);
            dialog.Owner = that;
            return dialog.ShowDialog() == true;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // Closes the window with 'Yes'
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Closes the window with 'No'
        }
    }
}
