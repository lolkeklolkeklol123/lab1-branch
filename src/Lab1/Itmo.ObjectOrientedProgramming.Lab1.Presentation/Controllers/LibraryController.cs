using Itmo.ObjectOrientedProgramming.Lab1.Application.Abstractions;
using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Presentation.Controllers;

public class LibraryController : ILibraryController
{
    private readonly ILibraryService _service;

    public LibraryController(ILibraryService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public User RegisterUser(string name, IEnumerable<UserRole> roles) => _service.RegisterUser(name, roles);

    public LibraryRequest SubmitAddBook(string writerName, string title, string authorName, int editionSize, int quantity)
    {
        return _service.CreateAddBookRequest(writerName, title, authorName, editionSize, quantity);
    }

    public LibraryRequest SubmitBorrowBook(string readerName, string title, string authorName)
    {
        return _service.CreateBorrowBookRequest(readerName, title, authorName);
    }

    public LibraryRequest SubmitReturnBook(string readerName, string title, string authorName)
    {
        return _service.CreateReturnBookRequest(readerName, title, authorName);
    }

    public LibraryRequest Approve(string librarianName, int requestId) => _service.ApproveRequest(librarianName, requestId);

    public LibraryRequest Reject(string librarianName, int requestId) => _service.RejectRequest(librarianName, requestId);

    public LibraryRequest? GetRequest(int requestId) => _service.FindRequest(requestId);

    public Book? GetBook(BookIdentifier identifier) => _service.FindBook(identifier);
}
