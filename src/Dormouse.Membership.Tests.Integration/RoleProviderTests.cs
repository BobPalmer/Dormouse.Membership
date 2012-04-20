using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Dormouse.Membership.Data;
using Dormouse.Membership.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dormouse.Membership.Tests.Integration
{
    [TestClass]
    public class RoleProviderTests
    {
        private RoleProv _mpRole;
        private MembershipRepository _memberRepo = new MembershipRepository();
        private static NameValueCollection _testConfig;
        private static List<UserParameters> _testUsers;
        private static List<String> _testRoles;


        #region Setup and Teardown
        [ClassInitialize()]
        public static void ClassSetup(TestContext testContext)
        {
            //Set up a test configuration to use.
            _testConfig = new NameValueCollection();
            _testConfig.Add("applicationName", "DMTestApp");
            _testUsers = TestUtils.GetTestUsers(2, "Role");
            _testRoles = TestUtils.GetTestRoles(3, "Sample");

            //Clear out old data in case our previous run crashed
            ClassCleanup();

            var mRoleSetup = new RoleProv();
            mRoleSetup.Initialize("DormouseRoleProvider", _testConfig);
            mRoleSetup.CreateRole("DefaultRole");
            foreach (var r in _testRoles)
            {
                mRoleSetup.CreateRole(r);
            }

            var mpSetup = new MemberProv();
            mpSetup.Initialize("DormouseMembershipProvider", _testConfig);
            foreach (var u in _testUsers)
            {
                MembershipCreateStatus status;
                mpSetup.CreateUser(u.username, u.password, u.email, u.passwordQuestion,
                    u.passwordAnswer, u.isApproved, u.providerUserKey, out status);
            }

            mRoleSetup.AddUsersToRoles(_testUsers.Select(u=>u.username).ToArray(), new[] { "DefaultRole" });
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

            //And our sample role
            var mpRoleCleanup = new RoleProv();
            mpRoleCleanup.Initialize("DormouseMembershipProvider", _testConfig);
            if (mpRoleCleanup.RoleExists("DefaultRole"))
            {
                mpRoleCleanup.DeleteRole("DefaultRole", false);
            }
            foreach (var role in _testRoles)
            {
                if (mpRoleCleanup.RoleExists(role))
                {
                    mpRoleCleanup.DeleteRole(role, false);
                }
            }

        }

        [TestInitialize()]
        public void TestSetup()
        {
            _mpRole = new RoleProv();
            _mpRole.Initialize("DormouseMembershipProvider", _testConfig);
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            _mpRole = null;
        }
        #endregion

        [TestMethod]
        public void FindUsersInRole_GivenGoodRole_ReturnsUsers()
        {
            //Arrange
            //Act
            var actual = _mpRole.FindUsersInRole("DefaultRole", "");
            //Assert
            Assert.AreEqual(2,actual.Count());
        }

        [TestMethod]
        public void GetAllRoles_ReturnsRoleList()
        {
            //Arrange
            //Act
            var actual = _mpRole.GetAllRoles();
            //Assert
            Assert.AreEqual(4,actual.Count());
        }

        [TestMethod]
        public void GetRolesForUser_GivenGoodUser_ReturnsRoles()
        {
            //Arrange
            //Act
            var actual = _mpRole.GetRolesForUser(_testUsers[0].username);
            //Assert
            Assert.AreEqual(1,actual.Count());
        }

        [TestMethod]
        public void AddUsersRoles_GivenGoodData_AddsData()
        {
            //Arrange
            //Act
            _mpRole.AddUsersToRoles(
                new[]{_testUsers[1].username},
                new[]{_testRoles[0]});
            var actual = _mpRole.GetRolesForUser(_testUsers[1].username).Count();
            _mpRole.RemoveUsersFromRoles(
                new[] { _testUsers[1].username },
                new[] { _testRoles[0] });
            //Assert
            Assert.AreEqual(2,actual);
        }

        [TestMethod]
        public void CreateRole_GivenGoodRole_AddsRole()
        {
            //Arrange
            //Act
            _mpRole.CreateRole("SampleToAdd");
            var actual = _mpRole.RoleExists("SampleToAdd");
            _mpRole.DeleteRole("SampleToAdd",false);
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void DeleteRole_GivenGoodRole_DeletesRole()
        {
            //Arrange
            //Act
            _mpRole.CreateRole("SampleToDelete");
            var wasCreated = _mpRole.RoleExists("SampleToDelete");
            _mpRole.DeleteRole("SampleToDelete", false);
            var wasDeleted = _mpRole.RoleExists("SampleToDelete");
            //Assert
            Assert.IsTrue(wasCreated);
            Assert.IsFalse(wasDeleted);
        }

        [TestMethod]
        public void IsUserInRole_GivenGoodUser_ReturnsTrue()
        {
            //Arrange
            //Act
            var actual = _mpRole.IsUserInRole(_testUsers[0].username, "DefaultRole");
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsUserInRole_GivenBadUser_ReturnsFalse()
        {
            //Arrange
            //Act
            var actual = _mpRole.IsUserInRole(_testUsers[0].username, _testRoles[1]);
            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void RemoveUsersFromRoles_GivenGoodData_RemovesData()
        {
            //Arrange
            //Act
            //Arrange
            //Act
            _mpRole.AddUsersToRoles(
                new[] { _testUsers[1].username },
                new[] { _testRoles[2] });
            var numAdded = _mpRole.GetRolesForUser(_testUsers[1].username).Count();
            _mpRole.RemoveUsersFromRoles(
                new[] { _testUsers[1].username },
                new[] { _testRoles[2] });
            var numDeleted = _mpRole.GetRolesForUser(_testUsers[1].username).Count();
            //Assert
            Assert.AreEqual(2, numAdded);
            Assert.AreEqual(1, numDeleted);
        }

        [TestMethod]
        public void RoleExists_GivenGoodRole_ReturnsTrue()
        {
            //Arrange
            //Act
            var actual = _mpRole.RoleExists("DefaultRole");
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void RoleExists_GivenBadRole_ReturnsFalse()
        {
            //Arrange
            var actual = _mpRole.RoleExists("BadRole");
            //Assert
            Assert.IsFalse(actual);
        }
    }
}
