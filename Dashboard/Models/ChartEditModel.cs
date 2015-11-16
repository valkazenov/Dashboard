using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Media;
using DatabaseContext;
using Utilities;

namespace Dashboard.Models
{
    public class ChartEditModel: BaseEditModel
    {
        public ChartEditModel(TargetChartItem item, bool forEdit): base()
        {
            connectNeed = false;
            ForEdit = forEdit;
            Item = item;
            Name = "";
            Coeff = "";
            Color = null;
        }

        protected override void InternalLoad(MainContext context)
        {
            if (ForEdit)
            {
                Name = Item.Name;
                Coeff = Item.Coeff.ToString();
                Color = Item.Color;
            }
            else Coeff = "1";
        }

        protected override void InternalSave(MainContext context, TransactionScope transaction)
        {
            Item.Name = Name;
            Item.Coeff = double.Parse(Coeff);
            Item.Color = Color.Value;
        }

        public override string CheckField(string columnName)
        {
            if (IsColumn(columnName, "Name"))
            {
                if (String.IsNullOrEmpty(Name))
                    return "Name is required";
                else return null;
            }
            else if (IsColumn(columnName, "Coeff"))
            {
                double coeff;
                if (!double.TryParse(Coeff, out coeff))
                    return "Coefficient value is incorrect";
                if (coeff < 0)
                    return "Coefficient can be only positive number";
                else return null;
            }
            else if (IsColumn(columnName, "Color"))
            {
                if (Color == null)
                    return "Color is required";
                else return null;
            }
            return base.CheckField(columnName);
        }

        public TargetChartItem Item { get; private set; }
        public bool ForEdit { get; private set; }
        public string Name { get; set; }
        public string Coeff { get; set; }
        public Color? Color { get; set; }
    }
}
