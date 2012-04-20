using System.Collections.Generic;
using System.Configuration.Provider;
using Dormouse.Membership.Data;
using Dormouse.Membership.Model;
using Dormouse.Membership.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Security;
using System.Collections.Specialized;
using System;

namespace Dormouse.Membership.Tests.Integration
{
    [TestClass()]
    public class MembershipProviderTestsDefault
    {
        private MemberProv _mp;
        private MembershipRepository _memberRepo = new MembershipRepository();
        private static NameValueCollection _testConfig;
        private static List<UserParameters> _testUsers;

        #region Setup and Teardown
        [ClassInitialize()]
        public static void ClassSetup(TestContext testContext)
        {
            //Set up a test configuration to use.
            _testConfig = new NameValueCollection();
            _testConfig.Add("applicationName","DMTestApp");

            //We will create three test users to work with while
            //exercising our tests.  These will be used for various
            //read and find operations.
            var mpSetup = new MemberProv();
            mpSetup.Initialize("DormouseMembershipProvider", _testConfig);
            _testUsers = TestUtils.GetTestUsers(5, "Default");
            foreach (var u in _testUsers)
            {
                MembershipCreateStatus status;
                mpSetup.CreateUser(u.username, u.password, u.email, u.passwordQuestion,
                    u.passwordAnswer, u.isApproved, u.providerUserKey, out status);
            }
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            //We will remove our sample users
            var mpCleanup = new MemberProv();
            mpCleanup.Initialize("DormouseMembershipProvider", _testConfig);
            foreach (var user in _testUsers)
            {
                mpCleanup.DeleteUser(user.username, true);
            }
        }

        [TestInitialize()]
        public void TestSetup()
        {
            _mp = new MemberProv();
            _mp.Initialize("DormouseMembershipProvider", _testConfig);
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            _mp = null;
        }
        #endregion

