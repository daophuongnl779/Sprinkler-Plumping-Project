using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Model.Data;
using Model.Entity;
using SingleData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Utility;
using localNs = Model.Entity.PipeSprinklerFactoryNS;

namespace Model.Entity
{
    public static class PipeSprinklerFactoryUtil
    {
        private static RevitData revitData => RevitData.Instance;
        private static PipeSprinklerData data => PipeSprinklerData.Instance;
        private static PipeSprinklerFormData formData => PipeSprinklerFormData.Instance;

        public static ElementId GetLevelId(this PipeSprinklerFactory q)
        {
            return q.MainPipe!.LookupParameter("Reference Level").AsElementId();
        }

        public static ElementId GetSystemTypeId(this PipeSprinklerFactory q)
        {
            return q.MainPipe!.MEPSystem.GetTypeId();
        }

        public static Connector GetSprinklerConnector(this PipeSprinklerFactory q)
        {
            var sprinkler = q.Sprinkler!;
            return sprinkler.MEPModel.ConnectorManager.UnusedConnectors.Cast<Connector>().First();
        }

        public static ElementId GetMainPipeTypeId(this PipeSprinklerFactory q)
        {
            return q.MainPipe!.GetTypeId();
        }

        public static PipeType GetPipeType(this PipeSprinklerFactory q)
        {
            var pipeTypes = data.PipeTypes;
            return pipeTypes.FirstOrDefault(x => x.Name == "Steel pipe Fe-35");
        }

        #region Test
        //public static Pipe GetPipeSprinkler1(this PipeSprinklerFactory q)
        //{

        //    // Khoi lenh xu ly de tra ve thong tin Pipe
        //    var doc = revitData.Document;

        //    var sprinkler = q.Sprinkler!;
        //    var mainPipe = q.MainPipe!;

        //    var pipeType = q.PipeType;
        //    var pipeTypeId = pipeType!.Id;

        //    var levelId = q.LevelId;

        //    var sprinklerConnector = sprinkler.MEPModel.ConnectorManager.UnusedConnectors.Cast<Connector>().First();
        //    var connectorOrigin = sprinklerConnector.Origin;
        //    var connectorDirection = sprinklerConnector.CoordinateSystem.BasisZ;

        //    var endPoint = connectorOrigin + connectorDirection * 200.0.milimeter2Feet();
        //    var pipe = Pipe.Create(doc, pipeTypeId, levelId, sprinklerConnector, endPoint);


        //    var mainLocationLine = ((mainPipe.Location as LocationCurve)!.Curve as Line)!;
        //    var projectPoint = mainLocationLine.GetProjectPoint(connectorOrigin);

        //    //var mainPipeLine = ((mainPipe.Location as LocationCurve)!.Curve as Line)!;
        //    var mainPipeDirection = mainLocationLine.Direction;
        //    var mainConnectors = mainPipe.ConnectorManager.Connectors.Cast<Connector>()
        //                         .OrderBy(connector => connector.CoordinateSystem.BasisZ.IsOppositeDirection(mainPipeDirection) ? 0 : 1).ToList();

        //    // Output
        //    var mainPartialPipes = mainConnectors.Select((mainConnector, i) =>
        //    {
        //        var mainConnectorDirection = mainConnector.CoordinateSystem.BasisZ;
        //        var mainPipeEndPoint = projectPoint;

        //        Pipe? mainPartialPipe = null;
        //        if (i == 0)
        //        {
        //            var refConector = mainConnector.AllRefs.Cast<Connector>().First(refConn => refConn.Origin.IsEqual(mainConnector.Origin));
        //            mainPartialPipe = Pipe.Create(doc, pipeTypeId, levelId, refConector, mainPipeEndPoint);
        //        }
        //        else
        //        {
        //            mainPartialPipe = mainPipe;
        //            (mainPartialPipe.Location as LocationCurve)!.Curve = Line.CreateBound(mainPipeEndPoint, mainConnector.Origin);
        //        }
        //        return mainPartialPipe;
        //    }).ToList();

        //    var mainPartialConnectors = mainPartialPipes.Select(x => x.ConnectorManager.UnusedConnectors.Cast<Connector>().First()).ToList();

        //    var teeConnector3 = pipe.ConnectorManager.UnusedConnectors.Cast<Connector>().First();
        //    doc.Create.NewTeeFitting(mainPartialConnectors[0], mainPartialConnectors[1], teeConnector3);

        //    return pipe;
        //}
        #endregion

