using System.Text.Json.Serialization;

using FacilisDynamodb.Constants;

namespace FacilisDynamodb.Entities
{
    public class Identity : IIdentity
    {
        [JsonPropertyName(TableConstants.PrimaryKeyName)]
        public string PrimaryKey { get; set; }
        
        [JsonPropertyName(TableConstants.SortKeyName)]
        public string SortKey { get; set; }
    }
}