using SelesGames.ZestLibrary;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SelesGames.WP.PublisherInfoPage
{
    public class PublisherInfoViewModel
    {
        string publisherName;

        public ObservableCollection<ZuneAppViewModel> Results { get; private set; }

        public PublisherInfoViewModel(string publisherName)
        {
            this.publisherName = publisherName;
            Results = new ObservableCollection<ZuneAppViewModel>();
        }

        internal async Task GetAppsForPublisherAsync()
        {
            if (Results.Any())
                Results.Clear();

            var currentCultureString = CultureInfo.CurrentCulture.Name;
            var browseAppsService = new ZuneService();
            var result = await browseAppsService
                .BrowseAppsAsync(currentCultureString, publisher: publisherName, chunkSize: 50, orderBy: AppOrder.DownloadRank);

            var apps = result.Apps.ToViewModels();

            //await apps.AllIconsLoadedAsync().WaitNoLongerThan(2000);

            foreach (var app in apps)
                Results.Add(app);
        }
    }
}
