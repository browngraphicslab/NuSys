using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Async;
using SQLite.Net;
using Windows.Storage;
using System.IO;
using SQLite.Net.Platform.WinRT;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace NuSysApp
{
    public class SQLiteDatabase
    {
        public SQLiteDatabase(string filename)
        {
            DBPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, filename);
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