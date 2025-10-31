namespace OMS.Models;

public record User(
    int Id,
    string Name,
    UserRole Role
);

public enum UserRole
{
    Admin,
    Maker
}