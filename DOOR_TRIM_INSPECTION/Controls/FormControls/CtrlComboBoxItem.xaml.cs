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
    /// Interaction logic for CtrlFormItem.xaml
    /// </summary>
    public partial class CtrlComboBoxItem : UserControl
    {
        public string Title {
            get { return lblItemText.Content.ToString(); }
            set { lblItemText.Content = value; }
        }

        public List<KeyValuePair<int, string>> DataSource
        {
            get
            {
                List<KeyValuePair<int, string>> items = new List<KeyValuePair<int, string>>();
                foreach (var Item in cboItemValue.Items)
                {
                    if (Item is KeyValuePair<int, string> CurrentItem)
                    {
                        items.Add(CurrentItem);
                    }
                }
                return items;
            }
            set {
                cboItemValue.ItemsSource = value;
                cboItemValue.DisplayMemberPath = "Key";
            }
        }

        public KeyValuePair<int, string> SelectedItem
        {
            get { return (KeyValuePair<int, string>)cboItemValue.SelectedItem; }
            set { cboItemValue.SelectedItem = value; }
        }

        public CtrlComboBoxItem()
        {
            InitializeComponent();
        }

        

    }
}
