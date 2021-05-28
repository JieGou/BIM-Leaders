using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// DWG delete aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form"/>
    public partial class DWG_Name_Delete_Form : System.Windows.Forms.Form
    {
        /// <summary>
        /// The private reference to the <see cref="UIDocument"/>
        /// </summary>
        private UIDocument uidoc = null;

        /// <summary>
        /// The private DWG Id
        /// </summary>
        private ElementId dwg_id = null;

        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="DWG_Name_Delete_Form"/>
        /// </summary>
        /// <param name="uIDocumnet">The UI document.</param>
        public DWG_Name_Delete_Form(UIDocument uIDocument)
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
        private void DWG_Name_Delete_Form_Load(object sender, System.EventArgs e)
        {
            Create_DWG_List();
        }

        /// <summary>
        /// Handles the SelectedItemChanged event of the DWG_Name_Delete_List control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DWG_Name_Delete_List_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            dwg_id = ((KeyValuePair<string, ElementId>)DWG_Name_Delete_List.SelectedItem).Value;
        }

        System.Drawing.Point last_point;
        private void DWG_Name_Delete_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void DWG_Name_Delete_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
        }

        /// <summary>
        /// Gets the information from user.
        /// </summary>
        /// <returns></returns>
        public DWG_Name_Delete_Data GetInformation()
        {
            // Information gathered from window
            var information = new DWG_Name_Delete_Data()
            {
                result_dwg = dwg_id
            };
            return information;
        }

        private void Create_DWG_List()
        {
            var doc = uidoc.Document;

            // Get Imports
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<ImportInstance> dwg_types_all = collector.OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType()
                .Cast<ImportInstance>(); //LINQ function;

            // Get unique imports names list
            List<ImportInstance> dwg_types = new List<ImportInstance>();
            List<string> dwg_types_names = new List<string>();
            foreach (ImportInstance i in dwg_types_all)
            {
                string dwg_type_name = i.Category.Name;
                if (!dwg_types_names.Contains(dwg_type_name))
                {
                    dwg_types.Add(i);
                    dwg_types_names.Add(dwg_type_name);
                }
            }

            var list = new List<KeyValuePair<string, ElementId>>();

            foreach (var i in dwg_types)
            {
                list.Add(new KeyValuePair<string, ElementId>(i.Name, i.Id));
            }

            DWG_Name_Delete_List.DataSource = null;
            DWG_Name_Delete_List.DataSource = new BindingSource(list, null);
            DWG_Name_Delete_List.DisplayMember = "Key";
            DWG_Name_Delete_List.ValueMember = "Value";
        }

        string result = "";
        public string Result()
        {
            string result = DWG_Name_Delete_List.SelectedItem.ToString();
            return result; 
        }
    }
}
