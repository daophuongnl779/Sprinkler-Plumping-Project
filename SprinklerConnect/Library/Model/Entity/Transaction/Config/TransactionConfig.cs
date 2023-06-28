using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
    public class TransactionConfig
    {
        public string? Name { get; set; }

        public Action<Transaction>? OnCreatingTransaction { get; set; }

        public Action? Action { get; set; }
    }
}
