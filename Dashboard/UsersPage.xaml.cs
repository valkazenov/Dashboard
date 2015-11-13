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
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
            Model = new UserListModel();
        }

        public void ShowPage(int userId)
        {
            LoadUserList(userId);
            UserList.Focus();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CommonHelper.TuneListViewSort<UserListItem>(UserList, Model);
        }

        private void LoadUserList(int userId)
        {
            Model.Load();
            UserList.ItemsSource = Model.Items;
            if (Model.Items.Count > 0)
            {
                var user = userId > 0 ? Model.Items.Where(u => u.Id == userId).FirstOrDefault() : null;
                UserList.SelectedItem = user != null ? user : Model.Items[0];
            }
            else UserList.SelectedItem = null;
        }

        private void UserList_HeaderClick(object sender, RoutedEventArgs e)
        {
            var header = e.OriginalSource as GridViewColumnHeader;
            CommonHelper.ListViewHeaderClick<UserListItem>(header, Model);
            LoadUserList(-1);
            CommonHelper.TuneListViewSort<UserListItem>(UserList, Model);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var item = UserList.SelectedItem as UserListItem;
            MainWindow.NavigateUserDetailsPage(item != null ? item.Id : -1, false);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var item = UserList.SelectedItem as UserListItem;
            MainWindow.NavigateUserDetailsPage(item.Id, true);
        }

        private void UserList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UserList.SelectedItem != null)
                EditButton_Click(sender, e);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var item = UserList.SelectedItem as UserListItem;
            if (item.Id == MainWindow.CurrentUserId)
                throw new ApplicationException("You can't delete yourself");
            var model = new UserEditModel() { Id = item.Id };
            model.Delete();
            LoadUserList(-1);
        }

        private void PasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var item = UserList.SelectedItem as UserListItem;
            var window = new PasswordWindow(item.Id) { Owner = MainWindow };
            window.ShowDialog();
        }

        private UserListModel Model { get; set; }

        private MainWindow MainWindow
        {
            get { return Application.Current.MainWindow as MainWindow; }
        }

    }
}
