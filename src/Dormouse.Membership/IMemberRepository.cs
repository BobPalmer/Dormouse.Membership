using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace NHibernateProvider
{
    public interface IMemberRepository
    {
        void ChangePassword(string username, string newPwd);
        void ChangePasswordQuestionAndAnswer(string username, string newPwdQuestion, string newPwdAnswer);
        void CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved);
        void DeleteUser(string username);
        int GetAllUsers(int pageIndex, int pageSize, MembershipUserCollection users, int counter);
        int GetNumberOfUsersOnline(ref DateTime compareTime);
        User GetPassword(string username, ref string password, ref string passwordAnswer);
        MembershipUser GetUser(string username, bool userIsOnline, MembershipUser membershipUser);
        MembershipUser GetUser(object userID, bool userIsOnline, MembershipUser membershipUser);
        void UnlockUser(string username);
        string GetUserNameByEMail(string email);
        string ResetPassword(string username, string answer, string newPassword, ref string passwordAnswer);
        void UpdateUser(MembershipUser membershipUser);
        void ValidateUser(string username, string password, ref bool isValid, ref bool isApproved, ref string storedPassword);
        int FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, MembershipUserCollection users, int counter);
        int FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, MembershipUserCollection users, int counter);
        void UpdateFailureCount(string username, FailureType failureType);
        MembershipConfig Config { get; set; }
    }
}
