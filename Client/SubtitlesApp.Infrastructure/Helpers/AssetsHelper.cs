namespace SubtitlesApp.Infrastructure.Helpers;

public static class AssetsHelper
{
    public static string ExtractAssetToAppData(string assetPath, string destFileName, string destSubDir = "")
    {
        // 1. Get the AppData directory equivalent in Android (Internal Storage)
        // This resolves to /data/user/0/your.package.name/files
        var internalFilesDir = Application.Context.FilesDir!.AbsolutePath;
        var targetFile = Path.Combine(internalFilesDir, destSubDir, destFileName);

        if (File.Exists(targetFile))
            return targetFile;

        using var inputStream = Application.Context.Assets.Open(assetPath);

        if (!string.IsNullOrWhiteSpace(destSubDir))
        {
            Directory.CreateDirectory(Path.Combine(internalFilesDir, destSubDir));
        }

        using var outputStream = File.Create(targetFile);
        inputStream.CopyTo(outputStream);

        return targetFile;
    }
}
