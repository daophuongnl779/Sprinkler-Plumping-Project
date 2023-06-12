using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Model.Entity;
using Model.Form;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace Model.RevitCommand
{
    [Transaction(TransactionMode.Manual)]
    public class SanitaryConnectCommand : RevitCommand
    {
        public override void Execute()
        {
            var fixure = sel.PickElement<FamilyInstance>();
            var pipe = sel.PickElement<Pipe>();

            var firstOffset = 200.0.milimeter2Feet();
            var secondOffset = 200.0.milimeter2Feet();

            var systemTypeId = pipe.MEPSystem.GetTypeId();
            var pipeTypeId = pipe.GetTypeId();
            var levelId = pipe.LookupParameter("Reference Level").AsElementId();

            using (var transaction = new Transaction(doc, "Create pipe"))
            {
                transaction.Start();

                var fixtureConnector = fixure.MEPModel.ConnectorManager.UnusedConnectors.Cast<Connector>()
                    .First(x => x.CoordinateSystem.BasisZ.IsParallel(XYZ.BasisZ));
                var fixtureOrigin = fixtureConnector.Origin;

                var pipeLine = ((pipe.Location as LocationCurve)!.Curve as Line)!;
                var pipeDirec = pipeLine.Direction;

                var projectPoint = pipeLine.GetProjectPoint(fixtureOrigin);

                var v1 = fixtureOrigin - projectPoint;
                var v2 = new XYZ(v1.X, v1.Y, 0);
                var fixtureDir = v2.Normalize();

                var point1 = projectPoint;
                var point2 = projectPoint + (XYZ.BasisZ + fixtureDir) * firstOffset;
                var point5 = fixtureOrigin;

                var tempPoint = new XYZ(point5.X, point5.Y, point2.Z);
                var point4 = tempPoint + XYZ.BasisZ * secondOffset;
                var point3 = tempPoint - fixtureDir * secondOffset;

                var point = new List<XYZ> { point1, point2, point3, point4, point5 };
                var pipes = CreatePipes(doc, systemTypeId, pipeTypeId, levelId, point);

                transaction.Commit();
            }

        }
        public List<Pipe> CreatePipes(Document doc, ElementId systemTypeId, ElementId pipeTypeId, ElementId levelId, List<XYZ> points)

        {
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

        // Phương thức kết nối tập hợp Pipe liên tiếp nhau bằng ElbowFitting
        public void Connect(Document doc, List<Pipe> pipes)
        {
            for (int i = 0; i < pipes.Count - 1; i++)
            {
                var firstPipe = pipes[i];
                var secondPipe = pipes[i + 1];
                Connect(doc, firstPipe, secondPipe);
            }
        }

        //  KẾT NỐI 2 ĐỐI TƯỢNG PIPE BẰNG ElbowFitting
        public void Connect(Document doc, Pipe pipe1, Pipe pipe2)
        {
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
                    //Lấy ra 2 connector có vị trí chồng lên nhau
                    var origin2 = conn2.Origin;
                    if (origin1.IsEqual(origin2))
                    {
                        connector1 = conn1;
                        connector2 = conn2;
                        break;
                    }
                }
            }
            doc.Create.NewElbowFitting(connector1, connector2);
        }
        // Phương thức tách ống Pipe thành 2 đoạn ở vị trí chỉ định SplitPoint
        public List<Pipe> SplitPipe(Document doc, Pipe pipe, XYZ splitPoint)
        {
            var pipeLocation = (pipe.Location as LocationCurve)!;
            var pipeLine = (pipeLocation.Curve as Line)!;

            // Thiết lập lại thông tin định vị của Pipe tương ứng từ StartPoint tới SplitPoint
            var startPoint = pipeLine.GetEndPoint(0);
            var endPoint = pipeLine.GetEndPoint(1);

            pipeLocation.Curve = Line.CreateBound(startPoint, splitPoint);

            var systemTypeId = pipe.MEPSystem.GetTypeId();
            var pipeTypeId = pipe.GetTypeId();
            var levelId = pipe.LookupParameter("Reference Level").AsElementId();
            // Tạo ra đoạn Pipe thứ 2
            var secondPartPipe = Pipe.Create(doc, systemTypeId, pipeTypeId, levelId, splitPoint, endPoint);
            secondPartPipe.LookupParameter("Diameter").Set(pipe.LookupParameter("Diameter").AsDouble());

            return new List<Pipe> { pipe, secondPartPipe };
        }
        public void Connect ( Document doc,Pipe pipe1,Pipe pipe2,Pipe pipe3)
        {
            var connectors1 = pipe1.ConnectorManager.UnusedConnectors.Cast<Connector>();
            var connectors2 = pipe2.ConnectorManager.UnusedConnectors.Cast<Connector>();

            Connector? connector1 = null;
            Connector? connector2 = null;
            double? minDistance= null;

            foreach (var conn1 in connectors1)
            {
                var origin1 =conn1.Origin;
                foreach (var conn2 in connectors2)
                {
                    var origin2 = conn2.Origin;
                    var distance = (origin1 - origin2).GetLength();

                    if (minDistance==null || minDistance>distance)
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

            foreach(var conn3 in connectors3)
            {
                var origin = conn3.Origin;
                var distance = (origin - pipeLine.GetProjectPoint(origin)).GetLength();
                if(minDistance==null || minDistance>distance)
                {
                    connector3 = conn3;
                    minDistance= distance;
                }
            }
            doc.Create.NewTeeFitting(connector1,connector2, connector3);
        }
    }
}