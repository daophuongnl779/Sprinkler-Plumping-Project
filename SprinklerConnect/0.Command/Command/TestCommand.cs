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
    public class TestCommand1 : RevitCommand
    {
        public override void Execute()
        {
            // Input
            var sprinkler = sel.PickElement<FamilyInstance>();
            var mainPipe = sel.PickElement<Pipe>();

            var sprinklerConnector = sprinkler.MEPModel.ConnectorManager.UnusedConnectors.Cast<Connector>().First();
            var sprinklerConnectorOrigin = sprinklerConnector.Origin;
            var sprinklerConnectorDirection = sprinklerConnector.CoordinateSystem.BasisZ;

            using (var transaction = new Transaction(doc, "Test Connect sprinkler"))
            {
                transaction.Start();
                
                var pipeTypeId = mainPipe.PipeType.Id;
                var levelId = mainPipe.LookupParameter("Reference Level").AsElementId();
                var endPoint = sprinklerConnectorOrigin + sprinklerConnectorDirection * 200.0.milimeter2Feet();

                var secondPipe = Pipe.Create(doc, pipeTypeId, levelId, sprinklerConnector, endPoint);

                var teeConnector3 = secondPipe.ConnectorManager.UnusedConnectors.Cast<Connector>().First();

                var mainLocationLine = ((mainPipe.Location as LocationCurve)!.Curve as Line)!;
                var projectPoint = mainLocationLine.GetProjectPoint(sprinklerConnectorOrigin);

                var mainPipeLine = ((mainPipe.Location as LocationCurve)!.Curve as Line)!;
                var mainPipeDirection = mainPipeLine.Direction;
                var mainConnectors = mainPipe.ConnectorManager.Connectors.Cast<Connector>()
                                     .OrderBy(connector => connector.CoordinateSystem.BasisZ.IsOppositeDirection(mainPipeDirection) ? 0 : 1).ToList();

                // Output
                var mainPartialPipes = mainConnectors.Select((mainConnector, i) =>
                {
                    var mainConnectorDirection = mainConnector.CoordinateSystem.BasisZ;
                    var mainPipeEndPoint = projectPoint;

                    Pipe? mainPartialPipe = null;
                    if (i == 0)
                    {
                        //var modelLines = mainConnector.AllRefs.Cast<Connector>().
                        //                 Select(refConn => Line.CreateBound(refConn.Origin, refConn.Origin + XYZ.BasisZ * 200.0.milimeter2Feet())
                        //                 .CreateModel()).ToList();

                        var refConector = mainConnector.AllRefs.Cast<Connector>().First(refConn => refConn.Origin.IsEqual(mainConnector.Origin));
                        mainPartialPipe = Pipe.Create(doc, pipeTypeId, levelId, refConector, mainPipeEndPoint);
                    }
                    else
                    {
                        mainPartialPipe = mainPipe;
                        (mainPartialPipe.Location as LocationCurve)!.Curve = Line.CreateBound(mainPipeEndPoint, mainConnector.Origin);
                    }
                    return mainPartialPipe;
                }).ToList();

                var mainPartialConnectors = mainPartialPipes.Select(x => x.ConnectorManager.UnusedConnectors.Cast<Connector>().First()).ToList();
                doc.Create.NewTeeFitting(mainPartialConnectors[0], mainPartialConnectors[1], teeConnector3);


                transaction.Commit();
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class TestCommand2 : RevitCommand
    {
        public override void Execute()
        {
            var selectedElems = sel.PickElements();
            var sprinkler = selectedElems.Where(x => (BuiltInCategory)x.Category.Id.IntegerValue == BuiltInCategory.OST_Sprinklers).First() as FamilyInstance;
            var mainPipe = selectedElems.Where(x => (BuiltInCategory)x.Category.Id.IntegerValue == BuiltInCategory.OST_PipeCurves).First() as Pipe;

            var pipeTypes = new FilteredElementCollector(doc).OfClass(typeof(PipeType));

            var factory = new PipeSprinklerFactory
            {
                //Sprinkler = sprinkler,
                MainPipe = mainPipe,
                PipeType = (PipeType)pipeTypes.FirstOrDefault(x => x.Name == "Steel pipe Fe-35"),
            };
            factory.Create();
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class TestSprinklers : RevitCommand
    {
        public override void Execute()
        {
            var selectedElems = sel.PickElements();
            var sprinklers = selectedElems.Where(x => (BuiltInCategory)x.Category.Id.IntegerValue == BuiltInCategory.OST_Sprinklers).Cast<FamilyInstance>().ToList();
            var mainPipe = selectedElems.Where(x => (BuiltInCategory)x.Category.Id.IntegerValue == BuiltInCategory.OST_PipeCurves).First() as Pipe;

            var factory = new PipeDictSprinklerFactory
            {
                Sprinklers = sprinklers,
                //MainPipe = mainPipe,
            };
            factory.Do();
        }
    }
}