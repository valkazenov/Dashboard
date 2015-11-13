using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Security;
using DatabaseContext;
using Utilities;

namespace Dashboard.Models
{
    public class LoginModel : BaseEntityModel
    {
        public LoginModel()
            : base()
        {
            LoginName = "";
            Password = "";
            UserId = -1;
            Role = EUserRole.Admin;
        }

        protected override void InternalSave(MainContext context, TransactionScope transaction)
        {
            var user = context.Users.Where(u => u.LoginName == LoginName).Select(u => new { u.Id, u.Password, u.Role }).FirstOrDefault();
            if (user == null || user.Password != CommonHelper.GetMd5Hash(Password))
                throw new ApplicationException("Invalid user credentials");
            UserId = user.Id;
            Role = (EUserRole)user.Role;
        }

        public override string CheckField(string columnName)
        {
            if (IsColumn(columnName, "LoginName"))
            {
                if (String.IsNullOrEmpty(LoginName))
                    return "Login is required";
                else return null;
            }
            else if (IsColumn(columnName, "Password"))
            {
                if (Password == null || Password.Length == 0)
                    return "Password is required";
                else if (Password.Length < 6)
                    return "Password length should be more than 5 characters";
                else return null;
            }
            else return base.CheckField(columnName);
        }

        public string LoginName { get; set; }
        public string Password { get; set; }
        public int UserId { get; private set; }
        public EUserRole Role { get; private set; }
    }
}
