
using YarpExample.Gateway.Entity;

namespace YarpExample.Gateway
{
    public class AuthHandler(IHttpClientFactory httpClientFactory) : DelegatingHandler
    {

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage httpResponseMessage = base.Send(request, cancellationToken);

            if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                HttpClient httpClient = httpClientFactory.CreateClient("AuthServer");
                var authServerResponseMessage = await httpClient.PostAsJsonAsync("", new ConnectTokenRequestDto() { });

                if (authServerResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string accessToken = (await authServerResponseMessage.Content.ReadFromJsonAsync<TokenResponseDto>())!.AccessToken;
                    request.Headers.Add($"Bearer", accessToken);
                    return base.Send(request, cancellationToken);
                }
            }
            return httpResponseMessage;
        }
    }
}