        #region Test Initialize Method
        [TestMethod]
        public void Initialize_ValidParms_DoesNotThrowException()
        {
            //Arrange
            var mpTemp = new MemberProv();
            //Act
            mpTemp.Initialize("DormouseMembershipProvider", _testConfig);
            //Assert
            Assert.IsNotNull(mpTemp.ApplicationName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_EmptyConfig_ThrowsException()
        {
            //Arrange
            var mpTemp = new MemberProv();
            //Act
            mpTemp.Initialize("DormouseMembershipProvider", null);
        }

        [TestMethod]
        public void Initialize_NullName_SetsDefault()
        {
            //Arrange
            var mpTemp = new MemberProv();
            //Act
            mpTemp.Initialize(null, _testConfig);
            //Assert
            Assert.IsNotNull(mpTemp.ApplicationName);
        }
        #endregion

        #region Test CreateUser Method
        [TestMethod()]
        public void CreateUserTest_ValidTestUser_ReturnsSuccess()
        {
            UserParameters u = TestUtils.GetValidUser();
            MembershipCreateStatus status = new MembershipCreateStatus();
            MembershipCreateStatus statusExpected = MembershipCreateStatus.Success;
            MembershipUser newuser = _mp.CreateUser(u.username, u.password, u.email, u.passwordQuestion,
                u.passwordAnswer, u.isApproved, u.providerUserKey, out status);
            //Cleanup
            if (statusExpected == status) _mp.DeleteUser(u.username,true);
            Assert.AreEqual(statusExpected, status);
        }

        [TestMethod()]
        public void CreateUserTest_InvalidPassword_ReturnsSuccess()
        {
            UserParameters u = TestUtils.GetValidUser();
            MembershipCreateStatus status = new MembershipCreateStatus();
            MembershipCreateStatus statusExpected = MembershipCreateStatus.Success;
            MembershipUser newuser = _mp.CreateUser(u.username, u.password, u.email, u.passwordQuestion,
                u.passwordAnswer, u.isApproved, u.providerUserKey, out status);
            //Cleanup
            if (statusExpected == status) _mp.DeleteUser(u.username, true);
            Assert.AreEqual(statusExpected, status);
        }

        [TestMethod()]
        public void CreateUserTest_DuplicateUserName_ReturnsDuplicateUserName()
        {
            UserParameters u = TestUtils.GetValidUser();
            u.username = _testUsers[0].username;
            MembershipCreateStatus status = new MembershipCreateStatus();
            MembershipCreateStatus statusExpected = MembershipCreateStatus.DuplicateUserName;
            MembershipUser newuser = _mp.CreateUser(u.username, u.password, u.email, u.passwordQuestion,
                u.passwordAnswer, u.isApproved, u.providerUserKey, out status);
            Assert.AreEqual(statusExpected, status);
        }

        [TestMethod()]
        public void CreateUserTest_DuplicateEMail_ReturnsDuplicateEmail()
        {
            UserParameters u = TestUtils.GetValidUser();
            u.email = _testUsers[0].email;
            MembershipCreateStatus status = new MembershipCreateStatus();
            MembershipCreateStatus statusExpected = MembershipCreateStatus.DuplicateEmail;
            MembershipUser newuser = _mp.CreateUser(u.username, u.password, u.email, u.passwordQuestion,
                u.passwordAnswer, u.isApproved, u.providerUserKey, out status);
            Assert.AreEqual(statusExpected, status);
        }

        [TestMethod()]
        public void CreateUserTest_InvalidPassword_ReturnsInvalidPassword()
        {
            UserParameters u = TestUtils.GetValidUser();
            u.password = "";
            MembershipCreateStatus status = new MembershipCreateStatus();
            MembershipCreateStatus statusExpected = MembershipCreateStatus.InvalidPassword;
            MembershipUser newuser = _mp.CreateUser(u.username, u.password, u.email, u.passwordQuestion,
                u.passwordAnswer, u.isApproved, u.providerUserKey, out status);
            Assert.AreEqual(statusExpected, status);
        }
        #endregion

        #region Test ChangePassword Method
        [TestMethod()]
        public void ChangePasswordTest_UnapprovedUser_ReturnsFalse()
        {
            UserParameters u = _testUsers[0];
            //Change user to unapproved
            var user = _mp.GetUser(u.username, true);
            user.IsApproved = false;          
            _mp.UpdateUser(user);

            string newpass = "!Password??999";
            bool result = true;
            result = _mp.ChangePassword(u.username, u.password, newpass);
            //Cleanup
            if (result) _mp.ChangePassword(u.username, newpass, u.password);
            user.IsApproved = true;
            _mp.UpdateUser(user);

            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void ChangePasswordTest_ValidUser_ReturnsTrue()
        {
            UserParameters u = _testUsers[0];
            string newpass = "!Password??999";
            bool result = false;
            result = _mp.ChangePassword(u.username, u.password, newpass);
            //Cleanup
            if (result) _mp.ChangePassword(u.username, newpass, u.password);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void ChangePasswordTest_ValidUser_PasswordChanged()
        {
            UserParameters uparm = _testUsers[2];
            string newpass = "!Password??999";
            _mp.ChangePassword(uparm.username, uparm.password, newpass);
            string curpass = _mp.GetPassword(uparm.username, uparm.passwordAnswer);
            //Cleanup
            _mp.ChangePassword(uparm.username, newpass, uparm.password);
            Assert.AreEqual(newpass,curpass);
        }

        [TestMethod()]
        [ExpectedException(typeof(MembershipPasswordException))]
        public void ChangePasswordTest_BadNewPass_ThrowsException()
        {
            UserParameters u = _testUsers[0];
            string newpass = "";
            _mp.ChangePassword(u.username, u.password, newpass);
        }

        [TestMethod()]
        public void ChangePasswordTest_SamePasswords_ReturnsTrue()
        {
            UserParameters u = _testUsers[0];
            bool result = false;
            result = _mp.ChangePassword(u.username, u.password, u.password);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void ChangePasswordTest_BadOldPass_ReturnsFalse()
        {
            UserParameters u = _testUsers[0];
            string newpass = "!Password??999";
            string badpass = "!!!!BadPass999";
            bool result = true;
            result = _mp.ChangePassword(u.username, badpass, newpass);
            Assert.IsFalse(result);
        }
        #endregion

        #region Test ChangePasswordQuestionAndAnswer Method
        [TestMethod()]
        public void ChangePasswordQuestionAndAnswer_ValidUser_ReturnsTrue()
        {
            UserParameters u = _testUsers[0];
            string newquestion = "question";
            string newanswer = "answer";
            bool result = false;
            result = _mp.ChangePasswordQuestionAndAnswer(u.username, u.password, newquestion, newanswer);
            //Cleanup
            if (result) _mp.ChangePasswordQuestionAndAnswer(u.username, u.password, u.passwordQuestion, u.passwordAnswer);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void ChangePasswordQuestionAndAnswer_ValidUser_QandAChanged()
        {
            UserParameters uparm = _testUsers[3];
            User u = _memberRepo.GetUserByName(uparm.username, _mp.ApplicationName);
            var oldQA = new PasswordQandA() 
                {Answer = u.PasswordAnswer, Question = u.PasswordQuestion};
            var newQA = new PasswordQandA() 
                {Answer = "Lettuce And Carrots", Question = "What Do Rabbits Eat?" };
            
            _mp.ChangePasswordQuestionAndAnswer(uparm.username, uparm.password, newQA.Question, newQA.Answer);
            User uCur = _memberRepo.GetUserByName(uparm.username, _mp.ApplicationName);
            oldQA.Question = uCur.PasswordQuestion;
            oldQA.Answer = uCur.PasswordAnswer;

            //Cleanup
            _mp.ChangePasswordQuestionAndAnswer(uparm.username, uparm.password, uparm.passwordQuestion, uparm.passwordAnswer);
            Assert.AreEqual(oldQA, newQA);            
        }


        [TestMethod()]
        public void ChangePasswordQuestionAndAnswer_BadPassword_ReturnsFalse()
        {
            UserParameters u = _testUsers[0];
            string badpass = "BadPassword";
            bool result = false;
            result = _mp.ChangePasswordQuestionAndAnswer(u.username, badpass, u.passwordQuestion, u.passwordAnswer);
            //Cleanup
            Assert.IsFalse(result);
        }        

        #endregion

        #region Test FindUsersByEMail Method
        [TestMethod()]
        public void FindUsersByEmail_ValidUser_ReturnsOneRecord()
        {
            UserParameters u = _testUsers[0];
            int total = 0;
            MembershipUserCollection users = _mp.FindUsersByEmail(u.email, 0, 5, out total);
            Assert.AreEqual(total,1);
        }

        [TestMethod()]
        public void FindUsersByEmail_InvalidUser_ReturnsNoRecords()
        {
            string email = "InvalidEmailAddress";
            int total = -1;
            MembershipUserCollection users = _mp.FindUsersByEmail(email, 0, 5, out total);
            Assert.AreEqual(total, 0);
        }
        #endregion

        #region Test FindUsersByName Method
        [TestMethod()]
        public void FindUsersByName_ValidUser_ReturnsOneRecord()
        {
            UserParameters u = _testUsers[0];
            int total = 0;
            MembershipUserCollection users = _mp.FindUsersByName(u.username, 0, 5, out total);
            Assert.AreEqual(total, 1);
        }

        [TestMethod()]
        public void FindUsersByName_InvalidUser_ReturnsNoRecords()
        {
            string badname = "InvalidUserName";
            int total = -1;
            MembershipUserCollection users = _mp.FindUsersByName(badname, 0, 5, out total);
            Assert.AreEqual(total, 0);
        }
        #endregion    

        #region Test GetAllUsers Method
        [TestMethod()]
        public void GetAllUsers_FourPerPage_ReturnsFourRecords()
        {
            UserParameters u = _testUsers[0];
            int total = 0; 
            //We should at least get four of our five test users
            MembershipUserCollection users = _mp.GetAllUsers(0, 4, out total);
            Assert.AreEqual(total, 4);
        }
        
        #endregion

        #region Test GetNumberOfUsersOnline Method
        [TestMethod()]
        public void GetNumberOfUsersOnline_FourPerPage_ReturnsFourRecords()
        {
            UserParameters u = _testUsers[0];
            int total = -1;
            //We should at least get four of our five test users
            total = _mp.GetNumberOfUsersOnline();
            Assert.IsFalse(total < 0);
        }
        #endregion 

        #region Test GetPassword Method
        [TestMethod()]
        public void GetPassword_ValidAnswer_ReturnsGoodPassword()
        {
            UserParameters u = _testUsers[0];
            string password;
            password = _mp.GetPassword(u.username, u.passwordAnswer);
            Assert.AreEqual(password, u.password);
        }

        [TestMethod()]
        public void GetPassword_AnswerNotRequired_ReturnsGoodPassword()
        {
            UserParameters u = _testUsers[0];
            string answer = "KittyCatsLikeTuna";
            string password;
            password = _mp.GetPassword(u.username, answer);
            Assert.AreEqual(password, u.password);
        }

        [TestMethod()]
        [ExpectedException(typeof(MembershipPasswordException))]
        public void GetPassword_BadAnswer_ThrowsException()
        {
            UserParameters u = _testUsers[0];
            var mpTemp = new MemberProv();
            mpTemp.Initialize("DormouseMembershipProvider", TestUtils.GetComplexConfig());
            string answer = "KittyCatsLikeTuna";
            mpTemp.GetPassword(u.username, answer);
        }

        [TestMethod()]
        [ExpectedException(typeof(ProviderException))]
        public void GetPassword_NoRetrieval_ThrowsException()
        {
            UserParameters u = _testUsers[0];
            var mpTemp = new MemberProv();
            mpTemp.Initialize("DormouseMembershipProvider", TestUtils.GetNoPasswordConfig());
            string answer = "KittyCatsLikeTuna";
            mpTemp.GetPassword(u.username, answer);
        } 
        #endregion 

        #region Test ValidateUser Method
        [TestMethod()]
        public void ValidateUser_GoodPassword_ReturnsTrue()
        {
            UserParameters u = _testUsers[0];
            bool result;
            result = _mp.ValidateUser(u.username, u.password);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void ValidateUser_BadPassword_ReturnsFalse()
        {
            UserParameters u = _testUsers[0];
            bool result;
            result = _mp.ValidateUser(u.username, String.Empty);
            Assert.IsFalse(result);
        }


        [TestMethod()]
        public void ValidateUser_BadUser_ReturnsFalse()
        {
            bool result;
            result = _mp.ValidateUser("TheKingOfFrance", String.Empty);
            Assert.IsFalse(result);
        }

        #endregion

        #region Test GetUser Method
        [TestMethod()]
        public void GetUser_ValidUser_ReturnsObject()
        {
            UserParameters u = _testUsers[0];
            int total;
            MembershipUserCollection users = _mp.FindUsersByName(u.username, 0, 5, out total);
            MembershipUser mu;
            mu = _mp.GetUser(users[u.username].ProviderUserKey, true);
            Assert.AreEqual(mu.UserName,u.username);
        }

        #endregion

        #region Test ResetPassword Method
        [TestMethod()]
        public void ResetPassword_GoodUser_ReturnsNewPassword()
        {
            UserParameters u = _testUsers[1];
            string newPass ="";
            newPass = _mp.ResetPassword(u.username, u.passwordAnswer);
            _mp.ChangePassword(u.username, newPass, u.password);
            Assert.AreNotEqual(newPass, "");
        }

        [TestMethod()]
        [ExpectedException(typeof(MembershipPasswordException))]
        public void ResetPassword_BadUser_ThrowsException()
        {
            string baduser = "InvalidUser";
            string newPass = "";
            newPass = _mp.ResetPassword(baduser, baduser);
        }

        [TestMethod()]
        [ExpectedException(typeof(MembershipPasswordException))]
        public void ResetPassword_BadAnswer_ThrowsException()
        {
            UserParameters u = _testUsers[0]; 
            string badanswer = "BadPassword";
            string newPass = "";
            var mpTemp = new MemberProv();
            mpTemp.Initialize("DormouseMembershipProvider", TestUtils.GetComplexConfig());
            newPass = mpTemp.ResetPassword(u.username, badanswer);
        }

        [TestMethod()]
        [ExpectedException(typeof(ProviderException))]
        public void ResetPassword_NullAnswer_ThrowsException()
        {
            UserParameters u = _testUsers[0];
            string newPass = "";
            var mpTemp = new MemberProv();
            mpTemp.Initialize("DormouseMembershipProvider", TestUtils.GetComplexConfig());
            newPass = mpTemp.ResetPassword(u.username, null);
        }

        [TestMethod()]
        public void ResetPassword_AnswerNotRequired_ReturnsNewPassword()
        {
            UserParameters u = _testUsers[0];
            string badanswer = "BadPassword";
            string newPass = "";
            newPass = _mp.ResetPassword(u.username, badanswer);
            _mp.ChangePassword(u.username, newPass, u.password);
            Assert.AreNotEqual(newPass, "");
        }
        #endregion 

        #region Test UnlockUser Method
        [TestMethod()]
        public void UnlockUser_ValidUser_ReturnsTrue()
        {
            UserParameters u = _testUsers[4];
            bool result = false;
            result = _mp.UnlockUser(u.username);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void UnlockUser_InvalidUser_ReturnsTrue()
        {
            bool result = false;
            result = _mp.UnlockUser("TheKingOfFrance");
            Assert.IsTrue(result);
        }

        #endregion 

        #region Test Properties

        [TestMethod()]
        public void Properties_ExerciseAllGetsAndSets()
        {
            var mpTemp = new MemberProv();
            mpTemp.Initialize("DormouseMembershipProvider", TestUtils.GetComplexConfig());
            mpTemp.ApplicationName = "TempApp";
            Assert.AreEqual(mpTemp.ApplicationName, "TempApp");
            Assert.AreEqual(mpTemp.MaxInvalidPasswordAttempts, 3);
            Assert.AreEqual(mpTemp.MinRequiredNonAlphanumericCharacters, 1);
            Assert.AreEqual(mpTemp.MinRequiredPasswordLength, 7);
            Assert.AreEqual(mpTemp.PasswordAttemptWindow, 10);
            Assert.AreEqual(mpTemp.PasswordStrengthRegularExpression, "^.*(?=.{6,})(?=.*[a-z])(?=.*[A-Z]).*$");
        }

        #endregion
    }
}
