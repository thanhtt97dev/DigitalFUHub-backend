using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalFUHubApi.Comons
{
	public class JsonSerializerDateTimeConverter : JsonConverter<DateTime?>
	{
		public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (string.IsNullOrEmpty(reader.GetString())) return null;
			return DateTime.ParseExact(reader.GetString() ?? string.Empty, "dd/MM/yyyy HH:mm:ss", null);
		}

		public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
