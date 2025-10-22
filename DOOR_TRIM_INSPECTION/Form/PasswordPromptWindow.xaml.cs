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
using System.Windows.Shapes;

namespace DOOR_TRIM_INSPECTION.Form
{
    /// <summary>
    /// Interaction logic for PasswordPromptWindow.xaml
    /// </summary>
    public partial class PasswordPromptWindow : Window
    {
        public string Password { get; private set; }
        public PasswordPromptWindow()
        {
            InitializeComponent();
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            Password = PasswordBox.Password;
            this.DialogResult = true;
            Close();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Password = PasswordBox.Password;
                this.DialogResult = true;
                Close();
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            PasswordBox.Focus();
        }
    }
}
