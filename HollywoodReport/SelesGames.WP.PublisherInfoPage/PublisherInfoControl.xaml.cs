using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Linq;
using SelesGames.Phone;

namespace SelesGames.WP.PublisherInfoPage
{
    public partial class PublisherInfoControl : UserControl
    {
        Task<string> getPaidAppId;
        string publisherName;
        PublisherInfoViewModel viewModel;
        Button changelogButton, buyButton, rateButton, twitterButton, facebookButton;

        public string PublisherName
        {
            get { return publisherName; }
            set
            {
                publisherName = value;
                PageTitle.Text = publisherName.ToLowerInvariant();
                viewModel = new PublisherInfoViewModel(publisherName);
                DataContext = viewModel;
            }
        }

        public string TwitterUserName { get; set; }
        public string FacebookUserName { get; set; }
        public string ChangeLogUrl { get; set; }

        public async Task LoadDataAsync()
        {
            await viewModel.GetAppsForPublisherAsync();
        }

        public PublisherInfoControl()
        {
            InitializeComponent();
            list.Loaded += list_Loaded;
        }

        public override void OnApplyTemplate()
        {
            SetChangelogButton();
            SetBuyButton();
            SetRateButton();
            SetTwitterButton();
            SetFacebookButton();
        }

        void list_Loaded(object sender, RoutedEventArgs e)
        {
            list.Loaded -= list_Loaded;
            var llsHeader = list.Descendants<StackPanel>().OfType<StackPanel>().FirstOrDefault(o => o.Name == "buttonHeader");
            var buttons = llsHeader.Descendants<Button>().OfType<Button>().ToList();
            changelogButton = buttons[0];
            buyButton = buttons[1];
            rateButton = buttons[2];
            twitterButton = buttons[3];
            facebookButton = buttons[4];

            SetChangelogButton();
            SetBuyButton();
            SetRateButton();
            SetTwitterButton();
            SetFacebookButton();
        }



        void AppTap(object sender, System.Windows.RoutedEventArgs e)
        {
            var app = ((Button)sender).DataContext as ZuneAppViewModel;
            TaskService.ToMarketplaceDetailTask(app.AppId);
        }

        async void OnHeaderButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var button = (sender as Button);

            if (button == buyButton)
            {
                try
                {
                    if (getPaidAppId != null)
                    {
                        var appId = await getPaidAppId;
                        TaskService.ToMarketplaceDetailTask(appId);
                    }
                }
                catch
                {
                    MessageBox.Show("Unable to get the link for the paid version of AppVegas.  Please try again later!");
                }
            }

            else if (button == rateButton)
                TaskService.ToMarketplaceDetailTask();

            else if (button == twitterButton)
                TaskService.ToInternetExplorerTask("http://mobile.twitter.com/" + TwitterUserName);

            else if (button == facebookButton)
                TaskService.ToInternetExplorerTask("http://touch.facebook.com/" + FacebookUserName);

            else if (button == changelogButton)
                TaskService.ToInternetExplorerTask(ChangeLogUrl);
        }

        void OnEmailLinkTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TaskService.ToEmailComposeTask(To: "info@selesgames.com");
        }




        #region Button Text Functions

        void SetChangelogButton()
        {
            if (changelogButton == null)
                return;

            changelogButton.Content = ChangelogText;

            if (string.IsNullOrEmpty(ChangelogText))
                changelogButton.Visibility = Visibility.Collapsed;
            else
                changelogButton.Visibility = Visibility.Visible;
        }

        void SetBuyButton()
        {
            if (buyButton == null)
                return;

            buyButton.Content = BuyText;

            if (string.IsNullOrEmpty(BuyText))
                buyButton.Visibility = Visibility.Collapsed;
            else
                buyButton.Visibility = Visibility.Visible;
        }

        void SetRateButton()
        {
            if (rateButton == null)
                return;

            rateButton.Content = RateText;

            if (string.IsNullOrEmpty(RateText))
                rateButton.Visibility = Visibility.Collapsed;
            else
                rateButton.Visibility = Visibility.Visible;
        }

        void SetTwitterButton()
        {
            if (twitterButton == null)
                return;

            twitterButton.Content = TwitterText;

            if (string.IsNullOrEmpty(TwitterText))
                twitterButton.Visibility = Visibility.Collapsed;
            else
                twitterButton.Visibility = Visibility.Visible;
        }

        void SetFacebookButton()
        {
            if (facebookButton == null)
                return;

            facebookButton.Content = FacebookText;

            if (string.IsNullOrEmpty(FacebookText))
                facebookButton.Visibility = Visibility.Collapsed;
            else
                facebookButton.Visibility = Visibility.Visible;
        }

        #endregion




        #region Dependency Properties




        #region ChangelogText

        public static readonly DependencyProperty ChangelogTextProperty = DependencyProperty.Register(
            "ChangelogText", typeof(string), typeof(PublisherInfoControl), new PropertyMetadata(OnChangelogTextChanged));

        public string ChangelogText
        {
            get { return (string)GetValue(ChangelogTextProperty); }
            set { SetValue(ChangelogTextProperty, value); }
        }

        static void OnChangelogTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (PublisherInfoControl)obj;
            c.SetChangelogButton();
        }

        #endregion




        #region BuyText

        public static readonly DependencyProperty BuyTextProperty = DependencyProperty.Register(
            "BuyText", typeof(string), typeof(PublisherInfoControl), new PropertyMetadata(OnBuyTextChanged));

        public string BuyText
        {
            get { return (string)GetValue(BuyTextProperty); }
            set { SetValue(BuyTextProperty, value); }
        }

        static void OnBuyTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (PublisherInfoControl)obj;
            c.SetBuyButton();
        }

        #endregion




        #region RateText

        public static readonly DependencyProperty RateTextProperty = DependencyProperty.Register(
            "RateText", typeof(string), typeof(PublisherInfoControl), new PropertyMetadata(OnRateTextChanged));

        public string RateText
        {
            get { return (string)GetValue(RateTextProperty); }
            set { SetValue(RateTextProperty, value); }
        }

        static void OnRateTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (PublisherInfoControl)obj;
            c.SetRateButton();
        }

        #endregion




        #region TwitterText

        public static readonly DependencyProperty TwitterTextProperty = DependencyProperty.Register(
            "TwitterText", typeof(string), typeof(PublisherInfoControl), new PropertyMetadata(OnTwitterTextChanged));

        public string TwitterText
        {
            get { return (string)GetValue(TwitterTextProperty); }
            set { SetValue(TwitterTextProperty, value); }
        }

        static void OnTwitterTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (PublisherInfoControl)obj;
            c.SetTwitterButton();
        }

        #endregion




        #region FacebookText

        public static readonly DependencyProperty FacebookTextProperty = DependencyProperty.Register(
            "FacebookText", typeof(string), typeof(PublisherInfoControl), new PropertyMetadata(OnFacebookTextChanged));

        public string FacebookText
        {
            get { return (string)GetValue(FacebookTextProperty); }
            set { SetValue(FacebookTextProperty, value); }
        }

        static void OnFacebookTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (PublisherInfoControl)obj;
            c.SetFacebookButton();
        }

        #endregion




        #endregion
    }
}