        public static List<Pipe> GetPipeSprinklers(this PipeSprinklerFactory q)
        {
            // Khoi lenh xu ly de tra ve thong tin Pipe
            var doc = revitData.Document;

            var pipeType = q.PipeType!;
            var pipeTypeId = pipeType.Id;

            var levelId = q.LevelId;

            var sprinklerConnector = q.SprinklerConnector!;
            var connectorOrigin = sprinklerConnector.Origin;
            var connectorDirection = sprinklerConnector.CoordinateSystem.BasisZ;

            XYZ? endPoint = null;
            var mainPipe = q.MainPipe!;
            var mainLocationLine = ((mainPipe.Location as LocationCurve)!.Curve as Line)!;

            var connectorType = q.ConnectPipeType;
            switch (connectorType)
            {
                case localNs.ConnectPipeType.T:
                    endPoint = connectorOrigin + connectorDirection * 200.0.milimeter2Feet();
                    break;
                case localNs.ConnectPipeType.V:
                case localNs.ConnectPipeType.U:
                    var mainPipeZ = mainLocationLine.GetEndPoint(0).Z;
                    var heightOffset = connectorType == localNs.ConnectPipeType.V ? 0 : q.U_HeightOffset;
                    endPoint = new XYZ(connectorOrigin.X, connectorOrigin.Y, mainPipeZ + heightOffset);
                    break;
                default:
                    break;
            }

            // Output
            var pipe1 = Pipe.Create(doc, pipeTypeId, levelId, sprinklerConnector, endPoint);

            switch (connectorType)
            {
                case localNs.ConnectPipeType.T:
                    return new List<Pipe> { pipe1 };
                case localNs.ConnectPipeType.V:
                case localNs.ConnectPipeType.U:
                    {
                        var pipe2_StartPoint = endPoint!;

                        var projectPoint = q.ProjectionPoint;
                        var pipe2_Point2 = new XYZ(projectPoint.X,projectPoint.Y, pipe2_StartPoint.Z);

                        var pipe2_Direction = (pipe2_Point2 - pipe2_StartPoint).Normalize();
                        var pipe2_EndPoint = connectorType == localNs.ConnectPipeType.V ? pipe2_StartPoint + pipe2_Direction * 300.0.milimeter2Feet() : pipe2_Point2;

                        var pipe2 = Pipe.Create(doc, q.SystemTypeId, pipeTypeId, levelId, pipe2_StartPoint, pipe2_EndPoint);
                        pipe2.LookupParameter("Diameter").Set(pipe1.LookupParameter("Diameter").AsDouble());
                        {
                            var connector1 = pipe1.ConnectorManager.UnusedConnectors.Cast<Connector>().First();
                            var connector2 = pipe2.ConnectorManager.UnusedConnectors.Cast<Connector>().First(x => x.Origin.IsEqual(pipe2_StartPoint));
                            doc.Create.NewElbowFitting(connector1, connector2);
                        }
                        switch (connectorType)
                        {
                            case localNs.ConnectPipeType.V:
                                return new List<Pipe> { pipe1, pipe2 };
                            case localNs.ConnectPipeType.U:
                                var pipe3_StartPoint = pipe2_EndPoint;
                                var pipe3 = Pipe.Create(doc, q.SystemTypeId, pipeTypeId, levelId, pipe3_StartPoint, projectPoint);
                                pipe3.LookupParameter("Diameter").Set(pipe1.LookupParameter("Diameter").AsDouble());
                                {
                                    var connector2 = pipe2.ConnectorManager.UnusedConnectors.Cast<Connector>().First();
                                    var connector3 = pipe3.ConnectorManager.UnusedConnectors.Cast<Connector>().First(x => x.Origin.IsEqual(pipe3_StartPoint));
                                    doc.Create.NewElbowFitting(connector2, connector3);
                                }
                                return new List<Pipe> { pipe1, pipe2, pipe3 };
                        }
                        break;
                    }
            }
            throw new Exception("This code shouldn't be reached");
        }

        public static XYZ GetProjectionPoint(this PipeSprinklerFactory q)
        {
            var mainPipe = q.MainPipe!;

            var sprinklerConnector = q.SprinklerConnector!;
            var connectorOrigin = sprinklerConnector.Origin;

            var mainLocationLine = ((mainPipe.Location as LocationCurve)!.Curve as Line)!;
            return mainLocationLine.GetProjectPoint(connectorOrigin);
        }

