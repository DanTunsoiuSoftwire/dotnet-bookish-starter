using System.Diagnostics;
using dotnet_bookish_starter.Models;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using dotnet_bookish_starter.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

namespace dotnet_bookish_starter.Controllers;

[ApiController]
[Route("book")]
public class BookController : ControllerBase
{
    private readonly string _connectionString;
    private BookServices _bookServices;
 
    public BookController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DbConnectionString") ?? "";
        _bookServices = new BookServices(_connectionString);
    }

    [HttpGet]
    public async Task<IEnumerable<Book>> GetBooks()
    {
        return await _bookServices.GetBooks();
    }
    
    [HttpGet("{id}")]
    public async Task<Book> GetBook([FromRoute] int id)
    {
        Book bookFound = await _bookServices.GetBook(id);

        if (bookFound.Title == null!)
        {
            this.HttpContext.Response.StatusCode = 404;
        }

        return bookFound;
    }
    
    [HttpGet("available/{id}")]
    public async Task<int> GetNumberOfAvailableCopies([FromRoute] int id)
    {
        return await _bookServices.GetNumberOfAvailableCopies(id);
    }
    
    [HttpPost]
    public async Task<Book> AddBook([FromBody] Book book)
    {
        Book bookAdded = await _bookServices.AddBook(book);

        if (bookAdded.Title == null!)
        {
            this.HttpContext.Response.StatusCode = 404;
            return bookAdded;
        }

        if (bookAdded.Title != book.Title)
        {
            this.HttpContext.Response.StatusCode = 409;
            return bookAdded;
        }

        return bookAdded;
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<string> DeleteBook([FromRoute] int id)
    {
        return await _bookServices.DeleteBook(id);
    }

    [HttpPatch("{id}")]
    public async Task<Book> UpdateBook([FromBody] Book book)
    {
        Book bookUpdated = await _bookServices.UpdateBook(book);

        if (bookUpdated.Title == null!)
        {
            this.HttpContext.Response.StatusCode = 404;
        }
        
        return bookUpdated;
    }
}

