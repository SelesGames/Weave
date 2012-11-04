using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Linq;
using System.ComponentModel;

namespace weave
{
    public partial class FadeInImage : UserControl, INotifyPropertyChanged
    {
        string imageUrl;

        public FadeInImage()
        {
            InitializeComponent();
            if (this.IsInDesignMode())
                return;
            
            //image.Source = null;
            DataContext = this;
        }

        public string Source
        {
            get { return imageUrl; }
            set
            {
                imageUrl = value;
                PropertyChanged.Raise(this, "Source");
                //if (string.IsNullOrEmpty(value))
                //    return;

                //if (AsyncImageSourceConverter.Cache.ContainsKey(value))
                //{
                //    image.Source = AsyncImageSourceConverter.Cache[value];
                //    return;
                //}
                //if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
                //{
                //    WebRequest request = null;
                //    try
                //    {
                //        request = HttpWebRequest.Create(new Uri(value, UriKind.Absolute));
                //    }
                //    catch (Exception ex)
                //    {
                //        DebugEx.WriteLine("Problem creating {0}: {1}", value, ex.ToString());
                //    }
                //    if (request == null)
                //        return;

                //    IDisposable disposeHandle = null;
                //    disposeHandle = request
                //        .GetResponseStreamAsync()
                //        .ObserveOnDispatcher()
                //        .Select(i => new { x = new BitmapImage(), Stream = i })
                //        .Do(i =>
                //        {
                //            try
                //            {
                //                i.x.SetSource(i.Stream);
                //            }
                //            catch (Exception) { }
                //        })
                //        .Subscribe(i =>
                //        {
                //            AsyncImageSourceConverter.Cache.UpdateValueForKey(i.x, value);
                //            image.Source = i.x;
                //            //ImageFadeInSB.Begin();
                //            //ImageSlideInSB.Begin();
                //            ImagePlopInSB.Begin();
                //            i.Stream.Close();
                //            i.Stream.Dispose();
                //            if (disposeHandle != null)
                //                disposeHandle.Dispose();
                //        });
                //}
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
