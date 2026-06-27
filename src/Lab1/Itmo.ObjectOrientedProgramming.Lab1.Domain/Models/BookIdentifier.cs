namespace Itmo.ObjectOrientedProgramming.Lab1.Models;

public readonly record struct BookIdentifier
{
    public BookIdentifier(string title, string authorName)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Book title must be provided.", nameof(title));

        if (string.IsNullOrWhiteSpace(authorName))
            throw new ArgumentException("Author name must be provided.", nameof(authorName));

        Title = title;
        AuthorName = authorName;
    }

    public string Title { get; }

    public string AuthorName { get; }
}
