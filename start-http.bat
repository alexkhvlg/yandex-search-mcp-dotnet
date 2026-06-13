@echo off
:: Yandex Search MCP Server — HTTP mode (build + run)
if "%YANDEX_CLOUD_API_KEY%"=="" echo ERROR: YANDEX_CLOUD_API_KEY env var is not set & exit /b 1
if "%YANDEX_CLOUD_FOLDER_ID%"=="" echo ERROR: YANDEX_CLOUD_FOLDER_ID env var is not set & exit /b 1
cd /d "%~dp0"
start "yandex-mcp-http" dotnet run -c Release -- --api-key %YANDEX_CLOUD_API_KEY% --folder-id %YANDEX_CLOUD_FOLDER_ID% --transport http --host 0.0.0.0 --port 5883
echo Build + start on http://0.0.0.0:5883
