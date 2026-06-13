using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;
using yandex_search_mcp_dotnet.Models;
using yandex_search_mcp_dotnet.Serialization;
using yandex_search_mcp_dotnet.Services;

namespace yandex_search_mcp_dotnet.Tools;

[McpServerToolType]
public class FetchWithRegexTool(WebPageFetcher fetcher)
{
    [McpServerTool(Name = "fetch_with_regex"),
     Description("Fetch a web page, convert it to markdown format and then search for regex matches across the entire content. Returns page metadata and only the matching lines.")]
    public async Task<string> FetchWithRegex(
        [Description("URL of the web page")] string url,
        [Description("Regular expression pattern to search for in the markdown content")] string regex,
        [Description("Whether to preserve links in the markdown output (default: true)")] bool include_links = true,
        [Description("Whether to include image references in the markdown output (default: false)")] bool include_images = false,
        [Description("Request timeout in seconds (default: 10)")] int timeout = 10,
        [Description("Maximum number of characters to return in the combined matches output (default: 512)")] int limit = 512,
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

        if (string.IsNullOrWhiteSpace(regex))
        {
            return $"# Regex Error\n\n**Pattern:** (empty)\n**Error:** Regex parameter is required";
        }

        Regex compiled;
        try
        {
            compiled = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromSeconds(5));
        }
        catch (ArgumentException ex)
        {
            return $"# Regex Error\n\n**Pattern:** {regex}\n**Error:** Invalid regex - {ex.Message}";
        }

        timeout = Math.Clamp(timeout, 5, 30);

        var result = await fetcher.FetchAsync(url, include_links, include_images, timeout, cancellationToken);

        if (!result.Success)
        {
            return $"# Fetch Error\n\n**URL:** {result.Url}\n**Error:** {result.Error}";
        }

        var matches = compiled.Matches(result.Markdown);
        var lines = matches.Select(m =>
        {
            var val = m.Value;
            if (m.Groups.Count > 1)
            {
                var groups = new List<string>();
                for (int i = 1; i < m.Groups.Count; i++)
                {
                    if (m.Groups[i].Success && !string.IsNullOrEmpty(m.Groups[i].Value))
                    {
                        groups.Add(m.Groups[i].Value);
                    }
                }

                if (groups.Count > 0)
                {
                    return string.Join("", groups);
                }
            }
            return val;
        }).ToList();

        var combined = string.Join("\n", lines);
        if (combined.Length > limit)
        {
            var lastNewline = combined.LastIndexOf('\n', Math.Min(limit, combined.Length) - 1);
            combined = lastNewline > limit * 0.5
                ? combined[..lastNewline]
                : combined[..limit];
            combined += "\n\n...(truncated, limit exceeded)";
        }

        var response = new FetchWithRegexResponse(
            result.Url,
            result.FinalUrl,
            result.Title,
            result.StatusCode,
            result.ContentType,
            result.ContentLength,
            regex,
            lines.Count,
            combined);

        return JsonSerializer.Serialize(response, SearchJsonContext.Default.FetchWithRegexResponse);
    }
}
