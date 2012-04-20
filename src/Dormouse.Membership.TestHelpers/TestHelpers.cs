using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Configuration;
using Dormouse.Membership.Model;
using Dormouse.Membership.Repository;
using Rhino.Mocks;

namespace Dormouse.Membership.TestHelpers
{
    public struct PasswordQandA
    {
        public string Question { get; set;}
        public string Answer { get; set; }
    }
    
    public class UserParameters
    {
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string passwordQuestion { get; set; }
        public string passwordAnswer { get; set; }
        public bool isApproved { get; set; }
        public object providerUserKey { get; set; }
    }

    public static class TestUtils
    {
        public static UserParameters GetValidUser()
        {
            var u = new UserParameters();

            u.username = "TestUserName";
            u.password = "!Password?123";
            u.email = "user@domain.com";
            u.passwordQuestion = "TestPasswordQuestion";
            u.passwordAnswer = "TestPasswordAnswer";
            u.isApproved = false;
            u.providerUserKey = null;

            return u;
        }

        public static List<UserParameters> GetTestUsers(int numUsers, string prefix)
        {
            List<UserParameters> t = new List<UserParameters>();
            for (int i = 0; i < numUsers; i++)
            {
                var u = new UserParameters();

                u.username = prefix + "TestUser" + i;
                u.password = prefix + "!Password?" + i;
                u.email = u.username + "@testdomain.com";
                u.passwordQuestion = prefix + "TestPasswordQuestion" + i;
                u.passwordAnswer = prefix + "TestPasswordAnswer" + i;
                u.isApproved = true;
                u.providerUserKey = null;

                t.Add(u);
            }
            return t;
        }

        public static NameValueCollection GetComplexConfig()
        {
            var config = new NameValueCollection();
            config.Add("applicationName", "DMTestApp");
            config.Add("maxInvalidPasswordAttempts", "3");
            config.Add("passwordAttemptWindow", "10");
            config.Add("minRequiredNonAlphanumericCharacters", "1");
            config.Add("minRequiredPasswordLength", "7");
            config.Add("passwordStrengthRegularExpression", "^.*(?=.{6,})(?=.*[a-z])(?=.*[A-Z]).*$");
            config.Add("enablePasswordReset", "true");
            config.Add("enablePasswordRetrieval", "true");
            config.Add("requiresQuestionAndAnswer", "true");
            config.Add("requiresUniqueEmail", "true");

            return config;
        }

        public static NameValueCollection GetNoPasswordConfig()
        {
            var config = new NameValueCollection();
            config.Add("applicationName", "DMTestApp");
            config.Add("maxInvalidPasswordAttempts", "5");
            config.Add("passwordAttemptWindow", "10");
            config.Add("minRequiredNonAlphanumericCharacters", "1");
            config.Add("minRequiredPasswordLength", "7");
            config.Add("passwordStrengthRegularExpression", String.Empty);
            config.Add("enablePasswordReset", "true");
            config.Add("enablePasswordRetrieval", "false");
            config.Add("requiresQuestionAndAnswer", "true");
            config.Add("requiresUniqueEmail", "true");

            return config;
        }

        public static List<string> GetTestRoles(int numRoles, string prefix)
        {
            var r = new List<string>();
            for (int i = 0; i < numRoles; i ++)
            {
                r.Add(prefix + "Role" + i);
            }
            return r;
        }
    }

    public static class Utilities
    {
        public static MachineKeySection GetUnencryptedMachineKey()
        {
            var mk = new MachineKeySection();
            mk.ValidationKey = "AutoGenerate";
            return mk;            
        }

        public static MachineKeySection GetEncryptedMachineKey()
        {
            var mk = new MachineKeySection();
            mk.ValidationKey = "32F2AABDF6A7B29A509296629C659885774B58A8FF63FC59B5A68E72F7C10591AD68DA58FA312F93F6D8EAE3CE13424D356811DC9254145A4899AA99B83AE4C8";
            mk.DecryptionKey = "2FFF6B9F5703111234CD41A13D7ABF9176554A37F219952D39E1398EE53B830F";
            mk.Validation = MachineKeyValidation.SHA1;
            mk.Decryption = "AES";
            return mk;
        }
        
