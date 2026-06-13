using System.Xml;

namespace yandex_search_mcp_dotnet.Services;

public static class SearchResponseParser
{
    public record XmlDocFields(string? Url, string? Headline, string? Title, List<string> Passages, string? ExtendedText);

    public static List<XmlDocFields> ParseDocuments(string xmlContent)
    {
        var docFields = new List<XmlDocFields>();

        using var reader = XmlReader.Create(new StringReader(xmlContent), new XmlReaderSettings
        {
            Async = false,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Ignore,
        });

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "doc")
            {
                var doc = ParseSingleDocument(reader);
                if (doc.Url is not null)
                {
                    docFields.Add(doc);
                }
            }
        }

        return docFields;
    }

    private static XmlDocFields ParseSingleDocument(XmlReader reader)
    {
        string? url = null;
        string? headline = null;
        string? title = null;
        var passages = new List<string>();
        string? extendedText = null;

        var contentTags = new HashSet<string> { "url", "headline", "title", "passage", "extended-text" };

        var depth = reader.Depth;
        while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "doc" && reader.Depth == depth))
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            var name = reader.LocalName;
            if (!contentTags.Contains(name) || reader.IsEmptyElement)
            {
                continue;
            }

            var content = reader.ReadInnerXml();
            switch (name)
            {
                case "url": url = content; break;
                case "headline": headline = content; break;
                case "title": title = content; break;
                case "passage": passages.Add(content); break;
                case "extended-text": extendedText = content; break;
            }
        }

        return new XmlDocFields(url, headline, title, passages, extendedText);
    }
}
