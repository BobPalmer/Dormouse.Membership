using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using Dormouse.Membership.Model;
using Dormouse.Membership.Repository;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;

namespace Dormouse.Membership.Data
{
    public class MembershipRepository : IMembershipRepository
    {
        private static bool _buildSchema = false;
        private ISessionFactory _sessionFactory;

        public ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    _sessionFactory = CreateSessionFactory();
                }
                return _sessionFactory;
            }
        }

        public MembershipRepository()
        {
        }

        public MembershipRepository(bool buildSchema)
        {
            _buildSchema = buildSchema;
        }

        private static ISessionFactory CreateSessionFactory()
        {
            Configuration configuration = new Configuration();
            configuration.AddAssembly(Assembly.GetCallingAssembly());

            if (_buildSchema)
            {
                var schema = new SchemaExport(configuration);
                schema.Create(true, true);
            }

            return configuration.BuildSessionFactory();
        }

        public User GetUserByName(string userName, string appName)
        {
            using (var session = SessionFactory.OpenSession())
            {
                var u = session.CreateCriteria(typeof (User))
                            .Add(Restrictions.Eq("UserName", userName))
                            .Add(Restrictions.Eq("ApplicationName", appName))
                            .UniqueResult() as User;
                return u;
            }
        }

        public Role GetRoleByName(string roleName, string appName)
        {
            using (var session = SessionFactory.OpenSession())
            {
                var r = session.CreateCriteria(typeof (Role))
                            .Add(Restrictions.Eq("RoleName", roleName))
                            .Add(Restrictions.Eq("ApplicationName", appName))
                            .UniqueResult() as Role;
                return r;
            }
        }

        public void SaveUser(User userToSave)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    session.SaveOrUpdate(userToSave);
                    trans.Commit();
                }
            }
        }

        public void SaveRole(Role roleToSave)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    session.SaveOrUpdate(roleToSave);
                    trans.Commit();
                }
            }
        }


        public void DeleteUser(User userToDelete)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    session.Delete(userToDelete);
                    trans.Commit();
                }
            }
        }

        public void DeleteRole(Role roleToDelete)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    session.Delete(roleToDelete);
                    trans.Commit();
                }
            }
        }

        public List<User> GetAllUsers(int pageIndex, int pageSize, string appName)
        {
            var users = new List<User>();
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    users = session.CreateCriteria(typeof (User))
                                .Add(Expression.Eq("ApplicationName", appName))
                                .SetFirstResult(pageIndex*pageSize).SetMaxResults(pageSize)
                                .List<User>() as List<User>;
                }
            }
            return users;
        }

        public int GetNumberOfUsersOnline(DateTime compareTime, string appName)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof (User))
                        .Add(Expression.Eq("ApplicationName", appName))
                        .Add(Expression.Ge("LastActivityDate", compareTime))
                        .List();
                    return uList.Count;
                }
            }
        }

        public User GetUserByID(int userID)
        {
            using (var session = SessionFactory.OpenSession())
            {
                return session.Get<User>(userID);
            }
        }


        public string GetUserNameByEMail(string EMail, string appName)
        {
            if (EMail == null) return String.Empty;
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof (User))
                        .Add(Expression.Eq("EMail", EMail))
                        .Add(Expression.Eq("ApplicationName", appName))
                        .List();
                    if (uList.Count > 0)
                    {
                        User u = (User) uList[0];
                        return u.UserName;
                    }
                    else
                    {
                        return String.Empty;
                    }
                }
            }
        }

        public List<User> FindUsersByEMail(string email, int pageIndex, int pageSize, string appName)
        {
            var users = new List<User>();
            if (!String.IsNullOrEmpty(email))
            {
                using (var session = SessionFactory.OpenSession())
                {
                    users = session.CreateCriteria(typeof (User))
                                .Add(Expression.Like("EMail", email, MatchMode.Anywhere))
                                .Add(Expression.Eq("ApplicationName", appName))
                                .SetFirstResult(pageIndex*pageSize).SetMaxResults(pageSize)
                                .List<User>() as List<User>;
                }
            }
            return users;
        }

        public List<User> GetAllUsersInRole(string roleName, string appName)
        {
            return FindUsersInRole(roleName, "", appName);
        }

        public List<User> FindUsersInRole(string roleName, string userName, string appName)
        {
            var users = new List<User>();
            using (var session = SessionFactory.OpenSession())
            {
                var qCrit = session.CreateCriteria<User>();
                if (userName != "")
                {
                    qCrit = qCrit.Add(Expression.Like("UserName", userName));
                }
                users = qCrit.CreateCriteria("Roles")
                            .Add(Expression.Eq("RoleName", roleName))
                            .List<User>() as List<User>;
            }
            return users;
        }

        public List<Role> GetAllRolesForUser(string userName, string appName)
        {
            var user = GetUserByName(userName, appName);
            var roleList = new List<Role>();
            if (user != null) roleList = user.Roles.ToList();
            return roleList;
        }


        public List<Role> GetAllRoles(string appName)
        {
            var roles = new List<Role>();
            using (var session = SessionFactory.OpenSession())
            {
                roles = session.CreateCriteria<Role>()
                            .Add(Expression.Eq("ApplicationName", appName))
                            .List<Role>() as List<Role>;
            }
            return roles;
        }

        public List<User> FindUsersByName(string userName, int pageIndex, int pageSize, string appName)
        {
            var users = new List<User>();
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    users = session.CreateCriteria(typeof (User))
                                .Add(Expression.Like("UserName", userName, MatchMode.Anywhere))
                                .Add(Expression.Eq("ApplicationName", appName))
                                .SetFirstResult(pageIndex*pageSize).SetMaxResults(pageSize)
                                .List<User>() as List<User>;
                }
            }
            return users;
        }

        public void AddUsersToRoles(List<string> users, List<string> roles, string appName)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    var uList = session.CreateCriteria<User>()
                                    .Add(Expression.In("UserName", users))
                                    .Add(Expression.Eq("ApplicationName", appName))
                                    .List<User>() as List<User>;
                    var rList = session.CreateCriteria<Role>()
                                    .Add(Expression.In("RoleName", roles))
                                    .Add(Expression.Eq("ApplicationName", appName))
                                    .List<Role>() as List<Role>;
                    foreach (var user in uList)
                    {
                        var rolesToAdd = rList.Except(user.Roles);
                        foreach (var role in rolesToAdd)
                        {
                            user.Roles.Add(role);
                        }
                    }
                    trans.Commit();
                }
            }
        }

        public void RemoveUsersFromRoles(List<string> users, List<string> roles, string appName)
        {
            using (var session = SessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    var uList = session.CreateCriteria<User>()
                                    .Add(Expression.In("UserName", users))
                                    .Add(Expression.Eq("ApplicationName", appName))
                                    .List<User>() as List<User>;
                    var rList = session.CreateCriteria<Role>()
                                    .Add(Expression.In("RoleName", roles))
                                    .Add(Expression.Eq("ApplicationName", appName))
                                    .List<Role>() as List<Role>;
                    foreach (var user in uList)
                    {
                        var rolesToRemove = user.Roles.Where(r => rList.Contains(r)).ToList();
                        foreach (var role in rolesToRemove)
                        {
                            user.Roles.Remove(role);
                        }
                    }
                    trans.Commit();
                }
            }
        }
    }
}