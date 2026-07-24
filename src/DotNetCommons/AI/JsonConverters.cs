using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetCommons.AI;

public sealed class DoubleVectorConverter : JsonConverter<DoubleVector>
{
    public override DoubleVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new DoubleVector(JsonSerializer.Deserialize<double[]>(ref reader, options)!);
    }

    public override void Write(Utf8JsonWriter writer, DoubleVector value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (double[])value, options);
    }
}

public sealed class FloatVectorConverter : JsonConverter<FloatVector>
{
    public override FloatVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new FloatVector(JsonSerializer.Deserialize<float[]>(ref reader, options)!);
    }

    public override void Write(Utf8JsonWriter writer, FloatVector value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (float[])value, options);
    }
}
