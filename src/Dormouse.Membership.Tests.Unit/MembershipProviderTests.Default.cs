using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Security;
using Dormouse.Membership.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Dormouse.Membership.Tests.Unit
{
    [TestClass]
    public class MembershipProviderTestsDefault
    {
        private MemberProv _mProv;

        #region Setup and Teardown
        [TestInitialize()]
        public void InitializeTest()
        {
            _mProv = new MemberProv(Utilities.GetMockRepository(),Utilities.GetUnencryptedMachineKey());
            var config = new NameValueCollection();
            _mProv.Initialize("",config);
        }

        [TestCleanup()]
        public void CleanupTest()
        {
            _mProv = null;
        }
        #endregion

        #region Test Properties
        [TestMethod]
        public void Exercise_Public_Properties()
        {
            //Arrange
            //Act
            var applicationName = _mProv.ApplicationName;
            _mProv.ApplicationName = applicationName;
            var maxInvalidPasswordAttempts = _mProv.MaxInvalidPasswordAttempts;
            var minRequiredNonAlphanumericCharacters = _mProv.MinRequiredNonAlphanumericCharacters;
            var minRequiredPasswordLength = _mProv.MinRequiredPasswordLength;
            var passwordAttemptWindow = _mProv.PasswordAttemptWindow;
            var passwordStrengthRegularExpression = _mProv.PasswordStrengthRegularExpression;
            //Assert
            Assert.AreEqual(_mProv.ApplicationName, applicationName);
            Assert.AreEqual(_mProv.MaxInvalidPasswordAttempts, maxInvalidPasswordAttempts);
            Assert.AreEqual(_mProv.MinRequiredNonAlphanumericCharacters, minRequiredNonAlphanumericCharacters);
            Assert.AreEqual(_mProv.MinRequiredPasswordLength, minRequiredPasswordLength);
            Assert.AreEqual(_mProv.PasswordAttemptWindow, passwordAttemptWindow);
            Assert.AreEqual(_mProv.PasswordStrengthRegularExpression, passwordStrengthRegularExpression);
        }
        #endregion

        #region Test ChangePassword Method
        [TestMethod]
        public void ChangePassword_GoodUserGoodPass_ReturnsTrue()
        {
            //Arrange
            var user = "GoodUser";
            var oldpass = "GoodPass";
            var newpass = "ABC123!?";
            //Act
            var actual = _mProv.ChangePassword(user, oldpass, newpass);
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(MembershipPasswordException))]
        public void ChangePassword_GoodUserBadPass_ThrowsException()
        {
            //Arrange
            var user = "GoodUser";
            var oldpass = "GoodPass";
            var newpass = "Bad";
            //Act
            var actual = _mProv.ChangePassword(user, oldpass, newpass);
            //Assert
        }

        [TestMethod]
        public void ChangePassword_BadUserBadPass_ReturnsFalse()
        {
            //Arrange
            var user = "GoodUser";
            var oldpass = "BadPass";
            var newpass = "Bad";
            //Act
            var actual = _mProv.ChangePassword(user, oldpass, newpass);
            //Assert
            Assert.IsFalse(actual);
        }
        #endregion

        #region Test ChangePasswordQuestionAndAnswer Method
        [TestMethod]
        public void ChangePasswordQuestionAndAnswer_GoodUser_ReturnsTrue()
        {
            //Arrange
            var user = "GoodUser";
            var pass = "GoodPass";
            var question = "Good";
            var answer = "Answer";
            //Act
            var actual = _mProv.ChangePasswordQuestionAndAnswer(user, pass, question, answer);
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ChangePasswordQuestionAndAnswer_BadUser_ReturnsFalse()
        {
            //Arrange
            var user = "BadUser";
            var pass = "BadPass";
            var question = "Good";
            var answer = "Answer";
            //Act
            var actual = _mProv.ChangePasswordQuestionAndAnswer(user, pass, question, answer);
            //Assert
            Assert.IsFalse(actual);
        }
        #endregion

        #region Test CreateUser Method
        [TestMethod]
        public void CreateUser_ValidData_ReturnsSuccess()
        {
            //Arrange
            var user = "NewUser";
            var pass = "ABC123!?";
            var email = "NewEmail";
            var approved = true;
            var question = "Question";
            var answer = "Answer";
            var key = 1;
            var result = new MembershipCreateStatus();
            //Act
            var actual = _mProv.CreateUser(user, pass, email, question, answer, approved, key, out result);
            //Assert
            Assert.AreEqual(MembershipCreateStatus.Success, result);
        }

        [TestMethod]
        public void CreateUser_InvalidPassword_ReturnsInvalidPassword()
        {
            //Arrange
            var user = "NewUser";
            var pass = "Bad";
            var email = "NewEmail";
            var approved = true;
            var question = "Question";
            var answer = "Answer";
            var key = 1;
            var result = new MembershipCreateStatus();
            //Act
            var actual = _mProv.CreateUser(user, pass, email, question, answer, approved, key, out result);
            //Assert
            Assert.AreEqual(MembershipCreateStatus.InvalidPassword, result);
        }

        [TestMethod]
        public void CreateUser_DupUser_ReturnsDupUser()
        {
            //Arrange
            var user = "GoodUser";
            var pass = "ABC123!?";
            var email = "NewEmail";
            var approved = true;
            var question = "Question";
            var answer = "Answer";
            var key = 1;
            var result = new MembershipCreateStatus();
            //Act
            var actual = _mProv.CreateUser(user, pass, email, question, answer, approved, key, out result);
            //Assert
            Assert.AreEqual(MembershipCreateStatus.DuplicateUserName, result);
        }

        [TestMethod]
        public void CreateUser_DupEMail_ReturnsDupEMail()
        {
            //Arrange
            var user = "GoodUser";
            var pass = "ABC123!?";
            var email = "DupEmail";
            var approved = true;
            var question = "Question";
            var answer = "Answer";
            var key = 1;
            var result = new MembershipCreateStatus();
            //Act
            var actual = _mProv.CreateUser(user, pass, email, question, answer, approved, key, out result);
            //Assert
            Assert.AreEqual(MembershipCreateStatus.DuplicateEmail, result);
        }
        #endregion

        #region Test DeleteUser Method
        [TestMethod]
        public void DeleteUser_GoodUser_ReturnsTrue()
        {
            //Arrange   
            var username = "GoodUser";
            //Act
            var actual = _mProv.DeleteUser(username, true);
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void DeleteUser_BadUser_ReturnsTrue()
        {
            //Arrange   
            var username = "BadUser";
            //Act
            var actual = _mProv.DeleteUser(username, true);
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void DeleteUser_ExceptionUser_ThrowsException()
        {
            //Arrange   
            var username = "ExceptionUser";
            //Act
            var actual = _mProv.DeleteUser(username, true);
            //Assert
        }
        #endregion

        #region Test FindUserByEmail Method
        [TestMethod]
        public void FindUserByEmail_GivenGoodEmail_ReturnsOneRecord()
        {
            //Arrange
            var email = "GoodEmail";
            var recs = -1;
            var expectedRecs = 1;
            //Act
            var actual = _mProv.FindUsersByEmail(email, 0, 99, out recs);
            //Assert
            Assert.AreEqual(expectedRecs, recs);
            Assert.AreEqual(expectedRecs, actual.Count);
        }

        [TestMethod]
        public void FindUserByEmail_GivenDuplicateEmail_ReturnsTwoRecords()
        {
            //Arrange
            var email = "DupEmail";
            var recs = -1;
            var expectedRecs = 2;
            //Act
            var actual = _mProv.FindUsersByEmail(email, 0, 99, out recs);
            //Assert
            Assert.AreEqual(expectedRecs, recs);
            Assert.AreEqual(expectedRecs, actual.Count);
        }

        [TestMethod]
        public void FindUserByEmail_GivenBadEmail_ReturnsZeroRecords()
        {
            //Arrange
            var email = "BadEmail";
            var recs = -1;
            var expectedRecs = 0;
            //Act
            var actual = _mProv.FindUsersByEmail(email, 0, 99, out recs);
            //Assert
            Assert.AreEqual(expectedRecs, recs);
            Assert.AreEqual(expectedRecs, actual.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void FindUserByEmail_GivenException_ThrowsMemberAccessException()
        {
            //Arrange
            var email = "ExceptionUser";
            var recs = -1;
            //Act
            _mProv.FindUsersByEmail(email, 0, 99, out recs);
            //Assert
        }
        #endregion

        #region Test FindUserByName Method
        [TestMethod]
        public void FindUserByName_GivenGoodName_ReturnsOneRecord()
        {
            //Arrange
            var Name = "GoodName";
            var recs = -1;
            var expectedRecs = 1;
            //Act
            var actual = _mProv.FindUsersByName(Name, 0, 99, out recs);
            //Assert
            Assert.AreEqual(expectedRecs, recs);
            Assert.AreEqual(expectedRecs, actual.Count);
        }

        [TestMethod]
        public void FindUserByName_GivenDuplicateName_ReturnsTwoRecords()
        {
            //Arrange
            var Name = "DupName";
            var recs = -1;
            var expectedRecs = 2;
            //Act
            var actual = _mProv.FindUsersByName(Name, 0, 99, out recs);
            //Assert
            Assert.AreEqual(expectedRecs, recs);
            Assert.AreEqual(expectedRecs, actual.Count);
        }

        [TestMethod]
        public void FindUserByName_GivenBadName_ReturnsZeroRecords()
        {
            //Arrange
            var Name = "BadName";
            var recs = -1;
            var expectedRecs = 0;
            //Act
            var actual = _mProv.FindUsersByName(Name, 0, 99, out recs);
            //Assert
            Assert.AreEqual(expectedRecs, recs);
            Assert.AreEqual(expectedRecs, actual.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void FindUserByName_GivenException_ThrowsMemberAccessException()
        {
            //Arrange
            var Name = "ExceptionUser";
            var recs = -1;
            //Act
            _mProv.FindUsersByName(Name, 0, 99, out recs);
            //Assert
        }
        #endregion

        #region Test GetAllUsers Method
        [TestMethod]
        public void GetAllUsers_GivenTwoUsers_ReturnsTwoUsers()
        {
            //Arrange
            var expected = 2;
            var tot = -1;
            //Act
            var actual = _mProv.GetAllUsers(0, 99, out tot);
            //Assert
            Assert.AreEqual(expected, actual.Count);
            Assert.AreEqual(expected, tot);
        }

        [TestMethod]
        public void GetAllUsers_GivenZeroUsers_ReturnsZeroUsers()
        {
            //Arrange
            var expected = 0;
            var tot = -1;
            //Act
            var actual = _mProv.GetAllUsers(1, 99, out tot);
            //Assert
            Assert.AreEqual(expected, actual.Count);
            Assert.AreEqual(expected, tot);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))] 
        public void GetAllUsers_GivenExceptionUser_ThrowsException()
        {
            //Arrange
            var tot = -1;
            //Act
            var actual = _mProv.GetAllUsers(2, 99, out tot);
            //Assert
        }
        #endregion

        #region Test GetNumberOfUsersOnline Method
        [TestMethod]
        public void GetNumberOfUsersOnline_GivenTwoUsers_ReturnsTwoUsers()
        {
            //Arrange
            var u = Utilities.GetMockRepository();
            u.Stub(v => v.GetNumberOfUsersOnline(DateTime.Now, null)).IgnoreArguments().Return(2);
            var tmpProv = new MemberProv(u, Utilities.GetUnencryptedMachineKey());
            var expected = 2;
            //Act
            var actual = tmpProv.GetNumberOfUsersOnline();
            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetNumberOfUsersOnline_GivenZeroUsers_ReturnsZeroUsers()
        {
            //Arrange
            var u = Utilities.GetMockRepository();
            u.Stub(v => v.GetNumberOfUsersOnline(DateTime.Now, null)).IgnoreArguments().Return(0);
            var tmpProv = new MemberProv(u, Utilities.GetUnencryptedMachineKey());
            var expected = 0;
            //Act
            var actual = tmpProv.GetNumberOfUsersOnline();
            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void GetNumberOfUsersOnline_GivenExceptionUser_ThrowsException()
        {
            //Arrange
            var u = Utilities.GetMockRepository();
            u.Stub(v => v.GetNumberOfUsersOnline(DateTime.Now, null)).IgnoreArguments().Throw(new Exception());
            var tmpProv = new MemberProv(u, Utilities.GetUnencryptedMachineKey());
            //Act
            var actual = tmpProv.GetNumberOfUsersOnline();
            //Assert
        }
        #endregion

        #region Test GetPassword Method
        [TestMethod]
        public void GetPassword_GivenGoodUserAndGoodAnswer_ReturnsPassword()
        {
            //Arrange
            var name = "GoodUser";
            var answer = "GoodAnswer";
            var expected = "GoodPass";
            //Act
            var actual = _mProv.GetPassword(name, answer);
            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPassword_GivenGoodUserAndBadAnswer_WithoutRequireAnswer_ReturnsPassword()
        {
            //Arrange
            var name = "BadAnswerUser";
            var answer = "BadAnswer";
            var expected = "GoodPass";
            //Act
            var actual = _mProv.GetPassword(name, answer);
            //Assert
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void GetPassword_GivenBadUser_ThrowsException()
        {
            //Arrange
            var name = "BadUser";
            var answer = "BadAnswer";
            //Act
            _mProv.GetPassword(name, answer);
            //Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void GetPassword_WhenRetrievalDisabled_ThrowsException()
        {
            //Arrange
            var noPassProv = Utilities.GetProviderWithNoPasswordRetrievalOrReset();
            var name = "BadUser";
            var answer = "BadAnswer";
            //Act
            noPassProv.GetPassword(name, answer);
            //Assert
        }
        #endregion

        #region Test GetUser Method
        [TestMethod]
        public void GetUser_GoodUserOnline_ReturnsUser()
        {
            //Arrange
            var name = "GoodUser";
            //Act
            var actual = _mProv.GetUser(name, true);
            //Assert
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void GetUser_BadUser_ThrowsException()
        {
            //Arrange
            var name = "ExceptionUser";
            //Act
            _mProv.GetUser(name, true);
            //Assert
        }


        [TestMethod]
        public void GetUser_GoodUserIdOnline_ReturnsUser()
        {
            //Arrange
            var id = 1;
            //Act
            var actual = _mProv.GetUser(id, true);
            //Assert
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void GetUser_BadUserId_ThrowsException()
        {
            //Arrange
            var id = 999;
            //Act
            _mProv.GetUser(id, true);
            //Assert
        }
        #endregion

        #region Test GetUserNameByEmail Method
        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void GetUserNameByEmail_ExceptionUser_ThrowsException()
        {
            //Arrange
            var email = "ExceptionEmail";
            //Act
            _mProv.GetUserNameByEmail(email);
            //Assert
        }
        #endregion

        #region Test Initialize Method
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_NullConfig_ThrowsArgumentNullException()
        {
            //Arrange
            _mProv = new MemberProv(Utilities.GetMockRepository(), Utilities.GetUnencryptedMachineKey());
            //Act
            _mProv.Initialize("", null);
            //Assert
        }

        [TestMethod]
        public void Initialize_NullName_SetsDefaultName()
        {
            //Arrange
            _mProv = new MemberProv(Utilities.GetMockRepository(), Utilities.GetUnencryptedMachineKey());
            var expected = "DormouseMembershipProvider";
            //Act
            _mProv.Initialize("", new NameValueCollection());
            var actual = _mProv.Name;
            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void Initialize_CheckEncryptionKeyFails_ThrowsProviderException()
        {
            //Arrange
            var tmpProv = new MemberProv(Utilities.GetMockRepository(), Utilities.GetUnencryptedMachineKey());
            //Act
            var config = new NameValueCollection();
            config.Add("passwordFormat", "Hashed");
            tmpProv.Initialize("", config);
            //Assert
        }
        #endregion

        #region Test ResetPassword Method
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ResetPassword_WhenRetrievalDisabled_ThrowsException()
        {
            //Arrange
            var noPassProv = Utilities.GetProviderWithNoPasswordRetrievalOrReset();
            var name = "BadUser";
            var answer = "BadAnswer";
            //Act
            noPassProv.ResetPassword(name, answer);
            //Assert
        }

        [TestMethod]
        public void ResetPassword_GoodUserNoAnswer_ReturnsNewPassword()
        {
            //Arrange
            var name = "GoodUser";
            //Act
            var actual = _mProv.ResetPassword(name, null);
            //Assert
            Assert.AreNotEqual("", actual);
        }

        [TestMethod]
        [ExpectedException(typeof(MembershipPasswordException))]
        public void ResetPassword_LockedUser_ThrowsException()
        {
            //Arrange
            var name = "LockedUser";
            //Act
            var actual = _mProv.ResetPassword(name, null);
            //Assert
        }
        #endregion

        #region Test UnlockUser Method
        [TestMethod]
        public void UnlockUser_GoodUser_ReturnsTrue()
        {
            //Arrange
            var name = "GoodUser";
            //Act
            var actual = _mProv.UnlockUser(name);
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void UnlockUser_ExceptionUser_ThrowsException()
        {
            //Arrange
            var name = "ExceptionUser";
            //Act
            var actual = _mProv.UnlockUser(name);
            //Assert
        }
        #endregion

        #region Test UpdateUser Method
        [TestMethod]
        public void UpdateUser_GoodUser_DoesNotThrowError()
        {
            //Arrange
            var m = _mProv.GetUser("GoodUser", true);
            //Act
            _mProv.UpdateUser(m);
            //Assert
            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void UpdateUser_BadUser_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.UpdateUser(null);
            //Assert
        }
        #endregion

        #region Test ValidateUser Method 
        [TestMethod]
        public void ValidateUser_GivenGoodUserGoodPassword_ReturnsTrue()
        {
            //Arrange
            var userName = "GoodUser";
            var userPass = "GoodPass";
            //Act
            var actual = _mProv.ValidateUser(userName, userPass);
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ValidateUser_GivenGoodUserBadPassword_ReturnsFalse()
        {
            //Arrange
            var userName = "GoodUser";
            var userPass = "BadPass";
            //Act
            var actual = _mProv.ValidateUser(userName, userPass);
            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ValidateUser_GivenBadUserBadPassword_ReturnsFalse()
        {
            //Arrange
            var userName = "BadUser";
            var userPass = "BadPass";
            //Act
            var actual = _mProv.ValidateUser(userName, userPass);
            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(MemberAccessException))]
        public void ValidateUser_GivenException_ThrowsMemberAccessException()
        {
            //Arrange
            var userName = "ExceptionUser";
            var userPass = "BadPass";
            //Act
            var actual = _mProv.ValidateUser(userName, userPass);
            //Assert
        }
        #endregion 
    }
}
