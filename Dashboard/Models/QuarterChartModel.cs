using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dashboard.Models
{
    public class QuarterChartColumn
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public Color Color { get; set; }
        public string Level { get; set; }
    }

    public class QuarterChartModel : BaseChartModel
    {
        public QuarterChartModel()
            : base()
        {
            FactSumm = 0;
            FactColor = default(Color);
            FactLevel = null;
            PlanSumm = 0;
        }

        public double FactSumm { get; set; }
        public Color FactColor { get; set; }
        public string FactLevel { get; set; }
        public double PlanSumm { get; set; }

        public List<QuarterChartColumn> Columns
        {
            get
            {
                return new List<QuarterChartColumn>()
                {
                    new QuarterChartColumn() { Name = "Actual Sales", Value = Convert.ToInt32(FactSumm), Color = FactColor, Level=FactLevel },
                    new QuarterChartColumn() { Name = "Target", Value = Convert.ToInt32(PlanSumm), Color = PlanColor }
                };
            }
        }

        public int AxisInterval
        {
            get
            {
                var list = new List<int>() { 1, 2, 5, 10 };
                var result = Math.Max(PlanSumm, FactSumm) / 10;
                var multiplier = 1;
                while (result >= 10)
                {
                    result /= 10;
                    multiplier *= 10;
                }
                return list.Where(i => i >= result).OrderBy(i => i).First() * multiplier;
            }
        }

    }
}
