using System.Text.Json.Serialization;
using yandex_search_mcp_dotnet.Models;

namespace yandex_search_mcp_dotnet.Serialization;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(SearchResponse))]
[JsonSerializable(typeof(DocumentResult[]))]
[JsonSerializable(typeof(WebSearchApiRequest))]
[JsonSerializable(typeof(WebSearchApiResponse))]
[JsonSerializable(typeof(FetchResponse))]
[JsonSerializable(typeof(FetchWithRegexResponse))]
public partial class SearchJsonContext : JsonSerializerContext;
