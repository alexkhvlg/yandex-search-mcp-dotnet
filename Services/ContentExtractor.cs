using System.Text.RegularExpressions;

namespace yandex_search_mcp_dotnet.Services;

public static partial class ContentExtractor
{
    [GeneratedRegex(@"<hlword>|</hlword>")]
    private static partial Regex HighlightTagRegex();

    public static string StripHighlightTags(string text)
    {
        return HighlightTagRegex().Replace(text, "").Trim();
    }

    public static (string? Content, string? ContentKind) PickBestContent(SearchResponseParser.XmlDocFields fields)
    {
        if (!string.IsNullOrEmpty(fields.Headline))
            return (StripHighlightTags(fields.Headline), "headline");

        if (!string.IsNullOrEmpty(fields.Title))
            return (StripHighlightTags(fields.Title), "title");

        if (fields.Passages.Count > 0)
        {
            var cleaned = fields.Passages
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(StripHighlightTags);
            return (string.Join(" ", cleaned), "passages");
        }

        if (!string.IsNullOrEmpty(fields.ExtendedText))
            return (StripHighlightTags(fields.ExtendedText), "extended-text");

        return (null, null);
    }
}
