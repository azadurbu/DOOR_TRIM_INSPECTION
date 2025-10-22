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
    public partial class CtrlListBoxItem : UserControl
    {
        public string Title {
            get { return lblItemText.Content.ToString(); }
            set { lblItemText.Content = value; }
        }

        public List<KeyValuePair<int, string>> Value
        {
            get
            {
                List<KeyValuePair<int, string>> items = new List<KeyValuePair<int, string>>();
                foreach (var Item in lstItemValue.Items)
                {
                    if (Item is KeyValuePair<int, string> CurrentItem)
                    {
                        items.Add(CurrentItem);
                    }
                }
                return items;
            }
            set
            {
                lstItemValue.ItemsSource = value;
                lstItemValue.DisplayMemberPath = "Key";
            }
        }

        public List<KeyValuePair<int, string>> SelectedItems
        {
            get { 
                List<KeyValuePair<int, string>> items = new List<KeyValuePair<int, string>>();
                foreach (var Item in lstItemValue.SelectedItems)
                {
                    if (Item is KeyValuePair<int, string> CurrentItem)
                    {
                        items.Add(CurrentItem);
                    }
                }
                return items;
            }
            set {
                foreach (var Item in (List<KeyValuePair<int, string>>)value)
                {
                    if (Item is KeyValuePair<int, string> CurrentItem)
                    {
                        lstItemValue.SelectedItems.Add(CurrentItem);
                    }
                }
            }
        }

        
        public CtrlListBoxItem()
        {
            InitializeComponent();
        }

        

    }
}
