# Agent Instructions (`AGENTS.md`)

This repository is a monorepo consisting of a .NET MAUI Android Client and ASP.NET Core Whisper API servers, targeting **.NET 10.0**.

---

## 🛠️ Tooling & Code Style Commands

You must restore the local .NET tools before formatting or verifying code.

*   **Restore Local Tools:**
    ```bash
    dotnet tool restore
    ```
*   **Format Code (CSharpier):**
    ```bash
    dotnet csharpier format .
    ```
*   **Verify Formatting (No write):**
    ```bash
    dotnet csharpier check .
    ```
---

## 🏗️ Architecture & Boundaries

The codebase is split into Client and Server subprojects under `SubtitlesApp.sln`:

### Core / Shared
*   `SubtitlesApp.Core` (net10.0): Core domain models, interfaces, extension methods, and result types.
*   `Tests/SubtitlesApp.Core.Tests` (net10.0): NUnit unit tests for domain logic (e.g. `TimeSetTests`).

### Client-side
*   `Client/SubtitlesApp.Infrastructure` (net10.0): SQLite database persistence (`AppData.db3`), mapper profiles, and API clients.
*   `Client/SubtitlesApp.MAUI` (net10.0-android): **Android ONLY**. All other platforms are unsupported or deactivated in the csproj.

### Server-side
*   `Server/SubtitlesServer.WhisperApi` (net10.0): ASP.NET Core API server for Whisper speech-to-text / NLP transcription.
*   `Server/SubtitlesServer.Shared` (net10.0): Shared server utility methods.
*   `Tests/OpenAiMockServer` (net10.0): Minimal ASP.NET Core API mocking OpenAI transcribe/translate endpoints for sandbox testing.

---

## ⚠️ High-Signal Quirks & Gotchas

*   **FFmpeg Android Native Dependency:**
    Audio extraction inside `SubtitlesApp.Infrastructure` utilizes `[LibraryImport("ffmpegwrapper")]` via P/Invoke. The native binary `.so` libraries (`libffmpegwrapper.so`, `libavcodec.so`, etc.) are precompiled and placed inside `Client/SubtitlesApp.MAUI/Platforms/Android/lib/<arch>/` directories. Do not modify or delete these `.so` binaries.
*   **Source Generators:**
    `Riok.Mapperly` is used for source-generated mapping profiles.
*   **AutoMapper Vulnerability Warning:**
    `Server/SubtitlesServer.Shared` uses `AutoMapper 13.0.1` which throws a known NuGet vulnerability warning (`NU1903`). Do not attempt to upgrade or modify it unless explicitly instructed.
