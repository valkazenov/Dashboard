using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DatabaseContext;
using Utilities;

namespace Utilities
{
    public delegate void EntityLoadAction(MainContext context);

    public delegate void EntitySaveAction(MainContext context, TransactionScope transaction);

    public abstract class BaseEntityModel : IDataErrorInfo, INotifyPropertyChanged
    {
        protected bool connectNeed;

        public BaseEntityModel()
        {
            connectNeed = true;
            Context = null;
            Transaction = null;
        }

        protected DisposableContainer ConditialNewContext(out MainContext context)
        {
            if (connectNeed)
                context = Context == null ? CommonHelper.CreateMainContext() : Context;
            else context = null;
            var result = new DisposableContainer();
            result.DisposeNeed = connectNeed && Context == null;
            result.Object = context;
            return result;
        }

        protected DisposableContainer ConditialNewTransaction(out TransactionScope transaction)
        {
            if (connectNeed)
                transaction = Transaction == null ? CommonHelper.CreateTransaction() : Transaction;
            else transaction = null;
            var result = new DisposableContainer();
            result.DisposeNeed = connectNeed && Transaction == null;
            result.Object = transaction;
            return result;
        }

        protected void ConditionalTransactionComplete(DisposableContainer transactionContainer)
        {
            if (transactionContainer.DisposeNeed)
                (transactionContainer.Object as TransactionScope).Complete();
        }

        protected void ConditionalContextSave(DisposableContainer contextContainer)
        {
            if (contextContainer.DisposeNeed)
                CommonHelper.SaveContextChanges(contextContainer.Object as MainContext);
        }

        protected void ExecuteLoadAction(EntityLoadAction action)
        {
            MainContext context;
            using (var contextContainer = ConditialNewContext(out context))
                action(context);
        }

        protected void ExecuteSaveAction(EntitySaveAction action)
        {
            TransactionScope transaction;
            using (var transactionContainer = ConditialNewTransaction(out transaction))
            {
                MainContext context;
                using (var contextContainer = ConditialNewContext(out context))
                {
                    action(context, transaction);
                    ConditionalContextSave(contextContainer);
                }
                ConditionalTransactionComplete(transactionContainer);
            }
        }

        protected virtual void InternalLoad(MainContext context)
        {
            throw new NotImplementedException();
        }

        protected virtual void InternalSave(MainContext context, TransactionScope transaction)
        {
            throw new NotImplementedException();
        }

        public virtual void Load()
        {
            ExecuteLoadAction(InternalLoad);
        }

        public virtual void Save()
        {
            ExecuteSaveAction(InternalSave);
        }

        protected bool IsColumn(string columnName, string value)
        {
            return String.Compare(columnName, value, true) == 0;
        }

        public virtual string CheckField(string columnName)
        {
            return null;
        }

        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get { return CheckField(columnName); }
        }

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        public MainContext Context { get; set; }
        public TransactionScope Transaction { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
