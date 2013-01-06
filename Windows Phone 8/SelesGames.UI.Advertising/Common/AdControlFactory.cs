using SelesGames.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SelesGames.UI.Advertising
{
    public class AdControlFactory
    {
        readonly string adSettingsUrl;// = "http://weave.blob.core.windows.net/settings/sampleSettings.json";
        Common.AdSettings adSettings;
        IEnumerator<AdSettingsBase> adSettingsEnumerator;
        bool isInitialized = false;

        public AdControlFactory(string adSettingsUrl)
        {
            this.adSettingsUrl = adSettingsUrl;
        }

        async Task InitializeAsync()
        {
            if (isInitialized == false)
            {
                var client = new JsonRestClient<Common.AdSettings>();
                adSettings = await client.GetAsync(adSettingsUrl, System.Threading.CancellationToken.None);
                ResetEnumerator();
                isInitialized = true;
            }
        }

        public void ResetEnumerator()
        {
            adSettingsEnumerator = adSettings
                .AsEnumerable()
                .Select(o => Tuple.Create(o, o.FaultToleranceCount))
                .RepeatEnumerable()
                .Wrap()
                .GetEnumerator();
        }

        public async Task<IAdControlAdapter> CreateAdControl(string keywords = null)
        {
            await InitializeAsync();

            if (adSettingsEnumerator == null || !adSettingsEnumerator.MoveNext())
                throw new InvalidOperationException("no adSettings have been set in AdControlFactory.CreateAdControl");

            var currentSelectedSetting = adSettingsEnumerator.Current;
            return currentSelectedSetting.CreateAdControl(keywords);
        }
    }

    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> RepeatEnumerable<T>(this IEnumerable<Tuple<T, int>> o)
        {
            if (o == null)
                throw new ArgumentNullException("parameter in IEnumerableExtensions.Wrap");

            if (!o.Any())
                yield break;

            var enumerator = o.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                for (int i = 0; i < current.Item2; i++)
                    yield return current.Item1;
            }
        }

        public static IEnumerable<T> Wrap<T>(this IEnumerable<T> o)
        {
            if (o == null)
                throw new ArgumentNullException("parameter in IEnumerableExtensions.Wrap");

            if (!o.Any())
                yield break;

            var enumerator = o.GetEnumerator();

            while (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
                enumerator = o.GetEnumerator();
            }
        }
    }
}
