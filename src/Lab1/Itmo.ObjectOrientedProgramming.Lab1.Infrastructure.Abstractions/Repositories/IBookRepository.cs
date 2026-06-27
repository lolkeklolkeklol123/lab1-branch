using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.Abstractions.Repositories;

public interface IBookRepository
{
    Book? Find(BookIdentifier identifier);

    void Add(Book book);
}
