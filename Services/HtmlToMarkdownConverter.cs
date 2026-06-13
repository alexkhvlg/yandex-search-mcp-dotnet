using System.Text;
using AngleSharp.Dom;

namespace yandex_search_mcp_dotnet.Services;

public static class HtmlToMarkdownConverter
{
    public static string Convert(INode node, bool includeLinks = true, bool includeImages = false)
    {
        var sb = new StringBuilder();
        Walk(node, sb, includeLinks, includeImages, 0);
        return sb.ToString().Trim();
    }

    private static void Walk(INode node, StringBuilder sb, bool links, bool images, int indent)
    {
        foreach (var child in node.ChildNodes)
        {
            switch (child)
            {
                case IText text:
                    var t = text.Text;
                    if (sb.Length > 0 && sb[^1] != '\n' && sb[^1] != ' ')
                    {
                        sb.Append(' ');
                    }

                    sb.Append(t);
                    break;

                case IElement el:
                    var tag = el.LocalName.ToLowerInvariant();
                    var childrenBefore = sb.Length;
                    foreach (var c in el.ChildNodes)
                    {
                        Walk(c, sb, links, images, indent);
                    }

                    switch (tag)
                    {
                        case "h1": Wrap(sb, childrenBefore, "\n\n# ", "\n\n"); break;
                        case "h2": Wrap(sb, childrenBefore, "\n\n## ", "\n\n"); break;
                        case "h3": Wrap(sb, childrenBefore, "\n\n### ", "\n\n"); break;
                        case "h4": Wrap(sb, childrenBefore, "\n\n#### ", "\n\n"); break;
                        case "h5": Wrap(sb, childrenBefore, "\n\n##### ", "\n\n"); break;
                        case "h6": Wrap(sb, childrenBefore, "\n\n###### ", "\n\n"); break;
                        case "p": Wrap(sb, childrenBefore, "\n\n", "\n\n"); break;
                        case "br": sb.Append('\n'); break;
                        case "strong": case "b": Wrap(sb, childrenBefore, "**", "**"); break;
                        case "em": case "i": Wrap(sb, childrenBefore, "*", "*"); break;
                        case "code": Wrap(sb, childrenBefore, "`", "`"); break;
                        case "pre": Wrap(sb, childrenBefore, "\n\n```\n", "\n```\n\n"); break;
                        case "a" when links:
                            var href = el.GetAttribute("href");
                            if (!string.IsNullOrEmpty(href))
                            {
                                Wrap(sb, childrenBefore, "[", $"]({href})");
                            }

                            break;
                        case "img" when images:
                            var src = el.GetAttribute("src");
                            var alt = el.GetAttribute("alt") ?? "";
                            if (!string.IsNullOrEmpty(src))
                            {
                                Wrap(sb, childrenBefore, $"![{alt}](", $")");
                            }

                            break;
                        case "blockquote":
                            Wrap(sb, childrenBefore, "\n\n> ", "\n\n");
                            break;
                        case "li":
                            var prefix = IsOrderedList(el.ParentElement) ? "1. " : "- ";
                            Wrap(sb, childrenBefore, $"{new string(' ', indent)}{prefix}", "\n");
                            break;
                        case "ul": case "ol":
                            sb.Append('\n');
                            break;
                        case "hr":
                            sb.Append("\n\n---\n\n");
                            break;
                    }
                    break;
            }
        }
    }

    private static void Wrap(StringBuilder sb, int start, string before, string after)
    {
        var inner = sb.ToString(start, sb.Length - start).Trim();
        sb.Length = start;
        sb.Append(before);
        sb.Append(inner);
        sb.Append(after);
    }

    private static bool IsOrderedList(IElement? el)
    {
        if (el == null)
        {
            return false;
        }

        var tag = el.LocalName.ToLowerInvariant();
        if (tag == "ol")
        {
            return true;
        }

        if (tag == "ul")
        {
            return false;
        }

        return IsOrderedList(el.ParentElement);
    }
}
