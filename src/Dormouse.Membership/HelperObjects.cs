using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Configuration.Provider;
using System.Web.Security;
using System.Web.Configuration;

namespace NHibernateProvider
{
    public enum FailureType
    {
        Password = 1,
        PasswordAnswer = 2
    }

    public static class HelperObjects
    {
        /// <summary>
        /// Create a MembershipUser object from a data reader.
        /// </summary>
        /// <param name="sqlDataReader">Data reader.</param>
        /// <returns>MembershipUser object.</returns>
        public static MembershipUser GetUserFromObject(User u, string name)
        {
            DateTime creationDate = (DateTime)u.CreationDate;
            DateTime lastLoginDate = new DateTime();
            if (u.LastLoginDate != null)
            {
                lastLoginDate = (DateTime)u.LastLoginDate;
            }
            DateTime lastActivityDate = new DateTime();
            if (u.LastActivityDate != null)
            {
                lastActivityDate = (DateTime)u.LastActivityDate;
            }
            DateTime lastPasswordChangedDate = new DateTime();
            if (u.LastPasswordChangedDate != null)
            {
                lastPasswordChangedDate = (DateTime)u.LastPasswordChangedDate;
            }
            DateTime lastLockedOutDate = new DateTime();
            if (u.LastLockedOutDate != null)
            {
                lastLockedOutDate = (DateTime)u.LastLockedOutDate;
            }

            MembershipUser membershipUser = new MembershipUser(
              name,
             u.UserName,
             (object)u.UserID,
             u.EMail,
             u.PasswordQuestion,
             u.Comment,
             u.IsApproved,
             u.IsLockedOut,
             creationDate,
             lastLoginDate,
             lastActivityDate,
             lastPasswordChangedDate,
             lastLockedOutDate
              );
            return membershipUser;
        }


    }

    public struct MembershipConfig
    {
        public int newPasswordLength;
        public string applicationName;
        public bool enablePasswordReset;
        public bool enablePasswordRetrieval;
        public bool requiresQuestionAndAnswer;
        public bool requiresUniqueEmail;
        public int maxInvalidPasswordAttempts;
        public int passwordAttemptWindow;
        public MembershipPasswordFormat passwordFormat;
        public int minRequiredNonAlphanumericCharacters;
        public int minRequiredPasswordLength;
        public string passwordStrengthRegularExpression;
        public MachineKeySection machineKey;
        public string providerName;
    }

}
