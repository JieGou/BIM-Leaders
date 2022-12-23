using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "NamesChange"
    /// </summary>
    public class NamesChangeViewModel : BaseViewModel
    {
        #region PROPERTIES

        private NamesChangeModel _model;
        public NamesChangeModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private string _substringOld;
        public string SubstringOld
        {
            get { return _substringOld; }
            set
            {
                _substringOld = value;
                OnPropertyChanged(nameof(SubstringOld));
            }
        }

        private string _substringNew;
        public string SubstringNew
        {
            get { return _substringNew; }
            set
            {
                _substringNew = value;
                OnPropertyChanged(nameof(SubstringNew));
            }
        }

        private bool _partPrefix { get; set; }
        public bool PartPrefix
        {
            get { return _partPrefix; }
            set
            {
                _partPrefix = value;
                OnPropertyChanged(nameof(PartPrefix));
            }
        }
        private bool _partCenter { get; set; }
        public bool PartCenter
        {
            get { return _partCenter; }
            set
            {
                _partCenter = value;
                OnPropertyChanged(nameof(PartCenter));
            }
        }
        private bool _partSuffix { get; set; }
        public bool PartSuffix
        {
            get { return _partSuffix; }
            set
            {
                _partSuffix = value;
                OnPropertyChanged(nameof(PartSuffix));
            }
        }

        private List<bool> _selectedCategories { get; set; }
        public List<bool> SelectedCategories
        {
            get { return _selectedCategories; }
            set
            {
                _selectedCategories = value;
                OnPropertyChanged(nameof(SelectedCategories));
            }
        }

        #endregion

        public NamesChangeViewModel()
        {
            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region METHODS

        public override void SetInitialData()
        {
            Model = (NamesChangeModel)BaseModel;

            SubstringOld = "OLD";
            SubstringNew = "NEW";
            PartPrefix = true;
            SelectedCategories = Enumerable.Repeat(false, 24).ToList();
            SelectedCategories[6] = true;
        }

        #endregion

        #region VALIDATION

        private protected override string GetValidationError(string propertyName)
        {
            string error = null;
            
            switch (propertyName)
            {
                case "SubstringOld":
                    error = ValidateSubstringOld();
                    break;
                case "SubstringNew":
                    error = ValidateSubstringNew();
                    break;
                case "SelectedCategories":
                    error = ValidateSelectedCategories();
                    break;
            }
            return error;
        }

        private string ValidateSubstringOld()
        {
            if (string.IsNullOrEmpty(SubstringOld))
                return "Input is empty";
            else
            {
                if (SubstringOld.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string ValidateSubstringNew()
        {
            if (string.IsNullOrEmpty(SubstringNew))
                return "Input is empty";
            else
            {
                if (SubstringNew.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string ValidateSelectedCategories()
        {
            if (!SelectedCategories.Contains(true))
                return "Categories not checked";
            return null;
        }

        #endregion

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model.PartPrefix = PartPrefix;
            Model.PartSuffix = PartSuffix;
            Model.SelectedCategories = SelectedCategories;
            Model.SubstringOld = SubstringOld;
            Model.SubstringNew = SubstringNew;

            Model.Run();

            CloseAction(window);
        }

        private protected override void CloseAction(Window window)
        {
            if (window != null)
            {
                Closed = true;
                window.Close();
            }
        }

        #endregion
    }
}