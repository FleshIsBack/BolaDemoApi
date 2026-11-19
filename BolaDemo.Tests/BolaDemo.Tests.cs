using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
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

    [Fact]
    public async Task SecureEndpoint_TamperedTokenReturnsUnauthorized()
    {
        // Arrange: log in as user1, then change the user id claim in the token from 1 to 2 (keeping the old signature)
        var user1 = await LoginAs("user1", "password1");
        var parts = user1.DefaultRequestHeaders.Authorization!.Parameter!.Split('.'); // header.payload.signature
        var payload = JsonNode.Parse(Base64Url.DecodeFromChars(parts[1]))!.AsObject();
        var idClaim = payload.Select(p => p.Key).Single(k => k.Contains("nameid", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("1", payload[idClaim]!.ToString());
        payload[idClaim] = "2";
        var tampered = $"{parts[0]}.{Base64Url.EncodeToString(Encoding.UTF8.GetBytes(payload.ToJsonString()))}.{parts[2]}";

        var attacker = factory.CreateClient();
        attacker.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tampered);
        const int user2OrderId = 2;

        // Act
        var response = await attacker.GetAsync($"/api/secure/orders/{user2OrderId}");

        // Assert: the signature no longer matches, so the token is rejected
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
