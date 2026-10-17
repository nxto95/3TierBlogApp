namespace BlogApp.DAL.Entities;

public class UserEntity : BaseEntity
{
    public string? Username { get; private set; }
    public string? Email { get; private set; }
    public string? PasswordHash { get; private set; }
}
