using System;
using System.Collections.Generic;

namespace Dormouse.Membership.Model
{
    public class User
    {
        public virtual int UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual string ApplicationName { get; set; }
        public virtual string EMail { get; set; }
        public virtual string Comment { get; set; }
        public virtual string Password { get; set; }
        public virtual string PasswordQuestion { get; set; }
        public virtual string PasswordAnswer { get; set; }
        public virtual bool IsApproved { get; set; }
        public virtual DateTime? LastActivityDate { get; set; }
        public virtual DateTime? LastLoginDate { get; set; }
        public virtual DateTime? LastPasswordChangedDate { get; set; }
        public virtual DateTime? CreationDate { get; set; }
        public virtual bool IsOnline { get; set; }
        public virtual bool IsLockedOut { get; set; }
        public virtual DateTime? LastLockedOutDate { get; set; }
        public virtual int FailedPasswordAttemptCount { get; set; }
        public virtual DateTime? FailedPasswordAttemptWindowStart { get; set; }
        public virtual int FailedPasswordAnswerAttemptCount { get; set; }
        public virtual DateTime? FailedPasswordAnswerAttemptWindowStart { get; set; }
        public virtual DateTime? PrevLoginDate { get; set; }
        public virtual string OpenIdClaimedIdentifier { get; set; }
        public virtual IList<Role> Roles { get; set; }

        public User()
        {
            Roles = new List<Role>();
        }

        public virtual string Gravatar
        {
            get { return "http://www.gravatar.com/avatar/" + MD5(EMail.ToLower().Trim()) + "?s=75&d=identicon"; }
        }

        private string MD5(string theEmail)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Obj =
                new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytesToHash = System.Text.Encoding.ASCII.GetBytes(theEmail);
            bytesToHash = md5Obj.ComputeHash(bytesToHash);
            string strResult = "";
            foreach (byte b in bytesToHash)
            {
                strResult += b.ToString("x2");
            }
            return strResult;
        }
    }
}