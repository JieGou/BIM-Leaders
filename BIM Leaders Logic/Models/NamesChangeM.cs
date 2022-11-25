using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class NamesChangeM : BaseModel
    {
        private static int _countNamesChanged;

        #region PROPERTIES

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

        #endregion

        public NamesChangeM(
            ExternalCommandData commandData,
            string transactionName,
            Action<string, RunResult> showResultAction
            ) : base(commandData, transactionName, showResultAction) { }

        #region METHODS

        private protected override void TryExecute()
        {
            using (Transaction trans = new Transaction(_doc, TransactionName))
            {
                trans.Start();

                ReplaceNames();

                trans.Commit();
            }

            _result.Result = GetRunResult();
        }

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

        private protected override string GetRunResult()
        {
            string text = (_countNamesChanged == 0)
                ? "No names changed"
                : $"{_countNamesChanged} names changed";

            return text;
        }

        #endregion
    }
}