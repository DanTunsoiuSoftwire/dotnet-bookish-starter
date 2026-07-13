using dotnet_bookish_starter.Models;
using Microsoft.AspNetCore.Mvc;
using dotnet_bookish_starter.Services;


namespace dotnet_bookish_starter.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly string _connectionString;
    private UserServices _userServices;
 
    public UserController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DbConnectionString") ?? "";
        _userServices = new UserServices(_connectionString);
    }

    [HttpPost("login")]
    public async Task<string> LogIn([FromBody] UserCredentials credentials)
    {
        string token = await _userServices.LogIn(credentials);
        if (token == "")
        {
            this.HttpContext.Response.StatusCode = 401;
        }

        return token;
    }
    
    [HttpPost]
    public async Task<User> AddUser([FromBody] User user)
    {
        User addedUser = await _userServices.AddUser(user);
        if (addedUser.Email == null!)
        {
            this.HttpContext.Response.StatusCode = 409;
        }
        
        return addedUser;
    }

    [HttpGet("{id}")]
    public async Task<User> GetUser([FromRoute] int id)
    {
        User existingUser =  await _userServices.GetUser(id);
        if (existingUser.Email == null!)
        {
            this.HttpContext.Response.StatusCode = 404;
        }
        return existingUser;
    }

    [HttpPatch("password")]
    public async Task<User> UpdatePassword([FromBody] UserPasswordRequest request)
    {
        User existingUser = await _userServices.UpdatePassword(request);
        if (existingUser.Email == "password incorrect")
        {
            this.HttpContext.Response.StatusCode = 401;
        }

        if (existingUser.Email == null!)
        {
            this.HttpContext.Response.StatusCode = 401;
            existingUser = new User();
        }
        
        return existingUser;
    }

    [HttpPatch("email")]
    public async Task<User> UpdateEmail([FromBody] UserEmailRequest request)
    {
        User existingUser = await _userServices.UpdateEmail(request);
        if (existingUser.Email == "password incorrect")
        {
            this.HttpContext.Response.StatusCode = 401;
        }

        if (existingUser.Email == null!)
        {
            this.HttpContext.Response.StatusCode = 401;
            existingUser = new User();
        }
        
        return existingUser;
    }
    
    [HttpDelete("{id}")]
    public async Task<string> DeleteUser([FromRoute] int id)
    {
        return await _userServices.DeleteUser(id);
    }
}
