using Itmo.ObjectOrientedProgramming.Lab1.Application.Services;
using Itmo.ObjectOrientedProgramming.Lab1.Exceptions;
using Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.InMemory;
using Itmo.ObjectOrientedProgramming.Lab1.Models;
using Xunit;

namespace Itmo.ObjectOrientedProgramming.Lab1.Tests;

public class LibraryServiceTests
{
    private const string LibrarianName = "Librarian";
    private const string WriterName = "Author";
    private const string ReaderName = "Reader";
    private const string BookTitle = "Interesting Book";

    [Fact]
    public void RegisterUser_WhenNameIsDuplicate_ShouldThrow()
    {
        LibraryService service = CreateService();
        service.RegisterUser("Duplicated", new[] { UserRole.Reader });

        Assert.Throws<BusinessRuleViolationException>(
            () => service.RegisterUser("Duplicated", new[] { UserRole.Reader }));
    }

    [Fact]
    public void RegisterUser_WhenNameIsEmpty_ShouldThrow()
    {
        LibraryService service = CreateService();

        Assert.Throws<ArgumentException>(
            () => service.RegisterUser(string.Empty, new[] { UserRole.Reader }));
    }

    [Fact]
    public void RegisterUser_WhenRolesListIsEmpty_ShouldThrow()
    {
        LibraryService service = CreateService();

        Assert.Throws<ArgumentException>(
            () => service.RegisterUser("NewUser", Array.Empty<UserRole>()));
    }

    [Fact]
    public void RegisterUser_WhenRoleIsUndefined_ShouldThrow()
    {
        LibraryService service = CreateService();

        Assert.Throws<ArgumentException>(
            () => service.RegisterUser("NewUser", new[] { UserRole.Undefined }));
    }

