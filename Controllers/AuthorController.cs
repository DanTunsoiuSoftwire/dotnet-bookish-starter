using dotnet_bookish_starter.Models;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using Microsoft.Data.SqlClient;

namespace dotnet_bookish_starter.Controllers;

[ApiController]
[Route("author")]
public class AuthorController : ControllerBase
{
    private readonly string _connectionString;
 
    public AuthorController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DbConnectionString") ?? "";
    }

    [HttpPost]
    public async Task<Author> AddAuthor([FromBody] Author author)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Authors WHERE id = {0}", author.Id);
        Author existingAuthor = await connection.QuerySingleOrDefaultAsync<Author>(command);
        if (existingAuthor != null){
            this.HttpContext.Response.StatusCode = 409;
            return existingAuthor;
        }
        
        command = String.Format("INSERT INTO Authors VALUES ({0}, \'{1}\')",
            author.Id, author.author_name);
        await connection.ExecuteAsync(command);
        command = String.Format("SELECT * FROM Authors WHERE id = {0}", author.Id);
        
        return await connection.QuerySingleOrDefaultAsync<Author>(command);;
    }
    
    [HttpGet("{id}")]
    public async Task<Author> GetAuthor([FromRoute] int id)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Authors WHERE id = {0}", id);
        try
        {
            return (await connection.QueryAsync<Author>(command)).Single();
        }
        catch (Exception)
        {
            this.HttpContext.Response.StatusCode = 404;
            return new Author();
        }
    }
    
    [HttpGet("name/{name}")]
    public async Task<Author> GetAuthorByName([FromRoute] string name)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Authors WHERE author_name = \'{0}\'", name);
        try
        {
            return (await connection.QueryAsync<Author>(command)).Single();
        }
        catch (Exception)
        {
            this.HttpContext.Response.StatusCode = 404;
            return new Author();
        }
    }

    [HttpDelete("{id}")]
    public async Task<int> DeleteAuthor([FromRoute] int id)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Authors WHERE id = {0}", id);

        try
        {
            (await connection.QueryAsync<User>(command)).Single();
            command = String.Format("DELETE FROM Authors WHERE id = {0}", id);
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
