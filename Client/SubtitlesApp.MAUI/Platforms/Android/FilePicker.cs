using Android.Content;
using Android.Database;
using Android.Provider;
using AndroidX.Activity.Result;
using SubtitlesApp.Core.Result;
using SubtitlesApp.Infrastructure.Interfaces;
using SubtitlesApp.Platforms.Android;

namespace SubtitlesApp.Services;

/// <summary>
///  Implementation of a file picker that bypasses copying files to the cache but still operates within Android scoped storage
/// </summary>
public partial class FilePicker
{
    public async partial Task<string?> PickAsync(string[] mimeTypes)
    {
        var status = await Permissions.CheckStatusAsync<ReadAudioVideoPerms>();

        if (status != PermissionStatus.Granted)
        {
            return null;
        }

        MainActivity.Instance.FilePickerActivityCallback.Tcs = new TaskCompletionSource<string?>();

        MainActivity.Instance.FilePickerLauncher?.Launch(mimeTypes);

        return await MainActivity.Instance.FilePickerActivityCallback.Tcs.Task;
    }

    public partial Result<string> GetFileName(string uriString)
    {
        if (string.IsNullOrWhiteSpace(uriString))
        {
            return Result<string>.Failure(new(ErrorCode.ValidationFailed, "Uri string is empty"));
        }

        var nativeUri = Android.Net.Uri.Parse(uriString);
        if (nativeUri is null)
        {
            return Result<string>.Failure(
                new(ErrorCode.ValidationFailed, $"Could not parse native Android Uri from string: {uriString}")
            );
        }

        var resolver = Platform.CurrentActivity!.ContentResolver!;

        string[] projection = { IOpenableColumns.DisplayName };
        ICursor? cursor = null;

        try
        {
            cursor = resolver.Query(nativeUri, projection, null, null, null);

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

    public partial Result<IFileResource> GetLocalPath(Uri uri)
    {
        if (uri.Scheme != "content")
        {
            return Result<IFileResource>.Failure(
                new Error(ErrorCode.ValidationFailed, $"Uri scheme {uri.Scheme} is not supported")
            );
        }

        var uriString = uri.ToString();
        var nativeUri = Android.Net.Uri.Parse(uriString);

        if (nativeUri is null)
        {
            return Result<IFileResource>.Failure(
                new(ErrorCode.ValidationFailed, $"Could not parse native Android Uri from string: {uriString}")
            );
        }

        var resolver = Platform.CurrentActivity!.ContentResolver!;

        var parcelFd = resolver.OpenFileDescriptor(nativeUri, "rw");

        if (parcelFd is null)
        {
            return Result<IFileResource>.Failure(
                new(ErrorCode.InternalClientError, $"File descriptor is null for Uri: {uriString}")
            );
        }

        var resource = new FileDescriptorResource(parcelFd);

        return Result<IFileResource>.Success(resource);
    }
}

public class FilePickerActivityCallback : Java.Lang.Object, IActivityResultCallback
{
    public TaskCompletionSource<string?>? Tcs { get; set; }

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
            Tcs?.SetResult(uri.ToString());
        }
    }
}
