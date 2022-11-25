using System.Data;

namespace BIM_Leaders_Logic
{
    public class RunResult
    {
        #region PROPERTIES

        private string _transactionName;
        public string TransactionName
        {
            get { return _transactionName; }
            set { _transactionName = value; }
        }

        private bool _started;
        public bool Started
        {
            get { return _started; }
            set { _started = value; }
        }

        private bool _failed;
        public bool Failed
        {
            get { return _failed; }
            set { _failed = value; }
        }

        private string _result;
        public string Result
        {
            get { return _result; }
            set { _result = value; }
        }

        private DataSet _report;
        public DataSet Report
        {
            get { return _report; }
            set { _report = value; }
        }

        #endregion
    }
}