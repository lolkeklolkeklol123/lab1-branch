using Itmo.ObjectOrientedProgramming.Lab1.Exceptions;

namespace Itmo.ObjectOrientedProgramming.Lab1.Models;

public class Book
{
    public Book(BookIdentifier identifier, int editionSize, int initialCopies)
    {
        if (editionSize <= 0)
            throw new ArgumentException("Edition size must be positive.", nameof(editionSize));

        if (initialCopies < 0)
            throw new ArgumentException("Initial copies count cannot be negative.", nameof(initialCopies));

        if (initialCopies > editionSize)
            throw new BusinessRuleViolationException("Initial copies count cannot exceed edition size.");

        Identifier = identifier;
        EditionSize = editionSize;
        StoredCopies = initialCopies;
        AvailableCopies = initialCopies;
    }

    public BookIdentifier Identifier { get; }

    public int EditionSize { get; }

    public int StoredCopies { get; private set; }

    public int AvailableCopies { get; private set; }

    public void AddCopies(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Copies count must be positive.", nameof(amount));

        if (StoredCopies + amount > EditionSize)
            throw new BusinessRuleViolationException("Stored copies count cannot exceed edition size.");

        StoredCopies += amount;
        AvailableCopies += amount;
    }

    public void BorrowCopy()
    {
        if (AvailableCopies == 0)
            throw new BusinessRuleViolationException("There are no available copies.");

        AvailableCopies--;
    }

    public void ReturnCopy()
    {
        if (AvailableCopies == StoredCopies)
            throw new BusinessRuleViolationException("All stored copies are already available.");

        AvailableCopies++;
    }
}
