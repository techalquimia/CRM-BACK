namespace WorkflowApi.Application.Exceptions;

/// <summary>
/// Thrown when evidence image storage (e.g. GCP Storage) fails (billing disabled, permissions, etc.).
/// </summary>
public class EvidenceStorageException : Exception
{
    public EvidenceStorageException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}
