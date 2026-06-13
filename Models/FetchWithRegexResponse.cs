namespace yandex_search_mcp_dotnet.Models;

public record FetchWithRegexResponse(
    string Url,
    string FinalUrl,
    string Title,
    int StatusCode,
    string ContentType,
    int ContentLength,
    string RegexPattern,
    int MatchesFound,
    string MatchedContent);
