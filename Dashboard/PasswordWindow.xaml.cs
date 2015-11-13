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
    /// Interaction logic for PasswordWindow.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        public PasswordWindow(int userId)
        {
            InitializeComponent();
            var model = new SetPasswordModel() { Id = userId };
            model.Load();
            PasswordForm.DataContext = model;
            PasswordInput.Focus();
            CommonHelper.InitValidation(PasswordInput, ConfirmInput);
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            Model.Password = PasswordInput.Password;
            Model.ConfirmPassword = ConfirmInput.Password;
            var passwordResult = CommonHelper.ValidateField(Model, PasswordInput, "Password");
            var confirmResult = CommonHelper.ValidateField(Model, ConfirmInput, "ConfirmPassword");
            if (passwordResult && confirmResult)
            {
                Model.Save();
                Close();
            }
        }

        private SetPasswordModel Model
        {
            get { return PasswordForm.DataContext as SetPasswordModel; }
        }

        private MainWindow MainWindow
        {
            get { return Application.Current.MainWindow as MainWindow; }
        }

    }
}
