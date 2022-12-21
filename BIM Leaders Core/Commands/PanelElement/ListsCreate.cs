using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class ListsCreate : BaseCommand
    {
        public ListsCreate()
        {
            _transactionName = "Create Lists";

            _model = new ListsCreateModel();
            _viewModel = new ListsCreateViewModel();
            _view = new ListsCreateForm();
        }

        public static string GetPath() => typeof(ListsCreate).Namespace + "." + nameof(ListsCreate);
    }
}