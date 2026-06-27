using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Application.Abstractions;

public interface ILibraryService
{
    User RegisterUser(string name, IEnumerable<UserRole> roles);

    LibraryRequest CreateAddBookRequest(string writerName, string title, string authorName, int editionSize, int quantity);

    LibraryRequest CreateBorrowBookRequest(string readerName, string title, string authorName);

    LibraryRequest CreateReturnBookRequest(string readerName, string title, string authorName);

    LibraryRequest ApproveRequest(string librarianName, int requestId);

    LibraryRequest RejectRequest(string librarianName, int requestId);

    LibraryRequest? FindRequest(int requestId);

    Book? FindBook(BookIdentifier identifier);
}
