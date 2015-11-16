using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.DataVisualization.Charting;
using Dashboard.Models;
using Utilities;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for DayChartPage.xaml
    /// </summary>
    public partial class DayChartPage : Page
    {
        private ChartViewModel viewModel;

        public DayChartPage(ChartViewModel model)
        {
            InitializeComponent();
            viewModel = model;
        }

        public void Refresh()
        {
            var lastItem = viewModel.ChartDataList.Last();
            var model = new DayChartModel();
            model.Year = viewModel.WorkDate.Year;
            model.Quarter = viewModel.Quarter;
            model.PlanColor = viewModel.Target != null ? CommonHelper.StringToColor(viewModel.Target.Color) : default(Color);
            model.FactColor = lastItem.TargetColor;
            model.FactLevel = lastItem.TargetName;
            model.PlanColumns = viewModel.ChartDataList.Select(i => new DayChartColumn() { Date = i.Day.ToString("dd.MM"), Percent = i.TotalPlanPercent }).ToList();
            model.FactColumns = viewModel.ChartDataList.Select(i => new DayChartColumn() { Date = i.Day.ToString("dd.MM"), Percent = i.TotalFactPercent }).ToList();
            model.YAxisInterval = Math.Max(lastItem.TotalPlanPercent, lastItem.TotalFactPercent) > 10 ? 10 : 1;
            DayForm.DataContext = model;
            PlanSeries.ItemsSource = model.PlanColumns;
            FactSeries.ItemsSource = model.FactColumns;
            /*XAxis.IntervalType = DateTimeIntervalType.Days;
            XAxis.Interval = model.XAxisInterval;*/
            YAxis.Interval = model.YAxisInterval;
            PlanSeries.Background = new SolidColorBrush(model.PlanColor);
            FactSeries.Background = new SolidColorBrush(model.FactColor);
        }
    }
}
