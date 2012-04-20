using System.Collections.Generic;
using System.Configuration.Provider;
using Dormouse.Membership.Data;
using Dormouse.Membership.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Security;
using System.Collections.Specialized;

namespace Dormouse.Membership.Tests.Integration
{
    /// <summary>
    /// These tests are based on a membership provider set up for 
    /// Hashed passwords.
    /// </summary>
    [TestClass]
    public class MembershipProviderTestsHashed
    {
        private MemberProv _rp;
        private TestContext testContextInstance;

        private static NameValueCollection _testConfig;
        private static List<UserParameters> _testUsers;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void ClassSetup(TestContext testContext)
        {
            //Set up a test configuration to use.
            _testConfig = new NameValueCollection();
            _testConfig.Add("applicationName", "DMTestApp");
            _testConfig.Add("passwordFormat", "Hashed");

            //We will create three test users to work with while
            //exercising our tests.  These will be used for various
            //read and find operations.
            var mpSetup = new MemberProv(new MembershipRepository(), Utilities.GetEncryptedMachineKey());
            mpSetup.Initialize("DormouseMembershipProvider", _testConfig);
            _testUsers = TestUtils.GetTestUsers(5, "Hashed");
            foreach (var u in _testUsers)
            {
                MembershipCreateStatus status;
                mpSetup.CreateUser(u.username, u.password, u.email, u.passwordQuestion,
                    u.passwordAnswer, u.isApproved, u.providerUserKey, out status);
            }
        }


        //
        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void ClassCleanup()
        {
            //We will remove our sample users
            var mpCleanup = new MemberProv(new MembershipRepository(), Utilities.GetEncryptedMachineKey());
            mpCleanup.Initialize("DormouseMembershipProvider", _testConfig);
            foreach (var user in _testUsers)
            {
                mpCleanup.DeleteUser(user.username, true);
            }
        }
        //
        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void TestSetup()
        {
            _rp = new MemberProv(new MembershipRepository(), Utilities.GetEncryptedMachineKey());
            _rp.Initialize("DormouseMembershipProvider", _testConfig);
        }
        //
        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void TestCleanup()
        {
            _rp = null;
        }
        //
        #endregion

        #region GetPassword test methods

        [TestMethod()]
        [ExpectedException(typeof(ProviderException))]
        public void Hashed_GetPassword_AnyAnswer_ThrowsException()
        {
            UserParameters u = _testUsers[0];
            string answer = "KittyCatsLikeTuna";
            _rp.GetPassword(u.username, answer);
        }

        #endregion

        #region ValidateUser test methods

        [TestMethod()]
        public void Hashed_ValidateUser_GoodPassword_ReturnsTrue()
        {
            UserParameters u = _testUsers[0];
            bool result;
            result = _rp.ValidateUser(u.username, u.password);
            Assert.IsTrue(result);
        }

        #endregion

    }
}
