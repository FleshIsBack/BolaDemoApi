using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BolaDemo.Tests;

public class FallbackTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private record LoginResponse(string Token);

    private async Task<HttpClient> LoginAs(string username, string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
        return client;
    }

    [Fact]
    public async Task ForgottenAuthorize_NoTokenReturnsUnauthorized()
    {
        // FallbackPolicy blocks anonymous access even though [Authorize] is missing
        var client = factory.CreateClient();
        const int user1OrderId = 1;
        var response = await client.GetAsync($"/api/forgotten/orders/{user1OrderId}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ForgottenAuthorize_User1CanStillReadUser2Order()
    {
        // FallbackPolicy only checks authentication: BOLA is still possible
        var user1 = await LoginAs("user1", "password1");
        const int user2OrderId = 2;
        var response = await user1.GetAsync($"/api/forgotten/orders/{user2OrderId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
