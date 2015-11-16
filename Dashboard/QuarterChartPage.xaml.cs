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

        public void Refresh()
        {
            var lastItem = viewModel.ChartDataList.Last();
            var model = new QuarterChartModel();
            model.Year = viewModel.WorkDate.Year;
            model.Quarter = viewModel.Quarter;
            model.FactSumm = lastItem.TotalFactSumm;
            model.FactColor = lastItem.TargetColor;
            model.FactLevel = lastItem.TargetName;
            model.PlanSumm = lastItem.TotalPlanSumm;
            model.PlanColor = viewModel.Target != null ? CommonHelper.StringToColor(viewModel.Target.Color) : default(Color);
            QuarterForm.DataContext = model;
            BarSeries.ItemsSource = model.Columns;
        }
    }
}
