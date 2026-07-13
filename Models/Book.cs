namespace dotnet_bookish_starter.Models;

public class Book
{
    // TODO add the relevant properties
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int ISBN {get; set;}
    public int copies_owned {get; set;}
    public LinkedList<string> Authors { get; set; } = new LinkedList<string>();
}

