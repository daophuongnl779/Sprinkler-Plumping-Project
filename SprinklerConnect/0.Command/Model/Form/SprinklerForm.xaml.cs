using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using SingleData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utility;
using Model.Data;
using Model.Entity;

namespace Model.Form
{
    /// <summary>
    /// Interaction logic for PM_ProjectUC.xaml
    /// </summary>
    public partial class SprinklerForm : System.Windows.Window
    {
        private PipeSprinklerData data = PipeSprinklerData.Instance;
        private RevitData revitData = RevitData.Instance;

        public SprinklerForm()
        {
            InitializeComponent();
        }

        private void select_Clicked(object sender, RoutedEventArgs e)
        {
            //this.Hide();

            data.Select();

            //this.ShowDialog();
        }

        private void run_Clicked(object sender, RoutedEventArgs e)
        {
            //this.Close();
            //data.Run();

            // Kich hoat su kien
            revitData.ExternalEvent!.Raise();
        }
    }
}
