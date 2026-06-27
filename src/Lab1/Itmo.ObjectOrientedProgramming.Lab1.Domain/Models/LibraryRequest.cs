using Itmo.ObjectOrientedProgramming.Lab1.Exceptions;

namespace Itmo.ObjectOrientedProgramming.Lab1.Models;

public class LibraryRequest
{
    public LibraryRequest(int id, RequestType type, string creatorName, BookIdentifier book, int quantity, int editionSize)
    {
        if (id <= 0)
            throw new ArgumentException("Request id must be positive.", nameof(id));

        if (type == RequestType.Undefined || !Enum.IsDefined(type))
            throw new ArgumentException("Request type is invalid.", nameof(type));

        ArgumentException.ThrowIfNullOrWhiteSpace(creatorName);

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        if (type == RequestType.AddBook && editionSize <= 0)
            throw new ArgumentException("Edition size must be positive.", nameof(editionSize));

        Id = id;
        Type = type;
        CreatorName = creatorName;
        Book = book;
        Quantity = quantity;
        EditionSize = editionSize;
        Status = RequestStatus.Pending;
    }

    public int Id { get; }

    public RequestType Type { get; }

    public string CreatorName { get; }

    public BookIdentifier Book { get; }

    public int Quantity { get; }

    public int EditionSize { get; }

    public RequestStatus Status { get; private set; }

    public void Approve()
    {
        EnsurePending();
        Status = RequestStatus.Approved;
    }

    public void Reject()
    {
        EnsurePending();
        Status = RequestStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (Status != RequestStatus.Pending)
            throw new BusinessRuleViolationException("Only pending requests can change status.");
    }
}
