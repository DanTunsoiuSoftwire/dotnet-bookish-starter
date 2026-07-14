using dotnet_bookish_starter.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace dotnet_bookish_starter.Services;

public class BookServices
{
    private readonly string _connectionString;

    public BookServices(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<IEnumerable<Book>> GetBooks()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<Book>("SELECT * FROM Books");
    }
    
    public async Task<Book> GetBook(int id)
    {
        Book foundBook = new  Book();
        string command;
        using var connection = new SqlConnection(_connectionString);
        try
        {
            command = String.Format("SELECT * FROM Books WHERE id = {0}", id);
            foundBook = (await connection.QueryAsync<Book>(command)).Single();
        }
        catch (Exception)
        {
            return new Book();
        }
        
        command = String.Format("SELECT * FROM Book_Author WHERE book_id = {0}", id);
        List<Book_Author> authors = (await connection.QueryAsync<Book_Author>(command)).ToList();
        
        foreach (Book_Author bookAuthor in authors)
        {
            command = String.Format("SELECT * FROM Authors WHERE id = {0}", bookAuthor.author_Id);
            Console.WriteLine(bookAuthor.author_Id);
            try
            {
                Author authorFound = (await connection.QueryAsync<Author>(command)).Single();
                foundBook.Authors.AddLast(authorFound.author_name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        return foundBook;
    }
    
    public async Task<int> GetNumberOfAvailableCopies(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        try
        {
            string command = String.Format("SELECT * FROM Books WHERE id = {0}", id);
            return (await connection.QueryAsync<Book>(command)).Single().copies_owned;
        }
        catch (Exception)
        {
            return 0;
        }
    }
    
    public async Task<Book> AddBook(Book book)
    {
        using var connection = new SqlConnection(_connectionString);
        
        string checkCommand = String.Format("SELECT * FROM Books WHERE id = {0}", book.Id);
        if ((await connection.QueryAsync<Book>(checkCommand)).Any())
        {
            return book;
        }
        
        string command = String.Format("INSERT INTO Books VALUES ({0}, \'{1}\', {2}, {3})",
            book.Id, book.Title, book.ISBN, book.copies_owned);
        await connection.ExecuteAsync(command);
        
        foreach (string author in book.Authors)
        {
            try
            {
                command = String.Format("SELECT * FROM Authors WHERE author_name = \'{0}\'", author);
                Author authorFound = (await connection.QueryAsync<Author>(command)).Single();
                command = String.Format("INSERT INTO Book_Author VALUES ({0}, {1}, {2})",
                    book.Id * authorFound.Id, book.Id, authorFound.Id);
                await connection.ExecuteAsync(command);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new Book();
            }
        }
        
        Book addedBook = (await connection.QueryAsync<Book>(checkCommand)).First();
        command = String.Format("SELECT * FROM Books WHERE book_id = {0}", addedBook.Id);
        List<Book_Author> authors = (await connection.QueryAsync<Book_Author>(command)).ToList();
        foreach (Book_Author bookAuthor in authors)
        {
            command = String.Format("SELECT * FROM Authors WHERE id = {0}", bookAuthor.author_Id);
            addedBook.Authors.AddLast((await connection.QueryAsync<Author>(command)).Single().author_name);
        }
        return addedBook;
    }
    
    public async Task<string> DeleteBook(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        string command = String.Format("SELECT * FROM Books WHERE id = {0}", id);
        try
        {
            connection.Query<Book>(command).Single();
            command = String.Format("DELETE FROM Books WHERE id = {0}", id);
            await connection.QueryAsync<Book>(command);
            return "Book deleted successfully.";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "Book did not exist.";
        }
    }
    
    public async Task<Book> UpdateBook(Book book)
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
            return new Book();
        }
    }
}