using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dotnet_bookish_starter.Models;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using dotnet_bookish_starter.AuxiliaryClasses;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_bookish_starter.Services;

public class UserServices
{
    private readonly string _connectionString;

    public UserServices(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<string> LogIn(UserCredentials credentials)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Users WHERE email like \'{0}\'", credentials.Email);
        User existingUser = await connection.QuerySingleOrDefaultAsync<User>(command);
        
        if (existingUser == null)
        {
            return "";
        }

        if (Int32.Parse(existingUser.Password_Hash) != HashClass.HashPassword(credentials.Password))
        {
            return "";
        }
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Email, existingUser.Email)
        };
        
        const int TokenLifetimeMinutes = 60;
        
        var jwt = new JwtSecurityToken(
            issuer: "https://bookish.com",
            audience: "https://bookish.com",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes),
            signingCredentials: new(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("SecretKey00000000000000000000000")),
                SecurityAlgorithms.HmacSha256Signature));

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
    
    public async Task<User> AddUser(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Users WHERE id = {0}", user.Id);
        User existingUser = await connection.QuerySingleOrDefaultAsync<User>(command);
        if (existingUser != null){
            return new User();
        }

        try
        {
            command = String.Format("INSERT INTO Users VALUES ({0}, \'{1}\', \'{2}\')",
                user.Id, user.Email, HashClass.HashPassword(user.Password_Hash));
            await connection.ExecuteAsync(command);
            command = String.Format("SELECT * FROM Users WHERE id = {0}", user.Id);
        }
        catch (Exception e)
        {
            return new User();
        }
        
        return await connection.QuerySingleOrDefaultAsync<User>(command);
    }
    
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
            return new User();
        }
    }
    
    public async Task<User> UpdatePassword(UserPasswordRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Users WHERE id = {0}", request.Id);
        try
        {
            User user =  (await connection.QueryAsync<User>(command)).Single();
            if (Int32.Parse(user.Password_Hash) != HashClass.HashPassword(request.oldPassword))
            {
                user.Password_Hash = Convert.ToString(request.oldPassword.GetHashCode());
                user.Email = "password incorrect";
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
            return new User();
        }
    }

    [HttpPatch("email")]
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
                user.Email = "password incorrect";
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
            return new User();
        }
    }
    
    public async Task<string> DeleteUser(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Users WHERE id = {0}", id);

        try
        {
            (await connection.QueryAsync<User>(command)).Single();
            command = String.Format("DELETE FROM Users WHERE id = {0}", id);
            await connection.ExecuteAsync(command);
            return "User deleted successfully!";
        }
        catch (Exception)
        {
            return "User did not exist!";
        }
    }
}
