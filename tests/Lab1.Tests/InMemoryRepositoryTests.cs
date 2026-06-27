using Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.InMemory;
using Itmo.ObjectOrientedProgramming.Lab1.Models;
using Xunit;

namespace Itmo.ObjectOrientedProgramming.Lab1.Tests;

public class InMemoryRepositoryTests
{
    private static readonly BookIdentifier SampleIdentifier = new BookIdentifier("Clean Code", "Robert Martin");

    [Fact]
    public void BookRepository_AfterAdd_FindReturnsSameBook()
    {
        var repo = new InMemoryBookRepository();
        var book = new Book(SampleIdentifier, 10, 0);

        repo.Add(book);
        Book? found = repo.Find(SampleIdentifier);

        Assert.NotNull(found);
        Assert.Equal(SampleIdentifier, found?.Identifier);
        Assert.Equal(10, found?.EditionSize);
    }

    [Fact]
    public void BookRepository_FindWhenEmpty_ReturnsNull()
    {
        var repo = new InMemoryBookRepository();

        Book? found = repo.Find(SampleIdentifier);

        Assert.Null(found);
    }

    [Fact]
    public void BookRepository_FindForUnknownIdentifier_ReturnsNull()
    {
        var repo = new InMemoryBookRepository();
        var book = new Book(SampleIdentifier, 5, 0);
        repo.Add(book);

        Book? found = repo.Find(new BookIdentifier("Other Title", "Other Author"));

        Assert.Null(found);
    }

    [Fact]
    public void BookRepository_FindReturnsMutableReference_MutationsAreVisible()
    {
        var repo = new InMemoryBookRepository();
        var book = new Book(SampleIdentifier, 5, 0);
        repo.Add(book);

        repo.Find(SampleIdentifier)?.AddCopies(3);
        Book? found = repo.Find(SampleIdentifier);

        Assert.Equal(3, found?.StoredCopies);
    }

    [Fact]
    public void UserRepository_AfterAdd_ExistsReturnsTrue()
    {
        var repo = new InMemoryUserRepository();
        var user = new User("Alice", new[] { UserRole.Reader });

        repo.Add(user);

        Assert.True(repo.Exists("Alice"));
    }

    [Fact]
    public void UserRepository_WhenUserNotAdded_ExistsReturnsFalse()
    {
        var repo = new InMemoryUserRepository();

        Assert.False(repo.Exists("Alice"));
    }

    [Fact]
    public void UserRepository_AfterAdd_FindReturnsSameUser()
    {
        var repo = new InMemoryUserRepository();
        var user = new User("Alice", new[] { UserRole.Reader });

        repo.Add(user);
        User? found = repo.Find("Alice");

        Assert.NotNull(found);
        Assert.Equal("Alice", found?.Name);
        Assert.True(found?.HasRole(UserRole.Reader));
    }

    [Fact]
    public void UserRepository_FindWhenEmpty_ReturnsNull()
    {
        var repo = new InMemoryUserRepository();

        User? found = repo.Find("Alice");

        Assert.Null(found);
    }

    [Fact]
    public void UserRepository_FindForUnknownName_ReturnsNull()
    {
        var repo = new InMemoryUserRepository();
        repo.Add(new User("Alice", new[] { UserRole.Reader }));

        User? found = repo.Find("Bob");

        Assert.Null(found);
    }

    [Fact]
    public void UserRepository_MultipleUsers_FindReturnsCorrectOne()
    {
        var repo = new InMemoryUserRepository();
        repo.Add(new User("Alice", new[] { UserRole.Reader }));
        repo.Add(new User("Bob", new[] { UserRole.Writer }));

        User? found = repo.Find("Bob");

        Assert.NotNull(found);
        Assert.True(found?.HasRole(UserRole.Writer));
        Assert.False(found?.HasRole(UserRole.Reader));
    }

    [Fact]
    public void RequestRepository_AfterAdd_FindReturnsSameRequest()
    {
        var repo = new InMemoryRequestRepository();
        var identifier = new BookIdentifier("Title", "Author");
        var request = new LibraryRequest(1, RequestType.AddBook, "Writer", identifier, 2, 5);

        repo.Add(request);
        LibraryRequest? found = repo.Find(1);

        Assert.NotNull(found);
        Assert.Equal(1, found?.Id);
        Assert.Equal(RequestType.AddBook, found?.Type);
        Assert.Equal(RequestStatus.Pending, found?.Status);
    }

    [Fact]
    public void RequestRepository_FindWhenEmpty_ReturnsNull()
    {
        var repo = new InMemoryRequestRepository();

        LibraryRequest? found = repo.Find(1);

        Assert.Null(found);
    }

    [Fact]
    public void RequestRepository_FindForUnknownId_ReturnsNull()
    {
        var repo = new InMemoryRequestRepository();
        var identifier = new BookIdentifier("Title", "Author");
        repo.Add(new LibraryRequest(1, RequestType.AddBook, "Writer", identifier, 2, 5));

        LibraryRequest? found = repo.Find(99);

        Assert.Null(found);
    }

    [Fact]
    public void RequestRepository_AfterRemove_FindReturnsNull()
    {
        var repo = new InMemoryRequestRepository();
        var identifier = new BookIdentifier("Title", "Author");
        repo.Add(new LibraryRequest(1, RequestType.AddBook, "Writer", identifier, 2, 5));

        repo.Remove(1);
        LibraryRequest? found = repo.Find(1);

        Assert.Null(found);
    }

    [Fact]
    public void RequestRepository_RemoveNonExistent_DoesNotThrow()
    {
        var repo = new InMemoryRequestRepository();

        Exception? exception = Record.Exception(() => repo.Remove(999));

        Assert.Null(exception);
    }

    [Fact]
    public void RequestRepository_MultipleRequests_FindReturnsCorrectOne()
    {
        var repo = new InMemoryRequestRepository();
        var identifier = new BookIdentifier("Title", "Author");
        repo.Add(new LibraryRequest(1, RequestType.AddBook, "Writer", identifier, 2, 5));
        repo.Add(new LibraryRequest(2, RequestType.BorrowBook, "Reader", identifier, 1, 0));

        LibraryRequest? found = repo.Find(2);

        Assert.NotNull(found);
        Assert.Equal(RequestType.BorrowBook, found?.Type);
    }

    [Fact]
    public void RequestRepository_RemoveOneOfTwo_OtherStillExists()
    {
        var repo = new InMemoryRequestRepository();
        var identifier = new BookIdentifier("Title", "Author");
        repo.Add(new LibraryRequest(1, RequestType.AddBook, "Writer", identifier, 2, 5));
        repo.Add(new LibraryRequest(2, RequestType.BorrowBook, "Reader", identifier, 1, 0));

        repo.Remove(1);

        Assert.Null(repo.Find(1));
        Assert.NotNull(repo.Find(2));
    }
}
