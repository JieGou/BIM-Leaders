using System;
using System.Collections.Generic;
using System.Data;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class LevelsAlignM : BaseModel
    {
        private int _countLevelsAligned;

        #region PROPERTIES

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

        #endregion

        public LevelsAlignM(ExternalCommandData commandData, string transactionName) : base(commandData, transactionName)
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

                    DatumPlaneUtils.SetDatumPlanes(_doc, typeof(Level), Switch2D, Switch3D, Side1, Side2, ref _countLevelsAligned);

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

        private protected override string GetRunResult()
        {
            string text = "No levels aligned.";

            if (Switch2D)
                text = $"{_countLevelsAligned} levels switched to 2D and aligned.";
            else if (Switch3D)
                text = $"{_countLevelsAligned} levels switched to 3D and aligned.";

            text += $"{Environment.NewLine}{_countLevelsAligned} levels changed bubbles.";

            return text;
        }

        private protected override DataSet GetRunReport(IEnumerable<ReportMessage> reportMessages) { return null; }

        #endregion
    }
}