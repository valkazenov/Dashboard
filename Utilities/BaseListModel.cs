using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseContext;

namespace Utilities
{
    public abstract class BaseListModel<TItem>: BaseEntityModel where TItem: class
    {
        public BaseListModel()
        {
            SortName = "";
            ByDescending = false;
            Items = null;
        }

        protected abstract List<TItem> InternalCreateItems(MainContext context);

        protected virtual void AfterLoad(MainContext context)
        { }

        protected override void InternalLoad(MainContext context)
        {
            Items = InternalCreateItems(context);
            AfterLoad(context);
        }

        public string SortName { get; set; }
        public bool ByDescending { get; set; }
        public List<TItem> Items { get; private set; }
    }
}
