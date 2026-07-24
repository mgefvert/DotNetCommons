using System.Text.Json.Serialization;

namespace DotNetCommons.AI;

public class OllamaEmbeddingRequest
{
    [JsonPropertyName("model")] public string? Model { get; set; }
    [JsonPropertyName("input")] public string[]? Input { get; set; }
}

public class OllamaEmbeddingResponse
{
    public FloatVector[] Embeddings { get; set; } = [];
}
