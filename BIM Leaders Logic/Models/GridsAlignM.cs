using System;
using System.Collections.Generic;
using System.Data;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class GridsAlignM : BaseModel
    {
        private int _countGridsAligned;

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

        private bool _switch2D;
        public bool Switch2D
        {
            get { return _switch2D; }
            set
            {
                _switch2D = value;
                OnPropertyChanged(nameof(Switch2D));
            }
        }

        private bool _switch3D;
        public bool Switch3D
        {
            get { return _switch3D; }
            set
            {
                _switch3D = value;
                OnPropertyChanged(nameof(Switch3D));
            }
        }

        private bool _side1;
        public bool Side1
        {
            get { return _side1; }
            set
            {
                _side1 = value;
                OnPropertyChanged(nameof(Side1));
            }
        }

        private bool _side2;
        public bool Side2
        {
            get { return _side2; }
            set
            {
                _side2 = value;
                OnPropertyChanged(nameof(Side2));
            }
        }

        private bool _runStarted;
        public bool RunStarted
        {
            get { return _runStarted; }
            set
            {
                _runStarted = value;
                OnPropertyChanged(nameof(RunStarted));
            }
        }

        private bool _runFailed;
        public bool RunFailed
        {
            get { return _runFailed; }
            set
            {
                _runFailed = value;
                OnPropertyChanged(nameof(RunFailed));
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

        public GridsAlignM(ExternalCommandData commandData, string transactionName) : base(commandData, transactionName)
        {
        }

        #region IEXTERNALEVENTHANDLER

        public override void Execute(UIApplication app)
        {
            RunStarted = true;

            try
            {
                using (Transaction trans = new Transaction(_doc, TransactionName))
                {
                    trans.Start();

                    DatumPlaneUtils.SetDatumPlanes(_doc, typeof(Grid), Switch2D, Switch3D, Side1, Side2, ref _countGridsAligned);

                    trans.Commit();
                }

                RunResult = GetRunResult();
            }
            catch (Exception e)
            {
                RunFailed = true;
                RunResult = ExceptionUtils.GetMessage(e);
            }
        }

        #endregion

        #region METHODS

        private protected override string  GetRunResult()
        {
            string text = "No grids aligned.";
            
            if (Switch2D)
                text = $"{_countGridsAligned} grids switched to 2D and aligned.";
            else if (Switch3D)
                text = $"{_countGridsAligned} grids switched to 3D and aligned.";

            text += $"{Environment.NewLine}{_countGridsAligned} grids changed bubbles.";

            return text;
        }

        private protected override DataSet GetRunReport(IEnumerable<ReportMessage> reportMessages) { return null; }

        #endregion
    }
}