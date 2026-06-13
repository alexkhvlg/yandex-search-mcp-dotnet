namespace yandex_search_mcp_dotnet.Validation;

public static class InputValidator
{
    public static string? Validate(string? query, string? searchRegion)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(searchRegion))
            return "Missing required keys: query, search_region";
        return null;
    }
}
