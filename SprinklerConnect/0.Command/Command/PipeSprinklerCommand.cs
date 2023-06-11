using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Model.Data;
using Model.Entity;
using Model.Form;
using SingleData;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using Utility;

namespace Model.RevitCommand
{
    [Transaction(TransactionMode.Manual)]
    public class PipeSprinklerCommand : RevitCommand
    {
        private PipeSprinklerFormData formData => PipeSprinklerFormData.Instance;
        private PipeSprinklerData data => PipeSprinklerData.Instance;

        protected override bool HasExternalEvent => true;
        protected override bool IsAutoDisposed => base.IsAutoDisposed;

        public override void Execute()
        {
            // Gan su kien
            revitData.ExternalEventHandler.Action = () => data.Run();

            var form = formData.Form;
            form.Show();
        }
    }
}