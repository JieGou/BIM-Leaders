using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "ListsCreate"
    /// </summary>
    public class ListsCreateViewModel : BaseViewModel
    {
        #region PROPERTIES

        private ListsCreateModel _model;
        public ListsCreateModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private SortedDictionary<string, int> _viewTypeListSection;
        public SortedDictionary<string, int> ViewTypeListSection
        {
            get { return _viewTypeListSection; }
            set
            {
                _viewTypeListSection = value;
                OnPropertyChanged(nameof(ViewTypeListSection));
            }
        }

        private int _viewTypeListSectionSelected;
        public int ViewTypeListSectionSelected
        {
            get { return _viewTypeListSectionSelected; }
            set
            {
                _viewTypeListSectionSelected = value;
                OnPropertyChanged(nameof(ViewTypeListSectionSelected));
            }
        }

        private SortedDictionary<string, int> _viewTemplateListSection;
        public SortedDictionary<string, int> ViewTemplateListSection
        {
            get { return _viewTemplateListSection; }
            set
            {
                _viewTemplateListSection = value;
                OnPropertyChanged(nameof(ViewTemplateListSection));
            }
        }

        private int _viewTemplateListSectionSelected;
        public int ViewTemplateListSectionSelected
        {
            get { return _viewTemplateListSectionSelected; }
            set
            {
                _viewTemplateListSectionSelected = value;
                OnPropertyChanged(nameof(ViewTemplateListSectionSelected));
            }
        }

        private SortedDictionary<string, int> _viewTypeListPlan;
        public SortedDictionary<string, int> ViewTypeListPlan
        {
            get { return _viewTypeListPlan; }
            set
            {
                _viewTypeListPlan = value;
                OnPropertyChanged(nameof(ViewTypeListPlan));
            }
        }

        private int _viewTypeListPlanSelected;
        public int ViewTypeListPlanSelected
        {
            get { return _viewTypeListPlanSelected; }
            set
            {
                _viewTypeListPlanSelected = value;
                OnPropertyChanged(nameof(ViewTypeListPlanSelected));
            }
        }

        private SortedDictionary<string, int> _viewTemplateListPlan;
        public SortedDictionary<string, int> ViewTemplateListPlan
        {
            get { return _viewTemplateListPlan; }
            set
            {
                _viewTemplateListPlan = value;
                OnPropertyChanged(nameof(ViewTemplateListPlan));
            }
        }

        private int _viewTemplateListPlanSelected;
        public int ViewTemplateListPlanSelected
        {
            get { return _viewTemplateListPlanSelected; }
            set
            {
                _viewTemplateListPlanSelected = value;
                OnPropertyChanged(nameof(ViewTemplateListPlanSelected));
            }
        }

        private string _viewNamePrefix;
        public string ViewNamePrefix
        {
            get { return _viewNamePrefix; }
            set
            {
                _viewNamePrefix = value;
                OnPropertyChanged(nameof(ViewNamePrefix));
            }
        }

        private bool _sortIsNeeded;
        public bool SortIsNeeded
        {
            get { return _sortIsNeeded; }
            set
            {
                _sortIsNeeded = value;
                OnPropertyChanged(nameof(SortIsNeeded));
            }
        }

        private bool _createAluminium;
        public bool CreateAluminium
        {
            get { return _createAluminium; }
            set
            {
                _createAluminium = value;
                OnPropertyChanged(nameof(CreateAluminium));
                OnPropertyChanged(nameof(CreateMetal));
                OnPropertyChanged(nameof(CreateCarpentry));
            }
        }

        private string _typeCommentsAluminium;
        public string TypeCommentsAluminium
        {
            get { return _typeCommentsAluminium; }
            set
            {
                _typeCommentsAluminium = value;
                OnPropertyChanged(nameof(TypeCommentsAluminium));
            }
        }

        private SortedDictionary<string, int> _tagGenAluminiumList;
        public SortedDictionary<string, int> TagGenAluminiumList
        {
            get { return _tagGenAluminiumList; }
            set
            {
                _tagGenAluminiumList = value;
                OnPropertyChanged(nameof(TagGenAluminiumList));
            }
        }

        private int _tagGenAluminiumListSelected;
        public int TagGenAluminiumListSelected
        {
            get { return _tagGenAluminiumListSelected; }
            set
            {
                _tagGenAluminiumListSelected = value;
                OnPropertyChanged(nameof(TagGenAluminiumListSelected));
            }
        }

        private string _typeNamePrefixAluminiumWalls;
        public string TypeNamePrefixAluminiumWalls
        {
            get { return _typeNamePrefixAluminiumWalls; }
            set
            {
                _typeNamePrefixAluminiumWalls = value;
                OnPropertyChanged(nameof(TypeNamePrefixAluminiumWalls));
            }
        }

        private bool _createMetal;
        public bool CreateMetal
        {
            get { return _createMetal; }
            set
            {
                _createMetal = value;
                OnPropertyChanged(nameof(CreateAluminium));
                OnPropertyChanged(nameof(CreateMetal));
                OnPropertyChanged(nameof(CreateCarpentry));
            }
        }

        private string _typeCommentsMetal;
        public string TypeCommentsMetal
        {
            get { return _typeCommentsMetal; }
            set
            {
                _typeCommentsMetal = value;
                OnPropertyChanged(nameof(TypeCommentsMetal));
            }
        }

        private SortedDictionary<string, int> _tagGenMetalList;
        public SortedDictionary<string, int> TagGenMetalList
        {
            get { return _tagGenMetalList; }
            set
            {
                _tagGenMetalList = value;
                OnPropertyChanged(nameof(TagGenMetalList));
            }
        }

        private int _tagGenMetalListSelected;
        public int TagGenMetalListSelected
        {
            get { return _tagGenMetalListSelected; }
            set
            {
                _tagGenMetalListSelected = value;
                OnPropertyChanged(nameof(TagGenMetalListSelected));
            }
        }

        private bool _createCarpentry;
        public bool CreateCarpentry
        {
            get { return _createCarpentry; }
            set
            {
                _createCarpentry = value;
                OnPropertyChanged(nameof(CreateAluminium));
                OnPropertyChanged(nameof(CreateMetal));
                OnPropertyChanged(nameof(CreateCarpentry));
            }
        }

        private string _typeCommentsCarpentry;
        public string TypeCommentsCarpentry
        {
            get { return _typeCommentsCarpentry; }
            set
            {
                _typeCommentsCarpentry = value;
                OnPropertyChanged(nameof(TypeCommentsCarpentry));
            }
        }

        private SortedDictionary<string, int> _tagGenCarpentryList;
        public SortedDictionary<string, int> TagGenCarpentryList
        {
            get { return _tagGenCarpentryList; }
            set
            {
                _tagGenCarpentryList = value;
                OnPropertyChanged(nameof(TagGenCarpentryList));
            }
        }

        private int _tagGenCarpentryListSelected;
        public int TagGenCarpentryListSelected
        {
            get { return _tagGenCarpentryListSelected; }
            set
            {
                _tagGenCarpentryListSelected = value;
                OnPropertyChanged(nameof(TagGenCarpentryListSelected));
            }
        }

        private SortedDictionary<string, int> _titleBlockList;
        public SortedDictionary<string, int> TitleBlockList
        {
            get { return _titleBlockList; }
            set
            {
                _titleBlockList = value;
                OnPropertyChanged(nameof(TitleBlockList));
            }
        }

        private int _titleBlockListSelected;
        public int TitleBlockListSelected
        {
            get { return _titleBlockListSelected; }
            set
            {
                _titleBlockListSelected = value;
                OnPropertyChanged(nameof(TitleBlockListSelected));
            }
        }

        private double _tagPlacementOffsetX;
        public double TagPlacementOffsetX
        {
            get { return _tagPlacementOffsetX; }
            set
            {
                _tagPlacementOffsetX = value;
                OnPropertyChanged(nameof(TagPlacementOffsetX));
            }
        }

        private string _tagPlacementOffsetXstring;
        public string TagPlacementOffsetXstring
        {
            get { return _tagPlacementOffsetXstring; }
            set
            {
                _tagPlacementOffsetXstring = value;
                OnPropertyChanged(nameof(TagPlacementOffsetXstring));
            }
        }

        private double _tagPlacementOffsetY;
        public double TagPlacementOffsetY
        {
            get { return _tagPlacementOffsetY; }
            set
            {
                _tagPlacementOffsetY = value;
                OnPropertyChanged(nameof(TagPlacementOffsetY));
            }
        }

        private string _tagPlacementOffsetYstring;
        public string TagPlacementOffsetYstring
        {
            get { return _tagPlacementOffsetYstring; }
            set
            {
                _tagPlacementOffsetYstring = value;
                OnPropertyChanged(nameof(TagPlacementOffsetYstring));
            }
        }

        #endregion

        public ListsCreateViewModel()
        {
            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region METHODS

        public override void SetInitialData()
        {
            Model = (ListsCreateModel)BaseModel;

            ViewTypeListSection = Model.GetViewTypeListSection();
            ViewTypeListSectionSelected = ViewTypeListSection.First().Value;
            ViewTemplateListSection = Model.GetViewTemplateListSection();
            ViewTemplateListSectionSelected = ViewTemplateListSection.First().Value;
            ViewTypeListPlan = Model.GetViewTypeListPlan();
            ViewTypeListPlanSelected = ViewTypeListPlan.First().Value;
            ViewTemplateListPlan = Model.GetViewTemplateListPlan();
            ViewTemplateListPlanSelected = ViewTemplateListPlan.First().Value;
            ViewNamePrefix = "LIST_";
            SortIsNeeded = true;

            CreateAluminium = true;
            TypeCommentsAluminium = "A";
            TagGenAluminiumList = Model.GetTagGenList();
            TagGenAluminiumListSelected = TagGenAluminiumList.First().Value;
            TypeNamePrefixAluminiumWalls = "Aluminium Walls";

            CreateMetal = true;
            TypeCommentsMetal = "M";
            TagGenMetalList = Model.GetTagGenList();
            TagGenMetalListSelected = TagGenMetalList.First().Value;

            CreateCarpentry = true;
            TypeCommentsCarpentry = "C";
            TagGenCarpentryList = Model.GetTagGenList();
            TagGenCarpentryListSelected = TagGenCarpentryList.First().Value;

            TitleBlockList = Model.GetTitleBlockList();
            TitleBlockListSelected = TitleBlockList.First().Value;
            TagPlacementOffsetX = 177.5;
            TagPlacementOffsetXstring = TagPlacementOffsetX.ToString();
            TagPlacementOffsetY = 248.7;
            TagPlacementOffsetYstring = TagPlacementOffsetY.ToString();
        }

        #endregion

        #region VALIDATION

        private protected override string GetValidationError(string propertyName)
        {
            string error = null;

            switch (propertyName)
            {
                case "ViewNamePrefix":
                    error = ValidateString(ViewNamePrefix);
                    break;
                case "TagPlacementOffsetXstring":
                    error = ValidateNumber(out double tagPlacementOffsetX, TagPlacementOffsetXstring);
                    if (string.IsNullOrEmpty(error))
                    {
                        TagPlacementOffsetX = tagPlacementOffsetX;
                    }
                    break;
                case "TagPlacementOffsetYstring":
                    error = ValidateNumber(out double tagPlacementOffsetY, TagPlacementOffsetYstring);
                    if (string.IsNullOrEmpty(error))
                    {
                        TagPlacementOffsetY = tagPlacementOffsetY;
                    }
                    break;
                case "CreateAluminium":
                    error = ValidateCheckboxes();
                    break;
                case "CreateMetal":
                    error = ValidateCheckboxes();
                    break;
                case "CreateCarpentry":
                    error = ValidateCheckboxes();
                    break;
                case "TypeCommentsAluminium":
                    error = ValidateString(TypeCommentsAluminium);
                    break;
                case "TypeCommentsMetal":
                    error = ValidateString(TypeCommentsMetal);
                    break;
                case "TypeCommentsCarpentry":
                    error = ValidateString(TypeCommentsCarpentry);
                    break;
            }
            return error;
        }

        private string ValidateString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "Input is empty";
            return null;
        }

        private string ValidateNumber(out double numberParsed, string number)
        {
            numberParsed = 0;

            if (string.IsNullOrEmpty(number))
                return "Input is empty";
            if (!double.TryParse(number, out numberParsed))
                return "Wrong input";

            return null;
        }

        private string ValidateCheckboxes()
        {
            if (CreateAluminium == false &&
                CreateMetal == false &&
                CreateCarpentry == false)
                return "Check at least one check";
            return null;
        }

        #endregion

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model.SelectedViewTypeSection = ViewTypeListSectionSelected;
            Model.SelectedViewTemplateSection = ViewTemplateListSectionSelected;
            Model.SelectedViewTypePlan = ViewTypeListPlanSelected;
            Model.SelectedViewTemplatePlan = ViewTemplateListPlanSelected;
            Model.ViewNamePrefix = ViewNamePrefix;
            Model.SortIsNeeded = SortIsNeeded;

            Model.CreateAluminium = CreateAluminium;
            Model.TypeCommentsAluminium = TypeCommentsAluminium;
            Model.SelectedTagGenAluminium = TagGenAluminiumListSelected;
            Model.TypeNamePrefixAluminiumWalls = TypeNamePrefixAluminiumWalls;

            Model.CreateMetal = CreateMetal;
            Model.TypeCommentsMetal = TypeCommentsMetal;
            Model.SelectedTagGenMetal = TagGenMetalListSelected;

            Model.CreateCarpentry = CreateCarpentry;
            Model.TypeCommentsCarpentry = TypeCommentsCarpentry;
            Model.SelectedTagGenCarpentry = TagGenCarpentryListSelected;

            Model.SelectedTitleBlock = TitleBlockListSelected;
            Model.TagPlacementOffsetXmm = TagPlacementOffsetX;
            Model.TagPlacementOffsetYmm = TagPlacementOffsetY;

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