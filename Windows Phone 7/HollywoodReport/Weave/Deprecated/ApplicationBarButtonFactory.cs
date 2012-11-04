using System;
using System.Windows;
using Microsoft.Phone.Shell;
using weave.Services.Instapaper;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace weave
{
    public class ApplicationBarButtonFactory
    {
        IApplicationBar appBar;
        Func<INewsItem> newsItemProvider;

        ApplicationBarIconButton facebookButton = new ApplicationBarIconButton { IconUri = new Uri("/Assets/Icons/facebook.png", UriKind.Relative), Text = "facebook" };
        ApplicationBarIconButton emailButton = new ApplicationBarIconButton { IconUri = new Uri("/Assets/Icons/appbar.feature.email.rest.png", UriKind.Relative), Text = "email" };
        ApplicationBarIconButton starButton = new ApplicationBarIconButton { IconUri = new Uri("/Assets/Icons/appbar.favs.addto.rest.png", UriKind.Relative), Text = "star" };
        ApplicationBarIconButton markReadButton = new ApplicationBarIconButton { IconUri = new Uri("/Assets/Icons/appbar.check.rest.png", UriKind.Relative), Text = "mark read" };
        ApplicationBarIconButton inAppBrowserButton = new ApplicationBarIconButton { IconUri = new Uri("/Assets/Icons/appbar.eye.png", UriKind.Relative), Text = "view article" };
        
        ApplicationBarMenuItem facebookMI = new ApplicationBarMenuItem { Text = "facebook wall post" };
        ApplicationBarMenuItem emailMI = new ApplicationBarMenuItem { Text = "email article" };
        ApplicationBarMenuItem starMI = new ApplicationBarMenuItem { Text = "star" };
        ApplicationBarMenuItem tweetMI = new ApplicationBarMenuItem { Text = "tweet" };
        ApplicationBarMenuItem instapaperMI = new ApplicationBarMenuItem { Text = "send to instapaper" };
        ApplicationBarMenuItem ieMI = new ApplicationBarMenuItem { Text = "open in internet explorer" };

        public event EventHandler FacebookPressed;
        public event EventHandler EmailPressed;
        public event EventHandler StarPressed;
        public event EventHandler MarkReadPressed;
        public event EventHandler TweetPressed;
        public event EventHandler InstapaperPressed;
        public event EventHandler InternetExplorerPressed;
        public event EventHandler InAppBrowserButtonPressed;

        public enum AppBarMode
        {
            WebBrowserMode,
            PopupMode,
        }

        public ApplicationBarButtonFactory(AppBarMode mode, IApplicationBar bar, Func<INewsItem> newsItemProvider)
        {
            this.appBar = bar;
            this.newsItemProvider = newsItemProvider;

            facebookButton.Click += (s, e) => facebookButton_Click();
            facebookMI.Click += (s, e) => facebookButton_Click();
            emailButton.Click += (s, e) => emailButton_Click();
            emailMI.Click += (s, e) => emailButton_Click();
            ieMI.Click += (s, e) => ieMI_Click();
            instapaperMI.Click += (s, e) => instapaperMI_Click();
            tweetMI.Click += (s, e) => tweetMI_Click();
            markReadButton.Click += (s, e) => markReadButton_Click();
            starButton.Click += (s, e) => starButton_Click();
            starMI.Click += (s, e) => starButton_Click();
            inAppBrowserButton.Click += (s, e) => inAppBrowserButton_Click();

            switch (mode)
            {
                case AppBarMode.PopupMode:
                    ConstructAppBarInMainPagePopupMode();
                    break;
                case AppBarMode.WebBrowserMode:
                    ConstructAppBarInWebBrowserMode();
                    break;
                default:
                    break;
            }

            if (StarredNewsItemsService.IsStarred(newsItemProvider() as NewsItem))
            {
                starButton.IsEnabled = false;
                starMI.IsEnabled = false;
            }
        }

        void ConstructAppBarInWebBrowserMode()
        {
            appBar.Buttons.Clear();
            appBar.MenuItems.Clear();

            appBar.Buttons.Add(facebookButton);
            appBar.Buttons.Add(emailButton);
            //appBar.MenuItems.Add(starMI);
            appBar.MenuItems.Add(tweetMI);
            appBar.MenuItems.Add(instapaperMI);
            appBar.MenuItems.Add(ieMI);
        }

        void ConstructAppBarInMainPagePopupMode()
        {
            appBar.Buttons.Clear();
            appBar.MenuItems.Clear();

            appBar.Buttons.Add(markReadButton);
            appBar.Buttons.Add(inAppBrowserButton);
            //appBar.Buttons.Add(starButton);

            appBar.MenuItems.Add(emailMI);
            appBar.MenuItems.Add(facebookMI);
            appBar.MenuItems.Add(tweetMI);
            appBar.MenuItems.Add(instapaperMI);
            appBar.MenuItems.Add(ieMI);
            //appBar.MenuItems.Add(starMI);
        }

        void facebookButton_Click()
        {
            if (AppSettings.IsTrial)
            {
                PromptForPurchase();
                return;
            }

            //newsItemProvider().ShareToFacebook();
            //GlobalNavigationService.ToFacebookPostPage(newsItemProvider());
            FacebookPressed.Raise(this);
        }

        void emailButton_Click()
        {
            if (AppSettings.IsTrial)
            {
                PromptForPurchase();
                return;
            }

            var newsItem = newsItemProvider();
            newsItem.ShareToEmail();

            EmailPressed.Raise(this);
        }

        void starButton_Click()
        {
            if (AppSettings.IsTrial)
            {
                PromptForPurchase();
                return;
            }

            StarredNewsItemsService.AddToStarredItems(newsItemProvider() as NewsItem);
            starButton.IsEnabled = false;
            StarPressed.Raise(this);
        }

        void markReadButton_Click()
        {
            if (AppSettings.IsTrial)
            {
                PromptForPurchase();
                return;
            }

            MarkReadPressed.Raise(this);
        }

        void tweetMI_Click()
        {
            if (AppSettings.IsTrial)
            {
                PromptForPurchase();
                return;
            }

            //GlobalNavigationService.ToTwitterPostPage(newsItemProvider());
            //newsItemProvider().ShareToTwitter();
            TweetPressed.Raise(this);
        }

        void instapaperMI_Click()
        {
            if (AppSettings.IsTrial)
            {
                PromptForPurchase();
                return;
            }

            newsItemProvider().SendToInstapaper();
            //InstapaperService.SendToInstapaper(newsItemProvider());
            InstapaperPressed.Raise(this);
        }

        void ieMI_Click()
        {
            if (AppSettings.IsTrial)
            {
                PromptForPurchase();
                return;
            }

            var newsItem = newsItemProvider() as NewsItem;
            newsItem.SendToInternetExplorer();
            //if (newsItem != null)
            //    newsItem.HasBeenViewed = true;

            //GlobalNavigationService.ToInternetExplorer(newsItemProvider());
            InternetExplorerPressed.Raise(this);
        }

        void inAppBrowserButton_Click()
        {
            if (AppSettings.IsTrial)
            {
                PromptForPurchase();
                return;
            }

            var newsItem = newsItemProvider() as NewsItem;
            if (newsItem == null)
                return;

            newsItem.HasBeenViewed = true;
            GlobalNavigationService.ToWebBrowserPage(newsItem);
            InAppBrowserButtonPressed.Raise(this);
        }

        static void PromptForPurchase()
        {
            var result = MessageBox.Show("\r\n\r\nThat feature is not available in the Trial Mode.\r\n\r\nWould you like to purchase this app to unlock all features and eliminate all ads?\r\n\r\n", "Buy Me!", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
                SelesGames.Phone.TaskService.ToMarketplaceDetailTask();
        }
    }
}
