using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseContext;
using Utilities;

namespace Dashboard.Models
{
    public class UserListItem
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public EUserRole Role { get; set; }

        public string RoleName
        {
            get { return Role == EUserRole.SuperAdmin ? "Super Admin" : "Admin"; }
        }
    }

    public class UserListModel : BaseListModel<UserListItem>
    {
        public UserListModel()
        {
            SortName = "LoginName";
        }

        protected override List<UserListItem> InternalCreateItems(MainContext context)
        {
            var list = context.Users as IQueryable<User>;
            Func<User, object> func;
            switch (SortName.ToLower())
            {
                case "firstName": func = u => u.FirstName; break;
                case "lastName": func = u => u.LastName; break;
                case "roleName": func = u => u.Role; break;
                default: func = u => u.LoginName; break;
            }
            return (ByDescending ? list.OrderByDescending(func) : list.OrderBy(func)).
                Select(u => new UserListItem { Id = u.Id, LoginName = u.LoginName, FirstName = u.FirstName, LastName = u.LastName, Role = (EUserRole)u.Role }).ToList();
        }
    }
}
