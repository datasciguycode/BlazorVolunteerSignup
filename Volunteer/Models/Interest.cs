using System.Text.Json.Serialization;

namespace Volunteer.Models;

public class Interest
{
    public long Id { get; set; }

    [JsonPropertyName("interest_type_id")]
    public long? InterestTypeId { get; set; }

    [JsonPropertyName("interest")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("order_by")]
    public int? OrderBy { get; set; }
}
