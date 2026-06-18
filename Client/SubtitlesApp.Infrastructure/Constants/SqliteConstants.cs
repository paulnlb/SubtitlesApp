namespace SubtitlesApp.Infrastructure.Constants;

public static class SqliteConstants
{
    public const string DatabaseFilename = "AppData.db3";

    public const SQLite.SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLite.SQLiteOpenFlags.ReadWrite
        |
        // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.Create
        |
        // enable multi-threaded database access
        SQLite.SQLiteOpenFlags.SharedCache;
}
