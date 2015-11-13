using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dashboard.Models
{
    public class TargetBandItem
    {
        public double StartSumm { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
    }

    public class QuarterChartColumn
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public Color Color { get; set; }
        public string Level { get; set; }
    }

    public class QuarterChartModel
    {
        public QuarterChartModel()
        {
            Quarter = 0;
            Year = 0;
            WorkDate = default(DateTime);
            PlanSumm = 0;
            FactSumm = 0;
            PlanColor = default(Color);
            BandItems = null;
        }

        private void SplitPlanSummByMonths(double summ, out double summ1, out double summ2, out double summ3)
        {
            var totalCount = Month1Weight + Month2Weight + Month3Weight;
            summ1 = Math.Round(summ / totalCount * Month1Weight, 2);
            summ -= summ1; totalCount -= Month1Weight;
            summ2 = Math.Round(summ / totalCount * Month2Weight, 2);
            summ3 = summ - summ2;
        }

        private int GetWorkDays(int year, int month, int day)
        {
            return day;
        }

        private int GetMonthWorkDays(int year, int month)
        {
            return GetWorkDays(year, month, DateTime.DaysInMonth(year, month));
        }

        private double CalcCurrentPlanSumm(double summ)
        {
            var startMonth = (Quarter - 1) * 3;
            double summ1; double summ2; double summ3;
            SplitPlanSummByMonths(summ, out summ1, out summ2, out summ3);
            int month = WorkDate.Month - startMonth;
            int day = WorkDate.Day;
            var coeff = (double)GetWorkDays(Year, startMonth + month, day) / GetMonthWorkDays(Year, startMonth + month);
            if (month == 1)
                return Math.Round(summ1 * coeff, 2);
            else if (month == 2)
                return summ1 + Math.Round(summ2 * coeff, 2);
            else return summ1 + summ2 + Math.Round(summ3 * coeff, 2);
        }

        public int Quarter { get; set; }
        public int Year { get; set; }
        public DateTime WorkDate { get; set; }
        public double PlanSumm { get; set; }
        public double FactSumm { get; set; }
        public Color PlanColor { get; set; }
        public double Month1Weight { get; set; }
        public double Month2Weight { get; set; }
        public double Month3Weight { get; set; }
        public List<TargetBandItem> BandItems { get; set; }

        public string ChartTitle
        {
            get
            {
                var quarterSuffix = Quarter == 1 ? "st" : Quarter == 2 ? "nd" : Quarter == 3 ? "rd" : "th";
                return String.Format("{0}{1} Quarter {2}", Quarter, quarterSuffix, Year);
            }
        }

        public List<QuarterChartColumn> Columns
        {
            get
            {
                return new List<QuarterChartColumn>()
                {
                    new QuarterChartColumn() { Name = "Actual Sales", Value = Convert.ToInt32(FactSumm), Color = CurrentFactColor, Level=CurrentFactName },
                    new QuarterChartColumn() { Name = "Target", Value = Convert.ToInt32(CurrentPlanSumm), Color = PlanColor }
                };
            }
        }

        public int AxisInterval
        {
            get
            {
                var list = new List<int>() { 1, 2, 5, 10 };
                var max = CurrentPlanSumm > FactSumm ? CurrentPlanSumm : FactSumm;
                var result = max / 10;
                var multiplier = 1;
                while (result >= 10)
                {
                    result /= 10;
                    multiplier *= 10;
                }
                return list.Where(i => i >= result).OrderBy(i => i).First() * multiplier;
            }
        }

        public double CurrentPlanSumm
        {
            get { return CalcCurrentPlanSumm(PlanSumm); }
        }

        public Color CurrentFactColor
        {
            get
            {
                if (BandItems != null)
                {
                    foreach (var item in BandItems)
                        if (FactSumm >= CalcCurrentPlanSumm(item.StartSumm))
                            return item.Color;
                }
                return default(Color);
            }
        }

        public string CurrentFactName
        {
            get
            {
                if (BandItems != null)
                {
                    foreach (var item in BandItems)
                        if (FactSumm >= CalcCurrentPlanSumm(item.StartSumm))
                            return item.Name;
                }
                return "";
            }
        }
    }
}
