using System;
using System.IO;
using Windows.Storage;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Platform.WinRT;

namespace NuSysApp
{
    public class SQLiteDatabase
    {
        public SQLiteDatabase(string filename)
        {
            DBPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, filename);
            Func<SQLiteConnectionWithLock> connectionFactory = new Func<SQLiteConnectionWithLock>(() => 
                new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(DBPath, storeDateTimeAsTicks: false)));
            DBConnection = new SQLiteAsyncConnection(connectionFactory);
        }

        public SQLiteAsyncConnection DBConnection
        {
            get; set;
        }

        public String DBPath
        {
            get; set;
        }
    }
}