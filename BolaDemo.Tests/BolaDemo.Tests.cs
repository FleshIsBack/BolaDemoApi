using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BolaDemo.Tests;

public class BolaTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
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
    public async Task VulnerableEndpoint_User1CanReadUser2Order()
    {
        var user1 = await LoginAs("user1", "password1");

        const int user2OrderId = 2;

        var response = await user1.GetAsync($"/api/vulnerable/orders/{user2OrderId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SecureEndpoint_User1CannotReadUser2Order()
    {
        var user1 = await LoginAs("user1", "password1");
        const int user2OrderId = 2;
        var response = await user1.GetAsync($"/api/secure/orders/{user2OrderId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SecureEndpoint_User2CannotReadUser1Order()
    {
        var user1 = await LoginAs("user1", "password1");
        const int user1OrderId = 1;
        var response = await user1.GetAsync($"/api/secure/orders/{user1OrderId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    [Fact]
    public async Task SecureEndpoint_UserCanReadOwnOrder()
    {
        var user1 = await LoginAs("user1", "password1");
        const int user2OrderId = 2;
        var response = await user1.GetAsync($"/api/secure/orders/{user2OrderId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task SecureEndpoint_NoTokenReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        const int user1OrderId = 1;
        var response = await client.GetAsync($"/api/secure/orders/{user1OrderId}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
