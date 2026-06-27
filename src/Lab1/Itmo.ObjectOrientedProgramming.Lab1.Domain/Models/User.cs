using Itmo.ObjectOrientedProgramming.Lab1.Exceptions;
using System.Collections.ObjectModel;

namespace Itmo.ObjectOrientedProgramming.Lab1.Models;

public class User
{
    private readonly HashSet<UserRole> _roles;
    private readonly HashSet<BookIdentifier> _borrowedBooks;

    public User(string name, IEnumerable<UserRole> roles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(roles);

        var userRoles = new List<UserRole>(roles);

        if (userRoles.Count == 0)
            throw new ArgumentException("At least one role must be provided.", nameof(roles));

        foreach (UserRole role in userRoles)
        {
            if (role == UserRole.Undefined || !Enum.IsDefined(role))
                throw new ArgumentException("User role is invalid.", nameof(roles));
        }

        Name = name;
        _roles = new HashSet<UserRole>(userRoles);
        _borrowedBooks = new HashSet<BookIdentifier>();
    }

    public string Name { get; }

    public IReadOnlyCollection<UserRole> Roles => new ReadOnlyCollection<UserRole>(new List<UserRole>(_roles));

    public bool HasRole(UserRole role) => _roles.Contains(role);

    public bool HasBorrowed(BookIdentifier identifier) => _borrowedBooks.Contains(identifier);

    public void Borrow(BookIdentifier identifier)
    {
        if (!_borrowedBooks.Add(identifier))
            throw new BusinessRuleViolationException("The book has already been borrowed by this user.");
    }

    public void Return(BookIdentifier identifier)
    {
        if (!_borrowedBooks.Remove(identifier))
            throw new BusinessRuleViolationException("The book was not borrowed by this user.");
    }
}
