# Yandex Search MCP Server (.NET)

MCP (Model Context Protocol) server for Yandex Search API v2, built with .NET 10. Enables AI assistants to perform web searches via the Yandex Search API. AOT native compilation, stdio and HTTP transports.

## Features

- **Web search** via Yandex Search API v2
- **Web page fetching** — convert any page to Markdown via `fetch` and `fetch_with_regex`
- Multi-region support: Turkish (`tr`) and English (`en`)
- AOT native compilation (`PublishAot=true`)
- **stdio** transport (default) and **HTTP** (streamable, stateless, CORS)
- `<hlword>` highlight tag stripping (Yandex XML markup cleanup)
- Proxy fallback for page fetching (`--proxy-url`)

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Yandex Cloud account with Search API access (API key + Folder ID)

## Quick Start

```bash
# stdio (default)
dotnet run -- --api-key <key> --folder-id <id>

# HTTP
dotnet run -- --api-key <key> --folder-id <id> --transport http --host 0.0.0.0 --port 5883
start-http.bat            # build + run HTTP mode (0.0.0.0:5883)

# AOT compile
dotnet publish -c Release
./bin/Release/net10.0/win-x64/publish/yandex-search-mcp-dotnet.exe --api-key <key> --folder-id <id>
```

## CLI Arguments

| Arg | Required | Default | Description |
|-----|----------|---------|-------------|
| `--api-key` | Yes | — | Yandex Search API key |
| `--folder-id` | Yes | — | Yandex Cloud folder ID |
| `--transport` | No | `stdio` | `stdio` or `http` |
| `--host` | No | `localhost` | Host for HTTP transport |
| `--port` | No | `3001` | Port for HTTP transport |
| `--proxy-url` | No | — | Proxy URL for page fetching fallback (e.g., `http://proxy:8080`) |

`--host` and `--port` only valid with `--transport http`.

## MCP Client Configuration

### stdio (AOT binary)

```json
{
  "mcpServers": {
    "yandex-search": {
      "command": "path/to/yandex-search-mcp-dotnet.exe",
      "args": ["--api-key", "your-key", "--folder-id", "your-folder-id"]
    }
  }
}
```

### HTTP

```json
{
  "mcpServers": {
    "yandex-search": {
      "type": "url",
      "url": "http://localhost:3001"
    }
  }
}
```

Start the server first: `yandex-search-mcp-dotnet.exe --api-key <key> --folder-id <id> --transport http --port 3001`

## Tools

### `web_search`

Performs a web search using Yandex Search API.

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `query` | string | Yes | Search query (keywords or question) |
| `search_region` | string | Yes | `"ru"` — Russian, `"en"` — English |

**Response:**

```json
{
  "responses": [
    {
      "data": ["content text", "title"],
      "source": "https://example.com/page"
    }
  ]
}
```

`data[0]` — content (headline, title, or passage), `data[1]` — content type.

### `fetch`

Fetches a web page and converts it to Markdown.

**Parameters:**

| Name | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `url` | string | Yes | — | URL of the web page |
| `include_links` | bool | No | `true` | Preserve links in Markdown output |
| `include_images` | bool | No | `false` | Include image references |
| `timeout` | int | No | `10` | Request timeout (5–30 sec) |
| `offset` | int | No | `0` | Character offset for content slice |
| `limit` | int | No | `512` | Max returned characters |

### `fetch_with_regex`

Fetches a page, converts to Markdown, searches with regex.

**Parameters:**

| Name | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `url` | string | Yes | — | URL of the web page |
| `regex` | string | Yes | — | Regex pattern (case-insensitive, singleline) |
| `include_links` | bool | No | `true` | Preserve links in Markdown output |
| `include_images` | bool | No | `false` | Include image references |
| `timeout` | int | No | `10` | Request timeout (5–30 sec) |
| `limit` | int | No | `512` | Max returned characters |

## Dependencies

| Package | Version |
|---------|---------|
| `AngleSharp` | 1.5.1 |
| `ModelContextProtocol` | 1.4.0 |
| `ModelContextProtocol.AspNetCore` | 1.4.0 |
| `Microsoft.Extensions.Hosting` | 10.0.9 |
| `Microsoft.Extensions.Http` | 10.0.4 |

## License

Apache License 2.0
