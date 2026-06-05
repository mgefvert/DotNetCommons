namespace DotNetCommons.Services;

public static class Errors
{
    public const string JobErrors = "Job Queue Errors";

    public static readonly Error JobAlreadyClosed        = new(JobErrors, "The job has already been closed.");
    public static readonly Error JobAlreadyExists        = new(JobErrors, "The job already exists and cannot be resubmitted.");
    public static readonly Error JobIsNotOwnedByWorker   = new(JobErrors, "The job is currently being processed by another worker.");
    public static readonly Error JobIsProcessing         = new(JobErrors, "The job is currently being processed.");
    public static readonly Error JobNotFound             = new(JobErrors, "The job doesn't exist or has been archived.");

    public static readonly Error JobTypeNotFound         = new(JobErrors, "The given job type doesn't exist.");

    public static readonly Error WorkerAlreadyRegistered = new(JobErrors, "Job worker is already registered.");
    public static readonly Error WorkerNotFound          = new(JobErrors, "Job worker doesn't exist or has been evicted.");
}