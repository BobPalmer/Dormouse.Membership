using System;
using System.Linq;
using System.Collections.Specialized;
using System.Configuration.Provider;
using Dormouse.Membership.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dormouse.Membership.Tests.Unit
{
    [TestClass]
    public class RoleProviderTests
    {
        private RoleProv _mProv;

        [TestInitialize]
        public void TestSetup()
        {
            _mProv = new RoleProv(Utilities.GetMockRepository());
            var config = new NameValueCollection();
            _mProv.Initialize("", config);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialize_NullConfig_ThrowsArgumentNullException()
        {
            //Arrange
            _mProv = new RoleProv(Utilities.GetMockRepository());
            //Act
            _mProv.Initialize("", null);
            //Assert
        }

        #region Test Initialize Method
        [TestMethod]
        public void Initialize_NullName_SetsDefaultName()
        {
            //Arrange
            _mProv = new RoleProv(Utilities.GetMockRepository());
            //Act
            _mProv.Initialize("", new NameValueCollection());
            var actual = _mProv.ApplicationName;
            //Assert
            Assert.IsNull( actual);
        }

        [TestMethod]
        public void Initialize_AppName_SetsAppName()
        {
            //Arrange
            _mProv = new RoleProv(Utilities.GetMockRepository());
            var expected = "SampleApplication";
            //Act
            _mProv.Initialize("", new NameValueCollection { { "applicationName", "SampleApplication" } });
            var actual = _mProv.ApplicationName;
            //Assert
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region Test CreateRole Method
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateRole_GivenNull_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.CreateRole(null);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateRole_GivenBlank_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.CreateRole("");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateRole_GivenRoleWithComma_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.CreateRole("my,role");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void CreateRole_GivenExistingRole_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.CreateRole("ValidRole");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        public void CreateRole_GivenAnyRole_DoesNotThrowException()
        {
            //Arrange
            //Act
            _mProv.CreateRole("SampleRole");
            //Assert
            Assert.IsTrue(true);
        }
        #endregion

        #region Test RoleExists Method
        [TestMethod]
        public void RoleExists_GivenGoodRole_ReturnsTrue()
        {
            //Arrange
            //Act
            var actual = _mProv.RoleExists("ValidRole");
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void RoleExists_GivenBadRole_ReturnsFalse()
        {
            //Arrange
            //Act
            var actual = _mProv.RoleExists("InvalidRole");
            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RoleExists_GivenNull_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.RoleExists(null);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RoleExists_GivenBlank_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.RoleExists("");
            //Assert
            Assert.Fail();
        }
        #endregion

        #region Test DeleteRole Method
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteRole_GivenNull_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.DeleteRole(null,false);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteRole_GivenBlank_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.DeleteRole("", false);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        public void DeleteRole_GivenGoodRoleThrowPopulateWithoutRecords_ReturnsTrue()
        {
            //Arrange
            //Act
            var actual = _mProv.DeleteRole("UnpopulatedRole", true);
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void DeleteRole_GivenGoodRoleThrowPopulateWithRecords_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.DeleteRole("PopulatedRole", true);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        public void DeleteRole_GivenGoodRoleWithoutRecordsNoThrow_ReturnsTrue()
        {
            //Arrange
            //Act
            var actual = _mProv.DeleteRole("UnpopulatedRole", false);
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void DeleteRole_GivenGoodRoleWithRecordsNoThrow_ReturnsTrue()
        {
            //Arrange
            //Act
            var actual = _mProv.DeleteRole("PopulatedRole", false);
            //Assert
            Assert.IsTrue(actual);
        }

        
        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void DeleteRole_GivenBadRole_ThrowsException()
        {
            //Arrange
            //Act
            var actual = _mProv.DeleteRole("InvalidRole",true);
            //Assert
            Assert.Fail();
        }
        #endregion

        #region Test FindUsersInRole Method
        [TestMethod]
        public void FindUsersInRole_GoodRoleGoodUsers_ReturnsArray()
        {
            //Arrange
            var expected = 1;
            //Act
            var users = _mProv.FindUsersInRole("PopulatedRole", "SampleUser");
            //Assert
            Assert.AreEqual(expected,users.Count());
        }

        [TestMethod]
        public void FindUsersInRole_GoodRoleBlankUser_ReturnsArray()
        {
            //Arrange
            var expected = 1;
            //Act
            var users = _mProv.FindUsersInRole("PopulatedRole", "");
            //Assert
            Assert.AreEqual(expected, users.Count());

        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void FindUsersInRole_BadRole_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.FindUsersInRole("InvalidRole", "");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindUsersInRole_NullRole_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.FindUsersInRole(null, "");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FindUsersInRole_BlankRoleThrowsException()
        {
            //Arrange
            //Act
            _mProv.FindUsersInRole("", "");
            //Assert
            Assert.Fail();
        }
        #endregion

        #region Test GetAllRoles Method
        [TestMethod]
        public void GetAllRoles_AgainstMockRepository_ReturnsAllRoles()
        {
            //Arrange
            var expected = 2;
            //Act
            var actual = _mProv.GetAllRoles();
            //Assert
            Assert.AreEqual(expected,actual.Count());
        }
        #endregion

        #region Test IsUserInRole Method
        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void IsUserInRole_WithBadRole_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.IsUserInRole("GoodUser","BadRole");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsUserInRole_WithNullRole_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.IsUserInRole("GoodUser",null);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsUserInRole_WithBlankRole_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.IsUserInRole("GoodUser","");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void IsUserInRole_WithBadUser_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.IsUserInRole("NewUser","ValidRole");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsUserInRole_WithNullUser_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.IsUserInRole(null, "ValidRole");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsUserInRole_WithBlankUser_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.IsUserInRole("","ValidRole");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        public void IsUserInRole_InRole_ReturnsTrue()
        {
            //Arrange
            //Act
            var actual = _mProv.IsUserInRole("GoodUser", "ValidRole");
            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsUserInRole_NotInRole_ReturnsFalse()
        {
            //Arrange
            //Act
            var actual = _mProv.IsUserInRole("GoodUser", "UnpopulatedRole");
            //Assert
            Assert.IsFalse(actual);
        }
        #endregion

        #region Test GetRolesForUser Method 

        [TestMethod]
        public void GetRolesForUser_WithGoodUser_ReturnsRoles()
        {
            //Arrange
            var expected = 2;
            //Act
            var actual = _mProv.GetRolesForUser("GoodUser");
            //Assert
            Assert.AreEqual(expected,actual.Count());
        }

        [TestMethod]
        public void GetRolesForUser_WithUserThatHasNoRoles_ReturnsEmptyList()
        {
            //Arrange
            var expected = 0;
            //Act
            var actual = _mProv.GetRolesForUser("EmptyUser");
            //Assert
            Assert.AreEqual(expected, actual.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetRolesForUser_WithNullUser_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.GetRolesForUser(null);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetRolesForUser_WithBlankUser_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.GetRolesForUser("");
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void GetRolesForUser_WithBadUser_ThrowsException()
        {
            //Arrange
            //Act
            _mProv.GetRolesForUser("BadUser");
            //Assert
            Assert.Fail();
        }

        #endregion

        #region Test AddUsersToRoles Method
        [TestMethod] 
        public void AddUsersToRoles_WithGoodData_DoesNotThrowException()
        {
            //Arrange
            var goodRoles = new [] {"UnpopulatedRole"};
            var goodUsers = new [] {"GoodUser"};
            //Act
            _mProv.AddUsersToRoles(goodUsers,goodRoles);
            //Assert
            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddUsersToRoles_WithCommaData_ThrowsException()
        {
            //Arrange
            var goodRoles = new[] { "UnpopulatedRole" };
            var goodUsers = new[] { "GoodUser,GoodUser" };
            //Act
            _mProv.AddUsersToRoles(goodUsers, goodRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void AddUsersToRoles_WithExistingData_ThrowsException()
        {
            //Arrange
            var goodRoles = new[] { "ValidRole" };
            var goodUsers = new[] { "GoodUser" };
            //Act
            _mProv.AddUsersToRoles(goodUsers, goodRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddUsersToRoles_WithNullUser_ThrowsException()
        {
            //Arrange
            var goodRoles = new[] { "ValidRole" };
            var badUsers = new string[] {null};
            //Act
            _mProv.AddUsersToRoles(badUsers, goodRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddUsersToRoles_WithBlankUser_ThrowsException()
        {
            //Arrange
            var goodRoles = new[] { "ValidRole" };
            var badUsers = new[] { "" };
            //Act
            _mProv.AddUsersToRoles(badUsers, goodRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void AddUsersToRoles_WithInvalidUser_ThrowsException()
        {
            //Arrange
            var goodRoles = new[] { "ValidRole" };
            var badUsers = new[] { "BadUser" };
            //Act
            _mProv.AddUsersToRoles(badUsers, goodRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddUsersToRoles_WithNullRole_ThrowsException()
        {
            //Arrange
            var badRoles = new string[] { null };
            var goodUsers = new[] { "GoodUser" };
            //Act
            _mProv.AddUsersToRoles(goodUsers, badRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddUsersToRoles_WithBlankRole_ThrowsException()
        {
            //Arrange
            var badRoles = new[] { "" };
            var goodUsers = new[] { "GoodUser" };
            //Act
            _mProv.AddUsersToRoles(goodUsers, badRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void AddUsersToRoles_WithInvalidRole_ThrowsException()
        {
            //Arrange
            var badRoles = new[] { "BadRole" };
            var goodUsers = new[] { "GoodUser" };
            //Act
            _mProv.AddUsersToRoles(goodUsers, badRoles);
            //Assert
            Assert.Fail();
        }

        #endregion

        #region Test RemoveUsersFromRoles Method

        [TestMethod]
        public void RemoveUsersFromRoles_WithGoodData_DoesNotThrowException()
        {
            //Arrange
            var goodRoles = new[] { "ValidRole" };
            var goodUsers = new[] { "GoodUser" };
            //Act
            _mProv.RemoveUsersFromRoles(goodUsers, goodRoles);
            //Assert
            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RemoveUsersFromRoles_WithCommaData_ThrowsException()
        {
            //Arrange
            var goodRoles = new[] { "UnpopulatedRole" };
            var goodUsers = new[] { "GoodUser,GoodUser" };
            //Act
            _mProv.RemoveUsersFromRoles(goodUsers, goodRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void RemoveUsersFromRoles_WithNonExistingData_ThrowsException()
        {
            //Arrange
            var goodRoles = new[] { "UnpopulatedRole" };
            var goodUsers = new[] { "GoodUser" };
            //Act
            _mProv.RemoveUsersFromRoles(goodUsers, goodRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveUsersFromRoles_WithNullUser_ThrowsException()
        {
            //Arrange
            var goodRoles = new[] { "ValidRole" };
            var badUsers = new string[] { null };
            //Act
            _mProv.RemoveUsersFromRoles(badUsers, goodRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RemoveUsersFromRoles_WithBlankUser_ThrowsException()
        {
            //Arrange
            var goodRoles = new[] { "ValidRole" };
            var badUsers = new[] { "" };
            //Act
            _mProv.RemoveUsersFromRoles(badUsers, goodRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void RemoveUsersFromRoles_WithInvalidUser_ThrowsException()
        {
            //Arrange
            var goodRoles = new[] { "ValidRole" };
            var badUsers = new[] { "BadUser" };
            //Act
            _mProv.RemoveUsersFromRoles(badUsers, goodRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveUsersFromRoles_WithNullRole_ThrowsException()
        {
            //Arrange
            var badRoles = new string[] { null };
            var goodUsers = new[] { "GoodUser" };
            //Act
            _mProv.RemoveUsersFromRoles(goodUsers, badRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RemoveUsersFromRoles_WithBlankRole_ThrowsException()
        {
            //Arrange
            var badRoles = new[] { "" };
            var goodUsers = new[] { "GoodUser" };
            //Act
            _mProv.RemoveUsersFromRoles(goodUsers, badRoles);
            //Assert
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ProviderException))]
        public void RemoveUsersFromRoles_WithInvalidRole_ThrowsException()
        {
            //Arrange
            var badRoles = new[] { "BadRole" };
            var goodUsers = new[] { "GoodUser" };
            //Act
            _mProv.RemoveUsersFromRoles(goodUsers, badRoles);
            //Assert
            Assert.Fail();
        }
        #endregion
    }
}
