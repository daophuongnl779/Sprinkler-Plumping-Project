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
using System.Windows.Media.Media3D;
using Utility;
using localNs = Model.Entity.PipeSprinklerFactoryNS;

namespace Model.Entity
{
    public static class PipeDictSprinklerFactoryUtil
    {
        private static RevitData revitData => RevitData.Instance;

        public static Dict<bool> GetConnectTypeCheckeds(this PipeDictSprinklerFactory q)
        {
            var qI = q.ConnectPipeTypes.Select(x => x == q.InitialConnectType).ToDict();
            qI.OnSetItem = (value, index) =>
            {
                q.U_ValueVisibility = q.GetU_ValueVisibility();
            };
            return qI;
        }

        // T => IsEnabled = false
        public static List<bool> GetConnectTypeEnables(this PipeDictSprinklerFactory q)
        {
            var connectType = q.InitialConnectType;
            return q.ConnectPipeTypes.Select(x => connectType == localNs.ConnectPipeType.T ? false : x != localNs.ConnectPipeType.T).ToList();
        }

        public static List<localNs.ConnectPipeType> GetConnectPipeTypes(this PipeDictSprinklerFactory q)
        {
            return Enum.GetValues(typeof(localNs.ConnectPipeType)).Cast<localNs.ConnectPipeType>().ToList();
        }

        public static System.Windows.Visibility GetU_ValueVisibility(this PipeDictSprinklerFactory q)
        {
            return q.ConnectPipeType == localNs.ConnectPipeType.U ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        public static PipeSprinklerFactory GetInitFactory(this PipeDictSprinklerFactory q)
        {
            return new PipeSprinklerFactory
            {
                Sprinkler = q.Sprinklers![0],
                MainPipe = q.MainPipes![0],
                // Dict = q
            };
        }

        #region Method

        public static void Do(this PipeDictSprinklerFactory q)
        {
            var doc = revitData.Document;

            using (var tx = new Transaction(doc, "Create Sprinkler Pipe"))
            {
                tx.Start();

                var sprinklers = q.Sprinklers!;
                //var pipes = new List<Pipe>() { q.MainPipe! };
                var pipes = q.MainPipes!;

                foreach (var sprinkler in sprinklers)
                {
                    Pipe? pipe = null;
                    if (pipes.Count == 1)
                    {
                        pipe = pipes[0];
                    }
                    else
                    {
                        var origin = (sprinkler.Location as LocationPoint)!.Point;
                        pipe = pipes.FirstOrDefault(p =>
                        {
                            var pipeLine = ((p.Location as LocationCurve)!.Curve as Line)!;
                            var projectPoint = pipeLine.GetProjectPoint(origin);

                            var point1 = pipeLine.GetEndPoint(0);
                            var point2 = pipeLine.GetEndPoint(1);

                            return (projectPoint - point1).IsOppositeDirection(projectPoint - point2);
                        });
                    }

                    if (pipe == null)
                    {
                        continue;
                    }

                    var itemFactory = new PipeSprinklerFactory
                    {
                        Sprinkler = sprinkler,
                        MainPipe = pipe,
                        Dict = q
                    };
                    itemFactory.Create();

                    pipes.Remove(pipe);
                    pipes.AddRange(itemFactory.MainPartialPipes);
                }

                tx.Commit();
            };
        }

        #endregion
    }
}
