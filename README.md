# SubtitlesApp

SubtitlesApp is a full-stack .NET application that uses AI tools to generate and translate subtitles for any video.

<img src="https://github.com/user-attachments/assets/c47a804b-c9cf-428e-b13b-6adb7d9b8a91" width="900"/>

## Tech stack
- Client app: .NET MAUI application;
- Backends: ASP.NET Core WebApi applications. 

## SubtitlesApp Client

### Features
Inside the client app, a user can:
- Play local or remote videos;
- See generated subtitles for any video;
- Hide or show subtitles by swiping the player;
- Translate subtitles;
- Hide or show translation for each subtitle by swipe;
- Set languages for subtitles/translations.
- Register or log in to access protected transcription and translation APIs;

Subtitles are shown inside a scrollable list which is automatically synchronized with the current video playback.

To simulate near real-time transcription during video playback, the app extracts portions of the video (60 seconds by default) in the background, converts them to audio, resamples them, and then sends them to the backend.

### Platforms:
 ✅ Android;\
 ❌ iOS.

### Important note about FfmpegAndroidBinding
[FfmpegAndroidBinding](https://github.com/paulnlb/SubtitlesApp/tree/master/Client/FfmpegAndroidBinding) allows to use FfmpegKit API inside the .NET Maui application.

**BUT**, it won't work until you add the following files to the [Jars](https://github.com/paulnlb/SubtitlesApp/tree/master/Client/FfmpegAndroidBinding/Jars) directory:
- [ffmpeg-kit-full-6.0-2.LTS.aar](https://github.com/arthenica/ffmpeg-kit/releases/download/v6.0.LTS/ffmpeg-kit-full-6.0-2.LTS.aar)
- [smart-exception-common-0.2.1.jar](https://github.com/tanersener/smart-exception/releases/download/v0.2.1/smart-exception-common-0.2.1.jar)
- [smart-exception-java-0.2.1.jar](https://github.com/tanersener/smart-exception/releases/download/v0.2.1/smart-exception-java-0.2.1.jar)

After adding these binaries, build the FfmpegAndroidBinding project.

## SubtitlesApp Server

Back-end part is a set of multiple ASP.NET Core web apps. Each of them is responsible for different things:
- **SubtitlesServer.WhisperApi** - API for transcription. Uses OpenAI Whisper under the hood. It can be easily configured to use different Whisper sizes and quantizations.
- **SubtitlesServer.TranslationApi** - forms promts and connects to an Ollama-hosted LLM to provide translations for batches of subtitles, taking into account their common context. As the LLM speed may be a bottleneck, Translation API supports streaming of translated subtitles to the client, one by one. The API can be configured to use any LLM that Ollama supports.
- **SubtitlesServer.IdentityApi** - IdentityServer project, responsible for authentication and authorization accross the APIs;
- **SubtitlesServer.BFF** - reverse proxy which redirects client requests to all of the above.

> Note the difference: while TranslationApi forms prompts, interacts with external API and processes its responses, WhisperApi, in contrast, works with models directly via Whisper.net library. 

### Available OpenAI Whisper models
https://github.com/openai/whisper?tab=readme-ov-file#available-models-and-languages

### Available Ollama LLMs
https://ollama.com/search
