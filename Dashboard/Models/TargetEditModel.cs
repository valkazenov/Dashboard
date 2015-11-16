using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Globalization;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;
using EntityFramework.Extensions;
using System.Runtime.CompilerServices;
using DatabaseContext;
using Utilities;

namespace Dashboard.Models
{
    public class TargetChartItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string name;
        private double coeff;
        private Color color;

        public int Id { get; set; }
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }
        public double Coeff 
        {
            get { return coeff; }
            set { coeff = value; NotifyPropertyChanged(); }
        }
        public string ColorText { get; set; }
        public Color Color
        {
            get { return color; }
            set { color = value; NotifyPropertyChanged(); }
        }
    }

    public class TargetEditModel : BaseEditModel
    {
        private int quarter;

        public TargetEditModel()
        {
            Year = 0;
            quarter = 0;
            Summ = "";
            Month1Weight = "";
            Month2Weight = "";
            Month3Weight = "";
            Color = null;
            YearList = null;
            QuarterList = null;
            AddChartList = null;
        }

        private List<int> CreateYearList()
        {
            var result = new List<int>();
            var year = DateTime.Now.Year;
            for (var i = year - 10; i <= year + 10; i++)
                result.Add(i);
            return result;
        }

        private List<int> CreateQuarterList()
        {
            return new List<int>() { 1, 2, 3, 4 };
        }

        protected override void InternalLoad(MainContext context)
        {
            YearList = CreateYearList();
            QuarterList = CreateQuarterList();
            if (Id > 0)
            {
                var target = context.Targets.Where(t => t.Id == Id).Single();
                Year = target.Year;
                Quarter = target.Quarter;
                Summ = target.Summ.ToString();
                Month1Weight = target.Month1Weight.ToString();
                Month2Weight = target.Month2Weight.ToString();
                Month3Weight = target.Month3Weight.ToString();
                Color = CommonHelper.StringToColor(target.Color);
                AddChartList = new ObservableCollection<TargetChartItem>(context.TargetCharts.Where(c => c.TargetId == Id).OrderBy(c => c.Coeff).Select(c => new TargetChartItem()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Coeff = c.Coeff,
                    ColorText = c.Color
                }));
                foreach (var item in AddChartList)
                    item.Color = CommonHelper.StringToColor(item.ColorText);
            }
            else
            {
                Year = DateTime.Now.Year;
                Quarter = (DateTime.Now.Month + 2) / 3;
                AddChartList = new ObservableCollection<TargetChartItem>();
            }
        }

        private void SaveTargetCharts(MainContext context, int targetId)
        {
            if (AddChartList.Select(c => c.Name).Count() != AddChartList.Select(c => c.Name).Distinct().Count())
                throw new ApplicationException("Names of additional targets should be unique");
            context.TargetCharts.Where(c => c.TargetId == targetId).Delete();
            var counter = 0;
            foreach (var item in AddChartList)
            {
                var chart = context.TargetCharts.Create();
                chart.TargetId = targetId;
                chart.Name = item.Name;
                chart.Coeff = item.Coeff;
                chart.Color = CommonHelper.ColorToString(item.Color);
                chart.Sort = counter;
                context.TargetCharts.Add(chart);
                counter++;
            }
        }

        protected override void InternalSave(MainContext context, TransactionScope transaction)
        {
            if (context.Targets.Any(t => t.Year == Year && t.Quarter == Quarter && t.Id != Id))
                throw new ApplicationException("Target for this year and quarter already exists");
            Target target;
            if (Id <= 0)
                target = context.Targets.Create();
            else target = context.Targets.Where(t => t.Id == Id).Single();
            target.Id = Id;
            target.Year = Year;
            target.Quarter = Quarter;
            target.Summ = decimal.Parse(Summ);
            target.Month1Weight = double.Parse(Month1Weight);
            target.Month2Weight = double.Parse(Month2Weight);
            target.Month3Weight = double.Parse(Month3Weight);
            target.Color = CommonHelper.ColorToString(Color.Value);
            if (Id <= 0)
            {
                context.Targets.Add(target);
                context.SaveChanges();
            }
            SaveTargetCharts(context, target.Id);
        }

        protected override void InternalDelete(MainContext context, TransactionScope transaction)
        {
            context.Targets.Where(t => t.Id == Id).Delete();
        }

        public override string CheckField(string columnName)
        {
            if (IsColumn(columnName, "Summ"))
            {
                decimal summ;
                if (!decimal.TryParse(Summ, out summ))
                    return "Summ value is incorrect";
                if (summ < 0)
                    return "Summ cannot be negative number";
                else return null;
            }
            else if (IsColumn(columnName, "Month1Weight") || IsColumn(columnName, "Month2Weight") || IsColumn(columnName, "Month3Weight"))
            {
                var value = IsColumn(columnName, "Month1Weight") ? Month1Weight : IsColumn(columnName, "Month2Weight") ? Month2Weight : Month3Weight;
                double weight;
                if (!double.TryParse(value, out weight))
                    return "Weight value is incorrect";
                if (weight <= 0)
                    return "Weight can be only positive number";
                else return null;
            }
            else if (IsColumn(columnName, "Color"))
            {
                if (Color == null)
                    return "Color is required";
                else return null;
            }
            else return base.CheckField(columnName);
        }

        private string GetMonthName(int number)
        {
            var month = (Quarter - 1) * 3 + number;
            switch (month)
            {
                case 1: return "January";
                case 2: return "February";
                case 3: return "March";
                case 4: return "April";
                case 5: return "May";
                case 6: return "June";
                case 7: return "July";
                case 8: return "August";
                case 9: return "September";
                case 10: return "October";
                case 11: return "November";
                case 12: return "December";
            }
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }

        public void SortAddChartList()
        {
            if (AddChartList != null)
            {
                var list = AddChartList.OrderBy(i => i.Coeff).ToList();
                AddChartList.Clear();
                foreach (var item in list)
                    AddChartList.Add(item);
            }
        }

        public int Year { get; set; }
        public int Quarter
        {
            get { return quarter; }
            set
            {
                if (quarter != value)
                {
                    quarter = value;
                    NotifyPropertyChanged("Month1Name");
                    NotifyPropertyChanged("Month2Name");
                    NotifyPropertyChanged("Month3Name");
                }

            }
        }
        public string Summ { get; set; }
        public string Month1Weight { get; set; }
        public string Month2Weight { get; set; }
        public string Month3Weight { get; set; }
        public Color? Color { get; set; }
        public List<int> YearList { get; private set; }
        public List<int> QuarterList { get; private set; }
        public string Month1Name
        {
            get { return GetMonthName(1) + " weight"; }
        }
        public string Month2Name
        {
            get { return GetMonthName(2) + " weight"; }
        }
        public string Month3Name
        {
            get { return GetMonthName(3) + " weight"; }
        }
        public ObservableCollection<TargetChartItem> AddChartList { get; private set; }
    }
}
