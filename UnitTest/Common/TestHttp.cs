namespace UnitTest.Common;

/// <summary>GET con Uri relativa (CA2234 prohíbe GetAsync(string)).</summary>
public static class TestHttp
{
    public static Task<HttpResponseMessage> GetAsync(HttpClient client, string path)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(path);
        return client.GetAsync(new Uri(path, UriKind.Relative));
    }
}
