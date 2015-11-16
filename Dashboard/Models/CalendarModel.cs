using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EntityFramework.Extensions;
using DatabaseContext;
using Utilities;

namespace Dashboard.Models
{
    public class CalendarModel : BaseEntityModel
    {
        public CalendarModel()
            : base()
        {
            Date = default(DateTime);
            IsHoliday = false;
        }

        protected override void InternalSave(MainContext context, TransactionScope transaction)
        {
            var weekDay = Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;
            var updateKey = IsHoliday != weekDay;
            int id = context.WorkCalendar.Where(c => c.Day == Date).Select(c => c.Id).FirstOrDefault();
            if (updateKey)
            {
                if (id <= 0)
                {
                    var item = context.WorkCalendar.Create();
                    item.Day = Date;
                    item.IsHoliday = IsHoliday;
                    context.WorkCalendar.Add(item);
                }
                else context.WorkCalendar.Where(c => c.Id == id).Update(c => new WorkCalendar() { IsHoliday = IsHoliday });
                CalendarDayConverter.UpdateItem(Date, IsHoliday);
            }
            else if (id > 0)
            {
                context.WorkCalendar.Where(c => c.Id == id).Delete();
                CalendarDayConverter.DeleteItem(Date);
            }
        }

        public DateTime Date { get; set; }
        public bool IsHoliday { get; set; }
    }
}
