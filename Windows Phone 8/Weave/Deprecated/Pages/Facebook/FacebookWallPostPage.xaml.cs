using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using weave.Services.Facebook;

namespace weave
{
    public partial class FacebookWallPostPage : PhoneApplicationPage, IDisposable
    {
        bool navigatedAway = false;
        string name, caption, description, link, profilePicUrl;
        bool wentForwardToAuthPage = false;
        //static ImageCache profilePicCache = new ImageCache();
        CompositeDisposable disposables = new CompositeDisposable();

        public FacebookWallPostPage()
        {
            InitializeComponent();
            profilePic.Opacity = 0;
            profileImageBrush.ImageSource = null;

            MessageTextBox.Text = string.Empty;
            name = caption = description = link = null;

            VisualStateManager.GoToState(this, "PreLoad", false);

            IDisposable disp;
            disp = 
                this.GetLoaded().Subscribe(notUsed =>
                {
                    SetPicSource();
                    this.wallPostTitle.Text = this.name;
                    this.wallPostCaption.Text = this.caption;
                    LoadSB.Begin();
                });

            disposables.Add(disp);

            disp =
                Observable.FromEventPattern<OrientationChangedEventArgs>(GlobalNavigationService.CurrentFrame, "OrientationChanged")
                .Select(o => o.EventArgs)
                .Subscribe(e =>
                {
                    if (e.Orientation.IsAnyLandscape())
                        MessageTextBox.Height = 140;
                    else if (e.Orientation.IsAnyPortrait())
                        MessageTextBox.Height = 260;
                });

            disposables.Add(disp);

            shareButton.GetClick().Take(1).Subscribe(notUsed => PostToWall());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (FacebookAccount.CurrentfacebookAccessCredentials == null)
            {
                if (!wentForwardToAuthPage)
                {
                    GlobalNavigationService.ToFacebookAuthPage();
                    wentForwardToAuthPage = true;
                }
                else // we didn't authenticate for whatever reason, so leave this page
                {
                    this.Visibility = Visibility.Collapsed;
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                }
                return;
            }

            if (string.IsNullOrEmpty(FacebookAccount.CurrentfacebookAccessCredentials.UserId))
            {
                FacebookMyProfileService.GetMyInfo(FacebookAccount.CurrentfacebookAccessCredentials)
                    .Subscribe(
                        meJson => GetProfilePicUrl(meJson.id),
                        HandleGetMyFacebookInfoException);
            }
            else if (string.IsNullOrEmpty(FacebookAccount.CurrentfacebookAccessCredentials.ProfilePicUrl))
            {
                GetProfilePicUrl(FacebookAccount.CurrentfacebookAccessCredentials.UserId);
            }
            else
            {
                this.profilePicUrl = FacebookAccount.CurrentfacebookAccessCredentials.ProfilePicUrl;
            }


            if (NavigationContext.QueryString.ContainsKey("name"))
                name = NavigationContext.QueryString["name"];

            if (NavigationContext.QueryString.ContainsKey("caption"))
                caption = NavigationContext.QueryString["caption"];

            if (NavigationContext.QueryString.ContainsKey("description"))
                description = NavigationContext.QueryString["description"];

            if (NavigationContext.QueryString.ContainsKey("link"))
                link = NavigationContext.QueryString["link"];
        }

        void HandleReceivedUserId(MeJson userInfo)
        {
            var credentials = FacebookAccount.CurrentfacebookAccessCredentials;
            credentials.UserId = userInfo.id;
            FacebookAccount.Save();
            GetProfilePicUrl(credentials.UserId);
        }

        void GetProfilePicUrl(string userId)
        {
            FacebookMyPicService.GetMyPicUrl(userId)
                .ObserveOnDispatcher()
                .Subscribe(HandleMyPicUrl, HandleMyPicUrlException);
        }

        void HandleMyPicUrl(string picUrl)
        {
            FacebookAccount.CurrentfacebookAccessCredentials.ProfilePicUrl = picUrl;
            FacebookAccount.Save();
            this.profilePicUrl = picUrl;
            SetPicSource();
        }

        void SetPicSource()
        {
            if (string.IsNullOrEmpty(this.profilePicUrl))
                return;

            try
            {
                var bitmap = this.profilePicUrl.ToBitmapImage();
                profileImageBrush.ImageSource = bitmap;
                bitmap.ImageOpenedOrFailed().Subscribe(
                    () => ImageFadeInSB.Begin(),
                    ex => { ; })
                    .DisposeWith(this.disposables);
                //var tuple = profilePicCache.GetImageWithNotification(this.profilePicUrl);
                //profileImageBrush.ImageSource = tuple.Item1;
                //var disp = tuple.Item2.Subscribe(notUsed => ImageFadeInSB.Begin());
                //disposables.Add(disp);
            }
            catch (Exception) { }
        }

        void HandleMyPicUrlException(Exception exception)
        {
            ;
        }
        void HandleGetMyFacebookInfoException(Exception exception)
        {
            ;
        }

        void MessageTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Focus();
                PostToWall();
            }
        }

        void PostToWall()
        {
            var wallPost = new FacebookWallPost
            {
                Message = MessageTextBox.Text,
                Name = name,
                Link = link,
                Description = description,
                Caption = caption,
            };

            ProgressBar.Show();

            FacebookPostService.PostToWall(wallPost, FacebookAccount.CurrentfacebookAccessCredentials)
                .ObserveOnDispatcher()
                .Subscribe(
                    notUsed =>
                    {
                        ProgressBar.Hide();

                        ToastService.ToastPrompt("Posted to your wall!");

                        if (NavigationService.CanGoBack && !navigatedAway)
                            NavigationService.GoBack();
                    },
                    exception => HandleWallPostException(exception));
        }

        void HandleWallPostException(Exception exception)
        {
            ProgressBar.Hide();

            if (exception is UnauthorizedCredentialsException)
            {
                var result = MessageBox.Show(
                    string.Format(
                    "There is an error with your Facebook credentials - did you recently revoke access to {0}?  Please re-enter your Facebook username and password",
                    AppSettings.AppName),  string.Empty, MessageBoxButton.OKCancel);

                FacebookAccount.CurrentfacebookAccessCredentials = null;

                if (result == MessageBoxResult.Cancel)
                {
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                }
                else
                {
                    GlobalNavigationService.ToFacebookAuthPage();
                    wentForwardToAuthPage = true;
                }
            }
            else
                MessageBox.Show("There was an error connecting to Facebook");
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            navigatedAway = true;
        }

        private void MessageTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MessageTextBox.Text))
            {
                shorteningTextBlurb.Visibility = Visibility.Visible;
            }
        }

        private void MessageTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            shorteningTextBlurb.Visibility = Visibility.Collapsed;
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}