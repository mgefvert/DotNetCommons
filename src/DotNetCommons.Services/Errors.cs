namespace DotNetCommons.Services;

public static class Errors
{
    public static readonly Error JobAlreadyClosed        = new(ErrorCategory.AlreadyCompleted, "The job has already been closed.");
    public static readonly Error JobAlreadyExists        = new(ErrorCategory.AlreadyCompleted, "The job already exists and cannot be resubmitted.");
    public static readonly Error JobIsNotOwnedByWorker   = new(ErrorCategory.Conflict, "The job is currently being processed by another worker.");
    public static readonly Error JobIsProcessing         = new(ErrorCategory.AlreadyCompleted, "The job is currently being processed.");
    public static readonly Error JobNotFound             = new(ErrorCategory.NotFound, "The job doesn't exist or has been archived.");

    public static readonly Error JobTypeNotFound         = new(ErrorCategory.NotFound, "The given job type doesn't exist.");

    public static readonly Error WorkerAlreadyRegistered = new(ErrorCategory.AlreadyCompleted, "Job worker is already registered.");
    public static readonly Error WorkerNotFound          = new(ErrorCategory.NotFound, "Job worker doesn't exist or has been evicted.");
}