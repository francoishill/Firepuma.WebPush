namespace Firepuma.WebPush.Client.Services.Results;

public class EnqueueWebPushResult
{
    public bool IsSuccessful { get; set; }

    public FailureReason? FailedReason { get; set; }
    public string[] FailedErrors { get; set; }

    private EnqueueWebPushResult(
        bool isSuccessful,
        FailureReason? failedReason,
        string[] failedErrors)
    {
        IsSuccessful = isSuccessful;
        FailedReason = failedReason;
        FailedErrors = failedErrors;
    }

    public static EnqueueWebPushResult Success()
    {
        return new EnqueueWebPushResult(true, null, null);
    }

    public static EnqueueWebPushResult Failed(FailureReason reason, params string[] errors)
    {
        return new EnqueueWebPushResult(false, reason, errors);
    }

    public enum FailureReason
    {
        InputValidationFailed,
    }
}