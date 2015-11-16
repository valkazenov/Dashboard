using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Media;
using DatabaseContext;
using Utilities;

namespace Dashboard.Models
{
    public class ChartDataItem
    {
        public DateTime Day { get; set; }
        public double FactSumm { get; set; }
        public double TotalFactSumm { get; set; }
        public double FactPercent { get; set; }
        public double TotalFactPercent { get; set; }
        public double PlanSumm { get; set; }
        public double TotalPlanSumm { get; set; }
        public double PlanPercent { get; set; }
        public double TotalPlanPercent { get; set; }
        public string TargetName { get; set; }
        public Color TargetColor { get; set; }
    }

    public class ChartViewModel : BaseEntityModel
    {
        private ETargetDateType dateType;
        private DateTime date;

        public ChartViewModel()
            : base()
        {
            DateTypeList = null;
            dateType = ETargetDateType.Current;
            date = default(DateTime);
            RealSales = null;
            Target = null;
            TargetCharts = null;
            CalendarItems = null;
            ChartDataList = null;
        }

        private List<ETargetDateType> CreateDateTypeList()
        {
            return new List<ETargetDateType>() { ETargetDateType.Current, ETargetDateType.Selected };
        }

        protected override void InternalLoad(MainContext context)
        {
            DateTypeList = CreateDateTypeList();
            var type = Properties.Settings.Default["ChartDateType"] as ETargetDateType?;
            if (type != null)
            {
                dateType = type.Value;
                date = Properties.Settings.Default.ChartDate;
            }
            else
            {
                dateType = ETargetDateType.Current;
                date = DateTime.Now;
            }
        }

        protected override void InternalSave(MainContext context, TransactionScope transaction)
        {
            Properties.Settings.Default.ChartDateType = DateType;
            Properties.Settings.Default.ChartDate = Date;
        }

        private DateTime GetQuarterStartDate()
        {
            var quarter = (WorkDate.Month + 2) / 3;
            var month = (quarter - 1) * 3 + 1;
            return new DateTime(WorkDate.Year, month, 1);
        }

