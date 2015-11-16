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
using System.Windows.Threading;
using System.Windows.Markup;
using Dashboard.Models;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private ChartViewModel model;
        private QuarterChartPage quarterPage;
        private DayChartPage dayPage;
        private DispatcherTimer refreshTimer;

        public HomePage()
        {
            InitializeComponent();
            refreshTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(Properties.Settings.Default.RefreshInterval) };
            refreshTimer.Tick += (s, e) => Model.Calculate();
            model = new ChartViewModel();
            model.OnBeginCalculate += BeginCalculate;
            model.OnEndCalculate += EndCalculate;
            quarterPage = new QuarterChartPage(model);
            dayPage = new DayChartPage(model);
            model.Load();
            ChartViewForm.DataContext = model;
            ChartContainer.Content = /*quarterPage*/dayPage;
            refreshTimer.Start();
        }

        private void SlideButton_Clik(object sender, RoutedEventArgs e)
        {
            if (ChartContainer.Content == quarterPage)
            {
                dayPage = new DayChartPage(model);
                dayPage.Refresh();
                ChartContainer.Content = dayPage;
            }
            else
            {
                quarterPage = new QuarterChartPage(model);
                quarterPage.Refresh();
                ChartContainer.Content = quarterPage;
            }
        }

        private void BeginCalculate()
        {
            WaitPanel.Visibility = Visibility.Visible;
        }

        private void EndCalculate()
        {
            WaitPanel.Visibility = Visibility.Collapsed;
            quarterPage.Refresh();
            dayPage.Refresh();
        }

        private MainWindow MainWindow
        {
            get { return Application.Current.MainWindow as MainWindow; }
        }

        public ChartViewModel Model
        {
            get { return ChartViewForm.DataContext as ChartViewModel; }
        }

    }
}
