using Itmo.ObjectOrientedProgramming.Lab1.Models;

namespace Itmo.ObjectOrientedProgramming.Lab1.Presentation.Controllers;

public interface ILibraryController
{
    User RegisterUser(string name, IEnumerable<UserRole> roles);

    LibraryRequest SubmitAddBook(string writerName, string title, string authorName, int editionSize, int quantity);

    LibraryRequest SubmitBorrowBook(string readerName, string title, string authorName);

    LibraryRequest SubmitReturnBook(string readerName, string title, string authorName);

    LibraryRequest Approve(string librarianName, int requestId);

    LibraryRequest Reject(string librarianName, int requestId);

    LibraryRequest? GetRequest(int requestId);

    Book? GetBook(BookIdentifier identifier);
}
