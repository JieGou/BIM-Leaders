using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "WallsCompare"
    /// </summary>
    public class WallsCompareViewModel : BaseViewModel
    {
        #region PROPERTIES

        private WallsCompareModel _model;
        public WallsCompareModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private bool _checkOneLink;
        public bool CheckOneLink
        {
            get { return _checkOneLink; }
            set
            {
                _checkOneLink = value;
                OnPropertyChanged(nameof(CheckOneLink));
            }
        }

        private SortedDictionary<string, int> _materials;
        public SortedDictionary<string, int> Materials
        {
            get { return _materials; }
            set { _materials = value; }
        }

        private SortedDictionary<string, int> _fillTypes;
        public SortedDictionary<string, int> FillTypes
        {
            get { return _fillTypes; }
            set { _fillTypes = value; }
        }

        private int _materialsSelected;
        public int MaterialsSelected
        {
            get { return _materialsSelected; }
            set
            {
                _materialsSelected = value;
                OnPropertyChanged(nameof(MaterialsSelected));
            }
        }

        private int _fillTypesSelected;
        public int FillTypesSelected
        {
            get { return _fillTypesSelected; }
            set
            {
                _fillTypesSelected = value;
                OnPropertyChanged(nameof(FillTypesSelected));
            }
        }

        #endregion

        public WallsCompareViewModel()
        {
            Materials = Model.MaterialsList;
            FillTypes = Model.FillTypesList;
            MaterialsSelected = Materials.First().Value;
            FillTypesSelected = FillTypes.First().Value;

            CheckOneLink = true;

            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model = (WallsCompareModel)BaseModel;

            Model.CheckOneLink = CheckOneLink;
            Model.MaterialsSelected = MaterialsSelected;
            Model.FillTypesSelected = FillTypesSelected;

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