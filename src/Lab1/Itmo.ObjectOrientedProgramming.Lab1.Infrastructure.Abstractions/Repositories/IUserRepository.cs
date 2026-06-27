using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.Abstractions.Repositories;

public interface IUserRepository
{
    bool Exists(string name);

    void Add(User user);

    User? Find(string name);
}
