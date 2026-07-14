using dotnet_bookish_starter.Models;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using dotnet_bookish_starter.Services;
using Microsoft.Data.SqlClient;

namespace dotnet_bookish_starter.Controllers;

[ApiController]
[Route("author")]
public class AuthorController : ControllerBase
{
    private readonly string _connectionString;
    AuthorServices _authorServices;
 
    public AuthorController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DbConnectionString") ?? "";
        _authorServices = new AuthorServices(_connectionString);
    }

    [HttpPost]
    public async Task<Author> AddAuthor([FromBody] Author author)
    {
        Author authorAdded = await _authorServices.AddAuthor(author);
        if (authorAdded.author_name == null!)
        {
            this.HttpContext.Response.StatusCode = 500;
            return authorAdded;
        }

        if (authorAdded.author_name != author.author_name)
        {
            this.HttpContext.Response.StatusCode = 409;
            return authorAdded;
        }
        
        this.HttpContext.Response.StatusCode = 201;
        return authorAdded;
    }
    
    [HttpGet("{id}")]
    public async Task<Author> GetAuthor([FromRoute] int id)
    {
        Author authorFound = await _authorServices.GetAuthor(id);
        if (authorFound.author_name == null!)
        {
            this.HttpContext.Response.StatusCode = 404;
        }
        
        return authorFound;
    }
    
    [HttpGet("name/{name}")]
    public async Task<Author> GetAuthorByName([FromRoute] string name)
    {
        Author authorFound = await _authorServices.GetAuthorByName(name);
        if (authorFound.author_name == null!)
        {
            this.HttpContext.Response.StatusCode = 404;
        }
        
        return authorFound;
    }

    [HttpDelete("{id}")]
    public async Task<string> DeleteAuthor([FromRoute] int id)
    {
        return await _authorServices.DeleteAuthor(id);
    }
}
