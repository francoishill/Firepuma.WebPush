namespace Firepuma.WebPush.Client.Services.Results;

public class AddWebPushDeviceResult
{
    public bool IsSuccessful { get; set; }

    public FailureReason? FailedReason { get; set; }
    public string[] FailedErrors { get; set; }

    private AddWebPushDeviceResult(
        bool isSuccessful,
        FailureReason? failedReason,
        string[] failedErrors)
    {
        IsSuccessful = isSuccessful;
        FailedReason = failedReason;
        FailedErrors = failedErrors;
    }

    public static AddWebPushDeviceResult Success()
    {
        return new AddWebPushDeviceResult(true, null, null);
    }

    public static AddWebPushDeviceResult Failed(FailureReason reason, params string[] errors)
    {
        return new AddWebPushDeviceResult(false, reason, errors);
    }

    public enum FailureReason
    {
        Unknown,
        InputValidationFailed,
    }
}