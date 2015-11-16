using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using Utilities;

namespace Dashboard.Models
{
    [ValueConversion(typeof(ETargetDateType), typeof(Visibility))]
    public class DateTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((ETargetDateType)value == (ETargetDateType)Enum.Parse(typeof(ETargetDateType), (String)parameter))
                return Visibility.Visible;
            else return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DateTime), typeof(string))]
    public class CalendarDayConverter : IValueConverter
    {
        private static Dictionary<DateTime, bool> calendarItems;

        static CalendarDayConverter()
        {
            calendarItems = null;
        }

        private static void LoadCalendarItems()
        {
            calendarItems = new Dictionary<DateTime, bool>();
            try
            {
                using (var context = CommonHelper.CreateMainContext())
                {
                    foreach (var item in context.WorkCalendar.Select(c => new { c.Day, c.IsHoliday }))
                        calendarItems.Add(item.Day, item.IsHoliday);
                }
            }
            catch { }
        }

        public static void UpdateItem(DateTime date, bool isHoliday)
        {
            if (calendarItems == null) return;
            if (!calendarItems.ContainsKey(date))
                calendarItems.Add(date, isHoliday);
            else calendarItems[date] = isHoliday;
        }

        public static void DeleteItem(DateTime date)
        {
            if (calendarItems.ContainsKey(date))
                calendarItems.Remove(date);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (calendarItems == null) LoadCalendarItems();
            var date = (DateTime)value;
            if (calendarItems.ContainsKey(date))
                return calendarItems[date] ? "Holiday" : "Workday";
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return "Weekday";
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
