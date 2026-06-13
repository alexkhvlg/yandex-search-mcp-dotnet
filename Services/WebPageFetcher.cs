using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace yandex_search_mcp_dotnet.Services;

public partial class WebPageFetcher
{
    private static DateTime _lastRequestTime = DateTime.MinValue;
    private static readonly object _rateLock = new();
    private readonly HttpClient _httpClient;
    private readonly string? _proxyUrl;

    private static readonly string[] UnwantedSelectors =
    [
        "script", "style", "nav", "header", "footer", "aside",
        ".advertisement", ".ad", ".ads", ".sidebar", ".navigation",
        ".menu", ".navbar", ".header", ".footer", ".social-media",
        ".comments", ".comment-section", ".related-posts", ".share",
        ".popup", ".modal", ".overlay", ".cookie-notice", ".banner",
        "[role=\"banner\"]", "[role=\"navigation\"]", "[role=\"complementary\"]",
        ".breadcrumb", ".pagination", ".tags", ".categories"
    ];

    private static readonly string[] MainSelectors =
    [
        "main", "article", ".main-content", ".content", ".post-content",
        ".entry-content", ".article-content", "#content", "#main",
        ".container .content", ".page-content", ".single-content"
    ];

    private const string UserAgent =
        "Mozilla/5.0 (compatible; MCP-Fetch-As-Markdown/1.0; +https://github.com/modelcontextprotocol/fetch-as-markdown)";

    public WebPageFetcher(HttpClient httpClient, string? proxyUrl)
    {
        _httpClient = httpClient;
        _proxyUrl = proxyUrl;
    }

    public async Task<FetchResult> FetchAsync(string url, bool includeLinks, bool includeImages, int timeoutSec, CancellationToken cancellationToken)
    {
        RateLimit();

        var result = await TryFetch(url, timeoutSec, includeLinks, includeImages, false, cancellationToken);
        if (result.Success)
        {
            return result;
        }

        if (_proxyUrl is null)
        {
            return result;
        }

        RateLimit();
        var proxyResult = await TryFetch(url, timeoutSec, includeLinks, includeImages, true, cancellationToken);
        if (proxyResult.Success)
        {
            return proxyResult;
        }

        return new FetchResult { Success = false, Url = url, Error = "Both direct and proxy requests failed" };
    }

    private async Task<FetchResult> TryFetch(string url, int timeoutSec, bool includeLinks, bool includeImages, bool useProxy, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSec));

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
            request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");

            HttpResponseMessage response;
            if (useProxy)
            {
                using var handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(_proxyUrl),
                    UseProxy = true
                };
                using var proxyClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(timeoutSec) };
                response = await proxyClient.SendAsync(request, cts.Token);
            }
            else
            {
                response = await _httpClient.SendAsync(request, cts.Token);
            }

            response.EnsureSuccessStatusCode();

            var htmlBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var charSet = response.Content.Headers.ContentType?.CharSet;
            Encoding encoding;
            try
            {
                encoding = !string.IsNullOrEmpty(charSet) ? Encoding.GetEncoding(charSet) : Encoding.UTF8;
            }
            catch
            {
                encoding = Encoding.UTF8;
            }
            var html = encoding.GetString(htmlBytes);
            var finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? url;

            var markdown = ParseAndConvert(html, finalUrl, includeLinks, includeImages);

            return new FetchResult
            {
                Success = true,
                Url = url,
                FinalUrl = finalUrl,
                Title = markdown.Title,
                StatusCode = (int)response.StatusCode,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "unknown",
                ContentLength = markdown.Text.Length,
                Markdown = markdown.Text
            };
        }
        catch (OperationCanceledException)
        {
            return new FetchResult { Success = false, Url = url, Error = $"Request timeout after {timeoutSec} seconds" };
        }
        catch (HttpRequestException)
        {
            return new FetchResult { Success = false, Url = url, Error = "Connection error - unable to reach the server" };
        }
        catch (Exception ex)
        {
            return new FetchResult { Success = false, Url = url, Error = $"Request failed - {ex.Message}" };
        }
    }

    private static (string Title, string Text) ParseAndConvert(string html, string finalUrl, bool includeLinks, bool includeImages)
    {
        var parser = new HtmlParser();
        using var doc = parser.ParseDocument(html);

        var title = doc.QuerySelector("title")?.TextContent.Trim() ?? "Untitled";

        foreach (var selector in UnwantedSelectors)
        {
            foreach (var el in doc.QuerySelectorAll(selector))
            {
                el.Remove();
            }
        }

        IElement? mainContent = null;
        foreach (var selector in MainSelectors)
        {
            mainContent = doc.QuerySelector(selector);
            if (mainContent is not null)
            {
                break;
            }
        }

        if (mainContent is null)
        {
            mainContent = doc.Body;
            if (mainContent is not null)
            {
                foreach (var el in mainContent.QuerySelectorAll("header, nav, footer, aside"))
                {
                    el.Remove();
                }
            }
        }

        mainContent ??= doc.DocumentElement;

        foreach (var el in mainContent.QuerySelectorAll("*"))
        {
            var attrs = el.Attributes.ToArray();
            foreach (var attr in attrs)
            {
                var name = attr.Name;
                var keep = (includeLinks && el.LocalName == "a" && name == "href")
                        || (includeImages && el.LocalName == "img" && name is "src" or "alt")
                        || name is "title";
                if (!keep)
                {
                    el.RemoveAttribute(name);
                }
            }
        }

        var text = HtmlToMarkdownConverter.Convert(mainContent, includeLinks, includeImages);
        // Collapse 3+ blank lines
        text = Regex.Replace(text, @"\n\s*\n\s*\n", "\n\n");
        text = text.Trim();

        return (title, text);
    }

    private static void RateLimit()
    {
        lock (_rateLock)
        {
            var elapsed = DateTime.UtcNow - _lastRequestTime;
            var minInterval = TimeSpan.FromSeconds(1);
            if (elapsed < minInterval)
            {
                Thread.Sleep(minInterval - elapsed);
            }

            _lastRequestTime = DateTime.UtcNow;
        }
    }
}
