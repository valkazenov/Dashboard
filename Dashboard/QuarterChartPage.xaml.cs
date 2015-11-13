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
using Dashboard.Models;
using Utilities;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for QuerterChartPage.xaml
    /// </summary>
    public partial class QuarterChartPage : Page
    {
        private ChartViewModel viewModel;

        public QuarterChartPage(ChartViewModel model)
        {
            InitializeComponent();
            viewModel = model;
        }

        private List<TargetBandItem> CreateBandItems()
        {
            var result = new List<TargetBandItem>();
            result.Add(new TargetBandItem() { StartSumm = 0, Color = viewModel.Target != null ? CommonHelper.StringToColor(viewModel.Target.StartColor) : Colors.Green });
            if (viewModel.Target != null)
            {
                result.Add(new TargetBandItem() { StartSumm = Convert.ToDouble(viewModel.Target.Summ), Color = CommonHelper.StringToColor(viewModel.Target.Color), Name = "Target" });
                foreach (var item in viewModel.TargetCharts)
                    result.Add(new TargetBandItem() { StartSumm = Math.Round(Convert.ToDouble(viewModel.Target.Summ) * item.Coeff, 2), Color = CommonHelper.StringToColor(item.Color), Name = item.Name });
            }
            return result.OrderByDescending(i => i.StartSumm).ToList();
        }

        public void Refresh()
        {
            var model = new QuarterChartModel();
            model.Year = viewModel.WorkDate.Year;
            model.Quarter = viewModel.Quarter;
            model.WorkDate = viewModel.WorkDate;
            model.PlanSumm = viewModel.Target != null ? Convert.ToDouble(viewModel.Target.Summ) : 0;
            model.FactSumm = viewModel.RealSales.Values.Sum();
            model.PlanColor = viewModel.Target != null ? CommonHelper.StringToColor(viewModel.Target.Color) : default(Color);
            model.Month1Weight = viewModel.Target != null ? viewModel.Target.Month1Weight : 1;
            model.Month2Weight = viewModel.Target != null ? viewModel.Target.Month2Weight : 1;
            model.Month3Weight = viewModel.Target != null ? viewModel.Target.Month3Weight : 1;
            model.BandItems = CreateBandItems();

            QuarterForm.DataContext = model;
            Test.ItemsSource = model.Columns;
        }
    }
}
