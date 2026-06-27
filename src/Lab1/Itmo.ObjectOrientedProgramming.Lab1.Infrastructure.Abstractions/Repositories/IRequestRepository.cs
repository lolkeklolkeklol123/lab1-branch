using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.Abstractions.Repositories;

public interface IRequestRepository
{
    void Add(LibraryRequest request);

    LibraryRequest? Find(int id);

    void Remove(int id);
}
