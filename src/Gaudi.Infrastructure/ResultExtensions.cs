using System.Net.Http;

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
}
