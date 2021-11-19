using System.Linq;

namespace Gaudi.Infrastructure;

public static class ResultExtensions
{
    public static Task<Result<TOut>> ToModel<TOut>(this Task<Result<HttpResponseMessage>> response)
    => response.ActAsync(async res =>
        {
            if (!res.IsSuccessStatusCode)
            {
                return Result.WithMessages<TOut>(ValidationMessage.Error($"A request to an external client returned an unsuccessful status code:\nSTATUS {res.StatusCode}: {res.ReasonPhrase}"));
            }
            try
            {
                var asTOut = await res.Content.ReadAsAsync<TOut>();
                return Result.For(asTOut);
            }
            catch (Exception ex)
            {
                return Result.WithMessages<TOut>(ValidationMessage.Error($"An exception was thrown while trying to read the content of an HttpResponseMessage as {nameof(TOut)}: {ex.Message}"));
            }
        });

    public static async Task<Result> GuardSuccessful(this Task<Result<HttpResponseMessage>> response)
        => await response.ActAsync(res =>
        {
            return res.IsSuccessStatusCode
            ? Result.OK
            : Result.WithMessages(ValidationMessage.Error($"A request to an external client returned an unsuccessful status code:\nSTATUS {res.StatusCode}: {res.ReasonPhrase}"));
        });

    public static async Task<Result> Then(this Task<Result> response, Task<Result> next)
    {
        var result = await response;
        return (result.IsValid) ? await next : result;
    }

    public static async Task<Result<T>> ThenReturn<T>(this Task<Result> response, Result<T> next)
    {
        var result = await response;
        return (result.IsValid) ? next : Result.WithMessages<T>(result.Messages);
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
