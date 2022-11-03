using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class NamesChangeM : INotifyPropertyChanged, IExternalEventHandler
    {
        private UIDocument _uidoc;
        private Document _doc;
        private static int _countNamesChanged;

        private const string TRANSACTION_NAME = "Change Names";

        #region PROPERTIES

        /// <summary>
        /// ExternalEvent needed for Revit to run transaction in API context.
        /// So we must call not the main method but raise the event.
        /// </summary>
        public ExternalEvent ExternalEvent { get; set; }

        private List<bool> _selectedCategories;
        public List<bool> SelectedCategories
        {
            get { return _selectedCategories; }
            set
            {
                _selectedCategories = value;
                OnPropertyChanged(nameof(SelectedCategories));
            }
        }

        private bool _partPrefix;
        public bool PartPrefix
        {
            get { return _partPrefix; }
            set
            {
                _partPrefix = value;
                OnPropertyChanged(nameof(PartPrefix));
            }
        }

        private bool _partSuffix;
        public bool PartSuffix
        {
            get { return _partSuffix; }
            set
            {
                _partSuffix = value;
                OnPropertyChanged(nameof(PartSuffix));
            }
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

        private string _runResult;
        public string RunResult
        {
            get { return _runResult; }
            set
            {
                _runResult = value;
                OnPropertyChanged(nameof(RunResult));
            }
        }

        #endregion

        public NamesChangeM(ExternalCommandData commandData)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
        }

        public void Run()
        {
            ExternalEvent.Raise();
        }

        #region IEXTERNALEVENTHANDLER

        public string GetName()
        {
            return TRANSACTION_NAME;
        }

        public void Execute(UIApplication app)
        {
            RunResult = "";

            try
            {
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    ReplaceNames();

                    trans.Commit();
                }

                GetRunResult();
            }
            catch (Exception e)
            {
                RunResult = e.Message;
            }
        }

        #endregion

        #region METHODS

        private void ReplaceNames()
        {
            List<Type> types = Categories.GetTypesList(SelectedCategories);

            ElementMulticlassFilter elementMulticlassFilter = new ElementMulticlassFilter(types);

            IEnumerable<Element> elements = new FilteredElementCollector(_doc)
                .WherePasses(elementMulticlassFilter)
                .ToElements();

            // Prefix location replacement
            if (PartPrefix)
                ReplaceNamesPrefix(elements);
            else if (PartSuffix)
                ReplaceNamesSuffix(elements);
            else
                ReplaceNamesCenter(elements);
        }

        /// <summary>
        /// Replace substring in names of given elements.
        /// </summary>
        private void ReplaceNamesPrefix(IEnumerable<Element> elements)
        {
            foreach (Element element in elements)
            {
                string name = element.Name;
                if (name.StartsWith(SubstringOld))
                {
                    string nameNew = name.TrimStart(SubstringOld.ToCharArray());
                    nameNew = SubstringNew += nameNew;
                    element.Name = nameNew;

                    _countNamesChanged++;
                }
            }
        }

        /// <summary>
        /// Replace substring in names of given elements.
        /// </summary>
        private void ReplaceNamesSuffix(IEnumerable<Element> elements)
        {
            foreach (Element element in elements)
            {
                string name = element.Name;
                if (name.EndsWith(SubstringOld))
                {
                    string nameNew = name.TrimEnd(SubstringOld.ToCharArray());
                    nameNew += SubstringNew;
                    element.Name = nameNew;

                    _countNamesChanged++;
                }
            }
        }

        /// <summary>
        /// Replace substring in names of given elements.
        /// </summary>
        private void ReplaceNamesCenter(IEnumerable<Element> elements)
        {
            foreach (Element element in elements)
            {
                string name = element.Name;
                if (name.Contains(SubstringOld))
                {
                    string nameNew = name.Replace(SubstringOld, SubstringNew);
                    element.Name = nameNew;

                    _countNamesChanged++;
                }
            }
        }

        private void GetRunResult()
        {
            RunResult = (_countNamesChanged == 0)
                ? "No names changed"
                : $"{_countNamesChanged} names changed";
        }

        #endregion

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}