using System.Collections.Specialized;
using System.Configuration.Provider;
using Dormouse.Membership.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dormouse.Membership.Tests.Unit
{
    [TestClass]
    public class MembershipProviderTestsHashed
    {
        private MemberProv mProv;

        #region Setup and Teardown
        [TestInitialize()]
        public void InitializeTest()
        {
            mProv = new MemberProv(Utilities.GetMockRepository(), Utilities.GetEncryptedMachineKey());
            var config = new NameValueCollection();
            config.Add("passwordFormat", "Hashed");
            config.Add("requiresQuestionAndAnswer", "true");
            mProv.Initialize("", config);
        }

        [TestCleanup()]
        public void CleanupTest()
        {
            mProv = null;
        }
        #endregion

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void GetPassword_GivenGoodUserAndGoodAnswer_ThrowsException()
        {
            //Arrange
            var name = "GoodUser";
            var answer = "GoodAnswer";
            //Act
            mProv.GetPassword(name, answer);
        }

        [TestMethod]
        public void ResetPassword_GoodUser_QandARequired_ReturnsNewPassword()
        {
            //Arrange
            var name = "HashUser";
            var answer = "GoodAnswer";
            //Act
            var actual = mProv.ResetPassword(name, answer);
            //Assert
            Assert.AreNotEqual("", actual);
        }

        [TestMethod]
        public void ValidateUser_GivenGoodUserGoodPassword_ReturnsTrue()
        {
            //Arrange
            var userName = "HashUser";
            var userPass = "GoodPass";
            //Act
            var actual = mProv.ValidateUser(userName, userPass);
            //Assert
            Assert.IsTrue(actual);
        }
    }
}
