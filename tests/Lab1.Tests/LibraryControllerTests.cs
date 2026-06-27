using Itmo.ObjectOrientedProgramming.Lab1.Application.Services;
using Itmo.ObjectOrientedProgramming.Lab1.Exceptions;
using Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.InMemory;
using Itmo.ObjectOrientedProgramming.Lab1.Models;
using Itmo.ObjectOrientedProgramming.Lab1.Presentation.Controllers;
using Xunit;

namespace Itmo.ObjectOrientedProgramming.Lab1.Tests;

public class LibraryControllerTests
{
    private const string LibrarianName = "Librarian";
    private const string WriterName = "Author";
    private const string ReaderName = "Reader";
    private const string BookTitle = "Test Book";

    [Fact]
    public void RegisterUser_WhenNameIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.RegisterUser(string.Empty, new[] { UserRole.Reader }));
    }

    [Fact]
    public void RegisterUser_WhenNameIsWhitespace_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.RegisterUser("   ", new[] { UserRole.Reader }));
    }

    [Fact]
    public void RegisterUser_WhenRolesIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateController();

        Assert.Throws<ArgumentException>(
            () => controller.RegisterUser("NewUser", Array.Empty<UserRole>()));
    }

    [Fact]
    public void RegisterUser_WhenRoleIsUndefined_ShouldThrow()
    {
        LibraryController controller = CreateController();

        Assert.Throws<ArgumentException>(
            () => controller.RegisterUser("NewUser", new[] { UserRole.Undefined }));
    }

    [Fact]
    public void RegisterUser_WhenNameIsDuplicate_ShouldThrow()
    {
        LibraryController controller = CreateController();
        controller.RegisterUser("Alice", new[] { UserRole.Reader });

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.RegisterUser("Alice", new[] { UserRole.Reader }));
    }

    [Fact]
    public void RegisterUser_WhenValid_ShouldReturnUser()
    {
        LibraryController controller = CreateController();

        User user = controller.RegisterUser("Alice", new[] { UserRole.Reader });

        Assert.Equal("Alice", user.Name);
        Assert.True(user.HasRole(UserRole.Reader));
    }

    [Fact]
    public void SubmitAddBook_WhenEditionSizeIsZero_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.SubmitAddBook(WriterName, BookTitle, WriterName, 0, 1));
    }

    [Fact]
    public void SubmitAddBook_WhenEditionSizeIsNegative_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.SubmitAddBook(WriterName, BookTitle, WriterName, -5, 1));
    }

    [Fact]
    public void SubmitAddBook_WhenQuantityIsZero_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.SubmitAddBook(WriterName, BookTitle, WriterName, 5, 0));
    }

    [Fact]
    public void SubmitAddBook_WhenQuantityIsNegative_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.SubmitAddBook(WriterName, BookTitle, WriterName, 5, -3));
    }

    [Fact]
    public void SubmitAddBook_WhenTitleIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.SubmitAddBook(WriterName, string.Empty, WriterName, 5, 2));
    }

    [Fact]
    public void SubmitAddBook_WhenAuthorNameIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.SubmitAddBook(WriterName, BookTitle, string.Empty, 5, 2));
    }

    [Fact]
    public void SubmitAddBook_WhenWriterNameIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.SubmitAddBook(string.Empty, BookTitle, WriterName, 5, 2));
    }

    [Fact]
    public void SubmitAddBook_WhenQuantityExceedsEditionSize_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.SubmitAddBook(WriterName, BookTitle, WriterName, 3, 10));
    }

    [Fact]
    public void SubmitAddBook_WhenWriterIsNotAuthor_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.SubmitAddBook(WriterName, BookTitle, "SomeoneElse", 5, 2));
    }

    [Fact]
    public void SubmitAddBook_WhenValid_ShouldReturnPendingRequest()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        LibraryRequest request = controller.SubmitAddBook(WriterName, BookTitle, WriterName, 5, 2);

        Assert.Equal(RequestStatus.Pending, request.Status);
        Assert.Equal(RequestType.AddBook, request.Type);
    }

    [Fact]
    public void SubmitBorrowBook_WhenReaderNameIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.SubmitBorrowBook(string.Empty, BookTitle, WriterName));
    }

    [Fact]
    public void SubmitBorrowBook_WhenTitleIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.SubmitBorrowBook(ReaderName, string.Empty, WriterName));
    }

    [Fact]
    public void SubmitBorrowBook_WhenAuthorNameIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.SubmitBorrowBook(ReaderName, BookTitle, string.Empty));
    }

    [Fact]
    public void SubmitBorrowBook_WhenValid_ShouldReturnPendingRequest()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        LibraryRequest request = controller.SubmitBorrowBook(ReaderName, BookTitle, WriterName);

        Assert.Equal(RequestStatus.Pending, request.Status);
        Assert.Equal(RequestType.BorrowBook, request.Type);
    }

    [Fact]
    public void SubmitReturnBook_WhenReaderNameIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.SubmitReturnBook(string.Empty, BookTitle, WriterName));
    }

    [Fact]
    public void SubmitReturnBook_WhenTitleIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.SubmitReturnBook(ReaderName, string.Empty, WriterName));
    }

    [Fact]
    public void SubmitReturnBook_WhenAuthorNameIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => controller.SubmitReturnBook(ReaderName, BookTitle, string.Empty));
    }

    [Fact]
    public void Approve_WhenLibrarianNameIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();
        LibraryRequest request = controller.SubmitAddBook(WriterName, BookTitle, WriterName, 5, 2);

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.Approve(string.Empty, request.Id));
    }

    [Fact]
    public void Approve_WhenRequestIdNotFound_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.Approve(LibrarianName, 999));
    }

    [Fact]
    public void Approve_WhenValid_ShouldReturnApprovedRequest()
    {
        LibraryController controller = CreateControllerWithBasicUsers();
        LibraryRequest request = controller.SubmitAddBook(WriterName, BookTitle, WriterName, 5, 2);

        LibraryRequest approved = controller.Approve(LibrarianName, request.Id);

        Assert.Equal(RequestStatus.Approved, approved.Status);
    }

    [Fact]
    public void Reject_WhenLibrarianNameIsEmpty_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();
        LibraryRequest request = controller.SubmitAddBook(WriterName, BookTitle, WriterName, 5, 2);

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.Reject(string.Empty, request.Id));
    }

    [Fact]
    public void Reject_WhenRequestIdNotFound_ShouldThrow()
    {
        LibraryController controller = CreateControllerWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => controller.Reject(LibrarianName, 999));
    }

    [Fact]
    public void Reject_WhenValid_ShouldReturnRejectedRequest()
    {
        LibraryController controller = CreateControllerWithBasicUsers();
        LibraryRequest request = controller.SubmitAddBook(WriterName, BookTitle, WriterName, 5, 2);

        LibraryRequest rejected = controller.Reject(LibrarianName, request.Id);

        Assert.Equal(RequestStatus.Rejected, rejected.Status);
    }

    [Fact]
    public void GetRequest_WhenExists_ShouldReturnRequest()
    {
        LibraryController controller = CreateControllerWithBasicUsers();
        LibraryRequest request = controller.SubmitAddBook(WriterName, BookTitle, WriterName, 5, 2);

        LibraryRequest? found = controller.GetRequest(request.Id);

        Assert.NotNull(found);
        Assert.Equal(request.Id, found?.Id);
    }

    [Fact]
    public void GetRequest_WhenNotExists_ShouldReturnNull()
    {
        LibraryController controller = CreateController();

        LibraryRequest? found = controller.GetRequest(999);

        Assert.Null(found);
    }

    [Fact]
    public void GetBook_WhenExists_ShouldReturnBook()
    {
        LibraryController controller = CreateControllerWithBasicUsers();
        controller.SubmitAddBook(WriterName, BookTitle, WriterName, 5, 2);
        controller.Approve(LibrarianName, 1);

        var identifier = new BookIdentifier(BookTitle, WriterName);
        Book? book = controller.GetBook(identifier);

        Assert.NotNull(book);
    }

    [Fact]
    public void GetBook_WhenNotExists_ShouldReturnNull()
    {
        LibraryController controller = CreateController();

        Book? book = controller.GetBook(new BookIdentifier("Ghost", "Nobody"));

        Assert.Null(book);
    }

    private static LibraryController CreateControllerWithBasicUsers()
    {
        LibraryController controller = CreateController();
        controller.RegisterUser(LibrarianName, new[] { UserRole.Librarian });
        controller.RegisterUser(WriterName, new[] { UserRole.Writer });
        controller.RegisterUser(ReaderName, new[] { UserRole.Reader });
        return controller;
    }

    private static LibraryController CreateController()
    {
        var service = new LibraryService(
            new InMemoryUserRepository(),
            new InMemoryBookRepository(),
            new InMemoryRequestRepository());

        return new LibraryController(service);
    }
}
