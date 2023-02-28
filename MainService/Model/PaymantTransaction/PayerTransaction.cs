using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Model.PaymantTransaction
{
    public class PayerTransaction
    {
        public string Name { get; set; }
        public decimal Payment { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Date { get; set; }
        public long Account_number { get; set; }
    }
}
public class CustomDateTimeConverter : DateTimeConverterBase
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is DateTime dt)
        {
            if (dt == DateTime.MinValue)
            {
                writer.WriteValue("00-00-1000");
            }
            else
            {
                writer.WriteValue(dt.ToString("dd-MM-yyyy"));
            }
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}