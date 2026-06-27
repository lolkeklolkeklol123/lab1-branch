using Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.Abstractions.Repositories;
using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.InMemory;

public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, User> _users = new Dictionary<string, User>(StringComparer.Ordinal);

    public bool Exists(string name) => _users.ContainsKey(name);

    public void Add(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _users[user.Name] = user;
    }

    public User? Find(string name)
    {
        return _users.TryGetValue(name, out User? user) ? user : null;
    }
}
