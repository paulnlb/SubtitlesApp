# SubtitlesApp

SubtitlesApp is a .NET MAUI mobile application that uses AI tools to generate subtitles for any video, translate them into multiple languages, and display them as a navigable collection, similar to a deck of cards.

<img width="900" alt="SubAppPreview" src="https://github.com/user-attachments/assets/53e7db25-70c9-485e-aefd-a4e62cb0b3a4" />

## Supported Platforms
✅ Android\
❌ Everything else

## Features
Inside the app, users can:
- Play local or remote videos;
- Generate subtitles for any video by selecting the start time, end time, and optionally the source language;
- Translate subtitles into multiple languages;
- Instantly switch between original subtitles and their translations;
- Interact with the subtitle list: scroll through it and double-tap a subtitle to rewind the video;
- Hide or reveal the subtitle list using swipe gestures.

Subtitles are displayed in a scrollable list that is automatically synchronized with the current video playback. The list can also be manually scrolled and hidden or revealed when needed.

For video transcription, the app relies on an OpenAI-compatible transcription API. You can configure the model, endpoint (including local/self-hosted servers), and API key.

For translation, the app relies on the OpenAI-compatible `/responses` API. The app uses LLMs for subtitle translation because even small locally hosted models can provide fluent translations and broad language support. You can configure the model, endpoint (including local/self-hosted servers), and API key.




