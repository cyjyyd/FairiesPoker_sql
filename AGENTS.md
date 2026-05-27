# Repository Guidelines

## High-Priority Encoding Rule
Treat repository text files as UTF-8 unless file metadata proves otherwise. Before reading or editing Chinese strings, comments, `.resx`, or docs from PowerShell, set UTF-8 output and use explicit encoding, for example `[Console]::OutputEncoding = [System.Text.UTF8Encoding]::UTF8` and `Get-Content -Encoding UTF8`. If text appears garbled, stop and reopen it with the correct encoding before saving.

## Project Structure & Module Organization
`FairiesPoker.sln` contains the main projects. `FairiesPoker/` is the Windows Forms client with legacy game logic, UI forms, `Protocol/`, `Net/`, and bundled resources. `FairiesPoker.MG/` is the MonoGame client, organized into `Core/`, `Screens/`, `GameLogic/`, `Renderers/`, `Network/`, `UI/`, and `Content/`. `FairiesPoker.MG.Tests/` contains xUnit tests for MonoGame logic, screen behavior, and render helpers. `FPServer/` is the TCP game server with `Network/`, `Handlers/`, `Game/`, `Database/`, and `Cache/`.

## Build, Test, and Development Commands
Run commands from the repository root.

```bash
dotnet restore FairiesPoker.sln
dotnet build FairiesPoker.sln
dotnet build FairiesPoker.sln -c Release
dotnet test FairiesPoker.MG.Tests/FairiesPoker.MG.Tests.csproj
dotnet run --project FairiesPoker.MG/FairiesPoker.MG.csproj
dotnet run --project FairiesPoker/FairiesPoker.csproj
dotnet run --project FPServer/FPServer.csproj
```

Use `dotnet restore` on first checkout or after package changes. Windows-targeted projects require Windows APIs; `Directory.Build.props` enables Windows targeting in non-Windows CI containers.

## Coding Style & Naming Conventions
Use standard C# formatting with 4-space indentation. Keep namespaces and folders aligned with the existing layout. Use PascalCase for types, methods, properties, DTOs, and constants; use camelCase for locals and private fields unless an existing file uses another convention. Preserve WinForms `.Designer.cs` generated code and `.resx` resources; edit UI logic in paired `.cs` files. The root `.editorconfig` suppresses CS0618 obsolete-member warnings only.

## Testing Guidelines
The primary automated tests use xUnit in `FairiesPoker.MG.Tests/`. Name new test files after the component under test, such as `ChupaiTests.cs` or `CardLayoutManagerTests.cs`, and use descriptive method names that state expected behavior. Add tests for gameplay rules, network DTO behavior, rendering layout calculations, and regression-prone bug fixes. Run `dotnet test` before a pull request.

## Commit & Pull Request Guidelines
Recent history uses short, imperative summaries, for example `Align MG fight requests with server protocol`, mixed with concise Chinese feature/fix summaries. Keep commits focused and mention the affected area when useful. Pull requests should include a short description, test results, linked issues when applicable, and screenshots or recordings for visible UI changes.

## Security & Configuration Tips
Do not commit local secrets or database credentials. Copy `FPServer/appsettings.example.json` to `FPServer/appsettings.json` for local server settings, then edit host, port, MySQL credentials, and game economy values locally.
