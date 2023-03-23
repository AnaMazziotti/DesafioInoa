using System.Text.Json.Serialization;


namespace APIteste2
{
    public class ExchangeRate
    {
        [JsonPropertyName("USDBRL")]
        public Currency USDBRL { get; set; }

        [JsonPropertyName("EURBRL")]
        public Currency EURBRL { get; set; }

        [JsonPropertyName("BTCBRL")]
        public Currency BTCBRL { get; set; }
    }
}
