﻿using System.Windows;
using System.Windows.Media;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "TagsPlanCheck"
    /// </summary>
    public class TagsPlanCheckViewModel : BaseViewModel
    {
        #region PROPERTIES

        private TagsPlanCheckModel _model;
        public TagsPlanCheckModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private Color _filterColor;
        public Color FilterColor
        {
            get { return _filterColor; }
            set
            {
                _filterColor = value;
                OnPropertyChanged(nameof(FilterColor));
            }
        }

        #endregion

        public TagsPlanCheckViewModel()
        {
            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region METHODS

        public override void SetInitialData()
        {
            Model = (TagsPlanCheckModel)BaseModel;

            FilterColor = new Color
            {
                R = 255,
                G = 127,
                B = 39
            };
        }

        #endregion

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model.FilterColorSystem = FilterColor;

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