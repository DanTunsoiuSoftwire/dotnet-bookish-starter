using System.Diagnostics;
using dotnet_bookish_starter.Models;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

namespace dotnet_bookish_starter.Controllers;

[ApiController]
[Route("book/[action]")]
public class BookController : ControllerBase
{
    private readonly string _connectionString;
 
    public BookController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DbConnectionString") ?? "";
    }

    [HttpGet]
    public async Task<IEnumerable<Book>> GetBooks()
    {
        // TODO implement the GET method
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<Book>("SELECT * FROM Books");
        throw new NotImplementedException();
    }
    
    [HttpGet]
    public async Task<Book> GetBook([FromQuery] int id)
    {
        // TODO implement the GET method
        using var connection = new SqlConnection(_connectionString);
        try
        {
            string command = String.Format("SELECT * FROM Books WHERE id = {0}", id);
            return (await connection.QueryAsync<Book>(command)).Single();
        }
        catch (Exception)
        {
            this.HttpContext.Response.StatusCode = 404;
            return new Book();
        }
        throw new NotImplementedException();
    }
    
    [HttpGet]
    public async Task<int> GetNumberOfAvailableCopies([FromQuery] int id)
    {
        // TODO implement the GET method
        using var connection = new SqlConnection(_connectionString);
        try
        {
            string command = String.Format("SELECT * FROM Books WHERE id = {0}", id);
            return (await connection.QueryAsync<Book>(command)).Single().copies_owned;
        }
        catch (Exception)
        {
            this.HttpContext.Response.StatusCode = 404;
            return 0;
        }
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public async Task<Book> AddBook([FromBody] Book book)
    {
        // TODO implement the POST method
        using var connection = new SqlConnection(_connectionString);
        
        string checkCommand = String.Format("SELECT * FROM Books WHERE id = {0}", book.Id);
        if ((await connection.QueryAsync<Book>(checkCommand)).Any())
        {
            this.HttpContext.Response.StatusCode = 409;
            return book;
        }
        
        string command = String.Format("INSERT INTO Books VALUES ({0}, \'{1}\', {2}, {3})",
            book.Id, book.Title, book.ISBN, book.copies_owned);
        await connection.ExecuteAsync(command);
        return (await connection.QueryAsync<Book>(checkCommand)).First();
        throw new NotImplementedException();
    }

    [Authorize]
    [HttpDelete]
    public async Task<int> DeleteBook([FromQuery] int id)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Books WHERE id = {0}", id);
        try
        {
            connection.Query<Book>(command).Single();
            command = String.Format("DELETE FROM Books WHERE id = {0}", id);
            await connection.QueryAsync<Book>(command);
            return id;
        }
        catch (Exception)
        {
            this.HttpContext.Response.StatusCode = 404;
            return -1;
        }
    }

    [HttpPatch]
    public async Task<Book> UpdateBook([FromBody] Book book)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Books WHERE id = {0}", book.Id);
        try
        {
            Book existingBook = connection.Query<Book>(command).Single();
            if (book.Title == "")
            {
                book.Title = existingBook.Title;
            }

            if (book.ISBN == 0)
            {
                book.ISBN = existingBook.ISBN;
            }
            
            command = String.Format("UPDATE Books SET title = \'{0}\', isbn = {1}, copies_owned = {2}" +
                                    " WHERE id = {3}", book.Title, book.ISBN, book.copies_owned, book.Id);
            
            await connection.QueryAsync<Book>(command);
            return book;
        }
        catch (Exception)
        {
            this.HttpContext.Response.StatusCode = 404;
            return new Book();
        }
    }
}
