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
using Utilities;
using Dashboard.Models;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for TargetDetailPage.xaml
    /// </summary>
    public partial class TargetDetailsPage : Page
    {
        private int targetId;

        public TargetDetailsPage(int targetId, bool forEdit)
        {
            InitializeComponent();
            this.targetId = targetId;
            var model = new TargetEditModel() { Id = forEdit ? targetId : -1 };
            model.Load();
            TargetEditForm.DataContext = model;
            YearComboBox.Focus();
            if (model.AddChartList.Count > 0)
                AddChartList.SelectedItem = model.AddChartList.First();
            CommonHelper.InitValidation(SummInput, Month1WeightInput, Month2WeightInput, Month3WeightInput, ColorComboBox, StartColorComboBox);
            SetMoveButtonState();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = CommonHelper.ValidateBindedFileds(Model, SummInput, Month1WeightInput, Month2WeightInput, Month3WeightInput, ColorComboBox, StartColorComboBox);
            if (result)
            {
                Model.Save();
                MainWindow.NavigateTargetsPage(Model.Id);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigateTargetsPage(targetId);
        }

        private void AddChartButton_Click(object sender, RoutedEventArgs e)
        {
            var item = new TargetChartItem();
            var addChartWindow = new AddChartWindow(item, false) { Owner = MainWindow };
            if (addChartWindow.ShowDialog() == true)
            {
                Model.AddChartList.Add(item);
                AddChartList.SelectedItem = item;
            }
        }

        private void EditChartButton_Click(object sender, RoutedEventArgs e)
        {
            var item = AddChartList.SelectedItem as TargetChartItem;
            var addChartWindow = new AddChartWindow(item, true) { Owner = MainWindow };
            if (addChartWindow.ShowDialog() == true)
                AddChartList.SelectedItem = item;
        }

        private void AddChartList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AddChartList.SelectedItem != null)
                EditChartButton_Click(sender, e);
        }

        private void DeleteChartButton_Click(object sender, RoutedEventArgs e)
        {
            var item = AddChartList.SelectedItem as TargetChartItem;
            Model.AddChartList.Remove(item);
        }

        private void AddChartList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetMoveButtonState();
        }

        private void SetMoveButtonState()
        {
            MoveUpChartButton.IsEnabled = AddChartList.SelectedItem != null && AddChartList.SelectedIndex > 0;
            MoveDownChartButton.IsEnabled = AddChartList.SelectedItem != null && AddChartList.SelectedIndex < Model.AddChartList.Count - 1;
        }

        private void MoveUpChartButton_Click(object sender, RoutedEventArgs e)
        {
            var index = AddChartList.SelectedIndex;
            CommonHelper.ExchangeItems<TargetChartItem>(Model.AddChartList, index, index - 1);
            AddChartList.SelectedIndex = index - 1;
        }

        private void MoveDownChartButton_Click(object sender, RoutedEventArgs e)
        {
            var index = AddChartList.SelectedIndex;
            CommonHelper.ExchangeItems<TargetChartItem>(Model.AddChartList, index, index + 1);
            AddChartList.SelectedIndex = index + 1;
        }

        private MainWindow MainWindow
        {
            get { return Application.Current.MainWindow as MainWindow; }
        }

        private TargetEditModel Model
        {
            get { return TargetEditForm.DataContext as TargetEditModel; }
        }

    }
}
