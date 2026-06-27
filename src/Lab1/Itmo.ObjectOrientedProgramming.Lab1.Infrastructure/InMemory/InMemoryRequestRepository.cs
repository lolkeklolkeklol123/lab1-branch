using Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.Abstractions.Repositories;
using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.InMemory;

public class InMemoryRequestRepository : IRequestRepository
{
    private readonly Dictionary<int, LibraryRequest> _requests = new Dictionary<int, LibraryRequest>();

    public void Add(LibraryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _requests[request.Id] = request;
    }

    public LibraryRequest? Find(int id)
    {
        return _requests.TryGetValue(id, out LibraryRequest? request) ? request : null;
    }

    public void Remove(int id)
    {
        _requests.Remove(id);
    }
}
