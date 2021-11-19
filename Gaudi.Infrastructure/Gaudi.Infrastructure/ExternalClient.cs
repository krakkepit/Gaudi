using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Gaudi.Infrastructure;

public class ExternalClient
{
    private HttpClient Client { get; set; }

    protected ExternalClient(HttpClient client)
        => Client = client ?? throw new ArgumentNullException(nameof(client));

    protected Task Post(
        Uri uri,
        object payload,
        params KeyValuePair<string, string>[] extraHeaders)
        => Send(
            HttpMethod.Post,
            uri,
            payload,
            extraHeaders)
            .GuardSuccessful();

    protected Task Put(
        Uri uri,
        object payload,
        params KeyValuePair<string, string>[] extraHeaders)
        => Send(
            HttpMethod.Put,
            uri,
            payload,
            extraHeaders)
            .GuardSuccessful();

    protected Task Delete(
        Uri uri,
        object payload,
        params KeyValuePair<string, string>[] extraHeaders)
        => Send(
            HttpMethod.Delete,
            uri,
            payload,
            extraHeaders)
            .GuardSuccessful();

    protected Task<Result<TOut>> Post<TOut>(
        Uri uri,
        object payload,
        params KeyValuePair<string, string>[] extraHeaders)
        => Send(
            HttpMethod.Post,
            uri,
            payload,
            extraHeaders)
            .ToModel<TOut>();

    protected Task<Result<TOut>> Get<TOut>(
        Uri uri,
        object payload,
        params KeyValuePair<string, string>[] extraHeaders)
        => Send(
            HttpMethod.Get,
            uri,
            payload,
            extraHeaders)
            .ToModel<TOut>();

    private async Task<Result<HttpResponseMessage>> Send(
        HttpMethod httpMethod,
        Uri uri,
        object payload,
        params KeyValuePair<string, string>[] extraHeaders)
    {
        var httpRequest = new HttpRequestMessage(httpMethod, uri);

        var json = JsonConvert.SerializeObject(payload);
        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (extraHeaders != null)
        {
            foreach (var extraHeader in extraHeaders)
            {
                httpRequest.Headers.Add(extraHeader.Key, extraHeader.Value);
            }
        }

        try
        {
            return await Client.SendAsync(httpRequest);
        }
        catch (Exception ex)
        {
            return Result.WithMessages<HttpResponseMessage>(ValidationMessage.Error($"HttpClient SendAsync {httpMethod.Method} call to {uri} threw an unexpected error: {ex.Message}")); 
        }
    }
}