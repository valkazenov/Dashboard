using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EntityFramework.Extensions;
using DatabaseContext;
using Utilities;

namespace Dashboard.Models
{
    public class SetPasswordModel: BaseEntityModel
    {
        public SetPasswordModel()
        {
            Id = -1;
            Password = "";
            ConfirmPassword = "";
            FullName = "";
        }

        protected override void InternalLoad(MainContext context)
        {
            FullName = context.Users.Where(u => u.Id == Id).Select(u => u.FirstName + " " + u.LastName).Single();
        }

        protected override void InternalSave(MainContext context, TransactionScope transaction)
        {
            var password = CommonHelper.GetMd5Hash(Password);
            context.Users.Where(u => u.Id == Id).Update(u => new User() { Password = password });
        }

        public override string CheckField(string columnName)
        {
            if (IsColumn(columnName, "Password"))
            {
                if (String.IsNullOrEmpty(Password))
                    return "Password is required";
                else if (Password.Length < 6)
                    return "Password length should be more than 5 characters";
                else return null;
            }
            else if (IsColumn(columnName, "ConfirmPassword"))
            {
                if (String.IsNullOrEmpty(ConfirmPassword))
                    return "Confirm password is required";
                else if (ConfirmPassword != Password)
                    return "Password and Confirm password should be equal";
                else return null;
            }
            else return base.CheckField(columnName);
        }

        public int Id { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FullName { get; private set; }
    }
}
