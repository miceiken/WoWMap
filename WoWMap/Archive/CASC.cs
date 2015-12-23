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
            Locale = locale;
            Handler = CASCHandler.OpenLocalStorage(path);
            Handler.Root.SetFlags(locale, ContentFlags.None, false);

            Initialized = true;
        }

        public static void InitializeOnline(LocaleFlags locale)
        {
            Locale = locale;
            Handler = CASCHandler.OpenOnlineStorage("wow");
            Handler.Root.SetFlags(locale, ContentFlags.None, false);

            Initialized = true;
        }

        public static void InitializeOnline()
        {
            InitializeOnline(LocaleFlags.enUS);
        }

        public static void Initialize(string path)
        {
            Initialize(path, LocaleFlags.enUS);
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
            return Handler.OpenFile(filename);
        }

        public static bool FileExists(string filename)
        {
            return Handler.FileExists(filename);
        }
    }
}
