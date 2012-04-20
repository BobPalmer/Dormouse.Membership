using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using System.Reflection;
using NHibernate.Criterion;
using System.Web.Security;
using System.Collections;

namespace NHibernateProvider
{
    public class NHMemberRepository : IMemberRepository
    {
        private ISessionFactory sessionFactory;
        private MembershipConfig _config;

        public NHMemberRepository()
        {
            sessionFactory = CreateSessionFactory();
        }

        public MembershipConfig Config 
        {
            get { return _config; }
            set { _config = value; } 
        }

        private ISessionFactory CreateSessionFactory()
        {
            Configuration configuration = new Configuration();
            configuration.AddAssembly(Assembly.GetCallingAssembly());
            return configuration.BuildSessionFactory();
        }

        public void ChangePassword(string username, string newPwd)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserName", username))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    foreach (User u in uList)
                    {
                        u.Password = newPwd;
                        session.Save(u);
                    }
                    trans.Commit();
                }
            }
        }

        public void ChangePasswordQuestionAndAnswer(string username, string newPwdQuestion, string newPwdAnswer)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserName", username))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    foreach (User u in uList)
                    {
                        u.PasswordQuestion = newPwdQuestion;
                        u.PasswordAnswer = newPwdAnswer;
                        session.Save(u);
                    }
                    trans.Commit();
                }
            }
        }

        public void CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    User u = new User();
                    u.UserName = username;
                    u.ApplicationName = _config.applicationName;
                    u.Password = password;
                    u.EMail = email;
                    u.PasswordQuestion = passwordQuestion;
                    u.PasswordAnswer = passwordAnswer;
                    u.IsApproved = isApproved;
                    u.Comment = String.Empty;
                    u.CreationDate = DateTime.Now;
                    session.SaveOrUpdate(u);
                    transaction.Commit();
                }
            }
        }

        public void DeleteUser(string username)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserName", username))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    foreach (User u in uList)
                    {
                        session.Delete(u);
                    }
                    trans.Commit();
                }
            }
        }

        public int GetAllUsers(int pageIndex, int pageSize, MembershipUserCollection users, int counter)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    int startIndex = pageSize * pageIndex;
                    int endIndex = startIndex + pageSize - 1;

                    foreach (User u in uList)
                    {
                        if (counter >= startIndex)
                        {
                            users.Add(HelperObjects.GetUserFromObject(u, _config.providerName));
                        }
                        counter += 1;
                        if (counter > endIndex)
                        {
                            break;
                        }
                    }
                }
            }
            return counter;
        }

        public int GetNumberOfUsersOnline(ref DateTime compareTime)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .Add(Expression.Ge("LastActivityDate", compareTime))
                        .List();
                    return uList.Count;
                }
            }
        }

        public User GetPassword(string username, ref string password, ref string passwordAnswer)
        {
            User curUser;
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserName", username))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    curUser = (User)uList[0];
                    password = curUser.Password;
                    passwordAnswer = curUser.PasswordAnswer;
                }
            }
            return curUser;
        }

        public MembershipUser GetUser(string username, bool userIsOnline, MembershipUser membershipUser)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserName", username))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    if (uList.Count > 0)
                    {
                        User u = (User)uList[0];
                        membershipUser = HelperObjects.GetUserFromObject(u,_config.providerName);
                        if (userIsOnline)
                        {
                            u.LastActivityDate = DateTime.Now;
                            trans.Commit();
                        }
                    }
                }
            }
            return membershipUser;
        }

        public MembershipUser GetUser(object userID, bool userIsOnline, MembershipUser membershipUser)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserID", userID))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    if (uList.Count > 0)
                    {
                        User u = (User)uList[0];
                        membershipUser = HelperObjects.GetUserFromObject(u, _config.providerName);
                        if (userIsOnline)
                        {
                            u.LastActivityDate = DateTime.Now;
                            trans.Commit();
                        }
                    }
                }
            }
            return membershipUser;
        }

        public void UnlockUser(string username)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserName", username))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    foreach (User u in uList)
                    {
                        u.IsLockedOut = false;
                        session.Save(u);
                    }
                    trans.Commit();
                }
            }
        }

        public string GetUserNameByEMail(string email)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("EMail", email))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    if (uList.Count > 0)
                    {
                        User u = (User)uList[0];
                        return u.UserName;
                    }
                    else
                    {
                        return String.Empty;
                    }
                }
            }
        }

        public string ResetPassword(string username, string answer, string newPassword, ref string passwordAnswer)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserName", username))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    if (uList.Count > 0)
                    {
                        User u = (User)uList[0];
                        if (u.IsLockedOut)
                        {
                            throw new MembershipPasswordException("The supplied user is locked out.");
                        }
                        passwordAnswer = u.PasswordAnswer;
                        if (_config.requiresQuestionAndAnswer && (CheckPassword(answer, passwordAnswer, _config.passwordFormat, _config.machineKey)))
                        {
                            UpdateFailureCount(username, FailureType.PasswordAnswer);
                            throw new MembershipPasswordException("Incorrect password answer.");
                        }
                        u.Password = newPassword;
                        trans.Commit();
                        return newPassword;
                    }
                    else
                    {
                        throw new MembershipPasswordException("The supplied user name is not found.");
                    }
                }
            }
        }

        public void UpdateUser(MembershipUser membershipUser)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserName", membershipUser.UserName))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    foreach (User u in uList)
                    {
                        u.EMail = membershipUser.Email;
                        u.Comment = membershipUser.Comment;
                        u.IsApproved = membershipUser.IsApproved;
                        session.Save(u);
                    }
                    trans.Commit();
                }
            }
        }

        public void ValidateUser(string username, string password, ref bool isValid, ref bool isApproved, ref string storedPassword)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserName", username))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    if (uList.Count > 0)
                    {
                        User u = (User)uList[0];
                        storedPassword = u.Password;
                        isApproved = u.IsApproved;
                        if (CheckPassword(password, storedPassword,_config.passwordFormat, _config.machineKey))
                        {
                            if (isApproved)
                            {
                                isValid = true;
                                u.LastLoginDate = DateTime.Now;
                                trans.Commit();
                            }
                        }
                        else
                        {
                            UpdateFailureCount(username, FailureType.Password);
                        }


                    }
                    else
                    {
                        isValid = false;
                    }
                }
            }
        }

        public int FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, MembershipUserCollection users, int counter)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Like("UserName", usernameToMatch, MatchMode.Anywhere))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    int startIndex = pageSize * pageIndex;
                    int endIndex = startIndex + pageSize - 1;

                    foreach (User u in uList)
                    {
                        if (counter >= startIndex)
                        {
                            users.Add( HelperObjects.GetUserFromObject(u,_config.providerName));
                        }
                        counter += 1;
                        if (counter > endIndex)
                        {
                            break;
                        }
                    }
                }
            }
            return counter;
        }

        public int FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, MembershipUserCollection users, int counter)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("EMail", emailToMatch))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    int startIndex = pageSize * pageIndex;
                    int endIndex = startIndex + pageSize - 1;

                    foreach (User u in uList)
                    {
                        if (counter >= startIndex)
                        {
                            users.Add( HelperObjects.GetUserFromObject(u,_config.providerName));
                        }
                        counter += 1;
                        if (counter > endIndex)
                        {
                            break;
                        }
                    }
                }
            }
            return counter;
        }

        public void UpdateFailureCount(string username, FailureType failureType)
        {
            using (var session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    IList uList = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("UserName", username))
                        .Add(Expression.Eq("ApplicationName", _config.applicationName))
                        .List();
                    foreach (User u in uList)
                    {
                        if (failureType == FailureType.Password)
                        {
                            u.FailedPasswordAttemptCount++;
                        }
                        else
                        {
                            u.FailedPasswordAnswerAttemptCount++;
                        }
                        session.Save(u);
                    }
                    trans.Commit();
                }
            }
        }

    }
}
