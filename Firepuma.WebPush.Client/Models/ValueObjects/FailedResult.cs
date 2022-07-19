namespace Firepuma.WebPush.Client.Models.ValueObjects;

public class FailedResult
{
    public FailedReason Reason { get; set; }
    public string[] Errors { get; set; }

    public FailedResult(FailedReason reason, string[] errors)
    {
        Reason = reason;
        Errors = errors;
    }

    public enum FailedReason
    {
        Unknown,
        InputValidationFailed,
    }
}