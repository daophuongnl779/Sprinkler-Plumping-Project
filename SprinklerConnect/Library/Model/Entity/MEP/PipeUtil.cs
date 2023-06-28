using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using SingleData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class PipeUtil
    {
        private static RevitData revitData => RevitData.Instance;

        public static List<Pipe> CreatePipes(ElementId systemTypeId, ElementId pipeTypeId, ElementId levelId, List<XYZ> points)
        {
            var doc = revitData.Document;
            var pipes = new List<Pipe>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                var startPoint = points[i];
                var endPoint = points[i + 1];

                var pipe = Pipe.Create(doc, systemTypeId, pipeTypeId, levelId, startPoint, endPoint);
                pipes.Add(pipe);
            }

            return pipes;
        }

        private static FamilyInstance connectToByElbow(Pipe pipe1, Pipe pipe2)
        {
            var doc = revitData.Document;

            var connectors1 = pipe1.ConnectorManager.UnusedConnectors.Cast<Connector>();
            var connectors2 = pipe2.ConnectorManager.UnusedConnectors.Cast<Connector>();

            Connector? connector1 = null;
            Connector? connector2 = null;

            foreach (var conn1 in connectors1)
            {
                if (connector1 != null && connector2 != null)
                {
                    break;
                }

                var origin1 = conn1.Origin;
                foreach (var conn2 in connectors2)
                {
                    var origin2 = conn2.Origin;
                    if (origin1.IsEqual(origin2))
                    {
                        connector1 = conn1;
                        connector2 = conn2;
                        break;
                    }
                }
            }

            var elbowFitting = doc.Create.NewElbowFitting(connector1, connector2);
            return elbowFitting;
        }

        public static FamilyInstance ConnectTo(this Pipe pipe1, Pipe pipe2)
        {
            return connectToByElbow(pipe1, pipe2);
        }

        public static FamilyInstance ConnectTo(this Pipe pipe1, Pipe pipe2, Pipe pipe3)
        {
            var doc = revitData.Document;

            var connectors1 = pipe1.ConnectorManager.UnusedConnectors.Cast<Connector>();
            var connectors2 = pipe2.ConnectorManager.UnusedConnectors.Cast<Connector>();

            Connector? connector1 = null;
            Connector? connector2 = null;
            double? minDistance = null;

            foreach (var conn1 in connectors1)
            {
                var origin1 = conn1.Origin;
                foreach (var conn2 in connectors2)
                {
                    var origin2 = conn2.Origin;
                    var distance = (origin1 - origin2).GetLength();

                    if (minDistance == null || minDistance > distance)
                    {
                        connector1 = conn1;
                        connector2 = conn2;
                        minDistance = distance;
                    }
                }
            }

            var pipeLine = ((pipe1.Location as LocationCurve)!.Curve as Line)!;
            var connectors3 = pipe3.ConnectorManager.UnusedConnectors.Cast<Connector>();

            Connector? connector3 = null;
            minDistance = null;

            foreach (var conn3 in connectors3)
            {
                var origin = conn3.Origin;
                var distance = (origin - pipeLine.GetProjectPoint(origin)).GetLength();
                if (minDistance == null || minDistance > distance)
                {
                    connector3 = conn3;
                    minDistance = distance;
                }
            }

            var teeFitting = doc.Create.NewTeeFitting(connector1, connector2, connector3);
            return teeFitting;
        }

        public static void ConnectTo(this Pipe pipe, FamilyInstance fittingOrFixture)
        {
            var connectors1 = pipe.ConnectorManager.UnusedConnectors.Cast<Connector>();
            var connectors2 = fittingOrFixture.MEPModel.ConnectorManager.UnusedConnectors.Cast<Connector>();

            Connector? connector1 = null;
            Connector? connector2 = null;

            foreach (var conn1 in connectors1)
            {
                if (connector1 != null && connector2 != null)
                {
                    break;
                }

                var connDir1 = conn1.CoordinateSystem.BasisZ;
                foreach (var conn2 in connectors2)
                {
                    var connDir2 = conn2.CoordinateSystem.BasisZ;
                    if (connDir1.IsOppositeDirection(connDir2))
                    {
                        connector1 = conn1;
                        connector2 = conn2;
                        break;
                    }
                }
            }

            connector1!.ConnectTo(connector2);
        }

        public static void ConnectTo(this FamilyInstance fittingOrFixture, Pipe pipe)
        {
            ConnectTo(pipe, fittingOrFixture);
        }

        public static void AutoConnect(this List<Pipe> pipes)
        {
            for (int i = 0; i < pipes.Count - 1; i++)
            {
                var firstPipe = pipes[i];
                var secondPipe = pipes[i + 1];
                firstPipe.ConnectTo(secondPipe);
            }
        }

        public static List<Pipe> Split(this Pipe pipe, XYZ splitPoint)
        {
            var doc = revitData.Document;

            var pipeLocation = (pipe.Location as LocationCurve)!;
            var pipeLine = (pipeLocation.Curve as Line)!;

            var startPoint = pipeLine.GetEndPoint(0);
            var endPoint = pipeLine.GetEndPoint(1);

            pipeLocation.Curve = Line.CreateBound(startPoint, splitPoint);

            var systemTypeId = pipe.MEPSystem.GetTypeId();
            var pipeTypeId = pipe.GetTypeId();
            var levelId = pipe.LookupParameter("Reference Level").AsElementId();

            var secondPartPipe = Pipe.Create(doc, systemTypeId, pipeTypeId, levelId, splitPoint, endPoint);
            secondPartPipe.LookupParameter("Diameter").Set(pipe.LookupParameter("Diameter").AsDouble());

            return new List<Pipe> { pipe, secondPartPipe };
        }
    }
}
