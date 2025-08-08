using System.Text.Json;
using System.Text.Json.Serialization;
using SurfTimer.Shared.Types;

namespace CS2_Surf_NET_API.Shared.JsonConverters
{
    public class ReplayFramesStringConverter : JsonConverter<ReplayFramesString>
    {
        public override ReplayFramesString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            return value != null ? new ReplayFramesString(value) : null;
        }

        public override void Write(Utf8JsonWriter writer, ReplayFramesString value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
