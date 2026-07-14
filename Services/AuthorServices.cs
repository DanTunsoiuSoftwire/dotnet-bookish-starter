using dotnet_bookish_starter.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace dotnet_bookish_starter.Services;

public class AuthorServices
{
    private readonly string _connectionString;

    public AuthorServices(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<Author> AddAuthor(Author author)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Authors WHERE id = {0}", author.Id);
        Author existingAuthor = await connection.QuerySingleOrDefaultAsync<Author>(command);
        if (existingAuthor != null){
            return existingAuthor;
        }
        
        command = String.Format("INSERT INTO Authors VALUES ({0}, \'{1}\')",
            author.Id, author.author_name);
        try
        {
            await connection.ExecuteAsync(command);
        }
        catch (Exception e)
        {
            return new  Author();
        }

        command = String.Format("SELECT * FROM Authors WHERE id = {0}", author.Id);
        
        return await connection.QuerySingleOrDefaultAsync<Author>(command);;
    }
    
    public async Task<Author> GetAuthor(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Authors WHERE id = {0}", id);
        try
        {
            return (await connection.QueryAsync<Author>(command)).Single();
        }
        catch (Exception)
        {
            return new Author();
        }
    }
    
    public async Task<Author> GetAuthorByName(string name)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Authors WHERE author_name = \'{0}\'", name);
        try
        {
            return (await connection.QueryAsync<Author>(command)).Single();
        }
        catch (Exception)
        {
            return new Author();
        }
    }
    
    public async Task<string> DeleteAuthor(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Authors WHERE id = {0}", id);

        try
        {
            (await connection.QueryAsync<User>(command)).Single();
            command = String.Format("DELETE FROM Authors WHERE id = {0}", id);
            await connection.ExecuteAsync(command);
            return "Author deleted successfully.";
        }
        catch (Exception)
        {
            return "Author did not exist.";
        }
    }
}