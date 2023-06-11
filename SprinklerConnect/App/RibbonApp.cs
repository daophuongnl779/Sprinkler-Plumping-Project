using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SingleData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Model.Application
{
    public class RibbonApp : IExternalApplication
    {
        private RibbonData ribbonData => RibbonData.Instance;
        private IOData ioData => IOData.Instance;

        public Result OnStartup(UIControlledApplication application)
        {
            //application.Idling += Application_Idling;
            //ioData.IconDirectoryPath = Path.Combine(Directory.GetParent(ioData.AssemblyDirectoryPath).FullName, "Icon");
            ribbonData.Application = application;

            var nsp = "Model.RevitCommand";

            var hbTab = EntTabUtil.Get("BiMHoaBinh");
            var quanPanel = hbTab.GetPanel("Database");
            quanPanel.GetPushButton("Xuất dữ liệu", $"{nsp}.PMCommand", "bim");

            hbTab.CreateTab();

            return Result.Succeeded;
        }

        //private void Application_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        //{
        //    boQExcelLinkData.SelectElementByActiveCell();
        //}

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
