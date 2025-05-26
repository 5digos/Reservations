
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class DateTimeWithoutFractionConverter : JsonConverter<DateTime?>
    {
        private const string Format = "yyyy-MM-dd'T'HH:mm:ss";
        public override DateTime? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.Null
                ? (DateTime?)null
                : DateTime.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            DateTime? value,
            JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString(Format));
            else
                writer.WriteNullValue();
        }
    }
}
