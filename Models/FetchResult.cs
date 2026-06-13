namespace yandex_search_mcp_dotnet.Services;

public partial class WebPageFetcher
{
    public class FetchResult
    {
        public bool Success { get; init; }
        public string Url { get; init; } = "";
        public string FinalUrl { get; init; } = "";
        public string Title { get; init; } = "";
        public int StatusCode { get; init; }
        public string ContentType { get; init; } = "";
        public int ContentLength { get; init; }
        public string Markdown { get; init; } = "";
        public string? Error { get; init; }
    }
}
