using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using SelesGames.Rest;
using System.Diagnostics;
using System.Windows.Resources;
using System.IO;
using System.Threading.Tasks;

namespace ReadabilityTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        public class ReadabilityResult
        {
            public string author { get; set; }
            public string content { get; set; }
            public string date_published { get; set; }
            public string domain { get; set; }
            public string title { get; set; }
            public string url { get; set; }
            public string word_count { get; set; }
        }

        string accentColor;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            browser.Opacity = 0d;

            accentColor = ((Color)Resources["PhoneAccentColor"]).ToString().Substring(3).Insert(0, "#");
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var client = new JsonRestClient<ReadabilityResult>();

            var url = "http://www.fourhourworkweek.com/blog/2012/04/22/how-to-build-an-app-empire-can-you-create-the-next-instagram/";
            url = "http://www.engadget.com/2012/02/20/lenovo-ideapad-u300e-ultrabook-available-now/";
            url = "http://www.edge-online.com/news/find-your-future-creating-audio-dice";
            url = "http://www.edge-online.com/reviews/max-payne-3-review;";

            var readability = "https://www.readability.com/api/content/v1/parser?token={0}&url={1}";
            var fullUrl = string.Format(readability, "a142c0cc575c6760d5c46247f8aa6aabbacb6fd8", url);

            try
            {
                var result = await client.GetAsync(fullUrl, System.Threading.CancellationToken.None);
                Debug.WriteLine(result);

                string html = null;

                var path = "/ReadabilityTest;component/css.txt";
                Uri uri = new Uri(path, UriKind.Relative);
                StreamResourceInfo streamResourceInfo = Application.GetResourceStream(uri);
                using (StreamReader streamReader = new StreamReader(streamResourceInfo.Stream))
                {
                    var template = streamReader.ReadToEnd();
                    Debug.WriteLine(template);
                    html = template
                        .Replace("[DOMAIN]", result.domain)
                        .Replace("[TITLE]", result.title)
                        .Replace("[BODY]", result.content)
                        .Replace("[ACCENT]", accentColor);
                    Debug.WriteLine(html);
                }

                browser.NavigateToString(html);
                browser.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(browser_Navigated);
            }
            catch (Exception e2)
            {
                Debug.WriteLine(e2);
            }
        }

        async void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            await Task.Delay(100);
            browser.Opacity = 1d;
        }
    }
}