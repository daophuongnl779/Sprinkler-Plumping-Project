using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Model.Data;
using Model.Entity.PipeSprinklerFactoryNS;
using Model.ViewModel;
using SingleData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Utility;
using localNs = Model.Entity.PipeSprinklerFactoryNS;

namespace Model.Entity
{
    public class PipeDictSprinklerFactory : NotifyClass
    {
        public List<FamilyInstance>? Sprinklers { get; set; }

        //public Pipe? MainPipe { get; set; }

        public List<Pipe>? MainPipes { get; set; }

        private PipeSprinklerFactory? initFactory;  // chạy mồi
        public PipeSprinklerFactory InitFactory => this.initFactory ??= this.GetInitFactory();

        private PipeType? pipeType;
        public PipeType PipeType
        {
            get => this.pipeType ??= this.InitFactory.PipeType;
            set => this.pipeType = value;
        }

        private localNs.ConnectPipeType? initialConnectType;
        public localNs.ConnectPipeType InitialConnectType
        {
            get => this.initialConnectType ??= this.InitFactory.InitialConnectType;
            set
            {
                this.initialConnectType = value;
                this.ConnectTypeCheckeds = this.GetConnectTypeCheckeds();
                this.ConnectTypeEnables = this.GetConnectTypeEnables();
            }
        }

        private List<localNs.ConnectPipeType>? connectPipeTypes;
        public List<localNs.ConnectPipeType> ConnectPipeTypes => this.connectPipeTypes ??= this.InitFactory.ConnectPipeTypes;

        private Dict<bool>? connectTypeCheckeds;
        public Dict<bool> ConnectTypeCheckeds
        {
            get => this.connectTypeCheckeds ??= this.InitFactory.ConnectTypeCheckeds;
            set
            {
                this.connectTypeCheckeds = value;
                this.OnPropertyChanged();
            }
        }

        private List<bool>? connectTypeEnables;
        public List<bool> ConnectTypeEnables
        {
            get => this.connectTypeEnables ??= this.InitFactory.ConnectTypeEnables;
            set
            {
                this.connectTypeEnables = value;
                this.OnPropertyChanged();
            }
        }

        public localNs.ConnectPipeType ConnectPipeType => this.ConnectPipeTypes[this.ConnectTypeCheckeds.IndexOf(true)];

        private double? u_HeightOffset;
        public double U_HeightOffset
        {
            get => this.u_HeightOffset ??= this.InitFactory.U_HeightOffset;
            set => this.u_HeightOffset = value;
        }

        public double U_HeightOffsetMM
        {
            get => this.U_HeightOffset.feet2Milimeter();
            set => this.U_HeightOffset = value.milimeter2Feet();
        }

        private System.Windows.Visibility? u_ValueVisibility;
        public System.Windows.Visibility U_ValueVisibility
        {
            get => this.u_ValueVisibility ??= this.GetU_ValueVisibility();
            set
            {
                this.u_ValueVisibility = value;
                this.OnPropertyChanged();
            }
        }
    }
}
