using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using yandex_search_mcp_dotnet.Models;
using yandex_search_mcp_dotnet.Serialization;

namespace yandex_search_mcp_dotnet.Services;

public class YandexSearchClient(HttpClient httpClient, YandexConfig config)
{
    private const string WebSearchUrl = "https://searchapi.api.cloud.yandex.net/v2/web/search";

    public async Task<string> SearchAsync(string query, string searchRegion, CancellationToken cancellationToken)
    {
        var (searchType, l10n) = searchRegion switch
        {
            "ru" => ("SEARCH_TYPE_RU", "LOCALIZATION_RU"),
            "tr" => ("SEARCH_TYPE_TR", "LOCALIZATION_TR"),
            "be" => ("SEARCH_TYPE_BE", "LOCALIZATION_BE"),
            "kk" => ("SEARCH_TYPE_KK", "LOCALIZATION_KK"),
            "uz" => ("SEARCH_TYPE_UZ", "LOCALIZATION_EN"),
            "uk" => ("SEARCH_TYPE_COM", "LOCALIZATION_UK"),
            _ => ("SEARCH_TYPE_COM", "LOCALIZATION_EN"),
        };

        var request = new WebSearchApiRequest(
            new WebSearchApiQuery(
                searchType,
                query,
                "FAMILY_MODE_NONE",
                "FIX_TYPO_MODE_OFF"),
            config.FolderId,
            new WebSearchApiGroupSpec(4),
            l10n,
            searchRegion,
            "FORMAT_XML");

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(request, SearchJsonContext.Default.WebSearchApiRequest);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, WebSearchUrl);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Api-Key", config.ApiKey);
        httpRequest.Content = new ByteArrayContent(jsonBytes);
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var apiResponse = JsonSerializer.Deserialize(responseBytes, SearchJsonContext.Default.WebSearchApiResponse)!;
        var decodedBytes = Convert.FromBase64String(apiResponse.RawData);
        return Encoding.UTF8.GetString(decodedBytes);
    }
}
