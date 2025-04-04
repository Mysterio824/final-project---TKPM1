namespace DevTools.Application.DTOs.Response;

public class ApiResult<T>
{
    private ApiResult(bool succeeded, T result, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Result = result;
        Errors = errors;
    }

    public bool Succeeded { get; set; }

    public T Result { get; set; }

    public IEnumerable<string> Errors { get; set; }

    public static ApiResult<T> Success(T result)
    {
        return new ApiResult<T>(true, result, Array.Empty<string>());
    }

    public static ApiResult<T> Failure(IEnumerable<string> errors)
    {
        return new ApiResult<T>(false, default!, errors);
    }
}
