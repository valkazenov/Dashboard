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
    /// Interaction logic for UserDetails.xaml
    /// </summary>
    public partial class UserDetailsPage : Page
    {
        private int userId;

        public UserDetailsPage(int userId, bool forEdit)
        {
            InitializeComponent();
            this.userId = userId;
            var model = new UserEditModel() { Id = forEdit ? userId : -1 };
            model.Load();
            UserEditForm.DataContext = model;
            PasswordInput.Visibility = forEdit ? Visibility.Collapsed : Visibility.Visible;
            PasswordLabel.Visibility = forEdit ? Visibility.Collapsed : Visibility.Visible;
            ConfirmInput.Visibility = forEdit ? Visibility.Collapsed : Visibility.Visible;
            ConfirmLabel.Visibility = forEdit ? Visibility.Collapsed : Visibility.Visible;
            UserNameInput.Focus();
            CommonHelper.InitValidation(UserNameInput, PasswordInput, ConfirmInput, FirstNameInput, LastNameInput);
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            Model.Password = PasswordInput.Password;
            Model.ConfirmPassword = ConfirmInput.Password;
            var bindedResult = CommonHelper.ValidateBindedFileds(Model, UserNameInput, FirstNameInput, LastNameInput);
            var passwordResult = CommonHelper.ValidateField(Model, PasswordInput, "Password");
            var confirmResult = CommonHelper.ValidateField(Model, ConfirmInput, "ConfirmPassword");
            if (bindedResult && passwordResult && confirmResult)
            {
                if (Model.Role.Id == EUserRole.Admin && Model.Id == MainWindow.CurrentUserId)
                    throw new ApplicationException("You can't change your role to Admin");
                Model.Save();
                MainWindow.NavigateUsersPage(userId);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigateUsersPage(userId);
        }

        private MainWindow MainWindow
        {
            get { return Application.Current.MainWindow as MainWindow; }
        }

        private UserEditModel Model
        {
            get { return UserEditForm.DataContext as UserEditModel; }
        }

    }
}
