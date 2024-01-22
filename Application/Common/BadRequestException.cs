namespace Application.Common;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string[] errors) : base(errors[0])
    {
        Errors = errors;
    }

    public string[] Errors { get; set; }
}