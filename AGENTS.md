# AGENTS.md

## Project summary

- .NET 10.0 MCP server for Yandex Search API v2 — feature-complete port.
- Single-project solution (`yandex-search-mcp-dotnet.slnx` — XML-based format).
- Supports `stdio` and `http` MCP transports.

## Build & run

```bash
dotnet build                          # debug build
dotnet publish -c Release             # AOT native compilation (bin/Release/net10.0/win-x64/publish/)

# stdio mode (default)
dotnet run -- --api-key <key> --folder-id <id>

# http mode
dotnet run -- --api-key <key> --folder-id <id> --transport http --port 3001
```

## Quirks

- **AOT only**: `<PublishAot>true</PublishAot>` + `<InvariantGlobalization>true</InvariantGlobalization>`. Do not introduce culture-sensitive APIs or reflection-based libraries.
- JSON serialization uses source-generated `JsonSerializerContext` (`Serialization/SearchJsonContext.cs`) — always register new `[JsonSerializable]` types there.
- XML parsing uses `XmlReader` (streaming, AOT-safe). Do not use `XDocument`/`XmlSerializer` (reflection).
- **.NET 10.0** (`net10.0`) — verify tooling supports it (SDK 10.0+).
- Solution uses `.slnx` (XML-based), not the traditional `.sln` format. Use `dotnet` CLI, not Visual Studio-specific tooling.

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `AngleSharp` | 1.5.1 | HTML parsing, CSS selectors, DOM manipulation |
| `ModelContextProtocol` | 1.4.0 | MCP server SDK (stdio tools, DI) |
| `ModelContextProtocol.AspNetCore` | 1.4.0 | HTTP transport (Streamable HTTP) |
| `Microsoft.Extensions.Hosting` | 10.0.9 | Generic host (stdio mode) |
| `Microsoft.Extensions.Http` | 10.0.4 | Typed HttpClient via `AddHttpClient<T>()` |
| `Microsoft.AspNetCore.App` | — | Framework reference for HTTP mode |

## Project structure

```
├── Program.cs                          # CLI parsing, transport branching, DI setup
├── Models/
│   └── SearchModels.cs                 # Records: SearchResponse, DocumentResult, YandexConfig, API DTOs
├── Services/
│   ├── YandexSearchClient.cs           # HttpClient → POST Yandex API, base64 decode
│   ├── SearchResponseParser.cs         # XmlReader → XmlDocFields (url, headline, title, passage, extended-text)
│   ├── ContentExtractor.cs             # [GeneratedRegex] StripHighlightTags + PickBestContent priority ranking
│   ├── WebPageFetcher.cs               # HTTP GET → AngleSharp DOM → CSS cleaning → Markdown, rate-limit, proxy fallback
│   └── HtmlToMarkdownConverter.cs      # Static converter: AngleSharp INode → Markdown (headings, links, images, lists, code)
├── Validation/
│   └── InputValidator.cs               # query + search_region presence check
├── Tools/
│   ├── WebSearchTool.cs                # [McpServerToolType] web_search — Yandex Search API
│   ├── FetchTool.cs                    # [McpServerToolType] fetch — web page → markdown
│   └── FetchWithRegexTool.cs           # [McpServerToolType] fetch_with_regex — web page → markdown + regex search
├── Serialization/
│   └── SearchJsonContext.cs            # Source-gen JsonSerializerContext (CamelCase, AOT)
├── start-http.ps1                      # PowerShell: port check, kill old instance, build + run HTTP (0.0.0.0:5883)
├── start-http.bat                      # Launcher: pwsh -File start-http.ps1
```

## Flow

```
CLI args → ConfigurationBuilder.AddCommandLine → YandexConfig
  → if transport=http: WebApplication → WithHttpTransport(Stateless=true) → MapMcp()
  → if transport=stdio: Host → WithStdioServerTransport()

web_search tool call:
  InputValidator → YandexSearchClient.SearchAsync (POST, base64 decode)
  → SearchResponseParser.ParseDocuments (XmlReader → List<XmlDocFields>)
  → ContentExtractor.PickBestContent (headline > title > passages > extended-text)
  → JsonSerializer.Serialize (source-gen context)

fetch / fetch_with_regex tool call:
  URL validation → WebPageFetcher.FetchAsync (HTTP GET, User-Agent, rate-limit)
  → on failure + proxy configured → retry via proxy
  → AngleSharp HtmlParser → CSS cleanup (unwanted selectors, main content detection)
  → HtmlToMarkdownConverter.Convert (AngleSharp DOM walk → Markdown)
  → offset/limit slice or regex search → JsonSerializer.Serialize (source-gen context)
```
