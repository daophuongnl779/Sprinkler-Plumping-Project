using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Model.Entity;
using Model.Form;
using System;
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

            var systemTypeId = pipe.MEPSystem.GetTypeId();
            var pipeTypeId = pipe.GetTypeId();
            var levelId = pipe.LookupParameter("Reference Level").AsElementId();

            using (var transaction = new Transaction(doc, "Create pipe"))
            {
                transaction.Start();

                var fixtureConnector = fixure.MEPModel.ConnectorManager.UnusedConnectors.Cast<Connector>()
                    .First(x => x.CoordinateSystem.BasisZ.IsParallel(XYZ.BasisZ));
                //var fixtureConnector = fixure.MEPModel.ConnectorManager.UnusedConnectors.Cast<Connector>().First();

               var fixtureOrigin = fixtureConnector.Origin;

                var pipeLine = ((pipe.Location as LocationCurve)!.Curve as Line)!;
                var pipeDir = pipeLine.Direction;

                var projectPoint = pipeLine.GetProjectPoint(fixtureOrigin);

                var v1 = fixtureOrigin - projectPoint;
                var v2 = new XYZ(v1.X, v1.Y, 0);
                var fixtureDir = v2.Normalize();

                var pipeLength = 200.0.milimeter2Feet();
                var slope = 0.02;
                //head
                var point1 = projectPoint + pipeDir * pipeLength;
                //var point2 = GetPoint(projectPoint,fixtureDir*pipeLength, slope);

                var _point2 = projectPoint + fixtureDir * pipeLength;
                var point2 = new XYZ(_point2.X, _point2.Y, _point2.Z + (_point2 - point1).GetLength() * slope);


                var _point3 = new XYZ(fixtureOrigin.X, fixtureOrigin.Y, point2.Z) - fixtureDir * pipeLength;
                var point3 = new XYZ(_point3.X,_point3.Y, _point3.Z + (_point3- point2).GetLength() * slope);

                var point4 = new XYZ(fixtureOrigin.X, fixtureOrigin.Y, point2.Z) + XYZ.BasisZ * pipeLength;
                var point5 = fixtureOrigin;

                var points = new List<XYZ> { point1, point2, point3, point4, point5 };
                var pipes = CreatePipes(doc,systemTypeId, pipeTypeId, levelId, points);
                // Thêm set Diameter ống theo DN của fixture

                //Connect(doc, pipes);

                //var pipe5 = pipes.Last();
                //var pipe5Connector = pipe5.ConnectorManager.UnusedConnectors.Cast<Connector>().First();
                //pipe5Connector.ConnectTo(fixtureConnector);

                var splitpipe = SplitPipe(doc, pipe, point1);

                var _point6 = point1 + fixtureDir* pipeLength;
                var point6 = new XYZ(_point6.X, _point6.Y, _point6.Z + (_point6 - point1).GetLength() * slope);

                var tempPipe = CreatePipes(doc, systemTypeId, pipeTypeId, levelId, new List<XYZ> { point1, point6 }).First();
                
                //var teeFitting= Connect(doc, splitpipe[0], splitpipe[1], tempPipe);
                //doc.Delete(tempPipe.Id);

                //var basisX = pipeDir;
                //var basisY = basisX.CrossProduct(-XYZ.BasisZ);

                //var pipe1 = pipes.First();
                ////var pipe1Dir = ((pipe1.Location as LocationCurve)!.Curve as Line)!.Direction;
                //var pipe1Dir = (_point2 - point1).Normalize();
                //var angle = pipe1Dir.GetAngle(basisX, basisY);

                //teeFitting.ParametersMap.Cast<Parameter>().First(x => x.Definition.Name.ToUpper().Contains("ANGLE")).Set(angle);

                //var pipe1Connector = pipe1.ConnectorManager.UnusedConnectors.Cast<Connector>().First();
                //var teeFittingConnector = teeFitting.MEPModel.ConnectorManager.UnusedConnectors.Cast<Connector>().First();
                //pipe1Connector.ConnectTo(teeFittingConnector);

                transaction.Commit();
            }
        }

        //Phương thức tạo hàng loạt đối tượng Pipes nối tiếp nhau trong danh sách Point có sẵn
        public List<Pipe> CreatePipes(Document doc, ElementId systemTypeId, ElementId pipeTypeId, ElementId levelId, List<XYZ> points)
        {
            //Khởi tạo trước danh sách rỗng Pipe
            var pipes = new List<Pipe>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                var startPoint = points[i];
                var endPoint = points[i + 1];
                var pipe = Pipe.Create(doc, systemTypeId, pipeTypeId, levelId, startPoint, endPoint);
                var Dia = 110.0.milimeter2Feet();
                pipe.LookupParameter("Diameter").Set(Dia);
                pipes.Add(pipe);
            }
            return pipes;
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

        //Phương thức kết nối 3 đối tượng bằng TeeFitting
        public FamilyInstance Connect (Document doc,Pipe pipe1,Pipe pipe2,Pipe pipe3)
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
            var teeFitting= doc.Create.NewTeeFitting(connector1,connector2, connector3);
            return teeFitting;
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

        //TẠO ỐNG TẠM THỜI
        //public XYZ GetPoint (XYZ point, XYZ vec, double slope)
        //{
        //    var tempPoint = point + vec;
        //    return new XYZ(tempPoint.X + tempPoint.Y, tempPoint.Z + vec.GetLength() * slope);
        //}
    }

}