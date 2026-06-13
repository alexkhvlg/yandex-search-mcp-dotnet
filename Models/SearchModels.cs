namespace yandex_search_mcp_dotnet.Models;

public record DocumentResult(string[] Data, string Source);

public record SearchResponse(DocumentResult[] Responses);

public record YandexConfig(string ApiKey, string FolderId);

public record WebSearchApiRequest(
    WebSearchApiQuery Query,
    string FolderId,
    WebSearchApiGroupSpec GroupSpec,
    string L10n,
    string Region,
    string ResponseFormat);

public record WebSearchApiQuery(
    string SearchType,
    string QueryText,
    string FamilyMode,
    string FixTypoMode);

public record WebSearchApiGroupSpec(int GroupsOnPage);

public record WebSearchApiResponse(string RawData);

public record FetchResponse(
    string Url,
    string FinalUrl,
    string Title,
    int StatusCode,
    string ContentType,
    int ContentLength,
    string Markdown);

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
