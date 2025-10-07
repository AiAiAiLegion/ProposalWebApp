namespace ProposalWebApp.Shared.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class InvalidStateException : DomainException
    {
        public InvalidStateException(string message) : base(message) { }
    }
}