# Yandex Search MCP Server (.NET)

MCP (Model Context Protocol) server for Yandex Search API v2, built with .NET 10. Enables AI assistants to perform web searches via the Yandex Search API. AOT native compilation, stdio and HTTP transports.

## Features

- **Web search** via Yandex Search API v2
- Multi-region support: Turkish (`tr`) and English (`en`)
- AOT native compilation (`PublishAot=true`)
- **stdio** transport (default) and **HTTP** (streamable, stateless)
- `<hlword>` highlight tag stripping (Yandex XML markup cleanup)

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

## Dependencies

| Package | Version |
|---------|---------|
| `ModelContextProtocol` | 1.4.0 |
| `ModelContextProtocol.AspNetCore` | 1.4.0 |
| `Microsoft.Extensions.Hosting` | 10.0.9 |
| `Microsoft.Extensions.Http` | 10.0.4 |

## License

Apache License 2.0
