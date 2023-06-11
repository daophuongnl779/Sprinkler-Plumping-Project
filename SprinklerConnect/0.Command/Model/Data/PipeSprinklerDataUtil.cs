using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using SingleData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Entity;
using System.Windows.Controls;
using Utility;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Net.Mime;
using Model.Entity.PipeSprinklerFactoryNS;

namespace Model.Data
{
    public static class PipeSprinklerDataUtil
    {
        private static RevitData revitData => RevitData.Instance;
        private static PipeSprinklerData data => PipeSprinklerData.Instance;

        public static void Select(this PipeSprinklerData q)
        {
            var doc = revitData.Document;
            var sel = revitData.Selection;
 
            try
            {
                data.IsCollapsed = false;

                //var selectedElems = sel.PickElements();
                //var sprinkler = selectedElems.First(x => (BuiltInCategory)x.Category.Id.IntegerValue == BuiltInCategory.OST_Sprinklers) as FamilyInstance;
                //var mainPipe = selectedElems.First(x => (BuiltInCategory)x.Category.Id.IntegerValue == BuiltInCategory.OST_PipeCurves) as Pipe;

                //q.Factory = new PipeSprinklerFactory
                //{
                //    Sprinkler = sprinkler,
                //    MainPipe = mainPipe
                //};
                //q.Factory.RefreshInitialConnectType();

                var selectedElems = sel.PickElements();
                var sprinklers = selectedElems.Where(x => (BuiltInCategory)x.Category.Id.IntegerValue == BuiltInCategory.OST_Sprinklers).Cast<FamilyInstance>().ToList();
                var mainPipes = selectedElems.Where(x => (BuiltInCategory)x.Category.Id.IntegerValue == BuiltInCategory.OST_PipeCurves).Cast<Pipe>().ToList();

                q.Factory = new PipeDictSprinklerFactory
                {
                    Sprinklers = sprinklers,
                    MainPipes = mainPipes,
                };
                
                //q.Factory.RefreshInitialConnectType();

                data.IsCollapsed = true;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                MessageBox.Show("Please select Elements first !");
            }
        }

        public static void Run(this PipeSprinklerData q)
        {
            var factory = q.Factory!;

            //factory.Create();

            factory.Do();
            q.Factory = null;
        }

        public static List<PipeType> GetPipeTypes(this PipeSprinklerData q)
        {
            var doc = revitData.Document;
            var pipeTypes = new FilteredElementCollector(doc).OfClass(typeof(PipeType)).Cast<PipeType>().ToList();
            return pipeTypes;
        }

        public static double GetFormHeight(this PipeSprinklerData q)
        {
            return q.IsCollapsed ? 65 : 0;
        }
    }
}
