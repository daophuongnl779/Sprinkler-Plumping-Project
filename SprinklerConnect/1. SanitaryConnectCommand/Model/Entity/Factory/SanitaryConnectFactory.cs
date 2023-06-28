using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Model.Entity
{
    public class SanitaryConnectFactory
    {
        public Pipe? Pipe { get; set; }

        public FamilyInstance? PlumblingFixture { get; set; }

        private ElementId? levelId;
        public ElementId LevelId => this.levelId ??= this.GetLevelId();

        private ElementId? systemTypeId;
        public ElementId SystemTypeId => this.systemTypeId ??= this.GetSystemTypeId();

        private ElementId? pipeTypeId;
        public ElementId PipeTypeId => this.pipeTypeId ??= this.GetPipeTypeId();

        private Line? pipeLine;
        public Line PipeLine => this.pipeLine ??= this.GetPipeLine();

        public XYZ PipeDirection => this.PipeLine.Direction;

        private XYZ? normal;
        public XYZ Normal => this.normal ??= this.GetNormal();

        private Connector? fixtureConnector;
        public Connector FixtureConnector => this.fixtureConnector ??= this.GetFixtureConnector();

        public XYZ FixtureOrigin => this.FixtureConnector.Origin;

        public double Offset1 { get; set; } = 600.0.milimeter2Feet();

        public double Offset2 { get; set; } = 500.0.milimeter2Feet();

        private XYZ? projectPoint;
        public XYZ ProjectPoint => this.projectPoint ??= this.GetProjectPoint();

        private List<XYZ>? connectingPoints;
        public List<XYZ> ConnectingPoints => this.connectingPoints ??= this.GetConnectingPoints();

        public XYZ FirstPoint => this.ConnectingPoints[0];

        private List<Pipe>? pipeSystem;
        public List<Pipe> PipeSystem => this.pipeSystem ??= this.GetPipeSystem();

        public Pipe FirstPipe => this.PipeSystem[0];

        private FamilyInstance? teeFitting;
        public FamilyInstance TeeFitting => this.teeFitting ??= this.GetTeeFitting();
    }
}
