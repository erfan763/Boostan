using System.Diagnostics;

namespace Boostan.Application.Models.ApiResult;

public class ApiResult
{
    public ApiResult(string message = null)
    {
        Message = message;
        RequestId = Guid.Parse(Activity.Current?.TraceId.ToString() ?? Guid.Empty.ToString()).ToString();
    }

    public string Message { get; set; }
    public string RequestId { get; }
}

public class ApiResult<TData> : ApiResult where TData : class
{
    public ApiResult(TData data, string message = null)
        : base(message)
    {
        Data = data;
    }

    public TData Data { get; set; }
}