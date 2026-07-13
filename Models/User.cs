namespace dotnet_bookish_starter.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Password_Hash { get; set; } = null!;
}
