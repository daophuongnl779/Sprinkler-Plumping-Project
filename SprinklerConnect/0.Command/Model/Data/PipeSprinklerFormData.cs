using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Model.Form;
using Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utility;

namespace Model.Data
{
    public class PipeSprinklerFormData : NotifyClass
    {
        private static PipeSprinklerFormData? instance;
        public static PipeSprinklerFormData Instance
        {
            get => instance ?? (instance = new PipeSprinklerFormData());
            set=> instance = value;
        }

        private SprinklerForm? form;
        public SprinklerForm Form => form ?? (form = new SprinklerForm { DataContext = this });

        public PipeSprinklerData data => PipeSprinklerData.Instance;
    }
}
