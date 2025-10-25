namespace OMS.Models;

public record User(
    string Id,
    string Name,
    UserRole Role
);

public enum UserRole
{
    Admin,
    Maker
}