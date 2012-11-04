using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace weave
{
    [KnownType(typeof(NewsItem))]
    [DataContract]
    public class WebBrowserPageViewModel
    {
        const string INSTAPAPER_MOBILIZER_URI = AppSettings.MOBILIZER_URL;

        [DataMember] public NewsItem NewsItem { get; set; }
        [DataMember] public List<string> Uris { get; set; }
        [DataMember] public int CurrentIndex { get; set; }

        bool isInstapaperMobilizerEnabled;
        public bool IsInstapaperMobilizerEnabled 
        {
            get { return isInstapaperMobilizerEnabled; }
            set
            {
                isInstapaperMobilizerEnabled = value;
                AppSettings.PermanentState.IsInstapaperMobilizerEnabled = value;
                FormatUriHistory();
            }
        }

        public WebBrowserPageViewModel()
        {
            Uris = new List<string>();
            CurrentIndex = -1;
            isInstapaperMobilizerEnabled = AppSettings.PermanentState.IsInstapaperMobilizerEnabled;
        }

        public string CurrentUri
        {
            get
            {
                if (Uris.Count > CurrentIndex)
                    return Uris[CurrentIndex];
                else
                    return null;
            }
        }

        internal void Next()
        {
            CurrentIndex++;
        }


        internal void Previous()
        {
            CurrentIndex--;
        }

        internal bool HasNext
        {
            get
            {
                return (CurrentIndex + 1) < Uris.Count;
            }
        }

        internal bool HasPrevious
        {
            get
            {
                return (CurrentIndex - 1) >= 0;
            }
        }

        internal void Insert(string uri)
        {
            if (isInstapaperMobilizerEnabled)
                uri = PrependInstapaperUrl(uri);

            if (HasNext)
                Uris.RemoveRange(CurrentIndex + 1, Uris.Count - CurrentIndex - 1);
            CurrentIndex++;
            Uris.Add(uri);
        }

        static string PrependInstapaperUrl(string uri)
        {
            if (!uri.StartsWith(INSTAPAPER_MOBILIZER_URI))
            {
                uri = string.Format("{0}{1}",
                    INSTAPAPER_MOBILIZER_URI,
                    Uri.EscapeDataString(uri));
            }
            return uri;
        }

        static string RemoveInstapaperUrl(string uri)
        {
            if (uri.StartsWith(INSTAPAPER_MOBILIZER_URI))
            {
                uri = Uri.UnescapeDataString(uri.Replace(INSTAPAPER_MOBILIZER_URI, string.Empty));
            }
            return uri;
        }

        void FormatUriHistory()
        {
            if (isInstapaperMobilizerEnabled)
            {
                for (int i = 0; i < Uris.Count; i++ )
                {
                    Uris[i] = PrependInstapaperUrl(Uris[i]);
                }
            }
            else
            {
                for (int i = 0; i < Uris.Count; i++)
                {
                    Uris[i] = RemoveInstapaperUrl(Uris[i]);
                }
            }
        }
    }
}
