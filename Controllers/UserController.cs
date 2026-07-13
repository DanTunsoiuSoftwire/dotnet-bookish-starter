using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dotnet_bookish_starter.Models;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using dotnet_bookish_starter.AuxiliaryClasses;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_bookish_starter.Controllers;

[ApiController]
[Route("user/[action]")]
public class UserController :ControllerBase
{
    private readonly string _connectionString;
 
    public UserController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DbConnectionString") ?? "";
    }

    [HttpGet]
    public async Task<string> LogIn([FromBody] UserCredentials credentials)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Users WHERE email like \'{0}\'", credentials.Email);
        User existingUser = await connection.QuerySingleOrDefaultAsync<User>(command);

        if (existingUser == null)
        {
            this.HttpContext.Response.StatusCode = 404;
            return "User does not exist!!";
        }

        if (Int32.Parse(existingUser.Password_Hash) != HashClass.HashPassword(credentials.Password))
        {
            this.HttpContext.Response.StatusCode = 403;
            return "Invalid password!";
        }
        
        var claims = new[]  // Populate standard claims
        {
            new Claim(JwtRegisteredClaimNames.Email, existingUser.Email)
        };
        
        const int TokenLifetimeMinutes = 60;
        
        var jwt = new JwtSecurityToken(
            issuer: "https://bookish.com",
            audience: "https://bookish.com",
            claims: claims,
            //Typical short lifetime used with JWTs
            expires: DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes),
            signingCredentials: new(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("SecretKey00000000000000000000000")),
                SecurityAlgorithms.HmacSha256Signature));

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
    
    [HttpPost]
    public async Task<User> AddUser([FromBody] User user)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Users WHERE id = {0}", user.Id);
        User existingUser = await connection.QuerySingleOrDefaultAsync<User>(command);
        if (existingUser != null){
            this.HttpContext.Response.StatusCode = 409;
            return existingUser;
        }
        
        command = String.Format("INSERT INTO Users VALUES ({0}, \'{1}\', \'{2}\')",
            user.Id, user.Email, HashClass.HashPassword(user.Password_Hash));
        await connection.ExecuteAsync(command);
        command = String.Format("SELECT * FROM Users WHERE id = {0}", user.Id);
        
        return await connection.QuerySingleOrDefaultAsync<User>(command);;
    }

    [HttpGet]
    public async Task<User> GetUser(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Users WHERE id = {0}", id);
        try
        {
            return (await connection.QueryAsync<User>(command)).Single();
        }
        catch (Exception)
        {
            this.HttpContext.Response.StatusCode = 404;
            return new User();
        }
    }

    [HttpPatch]
    public async Task<User> UpdatePassword([FromBody] UserPasswordRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Users WHERE id = {0}", request.Id);
        try
        {
            User user =  (await connection.QueryAsync<User>(command)).Single();
            if (Int32.Parse(user.Password_Hash) != HashClass.HashPassword(request.oldPassword))
            {
                this.HttpContext.Response.StatusCode = 403;
                user.Password_Hash = Convert.ToString(request.oldPassword.GetHashCode());
                return user;
            }
            
            command = String.Format("UPDATE Users SET password_hash = {0}" +
                                    " WHERE id = {1}", HashClass.HashPassword(request.newPassword), request.Id);
            await connection.ExecuteAsync(command);
            
            command = String.Format("SELECT * FROM Users WHERE id = {0}", request.Id);
            return (await connection.QueryAsync<User>(command)).Single();
        }
        catch (Exception)
        {
            this.HttpContext.Response.StatusCode = 404;
            return new User();
        }
    }

    [HttpPatch]
    public async Task<User> UpdateEmail([FromBody] UserEmailRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Users WHERE id = {0}", request.Id);

        try
        {
            User user = (await connection.QueryAsync<User>(command)).Single();
            if (user.Email != request.oldEmail ||
                Int32.Parse(user.Password_Hash) != HashClass.HashPassword(request.password))
            {
                this.HttpContext.Response.StatusCode = 403;
                return user;
            }
            
            command = String.Format("UPDATE Users SET email = \'{0}\'" +
                                    " WHERE id = {1}", request.newEmail, request.Id);
            await connection.ExecuteAsync(command);
            
            command = String.Format("SELECT * FROM Users WHERE id = {0}", request.Id);
            return (await connection.QueryAsync<User>(command)).Single();
        }
        catch (Exception)
        {
            this.HttpContext.Response.StatusCode = 404;
            return new User();
        }
    }
    
    [HttpDelete]
    public async Task<int> DeleteUser([FromQuery] int id)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Users WHERE id = {0}", id);

        try
        {
            (await connection.QueryAsync<User>(command)).Single();
            command = String.Format("DELETE FROM Users WHERE id = {0}", id);
            await connection.ExecuteAsync(command);
            return 0;
        }
        catch (Exception)
        {
            this.HttpContext.Response.StatusCode = 404;
            return -1;
        }
    }
}