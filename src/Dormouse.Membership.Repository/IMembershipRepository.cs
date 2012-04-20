using System;
using System.Collections.Generic;
using Dormouse.Membership.Model;

namespace Dormouse.Membership.Repository
{
    public interface IMembershipRepository
    {
        User GetUserByName(string userName, string appName);
        string GetUserNameByEMail(string EMail, string appName);
        User GetUserByID(Int32 userID);
        void SaveUser(User userToSave);
        void DeleteUser(User userToDelete);
        List<User> GetAllUsers(int pageIndex, int pageSize, string appName);
        List<User> FindUsersByEMail(string email, int pageIndex, int pageSize, string appName);
        List<User> FindUsersByName(string userName, int pageIndex, int pageSize, string appName);
        int GetNumberOfUsersOnline(DateTime compareTime, string appName);

        void SaveRole(Role roleToSave);
        Role GetRoleByName(string roleName, string appName);
        void DeleteRole(Role roleToDelete);
        List<User> GetAllUsersInRole(string roleName, string appName);
        List<User> FindUsersInRole(string roleName, string userName, string appName);
        List<Role> GetAllRoles(string appName);
        List<Role> GetAllRolesForUser(string userName, string appName);
        void AddUsersToRoles(List<string> users, List<string> roles, string appName);
        void RemoveUsersFromRoles(List<string> users, List<string> roles, string appName);
    }
}