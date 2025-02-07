using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Object
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string[] formats = {
            "yyyy-MM-dd",           // 标准日期格式
            "yyyy-MM-ddTHH:mm:ss",  // 标准日期时间格式
            "yyyy-MM-ddTHH:mm:ss.fff", // 含毫秒的日期时间格式
            "yyyy-MM-ddTHH:mm:ss.fffZ", // 含毫秒的日期时间格式
            "yyyy-MM-ddTHH:mm",     // 日期+时分格式
            "yyyy-MM-dd HH:mm:ss",  // 标准日期时间格式
            "yyyy-MM-dd HH:mm:ss.fff", // 含毫秒的日期时间格式
            "yyyy-MM-dd HH:mm",     // 日期+时分格式
            "yyyy/MM/dd",           // 标准日期格式
            "yyyy/MM/ddTHH:mm:ss",  // 标准日期时间格式
            "yyyy/MM/ddTHH:mm:ss.fff", // 含毫秒的日期时间格式
            "yyyy/MM/ddTHH:mm",     // 日期+时分格式
            "yyyy/MM/dd HH:mm:ss",  // 标准日期时间格式
            "yyyy/MM/dd HH:mm:ss.fff", // 含毫秒的日期时间格式
            "yyyy/MM/dd HH:mm",     // 日期+时分格式
    };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (dateString != null)
            {
                foreach (var format in formats)
                {
                    if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    {
                        return date;
                    }
                }
            }

            throw new JsonException("Unable to convert \"" + dateString + "\" to DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss")); // 默认回写为 ISO 8601 格式
        }
    }
}
