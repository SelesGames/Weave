using System;
using System.Threading.Tasks;

namespace SelesGames.UI.Advertising
{
    public class AdService
    {
        #region Private Member variables

        string adSettingsUrl;
        int numberOfTimesAdsShown = 0;
        AdSettings settings;
        AdControlFactory controlFactory;
        bool isInitialized = false;

        #endregion




        #region Public Properties and Events

        public bool IsAddSupportedApp { get; set; }
        public int MaxAdsPerSession { get; set; }
        public Task Initialization { get; private set; }
        public event EventHandler AdsNoLongerShown;

        #endregion




        #region Public Read-Only Properties - dependent on Initialization occuring first

        public bool ShouldDisplayAds
        {
            get
            {
                CheckInitialization();

                return
                    settings.AreAdsActive &&
                    IsAddSupportedApp &&
                    numberOfTimesAdsShown < MaxAdsPerSession;
            }
        }

        internal AdSettings Settings
        {
            get
            {
                CheckInitialization();
                return settings;
            }
        }

        internal AdControlFactory ControlFactory
        {
            get
            {
                CheckInitialization();
                return controlFactory;
            }
        }

        #endregion




        #region Constructor 

        public AdService(string adSettingsUrl)
        {
            this.adSettingsUrl = adSettingsUrl;
            MaxAdsPerSession = 1;
            Initialization = Initialize();
        }

        #endregion




        public void AdEngaged()
        {
            numberOfTimesAdsShown++;
            if (numberOfTimesAdsShown >= MaxAdsPerSession)
                if (AdsNoLongerShown != null)
                    AdsNoLongerShown(null, EventArgs.Empty);
        }

        public SwitchingAdControl CreateAdControl(string keywords = null)
        {
            return new SwitchingAdControl(this, keywords);
        }




        #region Asynchronously Initialize the service

        void CheckInitialization()
        {
            if (!isInitialized)
                throw new AdServiceInitializationException();
        }

        async Task Initialize()
        {
            try
            {
                var client = new AdSettingsClient(adSettingsUrl);
                settings = await client.Get();
                controlFactory = new AdControlFactory(Settings);
                isInitialized = true;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        #endregion
    }
}