    [Fact]
    public void CreateAddBookRequest_WhenWriterDifferentFromAuthor_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateAddBookRequest(WriterName, BookTitle, "Another Author", 10, 3));
    }

    [Fact]
    public void CreateAddBookRequest_WhenUserIsNotWriter_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateAddBookRequest(ReaderName, BookTitle, ReaderName, 3, 2));
    }

    [Fact]
    public void CreateAddBookRequest_WhenUserNotFound_ShouldThrow()
    {
        LibraryService service = CreateService();

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateAddBookRequest("NoSuch", BookTitle, "NoSuch", 3, 2));
    }

    [Fact]
    public void CreateAddBookRequest_WhenQuantityExceedsEditionSize_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 2, 5));
    }

    [Fact]
    public void CreateAddBookRequest_WhenEditionSizeIsZero_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 0, 1));
    }

    [Fact]
    public void CreateAddBookRequest_WhenQuantityIsZero_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        Assert.Throws<ArgumentException>(
            () => service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 0));
    }

    [Fact]
    public void ApproveAddBookRequest_ShouldIncreaseAvailableCopies()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        LibraryRequest request = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 5, 2);
        LibraryRequest approved = service.ApproveRequest(LibrarianName, request.Id);

        Assert.Equal(RequestStatus.Approved, approved.Status);
        var identifier = new BookIdentifier(BookTitle, WriterName);
        Book? book = service.FindBook(identifier);

        Assert.NotNull(book);
        Assert.Equal(2, book?.AvailableCopies);
        Assert.Equal(2, book?.StoredCopies);
    }

    [Fact]
    public void AddBook_WhenCreatorIsLibrarian_ShouldBeAutoApproved()
    {
        LibraryService service = CreateService();
        service.RegisterUser("WriterLibrarian", new[] { UserRole.Writer, UserRole.Librarian });

        LibraryRequest request = service.CreateAddBookRequest(
            "WriterLibrarian",
            BookTitle,
            "WriterLibrarian",
            4,
            3);

        Assert.Equal(RequestStatus.Approved, request.Status);
        var identifier = new BookIdentifier(BookTitle, "WriterLibrarian");
        Book? book = service.FindBook(identifier);

        Assert.NotNull(book);
        Assert.Equal(3, book?.AvailableCopies);
    }

    [Fact]
    public void AddBook_WhenExceedsEditionSize_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 2, 2);
        service.ApproveRequest(LibrarianName, 1);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 2, 1));
    }

    [Fact]
    public void AddBook_WhenSecondBatchWithinEditionSize_ShouldSucceed()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 5, 2);
        service.ApproveRequest(LibrarianName, 1);

        LibraryRequest request2 = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 5, 2);
        service.ApproveRequest(LibrarianName, request2.Id);

        var identifier = new BookIdentifier(BookTitle, WriterName);
        Book? book = service.FindBook(identifier);
        Assert.NotNull(book);
        Assert.Equal(4, book?.StoredCopies);
        Assert.Equal(4, book?.AvailableCopies);
    }

    [Fact]
    public void AddBook_WhenEditionSizeMismatchAtApproval_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 5, 2);
        service.ApproveRequest(LibrarianName, 1);

        LibraryRequest request2 = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 2);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.ApproveRequest(LibrarianName, request2.Id));
    }

    [Fact]
    public void BorrowBook_ShouldDecreaseAvailableCopies()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 2, 2);
        service.ApproveRequest(LibrarianName, 1);

        LibraryRequest borrowRequest = service.CreateBorrowBookRequest(ReaderName, BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, borrowRequest.Id);

        var identifier = new BookIdentifier(BookTitle, WriterName);
        Book? book = service.FindBook(identifier);

        Assert.NotNull(book);
        Assert.Equal(1, book?.AvailableCopies);
    }

    [Fact]
    public void BorrowBook_WhenAlreadyBorrowed_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 2, 2);
        service.ApproveRequest(LibrarianName, 1);

        LibraryRequest borrowRequest = service.CreateBorrowBookRequest(ReaderName, BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, borrowRequest.Id);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateBorrowBookRequest(ReaderName, BookTitle, WriterName));
    }

    [Fact]
    public void BorrowBook_WhenNoAvailableCopies_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 1, 1);
        service.ApproveRequest(LibrarianName, 1);

        LibraryRequest firstBorrow = service.CreateBorrowBookRequest(ReaderName, BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, firstBorrow.Id);

        LibraryRequest secondBorrow = service.CreateBorrowBookRequest("SecondReader", BookTitle, WriterName);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.ApproveRequest(LibrarianName, secondBorrow.Id));
    }

    [Fact]
    public void BorrowBook_WhenReaderIsLibrarian_ShouldAutoApprove()
    {
        LibraryService service = CreateService();
        service.RegisterUser(LibrarianName, new[] { UserRole.Librarian });
        service.RegisterUser(WriterName, new[] { UserRole.Writer });
        service.RegisterUser("ReaderLibrarian", new[] { UserRole.Reader, UserRole.Librarian });

        LibraryRequest addRequest = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 2, 1);
        service.ApproveRequest(LibrarianName, addRequest.Id);

        LibraryRequest borrowRequest = service.CreateBorrowBookRequest("ReaderLibrarian", BookTitle, WriterName);

        Assert.Equal(RequestStatus.Approved, borrowRequest.Status);
        var identifier = new BookIdentifier(BookTitle, WriterName);
        Book? book = service.FindBook(identifier);

        Assert.NotNull(book);
        Assert.Equal(0, book?.AvailableCopies);
    }

    [Fact]
    public void CreateBorrowBookRequest_WhenUserIsNotReader_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateBorrowBookRequest(WriterName, BookTitle, WriterName));
    }

    [Fact]
    public void CreateBorrowBookRequest_WhenUserNotFound_ShouldThrow()
    {
        LibraryService service = CreateService();

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateBorrowBookRequest("NoSuch", BookTitle, WriterName));
    }

    [Fact]
    public void CreateBorrowBookRequest_WhenBookNotInLibrary_ShouldCreatePendingRequest()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        LibraryRequest request = service.CreateBorrowBookRequest(ReaderName, BookTitle, WriterName);

        Assert.Equal(RequestStatus.Pending, request.Status);
    }

    [Fact]
    public void ApproveBorrowRequest_WhenBookNotInLibrary_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        LibraryRequest borrowRequest = service.CreateBorrowBookRequest(ReaderName, BookTitle, WriterName);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.ApproveRequest(LibrarianName, borrowRequest.Id));
    }

    [Fact]
    public void CreateBorrowBookRequest_WhenReaderLibrarianAndBookMissing_ShouldThrowAndCleanupRequest()
    {
        LibraryService service = CreateService();
        service.RegisterUser("ReaderLibrarian", new[] { UserRole.Reader, UserRole.Librarian });

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateBorrowBookRequest("ReaderLibrarian", BookTitle, WriterName));

        LibraryRequest? found = service.FindRequest(1);
        Assert.Null(found);
    }

    [Fact]
    public void ReturnBook_ShouldIncreaseAvailableCopies()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 1, 1);
        service.ApproveRequest(LibrarianName, 1);

        LibraryRequest borrowRequest = service.CreateBorrowBookRequest(ReaderName, BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, borrowRequest.Id);

        LibraryRequest returnRequest = service.CreateReturnBookRequest(ReaderName, BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, returnRequest.Id);

        var identifier = new BookIdentifier(BookTitle, WriterName);
        Book? book = service.FindBook(identifier);

        Assert.NotNull(book);
        Assert.Equal(1, book?.AvailableCopies);
    }

    [Fact]
    public void ReturnBook_WhenNotBorrowed_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 1, 1);
        service.ApproveRequest(LibrarianName, 1);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateReturnBookRequest(ReaderName, BookTitle, WriterName));
    }

    [Fact]
    public void ReturnBook_WhenReaderIsLibrarian_ShouldAutoApprove()
    {
        LibraryService service = CreateService();
        service.RegisterUser(LibrarianName, new[] { UserRole.Librarian });
        service.RegisterUser(WriterName, new[] { UserRole.Writer });
        service.RegisterUser("ReaderLibrarian", new[] { UserRole.Reader, UserRole.Librarian });

        LibraryRequest addRequest = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 2, 1);
        service.ApproveRequest(LibrarianName, addRequest.Id);

        service.CreateBorrowBookRequest("ReaderLibrarian", BookTitle, WriterName);

        LibraryRequest returnRequest = service.CreateReturnBookRequest("ReaderLibrarian", BookTitle, WriterName);

        Assert.Equal(RequestStatus.Approved, returnRequest.Status);
        var identifier = new BookIdentifier(BookTitle, WriterName);
        Book? book = service.FindBook(identifier);
        Assert.Equal(1, book?.AvailableCopies);
    }

    [Fact]
    public void CreateReturnBookRequest_WhenUserIsNotReader_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateReturnBookRequest(WriterName, BookTitle, WriterName));
    }

    [Fact]
    public void CreateReturnBookRequest_WhenUserNotFound_ShouldThrow()
    {
        LibraryService service = CreateService();

        Assert.Throws<BusinessRuleViolationException>(
            () => service.CreateReturnBookRequest("NoSuch", BookTitle, WriterName));
    }

    [Fact]
    public void RejectRequest_ShouldChangeStatusToRejected()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        LibraryRequest request = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 2);

        LibraryRequest rejected = service.RejectRequest(LibrarianName, request.Id);

        Assert.Equal(RequestStatus.Rejected, rejected.Status);
    }

    [Fact]
    public void RejectRequest_WhenUserIsNotLibrarian_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        LibraryRequest request = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 2);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.RejectRequest(WriterName, request.Id));
    }

    [Fact]
    public void RejectRequest_WhenRequestNotFound_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => service.RejectRequest(LibrarianName, 999));
    }

    [Fact]
    public void RejectRequest_WhenAlreadyRejected_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        LibraryRequest request = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 2);
        service.RejectRequest(LibrarianName, request.Id);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.RejectRequest(LibrarianName, request.Id));
    }

    [Fact]
    public void RejectAddRequest_ShouldNotAddBook()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        LibraryRequest request = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 2);
        service.RejectRequest(LibrarianName, request.Id);

        var identifier = new BookIdentifier(BookTitle, WriterName);
        Book? book = service.FindBook(identifier);
        Assert.Null(book);
    }

    [Fact]
    public void ApproveRequest_WhenUserIsNotLibrarian_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        LibraryRequest request = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 2);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.ApproveRequest(WriterName, request.Id));
    }

    [Fact]
    public void ApproveRequest_WhenRequestNotFound_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();

        Assert.Throws<BusinessRuleViolationException>(
            () => service.ApproveRequest(LibrarianName, 999));
    }

    [Fact]
    public void ApproveRequest_WhenAlreadyApproved_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        LibraryRequest request = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 2);
        service.ApproveRequest(LibrarianName, request.Id);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.ApproveRequest(LibrarianName, request.Id));
    }

    [Fact]
    public void ApproveRequest_WhenAlreadyRejected_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        LibraryRequest request = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 2);
        service.RejectRequest(LibrarianName, request.Id);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.ApproveRequest(LibrarianName, request.Id));
    }

    [Fact]
    public void ApproveRequest_WhenUserNotFound_ShouldThrow()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        LibraryRequest request = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 2);

        Assert.Throws<BusinessRuleViolationException>(
            () => service.ApproveRequest("NoSuch", request.Id));
    }

    [Fact]
    public void FindRequest_WhenRequestExists_ShouldReturnRequest()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        LibraryRequest request = service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 3, 2);

        LibraryRequest? found = service.FindRequest(request.Id);

        Assert.NotNull(found);
        Assert.Equal(request.Id, found?.Id);
    }

    [Fact]
    public void FindRequest_WhenRequestNotExists_ShouldReturnNull()
    {
        LibraryService service = CreateService();

        LibraryRequest? found = service.FindRequest(999);

        Assert.Null(found);
    }

    [Fact]
    public void FindBook_WhenBookNotExists_ShouldReturnNull()
    {
        LibraryService service = CreateService();

        Book? found = service.FindBook(new BookIdentifier("NonExistent", "Nobody"));

        Assert.Null(found);
    }

    [Fact]
    public void MultipleCopies_TwoReadersBorrowAndReturn_ShouldWork()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 2, 2);
        service.ApproveRequest(LibrarianName, 1);

        LibraryRequest borrow1 = service.CreateBorrowBookRequest(ReaderName, BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, borrow1.Id);

        LibraryRequest borrow2 = service.CreateBorrowBookRequest("SecondReader", BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, borrow2.Id);

        var identifier = new BookIdentifier(BookTitle, WriterName);
        Book? book = service.FindBook(identifier);
        Assert.Equal(0, book?.AvailableCopies);

        LibraryRequest return1 = service.CreateReturnBookRequest(ReaderName, BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, return1.Id);

        Assert.Equal(1, book?.AvailableCopies);
    }

    [Fact]
    public void WriterAndReader_CanCreateBothRequestTypes()
    {
        LibraryService service = CreateService();
        service.RegisterUser(LibrarianName, new[] { UserRole.Librarian });
        service.RegisterUser("WriterReader", new[] { UserRole.Writer, UserRole.Reader });

        LibraryRequest addRequest = service.CreateAddBookRequest("WriterReader", BookTitle, "WriterReader", 3, 2);
        service.ApproveRequest(LibrarianName, addRequest.Id);

        LibraryRequest borrowRequest = service.CreateBorrowBookRequest("WriterReader", BookTitle, "WriterReader");
        service.ApproveRequest(LibrarianName, borrowRequest.Id);

        Assert.Equal(RequestStatus.Approved, borrowRequest.Status);
    }

    [Fact]
    public void AllThreeRoles_ShouldAutoApproveAllRequestTypes()
    {
        LibraryService service = CreateService();
        service.RegisterUser("AllRoles", new[] { UserRole.Writer, UserRole.Reader, UserRole.Librarian });

        LibraryRequest addRequest = service.CreateAddBookRequest("AllRoles", BookTitle, "AllRoles", 3, 2);
        Assert.Equal(RequestStatus.Approved, addRequest.Status);

        LibraryRequest borrowRequest = service.CreateBorrowBookRequest("AllRoles", BookTitle, "AllRoles");
        Assert.Equal(RequestStatus.Approved, borrowRequest.Status);

        LibraryRequest returnRequest = service.CreateReturnBookRequest("AllRoles", BookTitle, "AllRoles");
        Assert.Equal(RequestStatus.Approved, returnRequest.Status);
    }

    [Fact]
    public void BorrowAfterReturn_ShouldSucceed()
    {
        LibraryService service = CreateServiceWithBasicUsers();
        service.CreateAddBookRequest(WriterName, BookTitle, WriterName, 1, 1);
        service.ApproveRequest(LibrarianName, 1);

        LibraryRequest borrow = service.CreateBorrowBookRequest(ReaderName, BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, borrow.Id);

        LibraryRequest returnReq = service.CreateReturnBookRequest(ReaderName, BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, returnReq.Id);

        LibraryRequest secondBorrow = service.CreateBorrowBookRequest(ReaderName, BookTitle, WriterName);
        service.ApproveRequest(LibrarianName, secondBorrow.Id);

        Assert.Equal(RequestStatus.Approved, secondBorrow.Status);
    }

    [Fact]
    public void MultipleBooks_ReaderCanBorrowDifferentBooks()
    {
        LibraryService service = CreateService();
        service.RegisterUser(LibrarianName, new[] { UserRole.Librarian });
        service.RegisterUser(WriterName, new[] { UserRole.Writer });
        service.RegisterUser(ReaderName, new[] { UserRole.Reader });

        LibraryRequest add1 = service.CreateAddBookRequest(WriterName, "Book A", WriterName, 1, 1);
        service.ApproveRequest(LibrarianName, add1.Id);

        LibraryRequest add2 = service.CreateAddBookRequest(WriterName, "Book B", WriterName, 1, 1);
        service.ApproveRequest(LibrarianName, add2.Id);

        LibraryRequest borrow1 = service.CreateBorrowBookRequest(ReaderName, "Book A", WriterName);
        service.ApproveRequest(LibrarianName, borrow1.Id);

        LibraryRequest borrow2 = service.CreateBorrowBookRequest(ReaderName, "Book B", WriterName);
        service.ApproveRequest(LibrarianName, borrow2.Id);

        Assert.Equal(RequestStatus.Approved, borrow1.Status);
        Assert.Equal(RequestStatus.Approved, borrow2.Status);
    }

    private static LibraryService CreateServiceWithBasicUsers()
    {
        LibraryService service = CreateService();
        service.RegisterUser(LibrarianName, new[] { UserRole.Librarian });
        service.RegisterUser(WriterName, new[] { UserRole.Writer });
        service.RegisterUser(ReaderName, new[] { UserRole.Reader });
        service.RegisterUser("SecondReader", new[] { UserRole.Reader });
        return service;
    }

    private static LibraryService CreateService()
    {
        return new LibraryService(
            new InMemoryUserRepository(),
            new InMemoryBookRepository(),
            new InMemoryRequestRepository());
    }
}
