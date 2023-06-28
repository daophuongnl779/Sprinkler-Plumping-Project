using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using SingleData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Model.Entity
{
    public static class SanitaryConnectFactoryUtil
    {
        private static RevitData revitData => RevitData.Instance;

        public static ElementId GetLevelId(this SanitaryConnectFactory q)
        {
            return q.Pipe!.LookupParameter("Reference Level").AsElementId();
        }

        public static ElementId GetSystemTypeId(this SanitaryConnectFactory q)
        {
            return q.Pipe!.MEPSystem.GetTypeId();
        }

        public static ElementId GetPipeTypeId(this SanitaryConnectFactory q)
        {
            return q.Pipe!.GetTypeId();
        }

        public static Line GetPipeLine(this SanitaryConnectFactory q)
        {
            return ((q.Pipe!.Location as LocationCurve)!.Curve as Line)!;
        }

        public static Connector GetFixtureConnector(this SanitaryConnectFactory q)
        {
            return q.PlumblingFixture!.MEPModel.ConnectorManager.UnusedConnectors.Cast<Connector>()
                .First(x => x.CoordinateSystem.BasisZ.IsParallel(XYZ.BasisZ));
        }

        public static XYZ GetProjectPoint(this SanitaryConnectFactory q)
        {
            return q.PipeLine.GetProjectPoint(q.FixtureOrigin);
        }

        public static XYZ GetNormal(this SanitaryConnectFactory q)
        {
            var projectPoint = q.ProjectPoint;
            var fixtureOrigin = q.FixtureOrigin;

            var vector = (fixtureOrigin - projectPoint);
            var rVector = new XYZ(vector.X, vector.Y, 0);

            var normal = rVector.Normalize();
            return normal;
        }

        public static List<XYZ> GetConnectingPoints(this SanitaryConnectFactory q)
        {
            var pipeDir = q.PipeDirection;
            var normal = q.Normal;

            var offset1 = q.Offset1;
            var offset2 = q.Offset2;

            var fixtureOrigin = q.FixtureOrigin;
            var projectPoint = q.ProjectPoint;

            var point1 = projectPoint + pipeDir * offset1;
            //var point1 = projectPoint - pipeDir * offset1;
            var point2 = (projectPoint + normal * offset1).Sloping(point1, 0.02);

            var fixtureOriginOnPlane = new XYZ(fixtureOrigin.X, fixtureOrigin.Y, point2.Z);

            var point3 = (fixtureOriginOnPlane - normal * offset2).Sloping(point2, 0.02);
            var point4 = fixtureOriginOnPlane + XYZ.BasisZ * offset2;

            var point5 = fixtureOrigin;

            return new List<XYZ> { point1, point2, point3, point4, point5 };
        }

        public static List<Pipe> GetPipeSystem(this SanitaryConnectFactory q)
        {
            var pipes = PipeUtil.CreatePipes(q.SystemTypeId, q.PipeTypeId, q.LevelId, q.ConnectingPoints);
            pipes.AutoConnect();

            return pipes;
        }

        public static FamilyInstance GetTeeFitting(this SanitaryConnectFactory q)
        {
            var doc = revitData.Document;

            var points = q.ConnectingPoints;
            var projectPoint = q.ProjectPoint;

            var firstPoint = q.FirstPoint;
            var splitPipes = q.Pipe!.Split(firstPoint);

            var alignNormal = (points[1] - projectPoint).Normalize();

            var tempPoint = firstPoint + alignNormal * 200.0.milimeter2Feet();
            var tempPipe = Pipe.Create(doc, q.SystemTypeId, q.PipeTypeId, q.LevelId, firstPoint, tempPoint);

            var teeFitting = splitPipes[0].ConnectTo(splitPipes[1], tempPipe);
            doc.Delete(tempPipe.Id);

            //var basisX = (points[0] - projectPoint).Normalize();
            var basisX = q.PipeDirection;
            var basisY = alignNormal;

            var firstPipe = q.FirstPipe;
            var firstPipeDir = ((firstPipe.Location as LocationCurve)!.Curve as Line)!.Direction;

            var angle = firstPipeDir.GetAngle(basisX, basisY);
            teeFitting.ParametersMap.Cast<Parameter>()
                .First(x => x.Definition.Name.ToLower().Contains("angle")).Set(angle);

            doc.Regenerate();
            firstPipe.ConnectTo(teeFitting);

            return teeFitting;
        }

        public static void Do(this SanitaryConnectFactory q)
        {
            var doc = revitData.Document;

            doc.DoTransaction(new TransactionConfig
            {
                Name = "Sanitary Connect",
                Action = () =>
                {
                    var pipeSystem = q.PipeSystem;
                    var teeFitting = q.TeeFitting;

                    var lastPipe = pipeSystem.Last();
                    lastPipe.ConnectTo(q.PlumblingFixture!);
                }
            });
        }
    }
}