        public static List<Pipe> GetMainPartialPipes(this PipeSprinklerFactory q)
        {
            // Khoi lenh xu ly de tra ve thong tin MainPartialPipe
            var doc = revitData.Document;

            var mainPipe = q.MainPipe!;
            var levelId = q.LevelId;

            var mainLocationLine = ((mainPipe.Location as LocationCurve)!.Curve as Line)!;
            var projectPoint = q.ProjectionPoint;

            var mainPipeDirection = mainLocationLine.Direction;
            var mainConnectors = mainPipe.ConnectorManager.Connectors.Cast<Connector>()
                                 .OrderBy(connector => connector.CoordinateSystem.BasisZ.IsOppositeDirection(mainPipeDirection) ? 0 : 1).ToList();

            // Output
            var mainPartialPipes = mainConnectors.Select((mainConnector, i) =>
            {
                var mainConnectorDirection = mainConnector.CoordinateSystem.BasisZ;
                var mainPipeEndPoint = projectPoint;
                var mainPipeTypeId = q.MainPipeTypeId;

                Pipe? mainPartialPipe = null;
                if (i == 0)
                {
                    var refConector = mainConnector.AllRefs.Cast<Connector>().FirstOrDefault(refConn => refConn.ConnectorType != ConnectorType.Logical &&
                                                                              refConn.Origin.IsEqual(mainConnector.Origin));
                    if(refConector != null)
                    {
                        mainPartialPipe = Pipe.Create(doc, mainPipeTypeId, levelId, refConector, mainPipeEndPoint);
                    }
                    else
                    {
                        mainPartialPipe = Pipe.Create(doc, q.SystemTypeId, mainPipeTypeId, levelId, mainLocationLine.GetEndPoint(0), mainPipeEndPoint);
                        mainPartialPipe.LookupParameter("Diameter").Set(mainPipe.LookupParameter("Diameter").AsDouble());
                    } 
                }
                else
                {
                    mainPartialPipe = mainPipe;
                    (mainPartialPipe.Location as LocationCurve)!.Curve = Line.CreateBound(mainPipeEndPoint, mainConnector.Origin);
                }
                return mainPartialPipe;
            }).ToList();

            return mainPartialPipes;
        }

        public static FamilyInstance GetTeeFitting(this PipeSprinklerFactory q)
        {
            // Khoi lenh xu ly de tra ve thong tin TeeFitting
            var doc = revitData.Document;

            var pipe = q.PipeSprinklers.Last();
            var teeConnector3 = pipe.ConnectorManager.UnusedConnectors.Cast<Connector>().First();

            var mainPartialPipes = q.MainPartialPipes;
            var mainPartialConnectors = mainPartialPipes.Select(x => x.ConnectorManager.UnusedConnectors.Cast<Connector>().First()).ToList();

            // Output
            var teeFitting = doc.Create.NewTeeFitting(mainPartialConnectors[0], mainPartialConnectors[1], teeConnector3);

            return teeFitting;
        }

        public static List<localNs.ConnectPipeType> GetConnectPipeTypes(this PipeSprinklerFactory q)
        {
            return Enum.GetValues(typeof(localNs.ConnectPipeType)).Cast<localNs.ConnectPipeType>().ToList();
        }

        public static localNs.ConnectPipeType GetInitialConnectType(this PipeSprinklerFactory q)
        {
            var sprinklerConnector = q.SprinklerConnector!;
            var connectorOrigin = sprinklerConnector.Origin;
            var projectPoint = q.ProjectionPoint;

            var direction = (projectPoint - connectorOrigin).Normalize();
            return direction.IsParallel(XYZ.BasisZ) ? localNs.ConnectPipeType.T : localNs.ConnectPipeType.V;
        }

        public static Dict<bool> GetConnectTypeCheckeds(this PipeSprinklerFactory q)
        {
            var qI = q.ConnectPipeTypes.Select(x => x == q.InitialConnectType).ToDict();
            qI.OnSetItem = (value, index) =>
            {
                q.U_ValueVisibility = q.GetU_ValueVisibility();
            };
            return qI;
        }

        // T => IsEnabled = false
        public static List<bool> GetConnectTypeEnables(this PipeSprinklerFactory q)
        {
            var connectType = q.InitialConnectType;
            return q.ConnectPipeTypes.Select(x => connectType == localNs.ConnectPipeType.T ? false : x != localNs.ConnectPipeType.T).ToList();
        }

        // Cap nhat lai ConnectType
        public static void RefreshInitialConnectType(this PipeSprinklerFactory q)
        {
            q.InitialConnectType = q.GetInitialConnectType();
        }

        public static System.Windows.Visibility GetU_ValueVisibility(this PipeSprinklerFactory q)
        {
            return q.ConnectPipeType == localNs.ConnectPipeType.U ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        #region Method
        public static void Create(this PipeSprinklerFactory q)
        {
            var doc = revitData.Document;
            //var dict = q.Dict;

            //Action action = () =>
            //{
            //    var pipe = q.PipeSprinklers;
            //    var mainPartialPipes = q.MainPartialPipes;
            //    var teeFitting = q.TeeFitting;
            //};

            //if (dict != null)
            //{
            //    action();
            //}
            //else
            //{
            //    action();
            //}
            var pipe = q.PipeSprinklers;
            var mainPartialPipes = q.MainPartialPipes;
            var teeFitting = q.TeeFitting;
        }
        #endregion
    }
}
