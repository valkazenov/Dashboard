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

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for DayChartPage.xaml
    /// </summary>
    public partial class DayChartPage : Page
    {
        private ChartViewModel model;


        public DayChartPage(ChartViewModel model)
        {
            InitializeComponent();
            this.model = model;
        }

        public void Refresh()
        {
            DateLabel.Text = model.WorkDate.ToString();
            PlanLabel.Text = model.Target != null ? model.Target.Summ.ToString() : "";
        }
    }
}
