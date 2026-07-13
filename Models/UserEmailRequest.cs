namespace dotnet_bookish_starter.Models;

public class UserEmailRequest
{
    public int Id { get; set; }
    public string newEmail { get; set; } = null!;
    public string oldEmail { get; set; } = null!;
    public string password { get; set; } = null!;
}
