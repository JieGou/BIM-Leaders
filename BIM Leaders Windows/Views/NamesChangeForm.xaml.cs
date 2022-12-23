using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace BIM_Leaders_Windows
{
    public partial class NamesChangeForm : BaseView
    {
        public NamesChangeForm()
        {
            InitializeMaterialDesign();
            InitializeComponent();
        }

        // Select all categories if "Select all" label was clicked.
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<CheckBox> checkBoxList = new ObservableCollection<CheckBox>(){
                checkBox00, checkBox01, checkBox02, checkBox03, checkBox04, checkBox05, checkBox06, checkBox07,
                checkBox08, checkBox09, checkBox10, checkBox11, checkBox12, checkBox13, checkBox14, checkBox15,
                checkBox16, checkBox17, checkBox18, checkBox19, checkBox20, checkBox21, checkBox22, checkBox23,
            };

            foreach (CheckBox c in checkBoxList)
            {
                c.IsChecked = true;
            }
        }
    }
}