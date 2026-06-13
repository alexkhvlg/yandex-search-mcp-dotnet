using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using yandex_search_mcp_dotnet.Models;
using yandex_search_mcp_dotnet.Serialization;
using yandex_search_mcp_dotnet.Services;

namespace yandex_search_mcp_dotnet.Tools;

[McpServerToolType]
public class FetchTool(WebPageFetcher fetcher)
{
    [McpServerTool(Name = "fetch"), Description("Fetch a web page and convert it to markdown format")]
    public async Task<string> Fetch(
        [Description("URL of the web page")] string url,
        [Description("Whether to preserve links in the markdown output (default: true)")] bool include_links = true,
        [Description("Whether to include image references in the markdown output (default: false)")] bool include_images = false,
        [Description("Request timeout in seconds (default: 10)")] int timeout = 10,
        [Description("Character offset to start returning content from (default: 0)")] int offset = 0,
        [Description("Maximum number of characters to return from the content (default: 512)")] int limit = 512,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "# URL Validation Error\n\n**URL:** (empty)\n**Error:** URL parameter is required";
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return $"# URL Validation Error\n\n**URL:** {url}\n**Error:** Invalid URL format";
        }

        timeout = Math.Clamp(timeout, 5, 30);

        var result = await fetcher.FetchAsync(url, include_links, include_images, timeout, cancellationToken);

        if (!result.Success)
        {
            return $"# Fetch Error\n\n**URL:** {result.Url}\n**Error:** {result.Error}";
        }

        var sliced = result.Markdown;
        if (offset > 0 || sliced.Length > limit)
        {
            if (offset >= sliced.Length)
            {
                sliced = "";
            }
            else
            {
                sliced = sliced[offset..Math.Min(offset + limit, sliced.Length)];
            }
        }

        var response = new FetchResponse(
            result.Url,
            result.FinalUrl,
            result.Title,
            result.StatusCode,
            result.ContentType,
            result.ContentLength,
            sliced);

        return JsonSerializer.Serialize(response, SearchJsonContext.Default.FetchResponse);
    }
}
