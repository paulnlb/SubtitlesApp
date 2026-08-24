using Android.Content;
using Android.Database;
using Android.Provider;
using AndroidX.Activity.Result;
using SubtitlesApp.ClientModels;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Services;

/// <summary>
///  Implementation of a file picker that bypasses copying files to the cache but still operates within Android scoped storage
/// </summary>
public partial class LocalFileManager
{
    public async partial Task<MediaFileInfo?> PickFile(string[] mimeTypes)
    {
        var activity = (MainActivity)Platform.CurrentActivity!;
        var uri = await activity.LaunchFilePickingActivity(mimeTypes);

        if (uri is null)
        {
            return null;
        }

        var idResult = GetContentId(uri);
        var nameResult = GetFileName(uri);

        if (idResult.IsFailure)
        {
            throw new InvalidOperationException($"Could not retrieve local file id. Error: {idResult.Error.Description}");
        }

        if (nameResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Could not retrieve local file name. Error: {nameResult.Error.Description}"
            );
        }

        return new MediaFileInfo(
            ClientModels.Enums.FileResourceType.Local,
            idResult.Value,
            nameResult.Value,
            uri.ToString()
        );
    }

    public async partial Task<Result> SaveTextFile(string fileName, string content)
    {
        var activity = (MainActivity)Platform.CurrentActivity!;
        var uri = await activity.LaunchTextFileSavingActivity(fileName);

        if (uri is null)
        {
            return Result.Failure(new Error(ErrorCode.OperationCancelled));
        }

        var resolver = activity.ContentResolver!;

        using var stream = resolver.OpenOutputStream(uri);

        if (stream is null)
        {
            return Result.Failure(
                new Error(ErrorCode.InternalClientError, $"File stream is null for Uri: {uri.ToString()}")
            );
        }

        using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);

        return Result.Success();
    }

    public async partial Task<Result> SaveInternalTextFile(string outputFileName, string sourcePath)
    {
        var activity = (MainActivity)Platform.CurrentActivity!;
        var uri = await activity.LaunchTextFileSavingActivity(outputFileName);

        if (uri is null)
        {
            return Result.Failure(new Error(ErrorCode.OperationCancelled));
        }

        var resolver = activity.ContentResolver!;

        using var outputStream = resolver.OpenOutputStream(uri);

        if (outputStream is null)
        {
            return Result.Failure(
                new Error(ErrorCode.InternalClientError, $"File stream is null for Uri: {uri.ToString()}")
            );
        }

        using var srcStream = File.OpenRead(sourcePath);
        await srcStream.CopyToAsync(outputStream);

        return Result.Success();
    }

    public async partial Task<Result> SaveSrtFile(string fileName, string content)
    {
        var activity = (MainActivity)Platform.CurrentActivity!;
        var uri = await activity.LaunchSrtFileSavingActivity(fileName);

        if (uri is null)
        {
            return Result.Failure(new Error(ErrorCode.OperationCancelled));
        }

        var resolver = activity.ContentResolver!;

        using var stream = resolver.OpenOutputStream(uri);

        if (stream is null)
        {
            return Result.Failure(
                new Error(ErrorCode.InternalClientError, $"File stream is null for Uri: {uri.ToString()}")
            );
        }

        using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);

        return Result.Success();
    }

    public partial Result<Stream> GetFileStream(string uriString)
    {
        if (string.IsNullOrWhiteSpace(uriString))
        {
            return Result<Stream>.Failure(new(ErrorCode.ValidationFailed, "Uri string is empty"));
        }

        var nativeUri = Android.Net.Uri.Parse(uriString);
        if (nativeUri is null)
        {
            return Result<Stream>.Failure(
                new(ErrorCode.ValidationFailed, $"Could not parse native Android Uri from string: {uriString}")
            );
        }

        var resolver = Platform.CurrentActivity!.ContentResolver!;
        try
        {
            var stream = resolver.OpenInputStream(nativeUri);

            if (stream is null)
            {
                return Result<Stream>.Failure(
                    new Error(ErrorCode.InternalClientError, $"File stream is null for Uri: {uriString}")
                );
            }

            return Result<Stream>.Success(stream);
        }
        catch (Exception ex)
        {
            return Result<Stream>.Failure(new Error(ErrorCode.InternalClientError, ex.Message));
        }
    }

    private Result<string> GetFileName(Android.Net.Uri uri)
    {
        var resolver = Platform.CurrentActivity!.ContentResolver!;

        string[] projection = { IOpenableColumns.DisplayName };
        ICursor? cursor = null;

        try
        {
            cursor = resolver.Query(uri, projection, null, null, null);

            if (cursor == null || !cursor.MoveToFirst())
            {
                return Result<string>.Failure(
                    new(
                        ErrorCode.InternalClientError,
                        "Error when trying to get file name from content uri: cursor is null or empty"
                    )
                );
            }

            int nameIndex = cursor.GetColumnIndex(IOpenableColumns.DisplayName);

            if (nameIndex < 0)
            {
                return Result<string>.Failure(
                    new(ErrorCode.InternalClientError, "Error when trying to get file name from content uri: name index < 0")
                );
            }

            var fileName = cursor.GetString(nameIndex);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Result<string>.Failure(new(ErrorCode.InternalClientError, "File name is empty"));
            }

            return Result<string>.Success(fileName);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(new(ErrorCode.InternalClientError, $"Failed to get file name: {ex.Message}"));
        }
        finally
        {
            cursor?.Close();
        }
    }

    private Result<string> GetContentId(Android.Net.Uri uri)
    {
        var id = DocumentsContract.GetDocumentId(uri);

        if (id is null)
        {
            return Result<string>.Failure(new Error(ErrorCode.InternalClientError, "Document id not found"));
        }

        var split = id.Split(':');

        if (split.Length != 2)
        {
            return Result<string>.Success(id);
        }

        return Result<string>.Success(split[1]);
    }
}

public class FilePickerActivityCallback : Java.Lang.Object, IActivityResultCallback
{
    public TaskCompletionSource<Android.Net.Uri?>? Tcs { get; set; }

    public void OnActivityResult(Java.Lang.Object? result)
    {
        if (result is not Android.Net.Uri uri)
        {
            Tcs?.SetResult(null);
            return;
        }

        try
        {
            var contentResolver = Platform.CurrentActivity!.ContentResolver!;
            var takeFlags = ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission;

            contentResolver.TakePersistableUriPermission(uri, takeFlags);
        }
        finally
        {
            Tcs?.SetResult(uri);
        }
    }
}
