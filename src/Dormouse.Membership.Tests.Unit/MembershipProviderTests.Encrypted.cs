using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Security;
using Dormouse.Membership.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dormouse.Membership.Tests.Unit
{
    [TestClass]
    public class MembershipProviderTestsEncrypted
    {
        private MemberProv _mProv;

        #region Setup and Teardown
        [TestInitialize()]
        public void InitializeTest()
        {
            _mProv = new MemberProv(Utilities.GetMockRepository(),Utilities.GetEncryptedMachineKey());
            var config = new NameValueCollection();
            config.Add("requiresQuestionAndAnswer", "true");
            config.Add("passwordFormat", "Encrypted");
            _mProv.Initialize("", config);
        }

        [TestCleanup()]
        public void CleanupTest()
        {
            _mProv = null;
        }
        #endregion

        [TestMethod]
        [ExpectedException(typeof(MembershipPasswordException))]
        public void GetPassword_GivenGoodUserAndBadAnswer_WithRequireAnswer_ThrowsException()
        {
            //Arrange
            var name = "EncryptUser";
            var answer = "BadAnswer";
            //Act
            _mProv.GetPassword(name, answer);
            //Assert
        }

        [TestMethod]
        public void ChangePassword_GoodUserGoodPass_ReturnsTrue()
        {
            //Arrange
            var user = "EncryptUser";
            var oldpass = "GoodPass";
            var newpass = "ABC123!?";
            //Act
            var actual = _mProv.ChangePassword(user, oldpass, newpass);
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void GetPassword_GivenGoodUserAndGoodAnswer_WithRequireAnswer_ReturnsPassword()
        {
            //Arrange
            var name = "EncryptUser";
            var answer = "GoodAnswer";
            var expected = "GoodPass";
            //Act
            var actual = _mProv.GetPassword(name, answer);
            //Assert
            Assert.AreEqual(expected,actual);
        }

        [TestMethod]
        public void GetPassword_GivenGoodUserAndGoodAnswer_ReturnsPassword()
        {
            //Arrange
            var name = "EncryptUser";
            var answer = "GoodAnswer";
            var expected = "GoodPass";
            //Act
            var actual = _mProv.GetPassword(name, answer);
            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void ResetPassword_NullAnswer_QandARequired_ThrowsException()
        {
            //Arrange
            var name = "GoodUser";
            //Act
            _mProv.ResetPassword(name, null);
            //Assert
        }

        [TestMethod]
        [ExpectedException(typeof(MembershipPasswordException))]
        public void ResetPassword_BadAnswer_QandARequired_ThrowsException()
        {
            //Arrange
            var name = "EncryptUser";
            var answer = "BadAnswer";
            //Act
            _mProv.ResetPassword(name, answer);
            //Assert
        }

        [TestMethod]
        public void ResetPassword_GoodUser_QandARequired_ReturnsNewPassword()
        {
            //Arrange
            var name = "EncryptUser";
            var answer = "GoodAnswer";
            //Act
            var actual = _mProv.ResetPassword(name, answer);
            //Assert
            Assert.AreNotEqual("", actual);
        }
    }
}
