using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Model.Entity;
using Model.Form;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace Model.RevitCommand
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : RevitCommand

    {
        public override void Execute()
        {
            var sprinkler = sel.PickElement<FamilyInstance>();
            var mainPipe = sel.PickElement<Pipe>();

            var sprinklerConnector = sprinkler.MEPModel.ConnectorManager.UnusedConnectors.Cast<Connector>().First();
            var sprinklerConnectorOrigin = sprinklerConnector.Origin;
            var sprinklerConnectorDirection = sprinklerConnector.CoordinateSystem.BasisZ;

            using (var transaction = new Transaction(doc, "Connect sprinkler"))
            {
                transaction.Start();

                var pipeTypeId = 1543920.GetElementId();
                var levelId = mainPipe.LookupParameter("Reference Level").AsElementId();

                var endPoint = sprinklerConnectorOrigin + sprinklerConnectorDirection * 200.0.milimeter2Feet();
                var secondPipe = Pipe.Create(doc, pipeTypeId, levelId, sprinklerConnector, endPoint);

                var Dia = 32.0.milimeter2Feet();
                secondPipe.LookupParameter("Diameter").Set(Dia);

                var teeConnector3 = secondPipe.ConnectorManager.UnusedConnectors.Cast<Connector>().First();

                var mainLocationLine = ((mainPipe.Location as LocationCurve)!.Curve as Line)!;
                var projectPoint = mainLocationLine.GetProjectPoint(sprinklerConnectorOrigin);

                //var secondPipe = Pipe.Create(doc, pipeTypeId, levelId, sprinklerConnector, projectPoint);

                var minimumDistance = 20.0.milimeter2Feet();

                var mainConnectors = mainPipe.ConnectorManager.Connectors.Cast<Connector>();
                var mainPipeTypeId = mainPipe.PipeType.Id;

                //Connector? teeConnector1 = null, teeConnector2 = null;

                var points = new List<XYZ>();

                var mainPartialPipes = mainConnectors.Select(mainConnector =>
                {
                    var mainConnectorDirection = mainConnector.CoordinateSystem.BasisZ;
                    var mainPipeEndPoint = projectPoint + mainConnectorDirection * minimumDistance / 2;

                    points.Add(mainPipeEndPoint);

                    var mainPartialPipe = Pipe.Create(doc, mainPipeTypeId, levelId, mainConnector, mainPipeEndPoint);

                    

                    return mainPartialPipe;
                }).ToList();

                doc.Delete(mainPipe.Id);

                var mainPartialConnectors = mainPartialPipes.Select(pipe => pipe.ConnectorManager.UnusedConnectors.Cast<Connector>()
                .First(x => points.Any(point => x.Origin.IsEqual(point)))).ToList();

                doc.Create.NewTeeFitting(mainPartialConnectors[0], mainPartialConnectors[1], teeConnector3);

                transaction.Commit();
            }
        }
    }
}