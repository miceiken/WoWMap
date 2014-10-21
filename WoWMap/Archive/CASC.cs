using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using CASCExplorer;

namespace WoWMap.Archive
{
    public static class CASC
    {
        public static void Initialize(string path, LocaleFlags locale)
        {
            Worker = new BackgroundWorker();
            Locale = locale;
            Handler = CASCHandler.OpenLocalStorage(path, Worker);

            Initialized = true;
        }

        public static void Initialize(string path)
        {
            Initialize(path, LocaleFlags.All);
        }

        public static bool Initialized
        {
            get;
            private set;
        }

        public static CASCHandler Handler
        {
            get;
            private set;
        }

        public static LocaleFlags Locale
        {
            get;
            private set;
        }

        public static BackgroundWorker Worker
        {
            get;
            private set;
        }

        public static Stream OpenFile(string filename)
        {
            return Handler.OpenFile(filename, Locale);
        }

        public static bool FileExists(string filename)
        {
            return Handler.FileExis(filename);
        }
    }
}
