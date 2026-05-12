namespace HireConnect.Job.Middleware
{
    /// <summary>Thrown when a requested resource cannot be found.</summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>Thrown when input validation fails inside the service layer.</summary>
    public class JobValidationException : Exception
    {
        public IEnumerable<string> Errors { get; }

        public JobValidationException(string message) : base(message)
        {
            Errors = new[] { message };
        }

        public JobValidationException(IEnumerable<string> errors)
            : base(string.Join("; ", errors))
        {
            Errors = errors;
        }
    }

    /// <summary>Thrown when an operation is not permitted for the current user.</summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }

    /// <summary>Thrown when Elasticsearch indexing fails.</summary>
    public class ElasticsearchException : Exception
    {
        public ElasticsearchException(string message) : base(message) { }
        public ElasticsearchException(string message, Exception inner) : base(message, inner) { }
    }
}
