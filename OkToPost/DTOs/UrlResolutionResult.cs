using OkToPost.DTOs;

public class UrlResolutionResult
{
    public string? Url { get; set; }
    public string? Code { get; set; }
    public UrlResolutionStatus Status { get; set; }
}


public enum UrlResolutionStatus
{
    Success,
    NotFound,
    DatabaseError,
    UnexpectedError,
    InvalidRequest
}