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
    public class RoleListItem
    {
        public EUserRole Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class UserEditModel : BaseEditModel
    {
        public UserEditModel()
            : base()
        {
            LoginName = "";
            Password = "";
            ConfirmPassword = "";
            FirstName = "";
            LastName = "";
            Role = null;
            RoleList = null;
        }

        private List<RoleListItem> CreateRoleList()
        {
            var result = new List<RoleListItem>();
            result.Add(new RoleListItem() { Id = EUserRole.Admin, Name = "Admin" });
            result.Add(new RoleListItem() { Id = EUserRole.SuperAdmin, Name = "Super Admin" });
            return result;
        }

        protected override void InternalLoad(MainContext context)
        {
            RoleList = CreateRoleList();
            if (Id > 0)
            {
                var user = context.Users.Where(u => u.Id == Id).Single();
                LoginName = user.LoginName;
                Password = user.Password;
                FirstName = user.FirstName;
                LastName = user.LastName;
                Role = RoleList.Where(r => r.Id == (EUserRole)user.Role).Single();
            }
            else Role = RoleList.First();
        }

        protected override void InternalSave(MainContext context, TransactionScope transaction)
        {
            if (context.Users.Any(u => u.LoginName == LoginName && u.Id != Id))
                throw new ApplicationException("Login should be unique");
            User user;
            if (Id <= 0)
            {
                user = context.Users.Create();
                user.Password = CommonHelper.GetMd5Hash(Password);
            }
            else user = context.Users.Where(u => u.Id == Id).Single();
            user.Id = Id;
            user.LoginName = LoginName;
            user.FirstName = FirstName;
            user.LastName = LastName;
            user.Role = (int)Role.Id;
            if (Id <= 0)
                context.Users.Add(user);
        }

        protected override void InternalDelete(MainContext context, TransactionScope transaction)
        {
            context.Users.Where(u => u.Id == Id).Delete();
        }

        public override string CheckField(string columnName)
        {
            if (IsColumn(columnName, "LoginName"))
            {
                if (String.IsNullOrEmpty(LoginName))
                    return "Login is required";
                else return null;
            }
            else if (IsColumn(columnName, "Password") && Id <= 0)
            {
                if (String.IsNullOrEmpty(Password))
                    return "Password is required";
                else if (Password.Length < 6)
                    return "Password length should be more than 5 characters";
                else return null;
            }
            else if (IsColumn(columnName, "ConfirmPassword") && Id <= 0)
            {
                if (String.IsNullOrEmpty(ConfirmPassword))
                    return "Confirm password is required";
                else if (ConfirmPassword != Password)
                    return "Password and Confirm password should be equal";
                else return null;
            }
            else if (IsColumn(columnName, "FirstName"))
            {
                if (String.IsNullOrEmpty(FirstName))
                    return "First name is required";
                else return null;
            }
            else if (IsColumn(columnName, "LastName"))
            {
                if (String.IsNullOrEmpty(LastName))
                    return "Last name is required";
                else return null;
            }
            else return base.CheckField(columnName);
        }

        public string LoginName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public RoleListItem Role { get; set; }
        public List<RoleListItem> RoleList { get; private set; }
    }
}
