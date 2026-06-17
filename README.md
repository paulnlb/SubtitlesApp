# SubtitlesApp

SubtitlesApp (working title) is a .NET MAUI mobile application that uses AI tools to generate subtitles for any video, translate them into multiple languages, and display them as a navigable collection.

<img width="900" alt="SubAppPreview" src="https://github.com/user-attachments/assets/898c3c89-e4e9-4b66-b1aa-e9e93d64bb2c" />

## Supported Platforms
✅ Android\
❌ Everything else

## Features
Inside the app, users can:
- Play local or online videos;
- Generate subtitles for any video by selecting the start time, end time, and optionally the source language;
- Translate subtitles into multiple languages;
- Instantly switch between original subtitles and their translations;
- Interact with the subtitle list: scroll through it and double-tap a subtitle to rewind the video;
- Hide or reveal the subtitle list using swipe gestures.

Subtitles are displayed in a scrollable list that is automatically synchronized with the current video playback. The list can also be manually scrolled and hidden or revealed when needed.

For video transcription, the app relies on an OpenAI-compatible `/transcription` API. You can configure the model, endpoint (for third-party/self-hosted servers), and API key.

For translation, the app can be configured to use either an OpenAI-compatible `/responses` API or Google Gemini API. The app uses LLMs for subtitle translation because even small locally hosted models can provide fluent translations and broad language support. You can configure the model, endpoint (for third-party/self-hosted OpenAI-compatible servers), and API key.




