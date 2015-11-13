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
    /// Interaction logic for TargetsPage.xaml
    /// </summary>
    public partial class TargetsPage : Page
    {
        private Dictionary<string, string> manualColumns = new Dictionary<string, string>() 
        { 
            { "Year", "Year" }, { "Quarter", "Quarter" }, { "Sum", "Summ" }, { "Weight 1", "Month1Weight" }, { "Weight 2", "Month2Weight" }, { "Weight 3", "Month3Weight" }
        };

        public TargetsPage()
        {
            InitializeComponent();
            Model = new TargetListModel();
        }

        public void ShowPage(int targetId)
        {
            LoadTargetList(targetId);
            TargetList.Focus();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CommonHelper.TuneListViewSort<TargetListItem>(TargetList, Model, manualColumns);
        }

        private void TargetList_HeaderClick(object sender, RoutedEventArgs e)
        {
            var header = e.OriginalSource as GridViewColumnHeader;
            CommonHelper.ListViewHeaderClick<TargetListItem>(header, Model, manualColumns);
            LoadTargetList(-1);
            CommonHelper.TuneListViewSort<TargetListItem>(TargetList, Model, manualColumns);
        }

        private void LoadTargetList(int targetId)
        {
            Model.Load();
            TargetList.ItemsSource = Model.Items;
            if (Model.Items.Count > 0)
            {
                var target = targetId > 0 ? Model.Items.Where(t => t.Id == targetId).FirstOrDefault() : null;
                TargetList.SelectedItem = target != null ? target : Model.Items[0];
            }
            else TargetList.SelectedItem = null;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var item = TargetList.SelectedItem as TargetListItem;
            MainWindow.NavigateTargetDetailsPage(item != null ? item.Id : -1, false);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var item = TargetList.SelectedItem as TargetListItem;
            MainWindow.NavigateTargetDetailsPage(item.Id, true);
        }

        private void TargetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TargetList.SelectedItem != null)
                EditButton_Click(sender, e);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var item = TargetList.SelectedItem as TargetListItem;
            var model = new TargetEditModel() { Id = item.Id };
            model.Delete();
            LoadTargetList(-1);
        }

        private TargetListModel Model { get; set; }

        private MainWindow MainWindow
        {
            get { return Application.Current.MainWindow as MainWindow; }
        }

    }
}
