//Adapted from: http://msdn2.microsoft.com/en-us/library/6tc47t75(VS.80).aspx
//  - Converted to C#
//  - Integrated with NHibernate
//  - Added additional fields and features (Gravatar, etc.)
//  - Added tests
//  - Migrated to .Net 4.0/NHibernate 3.2

using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Web.Security;
using System.Text.RegularExpressions;
using Dormouse.Membership.Data;
using Dormouse.Membership.Model;
using Dormouse.Membership.Repository;


namespace Dormouse.Membership
{
    public class MemberProv : MembershipProvider
    {
        public MemberProv()
        {
            _memberRepo = new MembershipRepository();
            _machineKey = GetMachineKeySection();
        }

        public MemberProv(IMembershipRepository userRepo, MachineKeySection machineKey)
        {
            _memberRepo = userRepo;
            _machineKey = machineKey;
        }

        #region Class Variables

        private IMembershipRepository _memberRepo;
        private string _applicationName;
        private bool _enablePasswordReset;
        private bool _enablePasswordRetrieval;
        private bool _requiresQuestionAndAnswer;
        private bool _requiresUniqueEmail;
        private int _maxInvalidPasswordAttempts;
        private int _passwordAttemptWindow;
        private MembershipPasswordFormat _passwordFormat;
        private int _minRequiredNonAlphanumericCharacters;
        private int _minRequiredPasswordLength;
        private string _passwordStrengthRegularExpression;
        private MachineKeySection _machineKey;

        #endregion

        #region Enums

        private enum FailureType
        {
            Password = 1,
            PasswordAnswer = 2
        }

        #endregion

