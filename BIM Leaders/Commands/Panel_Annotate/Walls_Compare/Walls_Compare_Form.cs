using System.Windows.Forms;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Compare walls aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form"/>
    public partial class Walls_Compare_Form : System.Windows.Forms.Form
    {
        /// <summary>
        /// The private reference to the <see cref="UIDocument"/>
        /// </summary>
        private UIDocument uidoc = null;

        /// <summary>
        /// The private fill type Id
        /// </summary>
        private ElementId fill_id = null;

        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="Walls_Compare_Form"/>
        /// </summary>
        /// <param name="uIDocument"></param>
        public Walls_Compare_Form(UIDocument uIDocument)
        {
            uidoc = uIDocument;

            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the Button_delete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Button_delete_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        /// <summary>
        /// Handles the Click event of the Button_exit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Button_exit_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Handles the Load event of the DWG_Name_Delete_Form control.
        /// </summary>
        /// /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Walls_Compare_Form_Load(object sender, System.EventArgs e)
        {
            Create_Fill_List();
        }

        /// <summary>
        /// Handles the SelectedItemChanged event of the Walls_Compare_Form_List_Fills control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Walls_Compare_Form_List_Fills_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            fill_id = ((KeyValuePair<string, ElementId>)Walls_Compare_Form_List_Fills.SelectedItem).Value;
        }

        System.Drawing.Point last_point;
        private void Walls_Compare_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void Walls_Compare_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
        }

        /// <summary>
        /// Gets the information from user.
        /// </summary>
        /// <returns></returns>
        public Walls_Compare_Data GetInformation()
        {
            // Information gathered from window
            var information = new Walls_Compare_Data()
            {
                result_mat = textBox1.Text.ToString(),
                result_fill_id = fill_id
            };
            return information;
        }

        /// <summary>
        /// Populates the fill types list.
        /// </summary>
        private void Create_Fill_List()
        {
            var doc = uidoc.Document;

            // Get Fills
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<FilledRegionType> fill_types_all = collector.OfClass(typeof(FilledRegionType))
                .Cast<FilledRegionType>(); //LINQ function;

            // Get unique fills names list
            List<FilledRegionType> fill_types = new List<FilledRegionType>();
            List<string> fill_types_names = new List<string>();
            foreach (FilledRegionType i in fill_types_all)
            {
                string fill_type_name = i.Name;
                if (!fill_types_names.Contains(fill_type_name))
                {
                    fill_types.Add(i);
                    fill_types_names.Add(fill_type_name);
                }
            }

            var list = new List<KeyValuePair<string, ElementId>>();

            foreach (var i in fill_types)
            {
                list.Add(new KeyValuePair<string, ElementId>(i.Name, i.Id));
            }

            Walls_Compare_Form_List_Fills.DataSource = null;
            Walls_Compare_Form_List_Fills.DataSource = new BindingSource(list, null);
            Walls_Compare_Form_List_Fills.DisplayMember = "Key";
            Walls_Compare_Form_List_Fills.ValueMember = "Value";
        }

        private void textBox1_TextChanged(object sender, System.EventArgs e)
        {
            button_compare.Enabled = !string.IsNullOrWhiteSpace(textBox1.Text);
        }
        
        string result = "";
        public string Result_Material()
        {
            string result_mat = textBox1.Text.ToString();
            return result_mat; 
        }
        public string Result_Fill()
        {
            string result_fill = Walls_Compare_Form_List_Fills.SelectedItem.ToString();
            return result_fill;
        }
    }
}
