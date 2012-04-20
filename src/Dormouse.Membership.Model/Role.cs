namespace Dormouse.Membership.Model
{
    public class Role
    {
        public virtual int RoleId { get; set; }
        public virtual string RoleName { get; set; }
        public virtual string ApplicationName { get; set; }
    }
}