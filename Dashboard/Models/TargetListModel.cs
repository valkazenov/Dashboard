using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using DatabaseContext;

namespace Dashboard.Models
{
    public class TargetListItem
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal Summ { get; set; }
        public double Month1Weight { get; set; }
        public double Month2Weight { get; set; }
        public double Month3Weight { get; set; }
        public string ColorText { get; set; }
        public System.Windows.Media.Color Color { get; set; }
    }

    public class TargetListModel : BaseListModel<TargetListItem>
    {
        public TargetListModel()
        {
            SortName = "Year";
            ByDescending = true;
        }

        protected override List<TargetListItem> InternalCreateItems(MainContext context)
        {
            var list = context.Targets as IQueryable<Target>;
            Func<Target, object> func;
            switch (SortName.ToLower())
            {
                case "quarter": func = t => t.Quarter; break;
                case "summ": func = t => t.Summ; break;
                case "month1weight": func = t => t.Month1Weight; break;
                case "month2weight": func = t => t.Month2Weight; break;
                case "month3weight": func = t => t.Month3Weight; break;
                default: func = t => t.Year; break;
            }
            var sorted = ByDescending ? list.OrderByDescending(func) : list.OrderBy(func);
            if (SortName.ToLower() == "year")
                sorted = ByDescending ? sorted.ThenByDescending(t => t.Quarter) : sorted.ThenBy(t => t.Quarter);
            else if (SortName.ToLower() == "quarter")
                sorted = ByDescending ? sorted.ThenByDescending(t => t.Year) : sorted.ThenBy(t => t.Year);
            return sorted.Select(t => new TargetListItem()
            {
                Id = t.Id,
                Year = t.Year,
                Quarter = t.Quarter,
                Summ = t.Summ,
                Month1Weight = t.Month1Weight,
                Month2Weight = t.Month2Weight,
                Month3Weight = t.Month3Weight,
                ColorText = t.Color
            }).ToList();
        }

        protected override void AfterLoad(MainContext context)
        {
            foreach (var item in Items)
            {
                var drawingColor = System.Drawing.ColorTranslator.FromHtml(item.ColorText);
                item.Color = System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
            }
        }

    }
}
