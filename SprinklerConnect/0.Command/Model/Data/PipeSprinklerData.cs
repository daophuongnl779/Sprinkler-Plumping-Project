using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Model.Entity;
using Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Utility;

namespace Model.Data
{
    public class PipeSprinklerData : NotifyClass
    {
        private static PipeSprinklerFormData formData => PipeSprinklerFormData.Instance;

        private static PipeSprinklerData? instance;
        public static PipeSprinklerData Instance
        {
            get => instance ??= new PipeSprinklerData();
            set => instance = value;
        }

        private PipeDictSprinklerFactory? factory;
        public PipeDictSprinklerFactory? Factory
        {
            get => this.factory;
            set
            {
                this.factory = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("CanDo");
                this.OnPropertyChanged("FactoryVisibility");
            }
        }

        private List<PipeType>? pipeTypes;
        public List<PipeType> PipeTypes => this.pipeTypes ??= this.GetPipeTypes();

        public bool CanDo => this.Factory != null;

        private double? formHeight = -1;
        public double FormHeight
        {
            get => this.formHeight ??= this.GetFormHeight();
            set
            {
                formHeight = value;
                OnPropertyChanged();
            }
        }

        public System.Windows.Visibility FactoryVisibility => this.Factory != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        private bool isCollapsed = true;
        public bool IsCollapsed
        {
            get => isCollapsed;
            set
            {
                this.isCollapsed = value;
                formData.Form.gbTypes.Height = this.GetFormHeight();
            }
        }
    }
}
