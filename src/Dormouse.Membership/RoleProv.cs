using System.Linq;
using System.Web.Security;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System;
using Dormouse.Membership.Data;
using Dormouse.Membership.Model;
using Dormouse.Membership.Repository;

namespace Dormouse.Membership
{
    public class RoleProv : RoleProvider
    {
        private IMembershipRepository _memberRepo;
        public override string ApplicationName { get; set; }

        public RoleProv()
        {
            _memberRepo = new MembershipRepository();
        }

        public RoleProv(IMembershipRepository memberRepo)
        {
            _memberRepo = memberRepo;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "DormouseRoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Dormouse Role provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                ApplicationName = config["applicationName"];
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                RoleGuardClauses(rolename, true);
            }

            foreach (string username in usernames)
            {
                UserGuardClauses(username);
                foreach (string rolename in rolenames)
                {
                    if (IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is already in role.");
                    }
                }
            }
            _memberRepo.AddUsersToRoles(usernames.ToList(), rolenames.ToList(), ApplicationName);
        }

        public override void CreateRole(string rolename)
        {
            var role = RoleGuardClauses(rolename, false);
            if (role != null) throw new ProviderException("Role already exists.");

            var newRole = new Role();
            newRole.ApplicationName = ApplicationName;
            newRole.RoleName = rolename;
            _memberRepo.SaveRole(newRole);
        }

        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            var role = RoleGuardClauses(rolename, true);

            if (throwOnPopulatedRole && GetUsersInRole(rolename).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            _memberRepo.DeleteRole(role);
            return true;
        }

        public override string[] GetAllRoles()
        {
            var roleList = _memberRepo.GetAllRoles(ApplicationName);
            return roleList.Select(r => r.RoleName).ToArray();
        }

        public override string[] GetRolesForUser(string username)
        {
            UserGuardClauses(username);

            var roles = _memberRepo.GetAllRolesForUser(username, ApplicationName);
            return roles.Select(r => r.RoleName).ToArray();
        }

        public override string[] GetUsersInRole(string rolename)
        {
            var userList = _memberRepo.GetAllUsersInRole(rolename, ApplicationName);
            var userNames = userList.Select(user => user.UserName).ToArray();
            return userNames;
        }

        public override bool IsUserInRole(string username, string rolename)
        {
            UserGuardClauses(username);
            RoleGuardClauses(rolename, true);

            var userlist = FindUsersInRole(rolename, username);
            var userIsInRole = (userlist.Count() > 0);
            return userIsInRole;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                RoleGuardClauses(rolename, true);
            }

            foreach (string username in usernames)
            {
                UserGuardClauses(username);
                foreach (string rolename in rolenames)
                {
                    if (!IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is not in role.");
                    }
                }
            }
            _memberRepo.RemoveUsersFromRoles(usernames.ToList(), rolenames.ToList(), ApplicationName);
        }

        public override bool RoleExists(string rolename)
        {
            var role = RoleGuardClauses(rolename, false);
            var exists = (role != null);
            return exists;
        }

        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            RoleGuardClauses(rolename, true);
            string[] userList;
            if (usernameToMatch.Trim() != "")
            {
                userList = _memberRepo
                    .FindUsersInRole(rolename, usernameToMatch, ApplicationName)
                    .Select(u => u.UserName)
                    .ToArray();
            }
            else
            {
                userList = GetUsersInRole(rolename);
            }
            return userList;
        }

        private User UserGuardClauses(string username)
        {
            if (username == null) throw new ArgumentNullException("User cannot be null");
            if (username.Trim() == "") throw new ArgumentException("User cannot be blank");
            if (username.Contains(",")) throw new ArgumentException("User names cannot contain commas.");
            var user = _memberRepo.GetUserByName(username, ApplicationName);
            if (user == null) throw new ProviderException("Username not found.");
            return user;
        }

        private Role RoleGuardClauses(string rolename, bool mustExist)
        {
            if (rolename == null) throw new ArgumentNullException("Role cannot be null");
            if (rolename.Trim() == "") throw new ArgumentException("Role cannot be blank");
            if (rolename.Contains(",")) throw new ArgumentException("Role names cannot contain commas.");
            var role = _memberRepo.GetRoleByName(rolename, ApplicationName);
            if (role == null && mustExist) throw new ProviderException("Role does not exist");
            return role;
        }
    }
}