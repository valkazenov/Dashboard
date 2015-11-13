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
using System.Security;
using Dashboard.Models;
using Utilities;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            MainWindow.CurrentUserId = -1;
            LoginForm.DataContext = new LoginModel();
            CommonHelper.InitValidation(UserNameInput, PasswordInput);
            UserNameInput.Focus();
        }

        private LoginModel Model
        {
            get { return LoginForm.DataContext as LoginModel; }
        }

        private MainWindow MainWindow
        {
            get { return Application.Current.MainWindow as MainWindow; }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Model.Password = PasswordInput.Password;
            var userResult = CommonHelper.ValidateBindedFileds(Model, UserNameInput);
            var passwordResult = CommonHelper.ValidateField(Model, PasswordInput, "Password");
            if (userResult && passwordResult)
            {
                Model.Save();
                MainWindow.CurrentUserId = Model.UserId;
                MainWindow.UserItem.Visibility = Model.Role == EUserRole.SuperAdmin ? Visibility.Visible : Visibility.Hidden;
                MainWindow.NavigateHomePage();
            }
        }

    }
}
