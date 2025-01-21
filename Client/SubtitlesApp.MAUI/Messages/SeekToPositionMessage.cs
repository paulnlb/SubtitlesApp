using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SubtitlesApp.Messages;

public class SeekToPositionMessage(TimeSpan position) : ValueChangedMessage<TimeSpan>(position) { }
