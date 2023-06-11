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
    // Kieu du lieu tao ra doi tuong Pipe cho Sprinkler 
    public class PipeSprinklerFactory : NotifyClass
    {
        public static PipeSprinklerFormData formData => PipeSprinklerFormData.Instance;

        public PipeDictSprinklerFactory? Dict { get; set; }

        public FamilyInstance? Sprinkler { get; set; }

        public Pipe? MainPipe { get; set; }

        private ElementId? level;
        public ElementId LevelId => this.level ??= this.GetLevelId();

        private ElementId? systemTypeId;
        public ElementId SystemTypeId => this.systemTypeId ??= this.GetSystemTypeId();

        private ElementId? mainPipeTypeId;
        public ElementId MainPipeTypeId => this.mainPipeTypeId ??= this.GetMainPipeTypeId();

        // 1
        private PipeType? pipeType;
        public PipeType PipeType
        {
            get => this.Dict != null ? this.Dict.PipeType : (this.pipeType ??= this.GetPipeType());
            set=> this.pipeType = value;
        }

        private Connector? sprinklerConnector;
        public Connector SprinklerConnector => this.sprinklerConnector ??= this.GetSprinklerConnector();

        private List<Pipe>? pipeSprinklers;
        public List<Pipe> PipeSprinklers => this.pipeSprinklers ??= this.GetPipeSprinklers();

        // Get Project Point
        private XYZ? projectionPoint;
        public XYZ ProjectionPoint => this.projectionPoint ??= this.GetProjectionPoint();

        private List<Pipe>? mainPartialPipes;
        public List<Pipe> MainPartialPipes => this.mainPartialPipes ??= this.GetMainPartialPipes();

        private FamilyInstance? teeFitting;
        public FamilyInstance TeeFitting => this.teeFitting ??= this.GetTeeFitting();

        // 2
        private localNs.ConnectPipeType? initialConnectType;
        public localNs.ConnectPipeType InitialConnectType
        {
            get => this.Dict != null ? this.Dict.InitialConnectType : (this.initialConnectType ??= this.GetInitialConnectType());
            set
            {
                this.initialConnectType = value;
                this.ConnectTypeCheckeds = this.GetConnectTypeCheckeds();
                this.ConnectTypeEnables= this.GetConnectTypeEnables();
            }
        }

        private List<localNs.ConnectPipeType>? connectPipeTypes;
        public List<localNs.ConnectPipeType> ConnectPipeTypes => this.connectPipeTypes??= this.GetConnectPipeTypes();

        // 3
        private Dict<bool>? connectTypeCheckeds;
        public Dict<bool> ConnectTypeCheckeds
        {
            get => this.Dict != null ? this.Dict.ConnectTypeCheckeds : (this.connectTypeCheckeds ??= this.GetConnectTypeCheckeds());
            set
            {
                this.connectTypeCheckeds = value;
                this.OnPropertyChanged();
            }
        }

        private List<bool>? connectTypeEnables;
        public List<bool> ConnectTypeEnables
        {
            get => this.Dict != null ? this.Dict.ConnectTypeEnables : (this.connectTypeEnables ??= this.GetConnectTypeEnables());
            set
            {
                this.connectTypeEnables = value;
                this.OnPropertyChanged();
            }
        }

        public localNs.ConnectPipeType ConnectPipeType => this.ConnectPipeTypes[this.ConnectTypeCheckeds.IndexOf(true)];


        private double u_HeightOffset = 200.0.milimeter2Feet();
        public double U_HeightOffset
        {
            get => this.Dict != null ? this.Dict.U_HeightOffset : this.u_HeightOffset;
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
            get => this.u_ValueVisibility??= this.GetU_ValueVisibility();
            set
            {
                this.u_ValueVisibility = value;
                this.OnPropertyChanged();
            }
        }
    }
}
