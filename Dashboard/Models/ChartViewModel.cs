using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Data;
using System.Data.SqlClient;
using DatabaseContext;
using Utilities;

namespace Dashboard.Models
{
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
                                result.Add(date, summ);
                            else result[date] = result[date] + summ;
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
            using (var context = new MainContext())
                return context.Targets.Where(t => t.Year == WorkDate.Year && t.Quarter == quarter).FirstOrDefault();
        }

        private List<TargetChart> DoLoadTargetCharts()
        {
            using (var context = new MainContext())
                return context.TargetCharts.Where(c => c.TargetId == Target.Id).ToList();
        }

        public async void Calculate()
        {
            if (OnBeginCalculate != null) OnBeginCalculate();
            RealSales = await Task.Run<Dictionary<DateTime, double>>(() => DoLoadRealSales());
            Target = await Task.Run<Target>(() => DoLoadTarget());
            if (Target != null)
                TargetCharts = await Task.Run<List<TargetChart>>(() => DoLoadTargetCharts());
            else TargetCharts = null;
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
        
        public event Action OnBeginCalculate;
        public event Action OnEndCalculate;
    }
}
