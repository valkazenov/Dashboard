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
using System.Windows.Shapes;
using Utilities;
using Dashboard.Models;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for AddChartWindow.xaml
    /// </summary>
    public partial class AddChartWindow : Window
    {
        public AddChartWindow(TargetChartItem item, bool forEdit): base()
        {
            InitializeComponent();
            var model = new ChartEditModel(item, forEdit);
            model.Load();
            AddChartForm.DataContext = model;
            CommonHelper.InitValidation(NameInput, CoeffInput, ColorComboBox);
            NameInput.Focus();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = CommonHelper.ValidateBindedFileds(Model, NameInput, CoeffInput, ColorComboBox);
            if (result)
            {
                Model.Save();
                DialogResult = true;
                Close();
            }
        }

        private ChartEditModel Model
        {
            get { return AddChartForm.DataContext as ChartEditModel; }
        }

        private MainWindow MainWindow
        {
            get { return Application.Current.MainWindow as MainWindow; }
        }

    }
}
