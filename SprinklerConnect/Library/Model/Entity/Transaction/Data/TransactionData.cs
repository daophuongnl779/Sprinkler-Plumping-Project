using Autodesk.Revit.DB;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleData
{
    public class TransactionData
    {
        private static TransactionData? instance;
        public static TransactionData Instance
        {
            get => instance ??= new TransactionData();
            set => instance = value;
        }

        private Transaction? currentTransaction;
        public Transaction? CurrentTransaction
        {
            get => this.currentTransaction;
            set
            {
                this.currentTransaction = value;
                this.State = value != null ? TransactionState.Pending : TransactionState.Finish;
            }
        }

        public TransactionState State { get; set; } = TransactionState.NotSet;
    }
}
