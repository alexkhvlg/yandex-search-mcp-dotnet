using System.Text.Json.Serialization;
using yandex_search_mcp_dotnet.Models;

namespace yandex_search_mcp_dotnet.Serialization;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(SearchResponse))]
[JsonSerializable(typeof(DocumentResult[]))]
[JsonSerializable(typeof(WebSearchApiRequest))]
[JsonSerializable(typeof(WebSearchApiResponse))]
public partial class SearchJsonContext : JsonSerializerContext;
