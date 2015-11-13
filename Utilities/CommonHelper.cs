using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.ComponentModel;
using System.Configuration;
using Xceed.Wpf.Toolkit;
using System.Collections.ObjectModel;
using DatabaseContext;

namespace Utilities
{
    public static class CommonHelper
    {
        public static string GetConnectionString(string key)
        {
            return ConfigurationManager.ConnectionStrings[key].ConnectionString;
        }

        public static TransactionScope CreateTransaction()
        {
            return new TransactionScope(TransactionScopeOption.Required, TransactionManager.MaximumTimeout, TransactionScopeAsyncFlowOption.Enabled);
        }

        public static MainContext CreateMainContext()
        {
            var result = new MainContext();
            ((IObjectContextAdapter)result).ObjectContext.CommandTimeout = (int)(TransactionManager.MaximumTimeout.TotalSeconds);
            return result;
        }

        public static string DecodeEntityValidationException(DbEntityValidationException exception)
        {
            var result = "";
            foreach (var eve in exception.EntityValidationErrors)
            {
                result += String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:\r\n", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                foreach (var ve in eve.ValidationErrors)
                    result += String.Format("- Property: \"{0}\", Error: \"{1}\"\r\n", ve.PropertyName, ve.ErrorMessage);
            }
            return result;
        }

        public static void SaveContextChanges(DbContext context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                throw new ApplicationException(DecodeEntityValidationException(ex));
            }
        }

        public static string GetMd5Hash(string value)
        {
            var md5Hasher = MD5.Create();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            var sBuilder = new StringBuilder();
            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }

        private static void ValidationChange(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            control.ClearValue(Control.BorderBrushProperty);
            control.BorderThickness = new Thickness(1);
            control.ToolTip = null;
        }

        public static void InitValidation(params Control[] controls)
        {
            foreach (var control in controls)
            {
                if (control is TextBox)
                    (control as TextBox).TextChanged += ValidationChange;
                else if (control is PasswordBox)
                    (control as PasswordBox).PasswordChanged += ValidationChange;
                else if (control is ColorPicker)
                    (control as ColorPicker).SelectedColorChanged += ValidationChange;
            }
        }

        public static bool ValidateField(IDataErrorInfo model, Control control, string columnName)
        {
            var error = model[columnName];
            if (error != null)
                control.BorderBrush = Brushes.Red;
            else control.ClearValue(Control.BorderBrushProperty);
            control.BorderThickness = error != null ? new Thickness(2) : new Thickness(1);
            control.ToolTip = error != null ? error : null;
            return error == null;
        }

        private static string GetBindedColumnName(Control control)
        {
            DependencyProperty property = null;
            if (control is TextBox)
                property = TextBox.TextProperty;
            else if (control is ColorPicker)
                property = ColorPicker.SelectedColorProperty;
            if (property != null)
            {
                var expression = BindingOperations.GetBindingExpression(control, property);
                return expression.ParentBinding.Path.Path;
            }
            else return "";
        }

        public static bool ValidateBindedFileds(IDataErrorInfo model, params Control[] controls)
        {
            var result = true;
            foreach (var control in controls)
            {
                var columnName = GetBindedColumnName(control);
                if (!String.IsNullOrEmpty(columnName))
                {
                    if (!ValidateField(model, control, columnName))
                        result = false;
                }
            }
            return result;
        }

        public static List<T> FindInVisualTree<T>(DependencyObject element) where T : DependencyObject
        {
            var result = new List<T>();
            if (element is T)
                result.Add(element as T);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                result.AddRange(FindInVisualTree<T>(VisualTreeHelper.GetChild(element, i)));
            return result;
        }

        public static void TuneListViewSort<T>(ListView listView, BaseListModel<T> model, Dictionary<string, string> manualSorts = null) where T : class
        {
            var list = FindInVisualTree<GridViewColumnHeader>(listView);
            var resources = Application.Current.Resources;
            foreach (var header in list.Where(h => h.Column != null))
            {
                string sortName = "";
                if (manualSorts != null && manualSorts.ContainsKey(header.Content.ToString()))
                    sortName = manualSorts[header.Content.ToString()];
                else if (header.Column.DisplayMemberBinding != null)
                    sortName = ((Binding)header.Column.DisplayMemberBinding).Path.Path;
                if (!String.IsNullOrEmpty(sortName) && String.Compare(model.SortName, sortName) == 0)
                    header.Column.HeaderTemplate = model.ByDescending ? resources["HeaderTemplateArrowDown"] as DataTemplate : resources["HeaderTemplateArrowUp"] as DataTemplate;
                else header.Column.ClearValue(GridViewColumn.HeaderTemplateProperty);
            }
        }

        public static void ListViewHeaderClick<T>(GridViewColumnHeader listHeader, BaseListModel<T> model, Dictionary<string, string> manualSorts = null) where T : class
        {
            var column = listHeader.Column;
            if (column == null) return;
            string sortName = "";
            if (manualSorts != null && manualSorts.ContainsKey(column.Header.ToString()))
                sortName = manualSorts[column.Header.ToString()];
            else if (column.DisplayMemberBinding != null)
                sortName = (column.DisplayMemberBinding as Binding).Path.Path;
            if (!String.IsNullOrEmpty(sortName))
            {
                if (String.Compare(model.SortName, sortName) != 0)
                {
                    model.SortName = sortName;
                    model.ByDescending = false;
                }
                else model.ByDescending = !model.ByDescending;
            }
        }

        public static Color StringToColor(string value)
        {
            var drawingColor = System.Drawing.ColorTranslator.FromHtml(value);
            return System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }

        public static string ColorToString(Color value)
        {
            var drawingColor = System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);
            return System.Drawing.ColorTranslator.ToHtml(drawingColor);
        }

        public static void ExchangeItems<T>(Collection<T> collection, int index1, int index2)
        {
            var tmp = collection[index1];
            collection[index1] = collection[index2];
            collection[index2] = tmp;
        }
    }
}
