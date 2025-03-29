using Newtonsoft.Json;

namespace Horai.Mokushiroku.Models
{
    public class EncounterProfile : IHasDropRate
    {
        [JsonProperty("name")]
        public required string Name { get; init; }

        [JsonProperty("category")]
        public required string Category { get; init; }

        [JsonProperty("genre")]
        public required List<HasDropRate> Genre { get; init; }

        [JsonProperty("puissance")]
        public required List<HasDropRate> PowerLevel { get; init; }

        [JsonProperty("Type 1")]
        public required List<HasDropRate> Type1 { get; init; } = new();

        [JsonProperty("Type 2")]
        public required List<HasDropRate> Type2 { get; init; } = new();

        [JsonProperty("description")]
        public required string Description { get; init; }

        [JsonProperty("status")]
        public required List<string> Status { get; init; } = new();

        [JsonProperty("corruption")]
        public required List<HasDropRate> Corruption { get; init; }

        [JsonProperty("group")]
        public required Group Group { get; set; }

        [JsonProperty("drop_rate")]
        public required int DropRate { get; init; }

        public override string ToString()
        {
            return Name;
        }
    }
    public class HasDropRate : IHasDropRate
    {
        [JsonProperty("drop_rate")]
        public required int DropRate { get; init; }

        [JsonProperty("name")]
        public required string Name { get; init; }
    }

    public class Group
    {
        [JsonProperty("min")]
        public required int Min { get; init; }

        [JsonProperty("max")]
        public required int Max { get; init; }
    }

    public interface IHasDropRate
    {
        public int DropRate { get; }
        public string Name { get; }
    }
}
