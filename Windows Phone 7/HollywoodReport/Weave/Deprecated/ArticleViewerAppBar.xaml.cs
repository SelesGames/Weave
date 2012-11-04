using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace weave
{
    public partial class ArticleViewerAppBar : UserControl, IDisposable
    {
        Frame frame;
        PhoneApplicationPage page;
        bool hasSetFrameAndPage;

        CompositeDisposable disposables = new CompositeDisposable();

        ISubject<Unit> favoritePressed = new Subject<Unit>();
        ISubject<Unit> openInIEPressed = new Subject<Unit>();
        ISubject<Unit> sharePressed = new Subject<Unit>();

        public IObservable<Unit> FavoritePressed { get { return favoritePressed.AsObservable(); } }
        public IObservable<Unit> OpenInIEPressed { get { return openInIEPressed.AsObservable(); } }
        public IObservable<Unit> SharePressed { get { return sharePressed.AsObservable(); } }

        public bool IsExtended { get; private set; }

        public ArticleViewerAppBar()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            VisualStateManager.GoToState(this, "Closed", false);

            this.ellipsesButton.GetTap().Merge(this.leftCloseButton.GetTap()).Subscribe(OnCustomAppBarEllipsesButtonsClicked)
                .DisposeWith(this.disposables);

            this.favoriteButton.GetTap().Subscribe(OnFavoriteButtonClicked)
                .DisposeWith(disposables);

            this.openInIEButton.GetTap().Subscribe(OnOpenInIEButtonClicked)
                .DisposeWith(disposables);

            this.shareArticleButton.GetClick().Subscribe(OnShareButtonClicked)
                .DisposeWith(disposables);
        }

        void OnBackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.IsExtended)
            {
                CloseExtended(true);
                e.Cancel = true;
            }
        }

        internal void Show()
        {
            ToolBarOpenSB.Begin();
        }

        void AddBackButtonHandler()
        {
            if (!this.hasSetFrameAndPage)
            {
                this.frame = Application.Current.RootVisual as Frame;
                if (this.frame != null)
                    this.page = this.frame.Content as PhoneApplicationPage;

                this.hasSetFrameAndPage = true;
            }
            else
            {
                if (this.page != null)
                    this.page.BackKeyPress += OnBackKeyPress;
            }
        }

        void RemoveBackButtonHandler()
        {
            if (this.page != null)
                this.page.BackKeyPress -= OnBackKeyPress;
        }




        #region hide/show the extended state (with text for the buttons)

        void OnCustomAppBarEllipsesButtonsClicked()
        {
            if (!this.IsExtended)
                OpenExtended();
            else
                CloseExtended(true);
        }

        void OpenExtended()
        {
            this.IsExtended = true;
            VisualStateManager.GoToState(this, "Open", true);
            AddBackButtonHandler();
        }

        internal void CloseExtended(bool useAnimation = false)
        {
            this.IsExtended = false;
            VisualStateManager.GoToState(this, "Closed", useAnimation);
            RemoveBackButtonHandler();
        }

        #endregion




        #region app bar buttons (view article, share, mark read) button handling

        void OnFavoriteButtonClicked()
        {
            if (this.IsExtended)
                CloseExtended(true);

            this.favoritePressed.OnNext();
        }

        void OnOpenInIEButtonClicked()
        {
            if (this.IsExtended)
                CloseExtended(true);

            this.openInIEPressed.OnNext();
        }

        void OnShareButtonClicked()
        {
            if (this.IsExtended)
                CloseExtended(true);

            this.sharePressed.OnNext();
        }

        #endregion




        #region Color Change functions

        internal void ToComplementFill(bool useAnimation = false)
        {
            if (useAnimation)
                ToComplementFillUsingAnimation();
            else
                ToComplementFillNoAnimation();
        }

        internal void ToAccentFill(bool useAnimation = false)
        {
            if (useAnimation)
                ToAccentFillUsingAnimation();
            else
                ToAccentFillNoAnimation();
        }

        void ToComplementFillNoAnimation()
        {
            complementFill.SetValue(Canvas.ZIndexProperty, 1);
            accentFill.SetValue(Canvas.ZIndexProperty, 0);
            complementFill.Opacity = 1d;
            accentFill.Opacity = 0d;
            favoriteButton.Background = complementFill.Fill;
            openInIEButton.Background = complementFill.Fill;
            shareArticleButton.Background = complementFill.Fill;
        }

        void ToAccentFillNoAnimation()
        {
            accentFill.SetValue(Canvas.ZIndexProperty, 1);
            complementFill.SetValue(Canvas.ZIndexProperty, 0);
            accentFill.Opacity = 1d;
            complementFill.Opacity = 0d;
            favoriteButton.Background = accentFill.Fill;
            openInIEButton.Background = accentFill.Fill;
            shareArticleButton.Background = accentFill.Fill;
        }

        void ToComplementFillUsingAnimation()
        {
            complementFill.SetValue(Canvas.ZIndexProperty, 0);
            accentFill.SetValue(Canvas.ZIndexProperty, 1);
            favoriteButton.Background = complementFill.Fill;
            openInIEButton.Background = complementFill.Fill;
            shareArticleButton.Background = complementFill.Fill;
            CrossFadeToAccentSB.Pause();
            CrossFadeToComplementSB.Begin();
        }

        void ToAccentFillUsingAnimation()
        {
            accentFill.SetValue(Canvas.ZIndexProperty, 0);
            complementFill.SetValue(Canvas.ZIndexProperty, 1);
            favoriteButton.Background = accentFill.Fill;
            openInIEButton.Background = accentFill.Fill;
            shareArticleButton.Background = accentFill.Fill;
            CrossFadeToComplementSB.Pause();
            CrossFadeToAccentSB.Begin();
        }

        #endregion




        public void Dispose()
        {
            this.disposables.Dispose();
        }
    }
}
