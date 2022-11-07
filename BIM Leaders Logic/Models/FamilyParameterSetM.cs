using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class FamilyParameterSetM : INotifyPropertyChanged, IExternalEventHandler
    {
        private UIDocument _uidoc;
        private Document _doc;
        private int _countParametersSet = 0;

        #region PROPERTIES

        /// <summary>
        /// ExternalEvent needed for Revit to run transaction in API context.
        /// So we must call not the main method but raise the event.
        /// </summary>
        public ExternalEvent ExternalEvent { get; set; }

        private string _transactionName;
        public string TransactionName
        {
            get { return _transactionName; }
            set
            {
                _transactionName = value;
                OnPropertyChanged(nameof(TransactionName));
            }
        }

        private string _selectedParameterName;
        public string SelectedParameterName
        {
            get { return _selectedParameterName; }
            set
            {
                _selectedParameterName = value;
                OnPropertyChanged(nameof(SelectedParameterName));
            }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
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

        public FamilyParameterSetM(ExternalCommandData commandData, string transactionName)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            TransactionName = transactionName;
        }

        public void Run()
        {
            ExternalEvent.Raise();
        }

        #region IEXTERNALEVENTHANDLER

        public string GetName()
        {
            return TransactionName;
        }

        public void Execute(UIApplication app)
        {
            RunResult = "";

            try
            {
                using (Transaction trans = new Transaction(_doc, TransactionName))
                {
                    trans.Start();

                    ChangeParameter();

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

        /// <summary>
        /// Change the given parameter to given value in all family types.
        /// Value is given as string, so depends on parameter type value will be converted.
        /// </summary>
        private void ChangeParameter()
        {
            // Get parameter
            FamilyParameter parameter = _doc.FamilyManager.get_Parameter(SelectedParameterName);

            if (parameter.IsReadOnly)
                return;

            if (parameter.StorageType == StorageType.None)
                return;

            FamilyTypeSet familyTypeSet = _doc.FamilyManager.Types;

            if (parameter.StorageType == StorageType.Integer)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    _doc.FamilyManager.CurrentType = familyType;
#if VERSION2020
                    if (parameter.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(Value), DisplayUnitType.DUT_CENTIMETERS));
#else
                    if (parameter.GetUnitTypeId() == UnitTypeId.Centimeters)
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(Value), UnitTypeId.Centimeters));
#endif
                    else
                        _doc.FamilyManager.Set(parameter, Convert.ToInt32(Value));

                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.Double)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    _doc.FamilyManager.CurrentType = familyType;
#if VERSION2020
                    if (parameter.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(Value), DisplayUnitType.DUT_CENTIMETERS));
#else
                    if (parameter.GetUnitTypeId() == UnitTypeId.Centimeters)
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(Value), UnitTypeId.Centimeters));
#endif
                    else
                        _doc.FamilyManager.Set(parameter, Convert.ToDouble(Value));

                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.String)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    _doc.FamilyManager.Set(parameter, Value);

                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.ElementId)
            {
                ElementId id = new ElementId(0);

                switch (parameter.Definition.ParameterType)
                {
                    case ParameterType.Invalid:
                        break;
                    case ParameterType.Text:
                        break;
                    case ParameterType.Material:
                        ICollection<ElementId> materialIds = new FilteredElementCollector(_doc)
                            .OfClass(typeof(Material))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId materialId in materialIds)
                            if (_doc.GetElement(materialId).Name == Value)
                                id = materialId;

                        _doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF MATERIAL WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    case ParameterType.FamilyType:
                        ICollection<ElementId> familyTypeIds = new FilteredElementCollector(_doc)
                            .OfClass(typeof(FamilyType))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId familyTypeId in familyTypeIds)
                            if (_doc.GetElement(familyTypeId).Name == Value)
                                id = familyTypeId;

                        _doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF FAMILY TYPE WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    case ParameterType.Image:
                        ICollection<ElementId> imageIds = new FilteredElementCollector(_doc)
                            .OfClass(typeof(ImageType))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId imageId in imageIds)
                            if (_doc.GetElement(imageId).Name == Value)
                                id = imageId;

                        _doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF IMAGE WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    default:
                        break;
                }
            }
        }

        private void GetRunResult()
        {
            if (RunResult.Length == 0)
            {
                RunResult = (_countParametersSet == 0)
                    ? "No parameters set."
                    : $"{_countParametersSet} parameters set.";
            }
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