        #region Properties

        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return _enablePasswordRetrieval; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return _requiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqueEmail; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _passwordFormat; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _minRequiredPasswordLength; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _passwordStrengthRegularExpression; }
        }

        #endregion

        #region Initialization

        public override void Initialize(string name, NameValueCollection config)
        {
            config = SetDefaultConfigValues(config);
            name = SetDefaultName(name);

            base.Initialize(name, config);

            ValidatingPassword += DormouseMembership_ValidatingPassword;
            SetConfigurationProperties(config);
            CheckEncryptionKey();
        }

        private string SetDefaultName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                name = "DormouseMembershipProvider";
            }
            return name;
        }

        private NameValueCollection SetDefaultConfigValues(NameValueCollection config)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Dormouse Membership Provider");
            }
            return config;
        }

        private void SetConfigurationProperties(NameValueCollection config)
        {
            _applicationName = GetConfigValue(config["applicationName"],
                                              System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            _maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            _passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            _minRequiredNonAlphanumericCharacters =
                Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            _minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            _passwordStrengthRegularExpression =
                Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], String.Empty));
            _enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            _enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            _requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            _requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            SetPasswordFormat(config["passwordFormat"]);
            //if (_machineKey == null) _machineKey = GetMachineKeySection();
        }

        private static string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
            {
                return defaultValue;
            }
            return configValue;
        }

        private MachineKeySection GetMachineKeySection()
        {
            System.Configuration.Configuration cfg =
                WebConfigurationManager.OpenWebConfiguration(
                    System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            return cfg.GetSection("system.web/machineKey") as MachineKeySection;
        }

        private bool CheckEncryptionKey()
        {
            if (_machineKey.ValidationKey.Contains("AutoGenerate"))
            {
                if (PasswordFormat != MembershipPasswordFormat.Clear)
                {
                    throw new ProviderException(
                        "Hashed or Encrypted passwords are not supported with auto-generated keys.");
                }
            }
            return true;
        }

        private void SetPasswordFormat(string passwordFormat)
        {
            if (passwordFormat == null)
            {
                passwordFormat = "CLEAR";
            }

            switch (passwordFormat.ToUpper())
            {
                case "HASHED":
                    _passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "ENCRYPTED":
                    _passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "CLEAR":
                    _passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }
        }

        #endregion

        #region Implemented Abstract Methods from MembershipProvider

        /// <summary>
        /// Change the user password.
        /// </summary>
        /// <param name="username">UserName</param>
        /// <param name="oldPwd">Old password.</param>
        /// <param name="newPwd">New password.</param>
        /// <returns>T/F if password was changed.</returns>
        public override bool ChangePassword(string username, string oldPwd, string newPwd)
        {
            if (!ValidateUser(username, oldPwd))
            {
                return false;
            }
            ValidateNewPasswordFormat(username, newPwd);
            try
            {
                ChangeUserPassword(username, newPwd);
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
            return true;
        }

        private void ChangeUserPassword(string username, string newPwd)
        {
            User u = _memberRepo.GetUserByName(username, _applicationName);
            u.Password = EncodePassword(newPwd);
            _memberRepo.SaveUser(u);
        }

        private void ValidateNewPasswordFormat(string username, string newPwd)
        {
            var args = new ValidatePasswordEventArgs(username, newPwd, false);
            args.FailureInformation =
                new MembershipPasswordException("Change password canceled due to new password validation failure.");
            OnValidatingPassword(args);
            if (args.Cancel)
            {
                throw args.FailureInformation;
            }
        }

        /// <summary>
        /// Change the question and answer for a password validation.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="password">Password.</param>
        /// <param name="newPwdQuestion">New question text.</param>
        /// <param name="newPwdAnswer">New answer text.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPwdQuestion,
                                                             string newPwdAnswer)
        {
            if (ValidateUser(username, password))
            {
                try
                {
                    UpdateUserQuestionAndAnswer(username, newPwdQuestion, newPwdAnswer);
                }
                catch (Exception ex)
                {
                    throw new MemberAccessException("Error processing membership data - " + ex.Message);
                }
                return true;
            }
            return false;
        }

        private void UpdateUserQuestionAndAnswer(string username, string newPwdQuestion, string newPwdAnswer)
        {
            var u = _memberRepo.GetUserByName(username, _applicationName);
            u.PasswordQuestion = EncodePassword(newPwdQuestion);
            u.PasswordAnswer = EncodePassword(newPwdAnswer);
            _memberRepo.SaveUser(u);
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="password">Password.</param>
        /// <param name="email">Email address.</param>
        /// <param name="passwordQuestion">Security quesiton for password.</param>
        /// <param name="passwordAnswer">Security quesiton answer for password.</param>
        /// <param name="isApproved"></param>
        /// <param name="userID">User ID</param>
        /// <param name="status"></param>
        /// <returns>MembershipUser</returns>
        public override MembershipUser CreateUser(string username, string password, string email,
                                                  string passwordQuestion, string passwordAnswer, bool isApproved,
                                                  object userID, out MembershipCreateStatus status)
        {
            try
            {
                ValidateNewPasswordFormat(username, password);
            }
            catch (Exception)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            if (DuplicatesEmailAddress(email))
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }
            MembershipUser membershipUser = GetUser(username, false);
            if (membershipUser == null)
            {
                User u = new User();
                u.UserName = username;
                u.ApplicationName = _applicationName;
                u.Password = EncodePassword(password);
                u.EMail = email;
                u.PasswordQuestion = passwordQuestion;
                u.PasswordAnswer = EncodePassword(passwordAnswer);
                u.IsApproved = isApproved;
                u.Comment = String.Empty;
                u.CreationDate = DateTime.Now;
                try
                {
                    _memberRepo.SaveUser(u);
                    status = MembershipCreateStatus.Success;
                }
                catch (Exception ex)
                {
                    throw new MemberAccessException("Error processing membership data - " + ex.Message);
                }
                return GetUser(username, false);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }
            return null;
        }

        private bool DuplicatesEmailAddress(string email)
        {
            var dupEmail = false;
            if (RequiresUniqueEmail)
            {
                if (GetUserNameByEmail(email) != String.Empty)
                {
                    dupEmail = true;
                }
            }
            return dupEmail;
        }

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="deleteAllRelatedData">Whether to delete all related data.</param>
        /// <returns>T/F if the user was deleted.</returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            try
            {
                DeleteUserDataByName(username);
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
            return true;
        }

        private void DeleteUserDataByName(string username)
        {
            User u = _memberRepo.GetUserByName(username, _applicationName);
            if (u != null)
            {
                _memberRepo.DeleteUser(u);
            }
        }

        /// <summary>
        /// Get a collection of users.
        /// </summary>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="totalRecords">Total # of records to retrieve.</param>
        /// <returns>Collection of MembershipUser objects.</returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                totalRecords = 0;
                var users = GetListOfUsers(pageIndex, pageSize);
                totalRecords = users.Count;
                return users;
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
        }

        private MembershipUserCollection GetListOfUsers(int pageIndex, int pageSize)
        {
            var users = new MembershipUserCollection();
            var uList = _memberRepo.GetAllUsers(pageIndex, pageSize, _applicationName);
            foreach (var u in uList)
            {
                users.Add(GetUserFromObject(u));
            }
            return users;
        }

        /// <summary>
        /// Gets the number of users currently on-line.
        /// </summary>
        /// <returns>  /// # of users on-line.</returns>
        public override int GetNumberOfUsersOnline()
        {
            var onlineSpan = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            var compareTime = DateTime.Now.Subtract(onlineSpan);
            try
            {
                return _memberRepo.GetNumberOfUsersOnline(compareTime, _applicationName);
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
        }

        /// <summary>
        /// Get the password for a user.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="answer">Answer to security question.</param>
        /// <returns>Password for the user.</returns>
        public override string GetPassword(string username, string answer)
        {
            if (EnablePasswordRetrieval)
            {
                if (PasswordFormat == MembershipPasswordFormat.Hashed)
                {
                    throw new ProviderException("Cannot retrieve Hashed passwords.");
                }
                string password = RetrievePasswordFromDatabase(username, answer);
                return password;
            }
            throw new ProviderException("Password Retrieval Not Enabled.");
        }

        private string RetrievePasswordFromDatabase(string username, string answerGiven)
        {
            string password;
            string passwordAnswer;

            try
            {
                User curUser;
                curUser = _memberRepo.GetUserByName(username, _applicationName);
                password = curUser.Password;
                passwordAnswer = curUser.PasswordAnswer;
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }

            if (BadPasswordAnswerProvided(answerGiven, passwordAnswer))
            {
                UpdateFailureCount(username, FailureType.PasswordAnswer);
                throw new MembershipPasswordException("Incorrect password answer.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                password = UnEncodePassword(password);
            }
            return password;
        }

        private bool BadPasswordAnswerProvided(string answer, string passwordAnswer)
        {
            var badAnswer = false;
            if (RequiresQuestionAndAnswer)
            {
                if (CheckPassword(answer, passwordAnswer))
                {
                    badAnswer = false;
                }
                else
                {
                    badAnswer = true;
                }
            }
            return badAnswer;
        }

        /// <summary>
        /// Get a user record
        /// </summary>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser membershipUser = null;

            try
            {
                User u = _memberRepo.GetUserByName(username, _applicationName);
                if (u != null)
                {
                    if (userIsOnline)
                    {
                        u.LastActivityDate = DateTime.Now;
                        _memberRepo.SaveUser(u);
                    }
                    membershipUser = GetUserFromObject(u);
                }
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Unable to retrieve user data - " + ex.Message);
            }

            return membershipUser;
        }

        /// <summary>
        /// Get a user based upon provider key and if they are on-line.
        /// </summary>
        /// <param name="userID">Provider key.</param>
        /// <param name="userIsOnline">T/F whether the user is on-line.</param>
        /// <returns></returns>
        public override MembershipUser GetUser(object userID, bool userIsOnline)
        {
            MembershipUser membershipUser = null;

            try
            {
                int id;
                if (Int32.TryParse(userID.ToString(), out id))
                {
                    User u = _memberRepo.GetUserByID(id);
                    if (u != null)
                    {
                        if (userIsOnline)
                        {
                            u.LastActivityDate = DateTime.Now;
                            _memberRepo.SaveUser(u);
                        }
                        membershipUser = GetUserFromObject(u);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }

            return membershipUser;
        }

        /// <summary>
        /// Unlock a user.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <returns>T/F if unlocked.</returns>
        public override bool UnlockUser(string username)
        {
            try
            {
                var u = _memberRepo.GetUserByName(username, _applicationName);
                if (u != null)
                {
                    u.IsLockedOut = false;
                    _memberRepo.SaveUser(u);
                }
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
            return true;
        }

        public override string GetUserNameByEmail(string email)
        {
            try
            {
                return _memberRepo.GetUserNameByEMail(email, _applicationName);
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
        }

        /// <summary>
        /// Reset the user password.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="answer">Answer to security question.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("Password Reset is not enabled.");
            }

            if ((answer == null) && (RequiresQuestionAndAnswer))
            {
                UpdateFailureCount(username, FailureType.PasswordAnswer);
                throw new ProviderException("Password answer required for password Reset.");
            }

            string newPassword = "#" + Guid.NewGuid().ToString().Substring(0, 8) + "$";
            var args = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(args);
            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }
                else
                {
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");
                }
            }

            string passwordAnswer = String.Empty;
            try
            {
                User u = _memberRepo.GetUserByName(username, _applicationName);
                if (u.IsLockedOut)
                {
                    throw new MembershipPasswordException("The supplied user is locked out.");
                }
                passwordAnswer = u.PasswordAnswer;
                if (RequiresQuestionAndAnswer && (!CheckPassword(answer, passwordAnswer)))
                {
                    UpdateFailureCount(username, FailureType.PasswordAnswer);
                    throw new MembershipPasswordException("Incorrect password answer.");
                }
                u.Password = newPassword;
                _memberRepo.SaveUser(u);
                return newPassword;
            }
            catch (Exception ex)
            {
                throw new MembershipPasswordException(ex.Message);
            }
        }

        /// <summary>
        /// Update the user information.
        /// </summary>
        /// <param name="_membershipUser">MembershipUser object containing data.</param>
        public override void UpdateUser(MembershipUser membershipUser)
        {
            try
            {
                User u = _memberRepo.GetUserByName(membershipUser.UserName, _applicationName);
                u.EMail = membershipUser.Email;
                u.Comment = membershipUser.Comment;
                u.IsApproved = membershipUser.IsApproved;
                _memberRepo.SaveUser(u);
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
        }

        /// <summary>
        /// Validate the user based upon username and password.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="password">Password.</param>
        /// <returns>T/F if the user is valid.</returns>
        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;

            try
            {
                User u = _memberRepo.GetUserByName(username, _applicationName);
                if (u != null)
                {
                    string storedPassword = u.Password;
                    bool isApproved = u.IsApproved;
                    if (CheckPassword(password, storedPassword))
                    {
                        if (isApproved)
                        {
                            isValid = true;
                            u.LastLoginDate = DateTime.Now;
                            _memberRepo.SaveUser(u);
                        }
                    }
                    else
                    {
                        UpdateFailureCount(username, FailureType.Password);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
            return isValid;
        }

        /// <summary>
        /// Find all users matching a search string.
        /// </summary>
        /// <param name="usernameToMatch">Search string of user name to match.</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords">Total records found.</param>
        /// <returns>Collection of MembershipUser objects.</returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize,
                                                                 out int totalRecords)
        {
            var users = new MembershipUserCollection();
            totalRecords = 0;

            try
            {
                var uList = _memberRepo.FindUsersByName(usernameToMatch, pageIndex, pageSize, _applicationName);
                foreach (User u in uList)
                {
                    users.Add(GetUserFromObject(u));
                }
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
            totalRecords = users.Count;
            return users;
        }

        /// <summary>
        /// Find all users matching a search string of their email.
        /// </summary>
        /// <param name="emailToMatch">Search string of email to match.</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords">Total records found.</param>
        /// <returns>Collection of MembershipUser objects.</returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize,
                                                                  out int totalRecords)
        {
            var users = new MembershipUserCollection();
            totalRecords = 0;

            try
            {
                var uList = _memberRepo.FindUsersByEMail(emailToMatch, pageIndex, pageSize, _applicationName);
                foreach (User u in uList)
                {
                    users.Add(GetUserFromObject(u));
                }
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
            totalRecords = users.Count;
            return users;
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Create a MembershipUser object from a user object.
        /// </summary>
        /// <param name="u">User Object.</param>
        /// <returns>MembershipUser object.</returns>
        private MembershipUser GetUserFromObject(User u)
        {
            var creationDate = (DateTime) u.CreationDate;
            var lastLoginDate = new DateTime();
            if (u.LastLoginDate != null)
            {
                lastLoginDate = (DateTime) u.LastLoginDate;
            }
            var lastActivityDate = new DateTime();
            if (u.LastActivityDate != null)
            {
                lastActivityDate = (DateTime) u.LastActivityDate;
            }
            var lastPasswordChangedDate = new DateTime();
            if (u.LastPasswordChangedDate != null)
            {
                lastPasswordChangedDate = (DateTime) u.LastPasswordChangedDate;
            }
            var lastLockedOutDate = new DateTime();
            if (u.LastLockedOutDate != null)
            {
                lastLockedOutDate = (DateTime) u.LastLockedOutDate;
            }

            var membershipUser = new MembershipUser(
                this.Name,
                u.UserName,
                u.UserId,
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

        /// <summary>
        /// Converts a hexadecimal string to a byte array. Used to convert encryption key values from the configuration
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length/2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i*2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// Update password and answer failure information.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="failureType">Type of failure</param>
        /// <remarks></remarks>
        private void UpdateFailureCount(string username, FailureType failureType)
        {
            try
            {
                User u = _memberRepo.GetUserByName(username, _applicationName);
                if (u != null)
                {
                    if (failureType == FailureType.Password) u.FailedPasswordAttemptCount++;
                    else u.FailedPasswordAnswerAttemptCount++;
                    _memberRepo.SaveUser(u);
                }
            }
            catch (Exception ex)
            {
                throw new MemberAccessException("Error processing membership data - " + ex.Message);
            }
        }

        private void DormouseMembership_ValidatingPassword(object sender, ValidatePasswordEventArgs e)
        {
            //Enforce our criteria
            var errorMessage = "";
            var pwChar = e.Password.ToCharArray();
            //Check Length
            if (e.Password.Length < _minRequiredPasswordLength)
            {
                errorMessage += "[Minimum length: " + _minRequiredPasswordLength + "]";
                e.Cancel = true;
            }
            //Check Strength
            if (_passwordStrengthRegularExpression != string.Empty)
            {
                Regex r = new Regex(_passwordStrengthRegularExpression);
                if (!r.IsMatch(e.Password))
                {
                    errorMessage += "[Insufficient Password Strength]";
                    e.Cancel = true;
                }
            }
            //Check Non-alpha characters
            int iNumNonAlpha = 0;
            Regex rAlpha = new Regex(@"\w");
            foreach (char c in pwChar)
            {
                if (!char.IsLetterOrDigit(c)) iNumNonAlpha++;
            }
            if (iNumNonAlpha < _minRequiredNonAlphanumericCharacters)
            {
                errorMessage += "[Insufficient Non-Alpha Characters]";
                e.Cancel = true;
            }
            e.FailureInformation = new MembershipPasswordException(errorMessage);
        }

        /// <summary>
        /// Check the password format based upon the MembershipPasswordFormat.
        /// </summary>
        /// <param name="password">Password</param>
        /// <param name="dbpassword"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            if (pass1 == pass2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Encode password.
        /// </summary>
        /// <param name="password">Password.</param>
        /// <returns>Encoded password.</returns>
        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                        Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1();
                    hash.Key = HexToByte(_machineKey.ValidationKey);
                    encodedPassword =
                        Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }

        /// <summary>
        /// UnEncode password.
        /// </summary>
        /// <param name="encodedPassword">Password.</param>
        /// <returns>Unencoded password.</returns>
        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                        Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        #endregion
    }
}