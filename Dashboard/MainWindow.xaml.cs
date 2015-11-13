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
using DatabaseContext;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HomePage homePage;
        private UsersPage usersPage;
        private TargetsPage targetsPage;

        public MainWindow()
        {
            InitializeComponent();
            CurrentUserId = -1;
            homePage = new HomePage();
            usersPage = new UsersPage();
            targetsPage = new TargetsPage();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            if (WindowState == WindowState.Normal)
                Properties.Settings.Default.Bounds = new Rect(Left, Top, Width, Height);
            else Properties.Settings.Default.Bounds = RestoreBounds;
            Properties.Settings.Default.Maximized = WindowState == WindowState.Maximized;
            homePage.Model.Save();
            Properties.Settings.Default.Save();
        }

        private void LoadSettings()
        {
            var bounds = Properties.Settings.Default.Bounds;
            if (!bounds.IsEmpty && !(bounds.Width == 0 && bounds.Height == 0))
            {
                Left = bounds.Left;
                Top = bounds.Top;
                Width = bounds.Width;
                Height = bounds.Height;
            }
            if (Properties.Settings.Default.Maximized)
                WindowState = WindowState.Maximized;
        }

        private void MenuPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SeparatorItem.Width = MenuPanel.ActualWidth - LogoutItem.ActualWidth - HomeItem.ActualWidth - TargetItem.ActualWidth - UserItem.ActualWidth;
        }

        private void TunePageFrame(bool menuVisible, RadioButton menuItem, Page content)
        {
            MenuPanel.Visibility = menuVisible ? Visibility.Visible : Visibility.Collapsed;
            if (menuItem != null) menuItem.IsChecked = true;
            PageFrame.Content = content;
            Title = "Dashboard - " + content.Title;
        }

        public void NavigateLoginPage()
        {
            var page = new LoginPage();
            TunePageFrame(false, null, page);
        }

        public void NavigateHomePage()
        {
            homePage.Model.Calculate();
            TunePageFrame(true, HomeItem, homePage);
        }

        public void NavigateUsersPage(int userId)
        {
            usersPage.ShowPage(userId);
            TunePageFrame(true, UserItem, usersPage);
        }

        public void NavigateUserDetailsPage(int userId, bool forEdit)
        {
            var page = new UserDetailsPage(userId, forEdit);
            TunePageFrame(true, UserItem, page);
        }

        public void NavigateTargetsPage(int targetId)
        {
            targetsPage.ShowPage(targetId);
            TunePageFrame(true, TargetItem, targetsPage);
        }

        public void NavigateTargetDetailsPage(int targetId, bool forEdit)
        {
            var page = new TargetDetailsPage(targetId, forEdit);
            TunePageFrame(true, TargetItem, page);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            NavigateHomePage(); CurrentUserId = 8;
            //NavigateLoginPage();
        }

        private void HomeItem_Click(object sender, RoutedEventArgs e)
        {
            NavigateHomePage();
        }

        private void TargetItem_Click(object sender, RoutedEventArgs e)
        {
            NavigateTargetsPage(-1);
        }

        private void UserItem_Click(object sender, RoutedEventArgs e)
        {
            NavigateUsersPage(-1);
        }

        private void LogoutItem_Click(object sender, RoutedEventArgs e)
        {
            NavigateLoginPage();
        }

        public int CurrentUserId { get; set; }

    }
}
