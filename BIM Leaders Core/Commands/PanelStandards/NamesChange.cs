using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;
using System.Threading.Tasks;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class NamesChange : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Change Names";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Run(commandData);

            return Result.Succeeded;
        }

        private async void Run(ExternalCommandData commandData)
        {
            // Model
            NamesChangeM formM = new NamesChangeM(commandData, TRANSACTION_NAME);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            NamesChangeVM formVM = new NamesChangeVM(formM);

            // View
            NamesChangeForm form = new NamesChangeForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            ShowResult(formM.RunResult);
        }

        private void ShowResult(string resultText)
        {
            if (resultText.Length == 0)
                return;

            // ViewModel
            ReportVM formVM = new ReportVM(TRANSACTION_NAME, resultText);

            // View
            ReportForm form = new ReportForm() { DataContext = formVM };
            form.ShowDialog();
        }     

        /// <summary>
        /// Gets the full namespace path to this command
        /// </summary>
        /// <returns></returns>
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(NamesChange).Namespace + "." + nameof(NamesChange);
        }
    }
}