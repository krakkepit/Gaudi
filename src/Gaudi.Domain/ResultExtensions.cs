namespace Gaudi.Domain;

public static class ResultExtensions
{
    public static async Task<Result<TOut>> Then<TOut>(this Task<Result> finishedTask, Func<Task<Result<TOut>>> nextTask)
    {
        var result = await finishedTask ?? throw new ArgumentNullException(nameof(finishedTask));
        return result.IsValid ? await nextTask() : Result.WithMessages<TOut>(result.Messages);
    }
    public static async Task<Result> Then<TOut>(this Task<Result<TOut>> finishedTask, Func<TOut, Task<Result>> nextTask)
        => await (finishedTask ?? throw new ArgumentNullException(nameof(finishedTask)))
                .ActAsync(nextTask);

    public static async Task<Result> Then(this Task<Result> response, Task<Result> next)
    {
        var result = await response;
        return result.IsValid ? await next : result;
    }

    public static async Task<Result<T>> ThenReturn<T>(this Task<Result> response, Result<T> next)
    {
        var result = await response;
        return result.IsValid ? next : Result.WithMessages<T>(result.Messages);
    }

    public static async Task<Result<T>> Else<T>(this Task<Result<T>> response, Result<T> ifInvalid)
    {
        var result = await response;
        if (result.IsValid)
        {
            return result;
        }
        if (ifInvalid.IsValid)
        {
            return ifInvalid;
        }
        return Result.WithMessages<T>(result.Messages.Concat(ifInvalid.Messages));
    }

    public static async Task<Result<T>> Else<T>(this Task<Result<T>> response, Task<Result<T>> ifInvalid)
        => await Else(response, await ifInvalid);
}
