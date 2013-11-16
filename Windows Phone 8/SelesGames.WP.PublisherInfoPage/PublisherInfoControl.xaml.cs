using SelesGames.Phone;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace SelesGames.WP.PublisherInfoPage
{
    [ContentProperty("Footer")]
    public partial class PublisherInfoControl : UserControl
    {
        Task<string> getPaidAppId;
        string publisherName;

        public string PublisherName
        {
            get { return publisherName; }
            set
            {
                publisherName = value;
                PageTitle.Text = publisherName.ToLowerInvariant();
            }
        }

        public string TwitterUserName { get; set; }
        public string FacebookUserName { get; set; }

        public PublisherInfoControl()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            SetRateButton();
            SetTwitterButton();
            SetFacebookButton();
            SetFooterTemplate();
        }




        #region Event Handler for button taps

        void OnHeaderButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var button = (sender as Button);

            if (button == rateButton)
                TaskService.ToMarketplaceDetailTask();

            else if (button == twitterButton)
                TaskService.ToInternetExplorerTask("http://mobile.twitter.com/" + TwitterUserName);

            else if (button == facebookButton)
                TaskService.ToInternetExplorerTask("http://touch.facebook.com/" + FacebookUserName);
        }

        void OnEmailLinkTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TaskService.ToEmailComposeTask(To: "info@selesgames.com");
        }

        #endregion




        #region Button Text Functions

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




        void SetFooterTemplate()
        {
            footer.ContentTemplate = FooterTemplate;
        }




        #region Dependency Properties




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




        #region FooterTemplate

        public static readonly DependencyProperty FooterTemplateProperty = DependencyProperty.Register(
            "FooterTemplate", typeof(DataTemplate), typeof(PublisherInfoControl), new PropertyMetadata(OnFooterTemplateChanged));

        public DataTemplate FooterTemplate
        {
            get { return (DataTemplate)GetValue(FooterTemplateProperty); }
            set { SetValue(FooterTemplateProperty, value); }
        }

        static void OnFooterTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (PublisherInfoControl)obj;
            c.SetFooterTemplate();
        }

        #endregion




        #endregion
    }
}
