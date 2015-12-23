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
        public static void Initialize(string path, LocaleFlags locale, AsyncAction worker = null)
        {
            Locale = locale;
            Handler = CASCHandler.OpenLocalStorage(path, worker);
            Handler.Root.SetFlags(locale, ContentFlags.None, false);

            Initialized = true;
        }

        public static void InitializeOnline(LocaleFlags locale, AsyncAction worker = null)
        {
            Locale = locale;
            Handler = CASCHandler.OpenOnlineStorage("wow", worker);
            Handler.Root.SetFlags(locale, ContentFlags.None, false);

            Initialized = true;
        }

        public static void InitializeOnline(AsyncAction worker = null)
        {
            InitializeOnline(LocaleFlags.enUS, worker);
        }

        public static void Initialize(string path, AsyncAction worker = null)
        {
            Initialize(path, LocaleFlags.enUS, worker);
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

        public static Stream OpenFile(string filename)
        {
            if (File.Exists("files/" + filename.ToLowerInvariant()))
                return File.Open("files/" + filename.ToLowerInvariant(), FileMode.Open, FileAccess.Read, FileShare.Read);

            Console.WriteLine("Downloading {0}", filename);

            var str = Handler.OpenFile(filename);

            if (!Directory.Exists(Path.GetDirectoryName("files/" + filename.ToLowerInvariant())))
                Directory.CreateDirectory(Path.GetDirectoryName("files/" + filename.ToLowerInvariant()));

            using (var fs = File.Create("files/" + filename.ToLowerInvariant()))
                str.CopyTo(fs);
            str.Position = 0;
            return str;
        }

        public static bool FileExists(string filename)
        {
            return Handler.FileExists(filename);
        }
    }
}
