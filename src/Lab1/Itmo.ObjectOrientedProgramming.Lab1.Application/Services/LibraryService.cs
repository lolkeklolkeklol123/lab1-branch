using Itmo.ObjectOrientedProgramming.Lab1.Application.Abstractions;
using Itmo.ObjectOrientedProgramming.Lab1.Exceptions;
using Itmo.ObjectOrientedProgramming.Lab1.Infrastructure.Abstractions.Repositories;
using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Application.Services;

public class LibraryService : ILibraryService
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IRequestRepository _requestRepository;
    private int _nextRequestId = 1;

    public LibraryService(IUserRepository userRepository, IBookRepository bookRepository, IRequestRepository requestRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
    }

    public User RegisterUser(string name, IEnumerable<UserRole> roles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (_userRepository.Exists(name))
            throw new BusinessRuleViolationException("User with this name already exists.");

        var user = new User(name, roles);
        _userRepository.Add(user);

        return user;
    }

    public LibraryRequest CreateAddBookRequest(
        string writerName,
        string title,
        string authorName,
        int editionSize,
        int quantity)
    {
        User writer = GetUser(writerName);
        EnsureRole(writer, UserRole.Writer);

        if (!string.Equals(writerName, authorName, StringComparison.Ordinal))
            throw new BusinessRuleViolationException("Writer can add only their own books.");

        if (editionSize <= 0)
            throw new ArgumentException("Edition size must be positive.", nameof(editionSize));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        if (quantity > editionSize)
            throw new BusinessRuleViolationException("Quantity cannot exceed edition size.");

        var identifier = new BookIdentifier(title, authorName);
        EnsureCanCreateAddBookRequest(identifier, editionSize, quantity);

        LibraryRequest request = CreateRequest(RequestType.AddBook, writerName, identifier, quantity, editionSize);
        return writer.HasRole(UserRole.Librarian) ? ApproveAutomatically(request) : request;
    }

    public LibraryRequest CreateBorrowBookRequest(string readerName, string title, string authorName)
    {
        User reader = GetUser(readerName);
        EnsureRole(reader, UserRole.Reader);

        var identifier = new BookIdentifier(title, authorName);

        if (reader.HasBorrowed(identifier))
            throw new BusinessRuleViolationException("Reader has already borrowed this book.");

        LibraryRequest request = CreateRequest(RequestType.BorrowBook, readerName, identifier, 1, 0);
        return reader.HasRole(UserRole.Librarian) ? ApproveAutomatically(request) : request;
    }

    public LibraryRequest CreateReturnBookRequest(string readerName, string title, string authorName)
    {
        User reader = GetUser(readerName);
        EnsureRole(reader, UserRole.Reader);

        var identifier = new BookIdentifier(title, authorName);

        if (!reader.HasBorrowed(identifier))
            throw new BusinessRuleViolationException("Reader has not borrowed this book.");

        LibraryRequest request = CreateRequest(RequestType.ReturnBook, readerName, identifier, 1, 0);
        return reader.HasRole(UserRole.Librarian) ? ApproveAutomatically(request) : request;
    }

    public LibraryRequest ApproveRequest(string librarianName, int requestId)
    {
        User librarian = GetUser(librarianName);
        EnsureRole(librarian, UserRole.Librarian);

        LibraryRequest request = GetRequest(requestId);
        EnsurePendingRequest(request);
        ApplyRequest(request);
        request.Approve();

        return request;
    }

    public LibraryRequest RejectRequest(string librarianName, int requestId)
    {
        User librarian = GetUser(librarianName);
        EnsureRole(librarian, UserRole.Librarian);

        LibraryRequest request = GetRequest(requestId);
        EnsurePendingRequest(request);
        request.Reject();

        return request;
    }

    public LibraryRequest? FindRequest(int requestId) => _requestRepository.Find(requestId);

    public Book? FindBook(BookIdentifier identifier) => _bookRepository.Find(identifier);

    private LibraryRequest CreateRequest(
        RequestType type,
        string creatorName,
        BookIdentifier identifier,
        int quantity,
        int editionSize)
    {
        var request = new LibraryRequest(_nextRequestId, type, creatorName, identifier, quantity, editionSize);
        _nextRequestId++;
        _requestRepository.Add(request);

        return request;
    }

    private LibraryRequest ApproveAutomatically(LibraryRequest request)
    {
        try
        {
            ApplyRequest(request);
            request.Approve();
            return request;
        }
        catch
        {
            _requestRepository.Remove(request.Id);
            throw;
        }
    }

    private void ApplyRequest(LibraryRequest request)
    {
        if (request.Type == RequestType.AddBook)
        {
            ApplyAddBookRequest(request);
            return;
        }

        if (request.Type == RequestType.BorrowBook)
        {
            ApplyBorrowBookRequest(request);
            return;
        }

        if (request.Type == RequestType.ReturnBook)
        {
            ApplyReturnBookRequest(request);
            return;
        }

        throw new BusinessRuleViolationException("Request type is invalid.");
    }

    private void ApplyAddBookRequest(LibraryRequest request)
    {
        Book? book = _bookRepository.Find(request.Book);

        if (book is null)
        {
            var createdBook = new Book(request.Book, request.EditionSize, 0);
            createdBook.AddCopies(request.Quantity);
            _bookRepository.Add(createdBook);
            return;
        }

        if (book.EditionSize != request.EditionSize)
            throw new BusinessRuleViolationException("Edition size does not match existing book.");

        book.AddCopies(request.Quantity);
    }

    private void ApplyBorrowBookRequest(LibraryRequest request)
    {
        User reader = GetUser(request.CreatorName);

        if (reader.HasBorrowed(request.Book))
            throw new BusinessRuleViolationException("Reader has already borrowed this book.");

        Book book = GetBook(request.Book);
        book.BorrowCopy();
        reader.Borrow(request.Book);
    }

    private void ApplyReturnBookRequest(LibraryRequest request)
    {
        User reader = GetUser(request.CreatorName);

        if (!reader.HasBorrowed(request.Book))
            throw new BusinessRuleViolationException("Reader has not borrowed this book.");

        Book book = GetBook(request.Book);
        book.ReturnCopy();
        reader.Return(request.Book);
    }

    private void EnsureCanCreateAddBookRequest(BookIdentifier identifier, int editionSize, int quantity)
    {
        Book? book = _bookRepository.Find(identifier);

        if (book is null)
            return;

        if (book.EditionSize == editionSize && book.StoredCopies + quantity > editionSize)
            throw new BusinessRuleViolationException("Stored copies count cannot exceed edition size.");
    }

    private User GetUser(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("User was not found.");

        User? user = _userRepository.Find(name);
        return user ?? throw new BusinessRuleViolationException("User was not found.");
    }

    private Book GetBook(BookIdentifier identifier)
    {
        Book? book = _bookRepository.Find(identifier);
        return book ?? throw new BusinessRuleViolationException("Book was not found.");
    }

    private LibraryRequest GetRequest(int requestId)
    {
        LibraryRequest? request = _requestRepository.Find(requestId);
        return request ?? throw new BusinessRuleViolationException("Request was not found.");
    }

    private void EnsureRole(User user, UserRole role)
    {
        if (!user.HasRole(role))
            throw new BusinessRuleViolationException("User does not have the required role.");
    }

    private void EnsurePendingRequest(LibraryRequest request)
    {
        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleViolationException("Only pending requests can change status.");
    }
}
