namespace dotnet_bookish_starter.Models;

public class UserPasswordRequest
{
    public int Id { get; set; }
    public string oldPassword { get; set; } = null!;
    public string newPassword { get; set; } = null!;
}