        public static IMembershipRepository GetMockRepository()
        {
            User goodUser = new User() {Password = "GoodPass", IsApproved = true, CreationDate = DateTime.Now, 
                PasswordAnswer = "GoodAnswer", LastLockedOutDate = DateTime.Now, 
                LastPasswordChangedDate = DateTime.Now, LastLoginDate = DateTime.Now,
                UserName = "GoodUser"};
            var mockRepo = MockRepository.GenerateMock<IMembershipRepository>();
            mockRepo.Stub(v => v.GetUserByName("GoodUser", null)).Return(goodUser);
            mockRepo.Stub(v => v.GetUserByName("EmptyUser", null)).Return(goodUser);
            mockRepo.Stub(v => v.GetUserByName("LockedUser", null)).Return(new User() { IsLockedOut = true });
            mockRepo.Stub(v => v.GetUserByName("BadAnswerUser", null)).Return(new User() {PasswordAnswer = "GoodAnswer", Password = "GoodPass"});
            mockRepo.Stub(v => v.GetUserByName("EncryptUser", null)).Return(new User() { PasswordAnswer = "SerLEVf28XZ/mBLKLgqulBDfUK05rOsefCL0gd+WRDE=", Password = "Hei77AsDaWtwcvWYAJFawnB0X7BiukYVd+62O6lthNY=", IsApproved = true });
            mockRepo.Stub(v => v.GetUserByName("HashUser", null)).Return(new User() { PasswordAnswer = "IbltIxTKvuFlsy0hjV/7dQLS9wA=", Password = "rIFfDE7BPUwnFqBYCPI2PHHnVSM=", IsApproved = true });
            mockRepo.Stub(v => v.GetUserByName("NewUser", null)).Return(null);
            mockRepo.Stub(v => v.GetUserByName("ExceptionUser", null)).Throw(new Exception());
            mockRepo.Stub(v => v.GetUserByID(1)).Return(goodUser);
            mockRepo.Stub(v => v.GetUserByID(999)).Throw(new Exception());
            mockRepo.Stub(v => v.FindUsersByEMail("GoodEmail", 0, 99, null)).Return(GetStubUsers(1));
            mockRepo.Stub(v => v.FindUsersByEMail("BadEmail", 0, 99, null)).Return(new List<User>());
            mockRepo.Stub(v => v.FindUsersByEMail("DupEmail", 0, 99, null)).Return(GetStubUsers(2));
            mockRepo.Stub(v => v.FindUsersByEMail("ExceptionMail",0,99, null)).Throw(new Exception());
            mockRepo.Stub(v => v.FindUsersByName("GoodName", 0, 99, null)).Return(GetStubUsers(1));
            mockRepo.Stub(v => v.FindUsersByName("BadName", 0, 99, null)).Return(new List<User>());
            mockRepo.Stub(v => v.FindUsersByName("DupName", 0, 99, null)).Return(GetStubUsers(2));
            mockRepo.Stub(v => v.FindUsersByName("ExceptionMail", 0, 99, null)).Throw(new Exception());
            mockRepo.Stub(v => v.GetUserNameByEMail("NewEmail", null)).Return("");
            mockRepo.Stub(v => v.GetUserNameByEMail("DupEmail", null)).Return("DupUser");
            mockRepo.Stub(v => v.GetUserNameByEMail("ExceptionEmail", null)).Throw(new Exception());
            mockRepo.Stub(v => v.GetAllUsers(0, 99, null)).Return(GetStubUsers(2));
            mockRepo.Stub(v => v.GetAllUsers(1, 99, null)).Return(new List<User>());
            mockRepo.Stub(v => v.GetAllUsers(2, 99, null)).Throw(new Exception());
            
            mockRepo.Stub(v => v.GetRoleByName("InvalidRole", null)).Return(null);
            mockRepo.Stub(v => v.GetRoleByName("ValidRole", null)).Return(new Role());
            mockRepo.Stub(v => v.GetRoleByName("PopulatedRole", null)).Return(new Role{RoleName = "PopulatedRole"});
            mockRepo.Stub(v => v.GetRoleByName("UnpopulatedRole", null)).Return(new Role{RoleName = "UnpopulatedRole"});
            mockRepo.Stub(v => v.FindUsersInRole("PopulatedRole", "SampleUser", null)).
                Return(new List<User>{new User {UserName = "SampleUser"}});
            mockRepo.Stub(v => v.FindUsersInRole("UnpopulatedRole", "", null)).Return(new List<User>());
            mockRepo.Stub(v => v.FindUsersInRole("UnpopulatedRole", "GoodUser", null)).Return(new List<User>());
            mockRepo.Stub(v => v.GetAllUsersInRole("PopulatedRole", null)).
                Return(new List<User> { new User { UserName = "SampleUser" } });
            mockRepo.Stub(v => v.FindUsersInRole("ValidRole","GoodUser", null)).
                Return(new List<User> { goodUser } ); 
            mockRepo.Stub(v => v.GetAllUsersInRole("UnpopulatedRole", null)).Return(new List<User>());
            mockRepo.Stub(v => v.GetAllRoles(null)).Return(new List<Role> {new Role(), new Role()});

            mockRepo.Stub(v => v.GetAllRolesForUser("GoodUser",null)).Return(new List<Role> { new Role(), new Role() });
            mockRepo.Stub(v => v.GetAllRolesForUser("EmptyUser",null)).Return(new List<Role>());
            return mockRepo;
        }

        public static List<User> GetStubUsers(int numUsers)
        {
            var uList = new List<User>();
            for (int i = 0; i < numUsers; i++)
            {
                var u = new User();
                u.CreationDate = DateTime.Now;
                u.UserName = "SampleUser" + i;
                u.UserId = i;
                uList.Add(u);
            }
            return uList;
        }

        public static MemberProv GetProviderWithNoPasswordRetrievalOrReset()
        {
            var prov = new MemberProv(GetMockRepository(), GetUnencryptedMachineKey());
            var config = new NameValueCollection();
            config.Add("enablePasswordRetrieval", "false");
            config.Add("enablePasswordReset", "false");
            prov.Initialize("", config);
            return prov;
        }
    }
}