        private Dictionary<DateTime, double> DoLoadRealSales()
        {
            var result = new Dictionary<DateTime, double>();
            var startDate = GetQuarterStartDate();
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = CommonHelper.GetConnectionString("RealDatabase");
                connection.Open();
                var command = new SqlCommand("[Sales].[SalesOrderAuditLogSelect]", connection);
                command.Parameters.AddWithValue("@CustomerIDFrom", DBNull.Value);
                command.Parameters.AddWithValue("@CustomerIDTo", DBNull.Value);
                command.Parameters.AddWithValue("@EmployeeIDFrom", DBNull.Value);
                command.Parameters.AddWithValue("@EmployeeIDTo", DBNull.Value);
                command.Parameters.AddWithValue("@periodstart", startDate);
                command.Parameters.AddWithValue("@periodend", WorkDate);
                command.CommandType = CommandType.StoredProcedure;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var date = DateTime.Parse((string)reader["ActionDate"]);
                        var summ = Convert.ToDouble((Decimal)reader["ValueInBase"]);
                        if (summ != 0)
                        {
                            if (!result.ContainsKey(date))
                                result.Add(date, 0);
                            result[date] = Math.Round(result[date] + summ, 2);
                        }
                    }
                }
                connection.Close();
            }
            return result;
        }

        private Target DoLoadTarget()
        {
            var quarter = (WorkDate.Month + 2) / 3;
            using (var context = CommonHelper.CreateMainContext())
                return context.Targets.Where(t => t.Year == WorkDate.Year && t.Quarter == quarter).FirstOrDefault();
        }

        private List<TargetChart> DoLoadTargetCharts()
        {
            using (var context = CommonHelper.CreateMainContext())
                return context.TargetCharts.Where(c => c.TargetId == Target.Id).ToList();
        }

        private Dictionary<DateTime, bool> DoLoadCalendarItems()
        {
            using (var context = CommonHelper.CreateMainContext())
            {
                var result = new Dictionary<DateTime, bool>();
                foreach (var item in context.WorkCalendar)
                    result.Add(item.Day,item.IsHoliday);
                return result;
            }
        }

        private List<DateTime> GetMonthWorkDays(int month)
        {
            var result = new List<DateTime>();
            for (var i = 1; i <= DateTime.DaysInMonth(WorkDate.Year, month); i++)
            {
                var day = new DateTime(WorkDate.Year, month, i);
                bool addKey;
                if (CalendarItems.ContainsKey(day))
                    addKey = !CalendarItems[day];
                else addKey = day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday;
                if (addKey) result.Add(day);
            }
            return result;
        }

        protected void SplitSummByMonths(double summ, out double summ1, out double summ2, out double summ3)
        {
            summ1 = 0; summ2 = 0; summ3 = 0;
            if (summ == 0) return;
            var totalCount = Target.Month1Weight + Target.Month2Weight + Target.Month3Weight;
            summ1 = Math.Round(summ / totalCount * Target.Month1Weight, 2);
            summ -= summ1; totalCount -= Target.Month1Weight;
            summ2 = Math.Round(summ / totalCount * Target.Month2Weight, 2);
            summ3 = summ - summ2;
        }

        private void FillTargetParams(ChartDataItem item)
        {
            if (Target == null) return;
            item.TargetName = null; item.TargetColor = Colors.Transparent;
            var coeff = item.TotalFactSumm / item.TotalPlanSumm;
            foreach (var addTarget in TargetCharts.OrderByDescending(c => c.Coeff).ToList())
            {
                if (coeff >= addTarget.Coeff)
                {
                    item.TargetName = addTarget.Name;
                    item.TargetColor = CommonHelper.StringToColor(addTarget.Color);
                    break;
                }
            }
        }

        private List<ChartDataItem> CalculateChartDataList()
        {
            var result = new List<ChartDataItem>();
            var targetSumm = Target != null ? Convert.ToDouble(Target.Summ) : 0;
            double planSumm1, planSumm2, planSumm3;
            SplitSummByMonths(targetSumm, out planSumm1, out planSumm2, out planSumm3);
            var startMonth = (Quarter - 1) * 3;
            var totalFactSumm = 0.0; var prevTotalFactPercent = 0.0;
            var totalPlanSumm = 0.0; var prevTotalPlanPercent = 0.0;
            for (var i = startMonth + 1; i <= WorkDate.Month; i++)
            {
                var monthPlanSumm = i == startMonth + 1 ? planSumm1 : i == startMonth + 2 ? planSumm2 : planSumm3;
                var stopDay = i == WorkDate.Month ? WorkDate.Day : DateTime.DaysInMonth(WorkDate.Year, i);
                var days = GetMonthWorkDays(i);
                var dayCount = 0;
                foreach (var day in days)
                {
                    if (day.Day <= stopDay)
                    {
                        // Fact
                        var item = new ChartDataItem() { Day = day };
                        item.FactSumm = RealSales.ContainsKey(day) ? RealSales[day] : 0;
                        totalFactSumm = Math.Round(totalFactSumm + item.FactSumm, 2);
                        item.TotalFactSumm = totalFactSumm;
                        item.TotalFactPercent = targetSumm != 0 ? Math.Round(item.TotalFactSumm / targetSumm * 100, 2) : 0;
                        item.FactPercent = Math.Round(item.TotalFactPercent - prevTotalFactPercent, 2);
                        prevTotalFactPercent = item.TotalFactPercent;
                        // Plan
                        item.PlanSumm = Math.Round(monthPlanSumm / (days.Count - dayCount), 2);
                        monthPlanSumm = Math.Round(monthPlanSumm - item.PlanSumm, 2);
                        totalPlanSumm = Math.Round(totalPlanSumm + item.PlanSumm, 2);
                        item.TotalPlanSumm = totalPlanSumm;
                        item.TotalPlanPercent = targetSumm != 0 ? Math.Round(item.TotalPlanSumm / targetSumm * 100, 2) : 0;
                        item.PlanPercent = Math.Round(item.TotalPlanPercent - prevTotalPlanPercent, 2);
                        prevTotalPlanPercent = item.TotalPlanPercent;
                        // Target
                        FillTargetParams(item);
                        result.Add(item);
                        dayCount++;
                    }
                }
            }
            return result;
        }

        public async void Calculate()
        {
            if (OnBeginCalculate != null) OnBeginCalculate();
            RealSales = await Task.Run<Dictionary<DateTime, double>>(() => DoLoadRealSales());
            Target = await Task.Run<Target>(() => DoLoadTarget());
            if (Target != null)
                TargetCharts = await Task.Run<List<TargetChart>>(() => DoLoadTargetCharts());
            else TargetCharts = null;
            CalendarItems = await Task.Run<Dictionary<DateTime, bool>>(() => DoLoadCalendarItems());
            ChartDataList = CalculateChartDataList();
            if (OnEndCalculate != null) OnEndCalculate();
        }

        public List<ETargetDateType> DateTypeList { get; private set; }
        public ETargetDateType DateType
        {
            get { return dateType; }
            set
            {
                if (dateType != value)
                {
                    dateType = value;
                    NotifyPropertyChanged();
                    Calculate();
                }
            }
        }

        public DateTime Date
        {
            get { return date; }
            set
            {
                if (date != value)
                {
                    date = value;
                    NotifyPropertyChanged();
                    Calculate();
                }
            }
        }

        public DateTime WorkDate
        {
            get { return DateType == ETargetDateType.Selected ? Date : DateTime.Now; }
        }

        public int Quarter
        {
            get { return (WorkDate.Month + 2) / 3; }
        }

        public Dictionary<DateTime, double> RealSales { get; private set; }
        public Target Target { get; private set; }
        public List<TargetChart> TargetCharts { get; private set; }
        public List<ChartDataItem> ChartDataList { get; private set; }
        public Dictionary<DateTime, bool> CalendarItems { get; private set; }

        public event Action OnBeginCalculate;
        public event Action OnEndCalculate;
    }
}
