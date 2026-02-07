namespace Курсов_проект___ИтКариера.Data
{
    public class UserRole
    {
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }

}
