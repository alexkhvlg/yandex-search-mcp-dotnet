using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using yandex_search_mcp_dotnet.Models;
using yandex_search_mcp_dotnet.Serialization;
using yandex_search_mcp_dotnet.Services;
using yandex_search_mcp_dotnet.Validation;

namespace yandex_search_mcp_dotnet.Tools;

[McpServerToolType]
public class WebSearchTool(YandexSearchClient searchClient, LogFileWriter logger)
{
    [McpServerTool(Name = "web_search"),
     Description("""
        Use this tool when the user needs to search online and the search query is simple.

        Args:
            query: Required. search query string. Can contain a question and keywords
            search_region: Required. Search region. Valid values: 'ru' - Russian, 'en' - English, 'tr' - Turkish, 'be' - Belarusian, 'kk' - Kazakh, 'uz' - Uzbek, 'uk' - Ukrainian

        Returns:
            array of data and source

        Note:
            Default to Russian localization/region/domain unless query is explicitly non-Russian.
        """)]
    public async Task<string> WebSearch(
        [Description("Required. search query string. Can contain a question and keywords")] string query,
        [Description("Required. Search region. Valid values: 'ru' - Russian, 'en' - English, 'tr' - Turkish, 'be' - Belarusian, 'kk' - Kazakh, 'uz' - Uzbek, 'uk' - Ukrainian")] string search_region,
        CancellationToken cancellationToken)
    {
        var validationError = InputValidator.Validate(query, search_region);
        if (validationError is not null)
        {
            logger.Write("WebSearchTool", query, null, validationError);
            return validationError;
        }

        try
        {
            var xmlContent = await searchClient.SearchAsync(query, search_region, cancellationToken);
            var docFields = SearchResponseParser.ParseDocuments(xmlContent);
            var results = new List<DocumentResult>();

            foreach (var fields in docFields)
            {
                var (content, contentKind) = ContentExtractor.PickBestContent(fields);
                if (content is not null && contentKind is not null)
                {
                    results.Add(new DocumentResult([content, contentKind], fields.Url!));
                }
            }

            var response = new SearchResponse(results.ToArray());
            var json = JsonSerializer.Serialize(response, SearchJsonContext.Default.SearchResponse);
            logger.Write("WebSearchTool", query, json, null);
            return json;
        }
        catch (Exception ex)
        {
            logger.Write("WebSearchTool", query, null, ex.Message);
            throw;
        }
    }
}
