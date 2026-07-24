using System.Net.Http.Json;

namespace DotNetCommons.AI;

public class OllamaClient : IDisposable
{
    private readonly HttpClient _client;

    public static OllamaClient Localhost()
    {
        return new OllamaClient("http://localhost:11434/");
    }

    public OllamaClient(string baseUrl)
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public async Task<FloatVector> GetEmbedding(string model, string text)
    {
        var result = await GetEmbedding(model, [text]);
        return result[0];
    }

    public async Task<FloatVector[]> GetEmbedding(string model, string[] text)
    {
        var response = await _client.PostAsJsonAsync("/api/embed", new OllamaEmbeddingRequest
        {
            Model = model,
            Input = text
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
        return result == null
            ? throw new Exception("Failed to get embedding")
            : result.Embeddings.Select(e => new FloatVector(e)).ToArray();
    }
}