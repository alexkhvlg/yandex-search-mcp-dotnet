namespace yandex_search_mcp_dotnet.Models;

public record FetchResponse(
    string Url,
    string FinalUrl,
    string Title,
    int StatusCode,
    string ContentType,
    int ContentLength,
    string Markdown);
