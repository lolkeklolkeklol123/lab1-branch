namespace Itmo.ObjectOrientedProgramming.Lab1.Exceptions;

public class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
}
