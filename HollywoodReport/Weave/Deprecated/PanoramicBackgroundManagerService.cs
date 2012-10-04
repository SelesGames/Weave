using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SelesGames.WP.IsoStorage;

namespace weave.Services
{
    [DataContract]
    public class PanoramicBackgroundManagerService : INotifyPropertyChanged
    {
        Brush backgroundBrush;
        Brush foregroundBrush;
        Random r = new Random();
        PanoramaBackgroundSetting selectedBackgroundSetting;
        string backgroundImageUrl;
        string defaultBackgroundImageUrl;
        Color foregroundColor;
        Color defaultForegroundColor;


        public PanoramicBackgroundManagerService()
        {
            BackgroundForegroundCombos = new List<PanoramaBackgroundSetting>();
        }


        static PanoramicBackgroundManagerService current;
        public static PanoramicBackgroundManagerService Current
        {
            get
            {
                if (current == null)
                    current = Restore();
                return current;
            }
        }



        #region static and public methods for saving and restoring state from iso storage

        static SafeIsoStorageFileWrapper<PanoramicBackgroundManagerService> CreateReadWriterObject()
        {
            var serializer = new DataContractSerializer(typeof(PanoramicBackgroundManagerService));
            var wrapper = new SafeIsoStorageFileWrapper<PanoramicBackgroundManagerService>(
                "panoBGMS", serializer.WriteObject,
                stream => (PanoramicBackgroundManagerService)serializer.ReadObject(stream));
            return wrapper;
        }

        static PanoramicBackgroundManagerService Restore()
        {
            var panoBGService = CreateReadWriterObject().Get();
            var temp = panoBGService;
            temp.Wait();
            return temp.Result;
        }

        public void Save()
        {
            CreateReadWriterObject().Save(this);
        }

        #endregion




        #region the saved to iso storage background image source (url), as well as the color for the pano foreground

        [DataMember]
        public string BackgroundImageUrl 
        {
            get { return backgroundImageUrl; }
            set
            {
                if (backgroundImageUrl != value)
                {
                    backgroundImageUrl = value;
                    BackgroundBrush = new ImageBrush
                    {
                        ImageSource = new BitmapImage(backgroundImageUrl.ToUri(UriKind.Relative)),
                        Stretch = Stretch.None,
                    };
                }
            }
        }

        [DataMember]
        public Color ForegroundColor 
        {
            get { return foregroundColor; } 
            set
            {
                if (foregroundColor != value)
                {
                    foregroundColor = value;
                    ForegroundBrush = new SolidColorBrush(foregroundColor);
                }
            }
        }

        #endregion




        #region the default background image and foreground that should be used if there is no values from iso storage to restore.  Set in App.xaml.cs

        public string DefaultBackgroundImageUrl
        {
            get { return defaultBackgroundImageUrl; }
            set
            {
                if (defaultBackgroundImageUrl != value)
                {
                    defaultBackgroundImageUrl = value;
                    if (BackgroundImageUrl == null)
                    {
                        BackgroundImageUrl = defaultBackgroundImageUrl;
                    }
                }
            }
        }

        public Color DefaultForegroundColor
        {
            get { return defaultForegroundColor; }
            set
            {
                if (defaultForegroundColor != value)
                {
                    defaultForegroundColor = value;
                    if (ForegroundColor == null)
                    {
                        ForegroundColor = defaultForegroundColor;
                    }
                }
            }
        }

        #endregion




        #region the actual, bindable background brush and foreground brush that will be used in the panorama view

        public Brush BackgroundBrush
        {
            get { return backgroundBrush; }
            set
            {
                if (backgroundBrush != value)
                {
                    backgroundBrush = value;
                    PropertyChanged.Raise(this, "BackgroundBrush");
                }
            }
        }

        public Brush ForegroundBrush
        {
            get { return foregroundBrush; }
            set
            {
                if (foregroundBrush != value)
                {
                    foregroundBrush = value;
                    PropertyChanged.Raise(this, "ForegroundBrush");
                }
            }
        }

        #endregion




        public List<PanoramaBackgroundSetting> BackgroundForegroundCombos { get; set; }

        public PanoramaBackgroundSetting SelectedBackgroundSetting
        {
            get { return selectedBackgroundSetting; }
            set
            {
                if (selectedBackgroundSetting != value)
                {
                    selectedBackgroundSetting = value;
                    BackgroundImageUrl = value.SourceUrl;
                    ForegroundColor = value.Foreground;
                }
            }
        }

        public bool CanManuallyChooseBackground { get; set; }

        public void RegisterAdditionalBackground(string sourceUrl, string description, Color foreground)
        {
            if (BackgroundForegroundCombos == null)
                BackgroundForegroundCombos = new List<PanoramaBackgroundSetting>();

            BackgroundForegroundCombos.Add(new PanoramaBackgroundSetting
            {
                SourceUrl = sourceUrl,
                Description = description,
                Foreground = foreground,
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //public void InitializeRandom()
        //{
        //    if (BackgroundForegroundCombos.Count == 0)
        //        return;

        //    var index = r.Next(0, BackgroundForegroundCombos.Count);
        //    var newCombo = BackgroundForegroundCombos[index];
        //    SelectedBackgroundSetting = newCombo;
        //    BackgroundImageUrl = newCombo.SourceUrl;
        //    ForegroundColor = newCombo.Foreground;
        //}

        public void Initialize()
        {
            var currentSelectedCombo = BackgroundForegroundCombos.FirstOrDefault(o =>
                o.Foreground == this.ForegroundColor
                &&
                o.SourceUrl == this.BackgroundImageUrl);

            if (currentSelectedCombo == null)
                return;

            selectedBackgroundSetting = currentSelectedCombo;
        }
    }

    public class PanoramaBackgroundSetting
    {
        public string SourceUrl { get; set; }
        public string Description { get; set; }
        public Color Foreground { get; set; }
    }
}
