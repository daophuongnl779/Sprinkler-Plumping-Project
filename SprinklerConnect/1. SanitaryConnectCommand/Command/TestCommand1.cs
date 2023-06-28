using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Model.Data;
using Model.Entity;
using Model.Form;
using SingleData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using Utility;

namespace Model.RevitCommand
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand3 : RevitCommand
    {
        public override void Execute()
        {
            var factory = new SanitaryConnectFactory
            {
                Pipe = sel.PickElement<Pipe>(),
                PlumblingFixture = sel.PickElement<FamilyInstance>(),
            };

            factory.Do();
        }
    }
}