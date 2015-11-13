using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DatabaseContext;

namespace Utilities
{
    public class BaseEditModel : BaseEntityModel
    {
        public BaseEditModel()
            : base()
        {
            Id = -1;
        }

        protected virtual void InternalDelete(MainContext context, TransactionScope transaction)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            ExecuteSaveAction(InternalDelete);
        }

        public int Id { get; set; }
    }
}
