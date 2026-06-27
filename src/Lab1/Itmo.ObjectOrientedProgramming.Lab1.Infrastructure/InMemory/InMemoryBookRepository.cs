using Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.Abstractions.Repositories;
using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.InMemory;

public class InMemoryBookRepository : IBookRepository
{
    private readonly Dictionary<BookIdentifier, Book> _books = new Dictionary<BookIdentifier, Book>();

    public Book? Find(BookIdentifier identifier)
    {
        return _books.TryGetValue(identifier, out Book? book) ? book : null;
    }

    public void Add(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        _books[book.Identifier] = book;
    }
}
